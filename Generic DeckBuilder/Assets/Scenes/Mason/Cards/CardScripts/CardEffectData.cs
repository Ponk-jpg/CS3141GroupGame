using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
    ApplyStatusEffect,
    ExhaustHandCards,
    ExhaustSpecificPile
}


// dictates who recieves a specific effect from the card.
public enum TargetFilter
{
    SingleTarget,
    AllEnemies,
    RandomEnemy,
    Player
}

public enum CardSelectionMode { Random, ChosenByPlayer }
public enum DestinationPile { Hand, DrawPile, DiscardPile }

[System.Serializable]
public class SubEffect
{
    [Header("Chained operation settings")]
    public EffectType effectType;
    public TargetFilter targetFilter = TargetFilter.SingleTarget;

    [Tooltip("the raw base value of this secondary step.")]
    public int intensityValue;
    public int repeatCount = 1;
    public StatusEffectData statusToApply;

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

    [Header("Exhaustion Configuration Settings")]
    [Tooltip("Only configure this block if EffectType is set to an Exhaustion rule.")]
    public CardSelectionMode selectionMode = CardSelectionMode.Random;
    public int cardsToExhaustCount = 1;

    [Header("Post-Exhaust Generative Action Settings")]
    [Tooltip("The card data template file to create/add after an exhaustion event finishes.")]
    public CardData cardToGenerateTemplate;
    public DestinationPile targetDestination = DestinationPile.Hand;
    public int generationAmount = 1;

    [Header("Chained Dependent Operations")]
    [Tooltip("Add any separate effects we want to fire sequentially ONLY AFTER the exhaustion rule completes, (e.g. Gain 7 Block per card exhausted)")]
    public List<SubEffect> followUpEffectsPool = new List<SubEffect>();


}
