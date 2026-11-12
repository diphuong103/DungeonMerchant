using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DungeonMerchant.UI
{
    public sealed class DungeonSelectScreen : MonoBehaviour
    {
        [System.Serializable]
        public sealed class DungeonEntry
        {
            public string title;
            public string subtitle;
            public string difficulty;
            public string reward;
            public string boss;
            public string status;
            public bool unlocked;
            public Button button;
        }

        [SerializeField] private Text detailTitle;
        [SerializeField] private Text detailBody;
        [SerializeField] private Text actionLabel;
        private readonly List<DungeonEntry> entries = new List<DungeonEntry>();
        private DungeonEntry selected;
        private int selectedIndex;

        public void SetDetails(Text title, Text body, Text action)
        {
            detailTitle = title;
            detailBody = body;
            actionLabel = action;
        }

        public void AddDungeon(DungeonEntry entry)
        {
            entries.Add(entry);
            if (entry.button != null)
                entry.button.onClick.AddListener(() => SelectEntry(entry));
        }

        public void Initialize()
        {
            selectedIndex = Mathf.Max(0, entries.FindIndex(entry => entry.unlocked));
            if (entries.Count > 0)
                Select(entries[selectedIndex]);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || entries.Count == 0)
                return;

            if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
                SelectIndex((selectedIndex + 1) % entries.Count);
            else if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
                SelectIndex((selectedIndex + entries.Count - 1) % entries.Count);

            if (keyboard.eKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)
                ConfirmSelected();
        }

        private void SelectEntry(DungeonEntry entry)
        {
            selectedIndex = entries.IndexOf(entry);
            Select(entry);
        }

        private void SelectIndex(int index)
        {
            selectedIndex = index;
            Select(entries[selectedIndex]);
        }

        private void Select(DungeonEntry entry)
        {
            selected = entry;
            if (detailTitle != null)
                detailTitle.text = entry.title + "  /  " + entry.status;
            if (detailBody != null)
                detailBody.text = "Độ khó  " + entry.difficulty + "     Boss  " + entry.boss + "     Thưởng  " + entry.reward;
            if (actionLabel != null)
                actionLabel.text = entry.unlocked ? "[ E ]  VÀO HẦM" : "CỔNG ĐANG BỊ KHÓA";
        }

        public void ConfirmSelected()
        {
            if (selected == null || !selected.unlocked)
                return;
            if (actionLabel != null)
                actionLabel.text = "ĐÃ CHỌN  " + selected.title.ToUpperInvariant();
        }
    }
}
