using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RaftSurvival.Resources
{
    /// <summary>
    /// Periodically spawns random floating resources around the player/raft
    /// within a donut-shaped area (not too close, not too far), and cleans
    /// up old uncollected ones that drift too far away — keeping the ocean
    /// populated without unbounded object growth (important for mobile perf).
    /// </summary>
    public class ResourceSpawner : MonoBehaviour
    {
        [System.Serializable]
        public struct SpawnEntry
        {
            public GameObject prefab;
            [Tooltip("Relative chance weight — higher = more common.")]
            public float weight;
        }

        [Header("References")]
        [SerializeField] private Transform player; // spawn relative to player/raft

        [Header("Spawn Area")]
        [SerializeField] private float minSpawnRadius = 15f;
        [SerializeField] private float maxSpawnRadius = 45f;
        [SerializeField] private float despawnRadius = 80f;

        [Header("Spawn Timing")]
        [SerializeField] private float spawnIntervalSeconds = 4f;
        [SerializeField] private int maxActiveResources = 40;

        [Header("Resource Pool (weighted random)")]
        [SerializeField] private SpawnEntry[] spawnEntries;

        private readonly List<GameObject> activeResources = new List<GameObject>();
        private float totalWeight;

        private void Start()
        {
            CalculateTotalWeight();
            StartCoroutine(SpawnLoop());
        }

        private void CalculateTotalWeight()
        {
            totalWeight = 0f;
            foreach (var entry in spawnEntries)
            {
                totalWeight += entry.weight;
            }
        }

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(spawnIntervalSeconds);

                CleanupList();

                if (activeResources.Count < maxActiveResources && player != null)
                {
                    SpawnRandomResource();
                }
            }
        }

        private void SpawnRandomResource()
        {
            GameObject prefab = PickWeightedRandomPrefab();
            if (prefab == null) return;

            Vector2 randomCircle = Random.insideUnitCircle.normalized *
                Random.Range(minSpawnRadius, maxSpawnRadius);

            Vector3 spawnPos = player.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);
            activeResources.Add(instance);
        }

        private GameObject PickWeightedRandomPrefab()
        {
            if (spawnEntries.Length == 0 || totalWeight <= 0f) return null;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var entry in spawnEntries)
            {
                cumulative += entry.weight;
                if (roll <= cumulative)
                {
                    return entry.prefab;
                }
            }

            return spawnEntries[spawnEntries.Length - 1].prefab;
        }

        private void CleanupList()
        {
            for (int i = activeResources.Count - 1; i >= 0; i--)
            {
                GameObject res = activeResources[i];

                if (res == null)
                {
                    activeResources.RemoveAt(i);
                    continue;
                }

                float dist = Vector3.Distance(player.position, res.transform.position);
                if (dist > despawnRadius)
                {
                    Destroy(res);
                    activeResources.RemoveAt(i);
                }
            }
        }
    }
}
