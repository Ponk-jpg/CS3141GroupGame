using UnityEngine;

public class CombatEnemy : MonoBehaviour
{
    public string enemyName;
    public int currentHealth = 50;
    public int maxHealth = 50;
    public int currentBlock = 0;

    public void TakeDamage(int amount)
    {

        currentHealth -= amount;
        UnityEngine.Debug.Log($"{enemyName} took {amount} damage!");

    }



}
