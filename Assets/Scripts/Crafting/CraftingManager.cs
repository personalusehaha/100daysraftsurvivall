using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RaftSurvival.Inventory;

namespace RaftSurvival.Crafting
{
    /// <summary>
    /// Represents an in-world "Crafting Machine" the player interacts with.
    /// Holds a list of available recipes and processes crafting requests
    /// (consume ingredients -> wait craftTime -> spawn/add result item).
    /// Attach to the crafting machine prefab/GameObject placed on the raft.
    /// </summary>
    public class CraftingManager : MonoBehaviour
    {
        [Header("Available Recipes At This Station")]
        [SerializeField] private List<CraftingRecipe> availableRecipes;

        [Header("Output")]
        [Tooltip("If true, crafted items go straight into inventory. If false, they spawn in the world at outputPoint.")]
        [SerializeField] private bool outputToInventory = true;
        [SerializeField] private Transform outputPoint;

        public delegate void CraftStarted(CraftingRecipe recipe);
        public delegate void CraftCompleted(CraftingRecipe recipe);
        public event CraftStarted OnCraftStarted;
        public event CraftCompleted OnCraftCompleted;

        private bool isCrafting;

        public IReadOnlyList<CraftingRecipe> GetAvailableRecipes(int currentDay)
        {
            var unlocked = new List<CraftingRecipe>();
            foreach (var r in availableRecipes)
            {
                if (r.unlockDay <= currentDay) unlocked.Add(r);
            }
            return unlocked;
        }

        public bool TryCraft(CraftingRecipe recipe)
        {
            if (isCrafting) return false;
            if (recipe == null || PlayerInventory.Instance == null) return false;
            if (!recipe.CanCraft(PlayerInventory.Instance)) return false;

            StartCoroutine(CraftRoutine(recipe));
            return true;
        }

        private IEnumerator CraftRoutine(CraftingRecipe recipe)
        {
            isCrafting = true;

            // Consume ingredients up front.
            foreach (var ing in recipe.ingredients)
            {
                PlayerInventory.Instance.RemoveItem(ing.item, ing.amount);
            }

            OnCraftStarted?.Invoke(recipe);

            yield return new WaitForSeconds(recipe.craftTimeSeconds);

            GiveResult(recipe);
            OnCraftCompleted?.Invoke(recipe);

            isCrafting = false;
        }

        private void GiveResult(CraftingRecipe recipe)
        {
            if (outputToInventory && PlayerInventory.Instance != null)
            {
                bool added = PlayerInventory.Instance.AddItem(recipe.resultItem, recipe.resultAmount);
                if (!added)
                {
                    Debug.LogWarning("[CraftingManager] Inventory full — dropping crafted item in world instead.");
                    SpawnInWorld(recipe);
                }
            }
            else
            {
                SpawnInWorld(recipe);
            }
        }

        private void SpawnInWorld(CraftingRecipe recipe)
        {
            if (recipe.resultItem.worldPrefab == null || outputPoint == null) return;

            for (int i = 0; i < recipe.resultAmount; i++)
            {
                Instantiate(recipe.resultItem.worldPrefab, outputPoint.position, Quaternion.identity);
            }
        }
    }
}
