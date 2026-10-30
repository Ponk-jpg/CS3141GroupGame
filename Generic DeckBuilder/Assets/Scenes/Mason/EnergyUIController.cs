using UnityEngine;
using TMPro;

// class used to update energy UI during combat.
public class EnergyUIController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text element displaying current energy format, e.g., '3 / 3'")]
    public TextMeshProUGUI energyText;

    private void OnEnable()
    {
        // Listen for energy updates from the engine
        DeckManager.OnEnergyChanged += UpdateEnergyDisplay;
    }

    private void OnDisable()
    {
        DeckManager.OnEnergyChanged -= UpdateEnergyDisplay;
    }

    private void UpdateEnergyDisplay(int current, int max)
    {
        if (energyText != null)
        {
            energyText.text = $"{current} / {max}";
        }
    }

}
