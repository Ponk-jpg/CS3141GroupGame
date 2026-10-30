using System.Collections.Generic;
using UnityEngine;

// should be put in the status system but was lazy, this is our enum for all different status effects and whatnot
public enum StatusType { 
    Strength,
    Dexterity,
    Vulnerable,
    Weak,
    Frail,
    Poison
}

// effect type for what a card can do, we add new abilities for cards here.
public enum EffectType
{
    DealDamage,
    GainBlock,
    DrawCards,
    GainEnergy,
    ApplyStatusEffect
}


// dictates who recieves a specific effect from the card.
public enum TargetFilter
{
    SingleTarget,
    AllEnemies,
    RandomEnemy,
    Player
}

// base class to create objects for that hold all of our card data.
[System.Serializable]
public class CardEffectData
{
    [Header("Core Type Config")]
    public EffectType effectType;
    public TargetFilter targetFilter = TargetFilter.SingleTarget;

    [Header("Values & Repetitions")]
    [Tooltip("the raw numeric value of whatever specific card effect")]
    public int intensityValue;

    [Tooltip("How many times this exact effect repeats, ie multihits")]
    public int repeatCount = 1;

    [Header("Status Effect Specifics")]
    [Tooltip("Only fill this slot if its ApplyStatusEffect")]
    public StatusEffectData statusToApply;

}
