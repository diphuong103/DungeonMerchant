using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class Chest : MonoBehaviour
    {
        public GameObject rewardPrefab;
        public Transform rewardSpawnPoint;
        public bool opened;

        private bool playerNearby;

        private void Reset()
        {
            Collider2D trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
        }

        private void Update()
        {
            if (!opened && playerNearby && Input.GetKeyDown(KeyCode.E)) Open();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) playerNearby = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player")) playerNearby = false;
        }

        public void Open()
        {
            if (opened) return;
            opened = true;
            if (rewardPrefab == null) return;
            Vector3 position = rewardSpawnPoint != null ? rewardSpawnPoint.position : transform.position;
            Instantiate(rewardPrefab, position, Quaternion.identity);
        }
    }
}
