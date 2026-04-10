// Assets/Scripts/Building/BuildingBase.cs
using UnityEngine;
using OPZ.Data;
using OPZ.Core;
using OPZ.Combat;
using OPZ.Economy;

namespace OPZ.Building
{
    [RequireComponent(typeof(HealthComponent))]
    public class BuildingBase : MonoBehaviour
    {
        [Header("Definition")]
        [SerializeField] BuildingDef buildingDef;
        [SerializeField] Faction faction;

        [Header("Visuals")]
        [SerializeField] GameObject ghostVisual;
        [SerializeField] GameObject constructionVisual;
        [SerializeField] GameObject builtVisual;

        public BuildingDef Def => buildingDef;
        public Faction Faction => faction;
        public BuildingState State { get; private set; } = BuildingState.Ghost;
        public float ConstructionProgress { get; private set; }

        HealthComponent _health;
        DepositPoint _depot;

        void Awake()
        {
            _health = GetComponent<HealthComponent>();
            _depot = GetComponent<DepositPoint>();

            _health.OnDeath += OnDestroyed;
        }

        void Start()
        {
            if (buildingDef != null)
                _health.Init(buildingDef.maxHealth);

            GameManager.Instance.RegisterBuilding(this);
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnregisterBuilding(this);
                GameManager.Instance.CheckElimination(faction);
            }
        }

        // --- Initialization ---
        public void InitBuilding(BuildingDef def, Faction fac)
        {
            buildingDef = def;
            faction = fac;
            _health.Init(def.maxHealth);
            if (_depot != null) _depot.SetFaction(fac);
        }

        // --- State Transitions ---
        public void PlaceAsFoundation()
        {
            State = BuildingState.Foundation;
            ConstructionProgress = 0f;
            UpdateVisuals();
            TransitionTo(BuildingState.UnderConstruction);
        }

        public void PlaceAsBuilt()
        {
            State = BuildingState.Built;
            ConstructionProgress = 1f;
            UpdateVisuals();
            ActivateBuilding();
        }

        public void AddConstructionProgress(float deltaTime)
        {
            if (State != BuildingState.UnderConstruction) return;
            if (buildingDef == null || buildingDef.buildTime <= 0) return;

            ConstructionProgress += deltaTime / buildingDef.buildTime;
            if (ConstructionProgress >= 1f)
            {
                ConstructionProgress = 1f;
                TransitionTo(BuildingState.Built);
                ActivateBuilding();
            }
        }

        void TransitionTo(BuildingState newState)
        {
            State = newState;
            UpdateVisuals();
        }

        void ActivateBuilding()
        {
            // Enable deposit point if this is a depot/HQ
            if (_depot != null && buildingDef.isDepositPoint)
                _depot.enabled = true;
        }

        void OnDestroyed()
        {
            State = BuildingState.Destroyed;
            UpdateVisuals();
            if (_depot != null) _depot.enabled = false;
            Destroy(gameObject, 3f);
        }

        void UpdateVisuals()
        {
            if (ghostVisual != null) ghostVisual.SetActive(State == BuildingState.Ghost);
            if (constructionVisual != null) constructionVisual.SetActive(State == BuildingState.Foundation || State == BuildingState.UnderConstruction);
            if (builtVisual != null) builtVisual.SetActive(State == BuildingState.Built);
        }

        // --- Rally Point (hook for production) ---
        [HideInInspector] public Vector3 RallyPoint;

        public void SetRallyPoint(Vector3 pos) => RallyPoint = pos;
    }
}
