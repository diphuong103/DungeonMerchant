using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cainos.PixelArtTopDown_Basic
{
    /// <summary>
    /// Small persistent progression store for the world map.
    /// Call MarkDungeonCleared after the dungeon's victory condition succeeds.
    /// </summary>
    public static class WorldMapProgress
    {
        private const string ClearedKeyPrefix = "WorldMap.Cleared.";
        private const string UnlockedKeyPrefix = "WorldMap.Unlocked.";
        private const string CurrentDungeonKey = "WorldMap.CurrentDungeon";
        private const string ReturnSceneKey = "WorldMap.ReturnScene";

        public static bool IsCleared(string dungeonId)
        {
            return !string.IsNullOrWhiteSpace(dungeonId) && PlayerPrefs.GetInt(ClearedKeyPrefix + dungeonId, 0) == 1;
        }

        public static bool IsUnlocked(string dungeonId)
        {
            return !string.IsNullOrWhiteSpace(dungeonId) && PlayerPrefs.GetInt(UnlockedKeyPrefix + dungeonId, 0) == 1;
        }

        public static void Unlock(string dungeonId)
        {
            if (string.IsNullOrWhiteSpace(dungeonId))
                return;

            PlayerPrefs.SetInt(UnlockedKeyPrefix + dungeonId, 1);
            PlayerPrefs.Save();
        }

        /// <summary>Persist a clear and unlock the next dungeon in map order.</summary>
        public static void MarkDungeonCleared(string dungeonId)
        {
            if (string.IsNullOrWhiteSpace(dungeonId))
                return;

            PlayerPrefs.SetInt(ClearedKeyPrefix + dungeonId, 1);
            PlayerPrefs.SetInt(UnlockedKeyPrefix + dungeonId, 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Convenience hook for a dungeon victory screen. The selected ID is saved
        /// immediately before loading its configured scene.
        /// </summary>
        public static void MarkCurrentDungeonCleared()
        {
            string dungeonId = PlayerPrefs.GetString(CurrentDungeonKey, string.Empty);
            MarkDungeonCleared(dungeonId);
        }

        public static string GetCurrentDungeonId()
        {
            return PlayerPrefs.GetString(CurrentDungeonKey, string.Empty);
        }

        public static void SetCurrentDungeonId(string dungeonId)
        {
            PlayerPrefs.SetString(CurrentDungeonKey, dungeonId ?? string.Empty);
            PlayerPrefs.Save();
        }

        public static void SetReturnScene(string sceneName)
        {
            PlayerPrefs.SetString(ReturnSceneKey, sceneName ?? string.Empty);
            PlayerPrefs.Save();
        }

        /// <summary>Wire this to the exit/return button in a dungeon scene.</summary>
        public static void ReturnToVillage()
        {
            string sceneName = PlayerPrefs.GetString(ReturnSceneKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName))
            {
                SceneManager.LoadScene(sceneName);
                return;
            }

            Debug.LogError("Không thể quay lại làng: scene xuất phát chưa được lưu hoặc chưa có trong Build Settings.");
        }

        /// <summary>Use from a profile reset flow if map progression should be wiped.</summary>
        public static void ResetProgress()
        {
            PlayerPrefs.DeleteKey(CurrentDungeonKey);
            PlayerPrefs.DeleteKey(ReturnSceneKey);
            PlayerPrefs.DeleteKey(ClearedKeyPrefix + "crypt");
            PlayerPrefs.DeleteKey(ClearedKeyPrefix + "spider");
            PlayerPrefs.DeleteKey(ClearedKeyPrefix + "demon");
            PlayerPrefs.DeleteKey(ClearedKeyPrefix + "moon");
            PlayerPrefs.DeleteKey(ClearedKeyPrefix + "eye");
            PlayerPrefs.DeleteKey(UnlockedKeyPrefix + "crypt");
            PlayerPrefs.DeleteKey(UnlockedKeyPrefix + "spider");
            PlayerPrefs.DeleteKey(UnlockedKeyPrefix + "demon");
            PlayerPrefs.DeleteKey(UnlockedKeyPrefix + "moon");
            PlayerPrefs.DeleteKey(UnlockedKeyPrefix + "eye");
            PlayerPrefs.Save();
        }
    }
}
