using UnityEngine;

namespace DungeonMerchant.Dungeons
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class DungeonRewardChest : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer chestVisual;
        [SerializeField] private Color closedColor = new Color(0.72f, 0.52f, 0.18f);
        [SerializeField] private Color openColor = new Color(0.93f, 0.78f, 0.35f);
        [SerializeField] private string rewardName = "Dungeon Relic";
        private bool playerNearby;
        private bool opened;

        private void Update()
        {
            if (!opened && playerNearby && Input.GetKeyDown(KeyCode.E))
            {
                opened = true;
                if (chestVisual != null) chestVisual.color = openColor;
                Debug.Log("Chest opened: " + rewardName);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) playerNearby = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player")) playerNearby = false;
        }
    }
}
