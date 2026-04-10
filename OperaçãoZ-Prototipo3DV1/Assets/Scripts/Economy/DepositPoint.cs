using UnityEngine;
using OPZ.Data;

namespace OPZ.Economy
{
    public class DepositPoint : MonoBehaviour
    {
        [SerializeField] Faction faction;
        [SerializeField] Transform dropOffPoint;

        public Faction Faction => faction;
        public Vector3 DropOffPoint => dropOffPoint != null ? dropOffPoint.position : transform.position;

        public void SetFaction(Faction f) => faction = f;

        void OnEnable() => TryRegister();
        void Start() => TryRegister();
        void OnDisable() => EconomyManager.Instance?.UnregisterDepot(this);

        void TryRegister()
        {
            if (EconomyManager.Instance != null)
                EconomyManager.Instance.RegisterDepot(this);
        }
    }
}
