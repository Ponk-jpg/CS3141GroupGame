using UnityEngine;
using UnityEngine.UI;
using TMPro;

// basic UI class for a given status effect, hold the image and stack amount if applicable.
public class StatusIconUI : MonoBehaviour
{
    public UnityEngine.UI.Image statusIconImage;
    public TextMeshProUGUI stackCountText;

    // make sure to attach this to our ui prefab containing the image component and textmeshproUGUI component for the image and stack number overlay.

    // Keep a reference to the active data so future tooltip raycasts can read from it
    public StatusEffectData AssociatedData { get; private set; }

    public void SetupIcon(StatusController.ActiveStatus statusInstance)
    {
        AssociatedData = statusInstance.data;
        statusIconImage.sprite = statusInstance.data.icon;

        // hide number if its an unstacked or a single turn effect
        if (statusInstance.stacks <= 1 && !statusInstance.data.decaysAtEndOfTurn)
        {
            stackCountText.text = "";
        }
        else
        {
            stackCountText.text = statusInstance.stacks.ToString();
        }
    }




}
