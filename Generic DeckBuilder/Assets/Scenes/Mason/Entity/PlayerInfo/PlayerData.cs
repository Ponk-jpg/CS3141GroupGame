using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public string characterClassName = "Bob";
    public int maxHealth = 80;

    [Tooltip("How much armor block shields the player naturally brings into combat initialization.")]
    public int startingBlock = 0; // have this be updated from any sort of special relic we get later one.



}
