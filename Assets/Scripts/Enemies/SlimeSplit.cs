using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(EnemyBase))]
    public sealed class SlimeSplit : MonoBehaviour
    {
        public GameObject SmallerSlimePrefab;
        [Min(0)] public int SplitCount = 2;
        [Min(0f)] public float SplitRadius = 0.65f;

        private EnemyBase enemy;
        private bool hasSplit;

        private void Awake()
        {
            enemy = GetComponent<EnemyBase>();
            enemy.Die += HandleDeath;
        }

        private void OnDestroy()
        {
            if (enemy != null) enemy.Die -= HandleDeath;
        }

        private void HandleDeath(EnemyBase deadEnemy)
        {
            if (hasSplit || SmallerSlimePrefab == null) return;
            hasSplit = true;

            for (int i = 0; i < SplitCount; i++)
            {
                float angle = 360f * i / Mathf.Max(1, SplitCount);
                Vector2 offset = Quaternion.Euler(0f, 0f, angle) * Vector2.right * SplitRadius;
                GameObject child = Instantiate(SmallerSlimePrefab, (Vector2)transform.position + offset, Quaternion.identity);
                EnemyBase childEnemy = child.GetComponent<EnemyBase>();
                if (childEnemy != null && deadEnemy.Target != null) childEnemy.SetTarget(deadEnemy.Target);
                if (childEnemy != null) child.tag = "Enemy";
            }
        }
    }
}
