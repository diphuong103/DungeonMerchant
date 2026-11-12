using UnityEngine;

namespace DungeonMerchant.Dungeons
{
    /// <summary>Small dungeon-only health component; it does not alter the hub player controller.</summary>
    public sealed class DungeonPlayerHealth : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maxHealth = 8;
        public int CurrentHealth { get; private set; }

        private void Awake() => CurrentHealth = maxHealth;

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || CurrentHealth <= 0) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            if (CurrentHealth == 0)
            {
                // A lightweight defeat fallback that keeps the existing controller intact.
                CurrentHealth = maxHealth;
                transform.position = DungeonSpawnPoint.LastSpawnPosition;
            }
        }
    }
}
