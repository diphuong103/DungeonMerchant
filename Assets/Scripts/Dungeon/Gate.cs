using UnityEngine;

namespace Game
{
    public sealed class Gate : MonoBehaviour
    {
        public Collider2D blockingCollider;
        public GameObject closedVisual;
        public bool startsLocked = true;

        public bool IsLocked { get; private set; }

        private void Awake() => SetLocked(startsLocked);

        public void SetLocked(bool locked)
        {
            IsLocked = locked;
            if (blockingCollider != null) blockingCollider.enabled = locked;
            if (closedVisual != null) closedVisual.SetActive(locked);
        }
    }
}
