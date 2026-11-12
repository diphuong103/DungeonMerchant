using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class HazardZone : MonoBehaviour
    {
        [Min(1)] public int damage = 1;
        [Min(0.05f)] public float damageInterval = 1f;
        public bool damageOverTime = true;

        private float nextDamageTime;

        private void Reset()
        {
            Collider2D trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!damageOverTime) ApplyDamage(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (damageOverTime && Time.time >= nextDamageTime)
            {
                ApplyDamage(other);
                nextDamageTime = Time.time + damageInterval;
            }
        }

        private void ApplyDamage(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            IDamageable damageable = DamageUtility.FindDamageable(other);
            if (damageable != null) damageable.TakeDamage(damage);
            else other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }
    }
}
