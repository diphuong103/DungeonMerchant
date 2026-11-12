using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public sealed class DungeonController : MonoBehaviour
    {
        public EnemySpawner enemySpawner;
        [Tooltip("Optional sequential spawners. When assigned, each starts after the previous spawner clears.")]
        public List<EnemySpawner> waveSpawners = new List<EnemySpawner>();
        public Gate gate;
        public BossRoomTrigger bossRoomTrigger;
        public GameObject bossPrefab;
        public Transform bossSpawnPoint;
        public Transform playerTarget;
        public GameObject chestPrefab;
        public Transform chestSpawnPoint;
        public GameObject exitPortalPrefab;
        public Transform exitPortalSpawnPoint;

        public bool WavesCleared { get; private set; }
        public bool BossDefeated { get; private set; }

        private bool bossStarted;
        private int nextWaveSpawnerIndex;
        private EnemySpawner activeWaveSpawner;

        private void Awake()
        {
            if (bossRoomTrigger != null) bossRoomTrigger.controller = this;
        }

        private void Start()
        {
            if (gate != null) gate.SetLocked(true);
            if (waveSpawners != null && waveSpawners.Count > 0)
                BeginNextWaveSpawner();
            else if (enemySpawner != null)
            {
                activeWaveSpawner = enemySpawner;
                activeWaveSpawner.OnAllWavesCleared += HandleActiveSpawnerCleared;
                activeWaveSpawner.BeginWaves();
            }
            else HandleAllWavesCleared();
        }

        private void OnDestroy()
        {
            if (activeWaveSpawner != null)
                activeWaveSpawner.OnAllWavesCleared -= HandleActiveSpawnerCleared;
        }

        public void TryStartBoss(Transform player)
        {
            if (bossStarted || !WavesCleared || bossPrefab == null || bossSpawnPoint == null) return;
            bossStarted = true;
            GameObject instance = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
            BossBase boss = instance.GetComponent<BossBase>();
            if (boss == null)
            {
                Debug.LogError("DungeonController bossPrefab must have a BossBase component.", bossPrefab);
                Destroy(instance);
                bossStarted = false;
                return;
            }

            Transform target = playerTarget != null ? playerTarget : player;
            if (target != null) boss.SetTarget(target);
            instance.tag = "Boss";
            boss.Die += HandleBossDeath;
        }

        private void BeginNextWaveSpawner()
        {
            if (activeWaveSpawner != null)
                activeWaveSpawner.OnAllWavesCleared -= HandleActiveSpawnerCleared;

            while (nextWaveSpawnerIndex < waveSpawners.Count && waveSpawners[nextWaveSpawnerIndex] == null)
                nextWaveSpawnerIndex++;

            if (nextWaveSpawnerIndex >= waveSpawners.Count)
            {
                activeWaveSpawner = null;
                HandleAllWavesCleared();
                return;
            }

            activeWaveSpawner = waveSpawners[nextWaveSpawnerIndex++];
            activeWaveSpawner.OnAllWavesCleared += HandleActiveSpawnerCleared;
            activeWaveSpawner.BeginWaves();
        }

        private void HandleActiveSpawnerCleared()
        {
            if (waveSpawners != null && waveSpawners.Count > 0)
                BeginNextWaveSpawner();
            else
                HandleAllWavesCleared();
        }

        private void HandleAllWavesCleared()
        {
            WavesCleared = true;
            if (gate != null) gate.SetLocked(false);
        }

        private void HandleBossDeath(EnemyBase boss)
        {
            boss.Die -= HandleBossDeath;
            BossDefeated = true;
            SpawnReward(chestPrefab, chestSpawnPoint);
            GameObject exit = SpawnReward(exitPortalPrefab, exitPortalSpawnPoint);
            if (exit != null)
            {
                ExitPortal portal = exit.GetComponent<ExitPortal>();
                if (portal != null) portal.sceneToLoad = "Home";
            }
        }

        private static GameObject SpawnReward(GameObject prefab, Transform spawnPoint)
        {
            if (prefab == null || spawnPoint == null) return null;
            return Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
