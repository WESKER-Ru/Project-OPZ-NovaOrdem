// Assets/Scripts/Data/BuildingDef.cs
using UnityEngine;

namespace OPZ.Data
{
    [CreateAssetMenu(menuName = "OPZ/Building Definition")]
    public class BuildingDef : ScriptableObject
    {
        public string buildingName;
        public Faction faction;
        public GameObject prefab;
        public Sprite icon;

        [Header("Cost")]
        public int suppliesCost;
        public int metalCost;
        public int fuelCost;

        [Header("Stats")]
        public float maxHealth = 500f;
        public float buildTime = 10f;
        public float footprintRadius = 2f;

        [Header("Production")]
        public UnitDef[] producibleUnits;

        [Header("Economy")]
        public bool isDepositPoint;
        public bool isHQ;
    }
}
