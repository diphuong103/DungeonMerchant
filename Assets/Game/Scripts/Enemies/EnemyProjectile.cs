using UnityEngine;

namespace DungeonMerchant.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemySpriteAnimator))]
    public sealed class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private AnimatedSpriteEffect hitEffectPrefab;
        [SerializeField, Min(0.1f)] private float framesPerSecond = 10f;
        [SerializeField, Min(0.1f)] private float lifetime = 5f;
        private Vector2 direction;
        private float speed;
        private int damage;
        private float age;
        private SpriteRenderer spriteRenderer;
        private Collider2D hitCollider;
        public void Initialize(Vector2 heading, float moveSpeed, int hitDamage)
        {
            direction = heading.normalized; speed = moveSpeed; damage = hitDamage;
            age = 0f;
            if (frames != null && frames.Length > 0) spriteRenderer.sprite = frames[0];
            gameObject.SetActive(true);
        }
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            hitCollider = GetComponent<Collider2D>();
            GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            hitCollider.isTrigger = true;
        }
        private void Update()
        {
            age += Time.deltaTime;
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            if (frames != null && frames.Length > 0) spriteRenderer.sprite = frames[Mathf.FloorToInt(age * framesPerSecond) % frames.Length];
            if (age >= lifetime) Destroy(gameObject);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                var health = other.GetComponentInParent<DungeonMerchant.Player.PlayerHealth>();
                if (health != null) health.TakeDamage(damage, direction, 1.5f);
                if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
            else if (!other.isTrigger && other.GetComponentInParent<EnemyBrain>() == null)
            {
                if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}
