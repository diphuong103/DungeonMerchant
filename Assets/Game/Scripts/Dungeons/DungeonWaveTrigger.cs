using UnityEngine;

namespace DungeonMerchant.Dungeons
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class DungeonWaveTrigger : MonoBehaviour
    {
        [SerializeField] private DungeonEnemySpawner spawner;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && spawner != null) spawner.BeginWaves();
        }
    }
}
