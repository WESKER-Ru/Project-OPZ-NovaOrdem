// Assets/Scripts/Combat/HealthComponent.cs
using System;
using UnityEngine;

namespace OPZ.Combat
{
    public class HealthComponent : MonoBehaviour
    {
        public float MaxHealth { get; private set; } = 100f;
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;
        public float HealthRatio => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;

        public event Action OnDeath;
        public event Action<float, float> OnHealthChanged; // current, max

        public void Init(float maxHp)
        {
            MaxHealth = maxHp;
            CurrentHealth = maxHp;
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive) return;
            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
            if (CurrentHealth <= 0f) OnDeath?.Invoke();
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }
}
