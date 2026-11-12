using UnityEngine;

namespace DungeonMerchant.Dungeons
{
    public sealed class DungeonHazard : MonoBehaviour
    {
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField, Min(0.1f)] private float damageInterval = 1f;
        private float nextDamageTime;

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag("Player") || Time.time < nextDamageTime) return;
            DungeonPlayerHealth health = other.GetComponent<DungeonPlayerHealth>();
            if (health == null) return;
            nextDamageTime = Time.time + damageInterval;
            health.TakeDamage(damage);
        }
    }
}
