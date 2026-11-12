using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [Serializable]
        public sealed class Wave
        {
            public GameObject prefab;
            [Min(0)] public int count = 3;
            [Min(0f)] public float delay = 0.5f;
            [Tooltip("Optional second enemy type for mixed waves.")]
            public GameObject additionalPrefab;
            [Min(0)] public int additionalCount;
        }

        public List<Wave> waves = new List<Wave>();
        public List<Transform> spawnPoints = new List<Transform>();
        public Transform playerTarget;

        public event Action<int> OnWaveCleared;
        public event Action OnAllWavesCleared;
        public bool AllWavesCleared { get; private set; }

        private int activeEnemyCount;
        private bool hasStarted;

        public void BeginWaves()
        {
            if (hasStarted || AllWavesCleared) return;
            hasStarted = true;
            StartCoroutine(SpawnWaves());
        }

        private IEnumerator SpawnWaves()
        {
            for (int waveIndex = 0; waveIndex < waves.Count; waveIndex++)
            {
                Wave wave = waves[waveIndex];
                if (wave == null || (wave.count <= 0 && wave.additionalCount <= 0) ||
                    (wave.prefab == null && wave.additionalPrefab == null))
                {
                    Debug.LogWarning($"EnemySpawner on '{name}' has an empty wave at index {waveIndex}.", this);
                    OnWaveCleared?.Invoke(waveIndex + 1);
                    continue;
                }

                for (int i = 0; i < wave.count; i++)
                {
                    SpawnEnemy(wave.prefab, i);
                    if (wave.delay > 0f) yield return new WaitForSeconds(wave.delay);
                }
                for (int i = 0; i < wave.additionalCount; i++)
                {
                    SpawnEnemy(wave.additionalPrefab, wave.count + i);
                    if (wave.delay > 0f) yield return new WaitForSeconds(wave.delay);
                }
                while (activeEnemyCount > 0) yield return null;
                OnWaveCleared?.Invoke(waveIndex + 1);
            }

            AllWavesCleared = true;
            OnAllWavesCleared?.Invoke();
        }

        private void SpawnEnemy(GameObject prefab, int spawnIndex)
        {
            if (prefab == null) return;
            Vector3 position = transform.position;
            if (spawnPoints.Count > 0)
            {
                Transform point = spawnPoints[spawnIndex % spawnPoints.Count];
                if (point != null) position = point.position;
            }

            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            EnemyBase enemy = instance.GetComponent<EnemyBase>();
            if (enemy == null)
            {
                Debug.LogError($"Wave prefab '{prefab.name}' needs an EnemyBase component.", prefab);
                Destroy(instance);
                return;
            }

            if (playerTarget != null) enemy.SetTarget(playerTarget);
            instance.tag = enemy is BossBase ? "Boss" : "Enemy";
            activeEnemyCount++;
            enemy.Die += HandleEnemyDeath;
        }

        private void HandleEnemyDeath(EnemyBase enemy)
        {
            enemy.Die -= HandleEnemyDeath;
            activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);
        }

        private void OnDisable() => StopAllCoroutines();
    }
}
