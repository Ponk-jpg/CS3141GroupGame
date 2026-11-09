using UnityEngine;

public class PlayVictoryAudio : MonoBehaviour
{
    AudioSource audioSource;
    public BattleStateManager stateManager;
    BattleManager battleManager;

    private bool canPlay = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>( );
        battleManager = GetComponent<BattleManager>( );
        stateManager = battleManager.stateManager;
        
    }

    // Update is called once per frame
    void Update()
    {
        if ( battleManager.GetLivingEnemies( ).Count == 0 && stateManager != null  && canPlay == true )
        {
            audioSource.Play();
            canPlay = false;
            DeckManager.OnPlayerTurnStateChanged += VictorySoundCanPlay;
        }
    }

    private void VictorySoundCanPlay( bool playerTurn )
    {
        canPlay = playerTurn;
        DeckManager.OnPlayerTurnStateChanged -= VictorySoundCanPlay;
    }

}
