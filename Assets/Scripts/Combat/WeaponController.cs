using UnityEngine;

namespace RaftSurvival.Combat
{
    /// <summary>
    /// Handles the player's currently equipped weapon: triggers attack
    /// animations, performs melee hit-detection or spawns projectiles,
    /// and applies damage to anything implementing IDamageable.
    /// Attach to the Player GameObject.
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform attackOrigin; // e.g. hand or weapon tip
        [SerializeField] private LayerMask hittableLayers;

        [Header("Equipped Weapon")]
        [SerializeField] private WeaponDefinition equippedWeapon;

        private float lastAttackTime = -999f;

        public void EquipWeapon(WeaponDefinition weapon)
        {
            equippedWeapon = weapon;
        }

        /// <summary>
        /// Call this from an on-screen "Attack" button (mobile UI).
        /// </summary>
        public void TryAttack()
        {
            if (equippedWeapon == null) return;
            if (Time.time - lastAttackTime < equippedWeapon.attackCooldown) return;

            lastAttackTime = Time.time;

            if (!string.IsNullOrEmpty(equippedWeapon.attackAnimTrigger))
            {
                animator?.SetTrigger(equippedWeapon.attackAnimTrigger);
            }

            if (equippedWeapon.weaponType == WeaponDefinition.WeaponType.Melee)
            {
                // Actual hit detection is called from an Animation Event
                // (see PerformMeleeHit) so damage lands exactly when the
                // swing animation connects, not on button press.
            }
            else
            {
                FireProjectile();
            }
        }

        /// <summary>
        /// IMPORTANT: Call this via an Animation Event placed on the attack
        /// animation clip's "impact" frame — this syncs damage timing with
        /// the visual swing instead of applying damage instantly on input.
        /// </summary>
        public void PerformMeleeHit()
        {
            if (equippedWeapon == null || attackOrigin == null) return;

            Collider[] hits = Physics.OverlapSphere(attackOrigin.position, equippedWeapon.swingRadius, hittableLayers);

            foreach (var hit in hits)
            {
                Vector3 toTarget = (hit.transform.position - attackOrigin.position).normalized;
                float angle = Vector3.Angle(attackOrigin.forward, toTarget);

                if (angle <= equippedWeapon.swingAngle * 0.5f)
                {
                    IDamageable damageable = hit.GetComponent<IDamageable>();
                    if (damageable != null && damageable.IsAlive)
                    {
                        damageable.TakeDamage(equippedWeapon.damage, hit.ClosestPoint(attackOrigin.position), toTarget);
                    }
                }
            }
        }

        private void FireProjectile()
        {
            if (equippedWeapon.projectilePrefab == null || attackOrigin == null) return;

            GameObject proj = Instantiate(equippedWeapon.projectilePrefab, attackOrigin.position, attackOrigin.rotation);

            if (proj.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = attackOrigin.forward * equippedWeapon.projectileSpeed;
            }

            var projectileScript = proj.GetComponent<Projectile>();
            projectileScript?.SetDamage(equippedWeapon.damage);
        }

        private void OnDrawGizmosSelected()
        {
            if (attackOrigin == null || equippedWeapon == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackOrigin.position, equippedWeapon.swingRadius);
        }
    }
}
