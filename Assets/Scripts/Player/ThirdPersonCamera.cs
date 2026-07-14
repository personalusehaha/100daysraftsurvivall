using UnityEngine;

namespace RaftSurvival.Player
{
    /// <summary>
    /// Third-person camera that orbits and follows the player. Supports
    /// touch-drag input for Android (single finger drag = rotate camera).
    /// Attach to an empty "CameraRig" GameObject with the Main Camera as
    /// a child, or directly control the Main Camera's transform.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target; // player root transform
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

        [Header("Orbit Settings")]
        [SerializeField] private float distance = 5f;
        [SerializeField] private float minDistance = 2f;
        [SerializeField] private float maxDistance = 8f;
        [SerializeField] private float rotationSpeed = 0.25f;
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 70f;

        [Header("Smoothing")]
        [SerializeField] private float positionSmoothTime = 0.08f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionMask;
        [SerializeField] private float collisionRadius = 0.3f;

        private float yaw = 0f;
        private float pitch = 20f;
        private Vector3 velocityRef;

        private Vector2 lastTouchPos;
        private bool isDragging;

        private void LateUpdate()
        {
            if (target == null) return;

            HandleTouchInput();
            UpdateCameraPosition();
        }

        private void HandleTouchInput()
        {
            // Mobile single-finger drag to orbit camera.
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        lastTouchPos = touch.position;
                        isDragging = true;
                        break;

                    case TouchPhase.Moved:
                        if (isDragging)
                        {
                            Vector2 delta = touch.position - lastTouchPos;
                            yaw += delta.x * rotationSpeed;
                            pitch -= delta.y * rotationSpeed;
                            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
                            lastTouchPos = touch.position;
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        isDragging = false;
                        break;
                }
            }
            else
            {
                // Editor/desktop testing fallback with mouse.
                if (Input.GetMouseButton(0))
                {
                    yaw += Input.GetAxis("Mouse X") * rotationSpeed * 20f;
                    pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * 20f;
                    pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
                }
            }
        }

        private void UpdateCameraPosition()
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPosition = target.position + targetOffset - (rotation * Vector3.forward * distance);

            // Simple collision check so camera doesn't clip through raft/terrain.
            Vector3 pivot = target.position + targetOffset;
            if (Physics.SphereCast(pivot, collisionRadius, (desiredPosition - pivot).normalized,
                out RaycastHit hit, distance, collisionMask))
            {
                desiredPosition = pivot + (desiredPosition - pivot).normalized * hit.distance;
            }

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocityRef, positionSmoothTime);
            transform.LookAt(pivot);
        }

        public void SetTarget(Transform newTarget) => target = newTarget;
    }
}
