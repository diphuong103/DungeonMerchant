using UnityEngine;

namespace DungeonMerchant.Dungeons
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class DungeonBossRoomTrigger : MonoBehaviour
    {
        [SerializeField] private DungeonEnemySpawner waveSpawner;
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private Transform bossSpawnPoint;
        [SerializeField] private GameObject rewardChest;
        [SerializeField] private GameObject exitPortal;
        [SerializeField] private DungeonGate bossGate;
        private bool activated;

        private void Start()
        {
            if (rewardChest != null) rewardChest.SetActive(false);
            if (exitPortal != null) exitPortal.SetActive(false);
            if (bossGate != null) bossGate.SetLocked(true);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (activated || !other.CompareTag("Player") || waveSpawner == null || !waveSpawner.WavesCleared)
                return;
            activated = true;
            if (bossPrefab == null || bossSpawnPoint == null)
            {
                UnlockRewards();
                return;
            }
            GameObject boss = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
            DungeonEnemyBase enemy = boss.GetComponent<DungeonEnemyBase>();
            if (enemy == null)
            {
                UnlockRewards();
                return;
            }
            enemy.Died += OnBossDied;
        }

        private void OnBossDied(DungeonEnemyBase boss)
        {
            boss.Died -= OnBossDied;
            UnlockRewards();
        }

        private void UnlockRewards()
        {
            if (bossGate != null) bossGate.SetLocked(false);
            if (rewardChest != null) rewardChest.SetActive(true);
            if (exitPortal != null) exitPortal.SetActive(true);
        }
    }
}
