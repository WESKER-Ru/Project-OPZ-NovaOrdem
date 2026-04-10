using UnityEngine;
using UnityEngine.InputSystem;

namespace OPZ.Core
{
    public class RTSCameraController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float panSpeed = 30f;
        [SerializeField] private float edgePanSize = 15f;
        [SerializeField] private bool useEdgePan = true;
        [SerializeField] private float keyboardPanMultiplier = 1.2f;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 8f;
        [SerializeField] private float minHeight = 25f;
        [SerializeField] private float maxHeight = 140f;

        [Header("Rotation")]
        [SerializeField] private float rotateSpeed = 120f;

        [Header("Bounds")]
        [SerializeField] private Vector2 mapMin = new(-150f, -150f);
        [SerializeField] private Vector2 mapMax = new(150f, 150f);

        [Header("Look Angle")]
        [SerializeField] private float minPitch = 40f;
        [SerializeField] private float maxPitch = 75f;

        private Transform _rig;
        private float _currentZoom;

        private void Awake()
        {
            _rig = transform;
            _currentZoom = _rig.position.y;
        }

        private void LateUpdate()
        {
            HandlePan();
            HandleZoom();
            HandleRotation();
            ClampPosition();
            UpdatePitch();
        }

        private void HandlePan()
        {
            Vector3 move = Vector3.zero;
            Vector3 forward = Vector3.ProjectOnPlane(_rig.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(_rig.right, Vector3.up).normalized;

            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) move += forward;
                if ((kb.sKey.isPressed && !kb.leftCtrlKey.isPressed) || kb.downArrowKey.isPressed) move -= forward;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move += right;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move -= right;

                move *= keyboardPanMultiplier;
            }

            if (useEdgePan)
            {
                var mouse = Mouse.current;
                if (mouse != null)
                {
                    Vector2 mp = mouse.position.ReadValue();

                    if (mp.x < edgePanSize) move -= right;
                    if (mp.x > Screen.width - edgePanSize) move += right;
                    if (mp.y < edgePanSize) move -= forward;
                    if (mp.y > Screen.height - edgePanSize) move += forward;
                }
            }

            if (move.sqrMagnitude > 0.01f)
            {
                float speedMod = Mathf.Lerp(0.65f, 1.35f, Mathf.InverseLerp(minHeight, maxHeight, _currentZoom));
                _rig.position += panSpeed * speedMod * Time.unscaledDeltaTime * move.normalized;
            }
        }

        private void HandleZoom()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) < 0.01f) return;

            _currentZoom -= scroll * zoomSpeed * Time.unscaledDeltaTime * 10f;
            _currentZoom = Mathf.Clamp(_currentZoom, minHeight, maxHeight);

            Vector3 pos = _rig.position;
            pos.y = Mathf.Lerp(pos.y, _currentZoom, Time.unscaledDeltaTime * 12f);
            _rig.position = pos;
        }

        private void HandleRotation()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.middleButton.isPressed) return;

            float delta = mouse.delta.ReadValue().x;
            _rig.Rotate(Vector3.up, delta * rotateSpeed * Time.unscaledDeltaTime, Space.World);
        }

        private void ClampPosition()
        {
            Vector3 p = _rig.position;
            p.x = Mathf.Clamp(p.x, mapMin.x, mapMax.x);
            p.z = Mathf.Clamp(p.z, mapMin.y, mapMax.y);
            p.y = Mathf.Clamp(p.y, minHeight, maxHeight);
            _rig.position = p;
        }

        private void UpdatePitch()
        {
            float t = Mathf.InverseLerp(minHeight, maxHeight, _currentZoom);
            float pitch = Mathf.Lerp(minPitch, maxPitch, t);

            Vector3 euler = _rig.eulerAngles;
            euler.x = pitch;
            euler.z = 0f;
            _rig.eulerAngles = euler;
        }

        public void FocusOn(Vector3 worldPos)
        {
            Vector3 p = _rig.position;
            p.x = worldPos.x;
            p.z = worldPos.z;
            _rig.position = p;
        }
    }
}
