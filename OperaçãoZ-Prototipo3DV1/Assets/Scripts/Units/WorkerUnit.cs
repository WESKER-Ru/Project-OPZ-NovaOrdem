// Assets/Scripts/Units/WorkerUnit.cs
using UnityEngine;
using OPZ.Data;
using OPZ.Economy;
using OPZ.Building;
using OPZ.Core;
using OPZ.Combat;

namespace OPZ.Units
{
    public class WorkerUnit : UnitBase
    {
        [Header("Worker")]
        [SerializeField] float gatherTickInterval = 1f;
        [SerializeField] float fleeRadius = 8f;
        [SerializeField] float fleeDuration = 3f;

        int _carried;
        ResourceType _carriedType;
        ResourceNode _targetNode;
        BuildingBase _targetBuilding;
        float _gatherTimer;
        float _fleeTimer;

        // Remember previous command for ResumePreviousCommand
        UnitState _previousState;
        ResourceNode _previousNode;

        public int Carried => _carried;
        public ResourceType CarriedType => _carriedType;

        void Update()
        {
            if (!IsAlive) return;

            switch (CurrentState)
            {
                case UnitState.Idle:
                    CheckThreatsForFlee();
                    break;
                case UnitState.Move:
                    if (HasReachedDestination()) CurrentState = UnitState.Idle;
                    CheckThreatsForFlee();
                    break;
                case UnitState.Gather:
                    UpdateGather();
                    break;
                case UnitState.ReturnToDepot:
                    UpdateReturn();
                    break;
                case UnitState.Build:
                    UpdateBuild();
                    break;
                case UnitState.Flee:
                    UpdateFlee();
                    break;
                case UnitState.ResumePreviousCommand:
                    ResumeCommand();
                    break;
            }
        }

        // --- Commands ---
        public void CommandGather(ResourceNode node)
        {
            if (node == null || node.IsDepleted) return;
            _targetNode = node;
            _carriedType = node.Type;
            CurrentState = UnitState.Gather;
            MoveTo(node.GatherPoint);
        }

        public void CommandBuild(BuildingBase building)
        {
            if (building == null) return;
            _targetBuilding = building;
            CurrentState = UnitState.Build;
            MoveTo(building.transform.position);
        }

        // --- Gather Logic ---
        void UpdateGather()
        {
            if (_targetNode == null || _targetNode.IsDepleted)
            {
                // Node gone, return what we have or idle
                if (_carried > 0) BeginReturn();
                else CurrentState = UnitState.Idle;
                return;
            }

            if (!HasReachedDestination(1.5f))
                return; // Still walking

            Stop();
            FaceTarget(_targetNode.transform);

            _gatherTimer += Time.deltaTime;
            if (_gatherTimer >= gatherTickInterval)
            {
                _gatherTimer = 0f;
                int gathered = _targetNode.Extract(Mathf.CeilToInt(unitDef.gatherRate));
                _carried += gathered;

                if (_carried >= unitDef.carryCapacity || _targetNode.IsDepleted)
                    BeginReturn();
            }
        }

        void BeginReturn()
        {
            _previousState = UnitState.Gather;
            _previousNode = _targetNode;

            var depot = EconomyManager.Instance.FindNearestDepot(Faction, transform.position);
            if (depot == null)
            {
                Debug.LogWarning("[Worker] No depot found!");
                CurrentState = UnitState.Idle;
                return;
            }

            CurrentState = UnitState.ReturnToDepot;
            MoveTo(depot.DropOffPoint);
        }

        void UpdateReturn()
        {
            if (!HasReachedDestination(1.8f)) return;

            // Deposit
            EconomyManager.Instance.AddResource(Faction, _carriedType, _carried);
            _carried = 0;

            // Resume gathering
            CurrentState = UnitState.ResumePreviousCommand;
        }

        // --- Build Logic ---
        void UpdateBuild()
        {
            if (_targetBuilding == null || _targetBuilding.State == BuildingState.Built)
            {
                CurrentState = UnitState.Idle;
                return;
            }

            if (!HasReachedDestination(2.5f)) return;

            Stop();
            FaceTarget(_targetBuilding.transform);
            _targetBuilding.AddConstructionProgress(Time.deltaTime);
        }

        // --- Flee Logic ---
        void CheckThreatsForFlee()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, fleeRadius);
            foreach (var h in hits)
            {
                var u = h.GetComponentInParent<UnitBase>();
                if (u != null && u.Faction != Faction && u.IsAlive)
                {
                    _previousState = CurrentState;
                    _previousNode = _targetNode;
                    StartFlee(u.transform.position);
                    return;
                }
            }
        }

        void StartFlee(Vector3 threatPos)
        {
            CurrentState = UnitState.Flee;
            _fleeTimer = fleeDuration;
            Vector3 away = (transform.position - threatPos).normalized;
            MoveTo(transform.position + away * fleeRadius);
        }

        void UpdateFlee()
        {
            _fleeTimer -= Time.deltaTime;
            if (_fleeTimer <= 0f)
                CurrentState = UnitState.ResumePreviousCommand;
        }

        // --- Resume ---
        void ResumeCommand()
        {
            if (_previousState == UnitState.Gather && _previousNode != null && !_previousNode.IsDepleted)
                CommandGather(_previousNode);
            else
                CurrentState = UnitState.Idle;
        }

        void FaceTarget(Transform t)
        {
            if (t == null) return;
            Vector3 dir = (t.position - transform.position).normalized;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
        }
    }
}
