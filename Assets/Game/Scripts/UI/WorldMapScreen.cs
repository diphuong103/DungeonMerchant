using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cainos.PixelArtTopDown_Basic
{
    /// <summary>
    /// Builds the first-pass world map UI at runtime so placeholder art can be
    /// replaced later without changing the map interaction flow.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WorldMapScreen : MonoBehaviour
    {
        [Serializable]
        private sealed class DungeonNode
        {
            public string id;
            public string displayName;
            [TextArea] public string description;
            [Range(0f, 1f)] public float x;
            [Range(0f, 1f)] public float y;
            public bool unlocked;
            public string targetSceneName;
            public string icon = "✦";
        }

        public static WorldMapScreen Instance { get; private set; }
        public bool IsOpen => mapRoot != null && mapRoot.activeSelf;

        [SerializeField] private List<DungeonNode> dungeons = new List<DungeonNode>
        {
            new DungeonNode { id = "cursed_forest", displayName = "RỪNG NGUYÊN RỦA", description = "Một khu rừng cổ bị ma thuật hoang dã xâm chiếm. Những lối mòn sâu bên trong vẫn chưa được khám phá.", x = 0.25f, y = 0.67f, unlocked = true, icon = "R" },
            new DungeonNode { id = "mana_mine", displayName = "MỎ PHA LÊ MANA", description = "Mạch pha lê xanh phát sáng dưới lòng đất. Khu vực này hiện vẫn bị phong ấn.", x = 0.73f, y = 0.68f, unlocked = false, icon = "M" },
            new DungeonNode { id = "soul_swamp", displayName = "ĐẦM LẦY LINH HỒN", description = "Sương linh hồn phủ kín đầm lầy. Con đường đến đây chưa được khai thông.", x = 0.12f, y = 0.16f, unlocked = false, icon = "L" },
            new DungeonNode { id = "dragon_cave", displayName = "HANG ĐỘNG RỒNG", description = "Một hang động xa xôi được cho là nơi trú ngụ của loài rồng cổ.", x = 0.86f, y = 0.17f, unlocked = false, icon = "D" }
        };

        private static readonly Color Ink = new Color(0.08f, 0.075f, 0.15f, 0.96f);
        private static readonly Color Gold = new Color(0.95f, 0.72f, 0.34f, 1f);
        private static readonly Color PaleGold = new Color(1f, 0.88f, 0.62f, 1f);
        private static readonly Color Muted = new Color(0.72f, 0.70f, 0.77f, 1f);

        private GameObject mapRoot;
        private GameObject detailPopup;
        private TMP_Text statusText;
        private DungeonNode selectedDungeon;
        private Texture2D circleTexture;
        private Sprite circleSprite;
        private Texture2D whiteTexture;
        private Sprite whiteSprite;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureEventSystem();
            BuildMapUi();
        }

        private static void EnsureEventSystem()
        {
            var eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
                eventSystem = new GameObject("World Map EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).GetComponent<EventSystem>();
            else if (eventSystem.GetComponent<StandaloneInputModule>() == null && eventSystem.GetComponent<BaseInputModule>() == null)
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }

        public void Open()
        {
            if (mapRoot == null)
                BuildMapUi();

            mapRoot.SetActive(true);
            if (detailPopup != null)
                detailPopup.SetActive(false);
            if (statusText != null)
                statusText.text = "Chọn một hầm ngục để chiến đấu";
        }

        public void Close()
        {
            if (detailPopup != null)
                detailPopup.SetActive(false);
            if (mapRoot != null)
                mapRoot.SetActive(false);
        }

        private void Update()
        {
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
                Close();
        }

        private void BuildMapUi()
        {
            var canvasObject = new GameObject("World Map Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            mapRoot = MakeRect("World Map Screen", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            var background = mapRoot.AddComponent<Image>();
            background.color = new Color(0.035f, 0.045f, 0.12f, 1f);
            background.sprite = WhiteSprite;
            background.raycastTarget = true;

            // Soft teal and violet pools echo the existing magical lighting.
            AddGlow(mapRoot.transform, new Vector2(0.37f, 0.82f), new Vector2(520f, 420f), new Color(0.09f, 0.55f, 0.43f, 0.22f));
            AddGlow(mapRoot.transform, new Vector2(0.72f, 0.63f), new Vector2(570f, 470f), new Color(0.33f, 0.16f, 0.58f, 0.30f));
            AddGlow(mapRoot.transform, new Vector2(0.10f, 0.24f), new Vector2(480f, 380f), new Color(0.08f, 0.49f, 0.68f, 0.27f));
            AddGlow(mapRoot.transform, new Vector2(0.57f, 0.12f), new Vector2(420f, 300f), new Color(0.10f, 0.50f, 0.35f, 0.18f));
            AddAmbientMarks(mapRoot.transform);
            AddRoute(mapRoot.transform, new Vector2(0.25f, 0.67f), new Vector2(0.49f, 0.48f));
            AddRoute(mapRoot.transform, new Vector2(0.73f, 0.68f), new Vector2(0.49f, 0.48f));
            AddRoute(mapRoot.transform, new Vector2(0.12f, 0.16f), new Vector2(0.49f, 0.48f));
            AddRoute(mapRoot.transform, new Vector2(0.86f, 0.17f), new Vector2(0.49f, 0.48f));

            AddLabel(mapRoot.transform, "BẢN ĐỒ HẦM NGỤC", new Vector2(0.5f, 0.94f), 30, PaleGold, FontStyles.Bold);
            AddLabel(mapRoot.transform, "NHỮNG VÙNG ĐẤT ĐANG CHỜ ĐƯỢC KHÁM PHÁ", new Vector2(0.5f, 0.895f), 13, Muted, FontStyles.Normal);

            var back = MakeButton(mapRoot.transform, "←  QUAY LẠI LÀNG", new Vector2(0.12f, 0.93f), new Vector2(210f, 48f), Ink, PaleGold, 16);
            back.onClick.AddListener(Close);
            statusText = AddLabel(mapRoot.transform, "Chọn một hầm ngục để chiến đấu", new Vector2(0.5f, 0.055f), 14, Muted, FontStyles.Italic);

            AddSanctuary(mapRoot.transform);
            foreach (var dungeon in dungeons)
                AddDungeonNode(mapRoot.transform, dungeon);

            detailPopup = BuildDetailPopup(mapRoot.transform);
            mapRoot.SetActive(false);
        }

        private void AddDungeonNode(Transform parent, DungeonNode dungeon)
        {
            var nodeRect = MakeRect("Dungeon Node", parent, new Vector2(dungeon.x, dungeon.y), new Vector2(dungeon.x, dungeon.y), new Vector2(-37f, -37f), new Vector2(37f, 37f));
            var ring = nodeRect.gameObject.AddComponent<Image>();
            ring.sprite = CircleSprite;
            ring.type = Image.Type.Simple;
            ring.color = dungeon.unlocked ? Gold : new Color(0.48f, 0.46f, 0.50f, 1f);
            var button = nodeRect.gameObject.AddComponent<Button>();
            button.targetGraphic = ring;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = PaleGold;
            colors.pressedColor = Gold;
            button.colors = colors;

            var innerRect = MakeRect("Node Core", button.transform, Vector2.zero, Vector2.one, new Vector2(5f, 5f), new Vector2(-5f, -5f));
            var inner = innerRect.gameObject.AddComponent<Image>();
            inner.sprite = CircleSprite;
            inner.type = Image.Type.Simple;
            inner.color = dungeon.unlocked ? new Color(0.18f, 0.12f, 0.17f, 1f) : new Color(0.14f, 0.15f, 0.19f, 1f);
            inner.raycastTarget = false;
            AddText(button.transform, dungeon.unlocked ? dungeon.icon : "?", 28, dungeon.unlocked ? new Color(0.22f, 0.88f, 0.55f, 1f) : Muted, FontStyles.Bold, new Vector2(0f, 0f), new Vector2(0.9f, 0.8f));
            button.onClick.AddListener(() => SelectDungeon(dungeon));

            var label = AddLabel(parent, dungeon.displayName, new Vector2(dungeon.x, dungeon.y - 0.075f), 14, dungeon.unlocked ? PaleGold : new Color(0.63f, 0.62f, 0.65f, 1f), FontStyles.Bold);
            label.outlineWidth = 0.18f;
            label.outlineColor = Color.black;
            if (!dungeon.unlocked)
                AddLabel(parent, "BỊ PHONG ẤN", new Vector2(dungeon.x, dungeon.y - 0.105f), 10, new Color(0.80f, 0.43f, 0.43f, 1f), FontStyles.Normal);
        }

        private void AddSanctuary(Transform parent)
        {
            var sanctuary = MakeRect("Sanctuary Marker", parent, new Vector2(0.49f, 0.48f), new Vector2(0.49f, 0.48f), new Vector2(-39f, -39f), new Vector2(39f, 39f)).gameObject.AddComponent<Image>();
            sanctuary.sprite = CircleSprite;
            sanctuary.type = Image.Type.Simple;
            sanctuary.color = Gold;
            var core = MakeRect("Sanctuary Core", sanctuary.transform, Vector2.zero, Vector2.one, new Vector2(5f, 5f), new Vector2(-5f, -5f)).gameObject.AddComponent<Image>();
            core.sprite = CircleSprite;
            core.type = Image.Type.Simple;
            core.color = new Color(0.18f, 0.12f, 0.12f, 1f);
            AddText(sanctuary.transform, "T", 33, new Color(0.32f, 0.83f, 0.23f, 1f), FontStyles.Bold, Vector2.zero, new Vector2(0.9f, 0.85f));
            var title = AddLabel(parent, "CÂY CỔ THỤ THẦN", new Vector2(0.49f, 0.415f), 14, PaleGold, FontStyles.Bold);
            title.outlineWidth = 0.18f;
            title.outlineColor = Color.black;
        }

        private GameObject BuildDetailPopup(Transform parent)
        {
            var dim = MakeRect("Dungeon Detail Popup", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            var shade = dim.AddComponent<Image>();
            shade.color = new Color(0f, 0f, 0.04f, 0.64f);
            shade.sprite = WhiteSprite;

            var card = MakeRect("Popup Card", dim.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-235f, -190f), new Vector2(235f, 190f)).gameObject;
            var cardImage = card.AddComponent<Image>();
            cardImage.color = Gold;
            cardImage.sprite = WhiteSprite;
            AddBorder(card.transform, new Vector2(470f, 380f));
            var title = AddText(card.transform, "HẦM NGỤC", 23, PaleGold, FontStyles.Bold, new Vector2(0.08f, 0.76f), new Vector2(0.92f, 0.94f));
            title.name = "Dungeon Title";
            var description = AddText(card.transform, string.Empty, 16, new Color(0.86f, 0.83f, 0.80f, 1f), FontStyles.Normal, new Vector2(0.10f, 0.50f), new Vector2(0.90f, 0.79f));
            description.name = "Dungeon Description";
            description.alignment = TextAlignmentOptions.TopLeft;
            var lockText = AddText(card.transform, "KHU VỰC NÀY ĐANG BỊ PHONG ẤN", 13, new Color(0.91f, 0.46f, 0.43f, 1f), FontStyles.Bold, new Vector2(0.08f, 0.31f), new Vector2(0.92f, 0.41f));
            lockText.name = "Lock Message";
            var fight = MakeButton(card.transform, "CHIẾN ĐẤU", new Vector2(0.5f, 0.18f), new Vector2(190f, 52f), new Color(0.36f, 0.19f, 0.12f, 1f), PaleGold, 18);
            fight.name = "Fight Button";
            fight.onClick.AddListener(StartSelectedDungeon);
            var close = MakeButton(card.transform, "ĐÓNG", new Vector2(0.5f, 0.06f), new Vector2(130f, 38f), new Color(0.19f, 0.17f, 0.22f, 1f), Muted, 14);
            close.onClick.AddListener(() => dim.SetActive(false));
            dim.SetActive(false);
            return dim;
        }

        private void SelectDungeon(DungeonNode dungeon)
        {
            selectedDungeon = dungeon;
            var card = detailPopup.transform.Find("Popup Card");
            card.Find("Dungeon Title").GetComponent<TMP_Text>().text = dungeon.displayName;
            card.Find("Dungeon Description").GetComponent<TMP_Text>().text = dungeon.description;
            card.Find("Lock Message").gameObject.SetActive(!dungeon.unlocked);
            card.Find("Fight Button").GetComponent<Button>().interactable = dungeon.unlocked;
            detailPopup.SetActive(true);
        }

        private void StartSelectedDungeon()
        {
            if (selectedDungeon == null || !selectedDungeon.unlocked)
                return;

            if (!string.IsNullOrWhiteSpace(selectedDungeon.targetSceneName) && Application.CanStreamedLevelBeLoaded(selectedDungeon.targetSceneName))
            {
                SceneManager.LoadScene(selectedDungeon.targetSceneName);
                return;
            }

            statusText.text = "Màn chơi chưa được gán — có thể cập nhật sau khi hoàn thiện hình ảnh.";
            detailPopup.SetActive(false);
        }

        private void AddRoute(Transform parent, Vector2 from, Vector2 to)
        {
            const int dots = 22;
            for (int i = 0; i <= dots; i++)
            {
                float t = i / (float)dots;
                var dot = MakeRect("Map Route Dot", parent, Vector2.Lerp(from, to, t), Vector2.Lerp(from, to, t), new Vector2(-2.5f, -2.5f), new Vector2(2.5f, 2.5f)).gameObject.AddComponent<Image>();
                dot.sprite = CircleSprite;
                dot.color = new Color(Gold.r, Gold.g, Gold.b, 0.44f);
                dot.raycastTarget = false;
            }
        }

        private void AddAmbientMarks(Transform parent)
        {
            var random = new System.Random(371);
            string[] glyphs = { ".", "*", "+", "o", "^" };
            Color[] colors = { new Color(0.55f, 0.80f, 0.38f, 0.55f), new Color(0.89f, 0.82f, 0.63f, 0.60f), new Color(0.45f, 0.55f, 0.72f, 0.55f) };
            for (int i = 0; i < 82; i++)
            {
                float x = (float)random.NextDouble();
                float y = (float)random.NextDouble();
                int glyph = random.Next(glyphs.Length);
                var mark = AddLabel(parent, glyphs[glyph], new Vector2(x, y), random.Next(9, 20), colors[random.Next(colors.Length)], FontStyles.Bold);
                mark.raycastTarget = false;
            }
        }

        private void AddGlow(Transform parent, Vector2 anchor, Vector2 size, Color color)
        {
            var glow = MakeRect("Mystic Fog Glow", parent, anchor, anchor, size * -0.5f, size * 0.5f).gameObject.AddComponent<Image>();
            glow.sprite = CircleSprite;
            glow.color = color;
            glow.raycastTarget = false;
        }

        private void AddBorder(Transform parent, Vector2 size)
        {
            var inset = MakeRect("Card Face", parent, Vector2.zero, Vector2.one, new Vector2(3f, 3f), new Vector2(-3f, -3f)).gameObject.AddComponent<Image>();
            inset.color = new Color(0.10f, 0.09f, 0.15f, 1f);
            inset.sprite = WhiteSprite;
            inset.type = Image.Type.Simple;
            inset.raycastTarget = false;
            inset.transform.SetAsFirstSibling();
        }

        private Button MakeButton(Transform parent, string text, Vector2 anchor, Vector2 size, Color background, Color foreground, int fontSize)
        {
            var rect = MakeRect(string.IsNullOrEmpty(text) ? "Dungeon Node" : text, parent, anchor, anchor, size * -0.5f, size * 0.5f);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = background;
            image.sprite = WhiteSprite;
            image.type = Image.Type.Simple;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = background;
            colors.highlightedColor = new Color(Mathf.Min(1f, background.r + 0.12f), Mathf.Min(1f, background.g + 0.12f), Mathf.Min(1f, background.b + 0.12f), background.a);
            colors.pressedColor = new Color(background.r * 0.82f, background.g * 0.82f, background.b * 0.82f, background.a);
            button.colors = colors;
            if (!string.IsNullOrEmpty(text))
                AddText(rect, text, fontSize, foreground, FontStyles.Bold, Vector2.zero, Vector2.one);
            return button;
        }

        private TMP_Text AddLabel(Transform parent, string text, Vector2 anchor, int fontSize, Color color, FontStyles style, Vector2? size = null)
        {
            var rect = MakeRect(text, parent, anchor, anchor, new Vector2(-220f, -22f), new Vector2(220f, 22f));
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.fontStyle = style;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.raycastTarget = false;
            if (size.HasValue)
            {
                rect.anchorMin = anchor;
                rect.anchorMax = anchor;
                rect.sizeDelta = size.Value;
            }
            return label;
        }

        private TMP_Text AddText(Transform parent, string text, int fontSize, Color color, FontStyles style, Vector2 anchorMin, Vector2 anchorMax)
        {
            var rect = MakeRect(text, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.fontStyle = style;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.raycastTarget = false;
            return label;
        }

        private static RectTransform MakeRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            var rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        private Sprite CircleSprite
        {
            get
            {
                if (circleTexture == null)
                {
                    const int resolution = 64;
                    circleTexture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
                    circleTexture.name = "World Map Soft Circle";
                    circleTexture.wrapMode = TextureWrapMode.Clamp;
                    for (int y = 0; y < resolution; y++)
                    for (int x = 0; x < resolution; x++)
                    {
                        float distance = Vector2.Distance(new Vector2(x, y), new Vector2((resolution - 1) * 0.5f, (resolution - 1) * 0.5f)) / (resolution * 0.5f);
                        float alpha = Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(distance));
                        circleTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    circleTexture.Apply();
                }

                if (circleSprite == null)
                    circleSprite = Sprite.Create(circleTexture, new Rect(0f, 0f, circleTexture.width, circleTexture.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(16f, 16f, 16f, 16f));
                return circleSprite;
            }
        }

        private Sprite WhiteSprite
        {
            get
            {
                if (whiteTexture == null)
                {
                    whiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    whiteTexture.name = "World Map UI White Pixel";
                    whiteTexture.SetPixel(0, 0, Color.white);
                    whiteTexture.Apply();
                }

                if (whiteSprite == null)
                    whiteSprite = Sprite.Create(whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
                return whiteSprite;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            if (circleTexture != null)
                Destroy(circleTexture);
            if (circleSprite != null)
                Destroy(circleSprite);
            if (whiteSprite != null)
                Destroy(whiteSprite);
            if (whiteTexture != null)
                Destroy(whiteTexture);
        }
    }

    /// <summary>Simple vector circle used for map markers without external art.</summary>
    public sealed class WorldMapCircleGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            const int segments = 48;
            var rect = rectTransform.rect;
            var center = rect.center;
            vertexHelper.AddVert(center, color, Vector2.one * 0.5f);
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                var point = center + new Vector2(Mathf.Cos(angle) * rect.width * 0.5f, Mathf.Sin(angle) * rect.height * 0.5f);
                vertexHelper.AddVert(point, color, new Vector2(Mathf.Cos(angle) * 0.5f + 0.5f, Mathf.Sin(angle) * 0.5f + 0.5f));
            }

            for (int i = 0; i < segments; i++)
                vertexHelper.AddTriangle(0, (i + 1) % segments + 1, i + 1);
        }
    }
}
