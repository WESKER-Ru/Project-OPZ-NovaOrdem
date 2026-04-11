// Assets/Scripts/Building/ProductionQueue.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using OPZ.Data;
using OPZ.Economy;
using OPZ.Core;
using OPZ.Units;

namespace OPZ.Building
{
    /// <summary>
    /// Attach to any building that produces units.
    /// Manages a simple FIFO queue with progress bar.
    /// </summary>
    public class ProductionQueue : MonoBehaviour
    {
        [SerializeField] int maxQueueSize = 5;
        [SerializeField] Transform spawnPoint;

        readonly Queue<UnitDef> _queue = new();
        float _currentProgress;
        UnitDef _currentlyProducing;
        BuildingBase _building;

        public int QueueCount => _queue.Count + (_currentlyProducing != null ? 1 : 0);
        public float Progress => _currentlyProducing != null
            ? Mathf.Clamp01(_currentProgress / Mathf.Max(0.01f, _currentlyProducing.trainTime))
            : 0f;
        public UnitDef CurrentlyProducing => _currentlyProducing;

        void Awake()
        {
            _building = GetComponent<BuildingBase>();
            if (_building == null)
                Debug.LogError("[ProductionQueue] Missing BuildingBase component.", this);
        }

        void Update()
        {
            if (_building.State != BuildingState.Built) return;

            if (_currentlyProducing != null)
            {
                _currentProgress += Time.deltaTime;
                if (_currentProgress >= _currentlyProducing.trainTime)
                    FinishProduction();
            }
            else if (_queue.Count > 0)
            {
                _currentlyProducing = _queue.Dequeue();
                _currentProgress = 0f;
            }
        }

        /// <summary>Enqueue a unit for production. Returns false if queue full or can't afford.</summary>
        public bool Enqueue(UnitDef def)
        {
            if (def == null)
            {
                Debug.LogWarning("[ProductionQueue] Enqueue called with null UnitDef.", this);
                return false;
            }

            if (_building == null)
            {
                Debug.LogError("[ProductionQueue] Cannot enqueue without BuildingBase.", this);
                return false;
            }

            if (EconomyManager.Instance == null)
            {
                Debug.LogError("[ProductionQueue] EconomyManager.Instance is null.", this);
                return false;
            }

            if (QueueCount >= maxQueueSize) return false;

            Faction f = _building.Faction;
            if (!EconomyManager.Instance.TrySpend(f, def.suppliesCost, def.metalCost, def.fuelCost))
                return false;

            _queue.Enqueue(def);
            return true;
        }

        /// <summary>Cancel front of queue. Refunds cost.</summary>
        public void CancelFront()
        {
            if (_currentlyProducing != null)
            {
                Refund(_currentlyProducing);
                _currentlyProducing = null;
                _currentProgress = 0f;
            }
            else if (_queue.Count > 0)
            {
                var def = _queue.Dequeue();
                Refund(def);
            }
        }

        void FinishProduction()
        {
            var def = _currentlyProducing;
            if (def == null)
            {
                Debug.LogWarning("[ProductionQueue] FinishProduction called with null definition.", this);
                _currentProgress = 0f;
                return;
            }

            _currentlyProducing = null;
            _currentProgress = 0f;

            if (def.prefab == null)
            {
                Debug.LogError($"[ProductionQueue] UnitDef '{def.name}' has no prefab assigned.", this);
                return;
            }

            Vector3 desired = spawnPoint != null ? spawnPoint.position : transform.position + transform.forward * 3f;
            Vector3 pos = ResolveSpawnPositionOnNavMesh(desired, def.prefab);

            GameObject unitGO = Instantiate(def.prefab, pos, Quaternion.identity);
            var unit = unitGO.GetComponent<UnitBase>();
            if (unit != null)
            {
                unit.InitUnit(def, _building.Faction);

                // Rally point
                if (_building.RallyPoint != Vector3.zero)
                    unit.CommandMove(_building.RallyPoint);
            }
        }

        void Refund(UnitDef def)
        {
            if (def == null || _building == null || EconomyManager.Instance == null) return;

            Faction f = _building.Faction;
            EconomyManager.Instance.AddResource(f, ResourceType.Supplies, def.suppliesCost);
            EconomyManager.Instance.AddResource(f, ResourceType.Metal, def.metalCost);
            EconomyManager.Instance.AddResource(f, ResourceType.Fuel, def.fuelCost);
        }

        Vector3 ResolveSpawnPositionOnNavMesh(Vector3 desired, GameObject prefab)
        {
            float probeRadius = 5f;
            var prefabAgent = prefab != null ? prefab.GetComponent<NavMeshAgent>() : null;
            if (prefabAgent != null)
                probeRadius = Mathf.Max(probeRadius, prefabAgent.radius * 6f);

            if (NavMesh.SamplePosition(desired, out NavMeshHit hit, probeRadius, NavMesh.AllAreas))
                return hit.position;

            // Fallback ring search around desired spawn point.
            const int probeSteps = 8;
            const float ringDistance = 2.5f;
            for (int i = 0; i < probeSteps; i++)
            {
                float ang = (Mathf.PI * 2f * i) / probeSteps;
                Vector3 probe = desired + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * ringDistance;
                if (NavMesh.SamplePosition(probe, out hit, probeRadius, NavMesh.AllAreas))
                    return hit.position;
            }

            Debug.LogWarning("[ProductionQueue] Could not find NavMesh near spawn point; using desired position.", this);
            return desired;
        }
    }
}
