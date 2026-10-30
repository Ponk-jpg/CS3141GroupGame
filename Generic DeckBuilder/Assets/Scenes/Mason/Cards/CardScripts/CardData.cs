using System.Collections.Generic;
using System.Net;
using UnityEngine;

public enum CardTargetType
{
    Untargeted, // play anywhere (Block, buff cards, powers)
    Targeted    // requires pointing at an enemy.
}


// scriptable class used to create unique types of cards. does not implement the actual action the card does,
// just the main visual elements, and an array for the actual card effects that will be added.
[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardData : ScriptableObject
{
    public string Name;
    [TextArea(2, 5)]
    public string Description;
    public int energyCost;
    public Sprite artwork;
    public int cardID;

    [Header("Targeting Configuration")]
    public CardTargetType targetType; // assign in unity inspector

    [Header("Sequential Card effects Pool")]
    public List<CardEffectData> cardEffectsPool = new List<CardEffectData>();

}
