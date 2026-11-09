using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SingleButtonAudio : MonoBehaviour
{
    public AudioSource audioSource;
    //public AudioClip buttonPress;
    public Button button;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start( )
    {
        button.onClick.AddListener( ButtonClicked );
    }

    private void Update( )
    {
        
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener( ButtonClicked );
    }

    private void ButtonClicked( )
    {
        audioSource.gameObject.SetActive(true);
        audioSource.enabled = true;

        //audioSource.clip = buttonPress;
        audioSource.Play( );
    }
}
