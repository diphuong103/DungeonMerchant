using System;
using UnityEngine;

namespace DungeonMerchant.Dungeons
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class DungeonEnemyBase : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxHealth = 3;
        [SerializeField, Min(0f)] private float moveSpeed = 1.5f;
        [SerializeField, Min(0.1f)] private float chaseDistance = 7f;
        [SerializeField, Min(0.1f)] private float meleeRange = 0.9f;
        [SerializeField, Min(0.05f)] private float attackCooldown = 1.1f;
        [SerializeField, Min(1)] private int attackDamage = 1;
        [SerializeField] private GameObject lootPrefab;
        [SerializeField] private bool isBoss;

        private Rigidbody2D body;
        private Transform player;
        private int health;
        private float nextAttackTime;
        private bool dead;

        public event Action<DungeonEnemyBase> Died;
        public bool IsBoss => isBoss;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            health = maxHealth;
            body.gravityScale = 0f;
            body.freezeRotation = true;
        }

        private void Update()
        {
            if (dead) return;
            if (player == null)
            {
                GameObject target = GameObject.FindGameObjectWithTag("Player");
                if (target != null) player = target.transform;
            }
            if (player == null) return;

            Vector2 toPlayer = (Vector2)(player.position - transform.position);
            float distance = toPlayer.magnitude;
            if (distance <= chaseDistance && distance > meleeRange)
                body.linearVelocity = toPlayer.normalized * moveSpeed;
            else
                body.linearVelocity = Vector2.zero;

            if (distance <= meleeRange && Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                DungeonPlayerHealth healthTarget = player.GetComponent<DungeonPlayerHealth>();
                if (healthTarget != null) healthTarget.TakeDamage(attackDamage);
            }
        }

        private void OnDisable()
        {
            if (body != null) body.linearVelocity = Vector2.zero;
        }

        public void TakeDamage(int damage)
        {
            if (dead || damage <= 0) return;
            health -= damage;
            if (health > 0) return;

            dead = true;
            body.linearVelocity = Vector2.zero;
            if (lootPrefab != null)
            {
                GameObject drop = Instantiate(lootPrefab, transform.position, Quaternion.identity);
                Destroy(drop, 12f);
            }
            Died?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
