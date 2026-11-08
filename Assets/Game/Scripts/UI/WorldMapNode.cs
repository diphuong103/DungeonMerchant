using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cainos.PixelArtTopDown_Basic
{
    /// <summary>
    /// Data and UI references for one selectable dungeon on the Canvas map.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WorldMapNode : MonoBehaviour
    {
        [Header("Dungeon")]
        [SerializeField] private string nodeID;
        [SerializeField] private string nodeName;
        [SerializeField, TextArea(2, 5)] private string description;
        [SerializeField] private bool isUnlocked;
        [SerializeField, Min(1)] private int requiredLevel = 1;
#if UNITY_EDITOR
        [SerializeField] private UnityEditor.SceneAsset targetSceneAsset;
#endif
        [SerializeField] private string targetSceneName;
        [SerializeField] private Sprite iconSprite;
        [SerializeField] private Sprite unlockedFrameSprite;
        [SerializeField] private Sprite lockedFrameSprite;

        [Header("Canvas References")]
        [SerializeField] private Button nodeButton;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text iconPlaceholder;
        [SerializeField] private TMP_Text mapLabel;
        [SerializeField] private TMP_Text lockLabel;
        [SerializeField] private WorldMapCircleGraphic frame;
        [SerializeField] private WorldMapCircleGraphic frameCore;

        private WorldMapScreen mapScreen;

        public string NodeID => nodeID;
        public string NodeName => nodeName;
        public string Description => description;
        public bool IsUnlocked => isUnlocked;
        public int RequiredLevel => requiredLevel;
        public string TargetSceneName => targetSceneName;

        public void Configure(
            string id,
            string title,
            string details,
            bool unlocked,
            string sceneName,
            Button button,
            Image nodeIcon,
            TMP_Text placeholder,
            TMP_Text label,
            TMP_Text lockedLabel,
            WorldMapCircleGraphic outerFrame,
            WorldMapCircleGraphic innerFrame,
            int levelRequirement = 1)
        {
            nodeID = id;
            nodeName = title;
            description = details;
            isUnlocked = unlocked;
            requiredLevel = Mathf.Max(1, levelRequirement);
            targetSceneName = sceneName;
            nodeButton = button;
            iconImage = nodeIcon;
            iconPlaceholder = placeholder;
            mapLabel = label;
            lockLabel = lockedLabel;
            frame = outerFrame;
            frameCore = innerFrame;
            RefreshView();
        }

        public void Bind(WorldMapScreen owner)
        {
            mapScreen = owner;
            if (nodeButton == null)
                nodeButton = GetComponent<Button>();
            if (nodeButton == null)
                return;

            nodeButton.onClick.RemoveListener(Select);
            nodeButton.onClick.AddListener(Select);
            RefreshView();
        }

        public void SetUnlocked(bool unlocked)
        {
            isUnlocked = unlocked;
            RefreshView();
        }

        public void SetFrameSprites(Sprite unlockedSprite, Sprite lockedSprite)
        {
            unlockedFrameSprite = unlockedSprite;
            lockedFrameSprite = lockedSprite;
            RefreshView();
        }

        private void Select()
        {
            if (mapScreen != null)
                mapScreen.SelectNode(this);
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            // SceneAsset is editor-only. Keep its name in a runtime-safe string
            // so SceneManager can load it in builds.
            if (targetSceneAsset != null)
                targetSceneName = targetSceneAsset.name;
#endif
            RefreshView();
        }

        private void RefreshView()
        {
            if (mapLabel != null)
                mapLabel.text = nodeName;
            if (lockLabel != null)
            {
                lockLabel.text = "CẦN VƯỢT MÀN TRƯỚC";
                lockLabel.gameObject.SetActive(!isUnlocked);
            }
            if (iconImage != null)
            {
                iconImage.sprite = iconSprite;
                iconImage.enabled = iconSprite != null;
            }
            if (iconPlaceholder != null)
            {
                iconPlaceholder.text = iconSprite == null ? (isUnlocked ? "+" : "?") : string.Empty;
                iconPlaceholder.gameObject.SetActive(iconSprite == null);
            }
            if (frame != null)
            {
                frame.sprite = isUnlocked ? unlockedFrameSprite : lockedFrameSprite;
                frame.color = Color.white;
            }
            if (frameCore != null)
                frameCore.gameObject.SetActive(false);
        }
    }
}
