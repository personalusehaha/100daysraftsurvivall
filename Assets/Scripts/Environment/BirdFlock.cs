using UnityEngine;

namespace RaftSurvival.Environment
{
    /// <summary>
    /// Lightweight flocking behavior for a group of birds flying near
    /// islands or over the ocean. Uses simple boid-style rules (cohesion,
    /// separation, alignment) kept cheap for mobile performance — no
    /// physics, pure transform math.
    /// </summary>
    public class BirdFlock : MonoBehaviour
    {
        [Header("Flock Setup")]
        [SerializeField] private GameObject birdPrefab;
        [SerializeField] private int flockSize = 8;
        [SerializeField] private float spawnRadius = 5f;

        [Header("Flight Area")]
        [SerializeField] private Transform centerPoint;
        [SerializeField] private float wanderRadius = 25f;
        [SerializeField] private float minHeight = 8f;
        [SerializeField] private float maxHeight = 20f;

        [Header("Boid Behavior")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float rotationSpeed = 3f;
        [SerializeField] private float neighborRadius = 4f;
        [SerializeField] private float separationDistance = 1.5f;

        private Transform[] birds;
        private Vector3[] velocities;

        private void Start()
        {
            if (centerPoint == null) centerPoint = transform;
            SpawnFlock();
        }

        private void SpawnFlock()
        {
            if (birdPrefab == null) return;

            birds = new Transform[flockSize];
            velocities = new Vector3[flockSize];

            for (int i = 0; i < flockSize; i++)
            {
                Vector3 spawnPos = centerPoint.position + Random.insideUnitSphere * spawnRadius;
                spawnPos.y = Mathf.Clamp(spawnPos.y, minHeight, maxHeight);

                GameObject bird = Instantiate(birdPrefab, spawnPos, Random.rotation, transform);
                birds[i] = bird.transform;
                velocities[i] = Random.onUnitSphere * moveSpeed;
            }
        }

        private void Update()
        {
            if (birds == null) return;

            for (int i = 0; i < birds.Length; i++)
            {
                if (birds[i] == null) continue;

                Vector3 cohesion = Vector3.zero;
                Vector3 separation = Vector3.zero;
                Vector3 alignment = Vector3.zero;
                int neighborCount = 0;

                for (int j = 0; j < birds.Length; j++)
                {
                    if (i == j || birds[j] == null) continue;

                    float dist = Vector3.Distance(birds[i].position, birds[j].position);
                    if (dist < neighborRadius)
                    {
                        cohesion += birds[j].position;
                        alignment += velocities[j];
                        neighborCount++;

                        if (dist < separationDistance)
                        {
                            separation += (birds[i].position - birds[j].position).normalized / Mathf.Max(dist, 0.01f);
                        }
                    }
                }

                Vector3 toCenter = (centerPoint.position - birds[i].position).normalized * 0.5f;

                if (neighborCount > 0)
                {
                    cohesion = (cohesion / neighborCount - birds[i].position).normalized;
                    alignment = (alignment / neighborCount).normalized;
                }

                Vector3 desiredDir = (cohesion + separation + alignment + toCenter).normalized;
                if (desiredDir == Vector3.zero) desiredDir = birds[i].forward;

                velocities[i] = Vector3.Lerp(velocities[i], desiredDir * moveSpeed, Time.deltaTime * rotationSpeed);

                Vector3 nextPos = birds[i].position + velocities[i] * Time.deltaTime;
                nextPos.y = Mathf.Clamp(nextPos.y, minHeight, maxHeight);

                birds[i].position = nextPos;

                if (velocities[i] != Vector3.zero)
                {
                    birds[i].rotation = Quaternion.Slerp(birds[i].rotation,
                        Quaternion.LookRotation(velocities[i]), Time.deltaTime * rotationSpeed);
                }
            }
        }
    }
}
