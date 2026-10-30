using UnityEngine;
using UnityEngine.UI;
using TMPro;

// UI class used to display a specific reward into our total rewards container
public class RewardRowBannerUI : MonoBehaviour
{
    public Button actionButton;
    public UnityEngine.UI.Image iconImage;
    public TextMeshProUGUI statusLabelText;

    private RewardItem dynamicRewardLink;
    private RewardManager systemEngine;

    public void SetupRow(RewardItem reward, RewardManager engine)
    {
        dynamicRewardLink = reward;
        systemEngine = engine;

        iconImage.sprite = reward.icon;
        statusLabelText.text = reward.displayName;

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnRowClicked);

        // Gray out visual clarity if already claimed
        actionButton.interactable = !reward.isClaimed;
        statusLabelText.alpha = reward.isClaimed ? 0.4f : 1.0f;
    }

    private void OnRowClicked()
    {
        systemEngine.ClaimReward(dynamicRewardLink);
    }
}
