using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


// base scriptable object to hold all encounters of a given area (if we get far enough to make multiple areas)
// mainly just used to hold all possible encounters so our BattleManager class can load up the encounter.
[CreateAssetMenu(fileName = "EncounterDatabase", menuName = "Scriptable Objects/EncounterDatabase")]
public class EncounterDatabase : ScriptableObject
{
    [Header("Categorized Encounter Pools")]
    public List<EnemyEncounterData> easyPool = new List<EnemyEncounterData>();
    public List<EnemyEncounterData> hardPool = new List<EnemyEncounterData>();
    public List<EnemyEncounterData> bossPool = new List<EnemyEncounterData>();

    // Helper to grab a completely random encounter matching the difficulty category
    public EnemyEncounterData GetRandomEncounter(EncounterDifficulty difficulty)
    {
        List<EnemyEncounterData> targetPool = null;

        switch (difficulty)
        {
            case EncounterDifficulty.Easy:
                targetPool = easyPool;
                break;
            case EncounterDifficulty.Hard:
                targetPool = hardPool;
                break;
            case EncounterDifficulty.Boss:
                targetPool = bossPool;
                break;
        }

        if (targetPool == null || targetPool.Count == 0)
        {
            UnityEngine.Debug.LogError($"[DATABASE] Requested pool '{difficulty}' is empty or unassigned!");
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, targetPool.Count);
        return targetPool[randomIndex];
    }

}

// Simple enum mapping to dictate categorization criteria
public enum EncounterDifficulty { Easy, Hard, Boss }
