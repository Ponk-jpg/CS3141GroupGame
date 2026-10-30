using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

// screen used to hold all of the rewards a player gets at the end of combat.
public class RewardScreenUI : MonoBehaviour
{
    // attach this script to your main rewards panel. it needs a vertical layout group container object to hold spawned rows, and an exit button.


    [Header("UI Structure Anchors")]
    public Transform listVerticalContentParent;
    public Button skipProceedButton;

    [Header("Prefabs")]
    public GameObject rewardRowPrefab;

    private List<RewardItem> trackingRewardsRef;
    private RewardManager managerRef;
    private List<GameObject> activeRows = new List<GameObject>();

    void Awake()
    {
        skipProceedButton.onClick.AddListener(CloseAndProceedToNextMapNode);
    }

    public void OpenPanel(List<RewardItem> rewards, RewardManager manager)
    {
        trackingRewardsRef = rewards;
        managerRef = manager;

        gameObject.SetActive(true);
        RefreshListDisplay();
    }

    public void RefreshListDisplay()
    {
        // Sweep old instantiations away safely
        foreach (GameObject row in activeRows) if (row != null) Destroy(row);
        activeRows.Clear();

        // Spawn dynamic button links
        foreach (RewardItem item in trackingRewardsRef)
        {
            if (item.isClaimed && item.type == RewardType.CardDraft) continue; // Hide card choice row once draft closes

            GameObject rowObj = Instantiate(rewardRowPrefab, listVerticalContentParent);
            RewardRowBannerUI rowScript = rowObj.GetComponent<RewardRowBannerUI>();
            if (rowScript != null)
            {
                rowScript.SetupRow(item, managerRef);
            }
            activeRows.Add(rowObj);
        }
    }

    private void CloseAndProceedToNextMapNode()
    {
        gameObject.SetActive(false);
        UnityEngine.Debug.Log("[MAP TRANSITION] Closed rewards layout. Loading node maps systems back up...");
        // go to next encounter or something similar here.
    }
}
