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
        public float Progress => _currentlyProducing != null ? _currentProgress / _currentlyProducing.trainTime : 0f;
        public UnitDef CurrentlyProducing => _currentlyProducing;

        void Awake() => _building = GetComponent<BuildingBase>();

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
            _currentlyProducing = null;
            _currentProgress = 0f;

            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position + transform.forward * 3f;

            // Find valid NavMesh position
            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                pos = hit.position;

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
            Faction f = _building.Faction;
            EconomyManager.Instance.AddResource(f, ResourceType.Supplies, def.suppliesCost);
            EconomyManager.Instance.AddResource(f, ResourceType.Metal, def.metalCost);
            EconomyManager.Instance.AddResource(f, ResourceType.Fuel, def.fuelCost);
        }
    }
}
