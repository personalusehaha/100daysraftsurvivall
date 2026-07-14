using System.Collections.Generic;
using UnityEngine;

namespace RaftSurvival.Islands
{
    /// <summary>
    /// Spawns random tropical islands at intervals around the player's
    /// drift path. Each island prefab is expected to already contain its
    /// own decoration (palm trees, rocks, bushes, grass, flowers, huts,
    /// beach, dock, cave) pre-arranged — this spawner just places whole
    /// island prefabs at valid, spaced-out positions and removes ones left
    /// far behind, to keep the world bounded for mobile performance.
    /// </summary>
    public class IslandSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;

        [Header("Island Prefabs")]
        [Tooltip("A variety of whole tropical island prefabs (each with its own terrain, trees, huts, docks, caves already arranged).")]
        [SerializeField] private GameObject[] islandPrefabs;

        [Header("Spawn Settings")]
        [SerializeField] private float minDistanceFromPlayer = 60f;
        [SerializeField] private float maxDistanceFromPlayer = 150f;
        [SerializeField] private float minDistanceBetweenIslands = 80f;
        [SerializeField] private float despawnDistance = 300f;
        [SerializeField] private int maxActiveIslands = 5;
        [SerializeField] private float checkIntervalSeconds = 5f;

        private readonly List<GameObject> activeIslands = new List<GameObject>();
        private float timer;

        private void Update()
        {
            if (player == null) return;

            timer += Time.deltaTime;
            if (timer >= checkIntervalSeconds)
            {
                timer = 0f;
                CleanupFarIslands();

                if (activeIslands.Count < maxActiveIslands)
                {
                    TrySpawnIsland();
                }
            }
        }

        private void TrySpawnIsland()
        {
            if (islandPrefabs.Length == 0) return;

            const int maxAttempts = 10;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                float dist = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
                Vector3 candidatePos = player.position + new Vector3(randomDir.x, 0f, randomDir.y) * dist;
                candidatePos.y = 0f; // islands sit at sea level

                if (IsFarEnoughFromOtherIslands(candidatePos))
                {
                    GameObject prefab = islandPrefabs[Random.Range(0, islandPrefabs.Length)];
                    GameObject island = Instantiate(prefab, candidatePos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                    activeIslands.Add(island);
                    return;
                }
            }
        }

        private bool IsFarEnoughFromOtherIslands(Vector3 candidatePos)
        {
            foreach (var island in activeIslands)
            {
                if (island == null) continue;
                if (Vector3.Distance(island.transform.position, candidatePos) < minDistanceBetweenIslands)
                {
                    return false;
                }
            }
            return true;
        }

        private void CleanupFarIslands()
        {
            for (int i = activeIslands.Count - 1; i >= 0; i--)
            {
                GameObject island = activeIslands[i];

                if (island == null)
                {
                    activeIslands.RemoveAt(i);
                    continue;
                }

                float dist = Vector3.Distance(player.position, island.transform.position);
                if (dist > despawnDistance)
                {
                    Destroy(island);
                    activeIslands.RemoveAt(i);
                }
            }
        }
    }
}
