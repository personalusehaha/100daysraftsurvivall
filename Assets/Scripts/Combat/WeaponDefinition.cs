using UnityEngine;
using RaftSurvival.Inventory;

namespace RaftSurvival.Combat
{
    /// <summary>
    /// Defines a craftable/usable weapon's combat stats. Pairs with an
    /// ItemDefinition (category = Weapon) so it can live in the inventory
    /// and crafting system, while this asset holds combat-specific data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "RaftSurvival/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        public ItemDefinition linkedItem;

        public enum WeaponType { Melee, Ranged }
        public WeaponType weaponType;

        [Header("Combat Stats")]
        public float damage = 10f;
        public float attackRange = 1.5f;
        public float attackCooldown = 0.6f;

        [Header("Melee Swing (if weaponType == Melee)")]
        public float swingRadius = 1f;
        public float swingAngle = 90f; // degrees, forward cone

        [Header("Ranged (if weaponType == Ranged)")]
        public GameObject projectilePrefab;
        public float projectileSpeed = 20f;

        [Header("Animation")]
        [Tooltip("Trigger name in the Animator for this weapon's attack animation.")]
        public string attackAnimTrigger = "AttackMelee";
    }
}
