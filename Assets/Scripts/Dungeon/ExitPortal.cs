using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class ExitPortal : MonoBehaviour
    {
        public string sceneToLoad = "Home";

        private void Reset()
        {
            Collider2D trigger = GetComponent<Collider2D>();
            trigger.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player") || string.IsNullOrWhiteSpace(sceneToLoad)) return;
            SceneManager.LoadScene("Home");
        }
    }
}
