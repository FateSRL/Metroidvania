using System;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected int maxHealth = 3;
    public int currentHealth { get; private set; }

    [Header("Attack")]
    [SerializeField] protected int damage = 1;

    public Action onHealthChanged;
    public bool isDead { get; private set; }
    public bool isInvincible { get; private set; }

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual bool TakeDamage(int damageAmount)
    {
        if (isDead || isInvincible)
            return false;

        damageAmount = Mathf.Max(0, damageAmount);
        currentHealth = Mathf.Clamp(currentHealth - damageAmount, 0, maxHealth);
        onHealthChanged?.Invoke();

        if (currentHealth <= 0)
            Die();

        return true;
    }

    public virtual void IncreaseHealthBy(int amount)
    {
        if (isDead)
            return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        onHealthChanged?.Invoke();
    }

    public int GetDamage()
    {
        return damage;
    }

    public int GetMaxHealthValue()
    {
        return maxHealth;
    }

    public void MakeInvincible(bool value)
    {
        isInvincible = value;
    }

    protected virtual void Die()
    {
        isDead = true;
    }
}
