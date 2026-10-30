using System.Collections.Generic;
using UnityEngine;

public enum RewardType { Gold, CardDraft}

// class used to describe each reward the player COULD get at the end of a combat. right now its just gold and card rewards.
[System.Serializable]
public class RewardItem
{
    public RewardType type;
    public string displayName;
    public Sprite icon;

    [Header("Gold Settings")]
    public int goldAmount;

    [Header("Card Settings")]
    [Tooltip("The generated pool of card selections held open for drafting options.")]
    public List<CardData> draftOptions = new List<CardData>();

    [HideInInspector] public bool isClaimed = false;

    //helpers to generate structural instances
    public RewardItem(int amount, Sprite goldIcon)
    {
        this.type = RewardType.Gold;
        this.displayName = $"{amount} Gold";
        this.icon = goldIcon;
        this.goldAmount = amount;
    }

    public RewardItem(List<CardData> cards, Sprite cardIcon)
    {
        this.type = RewardType.CardDraft;
        this.displayName = "Add a Card to your Deck";
        this.icon = cardIcon;
        this.draftOptions = cards;
    }
}