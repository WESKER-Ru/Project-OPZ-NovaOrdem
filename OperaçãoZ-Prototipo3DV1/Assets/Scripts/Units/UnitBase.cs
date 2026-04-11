// Assets/Scripts/Units/UnitBase.cs
using UnityEngine;
using UnityEngine.AI;
using OPZ.Data;
using OPZ.Core;
using OPZ.Combat;

namespace OPZ.Units
{
    [RequireComponent(typeof(NavMeshAgent), typeof(HealthComponent))]
    public class UnitBase : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] protected UnitDef unitDef;
        [SerializeField] Faction faction;

        [Header("Selection Visual")]
        [SerializeField] GameObject selectionCircle;

        [Header("NavMesh Recovery")]
        [SerializeField] float navMeshRecoveryRadius = 25f;
        [SerializeField] float destinationRecoveryRadius = 12f;

        public Faction Faction => faction;
        public UnitDef Def => unitDef;
        public UnitState CurrentState { get; protected set; } = UnitState.Idle;
        public bool IsAlive => _health != null && _health.IsAlive;
        public bool IsSelected { get; private set; }

        protected NavMeshAgent Agent;
        protected HealthComponent _health;
        bool _warnedOffNavMesh;

        protected virtual void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            _health = GetComponent<HealthComponent>();

            if (unitDef != null)
            {
                Agent.speed = unitDef.moveSpeed;
                _health.Init(unitDef.maxHealth);
                faction = unitDef.faction;
            }

            _health.OnDeath += HandleDeath;

            if (selectionCircle != null) selectionCircle.SetActive(false);

            EnsureOnNavMesh();
        }

        protected virtual void Start()
        {
            GameManager.Instance.RegisterUnit(this);
        }

        protected virtual void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnregisterUnit(this);
                GameManager.Instance.CheckElimination(faction);
            }
        }

        // --- Commands ---
        public virtual void CommandMove(Vector3 destination)
        {
            if (!TrySetDestination(destination)) return;

            CurrentState = UnitState.Move;
        }

        public virtual void CommandStop()
        {
            CurrentState = UnitState.Idle;
            Agent.isStopped = true;
        }

        // --- Selection ---
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            if (selectionCircle != null) selectionCircle.SetActive(selected);
        }

        // --- Movement helpers ---
        protected bool HasReachedDestination(float threshold = 0.5f)
        {
            if (Agent == null || !Agent.enabled || !Agent.isOnNavMesh) return false;
            if (Agent.pathPending) return false;
            return Agent.remainingDistance <= threshold;
        }

        protected void MoveTo(Vector3 pos)
        {
            TrySetDestination(pos);
        }

        protected void Stop()
        {
            Agent.isStopped = true;
            Agent.ResetPath();
        }

        void HandleDeath()
        {
            CurrentState = UnitState.Dead;
            Agent.isStopped = true;
            Agent.enabled = false;
            SetSelected(false);
            // TODO: play death anim, spawn corpse
            Destroy(gameObject, 2f);
        }

        /// <summary>Initialize faction and def at runtime (for production spawning).</summary>
        public void InitUnit(UnitDef def, Faction fac)
        {
            unitDef = def;
            faction = fac;
            Agent.speed = def.moveSpeed;
            _health.Init(def.maxHealth);

            EnsureOnNavMesh();
        }

        bool TrySetDestination(Vector3 destination)
        {
            if (Agent == null || !Agent.enabled) return false;

            EnsureOnNavMesh();
            if (!Agent.isOnNavMesh)
            {
                // Last attempt: snap close to commanded destination and retry.
                if (NavMesh.SamplePosition(destination, out NavMeshHit destinationHit, destinationRecoveryRadius, NavMesh.AllAreas))
                    Agent.Warp(destinationHit.position);

                if (!_warnedOffNavMesh)
                {
                    Debug.LogWarning($"[UnitBase] Unit '{name}' is off NavMesh; command ignored until repositioned.", this);
                    _warnedOffNavMesh = true;
                }

                if (!Agent.isOnNavMesh)
                    return false;
            }

            Agent.isStopped = false;
            _warnedOffNavMesh = false;
            return Agent.SetDestination(destination);
        }

        void EnsureOnNavMesh()
        {
            if (Agent == null || !Agent.enabled || Agent.isOnNavMesh) return;

            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, navMeshRecoveryRadius, NavMesh.AllAreas))
                Agent.Warp(hit.position);
        }
    }
}
