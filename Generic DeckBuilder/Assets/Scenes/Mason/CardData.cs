using UnityEngine;

public enum CardTargetType
{
    Untargeted, // play anywhere (Block, buff cards, powers)
    Targeted    // requires pointing at an enemy.
}



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
}
