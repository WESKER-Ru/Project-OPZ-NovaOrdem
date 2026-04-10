// Assets/Scripts/Proto/Mover.cs
// Componente de movimento via NavMeshAgent.
// Coloque em cada unidade junto com NavMeshAgent.
using UnityEngine;
using UnityEngine.AI;

namespace OPZ.Proto
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Mover : MonoBehaviour
    {
        NavMeshAgent _agent;

        public bool IsMoving => _agent.hasPath && _agent.remainingDistance > _agent.stoppingDistance;

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        /// <summary>Move para a posição world-space.</summary>
        public void MoveTo(Vector3 worldPos)
        {
            _agent.isStopped = false;
            _agent.SetDestination(worldPos);
        }

        /// <summary>Para imediatamente.</summary>
        public void Stop()
        {
            _agent.isStopped = true;
            _agent.ResetPath();
        }
    }
}
