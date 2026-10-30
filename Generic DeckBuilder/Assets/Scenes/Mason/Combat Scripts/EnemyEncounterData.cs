using System.Collections.Generic;
using UnityEngine;


// scriptable object to hold all data regarding an encounter, IE it just holds multiple or single enemy prefabs and whether or not they summon enemies.
[CreateAssetMenu(fileName = "EnemyEncounterData", menuName = "Scriptable Objects/EnemyEncounterData")]
public class EnemyEncounterData : ScriptableObject
{
    public string encounterName;

    // Reusing the clean config layout
    [System.Serializable]
    public class EnemySpawnConfig
    {
        public GameObject enemyPrefab;
        public int reservedMinionSlotsForThisEnemy = 0;
    }

    [Tooltip("The group of enemies that populate this specific fight.")]
    public List<EnemySpawnConfig> enemiesInEncounter = new List<EnemySpawnConfig>();



}
