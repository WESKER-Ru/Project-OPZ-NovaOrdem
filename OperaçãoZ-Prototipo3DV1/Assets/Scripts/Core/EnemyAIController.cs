// Assets/Scripts/Core/EnemyAIController.cs
using System.Collections.Generic;
using UnityEngine;
using OPZ.Data;
using OPZ.Units;
using OPZ.Building;

namespace OPZ.Core
{
    /// <summary>
    /// Bare-bones AI for the enemy faction in the MVP.
    /// - Periodically produces units from its buildings
    /// - Sends combat units to attack the player base
    /// Will be replaced by a proper behavior tree in future phases.
    /// </summary>
    public class EnemyAIController : MonoBehaviour
    {
        [SerializeField] Faction aiFaction = Faction.EG;
        [SerializeField] float attackInterval = 45f;
        [SerializeField] float productionCheckInterval = 8f;

        float _attackTimer;
        float _prodTimer;

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.MatchOver) return;

            _prodTimer += Time.deltaTime;
            _attackTimer += Time.deltaTime;

            if (_prodTimer >= productionCheckInterval)
            {
                _prodTimer = 0f;
                TryProduce();
            }

            if (_attackTimer >= attackInterval)
            {
                _attackTimer = 0f;
                LaunchAttack();
            }
        }

        void TryProduce()
        {
            var buildings = GameManager.Instance.GetBuildings(aiFaction);
            foreach (var b in buildings)
            {
                if (b.State != BuildingState.Built) continue;
                var pq = b.GetComponent<ProductionQueue>();
                if (pq == null) continue;
                if (pq.QueueCount >= 2) continue;

                // Produce first available unit
                if (b.Def != null && b.Def.producibleUnits != null && b.Def.producibleUnits.Length > 0)
                    pq.Enqueue(b.Def.producibleUnits[0]);
            }
        }

        void LaunchAttack()
        {
            Faction playerFaction = GameManager.Instance.playerFaction;
            var playerBuildings = GameManager.Instance.GetBuildings(playerFaction);
            if (playerBuildings.Count == 0) return;

            // Pick a target
            Transform target = playerBuildings[Random.Range(0, playerBuildings.Count)].transform;

            // Send all idle combat units
            var units = GameManager.Instance.GetUnits(aiFaction);
            foreach (var u in units)
            {
                if (u is CombatUnit cu && cu.CurrentState == UnitState.Idle)
                    cu.CommandAttack(FindNearestPlayerUnit(cu.transform.position) ?? u); // fallback
            }

            // Simple: just move combat units toward player base
            foreach (var u in units)
            {
                if (u is CombatUnit && u.CurrentState == UnitState.Idle)
                    u.CommandMove(target.position);
            }
        }

        UnitBase FindNearestPlayerUnit(Vector3 from)
        {
            Faction player = GameManager.Instance.playerFaction;
            var playerUnits = GameManager.Instance.GetUnits(player);
            UnitBase closest = null;
            float bestDist = float.MaxValue;
            foreach (var u in playerUnits)
            {
                if (!u.IsAlive) continue;
                float d = Vector3.Distance(from, u.transform.position);
                if (d < bestDist) { bestDist = d; closest = u; }
            }
            return closest;
        }
    }
}
