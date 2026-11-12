using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Cainos.PixelArtTopDown_Basic
{
    public sealed class HubMenuHotkeys : MonoBehaviour
    {
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject characterPanel;
        [SerializeField] private GameObject steleInteractionPanel;

        private void Update()
        {
            if (KeyPressed(KeyCode.B))
                ToggleInventory();

            if (KeyPressed(KeyCode.Escape))
            {
                if (steleInteractionPanel != null && steleInteractionPanel.activeSelf)
                {
                    steleInteractionPanel.SetActive(false);
                    return;
                }

                ToggleSettings();
            }

            if (KeyPressed(KeyCode.Tab))
                ToggleCharacter();
        }

        public void ToggleInventory()
        {
            TogglePanel(inventoryPanel, settingsPanel, characterPanel, steleInteractionPanel);
        }

        public void ToggleSettings()
        {
            TogglePanel(settingsPanel, inventoryPanel, characterPanel, steleInteractionPanel);
        }

        public void ToggleCharacter()
        {
            TogglePanel(characterPanel, inventoryPanel, settingsPanel, steleInteractionPanel);
        }

        public void CloseInventory()
        {
            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);
        }

        public void CloseCharacter()
        {
            if (characterPanel != null)
                characterPanel.SetActive(false);
        }

        private static bool KeyPressed(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null)
                return false;

            Key inputKey = key == KeyCode.B ? Key.B
                : key == KeyCode.Tab ? Key.Tab
                : Key.Escape;
            return Keyboard.current[inputKey].wasPressedThisFrame;
#else
            return Input.GetKeyDown(key);
#endif
        }

        private static void TogglePanel(GameObject panelToToggle, GameObject otherPanel, GameObject thirdPanel, GameObject fourthPanel)
        {
            if (panelToToggle == null)
                return;

            bool shouldOpen = !panelToToggle.activeSelf;
            if (otherPanel != null)
                otherPanel.SetActive(false);
            if (thirdPanel != null)
                thirdPanel.SetActive(false);
            if (fourthPanel != null)
                fourthPanel.SetActive(false);

            panelToToggle.SetActive(shouldOpen);
        }
    }
}
