using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DungeonMerchant.UI
{
    /// <summary>Basic reusable pause, game-over, settings, and boss-victory overlay.</summary>
    [DisallowMultipleComponent]
    public sealed class DungeonOverlayUI : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TMP_Text rewardListText;

        private bool resultShown;
        public static DungeonOverlayUI Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            Bind(pausePanel, "Continue", ResumeGame);
            Bind(pausePanel, "Settings", OpenSettings);
            Bind(pausePanel, "Give Up", GiveUp);
            Bind(settingsPanel, "Back", ReturnToPause);
            Bind(gameOverPanel, "Retry", RestartLevel);
            Bind(gameOverPanel, "Return", ReturnToVillage);
            Bind(victoryPanel, "Return", CompleteAndReturnToVillage);

            SetPanels(false, false, false, false);
            Time.timeScale = 1f;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape) || resultShown) return;

            if (settingsPanel != null && settingsPanel.activeSelf)
                ReturnToPause();
            else if (pausePanel != null && pausePanel.activeSelf)
                ResumeGame();
            else
                PauseGame();
        }

        public void PauseGame()
        {
            if (resultShown) return;
            SetPanels(true, false, false, false);
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            if (resultShown) return;
            SetPanels(false, false, false, false);
            Time.timeScale = 1f;
        }

        public void OpenSettings()
        {
            if (resultShown) return;
            SetPanels(false, true, false, false);
            Time.timeScale = 0f;
        }

        public void ReturnToPause()
        {
            if (resultShown) return;
            SetPanels(true, false, false, false);
        }

        public void GiveUp()
        {
            resultShown = true;
            SetPanels(false, false, true, false);
            Time.timeScale = 0f;
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ReturnToVillage()
        {
            Time.timeScale = 1f;
            Cainos.PixelArtTopDown_Basic.WorldMapProgress.ReturnToVillage();
        }

        public void CompleteAndReturnToVillage()
        {
            string dungeonId = Cainos.PixelArtTopDown_Basic.WorldMapProgress.GetCurrentDungeonId();
            Cainos.PixelArtTopDown_Basic.WorldMapProgress.MarkDungeonCleared(
                string.IsNullOrWhiteSpace(dungeonId) ? "crypt" : dungeonId);
            ReturnToVillage();
        }

        public static void NotifyBossDefeated(string rewardName = null)
        {
            if (Instance == null)
            {
                Debug.LogWarning("Boss was defeated, but no DungeonOverlayUI is present in the scene.");
                return;
            }

            Instance.ShowVictory(rewardName);
        }

        private void ShowVictory(string rewardName)
        {
            resultShown = true;
            if (rewardListText != null)
            {
                rewardListText.text = string.IsNullOrWhiteSpace(rewardName)
                    ? "Vật phẩm nhận được:\nChưa có phần thưởng được cấu hình."
                    : "Vật phẩm nhận được:\n• " + rewardName.Replace("PF_", string.Empty).Replace('_', ' ');
            }

            SetPanels(false, false, false, true);
            Time.timeScale = 0f;
        }

        private void SetPanels(bool pause, bool settings, bool gameOver, bool victory)
        {
            if (pausePanel != null) pausePanel.SetActive(pause);
            if (settingsPanel != null) settingsPanel.SetActive(settings);
            if (gameOverPanel != null) gameOverPanel.SetActive(gameOver);
            if (victoryPanel != null) victoryPanel.SetActive(victory);
        }

        private static void Bind(GameObject panel, string buttonName, UnityEngine.Events.UnityAction action)
        {
            if (panel == null) return;
            var transforms = panel.GetComponentsInChildren<Transform>(true);
            foreach (var child in transforms)
            {
                if (child.name != buttonName) continue;
                var button = child.GetComponent<Button>();
                if (button != null) button.onClick.AddListener(action);
                return;
            }
        }
    }
}
