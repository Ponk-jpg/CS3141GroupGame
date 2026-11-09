using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GeneralButtonAudio : MonoBehaviour
{
    // Fields
    public AudioSource audioSource;
    public AudioClip buttonPress;

    public Button drawPile;
    public Button discardPile;
    public Button exhaustPile;
    public Button fullDeck;
    public Button endTurn;



    // Methods
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>( );

        drawPile.onClick.AddListener(ButtonClicked);
        discardPile.onClick.AddListener(ButtonClicked);
        exhaustPile.onClick.AddListener(ButtonClicked);
        fullDeck.onClick.AddListener(ButtonClicked);
        endTurn.onClick.AddListener(ButtonClicked);

        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable( )
    {
        drawPile.onClick.RemoveListener( ButtonClicked );
        discardPile.onClick.RemoveListener( ButtonClicked );
        exhaustPile.onClick.RemoveListener( ButtonClicked );
        fullDeck.onClick.RemoveListener( ButtonClicked );
        endTurn.onClick.RemoveListener( ButtonClicked );
    }

    private void ButtonClicked( )
    {
        audioSource.clip = buttonPress;
        audioSource.Play( );
    }
}
