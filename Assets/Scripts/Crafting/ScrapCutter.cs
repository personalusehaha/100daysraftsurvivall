using System.Collections;
using UnityEngine;
using RaftSurvival.Inventory;

namespace RaftSurvival.Crafting
{
    /// <summary>
    /// The "Scrap Cutter" station. Player inserts raw materials (Logs, Metal
    /// Scrap, Plastic) and it converts them into standardized "plane" blocks
    /// (Wooden Plank, Metal Plate, Plastic Sheet) at a fixed conversion
    /// ratio. These processed materials are then used by CraftingManager
    /// recipes to build walls/floor/ceiling/lamps/weapons.
    /// </summary>
    public class ScrapCutter : MonoBehaviour
    {
        [System.Serializable]
        public struct ConversionRule
        {
            public ItemDefinition rawInput;
            public int inputAmount;
            public ItemDefinition processedOutput;
            public int outputAmount;
        }

        [Header("Conversion Rules")]
        [Tooltip("e.g. 1 Log -> 3 Wooden Planks, 1 Metal Scrap -> 2 Metal Plates")]
        [SerializeField] private ConversionRule[] conversionRules;

        [SerializeField] private float processTimeSeconds = 1f;

        private bool isProcessing;

        public delegate void ProcessStarted(ConversionRule rule);
        public delegate void ProcessCompleted(ConversionRule rule);
        public event ProcessStarted OnProcessStarted;
        public event ProcessCompleted OnProcessCompleted;

        /// <summary>
        /// Attempts to cut/process the given raw item. Call this from a UI
        /// button that lists the raw materials the player currently holds.
        /// </summary>
        public bool TryProcess(ItemDefinition rawItem)
        {
            if (isProcessing || rawItem == null || PlayerInventory.Instance == null) return false;

            foreach (var rule in conversionRules)
            {
                if (rule.rawInput != rawItem) continue;

                if (!PlayerInventory.Instance.HasEnough(rule.rawInput, rule.inputAmount))
                    return false;

                StartCoroutine(ProcessRoutine(rule));
                return true;
            }

            return false; // no matching rule for this item
        }

        private IEnumerator ProcessRoutine(ConversionRule rule)
        {
            isProcessing = true;

            PlayerInventory.Instance.RemoveItem(rule.rawInput, rule.inputAmount);
            OnProcessStarted?.Invoke(rule);

            yield return new WaitForSeconds(processTimeSeconds);

            PlayerInventory.Instance.AddItem(rule.processedOutput, rule.outputAmount);
            OnProcessCompleted?.Invoke(rule);

            isProcessing = false;
        }

        public ConversionRule[] GetConversionRules() => conversionRules;
    }
}
