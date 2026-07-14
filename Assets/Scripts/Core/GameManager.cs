using UnityEngine;

namespace RaftSurvival.Core
{
    /// <summary>
    /// Central game state manager. Singleton that persists across the single
    /// ocean scene. Tracks day count, game state, and exposes events other
    /// systems (UI, spawners, audio) can subscribe to.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState
        {
            Playing,
            Paused,
            PlayerSwimming,
            PlayerOnRaft
        }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.PlayerOnRaft;
        public GameState CurrentState => currentState;

        [Header("Survival Day Counter")]
        [SerializeField] private int currentDay = 1;
        public int CurrentDay => currentDay;

        public delegate void DayChanged(int newDay);
        public event DayChanged OnDayChanged;

        public delegate void StateChanged(GameState newState);
        public event StateChanged OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Mobile performance target
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }

        public void SetState(GameState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            OnStateChanged?.Invoke(currentState);
        }

        /// <summary>
        /// Called by DayNightCycle whenever a full day completes.
        /// </summary>
        public void AdvanceDay()
        {
            currentDay++;
            OnDayChanged?.Invoke(currentDay);
        }
    }
}
