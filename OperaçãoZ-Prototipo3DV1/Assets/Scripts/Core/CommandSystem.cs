// Assets/Scripts/Core/CommandSystem.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using OPZ.Units;
using OPZ.Economy;
using OPZ.Building;
using OPZ.Combat;

namespace OPZ.Core
{
    public class CommandSystem : MonoBehaviour
    {
        public static CommandSystem Instance { get; private set; }

        [SerializeField] LayerMask groundLayer;
        [SerializeField] LayerMask unitLayer;
        [SerializeField] LayerMask resourceLayer;
        [SerializeField] LayerMask buildingLayer;

        [Header("Feedback")]
        [SerializeField] GameObject moveMarkerPrefab;
        [SerializeField] GameObject attackMarkerPrefab;

        Camera _cam;

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

            if (mouse.rightButton.wasPressedThisFrame)
                HandleRightClick(mouse.position.ReadValue());

            // Stop command
            if (Keyboard.current.sKey.wasPressedThisFrame)
                StopSelected();

            // Control groups 1-9
            for (int i = 1; i <= 9; i++)
            {
                Key key = (Key)((int)Key.Digit1 + (i - 1));
                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    if (Keyboard.current.leftCtrlKey.isPressed)
                        SelectionManager.Instance.AssignGroup(i);
                    else
                        SelectionManager.Instance.RecallGroup(i);
                }
            }
        }

        void HandleRightClick(Vector2 screenPos)
        {
            var selected = SelectionManager.Instance.SelectedUnits;
            if (selected.Count == 0) return;

            Ray ray = _cam.ScreenPointToRay(screenPos);

            // Priority 1: clicked enemy unit → attack
            if (Physics.Raycast(ray, out RaycastHit hitUnit, 500f, unitLayer))
            {
                var target = hitUnit.collider.GetComponentInParent<UnitBase>();
                if (target != null && target.Faction != GameManager.Instance.playerFaction)
                {
                    foreach (var u in selected)
                    {
                        if (u is CombatUnit cu)
                            cu.CommandAttack(target);
                    }
                    SpawnMarker(attackMarkerPrefab, hitUnit.point);
                    return;
                }
            }

            // Priority 2: clicked enemy building → attack
            if (Physics.Raycast(ray, out RaycastHit hitBld, 500f, buildingLayer))
            {
                var bldg = hitBld.collider.GetComponentInParent<BuildingBase>();
                if (bldg != null && bldg.Faction != GameManager.Instance.playerFaction)
                {
                    foreach (var u in selected)
                    {
                        if (u is CombatUnit cu)
                            cu.CommandAttackBuilding(bldg);
                    }
                    SpawnMarker(attackMarkerPrefab, hitBld.point);
                    return;
                }
            }

            // Priority 3: clicked resource → gather (workers only)
            if (Physics.Raycast(ray, out RaycastHit hitRes, 500f, resourceLayer))
            {
                var node = hitRes.collider.GetComponentInParent<ResourceNode>();
                if (node != null)
                {
                    foreach (var u in selected)
                    {
                        if (u is WorkerUnit w)
                            w.CommandGather(node);
                    }
                    return;
                }
            }

            // Priority 4: clicked own building under construction → build (workers)
            if (Physics.Raycast(ray, out RaycastHit hitOwnBld, 500f, buildingLayer))
            {
                var ownBld = hitOwnBld.collider.GetComponentInParent<BuildingBase>();
                if (ownBld != null && ownBld.Faction == GameManager.Instance.playerFaction
                    && ownBld.State == Data.BuildingState.UnderConstruction)
                {
                    foreach (var u in selected)
                    {
                        if (u is WorkerUnit w)
                            w.CommandBuild(ownBld);
                    }
                    return;
                }
            }

            // Priority 5: move (project click to nearest NavMesh point)
            if (TryResolveMovePoint(ray, out Vector3 movePoint))
            {
                foreach (var u in selected)
                    u.CommandMove(movePoint);
                SpawnMarker(moveMarkerPrefab, movePoint);
            }
        }

        bool TryResolveMovePoint(Ray ray, out Vector3 movePoint)
        {
            movePoint = default;

            Vector3 candidate;
            if (Physics.Raycast(ray, out RaycastHit hitGround, 1000f, groundLayer, QueryTriggerInteraction.Ignore))
            {
                candidate = hitGround.point;
            }
            else if (Physics.Raycast(ray, out RaycastHit hitAny, 1000f, ~0, QueryTriggerInteraction.Ignore))
            {
                candidate = hitAny.point;
            }
            else
            {
                var plane = new Plane(Vector3.up, Vector3.zero);
                if (!plane.Raycast(ray, out float enter)) return false;
                candidate = ray.GetPoint(enter);
            }

            if (NavMesh.SamplePosition(candidate, out NavMeshHit navHit, 30f, NavMesh.AllAreas))
            {
                movePoint = navHit.position;
                return true;
            }

            return false;
        }

        void StopSelected()
        {
            foreach (var u in SelectionManager.Instance.SelectedUnits)
                u.CommandStop();
        }

        void SpawnMarker(GameObject prefab, Vector3 pos)
        {
            if (prefab == null) return;
            var go = Instantiate(prefab, pos + Vector3.up * 0.1f, Quaternion.identity);
            Destroy(go, 1f);
        }
    }
}
