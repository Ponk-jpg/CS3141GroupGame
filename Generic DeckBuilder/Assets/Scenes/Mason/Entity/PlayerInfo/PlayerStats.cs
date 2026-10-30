using UnityEngine;
using UnityEngine.UI;
using TMPro;

// child class of CharacterStats specifically for our player, holds information a generic player class would need.
public class PlayerStats : CharacterStats
{
    [Header("Data Definition Asset")]
    [Tooltip("Assign PlayerData Scriptable Object file here.")]
    public PlayerData playerConfigurationData;

    [Header("Local UI Component Slots")]
    public TextMeshProUGUI nameText;
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    [Header("Local Block UI Components")]
    public GameObject blockShieldUIContainer;
    public TextMeshProUGUI blockShieldText;




    private void OnEnable()
    {
        
        DeckManager.OnPlayerTurnStateChanged += HandleTurnBlockDecay;
    }

    private void OnDisable()
    {
        DeckManager.OnPlayerTurnStateChanged -= HandleTurnBlockDecay;

        // Clean up visual updates subscriptions safely
        OnHealthChanged -= RefreshLocalHealthUI;
        OnBlockChanged -= RefreshLocalBlockUI;
    }

    protected override void Awake()
    {
        // Read base properties from config scriptable asset files prior to operational loops
        if (playerConfigurationData != null)
        {
            combatantName = playerConfigurationData.characterClassName;
            maxHealth = playerConfigurationData.maxHealth;
        }

        base.Awake();
    }

    protected override void Start()
    {

        //Establish baseline descriptions onto labels
        //if (playerConfigurationData != null && nameText != null)
        //{
        //    nameText.text = playerConfigurationData.characterClassName;
        //} not using currently.

        //Connect local listeners to health/block loops before initializing parent states
        OnHealthChanged += RefreshLocalHealthUI;
        OnBlockChanged += RefreshLocalBlockUI;

        //Initialize core values through the parent class
        base.Start();

        //Apply starting shield values if configured in the data asset profile
        if (playerConfigurationData != null && playerConfigurationData.startingBlock > 0)
        {
            AddBlock(playerConfigurationData.startingBlock);
        }
    }

    private void HandleTurnBlockDecay(bool isPlayerTurnStarted)
    {
        // When your turn starts, clear out previous round's armor blocks cleanly
        if (isPlayerTurnStarted)
        {
            UnityEngine.Debug.Log($"[PLAYER ROUND START] Decaying player block shields back down to 0.");
            ClearBlock();
        }
    }

    // --- UI DISPLAY: HEALTH SYNCHRONIZATION ---
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

    private void RefreshLocalBlockUI(int currentBlockAmount)
    {
        if (blockShieldUIContainer == null || blockShieldText == null) return;

        if (currentBlockAmount > 0)
        {
            blockShieldUIContainer.SetActive(true); // Illuminate blue armor shield icon
            blockShieldText.text = currentBlockAmount.ToString();
        }
        else
        {
            blockShieldUIContainer.SetActive(false); // Hide armor badge if shields hit 0
        }
    }

    protected override void Die()
    {
        base.Die();
        UnityEngine.Debug.LogError("============= GAME OVER: PLAYER DIED =============");
        // This is game over screen thing
    }



}
