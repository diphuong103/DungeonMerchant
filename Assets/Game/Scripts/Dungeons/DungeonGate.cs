using UnityEngine;

namespace DungeonMerchant.Dungeons
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class DungeonGate : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer gateVisual;
        [SerializeField] private Color lockedColor = new Color(0.72f, 0.28f, 0.25f);
        [SerializeField] private Color openColor = new Color(0.35f, 0.75f, 0.48f);
        private BoxCollider2D gateCollider;
        public bool IsLocked { get; private set; }

        private void Awake() => gateCollider = GetComponent<BoxCollider2D>();

        public void SetLocked(bool locked)
        {
            if (gateCollider == null) gateCollider = GetComponent<BoxCollider2D>();
            IsLocked = locked;
            if (gateCollider != null) gateCollider.enabled = locked;
            if (gateVisual != null) gateVisual.color = locked ? lockedColor : openColor;
        }
    }
}
