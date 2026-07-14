using UnityEngine;

namespace RaftSurvival.Ocean
{
    /// <summary>
    /// Simulates animated ocean waves using stacked Gerstner wave functions
    /// applied to a mesh's vertices in real time. Also exposes a static
    /// height-sampling method (GetWaveHeight) so other scripts (Raft, Player)
    /// can query water height at any world position without re-deforming
    /// the mesh — critical for floating physics.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class GerstnerOcean : MonoBehaviour
    {
        [System.Serializable]
        public struct Wave
        {
            public Vector2 direction;
            public float amplitude;
            public float wavelength;
            public float speed;
        }

        [Header("Wave Definitions")]
        [SerializeField]
        private Wave[] waves = new Wave[]
        {
            new Wave { direction = new Vector2(1f, 0.3f),  amplitude = 0.35f, wavelength = 12f, speed = 1.2f },
            new Wave { direction = new Vector2(0.6f, 1f),  amplitude = 0.22f, wavelength = 7f,  speed = 1.8f },
            new Wave { direction = new Vector2(-0.4f, 0.8f), amplitude = 0.12f, wavelength = 4f, speed = 2.4f },
        };

        [Header("Mesh Update Settings")]
        [Tooltip("Rebuild mesh every N frames to save mobile GPU/CPU cost.")]
        [SerializeField] private int updateEveryNFrames = 1;

        private Mesh mesh;
        private Vector3[] baseVertices;
        private Vector3[] displacedVertices;
        private int frameCount;

        // Static reference so any script can sample wave height cheaply.
        public static GerstnerOcean Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            mesh = GetComponent<MeshFilter>().mesh;
            baseVertices = mesh.vertices;
            displacedVertices = new Vector3[baseVertices.Length];
        }

        private void Update()
        {
            frameCount++;
            if (frameCount % updateEveryNFrames != 0) return;

            float t = Time.time;

            for (int i = 0; i < baseVertices.Length; i++)
            {
                Vector3 worldPos = transform.TransformPoint(baseVertices[i]);
                Vector3 offset = ComputeWaveOffset(worldPos.x, worldPos.z, t);
                displacedVertices[i] = baseVertices[i] + transform.InverseTransformVector(offset);
            }

            mesh.vertices = displacedVertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private Vector3 ComputeWaveOffset(float x, float z, float t)
        {
            Vector3 offset = Vector3.zero;

            foreach (var w in waves)
            {
                Vector2 dir = w.direction.normalized;
                float freq = 2f * Mathf.PI / Mathf.Max(w.wavelength, 0.001f);
                float phase = freq * (dir.x * x + dir.y * z) + t * w.speed;

                offset.y += w.amplitude * Mathf.Sin(phase);
                offset.x += w.amplitude * dir.x * Mathf.Cos(phase) * 0.5f;
                offset.z += w.amplitude * dir.y * Mathf.Cos(phase) * 0.5f;
            }

            return offset;
        }

        /// <summary>
        /// Samples wave height (world Y) at a given world XZ position without
        /// touching the mesh — used by Raft/Player floating logic every frame.
        /// </summary>
        public float GetWaveHeight(float worldX, float worldZ)
        {
            Vector3 offset = ComputeWaveOffset(worldX, worldZ, Time.time);
            return transform.position.y + offset.y;
        }
    }
}
