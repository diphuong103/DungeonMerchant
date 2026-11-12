using UnityEngine;

namespace Game
{
    public sealed class TombstoneSpawner : MonoBehaviour
    {
        [Header("Spawn")]
        public GameObject skeletonPrefab;
        public Transform playerTarget;
        [Min(1f)] public float spawnInterval = 18f;
        [Min(1)] public int maxSkeletonsSpawned = 2;
        [Min(0f)] public float crackOpenDuration = 0.75f;
        public Transform spawnPoint;

        [Header("Visual")]
        public SpriteRenderer tombstoneRenderer;
        public Color closedColor = new Color(0.68f, 0.72f, 0.78f, 1f);
        public Color crackedColor = new Color(0.72f, 0.42f, 0.95f, 1f);

        private float nextSpawnTime;
        private float restoreColorAt;
        private int spawnedCount;

        private void Start()
        {
            if (tombstoneRenderer == null) tombstoneRenderer = GetComponentInChildren<SpriteRenderer>();
            if (tombstoneRenderer != null) tombstoneRenderer.color = closedColor;
            nextSpawnTime = Time.time + Random.Range(spawnInterval * 0.5f, spawnInterval * 1.5f);
        }

        private void Update()
        {
            if (tombstoneRenderer != null && restoreColorAt > 0f && Time.time >= restoreColorAt)
            {
                tombstoneRenderer.color = closedColor;
                restoreColorAt = 0f;
            }

            if (skeletonPrefab == null || spawnedCount >= maxSkeletonsSpawned || Time.time < nextSpawnTime)
                return;

            nextSpawnTime = Time.time + spawnInterval;
            spawnedCount++;
            if (tombstoneRenderer != null)
            {
                tombstoneRenderer.color = crackedColor;
                restoreColorAt = Time.time + crackOpenDuration;
            }

            Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
            GameObject skeleton = Instantiate(skeletonPrefab, position, Quaternion.identity);
            EnemyBase enemy = skeleton.GetComponent<EnemyBase>();
            if (enemy != null && playerTarget != null) enemy.SetTarget(playerTarget);
            skeleton.tag = "Enemy";
        }
    }
}
