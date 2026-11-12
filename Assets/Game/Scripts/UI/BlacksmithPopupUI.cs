using UnityEngine;
using UnityEngine.UI;

namespace Cainos.PixelArtTopDown_Basic
{
    [DisallowMultipleComponent]
    public sealed class BlacksmithPopupUI : MonoBehaviour
    {
        public static BlacksmithPopupUI Instance { get; private set; }
        public bool IsOpen => popupRoot != null && popupRoot.activeSelf;

        [SerializeField] private Sprite panelFrame;
        [SerializeField] private Sprite slotFrame;
        [SerializeField] private Sprite selectedSlotFrame;
        [SerializeField] private Sprite wideButtonFrame;
        [SerializeField] private Font popupFont;
        [SerializeField] private GameObject popupRoot;
        private Text titleText;
        private Text weaponLevelText;
        private Text attackText;
        private Text materialOreText;
        private Text materialGemText;
        private Text materialWoodText;
        private Text actionText;
        private bool upgraded;
        private bool weaponsTab = true;

        private static readonly Color Gold = new Color(1f, 0.72f, 0.25f);
        private static readonly Color Panel = new Color(0.14f, 0.16f, 0.21f, 0.98f);
        private static readonly Color Muted = new Color(0.69f, 0.72f, 0.78f);

        private void Awake()
        {
            Instance = this;
            if (popupRoot == null) Build();
            else BindExisting();
            popupRoot.SetActive(false);
        }

        public void Open()
        {
            if (popupRoot == null) Build();
            upgraded = false;
            RefreshExample();
            popupRoot.SetActive(true);
        }

        public void Close()
        {
            if (popupRoot != null) popupRoot.SetActive(false);
        }

        private void Update()
        {
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
                Close();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Build()
        {
            if (popupRoot != null) return;

            popupRoot = MakeRect("Blacksmith Popup", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            var overlay = popupRoot.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.62f);
            var overlayButton = popupRoot.AddComponent<Button>();
            overlayButton.transition = Selectable.Transition.None;
            overlayButton.onClick.AddListener(Close);

            RectTransform frame = MakeRect("Frame", popupRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1040f, 620f));
            var frameImage = frame.gameObject.AddComponent<Image>();
            frameImage.sprite = panelFrame;
            frameImage.type = panelFrame != null ? Image.Type.Sliced : Image.Type.Simple;
            frameImage.color = Color.white;
            var blocker = frame.gameObject.AddComponent<Button>();
            blocker.transition = Selectable.Transition.None;
            blocker.onClick.AddListener(() => { });

            titleText = MakeText("Title", frame, "L\u00D2 R\u00C8N - N\u00C2NG C\u1EA4P V\u0168 KH\u00CD", 30, Gold, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(700f, 54f), TextAnchor.MiddleCenter);
            MakeButton("Close", frame, "X", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-42f, -38f), new Vector2(52f, 48f), new Color(0.48f, 0.18f, 0.16f), Close);

            RectTransform left = MakePanel("Equipment Panel", frame, new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(28f, 28f), new Vector2(-10f, -105f));
            RectTransform right = MakePanel("Upgrade Details", frame, new Vector2(0.5f, 0f), new Vector2(1f, 1f), new Vector2(10f, 28f), new Vector2(-28f, -105f));

            MakeButton("Equipment Tab", left, "TRANG B\u1ECA", new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(8f, -8f), new Vector2(-6f, 48f), new Color(0.34f, 0.29f, 0.22f), () => SelectTab(false));
            MakeButton("Weapons Tab", left, "V\u0168 KH\u00CD", new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(6f, -8f), new Vector2(-8f, 48f), Gold, () => SelectTab(true));

            RectTransform grid = MakeRect("Weapon Grid", left, Vector2.zero, Vector2.one, new Vector2(14f, 14f), new Vector2(-14f, -70f));
            var gridLayout = grid.gameObject.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(62f, 62f);
            gridLayout.spacing = new Vector2(11f, 11f);
            gridLayout.padding = new RectOffset(6, 6, 6, 6);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 5;
            for (int i = 0; i < 20; i++)
            {
                int index = i;
                var slot = MakeButton("Weapon Slot " + (i + 1), grid, i == 0 ? "KIẾM" : "", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                    i == 0 ? new Color(0.60f, 0.39f, 0.12f) : new Color(0.19f, 0.23f, 0.29f), () => SelectSlot(index));
                var rt = slot.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.zero;
                rt.sizeDelta = gridLayout.cellSize;
                var slotImage = slot.GetComponent<Image>();
                slotImage.sprite = i == 0 ? selectedSlotFrame : slotFrame;
                slotImage.type = slotImage.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            }

            MakeText("Selected Item", right, "THANH KI\u1EBEM TH\u00C1NH", 26, Gold, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(22f, -34f), new Vector2(-44f, 44f), TextAnchor.MiddleCenter);
            weaponLevelText = MakeText("Weapon Level", right, "", 22, Color.white, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(22f, -84f), new Vector2(-44f, 42f), TextAnchor.MiddleCenter);

            RectTransform stats = MakePanel("Upgrade Stats", right, new Vector2(0f, 0.53f), new Vector2(1f, 0.79f), new Vector2(24f, 0f), new Vector2(-24f, 0f));
            MakeText("Stats Heading", stats, "CH\u1EC8 S\u1ED0 N\u00C2NG C\u1EA4P", 18, Gold, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(8f, -4f), new Vector2(-16f, 30f), TextAnchor.MiddleCenter);
            MakeText("Attack Label", stats, "T\u1EA5n c\u00F4ng", 18, Muted, new Vector2(0f, 0.5f), new Vector2(0.4f, 0.5f), new Vector2(12f, 2f), new Vector2(-8f, 28f), TextAnchor.MiddleLeft);
            attackText = MakeText("Attack Value", stats, "", 18, Color.white, new Vector2(0.4f, 0.5f), new Vector2(1f, 0.5f), new Vector2(8f, 2f), new Vector2(-12f, 28f), TextAnchor.MiddleRight);

            RectTransform materials = MakePanel("Required Materials", right, new Vector2(0f, 0.21f), new Vector2(1f, 0.49f), new Vector2(24f, 0f), new Vector2(-24f, 0f));
            MakeText("Materials Heading", materials, "NGUY\u00CAN LI\u1EC6U C\u1EA6N THI\u1EBET", 18, Gold, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(8f, -4f), new Vector2(-16f, 30f), TextAnchor.MiddleCenter);
            materialOreText = MakeText("Ore Requirement", materials, "QU\u1EB6NG TH\u00C9P\n12 / 15", 16, Color.white, new Vector2(0f, 0f), new Vector2(0.34f, 0.84f), new Vector2(6f, 2f), new Vector2(-4f, -8f), TextAnchor.MiddleCenter);
            materialGemText = MakeText("Gem Requirement", materials, "\u0110\u00C1 C\u01AF\u1EDCNG H\u00D3A\n4 / 5", 16, Color.white, new Vector2(0.34f, 0f), new Vector2(0.67f, 0.84f), new Vector2(4f, 2f), new Vector2(-4f, -8f), TextAnchor.MiddleCenter);
            materialWoodText = MakeText("Wood Requirement", materials, "G\u1ED6 C\u1ED4 TH\u1EE4\n18 / 20", 16, Color.white, new Vector2(0.67f, 0f), new Vector2(1f, 0.84f), new Vector2(4f, 2f), new Vector2(-6f, -8f), TextAnchor.MiddleCenter);

            MakeText("Gold Cost", right, "250 V\u00C0NG", 18, Gold, new Vector2(0f, 0.12f), new Vector2(1f, 0.12f), new Vector2(18f, 0f), new Vector2(-36f, 32f), TextAnchor.MiddleCenter);
            var upgrade = MakeButton("Upgrade Button", right, "N\u00C2NG C\u1EA4P", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(250f, 60f), Gold, Upgrade);
            actionText = upgrade.GetComponentInChildren<Text>();
            RefreshExample();
        }


        private RectTransform MakePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = MakeRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = panelFrame != null ? panelFrame : slotFrame;
            image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = Color.white;
            image.raycastTarget = true;
            return rect;
        }

        private void BindExisting()
        {
            titleText = FindText("Blacksmith Popup/Frame/Title");
            weaponLevelText = FindText("Blacksmith Popup/Frame/Upgrade Details/Weapon Level");
            attackText = FindText("Blacksmith Popup/Frame/Upgrade Details/Upgrade Stats/Attack Value");
            materialOreText = FindText("Blacksmith Popup/Frame/Upgrade Details/Required Materials/Ore Requirement");
            materialGemText = FindText("Blacksmith Popup/Frame/Upgrade Details/Required Materials/Gem Requirement");
            materialWoodText = FindText("Blacksmith Popup/Frame/Upgrade Details/Required Materials/Wood Requirement");
            actionText = FindText("Blacksmith Popup/Frame/Upgrade Details/Upgrade Button/Label");
            BindButton("Blacksmith Popup/Close", Close);
            BindButton("Blacksmith Popup/Frame/Close", Close);
            BindButton("Blacksmith Popup/Frame/Equipment Panel/Equipment Tab", () => SelectTab(false));
            BindButton("Blacksmith Popup/Frame/Equipment Panel/Weapons Tab", () => SelectTab(true));
            BindButton("Blacksmith Popup/Frame/Upgrade Details/Upgrade Button", Upgrade);
        }

        private Text FindText(string path)
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<Text>() : null;
        }

        private void BindButton(string path, UnityEngine.Events.UnityAction action)
        {
            var child = transform.Find(path);
            var button = child != null ? child.GetComponent<Button>() : null;
            if (button != null) button.onClick.AddListener(action);
        }

        private void SelectTab(bool showWeapons)
        {
            weaponsTab = showWeapons;
            if (titleText != null)
                titleText.text = showWeapons ? "L\u00D2 R\u00C8N - N\u00C2NG C\u1EA4P V\u0168 KH\u00CD" : "L\u00D2 R\u00C8N - TRANG B\u1ECA";
        }

        private Button MakeButton(string name, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color, UnityEngine.Events.UnityAction click)
        {
            var rect = MakeRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = wideButtonFrame;
            image.type = wideButtonFrame != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(click);
            if (!string.IsNullOrEmpty(label))
                MakeText("Label", rect, label, 17, Color.white, Vector2.zero, Vector2.one, new Vector2(4f, 2f), new Vector2(-4f, -2f), TextAnchor.MiddleCenter);
            return button;
        }

        private RectTransform MakeRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            if (Mathf.Approximately(anchorMin.x, anchorMax.x) && Mathf.Approximately(anchorMin.y, anchorMax.y))
            {
                rect.anchoredPosition = offsetMin;
                rect.sizeDelta = offsetMax;
            }
            else
            {
                rect.offsetMin = offsetMin;
                rect.offsetMax = offsetMax;
            }
            return rect;
        }

        private Text MakeText(string name, Transform parent, string value, int size, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, TextAnchor alignment)
        {
            var rect = MakeRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var text = rect.gameObject.AddComponent<Text>();
            text.text = value;
            text.font = popupFont != null ? popupFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        private void SelectSlot(int index)
        {
            if (index == 0) return;
            if (titleText != null) titleText.text = "L\u00D2 R\u00C8N - CH\u1ECCN V\u0168 KH\u00CD";
        }

        private void RefreshExample()
        {
            if (titleText != null) titleText.text = "L\u00D2 R\u00C8N - N\u00C2NG C\u1EA4P V\u0168 KH\u00CD";
            if (weaponLevelText != null) weaponLevelText.text = upgraded ? "C\u1EA4P +8  OK" : "C\u1EA4P +7   ->   +8";
            if (attackText != null) attackText.text = upgraded ? "240  (DA NANG CAP)" : "210   ->   240  (+30)";
            if (materialOreText != null) materialOreText.text = upgraded ? "QU\u1EB6NG TH\u00C9P\n0 / 15" : "QU\u1EB6NG TH\u00C9P\n12 / 15";
            if (materialGemText != null) materialGemText.text = upgraded ? "\u0110\u00C1 C\u01AF\u1EDCNG H\u00D3A\n0 / 5" : "\u0110\u00C1 C\u01AF\u1EDCNG H\u00D3A\n4 / 5";
            if (materialWoodText != null) materialWoodText.text = upgraded ? "G\u1ED6 C\u1ED4 TH\u1EE4\n0 / 20" : "G\u1ED6 C\u1ED4 TH\u1EE4\n18 / 20";
            if (actionText != null) actionText.text = upgraded ? "DA NANG CAP" : "N\u00C2NG C\u1EA4P";
        }

        private void Upgrade()
        {
            upgraded = true;
            RefreshExample();
        }
    }
}

