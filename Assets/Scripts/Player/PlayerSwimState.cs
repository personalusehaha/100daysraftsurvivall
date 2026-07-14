using UnityEngine;
using RaftSurvival.Core;
using RaftSurvival.Ocean;

namespace RaftSurvival.Player
{
    /// <summary>
    /// Handles the transition between "on raft / on land" and "swimming"
    /// states. Keeps the player locked to the water SURFACE only — per
    /// spec, there is no underwater swimming in Phase 1.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerSwimState : MonoBehaviour
    {
        [SerializeField] private float waterEntryThreshold = 0.1f;
        [SerializeField] private Animator animator;

        private CharacterController controller;
        private bool isSwimming;

        public bool IsSwimming => isSwimming;

        private static readonly int AnimIsSwimming = Animator.StringToHash("IsSwimming");

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (GerstnerOcean.Instance == null) return;

            float waterHeight = GerstnerOcean.Instance.GetWaveHeight(transform.position.x, transform.position.z);
            bool touchingWater = transform.position.y <= waterHeight + waterEntryThreshold;

            // Only auto-enter swim state if not currently standing on the raft
            // (raft deck collider keeps the player above water height already).
            if (touchingWater && !isSwimming && !IsOnRaftSurface())
            {
                SetSwimming(true);
            }

            if (isSwimming)
            {
                // Lock player to the water surface — never sink below it.
                Vector3 pos = transform.position;
                pos.y = waterHeight;
                transform.position = pos;
            }
        }

        private bool IsOnRaftSurface()
        {
            return GameManager.Instance != null &&
                   GameManager.Instance.CurrentState == GameManager.GameState.PlayerOnRaft;
        }

        /// <summary>
        /// Called externally (RaftClimb, or when player touches an island/beach)
        /// to toggle swim state and update animation + game state accordingly.
        /// </summary>
        public void SetSwimming(bool swimming)
        {
            if (isSwimming == swimming) return;

            isSwimming = swimming;

            if (animator != null)
            {
                animator.SetBool(AnimIsSwimming, isSwimming);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(
                    isSwimming ? GameManager.GameState.PlayerSwimming
                               : GameManager.GameState.PlayerOnRaft);
            }
        }
    }
}
