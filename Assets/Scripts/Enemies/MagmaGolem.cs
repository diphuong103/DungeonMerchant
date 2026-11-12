using UnityEngine;

namespace Game
{
    public sealed class MagmaGolem : BossBase
    {
        [Header("Phase thresholds")]
        [Range(0.01f, 0.99f)] public float Phase2HPPercent = 0.66f;
        [Range(0.01f, 0.99f)] public float Phase3HPPercent = 0.33f;

        [Header("Fireball barrage")]
        public GameObject FireballPrefab;
        public Transform FireballOrigin;
        [Min(1)] public int FireballsPerBarrage = 5;
        [Range(0f, 180f)] public float BarrageSpreadAngle = 50f;
        [Min(1)] public int FireballDamage = 1;
        [Min(0.1f)] public float FireballSpeed = 5f;
        [Min(0.1f)] public float FireballLifetime = 5f;
        [Min(0.1f)] public float Phase1BarrageCooldown = 3f;
        [Min(0.1f)] public float Phase2BarrageCooldown = 2.2f;
        [Min(0.1f)] public float Phase3BarrageCooldown = 1.5f;
        [Min(0.1f)] public float BarrageRange = 8f;

        [Header("Lava geyser slam")]
        public GameObject LavaGeyserEffectPrefab;
        [Min(0.1f)] public float GeyserRange = 5f;
        [Min(0.1f)] public float GeyserRadius = 2.2f;
        [Min(1)] public int GeyserDamage = 2;
        [Min(0f)] public float GeyserWindup = 0.9f;
        [Min(0.1f)] public float Phase1GeyserCooldown = 7f;
        [Min(0.1f)] public float Phase2GeyserCooldown = 5.5f;
        [Min(0.1f)] public float Phase3GeyserCooldown = 4f;
        public LayerMask GeyserTargetLayers = ~0;

        public int CurrentPhase { get; private set; } = 1;

        private float nextBarrageTime;
        private float nextGeyserTime;

        protected override void Awake()
        {
            base.Awake();
            UpdatePhase();
        }

        protected override void OnHealthChanged()
        {
            base.OnHealthChanged();
            UpdatePhase();
        }

        private void Update()
        {
            if (Target == null) return;

            float distance = Vector2.Distance(transform.position, Target.position);
            if (FireballPrefab != null && distance <= BarrageRange && Time.time >= nextBarrageTime)
            {
                FireBarrage();
                nextBarrageTime = Time.time + GetBarrageCooldown();
            }

            if (distance <= GeyserRange && Time.time >= nextGeyserTime)
            {
                Vector2 impactPosition = Target.position;
                nextGeyserTime = Time.time + GetGeyserCooldown();
                SpawnGeyser(impactPosition);
            }
        }

        private void UpdatePhase()
        {
            int phase = HealthPercent <= Phase3HPPercent ? 3 : HealthPercent <= Phase2HPPercent ? 2 : 1;
            while (CurrentPhase < phase)
            {
                CurrentPhase++;
                MoveSpeed *= CurrentPhase == 2 ? 1.2f : 1.25f;
                AttackDamage++;
            }
        }

        private float GetBarrageCooldown()
        {
            if (CurrentPhase == 3) return Phase3BarrageCooldown;
            if (CurrentPhase == 2) return Phase2BarrageCooldown;
            return Phase1BarrageCooldown;
        }

        private float GetGeyserCooldown()
        {
            if (CurrentPhase == 3) return Phase3GeyserCooldown;
            if (CurrentPhase == 2) return Phase2GeyserCooldown;
            return Phase1GeyserCooldown;
        }

        private void FireBarrage()
        {
            Vector3 origin = FireballOrigin != null ? FireballOrigin.position : transform.position;
            Vector2 direction = (Vector2)(Target.position - origin).normalized;
            int count = Mathf.Max(1, FireballsPerBarrage + (CurrentPhase - 1) * 2);
            float startAngle = -BarrageSpreadAngle * 0.5f;
            float step = count > 1 ? BarrageSpreadAngle / (count - 1) : 0f;

            for (int i = 0; i < count; i++)
            {
                Vector2 spreadDirection = Quaternion.Euler(0f, 0f, startAngle + step * i) * direction;
                GameObject fireball = Instantiate(FireballPrefab, origin, Quaternion.identity);
                BossProjectile projectile = fireball.GetComponent<BossProjectile>();
                if (projectile == null) projectile = fireball.AddComponent<BossProjectile>();
                projectile.Launch(spreadDirection, gameObject, FireballDamage, FireballSpeed, FireballLifetime);
            }
        }

        private void SpawnGeyser(Vector2 impactPosition)
        {
            if (LavaGeyserEffectPrefab != null)
                Instantiate(LavaGeyserEffectPrefab, impactPosition, Quaternion.identity);
            StartCoroutine(GeyserSlam(impactPosition));
        }

        private System.Collections.IEnumerator GeyserSlam(Vector2 impactPosition)
        {
            if (GeyserWindup > 0f) yield return new WaitForSeconds(GeyserWindup);
            BossAttackUtility.DamagePlayersInRadius(impactPosition, GeyserRadius, GeyserDamage, GeyserTargetLayers);
        }
    }
}
