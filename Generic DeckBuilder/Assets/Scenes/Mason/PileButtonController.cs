using UnityEngine;
using UnityEngine.UI;

public class PileButtonController : MonoBehaviour
{
    public Button drawPileButton;
    public Button discardPileButton;

    [Header("References")]
    public DeckManager deckManager;
    public DeckViewerUI viewerUI;

    void Start()
    {
        if (deckManager == null)
        {
            deckManager = GetComponent<DeckManager>();
        }


        drawPileButton.onClick.AddListener(OnDrawPileClicked);
        discardPileButton.onClick.AddListener(OnDiscardPileClicked);
    }

    void OnDrawPileClicked()
    {
        viewerUI.OpenPanel("Draw Pile", deckManager.GetDrawPile());
    }

    void OnDiscardPileClicked()
    {
        viewerUI.OpenPanel("Discard Pile", deckManager.GetDiscardPile());
    }
}
