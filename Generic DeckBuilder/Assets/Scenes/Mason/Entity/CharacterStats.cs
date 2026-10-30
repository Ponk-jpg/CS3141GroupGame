using System;
using UnityEngine;


// BASE entity class, used to create player and enemy stats.
// holds all relevent information that pertains to BOTH players and enemies, such as health, block, status effects, etc.
[RequireComponent(typeof(StatusController))]
public abstract class CharacterStats : MonoBehaviour
{
    // Universal Events for UI Health/Block Bars to listen to
    public Action<int, int> OnHealthChanged; // Broadcasts (currentHealth, maxHealth)
    public Action<int> OnBlockChanged;       // Broadcasts (currentBlock)
    public Action OnDeath;

    [Header("Base Identity")]
    public string combatantName;

    [Header("Core Stats Pools")]
    public int maxHealth = 50;
    protected int currentHealth;
    protected int currentBlock = 0;

    // References
    public StatusController StatusEngine { get; private set; }

    public int GetCurrentHealth() => currentHealth;
    public int GetCurrentBlock() => currentBlock;

    protected virtual void Awake()
    {
        StatusEngine = GetComponent<StatusController>();
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        currentBlock = 0;

        // Force initial UI updates to synchronize values
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnBlockChanged?.Invoke(currentBlock);
    }

    // --- UNIVERSAL MATH: GAINING BLOCK ---
    public virtual void AddBlock(int amount)
    {
        currentBlock += amount;
        UnityEngine.Debug.Log($"[BLOCK] {combatantName} gained +{amount} Armor Block. Total Shield: {currentBlock}");
        OnBlockChanged?.Invoke(currentBlock);
    }

    // --- UNIVERSAL MATH: RESETTING BLOCK ---
    public virtual void ClearBlock()
    {
        currentBlock = 0;
        OnBlockChanged?.Invoke(currentBlock);
    }

    // --- UNIVERSAL MATH: DETECTING & SHIELDING DAMAGE ---
    public virtual void TakeDamage(int incomingDamage)
    {
        int initialDamage = incomingDamage;

        // Process block shielding calculation loops first
        if (currentBlock > 0)
        {
            if (incomingDamage >= currentBlock)
            {
                incomingDamage -= currentBlock;
                currentBlock = 0;
            }
            else
            {
                currentBlock -= incomingDamage;
                incomingDamage = 0;
            }
            OnBlockChanged?.Invoke(currentBlock);
        }

        //Route remaining bleed-through damage directly to structural health pools
        currentHealth -= incomingDamage;
        if (currentHealth < 0) currentHealth = 0;

        UnityEngine.Debug.Log($"[DAMAGE] {combatantName} took {initialDamage} damage ({initialDamage - incomingDamage} absorbed by block). Remaining HP: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // virtual function that we change on enemy or player death.
    protected virtual void Die()
    {
        UnityEngine.Debug.Log($"[DEATH] {combatantName} has died.");
        OnDeath?.Invoke();
    }




}
