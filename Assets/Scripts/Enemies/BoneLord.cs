using System.Collections;
using UnityEngine;

namespace Game
{
    public sealed class BoneLord : BossBase
    {
        [Header("Summoning at half health")]
        public GameObject SkeletonPrefab;
        [Min(0)] public int SkeletonsToSummon = 3;
        public Transform[] SummonPoints;
        [Min(0.1f)] public float SummonSpreadRadius = 1.5f;

        [Header("Purple projectile")]
        public GameObject PurpleProjectilePrefab;
        [Min(0f)] public float ProjectileRange = 7f;
        [Min(0.1f)] public float ProjectileCooldown = 1.8f;
        [Min(1)] public int ProjectileDamage = 1;
        [Min(0.1f)] public float ProjectileSpeed = 5f;
        [Min(0.1f)] public float ProjectileLifetime = 5f;
        public Transform ProjectileOrigin;

        [Header("Sarcophagus rise")]
        [Min(0f)] public float RiseOffset = 1.2f;
        [Min(0f)] public float RiseDuration = 0.9f;

        private bool hasSummoned;
        private bool isRising;
        private float nextProjectileTime;

        private void Start()
        {
            if (RiseOffset <= 0f || RiseDuration <= 0f) return;
            Vector3 finalPosition = transform.position;
            Vector3 startPosition = finalPosition - Vector3.up * RiseOffset;
            transform.position = startPosition;
            isRising = true;
            StartCoroutine(RiseFromSarcophagus(startPosition, finalPosition));
        }

        protected override void FixedUpdate()
        {
            if (isRising)
            {
                if (Body != null) Body.linearVelocity = Vector2.zero;
                return;
            }
            base.FixedUpdate();
        }

        public override void TakeDamage(int amount)
        {
            float oldHealthPercent = HealthPercent;
            base.TakeDamage(amount);
            if (!hasSummoned && HP > 0 && oldHealthPercent > 0.5f && HealthPercent <= 0.5f)
                SummonSkeletons();
        }

        private void Update()
        {
            if (Target == null || PurpleProjectilePrefab == null || Time.time < nextProjectileTime) return;
            if (Vector2.Distance(transform.position, Target.position) > ProjectileRange) return;

            FirePurpleProjectile();
            nextProjectileTime = Time.time + ProjectileCooldown;
        }

        private void SummonSkeletons()
        {
            hasSummoned = true;
            if (SkeletonPrefab == null) return;

            for (int i = 0; i < SkeletonsToSummon; i++)
            {
                Vector2 position = GetSummonPosition(i);
                GameObject skeleton = Instantiate(SkeletonPrefab, position, Quaternion.identity);
                EnemyBase enemy = skeleton.GetComponent<EnemyBase>();
                if (enemy != null && Target != null) enemy.SetTarget(Target);
                if (enemy != null) skeleton.tag = "Enemy";
            }
        }

        private Vector2 GetSummonPosition(int index)
        {
            if (SummonPoints != null && index < SummonPoints.Length && SummonPoints[index] != null)
                return SummonPoints[index].position;

            float angle = 360f * index / Mathf.Max(1, SkeletonsToSummon);
            Vector2 offset = (Vector2)(Quaternion.Euler(0f, 0f, angle) * Vector2.right * SummonSpreadRadius);
            return (Vector2)transform.position + offset;
        }

        private void FirePurpleProjectile()
        {
            Vector3 origin = ProjectileOrigin != null ? ProjectileOrigin.position : transform.position;
            Vector2 direction = (Vector2)(Target.position - origin);
            GameObject projectile = Instantiate(PurpleProjectilePrefab, origin, Quaternion.identity);
            BossProjectile behavior = projectile.GetComponent<BossProjectile>();
            if (behavior == null) behavior = projectile.AddComponent<BossProjectile>();
            behavior.Launch(direction, gameObject, ProjectileDamage, ProjectileSpeed, ProjectileLifetime);
        }

        private IEnumerator RiseFromSarcophagus(Vector3 startPosition, Vector3 finalPosition)
        {
            float elapsed = 0f;
            while (elapsed < RiseDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / RiseDuration);
                t = t * t * (3f - 2f * t);
                transform.position = Vector3.LerpUnclamped(startPosition, finalPosition, t);
                yield return null;
            }
            transform.position = finalPosition;
            isRising = false;
        }
    }
}
