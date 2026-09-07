using System;
using System.Collections;

public interface IHealth {

    float CurrentHealth { get; }
    float MaxHealth { get; }
    bool IsDead { get; }

    event Action OnDeath;
    event Action<float, float> OnHealthChanged;

    public void OnHeal(float healAmount);
    public void HealMax();
    public IEnumerator HealOverTime(float duration);
}