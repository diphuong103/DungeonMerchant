using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    /// <summary>
    /// Inspector-friendly callbacks for dungeon victory and return buttons.
    /// Add this component to a dungeon scene UI object, then wire its public
    /// methods to the corresponding Unity Button OnClick events.
    /// </summary>
    public sealed class DungeonFlowActions : MonoBehaviour
    {
        public void MarkCurrentDungeonCleared()
        {
            WorldMapProgress.MarkCurrentDungeonCleared();
        }

        public void ReturnToVillage()
        {
            WorldMapProgress.ReturnToVillage();
        }

        public void CompleteDungeonAndReturn()
        {
            WorldMapProgress.MarkCurrentDungeonCleared();
            WorldMapProgress.ReturnToVillage();
        }
    }
}
