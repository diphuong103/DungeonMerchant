using UnityEngine;

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
            if (Input.GetKeyDown(KeyCode.B))
                TogglePanel(inventoryPanel, settingsPanel, characterPanel, steleInteractionPanel);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (steleInteractionPanel != null && steleInteractionPanel.activeSelf)
                {
                    steleInteractionPanel.SetActive(false);
                    return;
                }

                TogglePanel(settingsPanel, inventoryPanel, characterPanel, steleInteractionPanel);
            }

            if (Input.GetKeyDown(KeyCode.Tab))
                TogglePanel(characterPanel, inventoryPanel, settingsPanel, steleInteractionPanel);
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
