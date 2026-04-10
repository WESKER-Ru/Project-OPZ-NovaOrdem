// Assets/Scripts/Data/UnitDef.cs
using UnityEngine;

namespace OPZ.Data
{
    [CreateAssetMenu(menuName = "OPZ/Unit Definition")]
    public class UnitDef : ScriptableObject
    {
        public string unitName;
        public Faction faction;
        public UnitRole role;
        public GameObject prefab;
        public Sprite icon;

        [Header("Cost")]
        public int suppliesCost;
        public int metalCost;
        public int fuelCost;
        public float trainTime = 5f;

        [Header("Stats")]
        public float maxHealth = 100f;
        public float moveSpeed = 4f;
        public float attackDamage = 10f;
        public float attackRange = 2f;
        public float attackCooldown = 1f;
        public float lineOfSight = 10f;

        [Header("Worker")]
        public bool isWorker;
        public int carryCapacity = 10;
        public float gatherRate = 1f;
    }
}
