using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


// UI class used to view the discard, draw, total deck, and exhaust pile decks.
public class DeckViewerUI : MonoBehaviour
{
    [Header("UI Visual Elements")]
    public TextMeshProUGUI panelTitleText;
    public Transform gridContentParent;
    public Button closeButton;

    [Header("Prefabs")]
    [Tooltip("UI_CardPrefab asset file")]
    public GameObject cardPrefabTemplate;

    private List<GameObject> activeDisplayClones = new List<GameObject>();

    void Awake()
    {
        // Wire up the close button listener interface manually
        closeButton.onClick.AddListener(ClosePanel);
    }

    public void OpenPanel(string title, List<CardData> targetPile)
    {
        ClearOldClones();

        gameObject.SetActive(true);
        panelTitleText.text = $"{title} ({targetPile.Count})"; // Displays total count next to name

        foreach (CardData cardData in targetPile)
        {
            GameObject displayCard = Instantiate(cardPrefabTemplate, gridContentParent);

            //STABILIZE SCALE AND ROTATION FOR GRID VIEW
            RectTransform cardTransform = displayCard.GetComponent<RectTransform>();
            cardTransform.localScale = Vector3.one;       // Reset to 100% normal size
            cardTransform.localRotation = Quaternion.identity; // Remove any layout curves/tilts

            //DISCONNECT HAND BEHAVIORS
            if (displayCard.TryGetComponent<CardController>(out var controller))
            {
                controller.Initialize(cardData, null);
                controller.enabled = false; // Completely disables the script update loops
            }

            if (displayCard.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 1f; // Ensure it's fully opaque and visible
            }

            activeDisplayClones.Add(displayCard);

        }
    }
    public void ClosePanel()
    {
        ClearOldClones();
        gameObject.SetActive(false);
    }

    private void ClearOldClones()
    {
        foreach (GameObject clone in activeDisplayClones)
        {
            if (clone != null) Destroy(clone);
        }
        activeDisplayClones.Clear();
    }







}
