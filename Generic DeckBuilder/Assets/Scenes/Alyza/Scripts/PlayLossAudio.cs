using UnityEngine;

public class PlayLossAudio : MonoBehaviour
{
    private PlayerStats playerStatsComponent; // PlayerStats script reference for current gameobject
    private AudioSource audioSource;          // The audio source on the game object

    // Gets component references and adds necessary listener
    void Start( )
    {
        audioSource = GetComponent<AudioSource>();
        playerStatsComponent = GetComponent<PlayerStats>();
        playerStatsComponent.OnDeath += CombatIsLost;
    }

    private void OnDisable( )
    {
        // Removes CombatIsLost as a listener to OnDeath
        playerStatsComponent.OnDeath -= CombatIsLost;
    }

    // Plays the sound for being defeated in combat when the player dies
    private void CombatIsLost( )
    {
        audioSource.Play( );
    }

}
