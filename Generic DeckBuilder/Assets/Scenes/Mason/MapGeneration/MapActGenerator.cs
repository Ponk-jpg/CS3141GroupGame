using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class MapActGenerator : MonoBehaviour
{
    [Header("Act Scaling Configurations")]
    public int actLength = 15;

    [Header("Rest Site Fixed Positions")]
    [Tooltip("forces a campfire right before the boss at floor 14.")]
    public List<int> guaranteedRestSiteFloors = new List<int>() { 14 };

    // Our actual runtime list array of 15 floors
    private List<MapNode> currentActLayout = new List<MapNode>();
    private int currentFloorIndex = 0; // Tracks player progress (0 to 14)
    private int currentActCount = 1;   // Tracks Act I, Act II, Act III...

    public List<MapNode> GetActLayout() => currentActLayout;
    public int GetCurrentFloor() => currentFloorIndex + 1;

    void Start()
    {
        //Roll our very first Act layout upon game initialization
        GenerateNewAct();
    }

    // --- 1. THE MAIN PROCEDURAL PIPELINE ---
    public void GenerateNewAct()
    {
        currentActLayout.Clear();
        currentFloorIndex = 0; // Reset player path positioning pointer

        UnityEngine.Debug.Log($"================ GENERATING ACT {currentActCount} ================");

        for (int floor = 1; floor <= actLength; floor++)
        {
            // The absolute final element (Floor 15) is ALWAYS a Boss Encounter
            if (floor == actLength)
            {
                currentActLayout.Add(new MapNode(floor, NodeType.BossEncounter, EncounterDifficulty.Boss));
                continue;
            }

            // Honor strict guaranteed campfire rest sites placement
            if (guaranteedRestSiteFloors.Contains(floor))
            {
                currentActLayout.Add(new MapNode(floor, NodeType.RestSite, EncounterDifficulty.Easy));
                continue;
            }

            // Pacing rules for early/late encounter difficulty weights
            NodeType chosenType = RollRandomNodeType(floor);
            EncounterDifficulty chosenDifficulty = DetermineEncounterDifficulty(floor, chosenType);

            currentActLayout.Add(new MapNode(floor, chosenType, chosenDifficulty));
        }

        LogGeneratedActStructure();
    }

    //CALL THIS WHEN PLAYER LEAVES A REWARDS/REST SCREEN ---
    public void AdvanceToNextFloor()
    {
        currentFloorIndex++;

        // IF THE PLAYER RECENTLY BEAT FLOOR 15:
        if (currentFloorIndex >= actLength)
        {
            UnityEngine.Debug.Log("[ACT COMPLETE] Boss defeated! Regenerating a fresh, harder layout pool for the next Act!");
            currentActCount++;
            GenerateNewAct();

            // Re-load the first node of the brand new Act
            LoadActiveNodeData(currentActLayout[0]);
            return;
        }

        // Otherwise, proceed to load the next step index normally
        LoadActiveNodeData(currentActLayout[currentFloorIndex]);
    }

    //PACING ALGORITHMS ENGINE ---
    private NodeType RollRandomNodeType(int floor)
    {
        // guarentee first floor is always an encounter
        if (floor == 1) return NodeType.EnemyEncounter;

        float roll = UnityEngine.Random.Range(0f, 100f);

        // Weighted distribution: 55% Monsters, 30% Mystery Events, 15% Random campfires
        if (roll < 55f) return NodeType.EnemyEncounter;
        if (roll < 85f) return NodeType.CombatEvent;
        return NodeType.RestSite;
    }

    private EncounterDifficulty DetermineEncounterDifficulty(int floor, NodeType type)
    {
        if (type != NodeType.EnemyEncounter) return EncounterDifficulty.Easy;

        // Early floors (2-6) roll from the Easy Pool. Beyond floor 6, fights scale up into the Hard Pool
        // might change to be the amount of encounters had in the act already.
        if (floor <= 6) return EncounterDifficulty.Easy;
        return EncounterDifficulty.Hard;
    }

    //DATA HOOK INTERFACE ---
    private void LoadActiveNodeData(MapNode activeNode)
    {
        UnityEngine.Debug.Log($"[MAP NAVIGATOR] Arrived at Floor {activeNode.floorNumber} -> Type: {activeNode.type}");

        BattleStateManager stateManager = UnityEngine.Object.FindAnyObjectByType<BattleStateManager>();
        BattleManager battleManager = UnityEngine.Object.FindAnyObjectByType<BattleManager>();

        if (stateManager == null || battleManager == null) return;

        switch (activeNode.type)
        {
            case NodeType.EnemyEncounter:
            case NodeType.BossEncounter:
                // Overwrites the test settings on BattleManager, forcing the database to instantiate the correct pool
                battleManager.testDifficultySetting = activeNode.associatedDifficulty;
                stateManager.StartCombat();
                break;

            case NodeType.CombatEvent:
                UnityEngine.Debug.Log("[EVENT SYSTEM] Open event canvas dialog option menus here");
                // link event choices system logic wrapper functions here
                battleManager.testDifficultySetting = EncounterDifficulty.Easy;
                stateManager.StartCombat();
                break;

            case NodeType.RestSite:
                UnityEngine.Debug.Log("[CAMPFIRE] Open rest site UI panel (Choose: remove a card or Heal HP)");
                // Simulating clearing a rest site immediately for now by clicking through to next floor
                battleManager.testDifficultySetting = EncounterDifficulty.Easy;
                stateManager.StartCombat();

                break;
        }
    }

    private void LogGeneratedActStructure()
    {
        string logText = $"Act {currentActCount} compiled path nodes map:\n";
        foreach (MapNode node in currentActLayout)
        {
            logText += $"Floor {node.floorNumber}: [{node.type}] ({node.associatedDifficulty})\n";
        }
        UnityEngine.Debug.Log(logText);
    }
}
