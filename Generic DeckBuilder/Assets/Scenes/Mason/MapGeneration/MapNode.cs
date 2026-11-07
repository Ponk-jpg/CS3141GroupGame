using UnityEngine;

public enum NodeType { EnemyEncounter, CombatEvent, RestSite, BossEncounter }

[System.Serializable]
public class MapNode
{
    public int floorNumber;
    public NodeType type;
    public EncounterDifficulty associatedDifficulty;

    public MapNode(int floor, NodeType type, EncounterDifficulty difficulty)
    {
        this.floorNumber = floor;
        this.type = type;
        this.associatedDifficulty = difficulty;
    }
}

