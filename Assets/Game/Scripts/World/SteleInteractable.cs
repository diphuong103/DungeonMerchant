using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    [DisallowMultipleComponent]
    public sealed class SteleInteractable : MonoBehaviour
    {
        [SerializeField] private string interactionTitle = "TƯƠNG TÁC";
        [SerializeField, TextArea(1, 2)] private string interactionHint = "";
        [SerializeField] private bool opensWorldMap;

        public string InteractionTitle => interactionTitle;
        public string InteractionHint => interactionHint;
        public bool OpensWorldMap => opensWorldMap;
    }
}
