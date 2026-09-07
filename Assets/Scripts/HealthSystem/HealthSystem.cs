using System;
using System.Collections;
using UnityEngine;

public class HealthSystem : MonoBehaviour, IHealth, IDamageable {

    public event Action OnDeath;
    public static event Action<float, float> OnHealthChangedUI;
    public event Action<float, float> OnHealthChanged;

    [Header("Health Data")]
    [SerializeField] private PlayerStats stats;

    [SerializeField] private float currentHealth;
    private readonly float maxHP = 100f;

    private IInvulnerable invulnerable;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => stats != null ? stats.MaxHealth : maxHP;
    public bool IsDead { get; private set; }

    private void Awake() {

        invulnerable = GetComponent<IInvulnerable>();
        InitializeHealth();
    }
    private void InitializeHealth() {

        currentHealth = MaxHealth;
        IsDead = false;
        OnHealthChangedUI?.Invoke(CurrentHealth, MaxHealth);
    }
    public void HealMax() {
        currentHealth = MaxHealth;
        OnHealthChangedUI?.Invoke(CurrentHealth, MaxHealth);
    }
    public void ResetHealth() {
        currentHealth = MaxHealth;
        IsDead = false;

        OnHealthChangedUI?.Invoke(CurrentHealth, MaxHealth);
    }

    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0f, MaxHealth);
        IsDead = currentHealth <= 0f;

        OnHealthChangedUI?.Invoke(CurrentHealth, MaxHealth);
    }

    public IEnumerator HealOverTime(float duration) {
        throw new NotImplementedException();
    }
    public void OnHeal(float healAmount) {

        if (IsDead || healAmount <= 0) return;

        currentHealth = Mathf.Max(MaxHealth, CurrentHealth + healAmount);

        if (CurrentHealth > MaxHealth) currentHealth = MaxHealth;

        OnHealthChangedUI?.Invoke(CurrentHealth, MaxHealth);
    }
    public void OnDamage(float damage) {
        
        if (IsDead || damage <= 0) return;

        //if (invulnerable != null && invulnerable.IsInvulnerable) return;

        currentHealth = Mathf.Max(0, CurrentHealth - damage);
        

        if (CurrentHealth <= 0) { 
            Death();
        } else {
            OnHealthChangedUI?.Invoke(CurrentHealth, MaxHealth);
        }
    }
    private void Death() {

        IsDead = true;
        OnDeath?.Invoke();
    }
}