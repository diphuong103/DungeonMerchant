using UnityEngine;
using UnityEngine.Events;
using DungeonMerchant.Combat;

namespace DungeonMerchant.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int maxHealth = 100;
        [SerializeField] private float invulnerabilitySeconds = 0.45f;
        [SerializeField] private UnityEvent<int, int> onHealthChanged;
        private int currentHealth;
        private float invulnerableUntil;
        private Rigidbody2D body;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => currentHealth <= 0;

        private void Awake()
        {
            currentHealth = maxHealth;
            body = GetComponent<Rigidbody2D>();
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(int amount, Vector2 hitDirection, float knockback = 0f)
        {
            if (IsDead || Time.time < invulnerableUntil || amount <= 0) return;
            currentHealth = Mathf.Max(0, currentHealth - amount);
            invulnerableUntil = Time.time + invulnerabilitySeconds;
            if (body != null && knockback > 0f)
                body.AddForce(hitDirection.normalized * knockback, ForceMode2D.Impulse);
            onHealthChanged?.Invoke(currentHealth, maxHealth);
            if (IsDead) gameObject.SendMessage("OnPlayerDeath", SendMessageOptions.DontRequireReceiver);
        }

        public void RestoreHealth(int amount)
        {
            if (amount <= 0 || IsDead) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
