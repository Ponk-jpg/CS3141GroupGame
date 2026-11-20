
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RestStopCardView : MonoBehaviour
{
    [Header("Card UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public Image artworkImage;
    public Image backgroundImage;
    public Button selectButton;

    private CardData cardData;
    private Action<RestStopCardView> onSelected;

    public void Initialize(CardData data, Action<RestStopCardView> selectionCallback)
    {
        cardData = data;
        onSelected = selectionCallback;

        nameText.text = data.Name;
        descriptionText.text = data.Description;
        costText.text = data.energyCost.ToString();
        artworkImage.sprite = data.artwork;
        backgroundImage.sprite = data.Background;

        selectButton.onClick.RemoveAllListeners();

        selectButton.interactable = selectionCallback != null;

        if (selectionCallback != null)
        {
            selectButton.onClick.AddListener(() => onSelected?.Invoke(this));
        }
    }

    public CardData GetCardData()
    {
        return cardData;
    }
}
