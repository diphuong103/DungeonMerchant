using System;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class EnemyBase : MonoBehaviour
    {
        [Header("Health and loot")]
        [Min(1)] public int MaxHP = 3;
        public GameObject LootDrop;
        [Range(0f, 1f)] public float LootDropChance = 1f;

        [Header("Target and combat")]
        public Transform Target;
        [Min(0f)] public float ChaseRange = 7f;
        [Min(0f)] public float MoveSpeed = 1.5f;
        [Min(0.1f)] public float MeleeRange = 0.9f;
        [Min(1)] public int AttackDamage = 1;
        [Min(0.05f)] public float AttackCooldown = 1f;

        public int HP { get; private set; }
        public float HealthPercent => MaxHP <= 0 ? 0f : (float)HP / MaxHP;
        public event Action<EnemyBase> Die;

        protected Rigidbody2D Body { get; private set; }
        private float nextAttackTime;
        private bool isDead;

        protected virtual void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            HP = Mathf.Max(1, MaxHP);
            Body.gravityScale = 0f;
            Body.freezeRotation = true;
        }

        protected virtual void FixedUpdate()
        {
            if (isDead || Target == null)
            {
                StopMoving();
                return;
            }

            Vector2 toTarget = (Vector2)(Target.position - transform.position);
            float distance = toTarget.magnitude;
            if (distance > ChaseRange)
            {
                StopMoving();
                return;
            }

            if (distance <= MeleeRange)
            {
                StopMoving();
                TryMeleeAttack();
                return;
            }

            Body.linearVelocity = toTarget.normalized * MoveSpeed;
        }

        public virtual void TakeDamage(int amount)
        {
            if (isDead || amount <= 0) return;
            HP = Mathf.Max(0, HP - amount);
            OnHealthChanged();
            if (HP == 0) HandleDeath();
        }

        public void SetTarget(Transform target) => Target = target;

        protected virtual void OnHealthChanged() { }

        protected virtual void OnDeath()
        {
            if (LootDrop != null && UnityEngine.Random.value <= LootDropChance)
                Instantiate(LootDrop, transform.position, Quaternion.identity);
        }

        private void TryMeleeAttack()
        {
            if (Time.time < nextAttackTime) return;
            nextAttackTime = Time.time + AttackCooldown;

            IDamageable damageable = DamageUtility.FindDamageable(Target);
            if (damageable != null)
                damageable.TakeDamage(AttackDamage);
            else
                Target.SendMessage("TakeDamage", AttackDamage, SendMessageOptions.DontRequireReceiver);
        }

        private void HandleDeath()
        {
            if (isDead) return;
            isDead = true;
            StopMoving();
            OnDeath();
            Die?.Invoke(this);
            Destroy(gameObject);
        }

        private void StopMoving()
        {
            if (Body != null) Body.linearVelocity = Vector2.zero;
        }
    }
}
