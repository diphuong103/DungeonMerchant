using UnityEngine;

namespace DungeonMerchant.Combat
{
    public interface IDamageable
    {
        void TakeDamage(int amount, Vector2 hitDirection, float knockback = 0f);
    }
}
