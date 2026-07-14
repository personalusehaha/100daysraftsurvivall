using UnityEngine;
using RaftSurvival.Inventory;

namespace RaftSurvival.Building
{
    /// <summary>
    /// Handles ghost-preview placement of building pieces (walls, floor,
    /// ceiling) onto valid snap points on the raft. Attach to the Player
    /// or a dedicated "BuildController" GameObject.
    ///
    /// Design: raft has empty "BuildSlot" child transforms placed at grid
    /// positions in the editor. Player selects a building piece from
    /// inventory, aims at a slot, and confirms placement.
    /// </summary>
    public class BuildPlacer : MonoBehaviour
    {
        [Header("Placement")]
        [SerializeField] private LayerMask buildSlotLayer;
        [SerializeField] private float placeRange = 4f;
        [SerializeField] private Camera playerCamera;

        [Header("Ghost Preview")]
        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;

        private GameObject currentGhost;
        private ItemDefinition selectedBuildItem;
        private BuildSlot targetSlot;

        public void SelectBuildItem(ItemDefinition item)
        {
            ClearGhost();
            selectedBuildItem = item;

            if (item != null && item.worldPrefab != null)
            {
                currentGhost = Instantiate(item.worldPrefab);
                SetGhostMaterial(currentGhost, invalidPlacementMaterial);
                SetGhostCollidersEnabled(currentGhost, false);
            }
        }

        private void Update()
        {
            if (currentGhost == null || selectedBuildItem == null) return;

            UpdateGhostPosition();

            if (Input.GetMouseButtonDown(0) && targetSlot != null && !targetSlot.IsOccupied)
            {
                ConfirmPlacement();
            }
        }

        private void UpdateGhostPosition()
        {
            targetSlot = null;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, placeRange, buildSlotLayer))
            {
                BuildSlot slot = hit.collider.GetComponent<BuildSlot>();
                if (slot != null)
                {
                    targetSlot = slot;
                    currentGhost.transform.position = slot.transform.position;
                    currentGhost.transform.rotation = slot.transform.rotation;
                    SetGhostMaterial(currentGhost, slot.IsOccupied ? invalidPlacementMaterial : validPlacementMaterial);
                    return;
                }
            }

            // No valid slot found — follow camera loosely as feedback.
            currentGhost.transform.position = playerCamera.transform.position + playerCamera.transform.forward * 2f;
            SetGhostMaterial(currentGhost, invalidPlacementMaterial);
        }

        private void ConfirmPlacement()
        {
            if (PlayerInventory.Instance == null) return;
            if (!PlayerInventory.Instance.RemoveItem(selectedBuildItem, 1)) return;

            GameObject placed = Instantiate(selectedBuildItem.worldPrefab, targetSlot.transform.position, targetSlot.transform.rotation);
            targetSlot.MarkOccupied(placed);

            ClearGhost();
        }

        private void ClearGhost()
        {
            if (currentGhost != null) Destroy(currentGhost);
            currentGhost = null;
            selectedBuildItem = null;
            targetSlot = null;
        }

        private void SetGhostMaterial(GameObject ghost, Material mat)
        {
            foreach (var renderer in ghost.GetComponentsInChildren<Renderer>())
            {
                renderer.material = mat;
            }
        }

        private void SetGhostCollidersEnabled(GameObject ghost, bool enabled)
        {
            foreach (var col in ghost.GetComponentsInChildren<Collider>())
            {
                col.enabled = enabled;
            }
        }
    }
}
