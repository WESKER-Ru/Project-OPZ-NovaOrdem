// Assets/Scripts/Economy/ResourceNode.cs
using UnityEngine;
using OPZ.Data;

namespace OPZ.Economy
{
    public class ResourceNode : MonoBehaviour
    {
        [SerializeField] ResourceType type = ResourceType.Supplies;
        [SerializeField] int totalAmount = 500;
        [SerializeField] Transform gatherPoint;

        public ResourceType Type => type;
        public bool IsDepleted => _remaining <= 0;
        public Vector3 GatherPoint => gatherPoint != null ? gatherPoint.position : transform.position;

        int _remaining;

        void Awake() => _remaining = totalAmount;

        /// <summary>Extract up to `amount` resources. Returns actual extracted.</summary>
        public int Extract(int amount)
        {
            int actual = Mathf.Min(amount, _remaining);
            _remaining -= actual;

            if (_remaining <= 0)
            {
                // Visual feedback: shrink or disable
                gameObject.SetActive(false);
            }

            return actual;
        }

        public int Remaining => _remaining;
    }
}
