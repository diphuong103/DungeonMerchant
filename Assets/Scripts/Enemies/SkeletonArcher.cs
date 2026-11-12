using UnityEngine;

namespace Game
{
    public sealed class SkeletonArcher : EnemyBase
    {
        [Header("Ranged attack")]
        public GameObject ProjectilePrefab;
        public Transform ProjectileOrigin;
        [Min(1f)] public float AttackRange = 6f;
        [Min(0.1f)] public float FireCooldown = 1.8f;
        [Min(1)] public int ProjectileDamage = 2;
        [Min(0.1f)] public float ProjectileSpeed = 7f;
        [Min(0.1f)] public float ProjectileLifetime = 4f;

        private float nextShotTime;

        protected override void FixedUpdate()
        {
            if (Target == null)
            {
                StopMoving();
                return;
            }

            Vector2 direction = (Vector2)(Target.position - transform.position);
            float distance = direction.magnitude;
            if (distance > AttackRange)
            {
                Body.linearVelocity = direction.normalized * MoveSpeed;
                return;
            }

            StopMoving();
            if (distance <= MeleeRange)
            {
                base.FixedUpdate();
                return;
            }

            if (Time.time >= nextShotTime && ProjectilePrefab != null)
            {
                Fire(direction);
                nextShotTime = Time.time + FireCooldown;
            }
        }

        private void Fire(Vector2 direction)
        {
            Vector3 origin = ProjectileOrigin != null ? ProjectileOrigin.position : transform.position;
            GameObject projectile = Instantiate(ProjectilePrefab, origin, Quaternion.identity);
            BossProjectile behavior = projectile.GetComponent<BossProjectile>();
            if (behavior == null) behavior = projectile.AddComponent<BossProjectile>();
            behavior.Launch(direction, gameObject, ProjectileDamage, ProjectileSpeed, ProjectileLifetime);
        }

        private void StopMoving()
        {
            if (Body != null) Body.linearVelocity = Vector2.zero;
        }
    }
}
