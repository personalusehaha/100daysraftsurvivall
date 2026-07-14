
using System.Collections.Generic;
using UnityEngine;

namespace RaftSurvival.Inventory
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemDefinition item;
        public int quantity;

        public bool IsEmpty => item == null || quantity <= 0;
    }

    /// <summary>
    /// Holds the player's carried items. Used by ScrapCutter and
    /// CraftingManager to check/consume/add resources. Attach to the
    /// Player GameObject (one per player, singleton-style access for
    /// simplicity in a single-player Phase 1-2 build).
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory Instance { get; private set; }

        [SerializeField] private int slotCount = 24;
        private List<InventorySlot> slots;

        public delegate void InventoryChanged();
        public event InventoryChanged OnInventoryChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            slots = new List<InventorySlot>(slotCount);
            for (int i = 0; i < slotCount; i++)
            {
                slots.Add(new InventorySlot());
            }
        }

        public int GetItemCount(ItemDefinition item)
        {
            int total = 0;
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty && slot.item == item)
                    total += slot.quantity;
            }
            return total;
        }

        public bool HasEnough(ItemDefinition item, int amount)
        {
            return GetItemCount(item) >= amount;
        }

        /// <summary>Adds an item, stacking into existing slots first, then empty slots.</summary>
        public bool AddItem(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0) return false;

            // Fill existing stacks first.
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty && slot.item == item && slot.quantity < item.maxStackSize)
                {
                    int space = item.maxStackSize - slot.quantity;
                    int toAdd = Mathf.Min(space, amount);
                    slot.quantity += toAdd;
                    amount -= toAdd;
                    if (amount <= 0) { NotifyChanged(); return true; }
                }
            }

            // Then fill empty slots.
            foreach (var slot in slots)
            {
                if (slot.IsEmpty && amount > 0)
                {
                    int toAdd = Mathf.Min(item.maxStackSize, amount);
                    slot.item = item;
                    slot.quantity = toAdd;
                    amount -= toAdd;
                }
                if (amount <= 0) break;
            }

            NotifyChanged();
            return amount <= 0; // false if inventory was full and couldn't fit everything
        }

        /// <summary>Removes up to 'amount' of item. Returns true if fully removed.</summary>
        public bool RemoveItem(ItemDefinition item, int amount)
        {
            if (item == null || amount <= 0) return false;
            if (!HasEnough(item, amount)) return false;

            foreach (var slot in slots)
            {
                if (slot.IsEmpty || slot.item != item) continue;

                int toRemove = Mathf.Min(slot.quantity, amount);
                slot.quantity -= toRemove;
                amount -= toRemove;

                if (slot.quantity <= 0)
                {
                    slot.item = null;
                    slot.quantity = 0;
                }

                if (amount <= 0) break;
            }

            NotifyChanged();
            return true;
        }

        public IReadOnlyList<InventorySlot> GetSlots() => slots;

        private void NotifyChanged() => OnInventoryChanged?.Invoke();
    }
}
