using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

// class used to update the main UI of combat for the user, along with maintaining other important variables
public class DeckManager : MonoBehaviour
{

    public static Action<int> OnExhaustSelectionStateChanged;

    public static Action<bool> OnPlayerTurnStateChanged;
    public static bool IsPlayerTurn { get; private set; } = true;

    public static Action<int, int> OnEnergyChanged; // broadcasts (current Energy, Max Energy )
    [Header("Energy Configuration")]
    public int maxEnergy = 3;
    private int currentEnergy = 0;

    [Header("Deck Sources")]
    [Tooltip("All the cards a player brings into combat.")]
    public List<CardData> masterDeck = new List<CardData>();

    [Header("Hand Settings")]
    [Tooltip("Max num of cards a player can hold at once.")]
    public int maxHandSize = 10;


    // Live runtime trackers
    private List<CardData> drawPile = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();
    private List<CardData> exhaustPile = new List<CardData>();

    private PlayerHand2D handSystem;

    public static bool IsInExhaustSelectionMode { get; private set; } = false;
    private CardEffectData activeExhaustEffectConfig;
    private CardController sourceExhaustCard;
    private CharacterStats capturedEnemyTarget;
    private PlayerStats capturedPlayerRef;

    // Counters to handle multi-card constraints tracking
    private int currentExhaustedCount = 0;
    private int requiredExhaustCount = 0;


    public List<CardData> GetDrawPile() => drawPile;
    public List<CardData> GetDiscardPile() => discardPile;
    public List<CardData> GetExhaustPile() => exhaustPile;
    public List<CardData> GetMasterDeck() => masterDeck;

    public int GetCurrentEnergy() => currentEnergy;

    void Awake()
    {
        handSystem = GetComponent<PlayerHand2D>();
    }

    void Start()
    {

        IsPlayerTurn = true;

    }

    // Retain a reference to hand manager to pull current cards out of it
    public void StartNewTurn(int cardsToDraw)
    {

        IsPlayerTurn = true;
        OnPlayerTurnStateChanged?.Invoke(true);

        currentEnergy = maxEnergy;
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

        // Draw a fresh hand of cards
        DrawCards(cardsToDraw);
    }

    public void ResetDeckForNewEncounter()
    {
        drawPile.Clear();
        discardPile.Clear();
        exhaustPile.Clear();

        if (masterDeck == null || masterDeck.Count == 0)
        {

            UnityEngine.Debug.LogError("Deck manager: Master Deck is empty");
            return;

        }

        drawPile.AddRange(masterDeck);

        ShufflePile(drawPile);

        UnityEngine.Debug.Log($"[DECK RESET] successfully initialized {drawPile.Count} total cards");

    }


    // function that happens when we end turn, mainly used to update deck values.
    public void EndCurrentTurn()
    {

        IsPlayerTurn = false;
        OnPlayerTurnStateChanged?.Invoke(false);


        //Gather all remaining cards sitting in the player's visual hand layout
        List<CardController> remainingHandCards = handSystem.GetCardsInHand();

        //Loop backwards to safely dump their data profiles into the discard heap
        for (int i = remainingHandCards.Count - 1; i >= 0; i--)
        {
            CardController card = remainingHandCards[i];

            // Route data to discard array tracking list
            if (card != null)
            {
                // We use a public property hook inside CardController to grab its data structure
                discardPile.Add(card.GetCardData());

                // Remove it from layout array tracking structures
                handSystem.RemoveCardFromHand(card);

                // Destroy its visual physical UI object instance block
                Destroy(card.gameObject);
            }
        }

        LogPileCounts();

    }

    public void SpendEnergy(int amount)
    {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;

        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

    }


    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {

            if (handSystem != null && handSystem.GetCardsInHand().Count >= maxHandSize)
            {
                UnityEngine.Debug.Log($"[DRAW STOPPED] Hand is completely full ({maxHandSize}/{maxHandSize}). Leaving remaining cards on top of the Draw Pile.");
                break;
            }


            // If the draw pile is empty, try to reshuffle the discard pile back in
            if (drawPile.Count == 0)
            {
                ReshuffleDiscardIntoDraw();
            }

            // If both piles are empty, stop drawing entirely
            if (drawPile.Count == 0)
            {
                UnityEngine.Debug.LogWarning("No cards available to draw in either pile!");
                return;
            }

            // Pull the top card (index 0) from the draw pile
            CardData cardToDraw = drawPile[0];
            drawPile.RemoveAt(0);

            // Pass it to our hand layout script to spawn the actual UI card element
            handSystem.AddCardToHand(cardToDraw);
        }

        LogPileCounts();
    }

    public void DiscardCard(CardData cardData)
    {
        discardPile.Add(cardData);
        LogPileCounts();
    }

    public void ExhaustCard(CardData cardData)
    {
        exhaustPile.Add(cardData);
        
    }

    // used for whenever a card the user plays or a enemy attack adds a card to our hand,
    // hasn't been tested yet but should work as intended.
    public void GenerateCardMidCombat(CardData specializedCard)
    {
        UnityEngine.Debug.Log($"[CARD GENERATION] Outside source attempting to add: {specializedCard.Name}");

        // If an outside source adds a card while your hand is full, it overflows to discard
        if (handSystem != null && handSystem.GetCardsInHand().Count >= maxHandSize)
        {
            UnityEngine.Debug.Log($"[HAND FULL] Outside card {specializedCard.Name} overflowed straight into the Discard Pile.");
            DiscardCard(specializedCard);
        }
        else
        {
            if (handSystem != null)
            {
                handSystem.AddCardToHand(specializedCard);
            }
        }

    }

    // Call this to turn on hand selection mode
    public void EnterInHandExhaustMode(CardEffectData effect, CardController sourceCard, PlayerStats player, CharacterStats enemyTarget)
    {
        IsInExhaustSelectionMode = true;
        activeExhaustEffectConfig = effect;
        sourceExhaustCard = sourceCard;
        capturedPlayerRef = player;
        capturedEnemyTarget = enemyTarget;

        currentExhaustedCount = 0;
        requiredExhaustCount = effect.cardsToExhaustCount;

        UnityEngine.Debug.Log("[HAND SELECTION] Entered Exhaust Selection Mode. Click a card in your hand to burn it!");

        // Trigger an event here if you want a global UI text label to tell the player "Select a Card to Exhaust"
        OnExhaustSelectionStateChanged?.Invoke(requiredExhaustCount - currentExhaustedCount);

        ToggleEndTurnButtonAvailability(false);

    }

    // Call this from a clicked card to execute the payoff payload
    public void ProcessInHandCardSelection(CardController selectedHandCard)
    {
        if (!IsInExhaustSelectionMode || selectedHandCard == sourceExhaustCard) return;


        PlayerHand2D handSystem = UnityEngine.Object.FindAnyObjectByType<PlayerHand2D>();
        if (handSystem == null || activeExhaustEffectConfig == null) return;

        UnityEngine.Debug.Log($"[EXHAUST SELECTION] Player clicked directly on {selectedHandCard.myCardData.Name} in hand!");

        // Burn the selected card into the backend exhaust tracking lists
        ExhaustCard(selectedHandCard.myCardData);

        //Remove it visually from the hand tracks and wipe its scene object
        handSystem.RemoveCardFromHand(selectedHandCard);
        Destroy(selectedHandCard.gameObject);

        currentExhaustedCount++;

        //Resolve payoffs using the exact payload wrapper function we set up inside CardController
        if (sourceExhaustCard != null)
        {
            // We pass an explicit list containing exactly 1 card to reuse your scalable payload math
            List<CardController> wrapperList = new List<CardController>() { selectedHandCard };
            sourceExhaustCard.ExecuteExhaustionPayload(wrapperList, activeExhaustEffectConfig, capturedPlayerRef, capturedEnemyTarget, this, handSystem);
        }

        int remainingBurnableCards = handSystem.GetCardsInHand().Count;
        if (sourceExhaustCard != null && handSystem.GetCardsInHand().Contains(sourceExhaustCard))
        {
            remainingBurnableCards--;
        }


        if (currentExhaustedCount >= requiredExhaustCount || remainingBurnableCards <= 0)
        {

            IsInExhaustSelectionMode = false;

            OnExhaustSelectionStateChanged?.Invoke(0);

            ToggleEndTurnButtonAvailability(true);

            activeExhaustEffectConfig = null;
            sourceExhaustCard = null;
            capturedPlayerRef = null;
            capturedEnemyTarget = null;

        }
        else
        {

            int remainingNeeded = requiredExhaustCount - currentExhaustedCount;
            OnExhaustSelectionStateChanged?.Invoke(remainingNeeded);
        }
    }

    private void ToggleEndTurnButtonAvailability(bool isAllowedToClick)
    {

        TurnButtonController EndTurnBtn = UnityEngine.Object.FindAnyObjectByType<TurnButtonController>();
        if (EndTurnBtn != null)
        {

            UnityEngine.UI.Button rawButton = EndTurnBtn.GetComponent<UnityEngine.UI.Button>();
            if (rawButton != null) rawButton.interactable = isAllowedToClick;

        }
        

    }


    private void ReshuffleDiscardIntoDraw()
    {
        if (discardPile.Count == 0) return;

        UnityEngine.Debug.Log("Draw Pile empty! Shuffling Discard Pile back into Draw Pile...");
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShufflePile(drawPile);
    }

    private void ShufflePile(List<CardData> pile)
    {
        // Fisher-Yates Shuffle Math Algorithm
        for (int i = pile.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            CardData temp = pile[i];
            pile[i] = pile[randomIndex];
            pile[randomIndex] = temp;
        }
    }

    private void LogPileCounts()
    {
        UnityEngine.Debug.Log($"[DECK STATUS] Draw Pile: {drawPile.Count} | Discard Pile: {discardPile.Count}");
    }

}
