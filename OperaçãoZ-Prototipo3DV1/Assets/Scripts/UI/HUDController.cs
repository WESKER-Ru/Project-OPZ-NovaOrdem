// Assets/Scripts/UI/HUDController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OPZ.Core;
using OPZ.Data;
using OPZ.Economy;
using OPZ.Building;
using OPZ.Units;

namespace OPZ.UI
{
    /// <summary>
    /// Main HUD controller. Wire up UI references in the Inspector.
    /// Layout target:
    ///   TOP BAR: Supplies | Metal | Fuel
    ///   BOTTOM LEFT: Selection info panel
    ///   BOTTOM CENTER: Command card / Build panel
    ///   BOTTOM RIGHT: Minimap
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Resource Bar (Top)")]
        [SerializeField] TMP_Text suppliesText;
        [SerializeField] TMP_Text metalText;
        [SerializeField] TMP_Text fuelText;

        [Header("Selection Panel (Bottom Left)")]
        [SerializeField] GameObject selectionPanel;
        [SerializeField] TMP_Text selectionTitle;
        [SerializeField] TMP_Text selectionInfo;
        [SerializeField] Image selectionIcon;
        [SerializeField] Slider healthBar;

        [Header("Build Panel (Bottom Center)")]
        [SerializeField] GameObject buildPanel;
        [SerializeField] Transform buildButtonContainer;
        [SerializeField] GameObject buildButtonPrefab;

        [Header("Production Panel")]
        [SerializeField] GameObject productionPanel;
        [SerializeField] Slider productionProgressBar;
        [SerializeField] TMP_Text productionQueueText;
        [SerializeField] Transform productionButtonContainer;
        [SerializeField] GameObject productionButtonPrefab;

        [Header("Match End")]
        [SerializeField] GameObject matchEndPanel;
        [SerializeField] TMP_Text matchEndText;

        Faction _player;

        void Start()
        {
            _player = GameManager.Instance.playerFaction;

            EconomyManager.Instance.OnResourceChanged += OnResourceChanged;
            SelectionManager.Instance.OnSelectionChanged += RefreshSelectionUI;
            GameManager.Instance.OnFactionEliminated += OnFactionEliminated;

            if (matchEndPanel != null) matchEndPanel.SetActive(false);

            RefreshResources();
            RefreshSelectionUI();
        }

        void OnDestroy()
        {
            if (EconomyManager.Instance != null) EconomyManager.Instance.OnResourceChanged -= OnResourceChanged;
            if (SelectionManager.Instance != null) SelectionManager.Instance.OnSelectionChanged -= RefreshSelectionUI;
            if (GameManager.Instance != null) GameManager.Instance.OnFactionEliminated -= OnFactionEliminated;
        }

        void Update()
        {
            UpdateProductionProgress();
        }

        // --- Resources ---
        void OnResourceChanged(Faction f, ResourceType t, int val)
        {
            if (f != _player) return;
            RefreshResources();
        }

        void RefreshResources()
        {
            var eco = EconomyManager.Instance;
            if (suppliesText != null) suppliesText.text = eco.GetResource(_player, ResourceType.Supplies).ToString();
            if (metalText != null) metalText.text = eco.GetResource(_player, ResourceType.Metal).ToString();
            if (fuelText != null) fuelText.text = eco.GetResource(_player, ResourceType.Fuel).ToString();
        }

        // --- Selection ---
        void RefreshSelectionUI()
        {
            var sel = SelectionManager.Instance;
            bool hasUnits = sel.SelectedUnits.Count > 0;
            bool hasBuilding = sel.SelectedBuilding != null;

            // Selection panel
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(hasUnits || hasBuilding);

                if (hasUnits && sel.SelectedUnits.Count == 1)
                {
                    var u = sel.SelectedUnits[0];
                    if (selectionTitle != null) selectionTitle.text = u.Def != null ? u.Def.unitName : u.name;
                    if (selectionIcon != null && u.Def != null) selectionIcon.sprite = u.Def.icon;
                    var hp = u.GetComponent<Combat.HealthComponent>();
                    if (healthBar != null && hp != null) healthBar.value = hp.HealthRatio;
                    if (selectionInfo != null) selectionInfo.text = $"HP: {hp.CurrentHealth:F0}/{hp.MaxHealth:F0}";
                }
                else if (hasUnits)
                {
                    if (selectionTitle != null) selectionTitle.text = $"{sel.SelectedUnits.Count} units selected";
                    if (selectionInfo != null) selectionInfo.text = "";
                }
                else if (hasBuilding)
                {
                    var b = sel.SelectedBuilding;
                    if (selectionTitle != null) selectionTitle.text = b.Def != null ? b.Def.buildingName : b.name;
                    var hp = b.GetComponent<Combat.HealthComponent>();
                    if (healthBar != null && hp != null) healthBar.value = hp.HealthRatio;
                    if (selectionInfo != null)
                    {
                        string stateStr = b.State == BuildingState.UnderConstruction
                            ? $"Building... {b.ConstructionProgress * 100:F0}%"
                            : b.State.ToString();
                        selectionInfo.text = stateStr;
                    }
                }
            }

            // Build panel: show when workers are selected
            RefreshBuildPanel(hasUnits && HasWorkerSelected(sel));

            // Production panel: show when a producing building is selected
            RefreshProductionPanel(hasBuilding ? sel.SelectedBuilding : null);
        }

        bool HasWorkerSelected(SelectionManager sel)
        {
            foreach (var u in sel.SelectedUnits)
                if (u is WorkerUnit) return true;
            return false;
        }

        // --- Build Panel ---
        void RefreshBuildPanel(bool show)
        {
            if (buildPanel == null) return;
            buildPanel.SetActive(show);
            // Build buttons are populated from a BuildingDef list — in real implementation,
            // iterate over available buildings for the faction and create buttons.
            // For now this is a hook; buttons call BuildPlacementController.Instance.EnterPlacementMode(def).
        }

        /// <summary>Call from UI button to start placing a building.</summary>
        public void OnBuildButtonClicked(BuildingDef def)
        {
            BuildPlacementController.Instance.EnterPlacementMode(def);
        }

        // --- Production Panel ---
        void RefreshProductionPanel(BuildingBase bldg)
        {
            if (productionPanel == null) return;

            ProductionQueue pq = bldg != null ? bldg.GetComponent<ProductionQueue>() : null;
            bool show = pq != null && bldg.State == BuildingState.Built;
            productionPanel.SetActive(show);

            if (!show) return;

            // Populate production buttons from buildingDef.producibleUnits
            // Each button calls pq.Enqueue(unitDef)
        }

        void UpdateProductionProgress()
        {
            var sel = SelectionManager.Instance;
            if (sel.SelectedBuilding == null) return;
            var pq = sel.SelectedBuilding.GetComponent<ProductionQueue>();
            if (pq == null) return;

            if (productionProgressBar != null) productionProgressBar.value = pq.Progress;
            if (productionQueueText != null) productionQueueText.text = $"Queue: {pq.QueueCount}";
        }

        // --- Match End ---
        void OnFactionEliminated(Faction eliminated)
        {
            if (matchEndPanel == null) return;
            matchEndPanel.SetActive(true);
            bool playerWon = eliminated != _player;
            if (matchEndText != null)
                matchEndText.text = playerWon ? "VITÓRIA — ANIQUILAÇÃO TOTAL" : "DERROTA — SUA FACÇÃO FOI ELIMINADA";
        }
    }
}
