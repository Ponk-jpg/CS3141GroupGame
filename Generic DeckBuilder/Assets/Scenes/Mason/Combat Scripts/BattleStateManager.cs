using System.Collections;
using System.Diagnostics;
using UnityEngine;

public enum BattleState { Setup, PlayerTurn, EnemyTurn, Victory, GameOver}

// the combat turn manager, which calls enemy attacks, starts the combat, ends the combat, and gives and removes control for the player
public class BattleStateManager : MonoBehaviour
{
    [Header("System References")]
    public BattleManager battleManager;
    public DeckManager deckManager;

    [Header("Turn Timing Tweak Variables")]
    [Tooltip("How long to wait during transitions to let visual animations breathe.")]
    public float phaseTransitionDelay = 1.0f;

    private BattleState currentState;

    public BattleState GetCurrentState() => currentState;

    void Start()
    {
        //Kick off combat right out of the gate
        StartCombat();
    }

    // START COMBAT (SETUP) ---
    public void StartCombat()
    {
        currentState = BattleState.Setup;
        UnityEngine.Debug.Log("================ COMBAT INITIALIZED ================");

        //Initialize encounter selection and spawn the physical enemies
        if (battleManager != null)
        {
            // Pull difficulty setting from the BattleManager inspector setup
            battleManager.LoadRandomEncounterFromPool(battleManager.testDifficultySetting);
        }

        //Delay slightly before handing control to the player
        StartCoroutine(TransitionToState(BattleState.PlayerTurn));
    }

    // PLAYER TURN INITIALIZATION ---
    private void StartPlayerTurn()
    {
        currentState = BattleState.PlayerTurn;
        UnityEngine.Debug.Log(">>> [PHASE] PLAYER TURN STARTED <<<");

        PlayerStats player = UnityEngine.Object.FindAnyObjectByType<PlayerStats>();
        if (player != null && player.StatusEngine != null)
        {
            player.StatusEngine.TickEndOfTurnStatuses();

        }

        foreach (CombatEnemy enemy in battleManager.GetLivingEnemies())
        {

            EnemyAI aiComponent = enemy.GetComponent<EnemyAI>();
            if (aiComponent != null) aiComponent.RollNewIntent();
            
        }


        // Trigger Deck Manager to give energy, broadcast events, and draw 5 cards
        // should change this so we can update how many cards a player an draw in the future.
        if (deckManager != null)
        {
            deckManager.StartNewTurn(5);
        }
    }

    // END PLAYER TURN -> START ENEMY TURN ---
    public void EndPlayerTurn()
    {
        // Guard check: prevent double tapping the button or forcing a phase shift out of turn
        if (currentState != BattleState.PlayerTurn) return;

        currentState = BattleState.EnemyTurn;
        UnityEngine.Debug.Log(">>> [PHASE] ENEMY TURN STARTED <<<");

        // Lock out layout inputs & dump remaining cards out of the hand container
        if (deckManager != null)
        {
            // We pass false to OnPlayerTurnStateChanged manually to freeze interfaces immediately
            DeckManager.OnPlayerTurnStateChanged?.Invoke(false);
            deckManager.EndCurrentTurn();
        }

        //Route control directly to our sequential Enemy AI Execution pipeline
        StartCoroutine(ExecuteEnemyTeamActions());
    }

    // used to execute enemy actions for each enemy in combat.
    private IEnumerator ExecuteEnemyTeamActions()
    {
        yield return new WaitForSeconds(phaseTransitionDelay);

        // Fetch all active living enemies from the manager tracking nodes
        var enemyTeam = battleManager.GetLivingEnemies();

        PlayerStats playerTarget = UnityEngine.Object.FindAnyObjectByType<PlayerStats>();

        // clear block for enemies at the start of their turn 
        foreach(CombatEnemy enemy in enemyTeam)
        {
            if (enemy != null) enemy.ClearBlock();
        }


        // Loop through each enemy sequentially so they attack one by one instead of all mashing instantly
        foreach (CombatEnemy enemy in enemyTeam)
        {
            if (enemy == null) continue;

            // Clear out enemy armor blocks or process status decays here if required
            EnemyAI aiComponent = enemy.GetComponent<EnemyAI>();
            if (aiComponent != null && playerTarget != null)
            {
                aiComponent.ExecutePlannedAction(playerTarget);
            }
            else
            {
                // do nothing lmao
            }
            yield return new WaitForSeconds(0.8f);
        }
        // decrement status effects if need be.
        foreach(CombatEnemy enemy in enemyTeam)
        {
            if (enemy != null && enemy.StatusEngine != null)
            {

                enemy.StatusEngine.TickEndOfTurnStatuses();

            }

        }

        // Return control back to the player loop phase
        yield return new WaitForSeconds(phaseTransitionDelay);

        // Double check to make sure the encounter didn't finish from an effect tick before returning control
        if (currentState == BattleState.EnemyTurn)
        {
            StartPlayerTurn();
        }
    }

    // END COMBAT RESOLUTION (VICTORY) ---
    public void EndCombatVictory()
    {
        currentState = BattleState.Victory;
        UnityEngine.Debug.Log("================ VICTORY RESOLVED ================");

        // Wipe player control structures
        DeckManager.OnPlayerTurnStateChanged?.Invoke(false);

        // clear any status effects the player may have
        PlayerStats player = UnityEngine.Object.FindAnyObjectByType<PlayerStats>();
        if (player != null && player.StatusEngine != null)
        {

            player.StatusEngine.ClearAllStatuses();

        }

        // Trigger rewards distribution drops
        if (battleManager != null)
        {
            battleManager.TriggerVictorySequence();
        }
    }

    // State Transition Routine Helper
    private IEnumerator TransitionToState(BattleState nextState)
    {
        yield return new WaitForSeconds(phaseTransitionDelay);

        switch (nextState)
        {
            case BattleState.PlayerTurn:
                StartPlayerTurn();
                break;
            case BattleState.Victory:
                EndCombatVictory();
                break;
        }
    }
}
