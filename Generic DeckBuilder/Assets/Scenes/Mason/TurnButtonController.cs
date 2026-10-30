using UnityEngine;
using UnityEngine.UI;

// class used to setup the end turn button for the player.
public class TurnButtonController : MonoBehaviour
{
    public Button endTurnButton;
    private DeckManager deckManager;

    void Start()
    {
        deckManager = GetComponent<DeckManager>();
        endTurnButton.onClick.AddListener(OnEndTurnPressed);
    }

    private void OnEnable()
    {
        DeckManager.OnPlayerTurnStateChanged += HandleTurnChanged;

    }

    private void OnDisable()
    {

        DeckManager.OnPlayerTurnStateChanged -= HandleTurnChanged;

    }

    private void HandleTurnChanged(bool isPlayerTurn)
    {

        if (endTurnButton != null)
        {
            endTurnButton.interactable = isPlayerTurn;
        }

    }


    void OnEndTurnPressed()
    {

        BattleStateManager stateManager = UnityEngine.Object.FindAnyObjectByType<BattleStateManager>();
        if (stateManager != null)
        {
            stateManager.EndPlayerTurn();
        }
    }
}
