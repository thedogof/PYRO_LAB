using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PyroLab.Fireworks
{
    [RequireComponent(typeof(ParticleSystem))]
    public class FireworkLauncher : MonoBehaviour
    {
        [SerializeField] private FireworkRecipe recipe;
        [SerializeField] private ParticleSystem liftSystem;
        [SerializeField] private ParticleSystem burstSystem;
        [SerializeField] private Light bloomLight;

        [Header("Scaling")]
        [SerializeField] private float liftSpeedScale = 2.2f;
        [SerializeField] private float burstRadiusScale = 0.8f;
        [SerializeField] private float trailScale = 0.25f;

        private Coroutine launchRoutine;
        private readonly List<LayerRuntimeData> activeLayers = new();
        private readonly System.Random random = new();

        public FireworkRecipe Recipe
        {
            get => recipe;
            set => recipe = value;
        }

        private void Awake()
        {
            if (liftSystem == null)
            {
                liftSystem = GetComponent<ParticleSystem>();
            }
        }

        public void Launch()
        {
            if (recipe == null || liftSystem == null || burstSystem == null)
            {
                Debug.LogWarning("FireworkLauncher is missing configuration.");
                return;
            }

            if (launchRoutine != null)
            {
                StopCoroutine(launchRoutine);
            }

            launchRoutine = StartCoroutine(LaunchRoutine());
        }

        public void ResetAndLaunch()
        {
            StopAllCoroutines();
            if (liftSystem != null)
            {
                liftSystem.Clear(true);
            }

            if (burstSystem != null)
            {
                burstSystem.Clear(true);
            }

            Launch();
        }

        private IEnumerator LaunchRoutine()
        {
            ConfigureLiftSystem();
            ConfigureBurstSystem();

            liftSystem.Play();
            UpdateLight(0f);

            float elapsed = 0f;
            while (elapsed < recipe.fuseTime)
            {
                elapsed += Time.deltaTime;
                UpdateLight(Mathf.Clamp01(elapsed / recipe.fuseTime));
                yield return null;
            }

            liftSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            UpdateLight(0f);

            foreach (var layer in activeLayers)
            {
                EmitLayer(layer, 0f, true);
            }

            yield return HandleTimingTrack();
            launchRoutine = null;
        }

        private void ConfigureLiftSystem()
        {
            if (liftSystem == null)
            {
                return;
            }

            float height = Mathf.Max(0f, recipe.GetScaledBurstHeight());
            float liftSpeed = Mathf.Sqrt(height) * liftSpeedScale;

            var main = liftSystem.main;
            main.startSpeed = liftSpeed;
            main.startLifetime = recipe.fuseTime;
            main.maxParticles = Mathf.Max(main.maxParticles, 16);
        }

        private void ConfigureBurstSystem()
        {
            if (burstSystem == null)
            {
                return;
            }

            activeLayers.Clear();
            foreach (var layer in recipe.layers)
            {
                if (layer == null)
                {
                    continue;
                }

                var runtime = new LayerRuntimeData(layer);
                runtime.Allocate(layer.starCount);
                activeLayers.Add(runtime);
            }

            var main = burstSystem.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(recipe.fuseTime * 0.5f, recipe.fuseTime * 1.1f);
            main.startSpeed = 0f;
            main.startSize = recipe.sizeOverLifetime.Evaluate(0f) * recipe.size * burstRadiusScale;
            main.maxParticles = Mathf.Max(main.maxParticles, EstimateMaxParticles());
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var colorOverLifetime = burstSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(recipe.colorOverLifetime);

            var sizeOverLifetime = burstSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, recipe.sizeOverLifetime);

            var trails = burstSystem.trails;
            trails.enabled = true;
            trails.mode = ParticleSystemTrailMode.PerParticle;
            trails.ratio = 1f;
            trails.lifetime = trailScale;

            foreach (var runtime in activeLayers)
            {
                foreach (var modifier in runtime.Modifiers)
                {
                    modifier?.OnSetup(burstSystem, runtime.Layer);
                }
            }
        }

        private int EstimateMaxParticles()
        {
            int total = 0;
            foreach (var layer in recipe.layers)
            {
                if (layer != null)
                {
                    total += layer.starCount * 4;
                }
            }

            return Mathf.Max(512, total);
        }

        private void EmitLayer(LayerRuntimeData runtime, float normalizedTime, bool regenerate)
        {
            if (runtime == null)
            {
                return;
            }

            if (regenerate)
            {
                FireworkBurst.GenerateLayerBurst(recipe, runtime.Layer, runtime.VelocityBuffer, random);
            }

            var gradient = runtime.Layer.layerColor ?? recipe.colorOverLifetime;
            float sizeMultiplier = recipe.sizeOverLifetime.Evaluate(0f) * recipe.size * burstRadiusScale;

            for (int i = 0; i < runtime.Layer.starCount; i++)
            {
                float indexNorm = runtime.Layer.starCount <= 1 ? 0f : i / (float)(runtime.Layer.starCount - 1);
                var emitParams = new ParticleSystem.EmitParams
                {
                    velocity = runtime.VelocityBuffer[i],
                    startColor = gradient.Evaluate(indexNorm) * recipe.hdrIntensity,
                    startSize = sizeMultiplier,
                    startLifetime = runtime.Layer.lifetime
                };

                foreach (var modifier in runtime.Modifiers)
                {
                    if (modifier == null || !modifier.isEnabled)
                    {
                        continue;
                    }

                    modifier.Apply(ref emitParams, runtime.Layer, indexNorm, normalizedTime);
                }

                burstSystem.Emit(emitParams, 1);
            }
        }

        private IEnumerator HandleTimingTrack()
        {
            if (recipe.timing == null || recipe.timing.events == null || recipe.timing.events.Count == 0)
            {
                yield break;
            }

            var events = new List<TimingEvent>(recipe.timing.events);
            events.Sort((a, b) => a.time.CompareTo(b.time));

            float baseLifetime = GetMaxLayerLifetime();
            float previous = 0f;

            foreach (var evt in events)
            {
                float wait = Mathf.Max(0f, evt.time - previous) * baseLifetime;
                previous = evt.time;
                if (wait > 0f)
                {
                    yield return new WaitForSeconds(wait);
                }

                ExecuteTimingEvent(evt);
            }
        }

        private float GetMaxLayerLifetime()
        {
            float lifetime = 1f;
            foreach (var layer in recipe.layers)
            {
                if (layer != null)
                {
                    lifetime = Mathf.Max(lifetime, layer.lifetime);
                }
            }

            return lifetime;
        }

        private void ExecuteTimingEvent(TimingEvent evt)
        {
            if (activeLayers.Count == 0)
            {
                return;
            }

            int index = Mathf.Clamp(evt.layerIndex, 0, activeLayers.Count - 1);
            var runtime = activeLayers[index];

            switch (evt.action)
            {
                case TimingAction.SubBurst:
                    EmitLayer(runtime, evt.time, true);
                    break;
                case TimingAction.Split:
                    TriggerSplit(runtime, evt);
                    break;
                case TimingAction.ColorShift:
                    TriggerModifier<ColorShiftModifier>(runtime, evt);
                    break;
                case TimingAction.StrobeToggle:
                    TriggerModifier<StrobeModifier>(runtime, evt, toggle: true);
                    break;
            }
        }

        private void TriggerSplit(LayerRuntimeData runtime, TimingEvent evt)
        {
            var splitModifier = runtime.GetModifier<SplitModifier>();
            if (splitModifier == null)
            {
                return;
            }

            FireworkBurst.GenerateLayerBurst(recipe, runtime.Layer, runtime.VelocityBuffer, random);
            for (int i = 0; i < runtime.Layer.starCount; i++)
            {
                Vector3 direction = runtime.VelocityBuffer[i].normalized;
                for (int j = 0; j < splitModifier.splitCount; j++)
                {
                    Vector3 splitDir = splitModifier.SampleSplitDirection(random, direction);
                    var emitParams = new ParticleSystem.EmitParams
                    {
                        velocity = splitDir * splitModifier.splitSpeed * recipe.size,
                        startColor = runtime.Layer.layerColor.Evaluate(UnityEngine.Random.value) * recipe.hdrIntensity,
                        startSize = recipe.sizeOverLifetime.Evaluate(0.5f) * recipe.size * 0.5f,
                        startLifetime = runtime.Layer.lifetime * 0.6f
                    };

                    burstSystem.Emit(emitParams, 1);
                }
            }
        }

        private void TriggerModifier<T>(LayerRuntimeData runtime, TimingEvent evt, bool toggle = false)
            where T : VisualModifier
        {
            var modifier = runtime.GetModifier<T>(evt.modifierIndex);
            if (modifier == null)
            {
                return;
            }

            if (toggle)
            {
                modifier.isEnabled = !modifier.isEnabled;
            }

            modifier.OnTimingEvent(burstSystem, runtime.Layer, evt);
        }

        private void UpdateLight(float normalized)
        {
            if (bloomLight == null)
            {
                return;
            }

            bloomLight.intensity = Mathf.Lerp(0f, recipe.hdrIntensity, normalized);
            bloomLight.color = recipe.colorOverLifetime.Evaluate(normalized);
        }

        private class LayerRuntimeData
        {
            public FireworkLayer Layer { get; }
            public VisualModifier[] Modifiers { get; }
            public Vector3[] VelocityBuffer { get; private set; }

            public LayerRuntimeData(FireworkLayer layer)
            {
                Layer = layer;
                Modifiers = layer.modifiers?.ToArray() ?? Array.Empty<VisualModifier>();
                VelocityBuffer = Array.Empty<Vector3>();
            }

            public void Allocate(int count)
            {
                if (VelocityBuffer == null || VelocityBuffer.Length != count)
                {
                    VelocityBuffer = new Vector3[count];
                }
            }

            public T GetModifier<T>() where T : VisualModifier
            {
                return GetModifier<T>(-1);
            }

            public T GetModifier<T>(int preferredIndex) where T : VisualModifier
            {
                if (preferredIndex >= 0 && preferredIndex < Modifiers.Length)
                {
                    if (Modifiers[preferredIndex] is T targeted)
                    {
                        return targeted;
                    }
                }

                foreach (var modifier in Modifiers)
                {
                    if (modifier is T typed)
                    {
                        return typed;
                    }
                }

                return null;
            }
        }
    }
}
