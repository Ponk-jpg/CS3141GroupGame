using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

// class used to initialize enemies and track whether the player has won the combat or not.
public class BattleManager : MonoBehaviour
{
    [Header("System References")]
    public EnemySpawnManager spawnManager;
    public EncounterDatabase encounterDatabase;
    public RewardManager rewardManager;

    public BattleStateManager stateManager;

    [Header("Active Run State")]
    [Tooltip("Change this difficulty in the inspector to test different categories")]
    public EncounterDifficulty testDifficultySetting = EncounterDifficulty.Easy;

    private List<CombatEnemy> livingEnemies = new List<CombatEnemy>();

    public List<CombatEnemy> GetLivingEnemies() => livingEnemies;

    // delegate enabler
    private void OnEnable()
    {

        CombatEnemy.OnEnemyDefeated += TrackVictoryConditions;

    }
    // delegat disabler
    private void OnDisable()
    {

        CombatEnemy.OnEnemyDefeated -= TrackVictoryConditions;


    }
    
    // tracks whether all enemies are defeated, and removes enemies that are defeated.
    // when all enemies are dead we end combat.
    private void TrackVictoryConditions(CombatEnemy defeatedEnemy)
    {

        if (livingEnemies.Contains(defeatedEnemy))
        {
            livingEnemies.Remove(defeatedEnemy);
        }

        if (livingEnemies.Count == 0 && stateManager != null)
        {
            stateManager.EndCombatVictory();
        }


    }

    public void TriggerVictorySequence()
    {

        // Tell DeckManager to halt player turn sequences safely 
        PlayerHand2D hand = UnityEngine.Object.FindAnyObjectByType<PlayerHand2D>();
        if (hand != null)
        {
            var activeCards = new List<CardController>(hand.GetCardsInHand());
            for(int i = activeCards.Count - 1; i >= 0; i--)
            {

                if (activeCards[i] != null) Destroy(activeCards[i].gameObject);

            }

        }


        //DeckManager deckEngine = UnityEngine.Object.FindAnyObjectByType<DeckManager>();
        //if (deckEngine != null)
        //{
        //    deckEngine.EndCurrentTurn(); // Wipes hand cards out of the active battlefield
        //}

        // Trigger the rewards popup, should change this to not be a static value between 15 and 30 gold.
        if (rewardManager != null)
        {
            rewardManager.OpenCombatRewards(minGold: 15, maxGold: 30);
        }
        else
        {
            UnityEngine.Debug.LogError("[BATTLE MANAGER] RewardManager reference missing! Cannot open rewards.");
        }


    }

    // checks the pool of encounters in a specific difficulty group, and will choose one randomly in that group.
    public void LoadRandomEncounterFromPool(EncounterDifficulty difficulty)
    {
        if (encounterDatabase == null)
        {
            UnityEngine.Debug.LogError("[BATTLE MANAGER] No Encounter Database asset assigned!");
            return;
        }

        //Roll a random encounter matching our difficulty criteria from the database asset
        EnemyEncounterData chosenEncounter = encounterDatabase.GetRandomEncounter(difficulty);

        if (chosenEncounter == null) return;

        UnityEngine.Debug.Log($"[BATTLE MANAGER] Selected Encounter: '{chosenEncounter.encounterName}' from the {difficulty} Pool.");

        livingEnemies.Clear();

        //DYNAMICALLY COMPILE SIZES & RESERVATIONS
        List<EnemySize> sizingProfile = new List<EnemySize>();
        int globalMinionReservationCount = 0;

        foreach (var config in chosenEncounter.enemiesInEncounter)
        {
            if (config.enemyPrefab != null)
            {
                CombatEnemy enemyScript = config.enemyPrefab.GetComponent<CombatEnemy>();
                if (enemyScript != null)
                {
                    sizingProfile.Add(enemyScript.visualSize);
                }
                globalMinionReservationCount += config.reservedMinionSlotsForThisEnemy;
            }
        }

        //GENERATE PROCEDURAL LAYOUT ROW
        spawnManager.SetupEncounterLayout(sizingProfile, globalMinionReservationCount);

        //GENERATE SQUAD ELEMENTS INTO SLOTS
        for (int i = 0; i < chosenEncounter.enemiesInEncounter.Count; i++)
        {
            var config = chosenEncounter.enemiesInEncounter[i];
            if (config.enemyPrefab != null)
            {
                GameObject spawnedEnemy = spawnManager.SpawnEnemyInSlot(config.enemyPrefab, i);

                if (spawnedEnemy != null)
                {
                    CombatEnemy enemyComponent = spawnedEnemy.GetComponent<CombatEnemy>();

                    if (enemyComponent != null)
                    {

                        livingEnemies.Add(enemyComponent);

                    }

                }

            }
        }
    }
}
