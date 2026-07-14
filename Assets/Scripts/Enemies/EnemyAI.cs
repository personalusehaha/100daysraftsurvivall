using UnityEngine;
using UnityEngine.AI;
using RaftSurvival.Combat;

namespace RaftSurvival.Enemies
{
    /// <summary>
    /// Base AI for hostile creatures (island monsters, pirates). Uses
    /// Unity's NavMeshAgent to chase the player and attack when in range.
    /// Derived/paired with HealthSystem for damage handling. Works for
    /// both island-based creatures and boarding pirates.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(HealthSystem))]
    public class EnemyAI : MonoBehaviour
    {
        public enum State { Idle, Chasing, Attacking, Dead }

        [Header("Target")]
        [SerializeField] private Transform target; // usually the player

        [Header("Detection")]
        [SerializeField] private float detectionRadius = 12f;
        [SerializeField] private float attackRange = 1.8f;
        [SerializeField] private float loseTargetRadius = 20f;

        [Header("Combat")]
        [SerializeField] private float attackDamage = 15f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private LayerMask targetLayer;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        private static readonly int AnimSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimAttack = Animator.StringToHash("Attack");

        private NavMeshAgent agent;
        private HealthSystem health;
        private State currentState = State.Idle;
        private float lastAttackTime = -999f;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<HealthSystem>();
            health.OnDied += HandleDeath;
        }

        private void OnDestroy()
        {
            if (health != null) health.OnDied -= HandleDeath;
        }

        private void Update()
        {
            if (currentState == State.Dead || target == null) return;

            float distToTarget = Vector3.Distance(transform.position, target.position);

            switch (currentState)
            {
                case State.Idle:
                    if (distToTarget <= detectionRadius)
                    {
                        currentState = State.Chasing;
                    }
                    break;

                case State.Chasing:
                    ChaseTarget(distToTarget);
                    break;

                case State.Attacking:
                    AttackTarget(distToTarget);
                    break;
            }
        }

        private void ChaseTarget(float distToTarget)
        {
            if (distToTarget > loseTargetRadius)
            {
                currentState = State.Idle;
                agent.ResetPath();
                return;
            }

            if (distToTarget <= attackRange)
            {
                currentState = State.Attacking;
                agent.ResetPath();
                return;
            }

            agent.SetDestination(target.position);
            animator?.SetFloat(AnimSpeed, agent.velocity.magnitude);
        }

        private void AttackTarget(float distToTarget)
        {
            if (distToTarget > attackRange)
            {
                currentState = State.Chasing;
                return;
            }

            // Face the target.
            Vector3 dir = (target.position - transform.position);
            dir.y = 0f;
            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 8f);
            }

            animator?.SetFloat(AnimSpeed, 0f);

            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                animator?.SetTrigger(AnimAttack);
                // Actual damage application should be called from an
                // Animation Event (see DealAttackDamage) for correct timing.
            }
        }

        /// <summary>
        /// Called via Animation Event on the attack clip's impact frame.
        /// </summary>
        public void DealAttackDamage()
        {
            if (target == null) return;

            float dist = Vector3.Distance(transform.position, target.position);
            if (dist > attackRange * 1.3f) return; // target moved away just in time

            IDamageable damageable = target.GetComponent<IDamageable>();
            damageable?.TakeDamage(attackDamage, target.position, (target.position - transform.position).normalized);
        }

        private void HandleDeath()
        {
            currentState = State.Dead;
            agent.isStopped = true;
            agent.enabled = false;

            // Disable colliders so the corpse doesn't block navigation/attacks.
            foreach (var col in GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            Destroy(gameObject, 5f); // cleanup after death animation plays
        }

        public void SetTarget(Transform newTarget) => target = newTarget;
    }
}
