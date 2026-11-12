using UnityEngine;

namespace DungeonMerchant.Dungeons
{
    /// <summary>Press J or Space to make a short pixel-RPG melee strike in the last move direction.</summary>
    public sealed class DungeonPlayerCombat : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float attackRange = 1.15f;
        [SerializeField, Min(0.1f)] private float attackRadius = 0.8f;
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField, Min(0.05f)] private float attackCooldown = 0.35f;
        private Vector2 facing = Vector2.down;
        private float nextAttackTime;

        private void Update()
        {
            Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (movement.sqrMagnitude > 0.01f)
                facing = Mathf.Abs(movement.x) > Mathf.Abs(movement.y)
                    ? new Vector2(Mathf.Sign(movement.x), 0f)
                    : new Vector2(0f, Mathf.Sign(movement.y));

            if ((Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Space)) && Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                Vector2 center = (Vector2)transform.position + facing * attackRange;
                Collider2D[] hits = Physics2D.OverlapCircleAll(center, attackRadius);
                foreach (Collider2D hit in hits)
                {
                    DungeonEnemyBase enemy = hit.GetComponentInParent<DungeonEnemyBase>();
                    if (enemy != null) enemy.TakeDamage(damage);
                }
            }
        }
    }
}
