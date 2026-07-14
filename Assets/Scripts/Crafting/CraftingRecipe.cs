using UnityEngine;
using RaftSurvival.Inventory;

namespace RaftSurvival.Crafting
{
    [System.Serializable]
    public struct RecipeIngredient
    {
        public ItemDefinition item;
        public int amount;
    }

    /// <summary>
    /// Defines a craftable recipe: N ingredients -> 1 result item.
    /// Create one asset per recipe (e.g. "Recipe_Wall.asset").
    /// Used by both the CraftingManager (main crafting table) and could be
    /// reused for the ScrapCutter if desired.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "RaftSurvival/Crafting Recipe")]
    public class CraftingRecipe : ScriptableObject
    {
        public string recipeId;
        public ItemDefinition resultItem;
        public int resultAmount = 1;
        public RecipeIngredient[] ingredients;
        public float craftTimeSeconds = 1.5f;

        [Tooltip("Optional: minimum player 'level' or unlocked day before this recipe is available.")]
        public int unlockDay = 1;

        public bool CanCraft(PlayerInventory inventory)
        {
            if (inventory == null) return false;

            foreach (var ing in ingredients)
            {
                if (!inventory.HasEnough(ing.item, ing.amount))
                    return false;
            }
            return true;
        }
    }
}
