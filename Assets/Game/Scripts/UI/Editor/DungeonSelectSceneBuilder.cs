using System.IO;
using DungeonMerchant.UI;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DungeonMerchant.Editor
{
    [InitializeOnLoad]
    public static class DungeonSelectSceneBuilder
    {
        private const string ScenePath = "Assets/Screen/Game/Asset/Demo1.unity";
        private static bool buildCheckScheduled;
        private static readonly Color Ink = Hex("20251C");
        private static readonly Color Panel = Hex("414536");
        private static readonly Color Trim = Hex("A18A55");
        private static readonly Color Paper = Hex("E7D9AC");
        private static readonly Color Muted = Hex("AAA27E");

        static DungeonSelectSceneBuilder()
        {
            ScheduleBuildCheck();
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            ScheduleBuildCheck();
        }

        private static void ScheduleBuildCheck()
        {
            if (buildCheckScheduled)
                return;
            buildCheckScheduled = true;
            EditorApplication.delayCall += BuildIfMissing;
        }

        [MenuItem("Dungeon Merchant/UI/Build Demo1 Screen")]
        public static void BuildFromMenu()
        {
            BuildScene(true);
        }

        private static void BuildIfMissing()
        {
            buildCheckScheduled = false;
            if (EditorApplication.isCompiling)
            {
                ScheduleBuildCheck();
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                return;
            }

            try
            {
                if (!File.Exists(ScenePath))
                    BuildScene(false);
                else
                    EnsureCameraInExistingScene();
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
                return;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            ScheduleBuildCheck();
        }

        private static void BuildScene(bool replace)
        {
            if (File.Exists(ScenePath) && !replace)
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            scene.name = "DungeonSelect";

            var cameraObject = new GameObject("Main Camera", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0, 0, -10);
            var sceneCamera = cameraObject.GetComponent<Camera>();
            sceneCamera.orthographic = true;
            sceneCamera.orthographicSize = 5;
            sceneCamera.clearFlags = CameraClearFlags.SolidColor;
            sceneCamera.backgroundColor = Hex("343923");

            var canvasObject = new GameObject("Dungeon Select UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;

            var eventObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            SceneManager.MoveGameObjectToScene(eventObject, scene);

            var root = canvasObject.transform;
            MakePanel(root, "Olive pixel backdrop", Vector2.zero, new Vector2(1280, 720), Hex("343923"));
            MakePanel(root, "Top frame", new Vector2(0, 316), new Vector2(1192, 68), Ink);
            MakePanel(root, "Top trim", new Vector2(0, 281), new Vector2(1192, 2), Trim);
            MakeText(root, "Title", "CHỌN HẦM NGỤC", new Vector2(0, 322), new Vector2(760, 38), 30, Paper, FontStyle.Bold, TextAnchor.MiddleCenter);
            MakeText(root, "Subtitle", "CHỌN CỔNG  •  CHUẨN BỊ TRƯỚC KHI XUẤT PHÁT", new Vector2(0, 292), new Vector2(760, 22), 13, Muted, FontStyle.Bold, TextAnchor.MiddleCenter);

            var controllerObject = new GameObject("Dungeon Select Controller");
            SceneManager.MoveGameObjectToScene(controllerObject, scene);
            var controller = controllerObject.AddComponent<DungeonSelectScreen>();

            var colors = new[] { Hex("659B9A"), Hex("65834A"), Hex("D26E36"), Hex("9A6AD1") };
            var names = new[] { "HẦM 1  •  GOLEM", "HẦM 2  •  RỪNG", "HẦM 3  •  LỬA", "HẦM 4  •  CÔNG NGHỆ" };
            var subtitles = new[] { "Đền đá vỡ", "Rễ sâu", "Biển cát đỏ", "Lõi neon" };
            var bosses = new[] { "Golem Cổ", "Mẫu Thể Rễ", "Vua Lò", "CORE-04" };
            var rewards = new[] { "Lõi Golem", "Hạt Cổ", "Tim Lò", "Lõi Neon" };
            var difficulties = new[] { "★☆☆☆☆", "★★☆☆☆", "★★★☆☆", "★★★★☆" };
            var statuses = new[] { "ĐANG MỞ", "ĐANG KHÓA", "ĐANG KHÓA", "ĐANG KHÓA" };
            var unlocked = new[] { true, false, false, false };
            var xPositions = new[] { -450f, -150f, 150f, 450f };

            for (var i = 0; i < names.Length; i++)
            {
                var card = MakeButton(root, "Gate Card " + (i + 1), new Vector2(xPositions[i], 16), new Vector2(278, 396), Panel, colors[i]);
                MakePanel(card.transform, "Card inner", Vector2.zero, new Vector2(262, 380), Hex("34382D"));
                MakeText(card.transform, "Dungeon name", names[i], new Vector2(0, 158), new Vector2(250, 34), 16, Paper, FontStyle.Bold, TextAnchor.MiddleCenter);
                MakePortalGlyph(card.transform, colors[i], unlocked[i]);
                MakeText(card.transform, "Location", subtitles[i], new Vector2(0, -111), new Vector2(246, 24), 14, Paper, FontStyle.Bold, TextAnchor.MiddleCenter);
                MakeText(card.transform, "Difficulty", difficulties[i], new Vector2(0, -143), new Vector2(246, 22), 14, colors[i], FontStyle.Bold, TextAnchor.MiddleCenter);
                MakeText(card.transform, "Gate status", unlocked[i] ? "✦  " + statuses[i] : "▣  " + statuses[i], new Vector2(0, -174), new Vector2(246, 22), 12, unlocked[i] ? colors[i] : Muted, FontStyle.Bold, TextAnchor.MiddleCenter);

                controller.AddDungeon(new DungeonSelectScreen.DungeonEntry
                {
                    title = names[i], subtitle = subtitles[i], difficulty = difficulties[i], reward = rewards[i],
                    boss = bosses[i], status = statuses[i], unlocked = unlocked[i], button = card
                });
            }

            MakePanel(root, "Detail panel", new Vector2(0, -252), new Vector2(1192, 96), Ink);
            MakePanel(root, "Detail top trim", new Vector2(0, -204), new Vector2(1192, 2), Trim);
            var detailTitle = MakeText(root, "Selected dungeon", "", new Vector2(-348, -228), new Vector2(440, 26), 16, Paper, FontStyle.Bold, TextAnchor.MiddleLeft);
            var detailBody = MakeText(root, "Dungeon details", "", new Vector2(-348, -262), new Vector2(710, 28), 13, Muted, FontStyle.Normal, TextAnchor.MiddleLeft);
            var enterButton = MakeButton(root, "Enter Dungeon", new Vector2(462, -251), new Vector2(216, 54), Hex("566E42"), Trim);
            var action = MakeText(enterButton.transform, "Enter label", "[ E ]  VÀO HẦM", Vector2.zero, new Vector2(206, 44), 16, Paper, FontStyle.Bold, TextAnchor.MiddleCenter);
            enterButton.onClick.AddListener(controller.ConfirmSelected);
            controller.SetDetails(detailTitle, detailBody, action);
            controller.Initialize();

            MakeText(root, "Footer controls", "WASD / ←→  CHỌN CỔNG      E  VÀO HẦM      ESC  QUAY LẠI HUB", new Vector2(0, -326), new Vector2(1000, 22), 12, Muted, FontStyle.Bold, TextAnchor.MiddleCenter);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.Refresh();
            Debug.Log("Dungeon Select screen created at " + ScenePath);
        }

        private static void EnsureCameraInExistingScene()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            var openedByBuilder = !scene.IsValid() || !scene.isLoaded;
            if (openedByBuilder)
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

            var hasCamera = false;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.GetComponent<Camera>() != null)
                {
                    hasCamera = true;
                    break;
                }
            }

            if (!hasCamera)
            {
                var cameraObject = new GameObject("Main Camera", typeof(Camera));
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0, 0, -10);
                var camera = cameraObject.GetComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Hex("343923");
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            if (openedByBuilder)
                EditorSceneManager.CloseScene(scene, true);
        }

        private static Button MakeButton(Transform parent, string name, Vector2 position, Vector2 size, Color fill, Color border)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), position, size);
            var image = go.GetComponent<Image>();
            image.color = border;
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock { normalColor = border, highlightedColor = Hex("D7C582"), pressedColor = Hex("887747"), selectedColor = border, disabledColor = Hex("575748"), colorMultiplier = 1, fadeDuration = 0.08f };
            MakePanel(go.transform, "Button fill", Vector2.zero, size - new Vector2(4, 4), fill);
            return button;
        }

        private static GameObject MakePanel(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), position, size);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return go;
        }

        private static Text MakeText(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize, Color color, FontStyle style, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), position, size);
            var text = go.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static void MakePortalGlyph(Transform parent, Color color, bool isOpen)
        {
            var glyph = new GameObject("Pixel portal", typeof(RectTransform));
            glyph.transform.SetParent(parent, false);
            Place(glyph.GetComponent<RectTransform>(), new Vector2(0, 30), new Vector2(100, 160));
            var dim = isOpen ? color : Hex("696957");
            MakePanel(glyph.transform, "Portal outer", Vector2.zero, new Vector2(72, 130), Ink);
            MakePanel(glyph.transform, "Portal rim", Vector2.zero, new Vector2(60, 118), dim);
            MakePanel(glyph.transform, "Portal shadow", new Vector2(0, -5), new Vector2(44, 94), Hex("26291F"));
            MakePanel(glyph.transform, "Portal light", new Vector2(0, 2), new Vector2(28, 70), isOpen ? Color.Lerp(color, Paper, 0.18f) : Hex("555548"));
            MakePanel(glyph.transform, "Portal step", new Vector2(0, -68), new Vector2(96, 12), Trim);
            MakePanel(glyph.transform, "Pixel spark left", new Vector2(-40, 55), new Vector2(8, 8), dim);
            MakePanel(glyph.transform, "Pixel spark right", new Vector2(39, 26), new Vector2(6, 6), dim);
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value, out var color);
            return color;
        }
    }
}
