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

        public Faction Faction => faction;
        public UnitDef Def => unitDef;
        public UnitState CurrentState { get; protected set; } = UnitState.Idle;
        public bool IsAlive => _health != null && _health.IsAlive;
        public bool IsSelected { get; private set; }

        protected NavMeshAgent Agent;
        protected HealthComponent _health;

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
            CurrentState = UnitState.Move;
            Agent.isStopped = false;
            Agent.SetDestination(destination);
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
            if (Agent.pathPending) return false;
            return Agent.remainingDistance <= threshold;
        }

        protected void MoveTo(Vector3 pos)
        {
            Agent.isStopped = false;
            Agent.SetDestination(pos);
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
        }
    }
}
