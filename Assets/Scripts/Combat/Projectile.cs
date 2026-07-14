using UnityEngine;

namespace RaftSurvival.Combat
{
    /// <summary>
    /// Simple projectile for ranged weapons (e.g. spear gun, bow).
    /// Moves forward via Rigidbody velocity (set by WeaponController),
    /// applies damage on hit, and self-destructs after a lifetime or
    /// on impact.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetimeSeconds = 5f;
        [SerializeField] private LayerMask hittableLayers;

        private float damage;
        private bool hasHit;

        public void SetDamage(float amount) => damage = amount;

        private void Start()
        {
            Destroy(gameObject, lifetimeSeconds);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasHit) return;

            // Only react to layers we care about.
            if ((hittableLayers.value & (1 << other.gameObject.layer)) == 0) return;

            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null && damageable.IsAlive)
            {
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitDir = (other.transform.position - transform.position).normalized;
                damageable.TakeDamage(damage, hitPoint, hitDir);
            }

            hasHit = true;
            Destroy(gameObject);
        }
    }
}
