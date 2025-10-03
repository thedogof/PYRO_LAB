using System;
using System.Collections.Generic;
using PyroLab.Fireworks.Workshop;
using UnityEngine;

namespace PyroLab.Fireworks
{
    public static class WorkshopRecipeBuilder
    {
        private const float SizeBase = 0.8f;
        private const float SizeLayerFactor = 0.02f;
        private const float RadiusScale = 0.8f;

        public static FireworkRecipe BuildRecipe(
            FireworkRecipe target,
            IReadOnlyList<PaperLayerDefinition> papers,
            FuseDefinition fuse,
            ShellDefinition shell,
            IReadOnlyList<StarCompoundDefinition> starCompounds,
            BurstCoreDefinition burstCore,
            TimingTrackDefinition timingDefinition,
            float baseHeight)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            float paperCount = papers?.Count ?? 0;
            float totalPaperLayers = 0f;
            float wrapSum = 0f;
            float thicknessSum = 0f;
            foreach (var paper in papers ?? Array.Empty<PaperLayerDefinition>())
            {
                totalPaperLayers += paper.layerCount;
                wrapSum += paper.wrapTension;
                thicknessSum += paper.thickness;
            }

            float averageWrap = paperCount > 0 ? wrapSum / paperCount : 0.7f;
            float averageLayers = paperCount > 0 ? totalPaperLayers / paperCount : 6f;
            float averageThickness = paperCount > 0 ? thicknessSum / paperCount : 0.2f;
            float size = SizeBase + SizeLayerFactor * totalPaperLayers;

            target.size = Mathf.Max(0.1f, size);
            target.desiredBurstHeight = Mathf.Max(10f, baseHeight);
            target.fuseTime = fuse != null ? fuse.delaySeconds : target.fuseTime;
            target.launchVariance = Mathf.Lerp(0.3f, 0.05f, fuse != null ? fuse.stability : 0f);
            target.burstSymmetry = Mathf.Lerp(0.7f, 1f, Mathf.Clamp01(averageWrap));
            float spreadT = Mathf.InverseLerp(1f, 20f, Mathf.Clamp(averageLayers, 1f, 20f));
            target.spreadJitter = Mathf.Lerp(0.15f, 0f, spreadT);

            float shellHardness = shell != null ? shell.hardness : 0.3f;
            float shellMass = shell != null ? shell.massFactor : 0.2f;
            float shellTightness = shell != null ? shell.burstTightness : 0.4f;
            target.angularSpread = Mathf.Lerp(6f, 1f, Mathf.Clamp01(shellTightness));
            target.gravityFactor = Mathf.Lerp(0.1f, 0.4f, Mathf.Clamp01(shellMass));
            target.trailLengthScale = Mathf.Lerp(1.5f, 4f, Mathf.Clamp01(shellHardness));

            target.layers ??= new List<FireworkLayer>();
            target.layers.Clear();

            var timing = timingDefinition != null && timingDefinition.track != null
                ? timingDefinition.track.Clone()
                : new TimingTrack();
            timing.events ??= new List<TimingEvent>();

            float maxIntensity = 0f;
            bool addedGravityModifier = false;
            float speedMin = burstCore != null ? burstCore.speedMinMax.x : 8f;
            float speedMax = burstCore != null ? burstCore.speedMinMax.y : 12f;

            foreach (var star in starCompounds ?? Array.Empty<StarCompoundDefinition>())
            {
                var layer = new FireworkLayer
                {
                    pattern = burstCore != null ? burstCore.pattern : BurstPatternType.Peony,
                    starCount = burstCore != null ? burstCore.starCount : 320,
                    speedMin = speedMin * target.size,
                    speedMax = speedMax * target.size,
                    spread = (burstCore != null ? burstCore.spread : 0.05f) + target.spreadJitter,
                    layerColor = star != null ? star.colorGradient : FireworkLayer.DefaultLayerGradient(),
                    lifetime = star != null ? star.lifeSeconds : 1.5f
                };

                float speedAvg = ((layer.speedMin + layer.speedMax) * 0.5f);
                layer.radius = speedAvg * layer.lifetime * RadiusScale;

                layer.modifiers.Clear();

                if (!addedGravityModifier)
                {
                    var gravity = ScriptableObject.CreateInstance<GravityDragModifier>();
                    gravity.gravityFactor = target.gravityFactor;
                    gravity.dragCurve = BuildDragCurve(averageThickness);
                    layer.modifiers.Add(gravity);
                    addedGravityModifier = true;
                }

                var trail = ScriptableObject.CreateInstance<TrailModifier>();
                float starTrail = star != null ? star.trail : 0.2f;
                trail.lengthScale = target.trailLengthScale + shellHardness * 2f + starTrail * 2f;
                trail.velocityStretch = 0.6f;
                layer.modifiers.Add(trail);

                if (star != null && star.strobeFrequency > 0f)
                {
                    var strobe = ScriptableObject.CreateInstance<StrobeModifier>();
                    strobe.frequency = star.strobeFrequency;
                    strobe.duty = star.strobeDuty;
                    layer.modifiers.Add(strobe);
                }

                if (star != null && star.twinkleAmount > 0f)
                {
                    var twinkle = ScriptableObject.CreateInstance<TwinkleModifier>();
                    twinkle.twinkleProbability = star.twinkleAmount;
                    twinkle.intensityVariance = Mathf.Lerp(0.1f, 0.6f, star.twinkleAmount);
                    layer.modifiers.Add(twinkle);
                }

                maxIntensity = Mathf.Max(maxIntensity, star != null ? star.hdrIntensity : 0f);
                target.layers.Add(layer);
            }

            if (target.layers.Count == 0)
            {
                // Ensure at least one layer for preview purposes.
                target.layers.Add(new FireworkLayer());
            }

            if (burstCore != null && burstCore.secondary)
            {
                var sourceLayer = target.layers[0];
                var secondaryLayer = CloneLayer(sourceLayer);
                secondaryLayer.pattern = burstCore.secondaryPattern;
                target.layers.Add(secondaryLayer);

                bool hasSecondaryEvent = false;
                foreach (var evt in timing.events)
                {
                    if (evt.action == TimingAction.SubBurst && evt.layerIndex == target.layers.Count - 1)
                    {
                        hasSecondaryEvent = true;
                        break;
                    }
                }

                if (!hasSecondaryEvent)
                {
                    timing.events.Add(new TimingEvent
                    {
                        name = "Secondary Burst",
                        time = Mathf.Clamp01(burstCore.secondaryDelay),
                        action = TimingAction.SubBurst,
                        layerIndex = target.layers.Count - 1
                    });
                }
            }

            target.timing = timing;
            target.hdrIntensity = maxIntensity > 0f ? maxIntensity : target.hdrIntensity;
            if (target.layers.Count > 0)
            {
                target.colorOverLifetime = target.layers[0].layerColor;
            }

            return target;
        }

        private static FireworkLayer CloneLayer(FireworkLayer source)
        {
            var clone = new FireworkLayer
            {
                pattern = source.pattern,
                starCount = source.starCount,
                speedMin = source.speedMin,
                speedMax = source.speedMax,
                spread = source.spread,
                layerColor = source.layerColor,
                lifetime = source.lifetime,
                radius = source.radius,
                ringThickness = source.ringThickness,
                palmArcCount = source.palmArcCount,
                palmBend = source.palmBend,
                pistilRadius = source.pistilRadius,
                projectionMask = source.projectionMask
            };

            clone.layeredShellRadii.Clear();
            clone.layeredShellRadii.AddRange(source.layeredShellRadii);
            foreach (var modifier in source.modifiers)
            {
                if (modifier == null)
                {
                    continue;
                }

                var cloneModifier = UnityEngine.Object.Instantiate(modifier);
                clone.modifiers.Add(cloneModifier);
            }

            return clone;
        }

        private static AnimationCurve BuildDragCurve(float thickness)
        {
            float endValue = Mathf.Lerp(0.2f, 0.5f, Mathf.Clamp01(thickness));
            return new AnimationCurve(
                new Keyframe(0f, 0.1f),
                new Keyframe(0.5f, Mathf.Lerp(0.15f, endValue, 0.5f)),
                new Keyframe(1f, endValue)
            );
        }
    }
}
