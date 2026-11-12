using UnityEngine;

namespace DungeonMerchant.Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class AnimatedSpriteEffect : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField, Min(1f)] private float framesPerSecond = 12f;
        [SerializeField] private bool loop;
        [SerializeField, Min(0.05f)] private float lifetime = 1f;
        private SpriteRenderer spriteRenderer;
        private float elapsed;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (frames != null && frames.Length > 0) spriteRenderer.sprite = frames[0];
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            if (frames != null && frames.Length > 0)
            {
                int frame = Mathf.FloorToInt(elapsed * framesPerSecond);
                spriteRenderer.sprite = frames[loop ? frame % frames.Length : Mathf.Min(frame, frames.Length - 1)];
            }
            if (elapsed >= lifetime) Destroy(gameObject);
        }
    }
}
