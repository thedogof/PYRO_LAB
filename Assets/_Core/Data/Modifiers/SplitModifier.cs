using UnityEngine;

namespace PyroLab.Fireworks
{
    [CreateAssetMenu(menuName = "PYRO/Modifiers/Split")]
    public class SplitModifier : VisualModifier
    {
        [Range(0f, 1f)] public float splitTime = 0.5f;
        [Min(1)] public int splitCount = 3;
        [Min(0.1f)] public float splitSpeed = 6f;
        [Range(0f, 1f)] public float angularJitter = 0.25f;

        public override void Apply(ref ParticleSystem.EmitParams emitParams, FireworkLayer layer, float normalizedIndex, float normalizedTime)
        {
        }

        public Vector3 SampleSplitDirection(System.Random random, Vector3 sourceDirection)
        {
            Vector3 baseDir = sourceDirection == Vector3.zero ? RandomPointOnSphere(random) : sourceDirection.normalized;
            Vector3 randomDir = RandomPointOnSphere(random);
            return Vector3.Slerp(baseDir, randomDir, angularJitter).normalized;
        }

        private static Vector3 RandomPointOnSphere(System.Random random)
        {
            if (random == null)
            {
                return UnityEngine.Random.onUnitSphere;
            }

            float u = (float)random.NextDouble();
            float v = (float)random.NextDouble();
            float theta = 2f * Mathf.PI * u;
            float phi = Mathf.Acos(2f * v - 1f);
            float sinPhi = Mathf.Sin(phi);
            return new Vector3(Mathf.Cos(theta) * sinPhi, Mathf.Cos(phi), Mathf.Sin(theta) * sinPhi);
        }
    }
}
