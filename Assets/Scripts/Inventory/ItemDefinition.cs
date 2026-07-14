using UnityEngine;

namespace RaftSurvival.Inventory
{
    /// <summary>
    /// Defines a single item type in the game (resource, plank, weapon, etc).
    /// Created as ScriptableObject assets in the editor — one asset per item
    /// (e.g. "Item_WoodenPlank.asset", "Item_MetalScrap.asset").
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "RaftSurvival/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        public string itemId;          // unique string id, e.g. "wooden_plank"
        public string displayName;     // shown in UI, e.g. "Wooden Plank"
        public Sprite icon;            // UI icon
        public GameObject worldPrefab; // prefab used when dropped/floating in ocean
        public int maxStackSize = 99;

        public enum ItemCategory
        {
            RawResource,   // logs, metal scrap, plastic, rope, fish
            CraftedMaterial, // wooden plank (after scrap cutting)
            BuildingPiece, // wall, floor, ceiling
            Weapon,
            Tool,
            Placeable      // lamp, etc.
        }

        public ItemCategory category;
    }
}
