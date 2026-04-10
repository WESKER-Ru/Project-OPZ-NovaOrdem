// Assets/Scripts/Building/BuildPlacementController.cs
using UnityEngine;
using UnityEngine.InputSystem;
using OPZ.Data;
using OPZ.Economy;
using OPZ.Core;

namespace OPZ.Building
{
    /// <summary>
    /// Handles the "place building" flow:
    /// 1. Player selects building from build panel → EnterPlacementMode(def)
    /// 2. Ghost follows mouse on ground
    /// 3. Left click → validate position + deduct cost → spawn foundation
    /// 4. Right click or Escape → cancel
    /// </summary>
    public class BuildPlacementController : MonoBehaviour
    {
        public static BuildPlacementController Instance { get; private set; }

        [SerializeField] LayerMask groundLayer;
        [SerializeField] Material ghostValidMat;
        [SerializeField] Material ghostInvalidMat;

        public bool IsPlacing { get; private set; }

        BuildingDef _currentDef;
        GameObject _ghostInstance;
        bool _validPosition;
        Camera _cam;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start() => _cam = Camera.main;

        void Update()
        {
            if (!IsPlacing) return;

            UpdateGhostPosition();

            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame && _validPosition)
                ConfirmPlacement();

            if (mouse.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
                CancelPlacement();
        }

        // --- Public API ---
        public void EnterPlacementMode(BuildingDef def)
        {
            if (IsPlacing) CancelPlacement();

            Faction f = GameManager.Instance.playerFaction;
            if (!EconomyManager.Instance.CanAfford(f, def.suppliesCost, def.metalCost, def.fuelCost))
            {
                Debug.Log("[Build] Cannot afford " + def.buildingName);
                return;
            }

            _currentDef = def;
            _ghostInstance = Instantiate(def.prefab);
            // Disable all colliders on ghost
            foreach (var col in _ghostInstance.GetComponentsInChildren<Collider>())
                col.enabled = false;
            // Disable scripts
            var bb = _ghostInstance.GetComponent<BuildingBase>();
            if (bb != null) bb.enabled = false;

            IsPlacing = true;
        }

        public void CancelPlacement()
        {
            if (_ghostInstance != null) Destroy(_ghostInstance);
            _ghostInstance = null;
            _currentDef = null;
            IsPlacing = false;
        }

        // --- Internal ---
        void UpdateGhostPosition()
        {
            Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundLayer))
            {
                _ghostInstance.transform.position = hit.point;
                _validPosition = CheckValidPlacement(hit.point);
                // TODO: tint ghost material based on validity
            }
        }

        bool CheckValidPlacement(Vector3 pos)
        {
            // Simple overlap check — no other building nearby
            float radius = _currentDef.footprintRadius;
            Collider[] hits = Physics.OverlapSphere(pos + Vector3.up, radius, ~groundLayer);
            foreach (var h in hits)
            {
                if (h.GetComponentInParent<BuildingBase>() != null) return false;
            }
            return true;
        }

        void ConfirmPlacement()
        {
            Faction f = GameManager.Instance.playerFaction;
            if (!EconomyManager.Instance.TrySpend(f, _currentDef.suppliesCost, _currentDef.metalCost, _currentDef.fuelCost))
            {
                Debug.Log("[Build] Cannot afford (race condition check).");
                return;
            }

            // Convert ghost to real building
            Vector3 pos = _ghostInstance.transform.position;
            Quaternion rot = _ghostInstance.transform.rotation;
            Destroy(_ghostInstance);

            GameObject bldgGO = Instantiate(_currentDef.prefab, pos, rot);
            var bldg = bldgGO.GetComponent<BuildingBase>();
            bldg.InitBuilding(_currentDef, f);
            bldg.PlaceAsFoundation();

            // Re-enable colliders
            foreach (var col in bldgGO.GetComponentsInChildren<Collider>())
                col.enabled = true;

            IsPlacing = false;
            _currentDef = null;

            // Auto-send selected workers to build
            var selected = SelectionManager.Instance.SelectedUnits;
            foreach (var u in selected)
            {
                if (u is Units.WorkerUnit w)
                    w.CommandBuild(bldg);
            }
        }
    }
}
