using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// used as a rewards panel to display 3 cards for the player to choose from to add to their deck, or skip entirely.
public class CardDraftUI : MonoBehaviour
{
    // attach this script to a distinct panel overlay containing 3 horizontal slot positons. drop a standard UI card asset clone template into each position.

    [Header("Layout Content Grid")]
    public Transform horizontalDraftContainerParent;

    [Header("Prefabs")]
    public GameObject visualDisplayCardPrefab;

    [Header("Skip Settings")]
    public Button skipDraftButton;

    private List<GameObject> spawnedCards = new List<GameObject>();

    // called to initialize and setup the draft UI.
    public void OpenDraftOverlay(List<CardData> options, RewardManager manager, RewardItem trackingNode)
    {
        gameObject.SetActive(true);

        foreach (GameObject oldCard in spawnedCards) if (oldCard != null) Destroy(oldCard);
        spawnedCards.Clear();

        // Wire up a "Skip Cards Choice" structural pass link
        skipDraftButton.onClick.RemoveAllListeners();
        skipDraftButton.onClick.AddListener(() => {
            trackingNode.isClaimed = true; // Still consume reward node link
            CloseDraftOverlay();
            manager.rewardScreenUI.RefreshListDisplay();
        });

        // Spawn choice candidates cards
        foreach (CardData card in options)
        {
            GameObject cardUI = Instantiate(visualDisplayCardPrefab, horizontalDraftContainerParent);

            // Clean up behaviors so they look pretty sitting still in standard displays grids
            if (cardUI.TryGetComponent<CardController>(out var controller))
            {
                controller.Initialize(card, null);
                controller.enabled = false;
            }

            // Inject a unique dynamic button overlay link onto the spawned choices cards 
            Button chooseBtn = cardUI.GetComponent<Button>();
            if (chooseBtn == null) chooseBtn = cardUI.AddComponent<Button>();

            chooseBtn.onClick.RemoveAllListeners();
            chooseBtn.onClick.AddListener(() => manager.CompleteCardDraft(card, trackingNode));

            spawnedCards.Add(cardUI);
        }
    }

    public void CloseDraftOverlay()
    {
        gameObject.SetActive(false);
    }
}
