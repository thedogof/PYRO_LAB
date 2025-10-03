using System;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace CoreVFX.Fireworks
{
    [AddComponentMenu("VFX/Firework Launcher")]
    public class VFXFireworkLauncher : MonoBehaviour
    {
        private static readonly int StarCountId = Shader.PropertyToID("starCount");
        private static readonly int BurstRadiusId = Shader.PropertyToID("burstRadius");
        private static readonly int ColorGradientId = Shader.PropertyToID("colorGradient");
        private static readonly int DragId = Shader.PropertyToID("drag");
        private static readonly int GravityFactorId = Shader.PropertyToID("gravityFactor");
        private static readonly int TrailLengthId = Shader.PropertyToID("trailLength");
        private static readonly int StrobeFrequencyId = Shader.PropertyToID("strobeFrequency");

        [Serializable]
        public class FireworkVariant
        {
            [Tooltip("Friendly label shown in the inspector for this variant.")]
            public string displayName = "Peony";

            [Tooltip("Visual Effect component bound to the variant graph.")]
            public VisualEffect effect;

            [Tooltip("Number of particles spawned during the explosion event.")]
            public int starCount = 10000;

            [Tooltip("Explosion radius in meters.")]
            public float burstRadius = 16f;

            [Tooltip("HDR gradient used to color the fireworks.")]
            public Gradient colorGradient = new Gradient();

            [Tooltip("Drag applied to the stars over their lifetime.")]
            public float drag = 0.06f;

            [Tooltip("Gravity factor applied during the willow trail.")]
            public float gravityFactor = -2.5f;

            [Tooltip("Normalized trail length multiplier (0 disables trails).")]
            public float trailLength = 0.45f;

            [Tooltip("Frequency of the strobe effect (0 disables strobe).")]
            public float strobeFrequency = 0.0f;

            [Tooltip("Time (seconds) between automatic launches.")]
            public float launchInterval = 3.0f;

            [Tooltip("Optional override for the launch to explosion delay (seconds)." )]
            public float explosionDelay = 1.5f;
        }

        [Header("Variants")]
        [Tooltip("List of firework variants that can be toggled with the numeric keys (1..n).")]
        public FireworkVariant[] variants = Array.Empty<FireworkVariant>();

        [Header("Input")]
        [Tooltip("Automatically fire the current variant when enabled.")]
        public bool autoLoop = true;

        private int _currentVariantIndex = -1;
        private Coroutine _launchLoop;

        private void OnEnable()
        {
            if (variants == null || variants.Length == 0)
            {
                Debug.LogWarning("No firework variants configured for VFXFireworkLauncher.");
                return;
            }

            // Activate first variant.
            SetActiveVariant(Mathf.Clamp(_currentVariantIndex, 0, variants.Length - 1));

            if (autoLoop)
            {
                _launchLoop = StartCoroutine(LaunchLoop());
            }
        }

        private void OnDisable()
        {
            if (_launchLoop != null)
            {
                StopCoroutine(_launchLoop);
                _launchLoop = null;
            }
        }

        private void Update()
        {
            if (variants == null || variants.Length == 0)
            {
                return;
            }

            for (var i = 0; i < variants.Length; i++)
            {
                var keyCode = KeyCode.Alpha1 + i;
                if (Input.GetKeyDown(keyCode))
                {
                    SetActiveVariant(i);
                    break;
                }
            }

            if (!autoLoop && Input.GetKeyDown(KeyCode.Space))
            {
                TriggerLaunch();
            }
        }

        private IEnumerator LaunchLoop()
        {
            while (enabled)
            {
                yield return TriggerLaunch();

                var variant = GetCurrentVariant();
                var wait = variant != null ? Mathf.Max(0.1f, variant.launchInterval) : 2.5f;
                yield return new WaitForSeconds(wait);
            }
        }

        private IEnumerator TriggerLaunch()
        {
            var variant = GetCurrentVariant();
            if (variant?.effect == null)
            {
                yield break;
            }

            ApplyVariantSettings(variant);

            variant.effect.SendEvent("OnLaunch");

            var delay = Mathf.Max(0f, variant.explosionDelay);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            variant.effect.SendEvent("OnBurst");
        }

        private void SetActiveVariant(int index)
        {
            if (variants == null || index < 0 || index >= variants.Length)
            {
                return;
            }

            if (_currentVariantIndex == index)
            {
                return;
            }

            // Disable previous effect
            if (_currentVariantIndex >= 0 && _currentVariantIndex < variants.Length)
            {
                var previous = variants[_currentVariantIndex]?.effect;
                if (previous != null)
                {
                    previous.Stop();
                    previous.gameObject.SetActive(false);
                }
            }

            _currentVariantIndex = index;

            var current = variants[_currentVariantIndex];
            if (current?.effect == null)
            {
                Debug.LogWarning($"Firework variant '{current?.displayName}' has no VisualEffect component assigned.");
                return;
            }

            current.effect.gameObject.SetActive(true);
            current.effect.Play();
            ApplyVariantSettings(current);
        }

        private FireworkVariant GetCurrentVariant()
        {
            if (variants == null || _currentVariantIndex < 0 || _currentVariantIndex >= variants.Length)
            {
                return null;
            }

            return variants[_currentVariantIndex];
        }

        private void ApplyVariantSettings(FireworkVariant variant)
        {
            if (variant?.effect == null)
            {
                return;
            }

            if (variant.effect.HasInt(StarCountId))
            {
                variant.effect.SetInt(StarCountId, Mathf.Max(0, variant.starCount));
            }

            if (variant.effect.HasFloat(BurstRadiusId))
            {
                variant.effect.SetFloat(BurstRadiusId, Mathf.Max(0f, variant.burstRadius));
            }

            if (variant.effect.HasGradient(ColorGradientId))
            {
                variant.effect.SetGradient(ColorGradientId, variant.colorGradient);
            }

            if (variant.effect.HasFloat(DragId))
            {
                variant.effect.SetFloat(DragId, variant.drag);
            }

            if (variant.effect.HasFloat(GravityFactorId))
            {
                variant.effect.SetFloat(GravityFactorId, variant.gravityFactor);
            }

            if (variant.effect.HasFloat(TrailLengthId))
            {
                variant.effect.SetFloat(TrailLengthId, Mathf.Clamp01(variant.trailLength));
            }

            if (variant.effect.HasFloat(StrobeFrequencyId))
            {
                variant.effect.SetFloat(StrobeFrequencyId, Mathf.Max(0f, variant.strobeFrequency));
            }
        }
    }
}
