using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class BossRoomTrigger : MonoBehaviour
    {
        public DungeonController controller;

        private void Reset()
        {
            Collider2D trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && controller != null)
                controller.TryStartBoss(other.transform);
        }
    }
}
