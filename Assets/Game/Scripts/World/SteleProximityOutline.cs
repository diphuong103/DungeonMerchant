using System.Collections.Generic;
using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    [DisallowMultipleComponent]
    public sealed class SteleProximityOutline : MonoBehaviour
    {
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private Color proximityColor = new Color(1f, 0.25f, 0.8f, 1f);
        [SerializeField, Min(0.1f)] private float proximityRadius = 2.1f;
        [SerializeField, Min(0.1f)] private float colorChangeSpeed = 8f;

        private readonly HashSet<Collider2D> playerColliders = new HashSet<Collider2D>();
        private SpriteRenderer outlineRenderer;
        private Color restingColor;

        public float ProximityRadius => proximityRadius;

        private void Awake()
        {
            var outline = transform.Find("Bright Outline");
            if (outline != null)
                outlineRenderer = outline.GetComponent<SpriteRenderer>();

            if (outlineRenderer == null)
            {
                Debug.LogWarning($"{nameof(SteleProximityOutline)} on {name} could not find a 'Bright Outline' SpriteRenderer.", this);
                enabled = false;
                return;
            }

            restingColor = outlineRenderer.color;

            var trigger = GetComponent<CircleCollider2D>();
            if (trigger == null)
                trigger = gameObject.AddComponent<CircleCollider2D>();

            trigger.isTrigger = true;
            var scale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
            trigger.radius = proximityRadius / Mathf.Max(scale, 0.001f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(playerTag))
                playerColliders.Add(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(playerTag))
                playerColliders.Remove(other);
        }

        private void Update()
        {
            var targetColor = playerColliders.Count > 0 ? proximityColor : restingColor;
            outlineRenderer.color = Color.Lerp(outlineRenderer.color, targetColor, colorChangeSpeed * Time.deltaTime);
        }

        private void OnDisable()
        {
            playerColliders.Clear();
            if (outlineRenderer != null)
                outlineRenderer.color = restingColor;
        }
    }
}
