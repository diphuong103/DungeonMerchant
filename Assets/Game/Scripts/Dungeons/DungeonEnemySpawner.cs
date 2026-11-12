using System.Collections;
using UnityEngine;

namespace DungeonMerchant.Dungeons
{
    public sealed class DungeonEnemySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private int[] waveSizes = { 3, 4 };
        [SerializeField, Min(0f)] private float spawnInterval = 0.6f;
        [SerializeField] private DungeonGate gate;

        private int waveIndex;
        private int aliveCount;
        private bool started;
        public bool WavesCleared { get; private set; }

        private void Start()
        {
            if (gate != null) gate.SetLocked(true);
        }

        public void BeginWaves()
        {
            if (started || WavesCleared) return;
            started = true;
            StartCoroutine(SpawnWaveRoutine());
        }

        private IEnumerator SpawnWaveRoutine()
        {
            while (waveIndex < waveSizes.Length)
            {
                int count = Mathf.Max(1, waveSizes[waveIndex]);
                for (int i = 0; i < count; i++)
                {
                    SpawnEnemy(i);
                    if (spawnInterval > 0f) yield return new WaitForSeconds(spawnInterval);
                }
                while (aliveCount > 0) yield return null;
                waveIndex++;
                if (waveIndex < waveSizes.Length) yield return new WaitForSeconds(0.8f);
            }

            WavesCleared = true;
            if (gate != null) gate.SetLocked(false);
        }

        private void SpawnEnemy(int index)
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0 || spawnPoints == null || spawnPoints.Length == 0)
                return;
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Transform point = spawnPoints[index % spawnPoints.Length];
            GameObject spawned = Instantiate(prefab, point.position, Quaternion.identity);
            DungeonEnemyBase enemy = spawned.GetComponent<DungeonEnemyBase>();
            if (enemy == null) return;
            aliveCount++;
            enemy.Died += OnEnemyDied;
        }

        private void OnEnemyDied(DungeonEnemyBase enemy)
        {
            enemy.Died -= OnEnemyDied;
            aliveCount = Mathf.Max(0, aliveCount - 1);
        }

        private void OnDisable() => StopAllCoroutines();
    }
}
