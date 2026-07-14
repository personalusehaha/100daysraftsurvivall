using UnityEngine;
using RaftSurvival.Ocean;

namespace RaftSurvival.Raft
{
    /// <summary>
    /// Makes the raft float naturally on the ocean surface by sampling wave
    /// height at multiple corner points (for realistic tilt/rock motion)
    /// and applying corrective forces via Rigidbody — instead of hard-setting
    /// position, which would look robotic.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class RaftFloater : MonoBehaviour
    {
        [Header("Float Points (corners of the raft)")]
        [SerializeField] private Transform[] floatPoints;

        [Header("Buoyancy Settings")]
        [SerializeField] private float buoyancyForce = 12f;
        [SerializeField] private float waterDrag = 1.5f;
        [SerializeField] private float waterAngularDrag = 1f;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = Vector3.zero;
        }

        private void FixedUpdate()
        {
            if (GerstnerOcean.Instance == null || floatPoints.Length == 0) return;

            int submergedCount = 0;

            foreach (var point in floatPoints)
            {
                float waveHeight = GerstnerOcean.Instance.GetWaveHeight(point.position.x, point.position.z);
                float submersion = waveHeight - point.position.y;

                if (submersion > 0f)
                {
                    submergedCount++;
                    Vector3 force = Vector3.up * (submersion * buoyancyForce);
                    rb.AddForceAtPosition(force, point.position, ForceMode.Acceleration);
                }
            }

            // Apply extra drag while any corner is in water, for stability.
            if (submergedCount > 0)
            {
                rb.linearVelocity *= (1f - waterDrag * Time.fixedDeltaTime);
                rb.angularVelocity *= (1f - waterAngularDrag * Time.fixedDeltaTime);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (floatPoints == null) return;
            Gizmos.color = Color.cyan;
            foreach (var p in floatPoints)
            {
                if (p != null) Gizmos.DrawWireSphere(p.position, 0.2f);
            }
        }
    }
}
