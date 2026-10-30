using UnityEngine;

// scritable object used to hold art and text and a description for the specific status effect.
[CreateAssetMenu(fileName = "StatusEffectData", menuName = "Scriptable Objects/StatusEffectData")]
public class StatusEffectData : ScriptableObject
{
    public string effectName;
    [TextArea(2, 5)]
    public string description;
    public Sprite icon;

    [Tooltip("Does this effect lose 1 stack at the end of a round (like Vulnerable), or stay persistent (like Strength)")]
    public bool decaysAtEndOfTurn = true;

    [Tooltip("Should this hide completely if the stack count reaches 0")]
    public bool hideAtZeroStacks = true;

    [Tooltip("used to mark status type files of what their specific status is")]
    public StatusType type;


}
