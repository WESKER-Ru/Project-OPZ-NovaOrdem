using System;
using System.Collections.Generic;
using UnityEngine;
using OPZ.Data;

namespace OPZ.Economy
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [Header("Starting Resources")]
        [SerializeField] int startSupplies = 200;
        [SerializeField] int startMetal = 100;
        [SerializeField] int startFuel = 50;

        readonly Dictionary<Faction, Dictionary<ResourceType, int>> _resources = new();
        readonly List<DepositPoint> _depots = new();

        public event Action<Faction, ResourceType, int> OnResourceChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            foreach (Faction f in Enum.GetValues(typeof(Faction)))
            {
                _resources[f] = new Dictionary<ResourceType, int>
                {
                    { ResourceType.Supplies, startSupplies },
                    { ResourceType.Metal, startMetal },
                    { ResourceType.Fuel, startFuel }
                };
            }
        }

        void Start()
        {
            RefreshDepotRegistry();
        }

        public void RefreshDepotRegistry()
        {
            _depots.Clear();
            foreach (var dp in FindObjectsByType<DepositPoint>(FindObjectsSortMode.None))
            {
                RegisterDepot(dp);
            }
        }

        public int GetResource(Faction f, ResourceType t) => _resources[f][t];

        public void AddResource(Faction f, ResourceType t, int amount)
        {
            _resources[f][t] += amount;
            OnResourceChanged?.Invoke(f, t, _resources[f][t]);
        }

        public bool TrySpend(Faction f, int supplies, int metal, int fuel)
        {
            var r = _resources[f];
            if (r[ResourceType.Supplies] < supplies || r[ResourceType.Metal] < metal || r[ResourceType.Fuel] < fuel)
                return false;

            r[ResourceType.Supplies] -= supplies;
            r[ResourceType.Metal] -= metal;
            r[ResourceType.Fuel] -= fuel;

            OnResourceChanged?.Invoke(f, ResourceType.Supplies, r[ResourceType.Supplies]);
            OnResourceChanged?.Invoke(f, ResourceType.Metal, r[ResourceType.Metal]);
            OnResourceChanged?.Invoke(f, ResourceType.Fuel, r[ResourceType.Fuel]);
            return true;
        }

        public bool CanAfford(Faction f, int supplies, int metal, int fuel)
        {
            var r = _resources[f];
            return r[ResourceType.Supplies] >= supplies && r[ResourceType.Metal] >= metal && r[ResourceType.Fuel] >= fuel;
        }

        public void RegisterDepot(DepositPoint dp)
        {
            if (dp == null) return;
            if (!_depots.Contains(dp))
                _depots.Add(dp);
        }

        public void UnregisterDepot(DepositPoint dp)
        {
            if (dp == null) return;
            _depots.Remove(dp);
        }

        public DepositPoint FindNearestDepot(Faction faction, Vector3 pos)
        {
            DepositPoint best = null;
            float bestDist = float.MaxValue;
            foreach (var dp in _depots)
            {
                if (dp == null) continue;
                if (dp.Faction != faction) continue;
                float d = Vector3.Distance(pos, dp.transform.position);
                if (d < bestDist) { bestDist = d; best = dp; }
            }
            return best;
        }
    }
}
