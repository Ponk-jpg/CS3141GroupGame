using UnityEngine;
using TMPro;

public class ExhaustPromptUI : MonoBehaviour
{
    [Header("Visual Layout Sub-Panel Anchor")]
    public GameObject visualPromptContainer;

    [Header("Text Injection Slot")]
    [Tooltip("Drag the TextMeshPro component that displays the prompt message here.")]
    public TextMeshProUGUI promptLabelText;


    private void OnEnable()
    {
        DeckManager.OnExhaustSelectionStateChanged += TogglePromptVisibility;

        // Make sure it starts hidden by default when entering a scene
        if (visualPromptContainer != null) visualPromptContainer.SetActive(false);
    }

    private void OnDisable()
    {
        // Always clean up subscriptions to avoid memory leaks
        DeckManager.OnExhaustSelectionStateChanged -= TogglePromptVisibility;
    }

    private void TogglePromptVisibility(int cardsNeededCount)
    {
        if (visualPromptContainer == null) return;

        if (cardsNeededCount <= 0)
        {

            visualPromptContainer.SetActive(false);
            return;
        }

        if (promptLabelText != null)
        {

            if (cardsNeededCount == 1)
            {

                promptLabelText.text = "Select <color=#FF5555>1</color> Card in Hand to Exhaust...";

            }
            else
            {

                promptLabelText.text = $"Select <color=#FF5555>{cardsNeededCount}</color> Cards in Hand to Exhaust...";

            }

        }


        visualPromptContainer.SetActive(true);
    }
}
