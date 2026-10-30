using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

// manager used to display UI elements for gold rewards and card rewards,
public class RewardManager : MonoBehaviour
{


    [Header("UI System Panels")]
    public RewardScreenUI rewardScreenUI;
    public CardDraftUI cardDraftUI;

    [Header("Visual Asset Overrides")]
    public Sprite goldIcon;
    public Sprite cardRewardIcon;

    [Header("Global Card Pool References")]
    [Tooltip("Drop every basic card in your game here so the system can choose 3 random options.")]
    public List<CardData> absoluteMasterCardPool = new List<CardData>();

    private List<RewardItem> currentActiveRewards = new List<RewardItem>();

    // Call this method when all combat enemies are dead
    public void OpenCombatRewards(int minGold, int maxGold)
    {
        currentActiveRewards.Clear();

        //Roll Random Gold Amount
        int rolledGold = UnityEngine.Random.Range(minGold, maxGold + 1);
        currentActiveRewards.Add(new RewardItem(rolledGold, goldIcon));

        //Generate 3 Random Distinct Cards for the Choice Draft
        List<CardData> cardChoices = RollRandomCards(3);
        currentActiveRewards.Add(new RewardItem(cardChoices, cardRewardIcon));

        //Command the UI to open up and display the items cleanly
        rewardScreenUI.OpenPanel(currentActiveRewards, this);
    }

    public void ClaimReward(RewardItem reward)
    {
        if (reward.isClaimed) return;

        switch (reward.type)
        {
            case RewardType.Gold:
                reward.isClaimed = true;
                // setup gold inventory stuff here, isnt implemented yet
                UnityEngine.Debug.Log($"[REWARD CLAIMED] Player gained +{reward.goldAmount} Gold!");
                break;

            case RewardType.CardDraft:
                // Do NOT mark as claimed yet, open the overlay so the player makes a choice first
                cardDraftUI.OpenDraftOverlay(reward.draftOptions, this, reward);
                break;
        }

        // Refresh layout displays
        rewardScreenUI.RefreshListDisplay();
    }

    public void CompleteCardDraft(CardData selectedInterfaceCard, RewardItem originalRewardNode)
    {
        // Add the chosen card structure into your Master Deck runtime storage list
        DeckManager deckEngine = UnityEngine.Object.FindAnyObjectByType<DeckManager>();
        if (deckEngine != null)
        {
            deckEngine.masterDeck.Add(selectedInterfaceCard);
            UnityEngine.Debug.Log($"[DECK ADDITION] Successfully injected {selectedInterfaceCard.Name} into master deck sources.");
        }

        // Mark the reward item node on the primary screen as taken
        originalRewardNode.isClaimed = true;

        cardDraftUI.CloseDraftOverlay();
        rewardScreenUI.RefreshListDisplay();
    }

    private List<CardData> RollRandomCards(int amountNeeded)
    {
        List<CardData> outputList = new List<CardData>();
        if (absoluteMasterCardPool.Count < amountNeeded) return new List<CardData>(absoluteMasterCardPool);

        // Copy list to shuffle cleanly without damaging game arrays
        List<CardData> temporaryPool = new List<CardData>(absoluteMasterCardPool);

        for (int i = 0; i < amountNeeded; i++)
        {
            int index = UnityEngine.Random.Range(0, temporaryPool.Count);
            outputList.Add(temporaryPool[index]);
            temporaryPool.RemoveAt(index); // Prevent identical card duplicates in draft choices
        }

        return outputList;
    }


}
