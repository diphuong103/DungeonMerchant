using UnityEngine;

namespace DungeonMerchant.Dungeons
{
    public sealed class DungeonSpawnPoint : MonoBehaviour
    {
        [SerializeField] private bool placePlayerOnStart = true;
        public static Vector3 LastSpawnPosition { get; private set; }

        private void Awake() => LastSpawnPosition = transform.position;

        private void Start()
        {
            if (!placePlayerOnStart) return;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) player.transform.position = transform.position;
        }

        public static void Set(Vector3 position) => LastSpawnPosition = position;
    }
}
