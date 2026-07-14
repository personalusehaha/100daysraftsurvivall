using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RaftSurvival.Core;

namespace RaftSurvival.Enemies
{
    /// <summary>
    /// Triggers a pirate raid every 5th night, per the game spec. Spawns a
    /// wave of pirate enemies near the raft (arriving by boat conceptually
    /// — actual boat visuals are an Asset Requirement), and despawns any
    /// survivors when the raid ends (dawn) to avoid endless clutter.
    /// </summary>
    public class PirateRaidManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DayNightCycle dayNightCycle;
        [SerializeField] private Transform player;
        [SerializeField] private GameObject[] piratePrefabs;

        [Header("Raid Timing")]
        [Tooltip("A raid happens every Nth night (5 = every 5th day/night per spec).")]
        [SerializeField] private int raidEveryNDays = 5;

        [Tooltip("Time of day (0-1) when night begins, raid can start.")]
        [SerializeField] private float nightStartTime = 0.8f;

        [Header("Raid Composition")]
        [SerializeField] private int minPirates = 2;
        [SerializeField] private int maxPirates = 5;
        [SerializeField] private float spawnRadius = 15f;
        [SerializeField] private float spawnRadiusVariance = 5f;

        private readonly List<GameObject> activeRaiders = new List<GameObject>();
        private bool raidTriggeredTonight = false;
        private bool wasNightLastFrame = false;

        public delegate void RaidStarted(int dayNumber);
        public delegate void RaidEnded();
        public event RaidStarted OnRaidStarted;
        public event RaidEnded OnRaidEnded;

        private void Awake()
        {
            if (dayNightCycle == null)
                Debug.LogWarning("[PirateRaidManager] DayNightCycle reference missing.");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDayChanged += HandleDayChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDayChanged -= HandleDayChanged;
            }
        }

        private void HandleDayChanged(int newDay)
        {
            // Reset the "already raided tonight" flag whenever a new day starts.
            raidTriggeredTonight = false;
        }

        private void Update()
        {
            if (dayNightCycle == null || GameManager.Instance == null) return;

            float t = dayNightCycle.GetNormalizedTimeOfDay();
            bool isNight = t >= nightStartTime || t < 0.2f;

            // Trigger raid exactly once when night begins, on eligible days.
            if (isNight && !wasNightLastFrame && !raidTriggeredTonight)
            {
                int currentDay = GameManager.Instance.CurrentDay;
                if (currentDay % raidEveryNDays == 0)
                {
                    StartRaid(currentDay);
                }
                raidTriggeredTonight = true;
            }

            // End raid at dawn — clean up survivors.
            if (!isNight && wasNightLastFrame && activeRaiders.Count > 0)
            {
                EndRaid();
            }

            wasNightLastFrame = isNight;
        }

        private void StartRaid(int dayNumber)
        {
            if (piratePrefabs.Length == 0 || player == null) return;

            int count = Random.Range(minPirates, maxPirates + 1);

            for (int i = 0; i < count; i++)
            {
                SpawnPirate();
            }

            OnRaidStarted?.Invoke(dayNumber);
            Debug.Log($"[PirateRaidManager] Pirate raid started on Day {dayNumber} — {count} pirates.");
        }

        private void SpawnPirate()
        {
            GameObject prefab = piratePrefabs[Random.Range(0, piratePrefabs.Length)];

            Vector2 dir = Random.insideUnitCircle.normalized;
            float radius = spawnRadius + Random.Range(-spawnRadiusVariance, spawnRadiusVariance);
            Vector3 spawnPos = player.position + new Vector3(dir.x, 0f, dir.y) * radius;

            GameObject pirate = Instantiate(prefab, spawnPos, Quaternion.identity);

            EnemyAI ai = pirate.GetComponent<EnemyAI>();
            ai?.SetTarget(player);

            activeRaiders.Add(pirate);
        }

        private void EndRaid()
        {
            for (int i = activeRaiders.Count - 1; i >= 0; i--)
            {
                if (activeRaiders[i] != null)
                {
                    Destroy(activeRaiders[i]);
                }
            }
            activeRaiders.Clear();

            OnRaidEnded?.Invoke();
            Debug.Log("[PirateRaidManager] Raid ended — survivors cleared at dawn.");
        }
    }
}
