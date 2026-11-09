using UnityEngine;

public class EnemyAttackAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public PlayerStats player;

    public BattleStateManager stateManager; // Set in inspector
    private int previousHealth;
    private int previousMaxHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<PlayerStats>( );
        player.OnHealthChanged += EnemyMayHaveAttacked;
        previousHealth = player.GetCurrentHealth( );
        previousMaxHealth = player.maxHealth;

    }

    public void EnemyMayHaveAttacked( int newHealth, int newMaxHealth )
    {
        if ( stateManager  == null )
        {
            Debug.Log( "Place a BattleStateManager on EnemyAttackAudio before proceeding!" );
            return;
        }

        if ( previousHealth > newHealth && stateManager.GetCurrentState( ) == BattleState.EnemyTurn )
        {
            audioSource.Play( );
        }

        previousMaxHealth = newMaxHealth;
        previousHealth = newHealth;
    }

}
