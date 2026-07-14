using UnityEngine;
using RaftSurvival.Core;

namespace RaftSurvival.Raft
{
    /// <summary>
    /// Detects when the player enters the raft's climb-trigger zone while
    /// swimming, and allows them to climb aboard. Placed on a trigger
    /// collider around the raft's edge (slightly below deck height).
    /// </summary>
    public class RaftClimb : MonoBehaviour
    {
        [SerializeField] private Transform climbExitPoint; // where player lands on deck
        private bool playerInRange;
        private Transform playerTransform;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = true;
            playerTransform = other.transform;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            playerInRange = false;
            playerTransform = null;
        }

        private void Update()
        {
            if (!playerInRange || playerTransform == null) return;
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState != GameManager.GameState.PlayerSwimming) return;

            // Mobile: climb triggers automatically on contact.
            // (Swap for an on-screen "Climb" button call if you want manual control.)
            ClimbAboard();
        }

        public void ClimbAboard()
        {
            if (playerTransform == null || climbExitPoint == null) return;

            playerTransform.position = climbExitPoint.position;
            GameManager.Instance.SetState(GameManager.GameState.PlayerOnRaft);

            var swimState = playerTransform.GetComponent<RaftSurvival.Player.PlayerSwimState>();
            swimState?.SetSwimming(false);
        }
    }
}
