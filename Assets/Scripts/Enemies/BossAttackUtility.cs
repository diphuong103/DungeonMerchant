using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static class BossAttackUtility
    {
        public static void DamagePlayersInRadius(Vector2 center, float radius, int damage, LayerMask layers)
        {
            if (damage <= 0 || radius <= 0f) return;

            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, layers);
            HashSet<Transform> damagedPlayers = new HashSet<Transform>();
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];
                if (hit == null || !TryGetPlayerRoot(hit.transform, out Transform playerRoot)) continue;
                if (!damagedPlayers.Add(playerRoot)) continue;

                IDamageable damageable = DamageUtility.FindDamageable(hit);
                if (damageable != null)
                    damageable.TakeDamage(damage);
                else
                    playerRoot.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
        }

        public static bool TryGetPlayerRoot(Transform source, out Transform playerRoot)
        {
            for (Transform current = source; current != null; current = current.parent)
            {
                if (!current.CompareTag("Player")) continue;
                playerRoot = current;
                return true;
            }

            playerRoot = null;
            return false;
        }
    }
}
