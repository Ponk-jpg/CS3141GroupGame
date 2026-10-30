using UnityEngine;
using UnityEngine.UI;

// button controller for looking at specific portions of the decks, such as draw pile, discard pile, exhaust pile, full deck.
public class PileButtonController : MonoBehaviour
{
    public Button drawPileButton;
    public Button discardPileButton;
    public Button exhaustPileButton;
    public Button fullDeckButton;


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
        exhaustPileButton.onClick.AddListener(OnExhaustPileClicked);
        fullDeckButton.onClick.AddListener(OnFullDeckClicked);
    }


    private void OnEnable()
    {
        DeckManager.OnPlayerTurnStateChanged += SetButtonInteraction;
    }

    private void OnDisable()
    {
        DeckManager.OnPlayerTurnStateChanged -= SetButtonInteraction;
    }

    private void SetButtonInteraction(bool isPlayerTurn)
    {
        if (drawPileButton != null) drawPileButton.interactable = isPlayerTurn;
        if (discardPileButton != null) discardPileButton.interactable = isPlayerTurn;
        if (exhaustPileButton != null) exhaustPileButton.interactable = isPlayerTurn;
        if (fullDeckButton != null) fullDeckButton.interactable = isPlayerTurn;

        if ( !isPlayerTurn && viewerUI != null)
        {
            viewerUI.ClosePanel();
        }


    }


    void OnDrawPileClicked()
    {
        viewerUI.OpenPanel("Draw Pile", deckManager.GetDrawPile());
    }

    void OnDiscardPileClicked()
    {
        viewerUI.OpenPanel("Discard Pile", deckManager.GetDiscardPile());
    }

    void OnExhaustPileClicked()
    {
        if (viewerUI != null && deckManager != null)
            viewerUI.OpenPanel("Exhaust Pile", deckManager.GetExhaustPile());
    }

    void OnFullDeckClicked()
    {
        if (viewerUI != null && deckManager != null)
            viewerUI.OpenPanel("Full Deck", deckManager.GetMasterDeck());
    }

}
