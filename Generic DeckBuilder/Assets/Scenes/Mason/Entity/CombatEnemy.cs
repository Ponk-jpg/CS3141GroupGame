using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum EnemySize { Small, Medium, Large}

// child class of CharacterStats, used to initilize enemies.
public class CombatEnemy : CharacterStats
{
    public static System.Action<CombatEnemy> OnEnemyDefeated;

    [Header("Data Definition Asset")]
    [Tooltip("Assign the EnemyData Scriptable Object file here.")]
    public EnemyData enemyConfigurationData;

    [Header("Local Object UI Component Slots")]
    public SpriteRenderer visualSpriteRenderer;
    public TextMeshProUGUI nameText;
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    [Header("Local Block UI Components")]
    public GameObject blockShieldUIContainer; // An empty child object holding armor art
    public TextMeshProUGUI blockShieldText;

    [HideInInspector]
    public EnemySize visualSize = EnemySize.Medium;

    protected override void Awake()
    {
        base.Awake();
        // Read configuration profiles from asset files prior to operational starts
        if (enemyConfigurationData != null)
        {
            combatantName = enemyConfigurationData.enemyName;
            maxHealth = enemyConfigurationData.maxHealth;
            visualSize = enemyConfigurationData.visualSize;
        }
    }

    protected override void Start()
    {
        //Establish visual data assets onto components
        if (enemyConfigurationData != null)
        {
            if (visualSpriteRenderer != null) visualSpriteRenderer.sprite = enemyConfigurationData.artwork;
            if (nameText != null) nameText.text = enemyConfigurationData.enemyName;
        }

        //Wire up listeners to local health/block loops before initializing parent states
        OnHealthChanged += RefreshLocalHealthUI;
        OnBlockChanged += RefreshLocalBlockUI;

        //Initialize core variables through the parent class
        base.Start();
    }

    private void OnDestroy()
    {
        // Clean up subscriptions safely
        OnHealthChanged -= RefreshLocalHealthUI;
        OnBlockChanged -= RefreshLocalBlockUI;
    }

    // --- UI DISPLAY ENGINE: HEALTH SLIDER SYNC ---
    private void RefreshLocalHealthUI(int currentHP, int maxHP)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHP;
            healthSlider.value = currentHP;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHP} / {maxHP}";
        }
    }

    // --- UI DISPLAY ENGINE: SHIELD SYNC ---
    private void RefreshLocalBlockUI(int currentBlockAmount)
    {
        if (blockShieldUIContainer == null || blockShieldText == null) return;

        if (currentBlockAmount > 0)
        {
            blockShieldUIContainer.SetActive(true); // Pop open the little shield badge artwork icon
            blockShieldText.text = currentBlockAmount.ToString();
        }
        else
        {
            blockShieldUIContainer.SetActive(false); // Hide armor badge if shields hit 0
        }
    }

    protected override void Die()
    {
        OnEnemyDefeated?.Invoke(this);
        base.Die();
        Destroy(gameObject);
    }
}
