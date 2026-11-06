using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cainos.PixelArtTopDown_Basic
{
    [DisallowMultipleComponent]
    public sealed class SteleInteractionController : MonoBehaviour
    {
        public static SteleInteractionController Instance { get; private set; }

        [SerializeField] private GameObject promptRoot;
        [SerializeField] private TMP_Text promptTitle;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private GameObject interactionPanel;
        [SerializeField] private TMP_Text interactionTitle;

        private readonly HashSet<SteleInteractable> nearbySteles = new HashSet<SteleInteractable>();
        private SteleInteractable[] allSteles = System.Array.Empty<SteleInteractable>();
        private Transform player;
        private SteleInteractable focusedStele;
        private Collider2D playerCollider;

        private void Awake()
        {
            Instance = this;
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                playerCollider = playerObject.GetComponent<Collider2D>();
            }
            allSteles = FindObjectsByType<SteleInteractable>(FindObjectsSortMode.None);
            if (promptText != null)
                promptText.text = "[ SPACE ]  /  CLICK ĐỂ TƯƠNG TÁC";
            if (promptRoot != null)
                promptRoot.SetActive(false);

            if (interactionPanel != null)
            {
                var closeButton = interactionPanel.transform.Find("Window Frame/Close Button");
                if (closeButton != null)
                {
                    var button = closeButton.GetComponent<Button>();
                    if (button == null)
                        button = closeButton.gameObject.AddComponent<Button>();
                    button.targetGraphic = closeButton.GetComponent<Graphic>();
                    button.onClick.AddListener(ClosePanel);
                }
            }
        }

        private void Update()
        {
            if (WorldMapScreen.Instance != null && WorldMapScreen.Instance.IsOpen)
            {
                if (promptRoot != null && promptRoot.activeSelf)
                    promptRoot.SetActive(false);
                return;
            }

            RefreshNearbySteles();
            focusedStele = FindClosestStele();
            bool canPrompt = focusedStele != null && (interactionPanel == null || !interactionPanel.activeSelf);
            if (promptRoot != null && promptRoot.activeSelf != canPrompt)
                promptRoot.SetActive(canPrompt);
            if (canPrompt)
            {
                if (promptTitle != null)
                    promptTitle.text = focusedStele.InteractionTitle;
                if (promptText != null)
                    promptText.text = "[ SPACE ] / CLICK ĐỂ TƯƠNG TÁC\n" + focusedStele.InteractionHint;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (interactionPanel != null && interactionPanel.activeSelf)
                    ClosePanel();
                else if (focusedStele != null)
                    OpenPanel(focusedStele);
            }

            if (Input.GetMouseButtonDown(0) && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
                TryInteractAtPointer();
        }

        private void RefreshNearbySteles()
        {
            if (player == null)
            {
                var playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    player = playerObject.transform;
                    playerCollider = playerObject.GetComponent<Collider2D>();
                }
            }

            nearbySteles.Clear();
            if (player == null)
                return;

            if (allSteles == null || allSteles.Length == 0)
                allSteles = FindObjectsByType<SteleInteractable>(FindObjectsSortMode.None);

            float playerPadding = playerCollider != null
                ? Mathf.Max(playerCollider.bounds.extents.x, playerCollider.bounds.extents.y)
                : 0f;

            foreach (var stele in allSteles)
            {
                if (stele == null || !stele.isActiveAndEnabled)
                    continue;

                var outline = stele.GetComponent<SteleProximityOutline>();
                var trigger = stele.GetComponent<CircleCollider2D>();
                float radius = trigger != null
                    ? Mathf.Max(trigger.bounds.extents.x, trigger.bounds.extents.y)
                    : outline != null ? outline.ProximityRadius : 2.1f;
                radius += playerPadding;
                Vector2 center = trigger != null ? trigger.bounds.center : (Vector2)stele.transform.position;
                Vector2 playerPosition = player.position;
                if ((playerPosition - center).sqrMagnitude <= radius * radius)
                    nearbySteles.Add(stele);
            }
        }

        public void TryInteract(SteleInteractable stele)
        {
            if (stele != null && nearbySteles.Contains(stele))
                OpenPanel(stele);
        }

        public void ClosePanel()
        {
            if (interactionPanel != null)
                interactionPanel.SetActive(false);
        }

        private SteleInteractable FindClosestStele()
        {
            if (player == null)
                return null;

            SteleInteractable closest = null;
            float closestDistance = float.PositiveInfinity;
            foreach (var stele in nearbySteles)
            {
                if (stele == null || !stele.isActiveAndEnabled)
                    continue;

                float distance = (stele.transform.position - player.position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closest = stele;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        private void TryInteractAtPointer()
        {
            var gameCamera = Camera.main;
            if (gameCamera == null)
                return;

            var point = gameCamera.ScreenToWorldPoint(Input.mousePosition);
            var hits = Physics2D.OverlapPointAll(point);
            foreach (var hit in hits)
            {
                if (hit == null || hit.isTrigger)
                    continue;

                var stele = hit.GetComponent<SteleInteractable>();
                if (stele == null || !nearbySteles.Contains(stele))
                    continue;

                OpenPanel(stele);
                return;
            }
        }

        private void OpenPanel(SteleInteractable stele)
        {
            if (stele != null && stele.OpensWorldMap && WorldMapScreen.Instance != null)
            {
                ClosePanel();
                WorldMapScreen.Instance.Open();
                return;
            }

            if (stele == null || interactionPanel == null)
                return;

            var canvasRoot = transform;
            for (int i = 0; i < canvasRoot.childCount; i++)
            {
                var sibling = canvasRoot.GetChild(i);
                if (sibling != interactionPanel.transform && sibling.name.StartsWith("Panel - "))
                    sibling.gameObject.SetActive(false);
            }

            if (interactionTitle != null)
                interactionTitle.text = stele.InteractionTitle;
            if (promptRoot != null)
                promptRoot.SetActive(false);

            interactionPanel.SetActive(true);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
