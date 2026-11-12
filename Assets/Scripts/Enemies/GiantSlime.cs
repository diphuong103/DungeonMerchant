using System.Collections;
using UnityEngine;

namespace Game
{
    public sealed class GiantSlime : BossBase
    {
        [Header("Splitting")]
        public GameObject SplitSlimePrefab;
        [Min(1)] public int SlimesPerSplit = 2;
        [Min(0f)] public float SplitSpreadRadius = 1.1f;

        [Header("Ground slam")]
        [Min(0.1f)] public float SlamRange = 2.5f;
        [Min(0.1f)] public float SlamRadius = 2f;
        [Min(1)] public int SlamDamage = 2;
        [Min(0.1f)] public float SlamCooldown = 4f;
        [Min(0f)] public float SlamWindup = 0.6f;
        public LayerMask SlamTargetLayers = ~0;
        public GameObject SlamEffectPrefab;

        private bool splitAt66;
        private bool splitAt33;
        private float nextSlamTime;

        public override void TakeDamage(int amount)
        {
            float oldHealthPercent = HealthPercent;
            base.TakeDamage(amount);
            if (HP <= 0) return;

            if (!splitAt66 && oldHealthPercent > 0.66f && HealthPercent <= 0.66f)
            {
                splitAt66 = true;
                SplitSlime();
            }

            if (!splitAt33 && oldHealthPercent > 0.33f && HealthPercent <= 0.33f)
            {
                splitAt33 = true;
                SplitSlime();
            }
        }

        private void Update()
        {
            if (Target == null || Time.time < nextSlamTime) return;
            if (Vector2.Distance(transform.position, Target.position) > SlamRange) return;

            nextSlamTime = Time.time + SlamCooldown;
            StartCoroutine(GroundSlam());
        }

        private void SplitSlime()
        {
            if (SplitSlimePrefab == null) return;

            for (int i = 0; i < SlimesPerSplit; i++)
            {
                float angle = 360f * i / Mathf.Max(1, SlimesPerSplit);
                Vector2 offset = Quaternion.Euler(0f, 0f, angle) * Vector2.right * SplitSpreadRadius;
                GameObject child = Instantiate(SplitSlimePrefab, (Vector2)transform.position + offset, Quaternion.identity);
                EnemyBase enemy = child.GetComponent<EnemyBase>();
                if (enemy != null && Target != null) enemy.SetTarget(Target);
                if (enemy != null) child.tag = "Enemy";
            }
        }

        private IEnumerator GroundSlam()
        {
            Vector2 impactPosition = Target != null ? (Vector2)Target.position : (Vector2)transform.position;
            if (SlamWindup > 0f) yield return new WaitForSeconds(SlamWindup);
            if (SlamEffectPrefab != null)
                Instantiate(SlamEffectPrefab, impactPosition, Quaternion.identity);
            BossAttackUtility.DamagePlayersInRadius(impactPosition, SlamRadius, SlamDamage, SlamTargetLayers);
        }
    }
}
