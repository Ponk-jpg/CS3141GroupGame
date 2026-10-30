using UnityEngine;

// helper class to hold damage calculations based on status effects and other variables that may change damage or block numbers.
public static class DamageCalculator
{
    //  DAMAGE CALCULATION MATRIX (Includes Strength, Vulnerable, and Weak) ---
    public static int CalculateModifiedDamage(int baseDamage, CharacterStats attacker, CharacterStats defender)
    {
        float finalDamage = baseDamage;

        // Process Attacker Modifiers
        if (attacker != null && attacker.StatusEngine != null)
        {
            // Flat modification from Strength
            int strengthStacks = GetStatusValue(attacker, StatusType.Strength);
            finalDamage += strengthStacks;

            // Percentage penalty from Weak (-25% outgoing damage)
            int weakStacks = GetStatusValue(attacker, StatusType.Weak);
            if (weakStacks > 0)
            {
                finalDamage *= 0.75f;
            }
        }

        // Process Defender Modifiers
        if (defender != null && defender.StatusEngine != null)
        {
            // Percentage multiplier from Vulnerable (+50% incoming damage)
            int vulnerableStacks = GetStatusValue(defender, StatusType.Vulnerable);
            if (vulnerableStacks > 0)
            {
                finalDamage *= 1.5f;
            }
        }

        int resolvedDamage = Mathf.FloorToInt(finalDamage);
        return resolvedDamage < 0 ? 0 : resolvedDamage;
    }

    // BLOCK CALCULATION MATRIX (Includes Frail, dexterity) 
    public static int CalculateModifiedBlock(int baseBlock, CharacterStats character)
    {
        float finalBlock = baseBlock;

        // Process recipient modifiers
        if (character != null && character.StatusEngine != null)
        {

            int dexterityStacks = GetStatusValue(character, StatusType.Dexterity);
            finalBlock += dexterityStacks;


            // Percentage penalty from Frail (-25% shield block gains)
            int frailStacks = GetStatusValue(character, StatusType.Frail);
            if (frailStacks > 0)
            {
                finalBlock *= 0.75f;
            }
        }

        int resolvedBlock = Mathf.FloorToInt(finalBlock);
        return resolvedBlock < 0 ? 0 : resolvedBlock;
    }

    // helper function to recieve the amount of stacks for a given status effect, given the type and entity provided.
    private static int GetStatusValue(CharacterStats character, StatusType targetType)
    {
        if (character == null || character.StatusEngine == null) return 0;

        foreach (var status in character.StatusEngine.GetActiveStatusesList())
        {
            if (status.data != null && status.data.type == targetType)
            {
                return status.stacks;
            }
        }
        return 0;
    }
}
