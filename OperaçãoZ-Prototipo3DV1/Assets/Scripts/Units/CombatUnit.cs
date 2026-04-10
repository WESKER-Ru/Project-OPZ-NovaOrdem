// Assets/Scripts/Units/CombatUnit.cs
using UnityEngine;
using OPZ.Data;
using OPZ.Combat;
using OPZ.Building;
using OPZ.Core;

namespace OPZ.Units
{
    [RequireComponent(typeof(AttackComponent))]
    public class CombatUnit : UnitBase
    {
        [Header("Combat")]
        [SerializeField] float autoDetectRadius = 12f;
        [SerializeField] float chaseLeashRange = 18f;

        AttackComponent _attack;
        HealthComponent _targetHealth;
        Transform _targetTransform;
        Vector3 _commandOrigin; // where the unit was when given the last command

        protected override void Awake()
        {
            base.Awake();
            _attack = GetComponent<AttackComponent>();
            if (unitDef != null)
                _attack.Init(unitDef);
        }

        void Update()
        {
            if (!IsAlive) return;

            switch (CurrentState)
            {
                case UnitState.Idle:
                    ScanForThreats();
                    break;
                case UnitState.Move:
                    if (HasReachedDestination()) CurrentState = UnitState.Idle;
                    else ScanForThreats();
                    break;
                case UnitState.Attack:
                    UpdateAttack();
                    break;
                case UnitState.Chase:
                    UpdateChase();
                    break;
                case UnitState.AutoDefend:
                    UpdateAttack();
                    break;
                case UnitState.Hold:
                    ScanForThreatsInPlace();
                    break;
            }
        }

        // --- Commands ---
        public void CommandAttack(UnitBase target)
        {
            if (target == null || !target.IsAlive) return;
            SetTarget(target.GetComponent<HealthComponent>(), target.transform);
            _commandOrigin = transform.position;
            CurrentState = UnitState.Attack;
        }

        public void CommandAttackBuilding(BuildingBase bldg)
        {
            if (bldg == null) return;
            SetTarget(bldg.GetComponent<HealthComponent>(), bldg.transform);
            _commandOrigin = transform.position;
            CurrentState = UnitState.Attack;
        }

        public override void CommandMove(Vector3 destination)
        {
            ClearTarget();
            _commandOrigin = transform.position;
            base.CommandMove(destination);
        }

        public override void CommandStop()
        {
            ClearTarget();
            base.CommandStop();
        }

        public void CommandHold()
        {
            ClearTarget();
            CurrentState = UnitState.Hold;
            Stop();
        }

        // --- Attack Logic ---
        void UpdateAttack()
        {
            if (_targetHealth == null || !_targetHealth.IsAlive) { ClearTarget(); CurrentState = UnitState.Idle; return; }

            if (_attack.InRange(_targetTransform))
            {
                Stop();
                FaceTarget();
                _attack.TryAttack(_targetHealth);
            }
            else
            {
                // Move toward target
                CurrentState = UnitState.Chase;
                MoveTo(_targetTransform.position);
            }
        }

        void UpdateChase()
        {
            if (_targetHealth == null || !_targetHealth.IsAlive) { ClearTarget(); CurrentState = UnitState.Idle; return; }

            // Leash check
            float distFromOrigin = Vector3.Distance(transform.position, _commandOrigin);
            if (distFromOrigin > chaseLeashRange)
            {
                ClearTarget();
                CommandMove(_commandOrigin);
                return;
            }

            if (_attack.InRange(_targetTransform))
            {
                CurrentState = UnitState.Attack;
                return;
            }

            MoveTo(_targetTransform.position);
        }

        // --- Auto Detect ---
        void ScanForThreats()
        {
            var enemy = FindNearestEnemy(autoDetectRadius);
            if (enemy == null) return;
            SetTarget(enemy.GetComponent<HealthComponent>(), enemy.transform);
            _commandOrigin = transform.position;
            CurrentState = UnitState.AutoDefend;
        }

        void ScanForThreatsInPlace()
        {
            var enemy = FindNearestEnemy(_attack.Range + 1f);
            if (enemy == null) return;
            SetTarget(enemy.GetComponent<HealthComponent>(), enemy.transform);
            CurrentState = UnitState.Attack;
        }

        UnitBase FindNearestEnemy(float radius)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            UnitBase closest = null;
            float closestDist = float.MaxValue;
            foreach (var h in hits)
            {
                var u = h.GetComponentInParent<UnitBase>();
                if (u == null || u == this || u.Faction == Faction || !u.IsAlive) continue;
                float d = Vector3.Distance(transform.position, u.transform.position);
                if (d < closestDist) { closestDist = d; closest = u; }
            }
            return closest;
        }

        void SetTarget(HealthComponent hp, Transform t)
        {
            _targetHealth = hp;
            _targetTransform = t;
        }

        void ClearTarget()
        {
            _targetHealth = null;
            _targetTransform = null;
        }

        void FaceTarget()
        {
            if (_targetTransform == null) return;
            Vector3 dir = (_targetTransform.position - transform.position).normalized;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
        }
    }
}
