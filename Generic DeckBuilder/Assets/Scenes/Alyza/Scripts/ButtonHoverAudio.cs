using UnityEngine;
using UnityEngine.Audio;

public class ButtonHoverAudio : MonoBehaviour
{
    public AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>( );
    }
    
    private void ButtonIsHoveredOver()
    {
        audioSource.Play();
    }
}
