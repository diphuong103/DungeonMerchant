using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cainos.PixelArtTopDown_Basic
{
    [DisallowMultipleComponent]
    public sealed class SteleServicePopupUI : MonoBehaviour
    {
        enum Service { Combat, Quest, Trade, Magic, Forge, Memorial }
        public static SteleServicePopupUI Instance { get; private set; }
        [SerializeField] string[] spriteNames;
        [SerializeField] Sprite[] kitSprites;
        [SerializeField] GameObject root;
        [SerializeField] RectTransform frame;
        [SerializeField] RectTransform body;
        [SerializeField] TMP_Text title;
        int selected;
        int tab;
        bool actionUsed;
        bool magicHandlersBound;
        bool tradeHandlersBound;
        bool questHandlersBound;
        bool combatHandlersBound;
        int combatDemoPoints = 1250;
        readonly int[] combatLevels = { 15, 15, 15, 15, 15 };
        Service service;
        string serviceTitle;
        static readonly Color Gold = new Color(1f, .76f, .33f);

        public bool IsOpen { get { return root != null && root.activeSelf; } }

        void Awake() { Instance = this; BuildShell(); root.SetActive(false); }
        void OnDestroy() { if (Instance == this) Instance = null; }
        void Update() { if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) Close(); }
        public void Close() { if (root != null) root.SetActive(false); }

        public void OpenFor(string value)
        {
            serviceTitle = string.IsNullOrEmpty(value) ? "BIA TƯƠNG TÁC" : value;
            string text = serviceTitle.ToUpperInvariant();
            service = text.Contains("LÒ RÈN") ? Service.Forge :
                text.Contains("GIAO THƯƠNG") ? Service.Trade :
                text.Contains("NHIỆM VỤ") ? Service.Quest :
                text.Contains("PHÉP THUẬT") ? Service.Magic :
                text.Contains("CHIẾN LƯỢC") || text.Contains("CHIẾN ĐẤU") ? Service.Combat : Service.Memorial;
            if (service == Service.Combat) serviceTitle = "BIA CHIEN DAU";
            else if (service == Service.Magic) serviceTitle = "BIA PHÙ THỦY";
            else if (service == Service.Forge) serviceTitle = "LÒ RÈN - NÂNG CẤP VŨ KHÍ";
            else if (service == Service.Trade) serviceTitle = "BIA GIAO THƯƠNG";
            selected = 0; tab = service == Service.Trade ? 2 : 0; actionUsed = false;
            title.text = serviceTitle;
            Rebuild();
            if (service == Service.Forge && body.Find("Forge Inventory") != null) BindForgeLayout();
            if (service == Service.Magic && body.Find("Magic Layout") != null) BindMagicLayout();
            if (service == Service.Trade && body.Find("Trade Layout") != null) BindTradeLayout();
            if (service == Service.Quest && body.Find("Quest Layout") != null) BindQuestLayout();
            if (service == Service.Combat && body.Find("Combat Layout") != null) BindCombatLayout();
            root.SetActive(true);
        }

        [ContextMenu("Create Editable Forge Popup Canvas")]
        public void CreateEditableForgeCanvas()
        {
            BuildShell();
            service = Service.Forge;
            serviceTitle = "LÒ RÈN - NÂNG CẤP VŨ KHÍ";
            title.text = serviceTitle;
            selected = 0; actionUsed = false;
            if (body.Find("Forge Inventory") == null) Rebuild();
            root.SetActive(false);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        }

        [ContextMenu("Create Editable Wizard Popup Layout")]
        public void CreateEditableWizardLayout()
        {
            BuildShell();
            service = Service.Magic;
            serviceTitle = "BIA PHÙ THỦY";
            title.text = serviceTitle;
            selected = 0; tab = service == Service.Trade ? 2 : 0; actionUsed = false;
            if (body.Find("Magic Layout") == null) Magic();
            SetStaticLayoutVisibility(Service.Magic);
            root.SetActive(true);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        }

        [ContextMenu("Create Editable Trade Popup Layout")]
        public void CreateEditableTradeLayout()
        {
            BuildShell();
            service = Service.Trade;
            serviceTitle = "BIA GIAO THƯƠNG";
            title.text = serviceTitle;
            selected = 0; tab = 2; actionUsed = false;
            if (body.Find("Trade Layout") == null) Trade();
            SetStaticLayoutVisibility(Service.Trade);
            UpdateTradeLayout();
            root.SetActive(true);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        }

        [ContextMenu("Create Editable Quest Popup Layout")]
        public void CreateEditableQuestLayout()
        {
            BuildShell();
            service = Service.Quest;
            serviceTitle = "BIA NHIỆM VỤ";
            title.text = serviceTitle;
            selected = 0; tab = 0; actionUsed = false;
            if (body.Find("Quest Layout") == null) Quest();
            SetStaticLayoutVisibility(Service.Quest);
            UpdateQuestLayout();
            root.SetActive(true);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        }

        [ContextMenu("Create Editable Combat Popup Layout")]
        public void CreateEditableCombatLayout()
        {
            BuildShell();
            service = Service.Combat;
            serviceTitle = "BIA CHIEN DAU";
            title.text = serviceTitle;
            selected = 0; actionUsed = false;
            if (body.Find("Combat Layout") == null) Combat();
            SetStaticLayoutVisibility(Service.Combat);
            UpdateCombatLayout();
            root.SetActive(true);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        }

        void BindCombatLayout()
        {
            if (combatHandlersBound) return;
            Transform close = frame.Find("Close");
            if (close != null) close.GetComponent<Button>().onClick.AddListener(Close);
            for (int i = 0; i < 5; i++)
            {
                int index = i;
                Transform button = body.Find("Combat Layout/Upgrade List/Upgrade Row " + i + "/Upgrade Action");
                if (button != null) button.GetComponent<Button>().onClick.AddListener(delegate
                {
                    if (combatDemoPoints < 250) return;
                    combatDemoPoints -= 250;
                    combatLevels[index]++;
                    UpdateCombatLayout();
                });
            }
            combatHandlersBound = true;
        }

        void UpdateCombatLayout()
        {
            Transform layout = body != null ? body.Find("Combat Layout") : null;
            if (layout == null) return;
            string[] names = { "TAN CONG", "PHONG THU", "SINH LUC", "NANG LUONG", "CHI MANG" };
            string[] units = { "sat thuong", "giap", "HP toi da", "Mana toi da", "%" };
            int[] gains = { 10, 8, 50, 20, 2 };
            Transform points = layout.Find("Points Value");
            if (points != null) points.GetComponent<TMP_Text>().text = combatDemoPoints.ToString("N0");
            for (int i = 0; i < names.Length; i++)
            {
                Transform row = layout.Find("Upgrade List/Upgrade Row " + i);
                if (row == null) continue;
                Transform label = row.Find("Stat Name");
                if (label != null) label.GetComponent<TMP_Text>().text = names[i];
                Transform value = row.Find("Stat Value");
                if (value != null) value.GetComponent<TMP_Text>().text = "Cấp " + combatLevels[i] + "  •  +" + gains[i] + " " + units[i];
                Transform button = row.Find("Upgrade Action/Label");
                if (button != null) button.GetComponent<TMP_Text>().text = combatDemoPoints >= 250 ? "NANG CAP (250)" : "THIEU DIEM";
                Button action = row.Find("Upgrade Action") != null ? row.Find("Upgrade Action").GetComponent<Button>() : null;
                if (action != null) action.interactable = combatDemoPoints >= 250;
            }
        }

        void BindMagicLayout()
        {
            if (magicHandlersBound) return;
            Transform close = frame.Find("Close");
            if (close != null) close.GetComponent<Button>().onClick.AddListener(Close);
            for (int i = 0; i < 12; i++)
            {
                Transform spell = body.Find("Magic Layout/Spell Grid/Spell Slot " + i);
                if (spell == null || !spell.gameObject.activeSelf) continue;
                int index = i;
                spell.GetComponent<Button>().onClick.AddListener(delegate
                {
                    if (index >= 8) return;
                    selected = index;
                    UpdateMagicSelection();
                });
            }
            Transform previous = body.Find("Magic Layout/Spell Grid/Page Previous");
            Transform next = body.Find("Magic Layout/Spell Grid/Page Next");
            if (previous != null) previous.GetComponent<Button>().onClick.AddListener(delegate { UpdateMagicPage(false); });
            if (next != null) next.GetComponent<Button>().onClick.AddListener(delegate { UpdateMagicPage(true); });
            Transform upgrade = body.Find("Magic Layout/Spell Details/Upgrade Action");
            Transform learn = body.Find("Magic Layout/Spell Details/Learn Action");
            if (upgrade != null) upgrade.GetComponent<Button>().onClick.AddListener(delegate { UpdateMagicStatus("Đã xác nhận nâng cấp minh họa."); });
            if (learn != null) learn.GetComponent<Button>().onClick.AddListener(delegate { UpdateMagicStatus("Đã xác nhận học phép minh họa."); });
            magicHandlersBound = true;
        }

        void BindTradeLayout()
        {
            if (tradeHandlersBound) return;
            Transform close = frame.Find("Close");
            if (close != null) close.GetComponent<Button>().onClick.AddListener(Close);
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                Transform tradeTab = body.Find("Trade Layout/Trade Tabs/Tab " + i);
                if (tradeTab != null) tradeTab.GetComponent<Button>().onClick.AddListener(delegate
                {
                    tab = index; selected = 0; actionUsed = false; UpdateTradeLayout();
                });
            }
            for (int i = 0; i < 16; i++)
            {
                int index = i;
                Transform slot = body.Find("Trade Layout/Player Inventory/Item Slot " + i);
                if (slot != null) slot.GetComponent<Button>().onClick.AddListener(delegate
                {
                    selected = index; UpdateTradeLayout();
                });
            }
            Transform action = body.Find("Trade Layout/Trade Details/Primary Action");
            if (action != null) action.GetComponent<Button>().onClick.AddListener(delegate
            {
                actionUsed = true; UpdateTradeLayout();
            });
            tradeHandlersBound = true;
        }

        void UpdateTradeLayout()
        {
            Transform layout = body != null ? body.Find("Trade Layout") : null;
            if (layout == null) return;
            string[] labels = { "MUA", "BÁN", "TRAO ĐỔI" };
            for (int i = 0; i < labels.Length; i++)
            {
                Transform button = layout.Find("Trade Tabs/Tab " + i);
                if (button == null) continue;
                button.GetComponent<Image>().sprite = SpriteFor(i == tab ? "tab_active" : "tab_inactive");
                Transform text = button.Find("Label");
                if (text != null) text.GetComponent<TMP_Text>().text = labels[i];
            }
            Transform inventoryTitle = layout.Find("Player Inventory/Inventory Heading");
            if (inventoryTitle != null)
                inventoryTitle.GetComponent<TMP_Text>().text = tab == 0 ? "VẬT PHẨM ĐANG BÁN" : "HÒM ĐỒ CỦA BẠN";
            for (int i = 0; i < 16; i++)
            {
                Transform slot = layout.Find("Player Inventory/Item Slot " + i);
                if (slot != null) slot.GetComponent<Image>().sprite = SpriteFor(i == selected ? "slot_selected" : "slot_empty");
            }
            Transform heading = layout.Find("Trade Details/Mode Heading");
            if (heading != null) heading.GetComponent<TMP_Text>().text = tab == 0 ? "CHI TIẾT VẬT PHẨM" : tab == 1 ? "BÁN VẬT PHẨM" : "CHI TIẾT TRAO ĐỔI";
            Transform action = layout.Find("Trade Details/Primary Action");
            if (action != null)
            {
                Transform label = action.Find("Label");
                if (label != null) label.GetComponent<TMP_Text>().text = actionUsed ? "ĐÃ XÁC NHẬN" : tab == 0 ? "MUA VẬT PHẨM" : tab == 1 ? "BÁN VẬT PHẨM" : "ĐĂNG TIN TRAO ĐỔI";
            }
            Transform status = layout.Find("Trade Details/Trade Status");
            if (status != null) status.GetComponent<TMP_Text>().text = actionUsed ? "Thao tác minh họa đã được xác nhận." : "Chưa chọn vật phẩm. Dữ liệu giao dịch là minh họa.";
        }

        void BindQuestLayout()
        {
            if (questHandlersBound) return;
            Transform close = frame.Find("Close");
            if (close != null) close.GetComponent<Button>().onClick.AddListener(Close);
            for (int i = 0; i < 2; i++)
            {
                int index = i;
                Transform questTab = body.Find("Quest Layout/Quest Tabs/Tab " + i);
                if (questTab != null) questTab.GetComponent<Button>().onClick.AddListener(delegate
                {
                    tab = index; selected = 0; actionUsed = false; UpdateQuestLayout();
                });
            }
            for (int i = 0; i < 5; i++)
            {
                int index = i;
                Transform row = body.Find("Quest Layout/Quest List/Quest Row " + i);
                if (row != null) row.GetComponent<Button>().onClick.AddListener(delegate
                {
                    selected = index; actionUsed = false; UpdateQuestLayout();
                });
            }
            Transform action = body.Find("Quest Layout/Quest Details/Accept Action");
            if (action != null) action.GetComponent<Button>().onClick.AddListener(delegate
            {
                actionUsed = true; UpdateQuestLayout();
            });
            questHandlersBound = true;
        }

        void UpdateQuestLayout()
        {
            Transform layout = body != null ? body.Find("Quest Layout") : null;
            if (layout == null) return;
            string[][] names =
            {
                new[] { "GIẢI CỨU LÀNG", "KHÁM PHÁ HANG ĐỘNG", "TÌM DẤU VẾT", "DỌN SẠCH QUÁI VẬT", "ĐƯA HÀNG ĐẾN LÀNG" },
                new[] { "THU THẬP DƯỢC THẢO", "SĂN SÓI ĐỒNG", "TÌM MẤT TÍCH", "GIAO HÀNG", "TÌM DI VẬT CỔ" }
            };
            for (int i = 0; i < 2; i++)
            {
                Transform button = layout.Find("Quest Tabs/Tab " + i);
                if (button == null) continue;
                button.GetComponent<Image>().sprite = SpriteFor(i == tab ? "tab_active" : "tab_inactive");
                Transform label = button.Find("Label");
                if (label != null) label.GetComponent<TMP_Text>().text = i == 0 ? "NHIỆM VỤ CHÍNH" : "NHIỆM VỤ PHỤ";
            }
            for (int i = 0; i < 5; i++)
            {
                Transform row = layout.Find("Quest List/Quest Row " + i);
                if (row == null) continue;
                row.GetComponent<Image>().sprite = SpriteFor(i == selected ? "row_selected" : "row_normal");
                Transform label = row.Find("Quest Name");
                if (label != null) label.GetComponent<TMP_Text>().text = names[tab][i];
            }
            Transform details = layout.Find("Quest Details");
            string selectedName = names[tab][Mathf.Clamp(selected, 0, 4)];
            Transform name = details.Find("Quest Name");
            if (name != null) name.GetComponent<TMP_Text>().text = selectedName;
            Transform description = details.Find("Quest Description");
            if (description != null) description.GetComponent<TMP_Text>().text = tab == 0
                ? "Giặc dữ kéo đến làng, người dân đang cần được giúp đỡ. Hãy lên đường và bảo vệ quê nhà."
                : "Dân làng cần người hỗ trợ. Hoàn thành công việc để nhận phần thưởng tương xứng.";
            Transform objectives = details.Find("Quest Objectives");
            if (objectives != null) objectives.GetComponent<TMP_Text>().text = tab == 0
                ? "• Nói chuyện với trưởng làng\n• Đánh bại nhóm quái vật ở phía bắc"
                : "• Tìm vật phẩm được yêu cầu\n• Quay lại báo cáo cho người giao nhiệm vụ";
            Transform rewards = details.Find("Quest Rewards");
            if (rewards != null) rewards.GetComponent<TMP_Text>().text = "100 Vàng\n10 EXP";
            Transform action = details.Find("Accept Action");
            if (action != null)
            {
                Transform label = action.Find("Label");
                if (label != null) label.GetComponent<TMP_Text>().text = actionUsed ? "ĐÃ NHẬN NHIỆM VỤ" : "NHẬN NHIỆM VỤ";
            }
            Transform status = details.Find("Quest Status");
            if (status != null) status.GetComponent<TMP_Text>().text = actionUsed
                ? "Nhiệm vụ minh họa đã được nhận."
                : "Thông tin và phần thưởng hiện là dữ liệu minh họa.";
        }

        void UpdateMagicSelection()
        {
            Transform grid = body.Find("Magic Layout/Spell Grid");
            if (grid == null) return;
            for (int i = 0; i < 8; i++)
            {
                Transform slot = grid.Find("Spell Slot " + i);
                if (slot != null) slot.GetComponent<Image>().sprite = SpriteFor(i == selected ? "slot_selected" : "slot_empty");
            }
            Transform spellTitle = body.Find("Magic Layout/Spell Details/Spell Name");
            if (spellTitle != null) spellTitle.GetComponent<TMP_Text>().text = SpellName(selected) + " - CẤP 5";
        }

        void UpdateMagicPage(bool forward)
        {
            Transform page = body.Find("Magic Layout/Spell Grid/Page Label");
            if (page != null) page.GetComponent<TMP_Text>().text = forward ? "5: 2/2" : "5: 1/2";
            UpdateMagicStatus("Danh sách phép minh họa.");
        }

        void UpdateMagicStatus(string message)
        {
            Transform status = body.Find("Magic Layout/Spell Details/Status");
            if (status != null) status.GetComponent<TMP_Text>().text = message;
        }

        static string SpellName(int index)
        {
            string[] names = { "HỎA CẦU", "BĂNG TIỄN", "HỒ CHÂN", "ĐỘI THÁP", "HỖN PHÁP", "HỎA CẦU", "TRẬY SẤU", "CHẤT PHÚC" };
            return names[Mathf.Clamp(index, 0, names.Length - 1)];
        }

        void BindForgeLayout()
        {
            Transform close = frame.Find("Close");
            if (close != null) close.GetComponent<Button>().onClick.AddListener(Close);
            string[] weapons = { "THANH KIẾM DŨNG SĨ", "KIẾM SẮT", "RÌU CHIẾN", "KHIÊN ĐỒNG", "CUNG GỖ" };
            for (int i = 0; i < weapons.Length; i++)
            {
                int index = i;
                Transform row = body.Find("Forge Inventory/Weapon Row " + i);
                if (row == null) continue;
                row.GetComponent<Button>().onClick.AddListener(delegate
                {
                    selected = index;
                    Transform name = body.Find("Forge Details/Selected Weapon Name");
                    if (name != null) name.GetComponent<TMP_Text>().text = weapons[index];
                    for (int j = 0; j < weapons.Length; j++)
                    {
                        Transform item = body.Find("Forge Inventory/Weapon Row " + j);
                        if (item != null)
                        {
                            item.GetComponent<Image>().sprite = SpriteFor(j == index ? "row_selected" : "row_normal");
                            Transform slot = item.Find("Empty Weapon Slot");
                            if (slot != null) slot.GetComponent<Image>().sprite = SpriteFor(j == index ? "slot_selected" : "slot_empty");
                        }
                    }
                });
            }
            Transform action = body.Find("Forge Details/Action");
            if (action != null) action.GetComponent<Button>().onClick.AddListener(delegate
            {
                actionUsed = true;
                Transform status = body.Find("Forge Details/Status");
                if (status != null) status.GetComponent<TMP_Text>().text = "Đã xác nhận demo • không trừ vật phẩm.";
                Transform label = action.Find("Label");
                if (label != null) label.GetComponent<TMP_Text>().text = "ĐÃ XÁC NHẬN";
            });
        }

        void BuildShell()
        {
            if (root != null && frame != null && body != null && title != null) return;
            Transform existing = transform.Find("Stele Service Popup Canvas");
            if (existing != null)
            {
                root = existing.gameObject;
                frame = existing.Find("Popup Frame") as RectTransform;
                body = frame != null ? frame.Find("Popup Body") as RectTransform : null;
                Transform existingTitle = frame != null ? frame.Find("Title") : null;
                title = existingTitle != null ? existingTitle.GetComponent<TMP_Text>() : null;
                if (frame != null && body != null && title != null) return;
            }
            root = Rect("Stele Service Popup Canvas", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            // This is a regular panel under the existing UI - Rough Frame Canvas.
            // A nested Canvas hid the popup in Scene view and added no value here;
            // the parent Canvas and GraphicRaycaster already handle rendering/input.
            Image shade = root.AddComponent<Image>(); shade.color = new Color(.02f, .03f, .05f, .78f);
            Button shadeButton = root.AddComponent<Button>(); shadeButton.transition = Selectable.Transition.None; shadeButton.onClick.AddListener(Close);
            frame = Rect("Popup Frame", root.transform, new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero, new Vector2(1080,680));
            frame.localScale = Vector3.one * 1.28f;
            Image frameImage = frame.gameObject.AddComponent<Image>(); frameImage.type = Image.Type.Sliced;
            Button blocker = frame.gameObject.AddComponent<Button>(); blocker.transition = Selectable.Transition.None; blocker.onClick.AddListener(delegate { });
            Img("Title Crest", frame, "crest_top", new Vector2(.5f,1), new Vector2(.5f,1), new Vector2(0,-4), new Vector2(124,68));
            Img("Title Banner", frame, "title_banner", new Vector2(.5f,1), new Vector2(.5f,1), new Vector2(0,-18), new Vector2(620,86));
            title = Txt("Title", frame, "BIA", 27, Gold, new Vector2(.12f,1), new Vector2(.88f,1), new Vector2(0,-72), new Vector2(0,-24), TextAlignmentOptions.Center);
            Btn("Close", frame, "×", "btn_close_red", new Vector2(1,1), new Vector2(1,1), new Vector2(-48,-38), new Vector2(48,48), Close);
            body = Rect("Popup Body", frame, Vector2.zero, Vector2.one, new Vector2(40,38), new Vector2(-40,-112));
        }

        void Rebuild()
        {
            if (service == Service.Forge && body.Find("Forge Inventory") != null)
            {
                SetStaticLayoutVisibility(Service.Forge);
                return;
            }
            if (service == Service.Magic && body.Find("Magic Layout") != null)
            {
                SetStaticLayoutVisibility(Service.Magic);
                return;
            }
            if (service == Service.Trade && body.Find("Trade Layout") != null)
            {
                SetStaticLayoutVisibility(Service.Trade);
                UpdateTradeLayout();
                return;
            }
            if (service == Service.Quest && body.Find("Quest Layout") != null)
            {
                SetStaticLayoutVisibility(Service.Quest);
                UpdateQuestLayout();
                return;
            }
            if (service == Service.Combat && body.Find("Combat Layout") != null)
            {
                SetStaticLayoutVisibility(Service.Combat);
                UpdateCombatLayout();
                return;
            }
            for (int i=body.childCount-1;i>=0;i--) Destroy(body.GetChild(i).gameObject);
            switch(service)
            {
                case Service.Combat: Combat(); break;
                case Service.Quest: Quest(); break;
                case Service.Trade: Trade(); break;
                case Service.Magic: Magic(); break;
                case Service.Forge: Forge(); break;
                default: Memorial(); break;
            }
        }

        void SetStaticLayoutVisibility(Service show)
        {
            Transform magic = body.Find("Magic Layout");
            Transform forgeInventory = body.Find("Forge Inventory");
            Transform forgeDetails = body.Find("Forge Details");
            Transform trade = body.Find("Trade Layout");
            Transform quest = body.Find("Quest Layout");
            Transform combat = body.Find("Combat Layout");
            if (magic != null) magic.gameObject.SetActive(show == Service.Magic);
            if (forgeInventory != null) forgeInventory.gameObject.SetActive(show == Service.Forge);
            if (forgeDetails != null) forgeDetails.gameObject.SetActive(show == Service.Forge);
            if (trade != null) trade.gameObject.SetActive(show == Service.Trade);
            if (quest != null) quest.gameObject.SetActive(show == Service.Quest);
            if (combat != null) combat.gameObject.SetActive(show == Service.Combat);
        }

        void Combat()
        {
            SetFrame("popup_frame_steel_cyan");
            RectTransform layout = Rect("Combat Layout", body, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform profile = Panel("Combat Summary", layout, "panel_inset_dark", new Vector2(0,0), new Vector2(.30f,1), new Vector2(0,0), new Vector2(-10,0));
            RectTransform upgrades = Panel("Upgrade List", layout, "panel_inset_dark", new Vector2(.30f,0), Vector2.one, new Vector2(8,0), Vector2.zero);
            Txt("Summary Heading", profile, "DIEM NANG CAP", 18, Gold, new Vector2(.05f,1), new Vector2(.95f,1), new Vector2(0,-48), new Vector2(0,-12), TextAlignmentOptions.Center);
            Txt("Points Value", profile, combatDemoPoints.ToString("N0"), 38, Color.white, new Vector2(.05f,.64f), new Vector2(.95f,.84f), Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);
            Txt("Points Source", profile, "Diem nhan khi hoan thanh\nham nguc", 17, Color.white, new Vector2(.08f,.48f), new Vector2(.92f,.64f), Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);
            Img("Points Divider", profile, "divider_ornate", new Vector2(.08f,.42f), new Vector2(.92f,.46f), Vector2.zero, Vector2.zero);
            Txt("Combat Note", profile, "Moi lan nang cap ton 250 diem.\n\nDu lieu hien tai la minh hoa.", 15, new Color(.78f,.84f,.92f), new Vector2(.09f,.17f), new Vector2(.91f,.40f), Vector2.zero, Vector2.zero, TextAlignmentOptions.TopLeft);
            Txt("Upgrade Heading", upgrades, "NANG CAP THUOC TINH", 20, Gold, new Vector2(.04f,1), new Vector2(.96f,1), new Vector2(0,-40), new Vector2(0,-8), TextAlignmentOptions.Center);
            string[] names = { "TAN CONG", "PHONG THU", "SINH LUC", "NANG LUONG", "CHI MANG" };
            string[] values = { "Cap 15  |  +10 sat thuong", "Cap 15  |  +8 giap", "Cap 15  |  +50 HP toi da", "Cap 15  |  +20 Mana toi da", "Cap 15  |  +2%" };
            for (int i = 0; i < names.Length; i++)
            {
                float top = -48 - i * 76;
                RectTransform row = Panel("Upgrade Row " + i, upgrades, i == 0 ? "row_selected" : "row_normal", new Vector2(.035f,1), new Vector2(.965f,1), new Vector2(0,top-68), new Vector2(0,top));
                Txt("Stat Name", row, names[i], 17, Gold, new Vector2(.04f,.52f), new Vector2(.53f,.95f), Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);
                Txt("Stat Value", row, values[i], 14, Color.white, new Vector2(.04f,.12f), new Vector2(.56f,.52f), Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);
                Btn("Upgrade Action", row, "NANG CAP (250)", "btn_ornate_teal", new Vector2(.59f,.17f), new Vector2(.98f,.83f), Vector2.zero, Vector2.zero, delegate { });
            }
        }

        void Quest()
        {
            SetFrame("popup_frame_brass");
            RectTransform layout = Rect("Quest Layout", body, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform tabs = Rect("Quest Tabs", layout, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -44), Vector2.zero);
            string[] tabLabels = { "NHIỆM VỤ CHÍNH", "NHIỆM VỤ PHỤ" };
            for (int i = 0; i < tabLabels.Length; i++)
                Btn("Tab " + i, tabs, tabLabels[i], i == tab ? "tab_active" : "tab_inactive",
                    new Vector2(i * .5f, 0), new Vector2((i + 1) * .5f, 1), new Vector2(4, 1), new Vector2(-4, -1), delegate { });

            RectTransform list = Panel("Quest List", layout, "panel_inset_dark",
                new Vector2(0, 0), new Vector2(.44f, 1), Vector2.zero, new Vector2(-10, -56));
            RectTransform detail = Panel("Quest Details", layout, "panel_light",
                new Vector2(.45f, 0), new Vector2(1, 1), new Vector2(8, 0), new Vector2(0, -56));
            Txt("List Heading", list, "DANH SÁCH NHIỆM VỤ", 16, Gold,
                new Vector2(.04f, 1), new Vector2(.96f, 1), new Vector2(0, -34), new Vector2(0, -6), TextAlignmentOptions.Center);

            string[][] questNames =
            {
                new[] { "GIẢI CỨU LÀNG", "KHÁM PHÁ HANG ĐỘNG", "TÌM DẤU VẾT", "DỌN SẠCH QUÁI VẬT", "ĐƯA HÀNG ĐẾN LÀNG" },
                new[] { "THU THẬP DƯỢC THẢO", "SĂN SÓI ĐỒNG", "TÌM NGƯỜI MẤT TÍCH", "GIAO HÀNG", "TÌM DI VẬT CỔ" }
            };
            for (int i = 0; i < 5; i++)
            {
                float top = -48 - i * 72;
                RectTransform row = Rect("Quest Row " + i, list,
                    new Vector2(.035f, 1), new Vector2(.965f, 1), new Vector2(0, top - 62), new Vector2(0, top));
                Image background = row.gameObject.AddComponent<Image>();
                background.sprite = SpriteFor(i == selected ? "row_selected" : "row_normal");
                background.type = Image.Type.Sliced;
                Button button = row.gameObject.AddComponent<Button>();
                button.targetGraphic = background;
                button.transition = Selectable.Transition.None;
                Txt("Quest Name", row, questNames[tab][i], 15, Gold,
                    new Vector2(.06f, 0), new Vector2(.95f, 1), Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);
            }

            Txt("Quest Name", detail, questNames[tab][selected], 22, new Color(.24f, .17f, .11f),
                new Vector2(.05f, 1), new Vector2(.95f, 1), new Vector2(0, -42), new Vector2(0, -8), TextAlignmentOptions.Center);
            Img("Quest Divider", detail, "divider_ornate",
                new Vector2(.06f, .87f), new Vector2(.94f, .90f), Vector2.zero, Vector2.zero);
            Txt("Quest Description", detail, "", 15, new Color(.22f, .17f, .12f),
                new Vector2(.07f, .66f), new Vector2(.93f, .86f), Vector2.zero, Vector2.zero, TextAlignmentOptions.TopLeft);
            Txt("Objectives Heading", detail, "MỤC TIÊU", 18, new Color(.27f, .16f, .10f),
                new Vector2(.07f, .59f), new Vector2(.93f, .65f), Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);
            Txt("Quest Objectives", detail, "", 15, new Color(.22f, .17f, .12f),
                new Vector2(.07f, .39f), new Vector2(.93f, .59f), Vector2.zero, Vector2.zero, TextAlignmentOptions.TopLeft);
            Txt("Rewards Heading", detail, "PHẦN THƯỞNG", 18, new Color(.27f, .16f, .10f),
                new Vector2(.07f, .31f), new Vector2(.93f, .38f), Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);
            Txt("Quest Rewards", detail, "", 16, new Color(.22f, .17f, .12f),
                new Vector2(.07f, .20f), new Vector2(.93f, .30f), Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);
            Txt("Quest Status", detail, "", 11, new Color(.30f, .43f, .34f),
                new Vector2(.06f, .13f), new Vector2(.94f, .18f), Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);
            Btn("Accept Action", detail, "NHẬN NHIỆM VỤ", "btn_ornate_teal",
                new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-145, 8), new Vector2(290, 48), delegate { });
        }

        void Trade()
        {
            SetFrame("popup_frame_brass");
            RectTransform layout = Rect("Trade Layout", body, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform tabBar = Rect("Trade Tabs", layout, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -44), Vector2.zero);
            string[] tabLabels = { "MUA", "BÁN", "TRAO ĐỔI" };
            for (int i = 0; i < tabLabels.Length; i++)
                Btn("Tab " + i, tabBar, tabLabels[i], i == tab ? "tab_active" : "tab_inactive",
                    new Vector2(i / 3f, 0), new Vector2((i + 1) / 3f, 1), new Vector2(4, 1), new Vector2(-4, -1), delegate { });

            RectTransform inventory = Panel("Player Inventory", layout, "panel_inset_dark",
                new Vector2(0, 0), new Vector2(.57f, 1), Vector2.zero, new Vector2(-12, -56));
            RectTransform detail = Panel("Trade Details", layout, "panel_card",
                new Vector2(.58f, 0), new Vector2(1, 1), new Vector2(10, 0), new Vector2(0, -56));
            Txt("Inventory Heading", inventory, "HÒM ĐỒ CỦA BẠN", 18, Gold,
                new Vector2(.04f, 1), new Vector2(.96f, 1), new Vector2(0, -38), new Vector2(0, -8), TextAlignmentOptions.Center);

            RectTransform filters = Rect("Inventory Filters", inventory,
                new Vector2(.03f, 1), new Vector2(.97f, 1), new Vector2(0, -88), new Vector2(0, -48));
            string[] filterLabels = { "TẤT CẢ", "TRANG BỊ", "TIÊU HAO", "NGUYÊN LIỆU" };
            for (int i = 0; i < filterLabels.Length; i++)
                Btn("Filter " + i, filters, filterLabels[i], i == 0 ? "tab_active" : "tab_inactive",
                    new Vector2(i / 4f, 0), new Vector2((i + 1) / 4f, 1), new Vector2(2, 1), new Vector2(-2, -1), delegate { });

            float cell = 76, gap = 16, left = 28, top = -108;
            for (int i = 0; i < 16; i++)
            {
                int row = i / 4, col = i % 4;
                float x = left + col * (cell + gap), y = top - row * (cell + gap);
                Btn("Item Slot " + i, inventory, "", i == selected ? "slot_selected" : "slot_empty",
                    new Vector2(0, 1), new Vector2(0, 1), new Vector2(x, y - cell), new Vector2(cell, cell), delegate { });
            }

            Txt("Mode Heading", detail, "CHI TIẾT TRAO ĐỔI", 20, Gold,
                new Vector2(.05f, 1), new Vector2(.95f, 1), new Vector2(0, -42), new Vector2(0, -8), TextAlignmentOptions.Center);
            RectTransform offered = Panel("Offered Item Card", detail, "panel_inset_dark",
                new Vector2(.05f, .58f), new Vector2(.95f, .84f), Vector2.zero, Vector2.zero);
            Txt("Offered Item Heading", offered, "VẬT PHẨM TRAO ĐỔI", 15, Gold,
                new Vector2(.04f, 1), new Vector2(.96f, 1), new Vector2(0, -30), new Vector2(0, -6), TextAlignmentOptions.Center);
            Image itemSlot = Img("Selected Item Slot", offered, "slot_empty",
                new Vector2(.08f, .12f), new Vector2(.08f, .12f), Vector2.zero, new Vector2(64, 64));
            itemSlot.type = Image.Type.Simple; itemSlot.preserveAspect = true;
            Txt("Selected Item Name", offered, "Chưa chọn vật phẩm", 17, Color.white,
                new Vector2(.24f, .12f), new Vector2(.94f, .88f), Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);

            RectTransform requirements = Panel("Trade Requirements", detail, "panel_inset_dark",
                new Vector2(.05f, .18f), new Vector2(.95f, .54f), Vector2.zero, Vector2.zero);
            Txt("Requirements Heading", requirements, "TRAO ĐỔI YÊU CẦU", 17, Gold,
                new Vector2(.04f, 1), new Vector2(.96f, 1), new Vector2(0, -32), new Vector2(0, -6), TextAlignmentOptions.Center);
            for (int i = 0; i < 2; i++)
            {
                float y = .67f - i * .38f;
                Image requirementSlot = Img("Requirement Slot " + i, requirements, "slot_empty",
                    new Vector2(.10f, y), new Vector2(.10f, y), Vector2.zero, new Vector2(42, 42));
                requirementSlot.type = Image.Type.Simple; requirementSlot.preserveAspect = true;
                Txt("Requirement Label " + i, requirements, i == 0 ? "Vật phẩm cần" : "Phí giao dịch",
                    15, Color.white, new Vector2(.25f, y - .06f), new Vector2(.78f, y + .06f), Vector2.zero, Vector2.zero,
                    TextAlignmentOptions.MidlineLeft);
                Txt("Requirement Value " + i, requirements, "—",
                    16, Gold, new Vector2(.78f, y - .06f), new Vector2(.94f, y + .06f), Vector2.zero, Vector2.zero,
                    TextAlignmentOptions.MidlineRight);
            }
            Txt("Trade Status", detail, "Chưa chọn vật phẩm. Dữ liệu giao dịch là minh họa.", 12,
                new Color(.7f, .85f, .78f), new Vector2(.05f, .12f), new Vector2(.95f, .18f), Vector2.zero, Vector2.zero,
                TextAlignmentOptions.Center);
            Btn("Primary Action", detail, "ĐĂNG TIN TRAO ĐỔI", "btn_ornate_gold",
                new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-145, 10), new Vector2(290, 48), delegate { });
        }

        void Magic()
        {
            SetFrame("popup_frame_steel_cyan");
            RectTransform layout = Rect("Magic Layout", body, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform book = Panel("Spell Grid", layout, "panel_inset_dark",
                new Vector2(0, 0), new Vector2(.53f, 1), new Vector2(0, 0), new Vector2(-10, 0));
            RectTransform detail = Panel("Spell Details", layout, "panel_card",
                new Vector2(.55f, 0), new Vector2(1, 1), new Vector2(8, 0), Vector2.zero);

            Txt("Spell Heading", book, "CHIÊU PHÉP", 20, Color.white,
                new Vector2(.04f, 1), new Vector2(.96f, 1), new Vector2(0, -45), new Vector2(0, -8), TextAlignmentOptions.Center);

            string[] labels = { "HỎA CẦU", "ICE SHARD", "HỒ CHÂN", "ĐỘI THÁP", "HỖN PHÁP", "HỎA CẦU", "TRẬY SẤU", "CHẤT PHÚC" };
            float cell = 80, gapX = 18, gapY = 22, left = 34, top = -62;
            for (int i = 0; i < 12; i++)
            {
                int index = i;
                int row = i / 3, col = i % 3;
                float x = left + col * (cell + gapX), y = top - row * (cell + gapY);
                bool locked = i >= 8;
                Button slot = Btn("Spell Slot " + i, book, "",
                    locked ? "slot_locked" : (i == selected ? "slot_selected" : "slot_empty"),
                    new Vector2(0, 1), new Vector2(0, 1), new Vector2(x, y - cell), new Vector2(cell, cell),
                    delegate { if (!locked) { selected = index; Rebuild(); } });
                slot.interactable = !locked;
                if (!locked)
                    Txt("Spell Label", slot.transform, labels[i], 10, Color.white,
                        new Vector2(.02f, 0), new Vector2(.98f, .34f), new Vector2(0, 3), Vector2.zero, TextAlignmentOptions.Center);
            }

            Btn("Page Previous", book, "", "icon_page_prev", new Vector2(0, 0), new Vector2(0, 0), new Vector2(34, 8), new Vector2(36, 36), delegate { });
            Txt("Page Label", book, "5: 1/2", 16, Color.white,
                new Vector2(.37f, 0), new Vector2(.63f, 0), new Vector2(0, 9), new Vector2(0, 35), TextAlignmentOptions.Center);
            Btn("Page Next", book, "", "icon_page_next", new Vector2(1, 0), new Vector2(1, 0), new Vector2(-70, 8), new Vector2(36, 36), delegate { });

            Txt("Detail Heading", detail, "THÔNG TIN CHI TIẾT", 22, Color.white,
                new Vector2(.04f, 1), new Vector2(.96f, 1), new Vector2(0, -44), new Vector2(0, -8), TextAlignmentOptions.Center);
            Image selectedSpellSlot = Img("Selected Spell Slot", detail, "slot_empty",
                new Vector2(.13f, .71f), new Vector2(.13f, .71f), Vector2.zero, new Vector2(64, 64));
            selectedSpellSlot.type = Image.Type.Simple;
            selectedSpellSlot.preserveAspect = true;
            Txt("Spell Name", detail, SpellName(selected) + " - CẤP 5", 22, Gold,
                new Vector2(.27f, .78f), new Vector2(.96f, .95f), Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);
            Txt("Spell Description", detail, "Phép thuật gây sát thương lên mục tiêu trong phạm vi.", 14, Color.white,
                new Vector2(.27f, .67f), new Vector2(.96f, .81f), Vector2.zero, Vector2.zero, TextAlignmentOptions.TopLeft);
            Txt("Spell Stats", detail, "Sát thương: 250  →  280\nMana tiêu hao: 100", 18, Color.white,
                new Vector2(.06f, .48f), new Vector2(.94f, .64f), Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);
            Img("Stats Divider", detail, "divider_ornate",
                new Vector2(.06f, .45f), new Vector2(.94f, .48f), Vector2.zero, Vector2.zero);
            Txt("Upgrade Heading", detail, "YÊU CẦU NÂNG CẤP", 19, Color.white,
                new Vector2(.04f, .39f), new Vector2(.96f, .45f), Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);

            string[] requirements = { "x5", "x1", "30" };
            string[] requirementSprites = { "slot_empty", "slot_empty", "slot_empty" };
            for (int i = 0; i < requirements.Length; i++)
            {
                float x = .06f + i * .17f;
                Image requirement = Img("Requirement " + i, detail, requirementSprites[i], new Vector2(x, .28f), new Vector2(x, .28f), Vector2.zero, new Vector2(58, 56));
                requirement.type = Image.Type.Simple;
                requirement.preserveAspect = true;
                Txt("Requirement Count " + i, detail, requirements[i], 13, Color.white,
                    new Vector2(x, .25f), new Vector2(x, .25f), new Vector2(35, 1), new Vector2(22, 18), TextAlignmentOptions.BottomRight);
            }
            Img("Gold Icon", detail, "icon_coin", new Vector2(.66f, .29f), new Vector2(.66f, .29f), Vector2.zero, new Vector2(28, 28));
            Txt("Gold Requirement", detail, "200\nVÀNG", 15, Gold,
                new Vector2(.72f, .23f), new Vector2(.96f, .34f), Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);
            Txt("Status", detail, "Dữ liệu nâng cấp minh họa.", 11, new Color(.7f, .85f, .78f),
                new Vector2(.05f, .15f), new Vector2(.95f, .20f), Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);
            Btn("Upgrade Action", detail, "NÂNG CẤP", "btn_ornate_teal",
                new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-150, 56), new Vector2(300, 50), delegate { });
            Btn("Learn Action", detail, "HỌC", "btn_blue",
                new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-100, 4), new Vector2(200, 44), delegate { });
        }

void Forge()
{
    SetFrame("popup_frame_steel_gold");

    RectTransform inventory = Panel("Forge Inventory", body, "panel_inset_dark",
        new Vector2(0, 0), new Vector2(.33f, 1), new Vector2(0, 0), new Vector2(-10, 0));
    RectTransform detail = Rect("Forge Details", body,
        new Vector2(.33f, 0), new Vector2(1, 1), new Vector2(8, 0), Vector2.zero);

    Txt("Inventory Heading", inventory, "VŨ KHÍ CỦA BẠN", 19, Gold,
        new Vector2(.04f, 1), new Vector2(.96f, 1), new Vector2(0, -48), new Vector2(0, -10),
        TextAlignmentOptions.Center);

    string[] weapons = { "THANH KIẾM DŨNG SĨ", "KIẾM SẮT", "RÌU CHIẾN", "KHIÊN ĐỒNG", "CUNG GỖ" };
    for (int i = 0; i < weapons.Length; i++)
    {
        int index = i;
        float top = -56 - i * 76;
        Button row = Btn("Weapon Row " + i, inventory, "",
            i == selected ? "row_selected" : "row_normal",
            new Vector2(.04f, 1), new Vector2(.96f, 1),
            new Vector2(0, top - 68), new Vector2(0, top),
            delegate { selected = index; actionUsed = false; Rebuild(); });

        Img("Empty Weapon Slot", row.transform, i == selected ? "slot_selected" : "slot_empty",
            new Vector2(0, .5f), new Vector2(0, .5f), new Vector2(34, 0), new Vector2(48, 48));
        Txt("Weapon Name", row.transform, weapons[i], 15, Color.white,
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(70, 0), new Vector2(-8, 0),
            TextAlignmentOptions.MidlineLeft);
    }

    Image emptyItem = Img("Empty Selected Item", detail, "slot_empty",
        new Vector2(.15f, .77f), new Vector2(.15f, .77f), Vector2.zero, new Vector2(88, 88));
    emptyItem.type = Image.Type.Simple;
    emptyItem.preserveAspect = true;

    Txt("Selected Weapon Name", detail, weapons[Mathf.Clamp(selected, 0, weapons.Length - 1)],
        21, Gold, new Vector2(.30f, .77f), new Vector2(.96f, .95f),
        Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);
    Txt("Upgrade From", detail, "CẤP +5", 18, Color.white,
        new Vector2(.30f, .64f), new Vector2(.52f, .77f),
        Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);
    Img("Upgrade Arrow", detail, "icon_arrow_right_green",
        new Vector2(.55f, .65f), new Vector2(.62f, .76f), Vector2.zero, Vector2.zero);
    Txt("Upgrade To", detail, "+6", 20, Color.white,
        new Vector2(.64f, .64f), new Vector2(.82f, .77f),
        Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);

    RectTransform stats = Panel("Stats Card", detail, "panel_card",
        new Vector2(.035f, .27f), new Vector2(.48f, .59f), new Vector2(0, 0), new Vector2(0, 0));
    RectTransform materials = Panel("Materials Card", detail, "panel_card",
        new Vector2(.52f, .27f), new Vector2(.965f, .59f), new Vector2(0, 0), new Vector2(0, 0));

    Txt("Stats Heading", stats, "CHỈ SỐ", 18, Gold,
        new Vector2(.04f, .7f), new Vector2(.96f, .98f), Vector2.zero, Vector2.zero,
        TextAlignmentOptions.Center);
    Txt("Stats Values", stats, "TẤN CÔNG\n150  ->  180",
        17, Color.white, new Vector2(.06f, .12f), new Vector2(.94f, .72f),
        Vector2.zero, Vector2.zero, TextAlignmentOptions.Center);

    Txt("Materials Heading", materials, "NGUYÊN LIỆU CẦN THIẾT", 16, Gold,
        new Vector2(.03f, .7f), new Vector2(.97f, .98f), Vector2.zero, Vector2.zero,
        TextAlignmentOptions.Center);
    Txt("Materials Values", materials, "ĐÁ CƯỜNG HÓA   5 / 10\nVÀNG     1000 / 2000",
        15, Color.white, new Vector2(.06f, .12f), new Vector2(.94f, .7f),
        Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);

    Action(detail, "NÂNG CẤP", "btn_ornate_gold");
}

        void Memorial()
        {
            SetFrame("popup_frame_brass"); Tabs(new[]{"LỜI KHẤN","PHƯỚC LÀNH","TƯỞNG NIỆM"});
            RectTransform list=Panel("Blessings",body,"panel_card",new Vector2(0,0),new Vector2(.42f,1),new Vector2(0,0),new Vector2(-10,-52));
            RectTransform detail=Panel("Blessing Details",body,"panel_light",new Vector2(.42f,0),new Vector2(1,1),new Vector2(10,0),new Vector2(0,-52));
            string[] names=tab==0?new[]{"Lời khấn bình an","Tưởng niệm anh hùng","Lịch sử chiến công"}:new[]{"Phước lành chiến binh","Phước lành lữ khách"};
            Rows(list,names,68,"slot_uncommon");
            Txt("Memorial Info",detail,Selected(names)+"\n\nHiệu ứng minh họa: tăng tinh thần\nThời lượng: 5 phút\n\nXem lời khấn, nhận phước lành và tưởng niệm các anh hùng.\n\nPhần thưởng sẽ nối dữ liệu game sau.",20,new Color(.22f,.17f,.12f),new Vector2(.1f,.31f),new Vector2(.9f,.9f),Vector2.zero,Vector2.zero,TextAlignmentOptions.TopLeft);
            Action(detail,"NHẬN PHƯỚC LÀNH","btn_ornate_gold");
        }

        void Tabs(string[] labels)
        {
            RectTransform bar=Rect("Tabs",body,new Vector2(0,1),new Vector2(1,1),new Vector2(0,-47),Vector2.zero);
            for(int i=0;i<labels.Length;i++)
            {
                int ix=i;
                Btn("Tab "+i,bar,labels[i],i==tab?"tab_active":"tab_inactive",new Vector2(i/(float)labels.Length,0),new Vector2((i+1)/(float)labels.Length,1),new Vector2(4,1),new Vector2(-4,-1),delegate{tab=ix;selected=0;Rebuild();});
            }
        }

        void Rows(RectTransform panel,string[] names,float height,string icon)
        {
            for(int i=0;i<names.Length;i++)
            {
                int ix=i; float y=-10-i*height;
                Button row=Btn("Entry "+i,panel,"",i==selected?"row_selected":"row_normal",new Vector2(.03f,1),new Vector2(.97f,1),new Vector2(0,y),new Vector2(0,y-height+6),delegate{selected=ix;Rebuild();});
                Img("Icon",row.transform,icon,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(30,0),new Vector2(42,42));
                Txt("Name",row.transform,names[i],17,Color.white,new Vector2(0,0),new Vector2(1,1),new Vector2(68,0),new Vector2(-6,0),TextAlignmentOptions.MidlineLeft);
            }
        }

        void Grid(RectTransform panel,string[] names,int cols,int rows,float cell,float gap,float panelWidth)
        {
            float left=(panelWidth-(cols*cell+(cols-1)*gap))*.5f;
            string[] slots={"slot_rare","slot_uncommon","slot_common","slot_epic"};
            for(int i=0;i<names.Length;i++)
            {
                int ix=i; float x=left+(i%cols)*(cell+gap); float y=-84-(i/cols)*(cell+gap);
                RectTransform fill=Rect("Item Fill "+i,panel,new Vector2(0,1),new Vector2(0,1),new Vector2(x,y),new Vector2(cell,cell));
                Image fillImage=fill.gameObject.AddComponent<Image>();fillImage.color=new Color(.055f,.07f,.09f,.95f);
                Button slot=Btn("Item "+i,panel,"",i==selected?"slot_selected":slots[i%slots.Length],new Vector2(0,1),new Vector2(0,1),new Vector2(x,y),new Vector2(cell,cell),delegate{selected=ix;Rebuild();});
                Txt("Glyph",slot.transform,names[i].Substring(0,1).ToUpperInvariant(),17,new Color(.98f,.83f,.56f),Vector2.zero,Vector2.one,new Vector2(3,3),new Vector2(-3,-3),TextAlignmentOptions.Center);
                Txt("Count",slot.transform,(i+2).ToString(),12,Color.white,new Vector2(1,0),new Vector2(1,0),new Vector2(-24,1),new Vector2(22,17),TextAlignmentOptions.BottomRight);
            }
        }

        void Action(RectTransform panel,string label,string button)
        {
            string message=actionUsed?"Đã xác nhận demo • không trừ vật phẩm.":"Dữ liệu/công thức hiện là minh họa.";
            Txt("Status",panel,message,14,new Color(.62f,.88f,.72f),new Vector2(.06f,.13f),new Vector2(.94f,.21f),Vector2.zero,Vector2.zero,TextAlignmentOptions.Center);
            Btn("Action",panel,actionUsed?"ĐÃ XÁC NHẬN":label,button,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(-132,12),new Vector2(264,52),delegate{actionUsed=true;Rebuild();});
        }

        void SetFrame(string name){Image image=frame.GetComponent<Image>();image.sprite=SpriteFor(name);image.type=Image.Type.Sliced;}
        string Selected(string[] a){return a.Length==0?string.Empty:a[Mathf.Clamp(selected,0,a.Length-1)];}
        Sprite SpriteFor(string name){if(spriteNames==null||kitSprites==null)return null;for(int i=0;i<spriteNames.Length&&i<kitSprites.Length;i++)if(spriteNames[i]==name)return kitSprites[i];return null;}
        RectTransform Rect(string name,Transform parent,Vector2 amin,Vector2 amax,Vector2 omin,Vector2 omax)
        {
            GameObject go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);RectTransform rt=(RectTransform)go.transform;rt.anchorMin=amin;rt.anchorMax=amax;
            if(amin==amax){rt.anchoredPosition=omin;rt.sizeDelta=omax;}else{rt.offsetMin=omin;rt.offsetMax=omax;}return rt;
        }
        RectTransform Panel(string name,Transform parent,string sprite,Vector2 amin,Vector2 amax,Vector2 omin,Vector2 omax)
        {
            RectTransform rt=Rect(name,parent,amin,amax,omin,omax);Image image=rt.gameObject.AddComponent<Image>();image.sprite=SpriteFor(sprite);image.type=Image.Type.Sliced;return rt;
        }
        Image Img(string name,Transform parent,string sprite,Vector2 amin,Vector2 amax,Vector2 omin,Vector2 omax)
        {
            RectTransform rt=Rect(name,parent,amin,amax,omin,omax);Image image=rt.gameObject.AddComponent<Image>();image.sprite=SpriteFor(sprite);image.type=Image.Type.Sliced;image.raycastTarget=false;return image;
        }
        TMP_Text Txt(string name,Transform parent,string value,int size,Color color,Vector2 amin,Vector2 amax,Vector2 omin,Vector2 omax,TextAlignmentOptions align)
        {
            RectTransform rt=Rect(name,parent,amin,amax,omin,omax);TMP_Text t=rt.gameObject.AddComponent<TextMeshProUGUI>();t.text=value;t.fontSize=size;t.color=color;t.alignment=align;t.enableWordWrapping=true;t.raycastTarget=false;return t;
        }
        Button Btn(string name,Transform parent,string label,string sprite,Vector2 amin,Vector2 amax,Vector2 omin,Vector2 omax,UnityEngine.Events.UnityAction action)
        {
            RectTransform rt=Rect(name,parent,amin,amax,omin,omax);Image image=rt.gameObject.AddComponent<Image>();image.sprite=SpriteFor(sprite);image.type=Image.Type.Sliced;
            Button b=rt.gameObject.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(action);
            if(!string.IsNullOrEmpty(label))Txt("Label",rt,label,16,Color.white,Vector2.zero,Vector2.one,new Vector2(5,2),new Vector2(-5,-2),TextAlignmentOptions.Center);
            return b;
        }
    }
}
