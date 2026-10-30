using System;
using UnityEngine;

public enum IntentType { Attack, Defend, Buff, Debuff}


// base scriptable object to create different intent types for our enemies, we create one for each unique type of attack and any attack with different values.
[CreateAssetMenu(fileName = "EnemyMoveData", menuName = "Scriptable Objects/EnemyMoveData")]
public class EnemyMoveData : ScriptableObject
{
    public string moveName;
    public IntentType intentType;
    public Sprite intentIcon;

    [Tooltip("How much damage this move deals (if an Attack) or how much shield it grants (if Defend).")]
    public int baseValue;

    [Tooltip("For attacks that strike multiple times (e.g., 6 x 3). Leave at 1 for standard single hits.")]
    public int hitCount = 1;

    [Header("Status Effect Action Settings")]
    [Tooltip("Only assing this if the move type is a buff or debuff")]
    public StatusEffectData statusEffectToApply;


}
