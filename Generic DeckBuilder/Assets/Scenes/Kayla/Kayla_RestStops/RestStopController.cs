using UnityEngine;
using UnityEngine.SceneManagement;

public class RestStopController : MonoBehaviour
{

    public void HealPlayer()
{
    // Find the player's stats
    PlayerStats player =
        UnityEngine.Object.FindAnyObjectByType<PlayerStats>();

    if (player == null)
    {
        UnityEngine.Debug.LogError(
            "[REST STOP] Could not find PlayerStats!"
        );
        return;
    }

    // Restore 20 health
    player.Heal(20);
}

    public void ContinueFromRestStop()
    {
        // Find the existing map progression system
        MapActGenerator mapEngine =
            UnityEngine.Object.FindAnyObjectByType<MapActGenerator>();

        if (mapEngine == null)
        {
            UnityEngine.Debug.LogError(
                "[REST STOP] Could not find MapActGenerator!"
            );
            return;
        }

        // Close the Rest Stop scene
        AsyncOperation unloadOperation =
            SceneManager.UnloadSceneAsync("RestStop");

        if (unloadOperation == null)
        {
            UnityEngine.Debug.LogError(
                "[REST STOP] Could not unload RestStop scene!"
            );
            return;
        }

        // Advance the map after the Rest Stop closes
        unloadOperation.completed += operation =>
        {
            mapEngine.AdvanceToNextFloor();
        };
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

}