using UnityEngine;

namespace RaftSurvival.Combat
{
    /// <summary>
    /// Generic health component used by the player, island creatures,
    /// pirates, and bosses. Handles damage, death, and regen (optional).
    /// Attach alongside an Animator that has a "Hit" trigger and "Die" trigger
    /// for reacting to damage/death visually.
    /// </summary>
    public class HealthSystem : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Regeneration (optional, mainly for player)")]
        [SerializeField] private bool canRegenerate = false;
        [SerializeField] private float regenPerSecond = 1f;
        [SerializeField] private float regenDelayAfterHit = 5f;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        private static readonly int AnimHit = Animator.StringToHash("Hit");
        private static readonly int AnimDie = Animator.StringToHash("Die");

        private float lastHitTime = -999f;

        public bool IsAlive => currentHealth > 0f;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        public delegate void HealthChanged(float current, float max);
        public event HealthChanged OnHealthChanged;

        public delegate void Died();
        public event Died OnDied;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Update()
        {
            if (canRegenerate && IsAlive && Time.time - lastHitTime > regenDelayAfterHit)
            {
                if (currentHealth < maxHealth)
                {
                    currentHealth = Mathf.Min(maxHealth, currentHealth + regenPerSecond * Time.deltaTime);
                    OnHealthChanged?.Invoke(currentHealth, maxHealth);
                }
            }
        }

        public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (!IsAlive || amount <= 0f) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            lastHitTime = Time.time;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            animator?.SetTrigger(AnimHit);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Die()
        {
            animator?.SetTrigger(AnimDie);
            OnDied?.Invoke();
        }
    }
}
