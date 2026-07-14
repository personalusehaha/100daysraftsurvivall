using UnityEngine;
using RaftSurvival.Ocean;

namespace RaftSurvival.Environment
{
    /// <summary>
    /// Triggers water splash/spray particle effects at the raft's bow when
    /// moving through waves, and at the player's feet when entering/exiting
    /// water. Keeps particle emission rate tied to relative speed for a
    /// realistic look without constant heavy emission (mobile-friendly).
    /// </summary>
    public class WaterParticleController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ParticleSystem splashParticles;
        [SerializeField] private Rigidbody attachedRigidbody; // raft's rigidbody, if used on the raft
        [SerializeField] private Transform trackedTransform;  // fallback: player or object to track

        [Header("Splash Settings")]
        [SerializeField] private float minSpeedForSplash = 0.5f;
        [SerializeField] private float maxEmissionRate = 40f;
        [SerializeField] private float waterContactOffset = 0.15f;

        private ParticleSystem.EmissionModule emission;

        private void Awake()
        {
            if (splashParticles != null)
            {
                emission = splashParticles.emission;
            }
        }

        private void Update()
        {
            if (splashParticles == null || GerstnerOcean.Instance == null) return;

            Transform t = attachedRigidbody != null ? attachedRigidbody.transform : trackedTransform;
            if (t == null) return;

            float waveHeight = GerstnerOcean.Instance.GetWaveHeight(t.position.x, t.position.z);
            bool nearWater = Mathf.Abs(t.position.y - waveHeight) <= waterContactOffset;

            float speed = attachedRigidbody != null ? attachedRigidbody.linearVelocity.magnitude : 0f;

            if (nearWater && speed > minSpeedForSplash)
            {
                if (!splashParticles.isPlaying) splashParticles.Play();

                float rate = Mathf.Clamp(speed / 5f, 0.1f, 1f) * maxEmissionRate;
                emission.rateOverTime = rate;

                // Keep the particle system positioned at the water contact point.
                Vector3 pos = t.position;
                pos.y = waveHeight;
                splashParticles.transform.position = pos;
            }
            else if (splashParticles.isPlaying)
            {
                splashParticles.Stop();
            }
        }

        /// <summary>
        /// Call this for a one-shot splash burst (e.g. player jumping into water).
        /// </summary>
        public void PlaySplashBurst(Vector3 worldPosition, int particleCount = 20)
        {
            if (splashParticles == null) return;

            splashParticles.transform.position = worldPosition;
            splashParticles.Emit(particleCount);
        }
    }
}
