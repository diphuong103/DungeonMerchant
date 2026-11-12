using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cainos.PixelArtTopDown_Basic
{
    /// <summary>
    /// Draws a curved, animated dungeon route inside the World Map's Screen Space Canvas.
    /// Individual dash sprites are placed along the curve so the route remains compatible
    /// with the existing overlay Canvas and does not rely on a world-space LineRenderer.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class MapPathLine : MonoBehaviour
    {
        public enum PathState
        {
            Locked,
            Unlocked,
            Cleared
        }

        [Header("Endpoints")]
        [SerializeField] private RectTransform from;
        [SerializeField] private RectTransform to;
        [SerializeField] private PathState state = PathState.Locked;

        [Header("Curve")]
        [SerializeField, Min(8)] private int segments = 40;
        [SerializeField, Min(0f)] private float curveHeight = 42f;

        [Header("Dash Pattern")]
        [SerializeField, Min(4f)] private float dashWidth = 24f;
        [SerializeField, Min(2f)] private float dashHeight = 10f;
        [SerializeField, Min(0f)] private float dashGap = 18f;
        [SerializeField, Min(0f)] private float flowSpeed = 40f;

        [Header("Sprites")]
        [SerializeField] private Sprite lockedDash;
        [SerializeField] private Sprite unlockedDash;
        [SerializeField] private Sprite clearedDash;
        [SerializeField] private Sprite lockedKnot;
        [SerializeField] private Sprite unlockedKnot;
        [SerializeField] private Sprite clearedKnot;
        [SerializeField] private Sprite trackSprite;
        [SerializeField] private Sprite lockedMidpoint;
        [SerializeField] private Sprite unlockedGlow;
        [SerializeField] private Sprite clearedGlow;
        [SerializeField] private Sprite goldSpark;
        [SerializeField] private Sprite greenSpark;

        private readonly List<Vector3> curvePoints = new List<Vector3>();
        private readonly List<Image> trackImages = new List<Image>();
        private readonly List<Image> dashImages = new List<Image>();
        private readonly List<Image> glowImages = new List<Image>();
        private Image fromKnot;
        private Image toKnot;
        private Image midpointLock;
        private Image spark;
        private float curveLength;
        private float visibleStart;
        private float visibleLength;
        private float flowOffset;
        private bool visualsBuilt;

        /// <summary>Assign endpoints and visual states when building the Canvas.</summary>
        public void Configure(RectTransform start, RectTransform end, PathState pathState,
            Sprite lockedDashSprite, Sprite unlockedDashSprite, Sprite clearedDashSprite,
            Sprite lockedKnotSprite, Sprite unlockedKnotSprite, Sprite clearedKnotSprite,
            Sprite continuousTrackSprite,
            Sprite lockedMidpointSprite, Sprite unlockedGlowSprite, Sprite clearedGlowSprite,
            Sprite goldSparkSprite, Sprite greenSparkSprite)
        {
            from = start;
            to = end;
            state = pathState;
            lockedDash = lockedDashSprite;
            unlockedDash = unlockedDashSprite;
            clearedDash = clearedDashSprite;
            lockedKnot = lockedKnotSprite;
            unlockedKnot = unlockedKnotSprite;
            clearedKnot = clearedKnotSprite;
            trackSprite = continuousTrackSprite;
            lockedMidpoint = lockedMidpointSprite;
            unlockedGlow = unlockedGlowSprite;
            clearedGlow = clearedGlowSprite;
            goldSpark = goldSparkSprite;
            greenSpark = greenSparkSprite;

            Rebuild();
        }

        private void Start()
        {
            if (!visualsBuilt)
                Rebuild();
            ApplyStateVisuals();
        }

        private void Update()
        {
            if (!visualsBuilt || from == null || to == null)
                return;

            if (state == PathState.Unlocked && visibleLength > 0f)
            {
                flowOffset = Mathf.Repeat(flowOffset + flowSpeed * Time.deltaTime, Mathf.Max(1f, dashWidth + dashGap));
                PositionDashes();
                PositionSpark(Time.time * flowSpeed * 1.8f);
            }
            else if (state == PathState.Cleared && visibleLength > 0f)
            {
                PositionSpark(Time.time * flowSpeed * 0.8f);
            }
        }

        /// <summary>Change the route's lock/clear state and refresh its sprites.</summary>
        public void SetState(PathState nextState)
        {
            state = nextState;
            ApplyStateVisuals();
        }

        /// <summary>Rebuild the sampled curve and its Canvas dash children.</summary>
        public void Rebuild()
        {
            if (from == null || to == null)
                return;

            BuildCurve();
            if (curveLength <= dashWidth * 1.5f)
                return;

            // Start dashes at each node's visible edge. The continuous track itself reaches node centers.
            float startPadding = Mathf.Max(from.rect.width, from.rect.height) * 0.45f;
            float endPadding = Mathf.Max(to.rect.width, to.rect.height) * 0.45f;
            float requiredVisibleLength = dashWidth * 1.5f;
            float paddingTotal = startPadding + endPadding;
            float maxPaddingTotal = Mathf.Max(0f, curveLength - requiredVisibleLength);
            if (paddingTotal > maxPaddingTotal && paddingTotal > 0f)
            {
                float paddingScale = maxPaddingTotal / paddingTotal;
                startPadding *= paddingScale;
                endPadding *= paddingScale;
            }

            visibleStart = startPadding;
            float visibleEnd = curveLength - endPadding;
            visibleLength = visibleEnd - visibleStart;
            int dashCount = Mathf.Max(2, Mathf.FloorToInt(visibleLength / (dashWidth + dashGap)) + 1);

            if (!visualsBuilt || dashImages.Count != dashCount)
                BuildVisuals(dashCount);

            ApplyStateVisuals();
            PositionTrack();
            PositionDashes();
            PositionKnot(fromKnot, visibleStart);
            PositionKnot(toKnot, curveLength - endPadding);
            PositionMidpointLock();
            PositionSpark(0f);
        }

        private void BuildCurve()
        {
            curvePoints.Clear();
            Vector3 a = transform.InverseTransformPoint(from.position);
            Vector3 b = transform.InverseTransformPoint(to.position);
            Vector3 delta = b - a;
            float chord = delta.magnitude;
            if (chord <= 0.01f)
            {
                curveLength = 0f;
                return;
            }

            Vector3 direction = delta / chord;
            Vector3 normal = new Vector3(-direction.y, direction.x, 0f);
            Vector3 controlA = a + delta * 0.33f + normal * curveHeight;
            Vector3 controlB = a + delta * 0.66f - normal * curveHeight * 0.6f;
            curveLength = 0f;
            Vector3 previous = a;

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float inverse = 1f - t;
                Vector3 point = inverse * inverse * inverse * a
                    + 3f * inverse * inverse * t * controlA
                    + 3f * inverse * t * t * controlB
                    + t * t * t * b;
                curvePoints.Add(point);
                if (i > 0)
                    curveLength += Vector3.Distance(previous, point);
                previous = point;
            }
        }

        private void BuildVisuals(int dashCount)
        {
            ClearVisuals();
            for (int i = 0; i < curvePoints.Count - 1; i++)
                trackImages.Add(CreateImage("Continuous Path Track " + i, trackSprite, Vector2.one));

            for (int i = 0; i < dashCount; i++)
                glowImages.Add(CreateImage("Path Glow " + i, unlockedGlow, new Vector2(dashWidth * 1.45f, dashHeight * 2f)));

            // Keep every glow underneath every dash so overlapping glows never soften the route texture.
            for (int i = 0; i < dashCount; i++)
                dashImages.Add(CreateImage("Path Dash " + i, unlockedDash, new Vector2(dashWidth, dashHeight)));

            fromKnot = CreateImage("Path Knot - Start", unlockedKnot, new Vector2(18f, 18f));
            toKnot = CreateImage("Path Knot - End", unlockedKnot, new Vector2(18f, 18f));
            midpointLock = CreateImage("Path Midpoint Lock", lockedMidpoint, new Vector2(32f, 32f));
            spark = CreateImage("Path Traveling Spark", goldSpark, new Vector2(24f, 24f));
            visualsBuilt = true;
        }

        private Image CreateImage(string objectName, Sprite sprite, Vector2 size)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(transform, false);
            var rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;

            var image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = false;
            image.raycastTarget = false;
            return image;
        }

        private void ClearVisuals()
        {
            // Only generated visual children belong here; path containers have no authored children.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }

            dashImages.Clear();
            glowImages.Clear();
            trackImages.Clear();
            fromKnot = null;
            toKnot = null;
            midpointLock = null;
            spark = null;
            visualsBuilt = false;
        }

        private void ApplyStateVisuals()
        {
            if (!visualsBuilt)
                return;

            Sprite dashSprite = state == PathState.Locked ? lockedDash
                : state == PathState.Cleared ? clearedDash : unlockedDash;
            Sprite knotSprite = state == PathState.Locked ? lockedKnot
                : state == PathState.Cleared ? clearedKnot : unlockedKnot;
            Sprite glowSprite = state == PathState.Cleared ? clearedGlow : unlockedGlow;
            Color lineColor = state == PathState.Locked ? new Color(0.76f, 0.77f, 0.84f, 0.92f) : Color.white;
            Color glowColor = state == PathState.Cleared
                ? new Color(0.45f, 1f, 0.47f, 0.22f)
                : new Color(1f, 0.72f, 0.24f, 0.18f);
            Color trackColor = state == PathState.Locked
                ? new Color(0.35f, 0.37f, 0.49f, 0.58f)
                : state == PathState.Cleared
                    ? new Color(0.40f, 0.88f, 0.43f, 0.72f)
                    : new Color(0.92f, 0.58f, 0.22f, 0.78f);

            foreach (Image track in trackImages)
            {
                track.sprite = trackSprite;
                track.color = trackColor;
            }

            for (int i = 0; i < dashImages.Count; i++)
            {
                dashImages[i].sprite = dashSprite;
                dashImages[i].color = lineColor;
                glowImages[i].sprite = glowSprite;
                glowImages[i].color = glowColor;
                glowImages[i].gameObject.SetActive(state != PathState.Locked);
            }

            fromKnot.sprite = knotSprite;
            toKnot.sprite = knotSprite;
            midpointLock.gameObject.SetActive(state == PathState.Locked);
            spark.sprite = state == PathState.Cleared ? greenSpark : goldSpark;
            spark.gameObject.SetActive(state != PathState.Locked);
        }

        private void PositionDashes()
        {
            if (visibleLength <= 0f)
                return;

            float spacing = dashWidth + dashGap;
            for (int i = 0; i < dashImages.Count; i++)
            {
                float distance = visibleStart + Mathf.Repeat(i * spacing + flowOffset, visibleLength);
                PlaceOnCurve(dashImages[i].rectTransform, distance, dashWidth, dashHeight);
                PlaceOnCurve(glowImages[i].rectTransform, distance, dashWidth * 1.45f, dashHeight * 2f);
            }
        }

        private void PositionTrack()
        {
            int count = Mathf.Min(trackImages.Count, curvePoints.Count - 1);
            for (int i = 0; i < count; i++)
            {
                Vector3 start = curvePoints[i];
                Vector3 end = curvePoints[i + 1];
                Vector3 difference = end - start;
                RectTransform rect = trackImages[i].rectTransform;
                rect.anchoredPosition = (start + end) * 0.5f;
                rect.sizeDelta = new Vector2(difference.magnitude + 2f, 3.5f);
                rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg);
            }
        }

        private void PositionKnot(Image knot, float distance)
        {
            if (knot != null)
            {
                EvaluateAtDistance(distance, out Vector3 point, out _);
                knot.rectTransform.anchoredPosition = point;
                knot.rectTransform.sizeDelta = new Vector2(18f, 18f);
                knot.rectTransform.localRotation = Quaternion.identity;
            }
        }

        private void PositionMidpointLock()
        {
            if (midpointLock != null)
            {
                EvaluateAtDistance(visibleStart + visibleLength * 0.5f, out Vector3 point, out _);
                midpointLock.rectTransform.anchoredPosition = point;
                midpointLock.rectTransform.sizeDelta = new Vector2(32f, 32f);
                midpointLock.rectTransform.localRotation = Quaternion.identity;
            }
        }

        private void PositionSpark(float distanceOffset)
        {
            if (spark != null && visibleLength > 0f)
                PlaceOnCurve(spark.rectTransform, visibleStart + Mathf.Repeat(distanceOffset, visibleLength), 24f, 24f);
        }

        private void PlaceOnCurve(RectTransform rect, float distance, float width, float height)
        {
            EvaluateAtDistance(distance, out Vector3 point, out Vector3 tangent);
            rect.anchoredPosition = point;
            rect.sizeDelta = new Vector2(width, height);
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg);
        }

        private void EvaluateAtDistance(float distance, out Vector3 point, out Vector3 tangent)
        {
            float travelled = 0f;
            for (int i = 1; i < curvePoints.Count; i++)
            {
                Vector3 a = curvePoints[i - 1];
                Vector3 b = curvePoints[i];
                float segmentLength = Vector3.Distance(a, b);
                if (travelled + segmentLength >= distance)
                {
                    float t = segmentLength > 0f ? (distance - travelled) / segmentLength : 0f;
                    point = Vector3.Lerp(a, b, t);
                    tangent = (b - a).normalized;
                    return;
                }
                travelled += segmentLength;
            }

            point = curvePoints.Count > 0 ? curvePoints[curvePoints.Count - 1] : Vector3.zero;
            tangent = Vector3.right;
        }
    }
}
