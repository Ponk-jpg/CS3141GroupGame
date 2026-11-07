using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardSelectionOverlayUI : MonoBehaviour
{
    [Header("UI Structure Anchors")]
    public TextMeshProUGUI promptTitleText;
    public Transform gridLayoutContentParent;
    public Button confirmSelectionButton;

    [Header("Prefabs")]
    [Tooltip("Pass your standard UI_DraftDisplayCard prefab template here.")]
    public GameObject visualDisplayCardPrefab;

    private List<CardController> originalHandCardsRef = new List<CardController>();
    private List<CardController> selectedCardsForExhaust = new List<CardController>();
    private List<GameObject> spawnedGridItems = new List<GameObject>();

    private int requiredSelectionCount = 1;
    private Action<List<CardController>> onCompleteCallback;

    void Awake()
    {
        if (confirmSelectionButton != null)
        {
            confirmSelectionButton.onClick.AddListener(OnConfirmButtonClicked);
        }
    }

    // API INTERFACE
    public void OpenSelectionPanel(string promptMessage, List<CardController> availableCards, int countNeeded, Action<List<CardController>> callbackClosure)
    {
        gameObject.SetActive(true);
        promptTitleText.text = promptMessage;

        requiredSelectionCount = countNeeded;
        onCompleteCallback = callbackClosure;

        originalHandCardsRef = availableCards;
        selectedCardsForExhaust.Clear();

        // Disable the confirm button until the player selects the exact amount of cards required
        confirmSelectionButton.interactable = false;

        RebuildSelectionGrid();
    }

    private void RebuildSelectionGrid()
    {
        // Wipe old visual elements
        foreach (GameObject item in spawnedGridItems) if (item != null) Destroy(item);
        spawnedGridItems.Clear();

        foreach (CardController handCard in originalHandCardsRef)
        {
            if (handCard == null) continue;

            GameObject gridItem = Instantiate(visualDisplayCardPrefab, gridLayoutContentParent);
            spawnedGridItems.Add(gridItem);

            // Turn off dynamic dragging behaviors so they sit nicely in the layout slots
            if (gridItem.TryGetComponent<CardController>(out var viewCtrl))
            {
                viewCtrl.Initialize(handCard.myCardData, null);
                viewCtrl.enabled = false;
            }

            // Style the visual component button to register selections
            Button btn = gridItem.GetComponent<Button>();
            if (btn == null) btn = gridItem.AddComponent<Button>();
            btn.interactable = true;

            // Highlight border overlay feedback if this specific card layout row is currently toggled selected
            UnityEngine.UI.Image bgImage = gridItem.GetComponent<UnityEngine.UI.Image>();
            bool isSelected = selectedCardsForExhaust.Contains(handCard);
            if (bgImage != null) bgImage.color = isSelected ? Color.green : Color.white;

            // Tie selection listeners directly to tracking arrays
            CardController capturedCardRef = handCard; // Fix variable scope hoisting closures trap
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => ToggleCardSelection(capturedCardRef, bgImage));
        }
    }

    private void ToggleCardSelection(CardController targetedCard, UnityEngine.UI.Image cardBg)
    {
        if (selectedCardsForExhaust.Contains(targetedCard))
        {
            selectedCardsForExhaust.Remove(targetedCard);
            if (cardBg != null) cardBg.color = Color.white;
        }
        else
        {
            // Limit checking constraints gate
            if (selectedCardsForExhaust.Count >= requiredSelectionCount)
            {
                // If trying to select more than allowed, unselect the oldest choice to make room
                selectedCardsForExhaust.RemoveAt(0);
            }

            selectedCardsForExhaust.Add(targetedCard);
            if (cardBg != null) cardBg.color = Color.green;
        }

        // Force rebuild grids to update visual colors
        RebuildSelectionGrid();

        // Unlock confirm button when requirements are met
        if (confirmSelectionButton != null)
        {
            confirmSelectionButton.interactable = (selectedCardsForExhaust.Count == requiredSelectionCount || selectedCardsForExhaust.Count == originalHandCardsRef.Count);
        }
    }

    private void OnConfirmButtonClicked()
    {
        // Fire callback to return selections back to the calling CardController instance pipeline
        onCompleteCallback?.Invoke(selectedCardsForExhaust);
        ClosePanel();
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
