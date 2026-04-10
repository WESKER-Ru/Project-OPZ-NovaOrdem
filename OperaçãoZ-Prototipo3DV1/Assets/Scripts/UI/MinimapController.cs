// Assets/Scripts/UI/MinimapController.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using OPZ.Core;

namespace OPZ.UI
{
    /// <summary>
    /// Setup:
    /// 1. Create a secondary Camera ("MinimapCamera") looking straight down (rotation 90,0,0), orthographic.
    ///    Set it to render to a RenderTexture (e.g. 256x256).
    /// 2. In the Canvas, create a RawImage for the minimap and assign the RenderTexture.
    /// 3. Assign references below.
    /// Click on the minimap RawImage to move the main camera.
    /// </summary>
    public class MinimapController : MonoBehaviour
    {
        [SerializeField] Camera minimapCamera;
        [SerializeField] RawImage minimapImage;

        [Header("Map Bounds (match camera bounds)")]
        [SerializeField] Vector2 mapMin = new(-150, -150);
        [SerializeField] Vector2 mapMax = new(150, 150);

        RectTransform _rect;

        void Start()
        {
            if (minimapImage != null)
                _rect = minimapImage.rectTransform;
        }

        void Update()
        {
            if (_rect == null || minimapImage == null) return;

            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;

            // Check if click is on minimap
            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rect, mouse.position.ReadValue(), null, out localPoint))
                return;

            Rect rect = _rect.rect;
            if (!rect.Contains(localPoint)) return;

            // Normalize to 0-1
            float nx = (localPoint.x - rect.x) / rect.width;
            float ny = (localPoint.y - rect.y) / rect.height;

            // Map to world
            float wx = Mathf.Lerp(mapMin.x, mapMax.x, nx);
            float wz = Mathf.Lerp(mapMin.y, mapMax.y, ny);

            var cam = FindAnyObjectByType<RTSCameraController>();
            if (cam != null) cam.FocusOn(new Vector3(wx, 0, wz));
        }
    }
}
