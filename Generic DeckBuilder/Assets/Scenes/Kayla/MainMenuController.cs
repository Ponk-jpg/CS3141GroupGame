using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // References to the two panels.
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    // Reference to the game's Audio Mixer.
    public AudioMixer audioMixer;
    // Reference to the quit confirmation panel.
    public GameObject quitConfirmationPanel;

    // Loads the game scene when Play is clicked.
    public void PlayGame()
    {
        SceneManager.LoadScene("BaseLevel");
    }

    // Hides the main menu and displays Settings.
    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // Hides Settings and displays the main menu.
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Changes the music volume.
    public void SetMusicVolume(float volume)
    {
        // Convert the slider value (0–1) into decibels.
        float volumeInDecibels = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;

        audioMixer.SetFloat("MusicVolume", volumeInDecibels);
    }

    // Changes the sound effects volume.
    public void SetSoundEffectsVolume(float volume)
    {
        // Convert the slider value (0–1) into decibels.
        float volumeInDecibels = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;

        audioMixer.SetFloat("SoundEffectsVolume", volumeInDecibels);
    }

    // Exits the game when Quit is clicked.
    public void QuitGame()
    {
        Application.Quit();
    }

    // Displays the quit confirmation panel.
    public void OpenQuitConfirmation()
    {
        mainMenuPanel.SetActive(false);
        quitConfirmationPanel.SetActive(true);
    }

    // Closes the quit confirmation panel.
    public void CancelQuit()
    {
        quitConfirmationPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Exits the game when the player confirms.
    public void ConfirmQuit()
    {
        Application.Quit();
    }
}