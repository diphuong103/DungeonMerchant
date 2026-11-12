using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonMerchant.Dungeons
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class DungeonPortal : MonoBehaviour
    {
        [SerializeField] private string destinationScene = "Gameplay";
        [SerializeField] private bool requireInteract = false;
        [SerializeField] private SpriteRenderer portalVisual;
        private bool playerNearby;

        private void Update()
        {
            if (requireInteract && playerNearby && Input.GetKeyDown(KeyCode.E))
                LoadDestination();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            playerNearby = true;
            if (!requireInteract) LoadDestination();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player")) playerNearby = false;
        }

        private void LoadDestination()
        {
            if (!string.IsNullOrWhiteSpace(destinationScene))
                SceneManager.LoadScene(destinationScene);
        }
    }
}
