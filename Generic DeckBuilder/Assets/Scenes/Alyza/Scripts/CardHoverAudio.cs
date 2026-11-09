using JetBrains.Annotations;
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;

public class CardHoverAudio : MonoBehaviour
{
    public AudioSource hoverAudio;

    void Start()
    {
        
    }

    public void CardIsHoveredOver( )
    {
        if (hoverAudio == null) Debug.Log( "Attempted to play sound without attached source!" );
        hoverAudio.Play( );
    }

}
