
using System.Collections.Generic;
using UnityEngine;

public class RestStopDeckManager : MonoBehaviour
{
    [Header("Deck Display References")]
    public Transform cardContainer;
    public GameObject cardPrefab;
    public GameObject deckRemovalPanel;
    public Transform viewCardContainer;
    public GameObject deckViewPanel;

    private DeckManager deckManager;

    private void Start()
    {
        deckManager = FindAnyObjectByType<DeckManager>();

        if (deckManager == null)
        {
            Debug.LogError("[REST STOP] Could not find DeckManager!");
            return;
        }

        // Hide the deck until the player clicks Remove Card
        deckRemovalPanel.SetActive(false);
        deckViewPanel.SetActive(false);
    }

    public void OpenDeckRemovalPanel()
    {
        deckRemovalPanel.SetActive(true);
        DisplayDeck();
    }

    public void DisplayDeck()
    {
        if (cardContainer == null || cardPrefab == null)
        {
            Debug.LogError("[REST STOP] Card display references are missing!");
            return;
        }

        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }

        List<CardData> masterDeck = deckManager.GetMasterDeck();

        foreach (CardData card in masterDeck)
        {
            GameObject newCard = Instantiate(cardPrefab, cardContainer);

            RestStopCardView cardView =
                newCard.GetComponent<RestStopCardView>();

            if (cardView != null)
            {
                cardView.Initialize(card, RemoveSelectedCard);
            }
        }
    }

    private void RemoveSelectedCard(RestStopCardView selectedCard)
    {
        CardData cardData = selectedCard.GetCardData();

        if (deckManager.GetMasterDeck().Remove(cardData))
        {
            Debug.Log("[REST STOP] Removed " + cardData.Name + " from the master deck.");

            Destroy(selectedCard.gameObject);
        }
    }

    public void CloseDeckRemovalPanel()
    {
        deckRemovalPanel.SetActive(false);
    }

    public void OpenDeckViewPanel()
    {
        deckViewPanel.SetActive(true);
        DisplayDeckForViewing();
    }

    public void DisplayDeckForViewing()
    {
        if (viewCardContainer == null || cardPrefab == null)
        {
            Debug.LogError("[REST STOP] View Deck references are missing!");
            return;
        }

        foreach (Transform child in viewCardContainer)
        {
            Destroy(child.gameObject);
        }

        List<CardData> masterDeck = deckManager.GetMasterDeck();

        foreach (CardData card in masterDeck)
        {
            GameObject newCard = Instantiate(cardPrefab, viewCardContainer);

            RestStopCardView cardView =
                newCard.GetComponent<RestStopCardView>();

            if (cardView != null)
            {
                cardView.Initialize(card, null);
            }
        }
    }

    public void CloseDeckViewPanel()
    {
        deckViewPanel.SetActive(false);
    }

    public void CloseAllPanels()
    {
        deckRemovalPanel.SetActive(false);
        deckViewPanel.SetActive(false);
    }


}
