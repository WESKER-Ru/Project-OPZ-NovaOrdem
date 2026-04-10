// Assets/Scripts/Core/SimpleRTSCamera.cs
using UnityEngine;
using UnityEngine.InputSystem;

namespace OPZ.Core
{
    [System.Obsolete("Legacy camera prototype. Use OPZ.Core.RTSCameraController in runtime scenes.", false)]
    [AddComponentMenu("OPZ/Core/LEGACY Simple RTS Camera (Do Not Use)")]
    public class SimpleRTSCamera : MonoBehaviour
    {
        [Header("Pan")]
        public float panSpeed = 50f;
        public float edgePanMargin = 15f;

        [Header("Zoom")]
        public float zoomSpeed = 300f;
        public float minHeight = 20f;
        public float maxHeight = 180f;

        [Header("Rotation")]
        public float rotateSpeed = 120f;

        [Header("Pitch (angulo da camera)")]
        public float minPitch = 40f;  // mais de lado (zoom in)
        public float maxPitch = 70f;  // mais de cima (zoom out)

        [Header("Bounds")]
        public float boundsX = 160f;
        public float boundsZ = 160f;

        void Awake()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning("[SimpleRTSCamera] Legacy camera component active. Prefer OPZ.Core.RTSCameraController.", this);
#endif
        }

        void LateUpdate()
        {
            var kb = Keyboard.current;
            var mouse = Mouse.current;
            if (kb == null || mouse == null) return;

            // ─── PAN (WASD + edge) ───
            Vector3 move = Vector3.zero;
            Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 right = transform.right;

            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    move += fwd;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  move -= fwd;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move += right;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  move -= right;

            Vector2 mp = mouse.position.ReadValue();
            if (mp.x < edgePanMargin && mp.x >= 0) move -= right;
            if (mp.x > Screen.width - edgePanMargin && mp.x <= Screen.width) move += right;
            if (mp.y < edgePanMargin && mp.y >= 0) move -= fwd;
            if (mp.y > Screen.height - edgePanMargin && mp.y <= Screen.height) move += fwd;

            float hFactor = Mathf.Lerp(0.5f, 1.8f, Mathf.InverseLerp(minHeight, maxHeight, transform.position.y));
            if (move.sqrMagnitude > 0.001f)
                transform.position += panSpeed * hFactor * Time.unscaledDeltaTime * move.normalized;

            // ─── ZOOM (scroll) ───
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                Vector3 pos = transform.position;
                pos.y -= scroll * zoomSpeed * Time.unscaledDeltaTime;
                pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);
                transform.position = pos;
            }

            // ─── ROTATE (middle mouse) ───
            if (mouse.middleButton.isPressed)
            {
                float dx = mouse.delta.ReadValue().x;
                transform.Rotate(Vector3.up, dx * rotateSpeed * Time.unscaledDeltaTime, Space.World);
            }

            // ─── CLAMP ───
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, -boundsX, boundsX);
            p.z = Mathf.Clamp(p.z, -boundsZ, boundsZ);
            transform.position = p;

            // ─── PITCH: mais de lado quando perto, mais de cima quando longe ───
            float t = Mathf.InverseLerp(minHeight, maxHeight, transform.position.y);
            Vector3 euler = transform.eulerAngles;
            euler.x = Mathf.Lerp(minPitch, maxPitch, t);
            transform.eulerAngles = euler;
        }
    }
}
