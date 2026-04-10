// Assets/Scripts/Core/SelectionManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using OPZ.Units;
using OPZ.Building;
using OPZ.Data;

namespace OPZ.Core
{
    public class SelectionManager : MonoBehaviour
    {
        public static SelectionManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] LayerMask selectableLayer;
        [SerializeField] LayerMask groundLayer;
        [SerializeField] float dragThreshold = 5f;

        public IReadOnlyList<UnitBase> SelectedUnits => _selectedUnits;
        public BuildingBase SelectedBuilding { get; private set; }

        readonly List<UnitBase> _selectedUnits = new();

        Camera _cam;
        Vector2 _dragStart;
        bool _isDragging;

        // Events
        public event System.Action OnSelectionChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start() => _cam = Camera.main;

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.MatchOver) return;

            var mouse = Mouse.current;
            if (mouse == null) return;

            // Left click down
            if (mouse.leftButton.wasPressedThisFrame)
            {
                _dragStart = mouse.position.ReadValue();
                _isDragging = false;
            }

            // Dragging
            if (mouse.leftButton.isPressed)
            {
                Vector2 current = mouse.position.ReadValue();
                if (Vector2.Distance(_dragStart, current) > dragThreshold)
                    _isDragging = true;
            }

            // Left click up
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                if (_isDragging)
                    BoxSelect(mouse.position.ReadValue());
                else
                    ClickSelect(mouse.position.ReadValue());
            }
        }

        void ClickSelect(Vector2 screenPos)
        {
            Ray ray = _cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 500f, selectableLayer))
            {
                // Try unit
                var unit = hit.collider.GetComponentInParent<UnitBase>();
                if (unit != null && unit.Faction == GameManager.Instance.playerFaction)
                {
                    bool additive = Keyboard.current.leftShiftKey.isPressed;
                    if (!additive) ClearSelection();
                    SelectUnit(unit);
                    return;
                }

                // Try building
                var building = hit.collider.GetComponentInParent<BuildingBase>();
                if (building != null && building.Faction == GameManager.Instance.playerFaction)
                {
                    ClearSelection();
                    SelectedBuilding = building;
                    OnSelectionChanged?.Invoke();
                    return;
                }
            }

            // Clicked nothing
            if (!Keyboard.current.leftShiftKey.isPressed)
                ClearSelection();
        }

        void BoxSelect(Vector2 endPos)
        {
            if (!Keyboard.current.leftShiftKey.isPressed)
                ClearSelection();

            Vector2 min = Vector2.Min(_dragStart, endPos);
            Vector2 max = Vector2.Max(_dragStart, endPos);
            Rect screenRect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

            Faction player = GameManager.Instance.playerFaction;
            foreach (var unit in GameManager.Instance.GetUnits(player))
            {
                Vector3 sp = _cam.WorldToScreenPoint(unit.transform.position);
                if (sp.z > 0 && screenRect.Contains(new Vector2(sp.x, sp.y)))
                    SelectUnit(unit);
            }
        }

        void SelectUnit(UnitBase unit)
        {
            if (!_selectedUnits.Contains(unit))
            {
                _selectedUnits.Add(unit);
                unit.SetSelected(true);
            }
            SelectedBuilding = null;
            OnSelectionChanged?.Invoke();
        }

        public void ClearSelection()
        {
            foreach (var u in _selectedUnits) u.SetSelected(false);
            _selectedUnits.Clear();
            SelectedBuilding = null;
            OnSelectionChanged?.Invoke();
        }

        // --- Control Groups (1-9) ---
        readonly Dictionary<int, List<UnitBase>> _controlGroups = new();

        public void AssignGroup(int group)
        {
            _controlGroups[group] = new List<UnitBase>(_selectedUnits);
        }

        public void RecallGroup(int group)
        {
            if (!_controlGroups.TryGetValue(group, out var units)) return;
            ClearSelection();
            foreach (var u in units)
            {
                if (u != null && u.IsAlive)
                    SelectUnit(u);
            }
        }

        // --- Box Selection Visual (OnGUI) ---
        void OnGUI()
        {
            if (!_isDragging || !Mouse.current.leftButton.isPressed) return;
            Vector2 current = Mouse.current.position.ReadValue();
            Vector2 start = new(_dragStart.x, Screen.height - _dragStart.y);
            Vector2 end = new(current.x, Screen.height - current.y);
            Rect rect = new(start, end - start);
            GUI.Box(rect, "", GetBoxStyle());
        }

        static GUIStyle _boxStyle;
        static GUIStyle GetBoxStyle()
        {
            if (_boxStyle != null) return _boxStyle;
            _boxStyle = new GUIStyle(GUI.skin.box);
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, new Color(0.2f, 0.8f, 0.2f, 0.25f));
            tex.Apply();
            _boxStyle.normal.background = tex;
            return _boxStyle;
        }
    }
}
