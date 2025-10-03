using System.Collections;
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

        private Coroutine launchRoutine;
        private readonly Vector3[] velocityBuffer = new Vector3[4096];

        public FireworkRecipe Recipe
        {
            get => recipe;
            set => recipe = value;
        }

        public void Launch()
        {
            if (recipe == null || liftSystem == null || burstSystem == null)
            {
                Debug.LogWarning("FireworkLauncher is not properly configured.");
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
            liftSystem.Clear(true);
            burstSystem.Clear(true);
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
            TriggerBurst();
            UpdateLight(0f);
        }

        private void ConfigureLiftSystem()
        {
            var main = liftSystem.main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(recipe.liftSpeed, recipe.liftSpeed + recipe.liftAcceleration);
            main.startLifetime = recipe.fuseTime;
            main.gravityModifier = recipe.gravityFactor * 0.1f;
        }

        private void ConfigureBurstSystem()
        {
            var main = burstSystem.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(recipe.fuseTime * 0.6f, recipe.fuseTime * 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(recipe.sizeOverLifetime.Evaluate(0f));
            main.startColor = recipe.colorGradient.Evaluate(0f);
            main.gravityModifier = recipe.gravityFactor;
            main.maxParticles = Mathf.Max(main.maxParticles, recipe.starCount * (recipe.multiLayer ? 2 : 1));

            var colorOverLifetime = burstSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(recipe.colorGradient);

            var sizeOverLifetime = burstSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, recipe.sizeOverLifetime);

            var trails = burstSystem.trails;
            trails.enabled = recipe.burstDrag > 0.05f;
        }

        private void TriggerBurst()
        {
            int starCount = recipe.starCount;
            if (recipe.multiLayer)
            {
                starCount *= 2;
            }

            if (velocityBuffer.Length < starCount)
            {
                Debug.LogError("Velocity buffer is too small for the configured firework.");
                return;
            }

            FireworkBurst.GenerateBurstVectors(recipe, velocityBuffer);

            var emitParams = new ParticleSystem.EmitParams
            {
                startColor = recipe.colorGradient.Evaluate(0f) * recipe.hdrIntensity,
                startSize = recipe.sizeOverLifetime.Evaluate(0f)
            };

            for (int i = 0; i < recipe.starCount; i++)
            {
                emitParams.velocity = velocityBuffer[i];
                burstSystem.Emit(emitParams, 1);
            }

            if (recipe.multiLayer)
            {
                for (int i = 0; i < recipe.starCount; i++)
                {
                    emitParams.velocity = velocityBuffer[i] * recipe.innerLayerSpeedMultiplier;
                    emitParams.startSize = recipe.sizeOverLifetime.Evaluate(0f) * recipe.innerLayerRatio;
                    burstSystem.Emit(emitParams, 1);
                }
            }

            if (bloomLight != null)
            {
                StartCoroutine(AnimateLightFlash());
            }
        }

        private IEnumerator AnimateLightFlash()
        {
            float duration = 0.4f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                UpdateLight(1f - (t / duration));
                yield return null;
            }
            UpdateLight(0f);
        }

        private void UpdateLight(float intensity)
        {
            if (bloomLight == null)
            {
                return;
            }

            bloomLight.intensity = Mathf.Lerp(0f, recipe.hdrIntensity, intensity);
            bloomLight.color = recipe.colorGradient.Evaluate(intensity);
        }
    }
}
