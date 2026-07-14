using UnityEngine;

namespace RaftSurvival.Building
{
    /// <summary>
    /// Marks a valid snap point on the raft where a building piece
    /// (wall/floor/ceiling) can be placed. Place these as empty child
    /// GameObjects on a grid across the raft, each with a small trigger
    /// Collider so BuildPlacer's raycast can detect them.
    /// </summary>
    public class BuildSlot : MonoBehaviour
    {
        public enum SlotType { Floor, Wall, Ceiling }

        [SerializeField] private SlotType slotType;
        public SlotType Type => slotType;

        public bool IsOccupied { get; private set; }
        private GameObject occupyingPiece;

        public void MarkOccupied(GameObject piece)
        {
            IsOccupied = true;
            occupyingPiece = piece;
        }

        public void ClearSlot()
        {
            if (occupyingPiece != null) Destroy(occupyingPiece);
            occupyingPiece = null;
            IsOccupied = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = IsOccupied ? Color.red : Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.3f);
        }
    }
}
