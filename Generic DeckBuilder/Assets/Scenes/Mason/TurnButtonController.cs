using UnityEngine;
using UnityEngine.UI;

public class TurnButtonController : MonoBehaviour
{
    public Button endTurnButton;
    private DeckManager deckManager;

    void Start()
    {
        deckManager = GetComponent<DeckManager>();
        endTurnButton.onClick.AddListener(OnEndTurnPressed);
    }

    void OnEndTurnPressed()
    {
        if (deckManager != null)
        {
            deckManager.EndCurrentTurn();
        }
    }
}
