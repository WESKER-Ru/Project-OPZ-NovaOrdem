// Assets/Scripts/Combat/AttackComponent.cs
using UnityEngine;
using OPZ.Data;

namespace OPZ.Combat
{
    public class AttackComponent : MonoBehaviour
    {
        float _damage;
        float _range;
        float _cooldown;
        float _lastAttackTime = -999f;

        public float Range => _range;
        public bool CanAttack => Time.time >= _lastAttackTime + _cooldown;

        public void Init(UnitDef def)
        {
            _damage = def.attackDamage;
            _range = def.attackRange;
            _cooldown = def.attackCooldown;
        }

        public void Init(float damage, float range, float cooldown)
        {
            _damage = damage;
            _range = range;
            _cooldown = cooldown;
        }

        /// <summary>Returns true if attack was executed.</summary>
        public bool TryAttack(HealthComponent target)
        {
            if (target == null || !target.IsAlive) return false;
            if (!CanAttack) return false;

            float dist = Vector3.Distance(transform.position, target.transform.position);
            if (dist > _range + 0.3f) return false;

            target.TakeDamage(_damage);
            _lastAttackTime = Time.time;
            return true;
        }

        public bool InRange(Transform target)
        {
            if (target == null) return false;
            return Vector3.Distance(transform.position, target.position) <= _range + 0.3f;
        }
    }
}
