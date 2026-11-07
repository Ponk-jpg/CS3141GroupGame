using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

// base enemyAI,  holds the turn actions and the loop that the enemy will go through of its possible actions.
// NOTE does not randomly do so, its sequential and then cycles back to the start.
[RequireComponent(typeof(CombatEnemy))]
public class EnemyAI : MonoBehaviour
{
    public static Action<CardData, EnemyCardGenerationPile, int> OnEnemyCardInjectedAlert;


    // A clean sub-structure allowing an inspector index row to contain multiple elements
    [System.Serializable]
    public class TurnActionBundle
    {
        public string turnLabel = "Standard Attack & Block Turn";
        [Tooltip("Add as many actions as you want this enemy to execute on this specific turn index row!")]
        public List<EnemyMoveData> actionsOnThisTurn = new List<EnemyMoveData>();
    }

    [Header("Sequential Turn Bundles Pattern")]
    [Tooltip("Design your loops from top-to-bottom. Each row represents a full single round turn block!")]
    public List<TurnActionBundle> turnPatternLoop = new List<TurnActionBundle>();

    [Header("UI Anchor Hook")]
    public EnemyIntentUI intentUI;

    private CombatEnemy myEnemyComponent;

    // Changing our tracking node from a single asset structure to an active operations list
    private List<EnemyMoveData> currentlySelectedMoves = new List<EnemyMoveData>();
    private int currentPatternIndex = 0;

    public EnemyMoveData GetCurrentlyActiveMove() { return currentlySelectedMoves[0]; }


    void Awake()
    {
        myEnemyComponent = GetComponent<CombatEnemy>();
        currentPatternIndex = 0;
    }

    // SELECTION ENGINE: CALLED BY STATE MANAGER AT ROUND START ---
    public void RollNewIntent()
    {
        currentlySelectedMoves.Clear();

        if (turnPatternLoop == null || turnPatternLoop.Count == 0)
        {
            UnityEngine.Debug.LogWarning($"[AI MULTI-LOOP] {gameObject.name} has no turn actions bundles configured!");
            if (intentUI != null) intentUI.HideIntent();
            return;
        }

        // Wrap pattern boundaries
        if (currentPatternIndex >= turnPatternLoop.Count)
        {
            currentPatternIndex = 0;
        }

        // Pull the exact data structure from your nested inspector row list
        TurnActionBundle activeBundle = turnPatternLoop[currentPatternIndex];
        if (activeBundle != null && activeBundle.actionsOnThisTurn != null)
        {
            currentlySelectedMoves.AddRange(activeBundle.actionsOnThisTurn);
        }

        //If this specific row index contains no moves, hide and exit
        if (currentlySelectedMoves.Count == 0)
        {
            if (intentUI != null) intentUI.HideIntent();
            return;
        }

        UnityEngine.Debug.Log($"[AI INTENT PREVIEW] {myEnemyComponent.combatantName} plans {currentlySelectedMoves.Count} actions for turn index {currentPatternIndex}.");

        // Now pass the filled list downstream to spawn the layout group icons
        if (intentUI != null)
        {
            intentUI.DisplayMultipleIntents(currentlySelectedMoves);
        }
    }

    // EXECUTION ENGINE: CALLED BY STATE MACHINE DURING ENEMY PHASE ---
    public void ExecutePlannedAction(PlayerStats playerTarget)
    {
        if (currentlySelectedMoves.Count == 0 || playerTarget == null) return;

        UnityEngine.Debug.Log($"[ACTION EXECUTION] {myEnemyComponent.combatantName} executing sequence bundle for turn index {currentPatternIndex}!");

        DeckManager deckEngine = UnityEngine.Object.FindAnyObjectByType<DeckManager>();

        TransformJuice movementJuice = GetComponent<TransformJuice>();

        // Execute every move in the bundle sequentially from left-to-right
        foreach (EnemyMoveData move in currentlySelectedMoves)
        {
            if (move == null) continue;


            switch (move.intentType)
            {
                case IntentType.Attack:

                        
                        StartCoroutine(ExecuteMultiHitLungeSequence(move.hitCount, move.baseValue, playerTarget, movementJuice));
                    break;

                case IntentType.Defend:

                    int finalBlock = DamageCalculator.CalculateModifiedBlock(move.baseValue, myEnemyComponent);
                    myEnemyComponent.AddBlock(finalBlock);
                    break;

                case IntentType.Buff:
                    if (move.statusEffectToApply != null && myEnemyComponent.StatusEngine != null)
                    {
                        if (movementJuice != null) movementJuice.TriggerLungeAnimation();
                        myEnemyComponent.StatusEngine.ApplyStatus(move.statusEffectToApply, move.baseValue);
                    }
                    break;

                case IntentType.Debuff:

                    if (movementJuice != null) movementJuice.TriggerLungeAnimation();

                    if (move.statusEffectToApply != null && playerTarget.StatusEngine != null)
                    {
                        playerTarget.StatusEngine.ApplyStatus(move.statusEffectToApply, move.baseValue);
                        UnityEngine.Debug.Log($"[AI DEBUFF] {myEnemyComponent.combatantName} successfully inflicted {move.baseValue} stacks of {move.statusEffectToApply.effectName} onto the player");
                    }

                    if (move.cardToInjectTemplate != null && deckEngine != null)
                    {

                        ExecuteEnemyCardInjectionPayload(move.cardToInjectTemplate, move.targetInjectionDestination, move.cardsToInjectCount, deckEngine);

                    }

                    break;
            }
        }

        if (intentUI != null) intentUI.HideIntent();

        // Advance turn counter index
        IncrementPatternLoop();
    }

    // signify sending a card to the player's decks, and add it based on the requirements.
    private void ExecuteEnemyCardInjectionPayload(CardData cardTemplate, EnemyCardGenerationPile destination, int quantity, DeckManager deck)
    {
        if (cardTemplate != null)
        {

            OnEnemyCardInjectedAlert?.Invoke(cardTemplate,destination, quantity);
            
        }


        for (int i = 0; i < quantity; i++)
        {
            switch (destination)
            {
                case EnemyCardGenerationPile.Hand:
                    // Leverages active mid-combat generation limits
                    deck.GenerateCardMidCombat(cardTemplate);
                    UnityEngine.Debug.Log($"[ENEMY INJECTION] Spawned {cardTemplate.Name} straight into the player's Hand.");
                    break;

                case EnemyCardGenerationPile.DrawPile:
                    // We randomly set into draw pile
                    List<CardData> playerDrawPile = deck.GetDrawPile();
                    int randomInsertIndex = UnityEngine.Random.Range(0, playerDrawPile.Count + 1);

                    playerDrawPile.Insert(randomInsertIndex, cardTemplate);
                    UnityEngine.Debug.Log($"[ENEMY INJECTION] Randomly shuffled {cardTemplate.Name} into the player's Draw Pile at index {randomInsertIndex}.");
                    break;

                case EnemyCardGenerationPile.DiscardPile:
                    deck.GetDiscardPile().Add(cardTemplate);
                    UnityEngine.Debug.Log($"[ENEMY INJECTION] Slid {cardTemplate.Name} directly into the player's Discard Pile.");
                    break;
            }
        }
    }

    private IEnumerator ExecuteMultiHitLungeSequence(int totalHits, int baseDamagePerHit, PlayerStats target, TransformJuice movementJuice)
    {
        for (int i = 0; i < totalHits; i++)
        {
            if (target == null) break;

            //Kick off the movement dash lunge script
            if (movementJuice != null)
            {
                movementJuice.TriggerLungeAnimation();
            }


            int finalCalculatedDamage = DamageCalculator.CalculateModifiedDamage(baseDamagePerHit, myEnemyComponent, target);

            //Subtract player health metrics
            target.TakeDamage(finalCalculatedDamage);

            //Pause briefly before processing the next strike loop so the hits feel heavy and distinct (e.g. 0.22 seconds)
            yield return new WaitForSeconds(0.22f);
        }
    }

    private void IncrementPatternLoop()
    {
        currentPatternIndex++;
        if (currentPatternIndex >= turnPatternLoop.Count)
        {
            UnityEngine.Debug.Log($"[AI MULTI-LOOP] {myEnemyComponent.combatantName} completed its bundle pattern loop. Resetting back to row 0.");
            currentPatternIndex = 0;
        }
    }
}
