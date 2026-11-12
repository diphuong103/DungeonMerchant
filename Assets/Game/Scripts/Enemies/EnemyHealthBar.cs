using UnityEngine;

namespace DungeonMerchant.Enemies
{
    /// <summary>World-space health bar that follows an EnemyBrain through its prefab hierarchy.</summary>
    [DisallowMultipleComponent]
    public sealed class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer fill;
        [SerializeField, Min(0)] private int sortingOrder = 20;

        private EnemyBrain enemy;
        private Vector3 fullFillScale;
        private float fullFillWidth;

        private void Awake()
        {
            enemy = GetComponentInParent<EnemyBrain>();
            if (fill == null)
                fill = transform.Find("Fill") != null ? transform.Find("Fill").GetComponent<SpriteRenderer>() : null;

            if (fill != null)
            {
                fullFillScale = fill.transform.localScale;
                fullFillWidth = fill.sprite != null ? fill.sprite.bounds.size.x * fullFillScale.x : 0f;
                fill.sortingOrder = sortingOrder + 1;
            }

            var frame = transform.Find("Frame");
            if (frame != null)
            {
                var frameRenderer = frame.GetComponent<SpriteRenderer>();
                if (frameRenderer != null) frameRenderer.sortingOrder = sortingOrder;
            }
        }

        private void LateUpdate()
        {
            if (enemy == null || fill == null || fullFillWidth <= 0f) return;

            float ratio = enemy.MaxHealth > 0
                ? Mathf.Clamp01((float)enemy.CurrentHealth / enemy.MaxHealth)
                : 0f;

            Vector3 scale = fullFillScale;
            scale.x *= ratio;
            fill.transform.localScale = scale;

            Vector3 position = fill.transform.localPosition;
            position.x = -fullFillWidth * (1f - ratio) * 0.5f;
            fill.transform.localPosition = position;
            fill.enabled = ratio > 0f;
        }
    }
}
