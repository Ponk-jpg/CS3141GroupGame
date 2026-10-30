using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Collections.Generic;
using System;


// THIS IS A BIG MF CLASS, just a heads up as it does quite a bit. 
// this class mainly controls what UI elements are doing what for all of the cards on screen, along with displaying UI elements
// for the player to see what is happening with a given card.
[RequireComponent(typeof(CanvasGroup))]
public class CardController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Energy Cost Colors")]
    public Color affordableColor = Color.white;
    public Color expensiveColor = Color.red;

    [Header("Targeting Settings")]
    [Tooltip("How many pixels to drop the arrows starting point to better fit")]
    public float arrowBaseYOffset = 150f;

    [Header("UI Visual References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public UnityEngine.UI.Image artworkImage;

    [Header("Hover Animation Settings")]
    public float hoverScale = 1.25f;
    public float hoverYOffset = 80f;
    public float animationSpeed = 15f;

    [Header("Play Settings")]
    [Tooltip("The Y position threshold in pixels. Moving above this triggers a play attempt.")]
    public float playYThreshold = 250f;

    private PlayerHand2D handManager;
    private TargetingSystem2D targetingSystem;
    private CardTargetType cardTargetType;
    private CardData myCardData;

    private Vector2 layoutPosition;
    private Quaternion layoutRotation;
    private int originalSiblingIndex;
    private bool isHovered = false;
    private bool isDragging = false;
    private bool isDragAllowed = true;

    private RectTransform rectTransform;
    private Canvas mainCanvas;
    private bool isTargetingMode = false;


    public CardData GetCardData()
    {
        return myCardData;
    }
    // delegate enabler
    private void OnEnable()
    {
        DeckManager.OnEnergyChanged += UpdateCostColor;

        StatusController.OnAnyStatusChanged += HandleGlobalStatusChanged;

    }
    
    // delegate disabler
    private void OnDisable()
    {
        DeckManager.OnEnergyChanged -= UpdateCostColor;

        StatusController.OnAnyStatusChanged -= HandleGlobalStatusChanged;

    }
    
    // delegate handler for basic conditions.
    private void HandleGlobalStatusChanged()
    {
        RefreshDynamicDescription(null);
    }
    
    // updates the cost color for a given card, ie when we dont have the energy to play it.
    private void UpdateCostColor(int currentEnergy, int maxEnergy)
    {
        if (costText == null || myCardData == null) return;

        if (myCardData.energyCost > currentEnergy)
        {

            costText.color = expensiveColor;

        }
        else
        {
            costText.color = affordableColor;
        }

    }

    // Call this helper to rebuild text and handle template tag replacements dynamically
    // mainly used to replace the text on cards whenever a status effect updates that data.
    public void RefreshDynamicDescription(CombatEnemy activeTarget = null)
    {
        if (myCardData == null || descriptionText == null) return;

        PlayerStats player = UnityEngine.Object.FindAnyObjectByType<PlayerStats>();

        string parsedDescription = myCardData.Description;

        foreach (CardEffectData effect in myCardData.cardEffectsPool)
        {
            if (effect.effectType == EffectType.DealDamage)
            {
                // If activeTarget is null, it passes null to skip checking Vulnerable on hand-view rendering frames
                int modifiedDamage = DamageCalculator.CalculateModifiedDamage(effect.intensityValue, player, activeTarget);

                string colorHex = "#FFFFFF";
                if (modifiedDamage > effect.intensityValue) colorHex = "#00FF00"; // Green (Buffed)
                else if (modifiedDamage < effect.intensityValue) colorHex = "#FF0000"; // Red (Weakened / Debuffed)

                string replacementString = $"<color={colorHex}>{modifiedDamage}</color>";
                parsedDescription = parsedDescription.Replace("[D]", replacementString);
            }
            else if (effect.effectType == EffectType.GainBlock)
            {
                int modifiedBlock = DamageCalculator.CalculateModifiedBlock(effect.intensityValue, player);

                string colorHex = "#FFFFFF";
                if (modifiedBlock < effect.intensityValue) colorHex = "#FF0000";

                string replacementString = $"<color={colorHex}>{modifiedBlock}</color>";
                parsedDescription = parsedDescription.Replace("[B]", replacementString);
            }
        }

        descriptionText.text = parsedDescription;
    }

    // base class creation call to set variables
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // Automatically find the parent UI canvas for accurate drag math
        mainCanvas = GetComponentInParent<Canvas>();
        targetingSystem = GetComponentInParent<TargetingSystem2D>();
    }

    // initialize object call to create a specific card into the hand.
    public void Initialize(CardData data, PlayerHand2D manager)
    {
        handManager = manager;
        myCardData = data;

        nameText.text = data.Name;
        descriptionText.text = data.Description;
        costText.text = data.energyCost.ToString();
        artworkImage.sprite = data.artwork;
        cardTargetType = data.targetType;


        // initial card energy check when the card is spawned into hand
        if (handManager != null)
        {
            DeckManager deckEngine = handManager.GetComponent<DeckManager>();
            if (deckEngine != null)
            {
                UpdateCostColor(deckEngine.GetCurrentEnergy(), deckEngine.maxEnergy);
            }

        }

        RefreshDynamicDescription();

    }
    
    // sets the position of the card based on values put into it, for updating card transforms during most actions.
    public void SetLayoutTarget(Vector2 position, Quaternion rotation, int siblingIndex)
    {
        layoutPosition = position;
        layoutRotation = rotation;
        originalSiblingIndex = siblingIndex;

        if (!isHovered && !isDragging)
        {
            rectTransform.SetSiblingIndex(siblingIndex);
        }
    }

    // sudo state machine update tick function, sets and changes the states of cards during runtime.
    void Update()
    {
        //STATE: Actively dragging an UNTARGETED card (or a targeted card below the line)
        if (isDragging && !isTargetingMode)
        {
            // Do nothing here—the OnDrag function is directly overriding position to match the mouse cursor!
            return;
        }

        //STATE: Actively targeting an enemy (Card sits frozen in the Preview Slot)
        if (isTargetingMode)
        {
            Vector2 previewPosition = new Vector2(0f, 100f); // Centers the card right above the hand
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, previewPosition, Time.deltaTime * animationSpeed);
            rectTransform.localRotation = Quaternion.Slerp(rectTransform.localRotation, Quaternion.identity, Time.deltaTime * animationSpeed);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one * hoverScale, Time.deltaTime * animationSpeed);
        }
        //STATE: Mouse is hovering over the card normally
        else if (isHovered)
        {
            Vector2 targetPos = new Vector2(layoutPosition.x, layoutPosition.y + hoverYOffset);
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPos, Time.deltaTime * animationSpeed);
            rectTransform.localRotation = Quaternion.Slerp(rectTransform.localRotation, Quaternion.identity, Time.deltaTime * animationSpeed);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one * hoverScale, Time.deltaTime * animationSpeed);
        }
        //STATE: Resting passively inside the hand layout arc
        else
        {
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, layoutPosition, Time.deltaTime * animationSpeed);
            rectTransform.localRotation = Quaternion.Slerp(rectTransform.localRotation, layoutRotation, Time.deltaTime * animationSpeed);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one, Time.deltaTime * animationSpeed);
        }
    }

    // for when we initialy look over a card
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging) return;
        isHovered = true;
        rectTransform.SetAsLastSibling();
    }
    
    // for when we stop looking over a card
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return;
        isHovered = false;
        ResetHierarchyDepth();
        handManager.RefreshSiblingOrders();
    }
    
    // used to adjust card index for either unhovering or undragging.
    public void ResetHierarchyDepth()
    {
        if (!isHovered && !isDragging)
        {
            rectTransform.SetSiblingIndex(originalSiblingIndex);
        }
    }

    // --- DRAG INTERFACE IMPLEMENTATIONS ---

    // event that plays when we click and try dragging a card, will only drag if we have the ability to.
    public void OnBeginDrag(PointerEventData eventData)
    {

        if (!DeckManager.IsPlayerTurn) return;


        DeckManager deckEngine = handManager.GetComponent<DeckManager>();
        if (deckEngine != null)
        {

            if(myCardData.energyCost > deckEngine.GetCurrentEnergy())
            {
                UnityEngine.Debug.LogWarning($"Not enough energy to play {myCardData.Name}! Requires: {myCardData.energyCost}");
                isDragAllowed = false;
                return;
            }

        }


        isDragging = true;
        isHovered = false;
        rectTransform.SetAsLastSibling(); // Render on top of all other cards
        rectTransform.localRotation = Quaternion.identity; // Flatten card out
        rectTransform.localScale = Vector3.one * hoverScale; // Keep scaled up

        // Force an immediate snap to the mouse position on frame one
        MoveCardToMouse(eventData);
    }

    // actual function that implements and checks whether the card is a targeting or untargeting card type,
    // and will spawn the arrow system we have to help user target an entity.
    public void OnDrag(PointerEventData eventData)
    {
        if (!DeckManager.IsPlayerTurn || !isDragAllowed) return;


        // TARGETED CARD BEHAVIOR: Check thresholds based on MOUSE position, not card position
        if (cardTargetType == CardTargetType.Targeted)
        {
            // Convert the screen mouse coordinate accurately into the local coordinate system of the hand anchor
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                handManager.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localMousePoint
            );

            // Check if the MOUSE is above the line, completely independent of where the card slides
            if (localMousePoint.y >= playYThreshold && !isTargetingMode)
            {
                isTargetingMode = true;

                if (targetingSystem != null)
                {
                    Vector2 lowerStartPoint = new Vector2(layoutPosition.x, layoutPosition.y - arrowBaseYOffset);
                    targetingSystem.StartTargeting(lowerStartPoint);
                }
            }
            else if (localMousePoint.y < playYThreshold && isTargetingMode)
            {
                isTargetingMode = false;
                if (targetingSystem != null) targetingSystem.StopTargeting();
            }

            if (isTargetingMode)
            {
                CombatEnemy enemyUnderCursor = GetEnemyUnderMouse(eventData);

                if(enemyUnderCursor != null)
                {
                    // pass the active enemy to calculate specific vulnerable and whatnot.
                    RefreshDynamicDescription(enemyUnderCursor);

                }
                else
                {
                    RefreshDynamicDescription(null);
                }
            }


        }

        // Keep updating positions normally if we are still holding the card below the line
        if (!isTargetingMode)
        {
            MoveCardToMouse(eventData);
        }
    }

    // moves card to mouse based on the mouse event data put into the function.
    private void MoveCardToMouse(PointerEventData eventData)
    {
        // Safely translates screen pixel mouse coordinates into the correct 3D Camera view plane
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPoint))
        {
            rectTransform.position = worldPoint;
        }
    }

    // function for ending a dragging sequence, IE we let go of the card, or play it
    public void OnEndDrag(PointerEventData eventData)
    {

        if (!isDragAllowed)
        {
            isDragAllowed = true;
            CancelCardPlay();
            return;
        }


        // turn check guard
        if (!DeckManager.IsPlayerTurn)
        {

            CancelCardPlay();
            return;
        }
        // back end guard
        DeckManager deckEngine = handManager.GetComponent<DeckManager>();
        if (deckEngine != null && myCardData.energyCost > deckEngine.GetCurrentEnergy())
        {
            UnityEngine.Debug.LogWarning($"Exploit Blocked: Tried to play {myCardData.Name} too fast without enough energy!");
            CancelCardPlay();
            return;
        }



        isDragging = false;
        isHovered = false;
        GetComponent<CanvasGroup>().alpha = 1f; // Force reset visibility flag

        if (targetingSystem != null) targetingSystem.StopTargeting();

        // Targeted Play Processing
        if (cardTargetType == CardTargetType.Targeted)
        {
            if (isTargetingMode)
            {
                isTargetingMode = false;

                // Attempt to find what enemy the pointer cursor dropped onto
                CombatEnemy enemy = GetEnemyUnderMouse(eventData);

                if (enemy != null)
                {
                    PlayCardWithTarget(enemy);
                }
                else
                {
                    // No enemy found, drop card backward into hand
                    handManager.RefreshSiblingOrders();
                }
            }
            else
            {
                handManager.RefreshSiblingOrders();
            }
        }
        // Untargeted Play Processing
        else
        {
            if (rectTransform.anchoredPosition.y >= playYThreshold)
            {
                PlayCardNoTarget();
            }
            else
            {
                handManager.RefreshSiblingOrders();
            }
        }
    }

    private void CancelCardPlay()
    {
        isDragging = false;
        isHovered = false;
        isTargetingMode = false;
        if (targetingSystem != null) targetingSystem.StopTargeting();
        if (handManager != null) handManager.RefreshSiblingOrders();
    }


    // --- RAYCAST ENEMY RESOLUTION MATH ---
    // used to check if we are currently hovering over an enemy with the mouse, checks multiple areas as i was at first unsure of how 
    // we wanted to do it so i just made both.
    private CombatEnemy GetEnemyUnderMouse(PointerEventData eventData)
    {
        // Check UI Canvas elements first (if enemy is a UI Button or Panel)
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            CombatEnemy enemyComponent = result.gameObject.GetComponentInParent<CombatEnemy>();
            if (enemyComponent != null) return enemyComponent;
        }

        // Check 2D Game World elements
        Vector2 mouseWorldPos = mainCanvas.worldCamera.ScreenToWorldPoint(eventData.position);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            return hit.collider.GetComponentInParent<CombatEnemy>();
        }

        return null; // Empty area dropped
    }

    // --- CARD RESOLUTION ACTIVATIONS ---
    // plays the card onto the target entity.
    private void PlayCardWithTarget(CombatEnemy target)
    {
        UnityEngine.Debug.Log($"[PLAY] Active targeted card: {nameText.text} successfully triggered against target: {target.combatantName}!");

        PlayerStats player = UnityEngine.Object.FindAnyObjectByType<PlayerStats>();
        BattleManager battleManager = UnityEngine.Object.FindAnyObjectByType<BattleManager>();



        foreach (CardEffectData effect in myCardData.cardEffectsPool)
        {

            ResolveAndExecuteEffect(effect, player, target, battleManager);

        }


        FinalizeCardPlay();
    }
    
    // plays the card with an untargeting card type.
    private void PlayCardNoTarget()
    {
        UnityEngine.Debug.Log($"[PLAY] Active area effect/buff: {nameText.text} played globally!");

        // Handle standalone player state shifts here (e.g., gain Armor)
        PlayerStats player = UnityEngine.Object.FindAnyObjectByType<PlayerStats>();
        BattleManager battleManager = UnityEngine.Object.FindAnyObjectByType<BattleManager>();

        // go through every effect on a card and run those steps.
        foreach (CardEffectData effect in myCardData.cardEffectsPool)
        {
            // untargeting cards pass null for direct enemy target slot.
            ResolveAndExecuteEffect(effect, player, null, battleManager);

        }


        FinalizeCardPlay();
    }

    // --- THE CENTRAL CORE EFFECT INTERPRETER ENGINE ---
    // used to resolve what filters and other card scenarios we may run into, based on if the card does aoe, multihits, etc
    // along with figuring out who the card is being applied to.
    private void ResolveAndExecuteEffect(CardEffectData effect, PlayerStats player, CombatEnemy directTarget, BattleManager battleManager)
    {
        // Compile target list
        List<CharacterStats> targetList = new List<CharacterStats>();
        List<CombatEnemy> activeEnemies = battleManager != null ? battleManager.GetLivingEnemies() : new List<CombatEnemy>();

        switch (effect.targetFilter)
        {
            case TargetFilter.SingleTarget:
                if (directTarget != null) targetList.Add(directTarget);
                break;

            case TargetFilter.AllEnemies:
                // aoe stuff: Add every single living enemy on the battlefield to the hit list
                foreach (CombatEnemy enemy in activeEnemies)
                {
                    if (enemy != null) targetList.Add(enemy);
                }
                break;

            case TargetFilter.Player:
                if (player != null) targetList.Add(player);
                break;

            case TargetFilter.RandomEnemy:
                // Handled uniquely inside the multi-hit loop below to pick a fresh target per bounce
                break;
        }

        // process multihit repetition loops
        for (int hit = 0; hit < effect.repeatCount; hit++)
        {
            // If the effect is a randomized target splitter
            if (effect.targetFilter == TargetFilter.RandomEnemy && activeEnemies.Count > 0)
            {
                int randIndex = UnityEngine.Random.Range(0, activeEnemies.Count);
                CombatEnemy randomSelection = activeEnemies[randIndex];
                if (randomSelection != null) ProcessSingleEffectActivation(effect, randomSelection, player);
            }
            else
            {
                // For standard single, player, or full-board AOE operations, execute across compiled list
                foreach (CharacterStats recipient in targetList)
                {
                    if (recipient == null) continue;
                    ProcessSingleEffectActivation(effect, recipient, player);
                }
            }
        }
    }

    // Resolves the baseline math arithmetic action directly onto the character instance
    private void ProcessSingleEffectActivation(CardEffectData effect, CharacterStats recipient, PlayerStats playerRef)
    {
        switch (effect.effectType)
        {
            case EffectType.DealDamage:
                // calculate full damage modifications (strength weak etc)
                int finalDamage = DamageCalculator.CalculateModifiedDamage(effect.intensityValue, playerRef, recipient);
                recipient.TakeDamage(finalDamage);
                break;

            case EffectType.GainBlock:
                // Cast onto whoever holds the recipient reference node
                int finalBlock = DamageCalculator.CalculateModifiedBlock(effect.intensityValue, recipient);
                recipient.AddBlock(finalBlock);
                break;

            case EffectType.DrawCards:
                DeckManager deck = UnityEngine.Object.FindAnyObjectByType<DeckManager>();
                if (deck != null) deck.DrawCards(effect.intensityValue);
                break;

            case EffectType.GainEnergy:
                DeckManager energyEngine = UnityEngine.Object.FindAnyObjectByType<DeckManager>();
                if (energyEngine != null) energyEngine.SpendEnergy(-effect.intensityValue);
                break;

            case EffectType.ApplyStatusEffect:
                if (effect.statusToApply != null && recipient.StatusEngine != null)
                {
                    recipient.StatusEngine.ApplyStatus(effect.statusToApply, effect.intensityValue);
                }
                break;
        }
    }

    // used to update the hand UI and deck effects to update most UI instances that revolve around playing cards.
    private void FinalizeCardPlay()
    {
        DeckManager deckEngine = handManager.GetComponent<DeckManager>();
        if (deckEngine != null)
        {

            deckEngine.SpendEnergy(myCardData.energyCost);

            deckEngine.DiscardCard(myCardData);
        }

        handManager.RemoveCardFromHand(this);
        Destroy(gameObject);
    }

    // from my knowledge I stopped using this, so its deprecated but i don't remember if something else is using it. (whoops)
    private void PlayCard()
    {
        UnityEngine.Debug.Log($"Successfully Played: {nameText.text}!");

        // Remove this card from the layout management array
        handManager.RemoveCardFromHand(this);

        // Destroy the card UI object
        Destroy(gameObject);
    }



}
