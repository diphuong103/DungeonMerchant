using UnityEngine;
using DungeonMerchant.Combat;
using DungeonMerchant.Player;
using DungeonMerchant.UI;

namespace DungeonMerchant.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemySpriteAnimator))]
    public sealed class EnemyBrain : MonoBehaviour, IDamageable
    {
        [Header("Identity and balance")]
        [SerializeField] private string enemyId = "skeleton_warrior";
        [SerializeField, Min(1)] private int maxHealth = 40;
        [SerializeField, Min(0)] private int damage = 8;
        [SerializeField, Min(0.1f)] private float moveSpeed = 2f;
        [SerializeField, Min(0.1f)] private float detectionRadius = 7f;
        [SerializeField, Min(0.1f)] private float attackRadius = 1.1f;
        [SerializeField, Min(0.1f)] private float attackCooldown = 1.4f;
        [SerializeField, Range(0f, 1f)] private float attackHitTime = 0.45f;
        [SerializeField, Min(0f)] private float patrolRadius = 2f;
        [SerializeField] private EnemyAttackType attackType;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 5f;
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private EnemyProjectile specialProjectilePrefab;
        [SerializeField] private AnimatedSpriteEffect attackEffectPrefab, hitEffectPrefab, deathEffectPrefab, telegraphEffectPrefab, blastEffectPrefab;
        [SerializeField, Min(0.1f)] private float specialCooldown = 12f;
        [SerializeField, Min(0.1f)] private float specialRadius = 2.8f;
        [SerializeField, Min(0.1f)] private float specialTelegraphDuration = 1.2f;
        [SerializeField] private GameObject lootPrefab;
        [SerializeField, Min(0f)] private float knockbackForce = 2f;
        [SerializeField] private bool startsIdle;
        private Rigidbody2D body;
        private Collider2D hitCollider;
        private EnemySpriteAnimator animationView;
        private Transform player;
        private PlayerHealth playerHealth;
        private EnemySpawner ownerPool;
        private int currentHealth;
        private EnemyMotionState state;
        private float attackReadyAt, actionTime, lostSightTimer, hurtUntil;
        private float specialReadyAt;
        private bool attackApplied, specialApplied;
        private Vector2 facing = Vector2.down;
        private Vector2 patrolGoal;
        private Vector2 home;
        private bool initialized;
        public string EnemyId => enemyId;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>(); hitCollider = GetComponent<Collider2D>(); animationView = GetComponent<EnemySpriteAnimator>();
            body.gravityScale = 0f; body.freezeRotation = true; body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        private void OnEnable()
        {
            if (!initialized) Initialize(null);
            if (startsIdle) SetState(EnemyMotionState.Idle); else SetState(EnemyMotionState.Patrol);
        }
        public void Initialize(EnemySpawner pool)
        {
            ownerPool = pool; home = transform.position; patrolGoal = home; currentHealth = maxHealth;
            var p = GameObject.FindGameObjectWithTag("Player");
            player = p != null ? p.transform : null; playerHealth = p != null ? p.GetComponent<PlayerHealth>() : null;
            initialized = true; attackReadyAt = Time.time + Random.Range(0f, attackCooldown * 0.5f); lostSightTimer = 0f;
            specialReadyAt = Time.time + specialCooldown;
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (hitCollider == null) hitCollider = GetComponent<Collider2D>();
            if (animationView == null) animationView = GetComponent<EnemySpriteAnimator>();
            if (body != null) { body.simulated = true; body.linearVelocity = Vector2.zero; }
            if (hitCollider != null) hitCollider.enabled = true;
        }
        private void Update()
        {
            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) { player = p.transform; playerHealth = p.GetComponent<PlayerHealth>(); }
            }
            if (state == EnemyMotionState.Dead) return;
            if (state == EnemyMotionState.Special) { UpdateSpecial(); return; }
            if (state == EnemyMotionState.Hurt)
            {
                if (Time.time >= hurtUntil) SetState(EnemyMotionState.Chase);
                return;
            }
            if (state == EnemyMotionState.Attack) { UpdateAttack(); return; }
            if (player != null && !playerHealthIsDead())
            {
                float distance = Vector2.Distance(transform.position, player.position);
                bool visible = distance <= detectionRadius && HasLineOfSight(player.position);
                if (visible) lostSightTimer = 0.6f; else lostSightTimer -= Time.deltaTime;
                if (visible && enemyId == "boss_death_lord" && Time.time >= specialReadyAt)
                {
                    BeginSpecial();
                    return;
                }
                if (visible && distance <= attackRadius)
                {
                    body.linearVelocity = Vector2.zero; facing = ((Vector2)player.position - (Vector2)transform.position).normalized;
                    animationView.SetFacing(facing);
                    if (Time.time >= attackReadyAt) BeginAttack(); else SetState(EnemyMotionState.Idle);
                    return;
                }
                if (lostSightTimer > 0f || visible)
                {
                    if (attackType == EnemyAttackType.Projectile && distance < attackRadius * 0.75f)
                        Move(ChooseSteering(((Vector2)transform.position - (Vector2)player.position).normalized), EnemyMotionState.Chase);
                    else Move(ChooseSteering(((Vector2)player.position - (Vector2)transform.position).normalized), EnemyMotionState.Chase);
                    return;
                }
            }
            Patrol();
        }
        private bool playerHealthIsDead() => playerHealth != null && playerHealth.IsDead;
        private void FixedUpdate() { if (state == EnemyMotionState.Dead) body.linearVelocity = Vector2.zero; }
        private void Patrol()
        {
            if (patrolRadius <= 0.05f) { body.linearVelocity = Vector2.zero; SetState(EnemyMotionState.Idle); return; }
            if (Vector2.Distance(transform.position, patrolGoal) < 0.2f || !CanMove((patrolGoal - (Vector2)transform.position).normalized))
            {
                Vector2 candidate = home + Random.insideUnitCircle * patrolRadius;
                if (IsWalkable(candidate)) patrolGoal = candidate;
                else { body.linearVelocity = Vector2.zero; SetState(EnemyMotionState.Idle); return; }
            }
            Move(ChooseSteering((patrolGoal - (Vector2)transform.position).normalized), EnemyMotionState.Patrol);
        }
        private void BeginAttack()
        {
            body.linearVelocity = Vector2.zero; actionTime = 0f; attackApplied = false; attackReadyAt = Time.time + attackCooldown;
            SetState(EnemyMotionState.Attack);
        }
        private void BeginSpecial()
        {
            body.linearVelocity = Vector2.zero;
            actionTime = 0f;
            specialApplied = false;
            specialReadyAt = Time.time + (currentHealth <= maxHealth * 0.3f ? 7f : specialCooldown);
            SetState(EnemyMotionState.Special);
            if (telegraphEffectPrefab != null) Instantiate(telegraphEffectPrefab, transform.position, Quaternion.identity);
        }
        private void UpdateSpecial()
        {
            actionTime += Time.deltaTime;
            if (player != null)
            {
                facing = ((Vector2)player.position - (Vector2)transform.position).normalized;
                animationView.SetFacing(facing);
            }
            if (!specialApplied && actionTime >= specialTelegraphDuration)
            {
                specialApplied = true;
                if (blastEffectPrefab != null) Instantiate(blastEffectPrefab, transform.position, Quaternion.identity);
                bool enraged = currentHealth <= maxHealth * 0.3f;
                if (player != null && Vector2.Distance(transform.position, player.position) <= specialRadius && playerHealth != null)
                    playerHealth.TakeDamage(enraged ? 36 : 30, ((Vector2)player.position - (Vector2)transform.position).normalized, 3f);
                if (specialProjectilePrefab != null)
                {
                    int count = enraged ? 12 : 8;
                    for (int i = 0; i < count; i++)
                    {
                        float angle = i < 8 ? i * 45f : (i - 8) * 90f + 22.5f;
                        Vector2 direction = Rotate(Vector2.right, angle);
                        EnemyProjectile projectile = Instantiate(specialProjectilePrefab, transform.position + (Vector3)(direction * 0.6f), Quaternion.identity);
                        projectile.Initialize(direction, 4.5f, 12);
                    }
                }
            }
            if (actionTime >= specialTelegraphDuration + 0.9f) SetState(EnemyMotionState.Chase);
        }
        private void UpdateAttack()
        {
            actionTime += Time.deltaTime;
            if (player != null)
            {
                facing = ((Vector2)player.position - (Vector2)transform.position).normalized;
                animationView.SetFacing(facing);
            }
            float duration = animationView.FrameCount > 1 ? (animationView.FrameCount - 1f) / 12f : 0.5f;
            if (!attackApplied && actionTime >= Mathf.Max(0.04f, duration * attackHitTime))
            {
                attackApplied = true;
                if (player != null && Vector2.Distance(transform.position, player.position) <= attackRadius * 1.2f && HasLineOfSight(player.position))
                {
                    if (attackEffectPrefab != null) Instantiate(attackEffectPrefab, transform.position + (Vector3)(facing * 0.65f), Quaternion.Euler(0f, 0f, Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg));
                    if (attackType == EnemyAttackType.Projectile && projectilePrefab != null)
                    {
                        EnemyProjectile projectile = Instantiate(projectilePrefab, transform.position + (Vector3)(facing * 0.5f), Quaternion.identity);
                        projectile.Initialize(facing, projectileSpeed, damage);
                    }
                    else if (playerHealth != null) playerHealth.TakeDamage(damage, facing, 2f);
                }
            }
            if (actionTime >= duration + 0.05f) SetState(EnemyMotionState.Chase);
        }
        private void Move(Vector2 direction, EnemyMotionState motion)
        {
            if (direction.sqrMagnitude < 0.001f) { body.linearVelocity = Vector2.zero; SetState(EnemyMotionState.Idle); return; }
            facing = direction.normalized; body.linearVelocity = facing * moveSpeed; animationView.SetFacing(facing); SetState(motion);
        }
        private Vector2 ChooseSteering(Vector2 desired)
        {
            if (desired.sqrMagnitude < 0.001f) return Vector2.zero;
            float[] angles = { 0f, 35f, -35f, 70f, -70f, 110f, -110f, 180f };
            for (int i = 0; i < angles.Length; i++)
            {
                Vector2 candidate = Rotate(desired, angles[i]);
                if (CanMove(candidate)) return candidate;
            }
            return Vector2.zero;
        }
        private bool CanMove(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.001f) return false;
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.22f, direction, 0.48f, Physics2D.DefaultRaycastLayers);
            return hit.collider == null || hit.collider == hitCollider || hit.collider.CompareTag("Player");
        }
        private bool IsWalkable(Vector2 point)
        {
            Collider2D hit = Physics2D.OverlapCircle(point, 0.22f, Physics2D.DefaultRaycastLayers);
            return hit == null || hit == hitCollider || hit.CompareTag("Player");
        }
        private bool HasLineOfSight(Vector2 point)
        {
            Vector2 delta = point - (Vector2)transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, delta.normalized, delta.magnitude, Physics2D.DefaultRaycastLayers);
            return hit.collider == null || hit.collider.CompareTag("Player") || hit.collider == hitCollider;
        }
        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float r = degrees * Mathf.Deg2Rad; float c = Mathf.Cos(r), s = Mathf.Sin(r);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c).normalized;
        }
        private void SetState(EnemyMotionState next)
        {
            if (state == next) return;
            state = next;
            animationView.Play(next, facing, next == EnemyMotionState.Idle || next == EnemyMotionState.Patrol || next == EnemyMotionState.Chase);
        }
        public void TakeDamage(int amount, Vector2 hitDirection, float knockback = 0f)
        {
            if (state == EnemyMotionState.Dead || amount <= 0) return;
            currentHealth = Mathf.Max(0, currentHealth - amount);
            if (currentHealth == 0) { Die(); return; }
            if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            if (hitDirection.sqrMagnitude > 0.001f) facing = hitDirection.normalized;
            if (body != null) body.AddForce(facing * (knockback > 0f ? knockback : knockbackForce), ForceMode2D.Impulse);
            hurtUntil = Time.time + 0.22f; SetState(EnemyMotionState.Hurt);
        }
        private void Die()
        {
            if (deathEffectPrefab != null) Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            SetState(EnemyMotionState.Dead);
            body.linearVelocity = Vector2.zero; body.simulated = false;
            if (hitCollider != null) hitCollider.enabled = false;
            if (lootPrefab != null) Instantiate(lootPrefab, transform.position, Quaternion.identity);
            if (enemyId == "boss_death_lord")
                DungeonOverlayUI.NotifyBossDefeated(lootPrefab != null ? lootPrefab.name : null);
            CancelInvoke(nameof(ReturnToPool));
            Invoke(nameof(ReturnToPool), Mathf.Max(0.45f, (animationView.FrameCount / 8f) + 0.15f));
        }
        private void ReturnToPool()
        {
            if (ownerPool != null) ownerPool.Return(this);
            else gameObject.SetActive(false);
        }
    }
}

