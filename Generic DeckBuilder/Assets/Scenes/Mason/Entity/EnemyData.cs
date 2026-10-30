using UnityEngine;

// base enemy data object used to initialize max health, the enemy size for generating locations,
// and the enemy name and sprite.
[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName = "Test";
    public Sprite artwork;
    public int maxHealth = 30;
    public EnemySize visualSize = EnemySize.Medium;

}
