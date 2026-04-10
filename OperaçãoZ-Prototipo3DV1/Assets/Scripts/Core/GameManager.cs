// Assets/Scripts/Core/GameManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using OPZ.Data;

namespace OPZ.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Match Settings")]
        public Faction playerFaction = Faction.AR;
        public Faction enemyFaction = Faction.EG;

        public bool MatchOver { get; private set; }

        // Registry of all living units and buildings per faction
        readonly Dictionary<Faction, List<Units.UnitBase>> _units = new();
        readonly Dictionary<Faction, List<Building.BuildingBase>> _buildings = new();

        public event Action<Faction> OnFactionEliminated;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            foreach (Faction f in Enum.GetValues(typeof(Faction)))
            {
                _units[f] = new List<Units.UnitBase>();
                _buildings[f] = new List<Building.BuildingBase>();
            }
        }

        // --- Registration ---
        public void RegisterUnit(Units.UnitBase unit) => _units[unit.Faction].Add(unit);
        public void UnregisterUnit(Units.UnitBase unit) => _units[unit.Faction].Remove(unit);
        public void RegisterBuilding(Building.BuildingBase b) => _buildings[b.Faction].Add(b);
        public void UnregisterBuilding(Building.BuildingBase b) => _buildings[b.Faction].Remove(b);

        public IReadOnlyList<Units.UnitBase> GetUnits(Faction f) => _units[f];
        public IReadOnlyList<Building.BuildingBase> GetBuildings(Faction f) => _buildings[f];

        // --- Win Condition: Annihilation ---
        public void CheckElimination(Faction faction)
        {
            if (MatchOver) return;
            if (_units[faction].Count == 0 && _buildings[faction].Count == 0)
            {
                MatchOver = true;
                OnFactionEliminated?.Invoke(faction);
                Debug.Log($"[GameManager] Faction {faction} eliminated!");
            }
        }
    }
}
