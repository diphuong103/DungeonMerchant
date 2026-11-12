using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class BossProjectile : MonoBehaviour
    {
        [Min(1)] public int Damage = 1;
        [Min(0.1f)] public float Speed = 6f;
        [Min(0.1f)] public float Lifetime = 5f;
        public GameObject Owner;
        public bool DestroyOnNonPlayerCollision = true;

        private Rigidbody2D body;
        private float expiresAt;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            Collider2D hitbox = GetComponent<Collider2D>();
            hitbox.isTrigger = true;
            expiresAt = Time.time + Lifetime;
        }

        public void Launch(Vector2 direction, GameObject owner, int damage, float speed, float lifetime)
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            Owner = owner;
            Damage = Mathf.Max(1, damage);
            Speed = Mathf.Max(0.1f, speed);
            Lifetime = Mathf.Max(0.1f, lifetime);
            expiresAt = Time.time + Lifetime;
            body.linearVelocity = direction.sqrMagnitude > 0f ? direction.normalized * Speed : Vector2.zero;
        }

        private void Update()
        {
            if (Time.time >= expiresAt) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null || IsOwnerCollider(other.transform)) return;

            if (BossAttackUtility.TryGetPlayerRoot(other.transform, out Transform playerRoot))
            {
                IDamageable damageable = DamageUtility.FindDamageable(other);
                if (damageable != null)
                    damageable.TakeDamage(Damage);
                else
                    playerRoot.SendMessage("TakeDamage", Damage, SendMessageOptions.DontRequireReceiver);
                Destroy(gameObject);
                return;
            }

            if (DestroyOnNonPlayerCollision) Destroy(gameObject);
        }

        private bool IsOwnerCollider(Transform other)
        {
            return Owner != null && (other == Owner.transform || other.IsChildOf(Owner.transform));
        }
    }
}
