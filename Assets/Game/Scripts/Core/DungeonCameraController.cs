using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class DungeonCameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0.1f)] private float followSpeed = 7f;
        [SerializeField, Min(0.1f)] private float defaultZoom = 9f;
        [SerializeField, Min(0.1f)] private float minZoom = 5f;
        [SerializeField, Min(0.1f)] private float maxZoom = 15f;
        [SerializeField, Min(0.1f)] private float zoomSpeed = 4f;

        private Camera controlledCamera;

        private void Awake()
        {
            controlledCamera = GetComponent<Camera>();
            controlledCamera.orthographic = true;
            controlledCamera.orthographicSize = Mathf.Clamp(defaultZoom, minZoom, maxZoom);

            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    target = player.transform;
            }
        }

        private void LateUpdate()
        {
            if (target != null)
            {
                Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
                transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                float zoom = controlledCamera.orthographicSize - scroll * zoomSpeed;
                controlledCamera.orthographicSize = Mathf.Clamp(zoom, minZoom, maxZoom);
            }
        }
    }
}
