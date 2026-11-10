using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cainos.PixelArtTopDown_Basic
{
    /// <summary>
    /// Controls the serialized World Map Canvas. All visual elements are normal
    /// scene objects so they can be arranged and edited from the Unity Editor.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WorldMapScreen : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public static WorldMapScreen Instance { get; private set; }
        public bool IsOpen => mapRoot != null && mapRoot.activeSelf;

        [Header("Canvas References")]
        [SerializeField] private GameObject mapRoot;
        [SerializeField] private RectTransform mapViewport;
        [SerializeField] private RectTransform mapContent;
        [SerializeField, Range(0.85f, 1.65f)] private float minZoom = 0.85f;
        [SerializeField, Range(0.85f, 1.65f)] private float maxZoom = 1.65f;
        [SerializeField, Min(0.1f)] private float zoomSpeed = 1.5f;
        [SerializeField] private GameObject detailPopup;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text detailTitle;
        [SerializeField] private TMP_Text detailDescription;
        [SerializeField] private TMP_Text lockedMessage;
        [SerializeField] private Button fightButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button closeDetailsButton;
        [SerializeField] private List<WorldMapNode> nodes = new List<WorldMapNode>();

        private WorldMapNode selectedNode;
        private bool isDraggingMap;
        private float mapZoom = 1f;
        private float previousPinchDistance;

        private static readonly Color Gold = new Color(0.95f, 0.72f, 0.34f, 1f);
        private static readonly Color PaleGold = new Color(1f, 0.88f, 0.62f, 1f);
        private static readonly Color Muted = new Color(0.72f, 0.70f, 0.77f, 1f);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureEventSystem();
            BindButtons();
            RefreshProgress();
            if (mapRoot != null)
                mapRoot.SetActive(false);
            if (detailPopup != null)
                detailPopup.SetActive(false);
        }

        private void Update()
        {
            if (!IsOpen)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
                return;
            }

            if (detailPopup != null && detailPopup.activeSelf)
            {
                previousPinchDistance = 0f;
                return;
            }

            HandleMouseZoom();
            HandleTouchPinch();
        }

        public void Open()
        {
            if (mapRoot == null)
            {
                Debug.LogError("World Map Canvas references are missing. Rebuild the Canvas from the WorldMapSystem component.", this);
                return;
            }

            mapRoot.SetActive(true);
            BindButtons();
            RefreshProgress();
            if (mapContent != null)
            {
                mapZoom = 1f;
                mapContent.localScale = Vector3.one;
                mapContent.anchoredPosition = Vector2.zero;
            }
            if (detailPopup != null)
                detailPopup.SetActive(false);
            if (statusText != null)
                statusText.text = "Chọn một hầm ngục để chiến đấu";
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsOpen || mapContent == null || mapViewport == null ||
                (detailPopup != null && detailPopup.activeSelf))
                return;

            GameObject pressedObject = eventData.pointerPress != null ? eventData.pointerPress : eventData.pointerEnter;
            if (pressedObject == null || !pressedObject.transform.IsChildOf(mapContent))
                return;

            isDraggingMap = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDraggingMap || mapContent == null)
                return;

            Canvas canvas = mapContent.GetComponentInParent<Canvas>();
            float scale = canvas != null ? canvas.scaleFactor : 1f;
            mapContent.anchoredPosition += eventData.delta / Mathf.Max(scale, 0.001f);
            ClampMapContent();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDraggingMap = false;
        }

        private void ClampMapContent()
        {
            if (mapContent == null || mapViewport == null)
                return;

            Vector2 contentSize = mapContent.rect.size * mapZoom;
            Vector2 viewportSize = mapViewport.rect.size;
            float maxX = Mathf.Max(0f, (contentSize.x - viewportSize.x) * 0.5f);
            float maxY = Mathf.Max(0f, (contentSize.y - viewportSize.y) * 0.5f);
            Vector2 position = mapContent.anchoredPosition;
            mapContent.anchoredPosition = new Vector2(
                Mathf.Clamp(position.x, -maxX, maxX),
                Mathf.Clamp(position.y, -maxY, maxY));
        }

        private void HandleMouseZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) < 0.001f || !PointerIsOverMapContent(Input.mousePosition))
                return;

            ZoomAt(scroll, Input.mousePosition);
        }

        private void HandleTouchPinch()
        {
            if (Input.touchCount != 2)
            {
                previousPinchDistance = 0f;
                return;
            }

            Touch first = Input.GetTouch(0);
            Touch second = Input.GetTouch(1);
            float distance = Vector2.Distance(first.position, second.position);
            Vector2 midpoint = (first.position + second.position) * 0.5f;

            if (previousPinchDistance > 0f)
            {
                float pinchDelta = (distance - previousPinchDistance) * 0.01f;
                ZoomAt(pinchDelta, midpoint);
            }

            previousPinchDistance = distance;
        }

        private bool PointerIsOverMapContent(Vector2 screenPosition)
        {
            if (mapContent == null || EventSystem.current == null)
                return false;

            var pointer = new PointerEventData(EventSystem.current) { position = screenPosition };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            if (hits.Count == 0)
                return false;

            return hits[0].gameObject.transform.IsChildOf(mapContent);
        }

        private void ZoomAt(float delta, Vector2 screenPosition)
        {
            if (mapContent == null || mapViewport == null)
                return;

            float nextZoom = Mathf.Clamp(mapZoom + delta * zoomSpeed, minZoom, maxZoom);
            if (Mathf.Approximately(nextZoom, mapZoom))
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    mapViewport, screenPosition, null, out Vector2 viewportPoint))
            {
                Vector2 position = mapContent.anchoredPosition;
                float zoomRatio = nextZoom / mapZoom;
                mapContent.anchoredPosition = viewportPoint - (viewportPoint - position) * zoomRatio;
            }

            mapZoom = nextZoom;
            mapContent.localScale = Vector3.one * mapZoom;
            ClampMapContent();
        }

        public void Close()
        {
            if (detailPopup != null)
                detailPopup.SetActive(false);
            if (mapRoot != null)
                mapRoot.SetActive(false);
        }

        public void SelectNode(WorldMapNode node)
        {
            if (node == null || detailPopup == null)
                return;

            selectedNode = node;
            if (detailTitle != null)
                detailTitle.text = node.NodeName;
            if (detailDescription != null)
                detailDescription.text = node.Description;
            if (lockedMessage != null)
            {
                lockedMessage.text = "KHU VỰC NÀY ĐANG BỊ PHONG ẤN\nHãy hoàn thành hầm ngục trước đó.";
                lockedMessage.color = node.IsUnlocked ? PaleGold : new Color(0.91f, 0.46f, 0.43f, 1f);
                lockedMessage.gameObject.SetActive(!node.IsUnlocked);
            }
            if (fightButton != null)
                fightButton.interactable = node.IsUnlocked;

            detailPopup.SetActive(true);
            detailPopup.transform.SetAsLastSibling();
        }

        private void StartSelectedDungeon()
        {
            if (selectedNode == null || !selectedNode.IsUnlocked)
                return;

            if (!string.IsNullOrWhiteSpace(selectedNode.TargetSceneName) && Application.CanStreamedLevelBeLoaded(selectedNode.TargetSceneName))
            {
                WorldMapProgress.SetReturnScene(SceneManager.GetActiveScene().name);
                WorldMapProgress.SetCurrentDungeonId(selectedNode.NodeID);
                SceneManager.LoadScene(selectedNode.TargetSceneName);
                return;
            }

            if (statusText != null)
                statusText.text = string.IsNullOrWhiteSpace(selectedNode.TargetSceneName)
                    ? "Scene hầm ngục chưa được gán cho node này."
                    : "Scene '" + selectedNode.TargetSceneName + "' chưa nằm trong Build Settings.";

            if (lockedMessage != null)
            {
                lockedMessage.text = string.IsNullOrWhiteSpace(selectedNode.TargetSceneName)
                    ? "CHƯA GÁN SCENE HẦM NGỤC\nThiết lập Target Scene trong Inspector."
                    : "SCENE CHƯA CÓ TRONG BUILD SETTINGS\n" + selectedNode.TargetSceneName;
                lockedMessage.color = PaleGold;
                lockedMessage.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Call WorldMapProgress.MarkCurrentDungeonCleared() from a dungeon
        /// victory flow. The following node will unlock the next time this map opens.
        /// </summary>
        public void RefreshProgress()
        {
            if (nodes == null)
                return;

            for (int i = 0; i < nodes.Count; i++)
            {
                WorldMapNode node = nodes[i];
                if (node == null)
                    continue;

                bool unlocked = node.IsUnlocked || WorldMapProgress.IsUnlocked(node.NodeID) || i == 0;
                if (i > 0 && nodes[i - 1] != null && WorldMapProgress.IsCleared(nodes[i - 1].NodeID))
                    unlocked = true;

                node.SetUnlocked(unlocked);
                if (unlocked)
                    WorldMapProgress.Unlock(node.NodeID);
            }

            RefreshRoutes();
        }

        private void RefreshRoutes()
        {
            // Route objects are authored in map order: crypt→tree, then each
            // subsequent dungeon branch. Keep their line state in sync with saves.
            MapPathLine[] routes = GetComponentsInChildren<MapPathLine>(true);
            if (routes.Length < 5 || nodes.Count < 5)
                return;

            routes[0].SetState(WorldMapProgress.IsCleared(nodes[0].NodeID)
                ? MapPathLine.PathState.Cleared
                : MapPathLine.PathState.Unlocked);

            for (int i = 1; i < 5; i++)
            {
                WorldMapNode destination = nodes[i];
                if (destination == null)
                    continue;

                MapPathLine.PathState state = WorldMapProgress.IsCleared(destination.NodeID)
                    ? MapPathLine.PathState.Cleared
                    : destination.IsUnlocked
                        ? MapPathLine.PathState.Unlocked
                        : MapPathLine.PathState.Locked;
                routes[i].SetState(state);
            }
        }

        private void BindButtons()
        {
            foreach (var node in nodes)
                if (node != null)
                    node.Bind(this);

            Bind(backButton, Close);
            Bind(closeDetailsButton, () => detailPopup.SetActive(false));
            Bind(fightButton, StartSelectedDungeon);
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction callback)
        {
            if (button == null)
                return;

            button.onClick.RemoveListener(callback);
            button.onClick.AddListener(callback);
        }

        private static void EnsureEventSystem()
        {
            var eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
                eventSystem = new GameObject("World Map EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).GetComponent<EventSystem>();
            else if (eventSystem.GetComponent<StandaloneInputModule>() == null && eventSystem.GetComponent<BaseInputModule>() == null)
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }

        /// <summary>
        /// One-time scene setup. Invoked by the editor during project setup; the
        /// resulting Canvas is serialized into the active scene.
        /// </summary>
        public void BuildCanvasForScene()
        {
            if (mapRoot != null)
            {
                Debug.LogWarning("World Map Canvas already exists. Remove its Canvas child before rebuilding.", this);
                return;
            }

            EnsureEventSystem();
            var canvasObject = CreateObject("World Map Canvas", transform,
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Render the map above the regular player HUD, while leaving service popups above the map.
            canvas.sortingOrder = 5001;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            mapRoot = CreatePanel("World Map Screen", canvasObject.transform, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, Color.white).gameObject;
            mapRoot.GetComponent<Image>().color = Color.white;
            mapRoot.GetComponent<Image>().raycastTarget = false;

            // The viewport clips an oversized map canvas, leaving room to drag in either direction.
            var viewportObject = CreateObject("World Map Pan Viewport", mapRoot.transform, typeof(RectMask2D));
            mapViewport = viewportObject.GetComponent<RectTransform>();
            SetRect(mapViewport, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var contentObject = CreateObject("World Map Content", viewportObject.transform);
            mapContent = contentObject.GetComponent<RectTransform>();
            SetRect(mapContent, Vector2.zero, Vector2.one, new Vector2(-192f, -108f), new Vector2(192f, 108f));

            // Transparent raycast surface makes empty map space draggable.
            var panSurfaceObject = CreateObject("Map Pan Surface", mapContent, typeof(Image));
            SetRect(panSurfaceObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var panSurface = panSurfaceObject.GetComponent<Image>();
            panSurface.color = new Color(1f, 1f, 1f, 0f);
            panSurface.raycastTarget = true;

            // Layered Canvas art lets designers replace or reposition each backdrop layer.
            CreateFullScreenImage("Background - Nebula", mapContent, "bg_layer0_base.png", Color.white);
            CreateFullScreenImage("Background - Violet Mystic Fog", mapContent, "bg_layer1_fog_a.png", new Color(1f, 1f, 1f, 0.72f));
            CreateFullScreenImage("Background - Teal Mystic Fog", mapContent, "bg_layer1_fog_b.png", new Color(1f, 1f, 1f, 0.72f));

            var routeRoot = CreateObject("Dungeon Routes", mapContent);
            SetRect(routeRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var crypt = new Vector2(0.15f, 0.60f);
            var spider = new Vector2(0.36f, 0.35f);
            var demon = new Vector2(0.57f, 0.55f);
            var moon = new Vector2(0.77f, 0.32f);
            var eye = new Vector2(0.89f, 0.70f);
            var sanctuary = new Vector2(0.48f, 0.52f);
            RectTransform sanctuaryMarker = AddSanctuary(mapContent, sanctuary);
            CreateSpriteImage("World Map Title Frame", mapRoot.transform, new Vector2(0.5f, 0.93f), new Vector2(490f, 98f), "title_frame.png");
            AddLabel(mapRoot.transform, "BẢN ĐỒ HẦM NGỤC", new Vector2(0.5f, 0.945f), 27, PaleGold, FontStyles.Bold);
            AddLabel(mapRoot.transform, "CHỌN CỔNG DỊCH CHUYỂN ĐỂ KHÁM PHÁ", new Vector2(0.5f, 0.905f), 11, Muted, FontStyles.Normal);
            backButton = CreateButton(mapRoot.transform, "←  QUAY LẠI LÀNG", new Vector2(0.10f, 0.93f), new Vector2(190f, 80f), new Color(0.08f, 0.075f, 0.15f, 0.96f), PaleGold, 15);
            ApplyButtonSprite(backButton, "back_button.png", new Vector2(190f, 80f));
            statusText = AddLabel(mapRoot.transform, "Chọn một hầm ngục để xem thông tin", new Vector2(0.5f, 0.055f), 14, Muted, FontStyles.Italic);

            nodes = new List<WorldMapNode>
            {
                CreateDungeonNode(mapContent, "crypt", "HẦM MỘ VONG HỒN", "Nơi linh hồn kẻ chết không được yên nghỉ.", crypt, 1, "dungeon1_crypt_node_unlocked.png"),
                CreateDungeonNode(mapContent, "spider", "SÀO HUYỆT NHỆN U MINH", "Tơ độc giăng kín, vạn mắt dõi theo trong bóng tối.", spider, 8, "dungeon2_spider_node_unlocked.png"),
                CreateDungeonNode(mapContent, "demon", "NGỤC HẮC VIÊM", "Ngọn lửa địa ngục thiêu rụi mọi hy vọng.", demon, 15, "dungeon3_demon_node_unlocked.png"),
                CreateDungeonNode(mapContent, "moon", "ĐIỆN HUYẾT NGUYỆT", "Tòa lâu đài cổ chìm trong ánh trăng máu.", moon, 22, "dungeon4_moon_node_unlocked.png"),
                CreateDungeonNode(mapContent, "eye", "VỰC MA NHÃN HƯ VÔ", "Con mắt của bóng tối vĩnh hằng đang thức tỉnh.", eye, 30, "dungeon5_eye_node_unlocked.png")
            };

            AddRoute(routeRoot.transform, "Crypt to Sanctuary", nodes[0].GetComponent<RectTransform>(), sanctuaryMarker, MapPathLine.PathState.Unlocked);
            AddRoute(routeRoot.transform, "Sanctuary to Spider", sanctuaryMarker, nodes[1].GetComponent<RectTransform>(), MapPathLine.PathState.Locked);
            AddRoute(routeRoot.transform, "Sanctuary to Demon", sanctuaryMarker, nodes[2].GetComponent<RectTransform>(), MapPathLine.PathState.Locked);
            AddRoute(routeRoot.transform, "Demon to Moon", nodes[2].GetComponent<RectTransform>(), nodes[3].GetComponent<RectTransform>(), MapPathLine.PathState.Locked);
            AddRoute(routeRoot.transform, "Moon to Eye", nodes[3].GetComponent<RectTransform>(), nodes[4].GetComponent<RectTransform>(), MapPathLine.PathState.Locked);

            CreateFullScreenImage("Screen Vignette", mapRoot.transform, "overlay_vignette.png", Color.white);
            BuildPopup(mapRoot.transform);
            mapRoot.SetActive(false);
            BindButtons();
        }

        private void CreateFullScreenImage(string name, Transform parent, string spriteName, Color tint)
        {
            var image = CreateSpriteImage(name, parent, new Vector2(0.5f, 0.5f), new Vector2(1280f, 720f), spriteName);
            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            image.color = tint;
            image.raycastTarget = false;
        }

        private WorldMapNode CreateDungeonNode(Transform parent, string id, string title, string description,
            Vector2 position, int requiredLevel, string unlockedSpriteName)
        {
            bool unlocked = requiredLevel <= 1;
            var nodeObject = CreateObject("Dungeon - " + id, parent);
            SetRect(nodeObject.GetComponent<RectTransform>(), position, position, new Vector2(-62f, -62f), new Vector2(62f, 62f));
            var frame = nodeObject.AddComponent<WorldMapCircleGraphic>();
            frame.color = Color.white;
            frame.raycastTarget = true;
            var button = nodeObject.AddComponent<Button>();
            button.targetGraphic = frame;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.88f, 0.56f, 1f);
            colors.pressedColor = new Color(0.82f, 0.67f, 0.39f, 1f);
            button.colors = colors;

            var plate = CreateSpriteImage("Name Plate - " + id, parent, new Vector2(position.x, position.y - 0.105f), new Vector2(300f, 52f), "label_plate_name.png");
            plate.raycastTarget = false;
            var label = AddLabel(parent, title, new Vector2(position.x, position.y - 0.105f), 15,
                unlocked ? PaleGold : new Color(0.69f, 0.68f, 0.72f, 1f), FontStyles.Bold);
            label.rectTransform.sizeDelta = new Vector2(290f, 42f);
            label.outlineWidth = 0.18f;
            label.outlineColor = Color.black;
            TMP_Text lockLabel = null;
            if (!unlocked)
            {
                lockLabel = AddLabel(parent, "HOÀN THÀNH MÀN TRƯỚC", new Vector2(position.x, position.y - 0.158f), 11,
                    new Color(1f, 0.65f, 0.58f, 1f), FontStyles.Bold);
            }

            var node = nodeObject.AddComponent<WorldMapNode>();
            string targetScene = id == "crypt" ? "Level01_Crypt" : string.Empty;
            node.Configure(id, title, description, unlocked, targetScene, button, null, null, label, lockLabel, frame, null);
            node.SetFrameSprites(LoadMapSprite(unlockedSpriteName), LoadMapSprite("node_locked_with_lock.png"));
            return node;
        }

        private void BuildPopup(Transform parent)
        {
            detailPopup = CreatePanel("Dungeon Detail Popup", parent, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, new Color(0f, 0f, 0.04f, 0.64f)).gameObject;
            var card = CreatePanel("Popup Card", detailPopup.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-284f, -211f), new Vector2(284f, 211f), Color.white);
            SetGraphicSprite(card.GetComponent<Image>(), "popup_frame.png");
            var cardFace = CreatePanel("Card Face", card.transform, Vector2.zero, Vector2.one, new Vector2(12f, 12f), new Vector2(-12f, -12f), new Color(0.10f, 0.09f, 0.15f, 1f));
            cardFace.gameObject.SetActive(false);
            CreateSpriteImage("Popup Title Plate", card.transform, new Vector2(0.5f, 0.85f), new Vector2(410f, 122f), "popup_title_plate.png");
            detailTitle = AddText(card.transform, "HẦM NGỤC", 23, PaleGold, FontStyles.Bold, new Vector2(0.08f, 0.76f), new Vector2(0.92f, 0.94f));
            detailTitle.name = "Dungeon Title";
            detailDescription = AddText(card.transform, string.Empty, 16, new Color(0.86f, 0.83f, 0.80f, 1f), FontStyles.Normal, new Vector2(0.10f, 0.50f), new Vector2(0.90f, 0.79f));
            detailDescription.name = "Dungeon Description";
            detailDescription.alignment = TextAlignmentOptions.TopLeft;
            lockedMessage = AddText(card.transform, "KHU VỰC NÀY ĐANG BỊ PHONG ẤN", 13, new Color(0.91f, 0.46f, 0.43f, 1f), FontStyles.Bold, new Vector2(0.08f, 0.31f), new Vector2(0.92f, 0.41f));
            lockedMessage.name = "Lock Message";
            fightButton = CreateButton(card.transform, "CHIẾN ĐẤU", new Vector2(0.5f, 0.26f), new Vector2(190f, 84f), new Color(0.36f, 0.19f, 0.12f, 1f), PaleGold, 18);
            ApplyButtonSprite(fightButton, "button_fight.png", new Vector2(190f, 84f));
            fightButton.name = "Fight Button";
            closeDetailsButton = CreateButton(card.transform, "ĐÓNG", new Vector2(0.5f, 0.07f), new Vector2(130f, 60f), new Color(0.19f, 0.17f, 0.22f, 1f), Muted, 14);
            ApplyButtonSprite(closeDetailsButton, "button_close.png", new Vector2(130f, 60f));
            closeDetailsButton.name = "Close Details Button";
            detailPopup.SetActive(false);
        }

        private WorldMapNode CreateNode(Transform parent, string id, string title, string description, Vector2 position, bool unlocked)
        {
            var nodeObject = CreateObject("Node - " + title, parent);
            var rect = nodeObject.GetComponent<RectTransform>();
            SetRect(rect, position, position, new Vector2(-37f, -37f), new Vector2(37f, 37f));
            var frame = nodeObject.AddComponent<WorldMapCircleGraphic>();
            frame.color = Color.white;
            SetGraphicSprite(frame, unlocked ? "node_unlocked.png" : GetLockedNodeSprite(id));
            var button = nodeObject.AddComponent<Button>();
            button.targetGraphic = frame;
            var buttonColors = button.colors;
            buttonColors.normalColor = Color.white;
            buttonColors.highlightedColor = PaleGold;
            buttonColors.pressedColor = Gold;
            button.colors = buttonColors;

            var coreObject = CreateObject("Icon Placeholder Frame", nodeObject.transform);
            var coreRect = coreObject.GetComponent<RectTransform>();
            SetRect(coreRect, Vector2.zero, Vector2.one, new Vector2(5f, 5f), new Vector2(-5f, -5f));
            var coreGraphic = coreObject.AddComponent<WorldMapCircleGraphic>();
            coreGraphic.color = new Color(0.13f, 0.12f, 0.17f, 1f);
            SetGraphicSprite(coreGraphic, "WorldMapCircle.png");
            var iconImageObject = CreateObject("Dungeon Art Placeholder", nodeObject.transform, typeof(Image));
            SetRect(iconImageObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(12f, 12f), new Vector2(-12f, -12f));
            var iconImage = iconImageObject.GetComponent<Image>();
            iconImage.color = Color.white;
            iconImage.raycastTarget = false;
            CreateSpriteImage("Dungeon Name Plate - " + id, parent, new Vector2(position.x, position.y - 0.075f), new Vector2(230f, 84f), "label_plate_name.png");
            var titleLabel = AddLabel(parent, title, new Vector2(position.x, position.y - 0.075f), 16,
                unlocked ? PaleGold : new Color(0.63f, 0.62f, 0.65f, 1f), FontStyles.Bold);
            titleLabel.outlineWidth = 0.18f;
            titleLabel.outlineColor = Color.black;
            TMP_Text lockLabel = null;
            if (!unlocked)
            {
                CreateSpriteImage("Dungeon Lock Plate - " + id, parent, new Vector2(position.x, position.y - 0.15f), new Vector2(140f, 70f), "label_plate_lock.png");
                lockLabel = AddLabel(parent, "BỊ PHONG ẤN", new Vector2(position.x, position.y - 0.15f), 12,
                    new Color(0.80f, 0.43f, 0.43f, 1f), FontStyles.Normal);
            }

            var node = nodeObject.AddComponent<WorldMapNode>();
            node.Configure(id, title, description, unlocked, string.Empty, button, iconImage, null, titleLabel, lockLabel, frame, coreObject.GetComponent<WorldMapCircleGraphic>());
            node.SetFrameSprites(LoadMapSprite("node_unlocked.png"), LoadMapSprite(GetLockedNodeSprite(id)));

            return node;
        }

        private RectTransform AddSanctuary(Transform parent, Vector2 position)
        {
            var marker = CreateObject("Sanctuary - World Tree", parent);
            SetRect(marker.GetComponent<RectTransform>(), position, position, new Vector2(-53f, -53f), new Vector2(53f, 53f));
            var glow = CreateSpriteImage("Sanctuary Glow", marker.transform, Vector2.zero, new Vector2(150f, 150f), "decor_region_glow_green.png");
            glow.color = new Color(1f, 1f, 1f, 0.75f);
            var emblem = CreateSpriteImage("World Tree Emblem", marker.transform, Vector2.zero, new Vector2(94f, 94f), "icon_tree.png");
            emblem.raycastTarget = false;
            var label = AddLabel(parent, "C\u00c2Y C\u1ed4 TH\u1ee4 TH\u1ea6N", new Vector2(position.x, position.y - 0.084f), 15, PaleGold, FontStyles.Bold);
            label.outlineWidth = 0.18f;
            label.outlineColor = Color.black;
            return marker.GetComponent<RectTransform>();
        }

        private void AddRoute(Transform parent, string routeName, RectTransform from, RectTransform to, MapPathLine.PathState state)
        {
            var routeObject = CreateObject("Path - " + routeName, parent);
            SetRect(routeObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var path = routeObject.AddComponent<MapPathLine>();
            path.Configure(from, to, state,
                LoadMapSprite("line_dash_locked.png"),
                LoadMapSprite("line_dash_unlocked.png"),
                LoadMapSprite("line_dash_cleared.png"),
                LoadMapSprite("line_knot_locked.png"),
                LoadMapSprite("line_knot_unlocked.png"),
                LoadMapSprite("line_knot_cleared.png"),
                LoadMapSprite("WorldMapWhite.png"),
                LoadMapSprite("line_midlock_plate.png"),
                LoadMapSprite("line_glow_unlocked.png"),
                LoadMapSprite("line_glow_cleared.png"),
                LoadMapSprite("line_spark_gold.png"),
                LoadMapSprite("line_spark_green.png"));
        }
        private static void AddAmbientMarks(Transform parent)
        {
            var random = new System.Random(371);
            string[] glyphs = { ".", "*", "+", "o", "^" };
            Color[] colors = { new Color(0.55f, 0.80f, 0.38f, 0.55f), new Color(0.89f, 0.82f, 0.63f, 0.60f), new Color(0.45f, 0.55f, 0.72f, 0.55f) };
            for (int i = 0; i < 82; i++)
                AddLabel(parent, glyphs[random.Next(glyphs.Length)], new Vector2((float)random.NextDouble(), (float)random.NextDouble()),
                    random.Next(9, 20), colors[random.Next(colors.Length)], FontStyles.Bold).raycastTarget = false;
        }

        private static RectTransform CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var rect = CreateObject(name, parent).GetComponent<RectTransform>();
            SetRect(rect, anchorMin, anchorMax, offsetMin, offsetMax);
            rect.gameObject.AddComponent<WorldMapSolidGraphic>().color = color;
            SetGraphicSprite(rect.gameObject.GetComponent<WorldMapSolidGraphic>(), "WorldMapWhite.png");
            return rect;
        }

        private static Image CreateSpriteImage(string name, Transform parent, Vector2 anchor, Vector2 size, string spriteName)
        {
            var obj = CreateObject(name, parent, typeof(Image));
            SetRect(obj.GetComponent<RectTransform>(), anchor, anchor, size * -0.5f, size * 0.5f);
            var image = obj.GetComponent<Image>();
            image.raycastTarget = false;
            SetGraphicSprite(image, spriteName);
            return image;
        }

        private static string GetLockedNodeSprite(string id)
        {
            if (id == "mana_mine") return "node_locked_1.png";
            if (id == "soul_swamp") return "node_locked_2.png";
            return "node_locked_3.png";
        }

        private static void ApplyButtonSprite(Button button, string spriteName, Vector2 size)
        {
            var image = button.targetGraphic as Image;
            if (image != null)
            {
                SetGraphicSprite(image, spriteName);
                image.color = Color.white;
            }
            var rect = button.transform as RectTransform;
            if (rect != null)
                rect.sizeDelta = size;
        }

        private static WorldMapCircleGraphic AddCircle(string name, Transform parent, Vector2 anchor, Vector2 size, Color color)
        {
            var obj = CreateObject(name, parent);
            SetRect(obj.GetComponent<RectTransform>(), anchor, anchor, size * -0.5f, size * 0.5f);
            var graphic = obj.AddComponent<WorldMapCircleGraphic>().AlsoSetColor(color);
            SetGraphicSprite(graphic, name.StartsWith("Mystic Glow") ? "WorldMapGlow.png" : "WorldMapCircle.png");
            return graphic;
        }

        private static Button CreateButton(Transform parent, string text, Vector2 anchor, Vector2 size, Color background, Color foreground, int fontSize)
        {
            var obj = CreateObject(text, parent);
            SetRect(obj.GetComponent<RectTransform>(), anchor, anchor, size * -0.5f, size * 0.5f);
            var plate = obj.AddComponent<WorldMapSolidGraphic>();
            plate.color = background;
            SetGraphicSprite(plate, "WorldMapWhite.png");
            var button = obj.AddComponent<Button>();
            button.targetGraphic = plate;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.91f, 0.68f, 1f);
            colors.pressedColor = new Color(0.82f, 0.67f, 0.39f, 1f);
            button.colors = colors;
            AddText(obj.transform, text, fontSize, foreground, FontStyles.Bold, Vector2.zero, Vector2.one);
            return button;
        }

        private static TMP_Text AddLabel(Transform parent, string text, Vector2 anchor, int fontSize, Color color, FontStyles style)
        {
            var obj = CreateObject(text, parent);
            SetRect(obj.GetComponent<RectTransform>(), anchor, anchor, new Vector2(-220f, -22f), new Vector2(220f, 22f));
            var label = obj.AddComponent<TextMeshProUGUI>();
            ConfigureText(label, text, fontSize, color, style);
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.raycastTarget = false;
            return label;
        }

        private static TMP_Text AddText(Transform parent, string text, int fontSize, Color color, FontStyles style, Vector2 anchorMin, Vector2 anchorMax)
        {
            var obj = CreateObject(text, parent);
            SetRect(obj.GetComponent<RectTransform>(), anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            var label = obj.AddComponent<TextMeshProUGUI>();
            ConfigureText(label, text, fontSize, color, style);
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.raycastTarget = false;
            return label;
        }

        private static void ConfigureText(TMP_Text text, string value, int fontSize, Color color, FontStyles style)
        {
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.fontStyle = style;
        }

        private static void SetGraphicSprite(Image image, string fileName)
        {
#if UNITY_EDITOR
            image.sprite = LoadMapSprite(fileName);
#endif
            image.type = Image.Type.Simple;
        }

        private static Sprite LoadMapSprite(string fileName)
        {
#if UNITY_EDITOR
            var guids = UnityEditor.AssetDatabase.FindAssets(System.IO.Path.GetFileNameWithoutExtension(fileName) + " t:Sprite", new[] { "Assets/Game/UI/WorldMap/WorldMapAssets" });
            if (guids.Length > 0)
                return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/UI/WorldMap/" + fileName);
#else
            return null;
#endif
        }

        private static GameObject CreateObject(string name, Transform parent, params System.Type[] components)
        {
            var gameObject = components.Length == 0
                ? new GameObject(name, typeof(RectTransform))
                : new GameObject(name, AddRectTransform(components));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static System.Type[] AddRectTransform(System.Type[] components)
        {
            var result = new System.Type[components.Length + 1];
            result[0] = typeof(RectTransform);
            for (int i = 0; i < components.Length; i++)
                result[i + 1] = components[i];
            return result;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }

    internal static class WorldMapGraphicExtensions
    {
        public static WorldMapCircleGraphic AlsoSetColor(this WorldMapCircleGraphic graphic, Color color)
        {
            graphic.color = color;
            return graphic;
        }
    }
}

