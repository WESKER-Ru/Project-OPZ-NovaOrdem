using UnityEngine;
using OPZ.Data;

namespace OPZ.Economy
{
    public class DepositPoint : MonoBehaviour
    {
        [SerializeField] Faction faction;
        [SerializeField] Transform dropOffPoint;
        bool _isRegistered;

        public Faction Faction => faction;
        public Vector3 DropOffPoint => dropOffPoint != null ? dropOffPoint.position : transform.position;

        public void SetFaction(Faction f) => faction = f;

        void OnEnable()
        {
            _isRegistered = false;
            TryRegister();
        }

        void Update()
        {
            if (!_isRegistered) TryRegister();
        }

        void OnDisable()
        {
            if (!_isRegistered) return;
            EconomyManager.Instance?.UnregisterDepot(this);
            _isRegistered = false;
        }

        void TryRegister()
        {
            if (_isRegistered) return;

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.RegisterDepot(this);
                _isRegistered = true;
            }
        }
    }
}
