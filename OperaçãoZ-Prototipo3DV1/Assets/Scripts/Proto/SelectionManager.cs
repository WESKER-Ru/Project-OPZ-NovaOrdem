// Assets/Scripts/Proto/SelectionManager.cs
// Gerencia seleção de unidades: clique individual e box select.
// Coloque em um GameObject vazio chamado "SelectionManager" na scene.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OPZ.Proto
{
    [System.Obsolete("Legacy prototype selector. Use OPZ.Core.SelectionManager in runtime scenes.", false)]
    [AddComponentMenu("OPZ/Proto/LEGACY SelectionManager (Do Not Use)")]
    public class SelectionManager : MonoBehaviour
    {
        [Header("Legacy")]
        [SerializeField] bool showLegacyWarning;

        [Header("Config")]
        public LayerMask selectableLayer; // layer das unidades
        public float dragThreshold = 8f;  // pixels antes de virar box select

        // Lista pública de leitura para outros sistemas
        public List<Selectable> Selected { get; private set; } = new();

        Camera _cam;
        Vector2 _mouseDownPos;
        bool _isDragging;

        void Awake()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (showLegacyWarning)
                Debug.LogWarning("[Proto.SelectionManager] Legacy prototype component active. Prefer OPZ.Core.SelectionManager.", this);
#endif
        }

        void Start()
        {
            _cam = Camera.main;
        }

        void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null || _cam == null) return;

            // Mouse down — marca posição inicial
            if (mouse.leftButton.wasPressedThisFrame)
            {
                _mouseDownPos = mouse.position.ReadValue();
                _isDragging = false;
            }

            // Mouse held — detecta drag
            if (mouse.leftButton.isPressed)
            {
                if (Vector2.Distance(_mouseDownPos, mouse.position.ReadValue()) > dragThreshold)
                    _isDragging = true;
            }

            // Mouse up — executa seleção
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                bool additive = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

                if (_isDragging)
                    BoxSelect(mouse.position.ReadValue(), additive);
                else
                    ClickSelect(mouse.position.ReadValue(), additive);
            }
        }

        // ─── CLICK SELECT ───
        void ClickSelect(Vector2 screenPos, bool additive)
        {
            Ray ray = _cam.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit, 500f, selectableLayer))
            {
                var sel = hit.collider.GetComponentInParent<Selectable>();
                if (sel != null)
                {
                    if (!additive) ClearSelection();
                    if (!Selected.Contains(sel))
                    {
                        Selected.Add(sel);
                        sel.Select();
                    }
                    return;
                }
            }

            // Clicou no vazio
            if (!additive) ClearSelection();
        }

        // ─── BOX SELECT ───
        void BoxSelect(Vector2 endPos, bool additive)
        {
            if (!additive) ClearSelection();

            // Retângulo na tela
            Vector2 min = Vector2.Min(_mouseDownPos, endPos);
            Vector2 max = Vector2.Max(_mouseDownPos, endPos);
            Rect screenRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

            // Testa todos os Selectable na scene
            foreach (var sel in FindObjectsByType<Selectable>(FindObjectsSortMode.None))
            {
                Vector3 screenPoint = _cam.WorldToScreenPoint(sel.transform.position);
                if (screenPoint.z > 0 && screenRect.Contains(new Vector2(screenPoint.x, screenPoint.y)))
                {
                    if (!Selected.Contains(sel))
                    {
                        Selected.Add(sel);
                        sel.Select();
                    }
                }
            }
        }

        // ─── CLEAR ───
        public void ClearSelection()
        {
            foreach (var s in Selected) s.Deselect();
            Selected.Clear();
        }

        // ─── VISUAL DO BOX SELECT (retângulo verde na tela) ───
        void OnGUI()
        {
            if (!_isDragging || Mouse.current == null || !Mouse.current.leftButton.isPressed) return;

            Vector2 current = Mouse.current.position.ReadValue();
            // GUI usa Y invertido
            Vector2 start = new(_mouseDownPos.x, Screen.height - _mouseDownPos.y);
            Vector2 end = new(current.x, Screen.height - current.y);

            Rect rect = new(start, end - start);
            GUI.Box(rect, "", BoxStyle());
        }

        static GUIStyle _style;
        static GUIStyle BoxStyle()
        {
            if (_style != null) return _style;
            _style = new GUIStyle(GUI.skin.box);
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, new Color(0.2f, 0.8f, 0.2f, 0.25f));
            tex.Apply();
            _style.normal.background = tex;
            return _style;
        }
    }
}
