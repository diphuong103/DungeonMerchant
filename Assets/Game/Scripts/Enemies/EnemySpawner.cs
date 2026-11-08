using System.Collections.Generic;
using UnityEngine;

namespace DungeonMerchant.Enemies
{
    [System.Serializable]
    public sealed class EnemySpawnEntry
    {
        public EnemyBrain prefab;
        [Min(0)] public int count = 1;
        public Vector2[] positions;
    }

    [DisallowMultipleComponent]
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemySpawnEntry[] entries;
        [SerializeField] private bool spawnOnStart = true;
        private readonly Dictionary<EnemyBrain, Queue<EnemyBrain>> pools = new Dictionary<EnemyBrain, Queue<EnemyBrain>>();
        private readonly Dictionary<EnemyBrain, EnemyBrain> instancePrefab = new Dictionary<EnemyBrain, EnemyBrain>();
        private void Start() { if (spawnOnStart) SpawnAll(); }
        public void SpawnAll()
        {
            if (entries == null) return;
            foreach (var entry in entries)
            {
                if (entry == null || entry.prefab == null) continue;
                for (int i = 0; i < entry.count; i++)
                {
                    Vector2 position = entry.positions != null && entry.positions.Length > 0 ? entry.positions[i % entry.positions.Length] : transform.position;
                    EnemyBrain enemy = GetOrCreate(entry.prefab, position);
                    enemy.transform.position = position;
                    enemy.gameObject.SetActive(true);
                    enemy.Initialize(this);
                }
            }
        }
        private EnemyBrain GetOrCreate(EnemyBrain prefab, Vector2 position)
        {
            if (!pools.TryGetValue(prefab, out Queue<EnemyBrain> pool)) pools[prefab] = pool = new Queue<EnemyBrain>();
            EnemyBrain enemy = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, position, Quaternion.identity, transform);
            instancePrefab[enemy] = prefab;
            return enemy;
        }
        public void Return(EnemyBrain enemy)
        {
            if (enemy == null || !instancePrefab.TryGetValue(enemy, out EnemyBrain prefab)) { if (enemy != null) enemy.gameObject.SetActive(false); return; }
            enemy.gameObject.SetActive(false);
            enemy.transform.SetParent(transform, false);
            pools[prefab].Enqueue(enemy);
        }
    }
}
