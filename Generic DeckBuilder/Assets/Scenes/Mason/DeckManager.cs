using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("Deck Sources")]
    [Tooltip("All the cards a player brings into combat.")]
    public List<CardData> masterDeck = new List<CardData>();

    // Live runtime trackers
    private List<CardData> drawPile = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();

    private PlayerHand2D handSystem;

    public List<CardData> GetDrawPile() => drawPile;
    public List<CardData> GetDiscardPile() => discardPile;

    void Awake()
    {
        handSystem = GetComponent<PlayerHand2D>();
    }

    void Start()
    {
        // 1. Initialize combat by adding master deck cards to draw pile
        drawPile.AddRange(masterDeck);

        // 2. Initial shuffle
        ShufflePile(drawPile);

        // 3. Draw an initial starting hand of 5 cards
        StartNewTurn(5);
    }

    // Retain a reference to hand manager to pull current cards out of it
    public void StartNewTurn(int cardsToDraw)
    {

        // 1. Draw a fresh hand of cards
        DrawCards(cardsToDraw);
    }

    public void EndCurrentTurn()
    {

        // 1. Gather all remaining cards sitting in the player's visual hand layout
        List<CardController> remainingHandCards = handSystem.GetCardsInHand();

        // 2. Loop backwards to safely dump their data profiles into the discard heap
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

        // 3. TRIGGER ENEMY TURN HERE
        // For now, we will simulate a quick 1-second enemy turn before passing it back
        StartCoroutine(SimulateEnemyTurn());
    }

    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
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

    private System.Collections.IEnumerator SimulateEnemyTurn()
    {
        yield return new WaitForSeconds(1.0f); // Delays execution for 1 second

        // Return control back to the player loop phase
        StartNewTurn(5);
    }
}
