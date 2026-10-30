using System.Collections.Generic;
using UnityEngine;


// managger used to spawn and spread out enemy spawn locations, along with allowing for enemies to summon other enemies mid combat.(hasnt been tested)
public class EnemySpawnManager : MonoBehaviour
{
    public enum SlotState { Empty, Occupied, ReservedForMinion }

    [System.Serializable]
    public class ProceduralSlot
    {
        public Vector2 localPosition;
        public EnemySize allowedSize;
        public SlotState currentState;
        public CombatEnemy activeEnemy;

        public ProceduralSlot(Vector2 pos, EnemySize size)
        {
            this.localPosition = pos;
            this.allowedSize = size;
            this.currentState = SlotState.Empty;
            this.activeEnemy = null;
        }
    }

    [Header("Dynamic Grid Settings")]
    [Tooltip("The center point on screen where the enemy group should be clustered.")]
    public Vector2 groupCenterOffset = new Vector2(400f, 0f);

    [Tooltip("The maximum horizontal width the enemy team is allowed to span across your screen.")]
    public float maxGroupWidth = 600f;

    [Tooltip("Default padding space between smaller minion entities.")]
    public float standardSpacing = 150f;

    // dynamic runtime tracking list
    private List<ProceduralSlot> activeSlots = new List<ProceduralSlot>();

    //CORE API: CALL THIS AT THE START OF AN ENCOUNTER ---
    public void SetupEncounterLayout(List<EnemySize> enemySizesInEncounter, int extraReservedMinionSlots = 0)
    {
        // Clear tracking structures from previous fights
        activeSlots.Clear();

        int totalSlotsToCreate = enemySizesInEncounter.Count + extraReservedMinionSlots;
        if (totalSlotsToCreate == 0) return;

        // Calculate condensed spacing dynamically so large squads don't bleed off screen edges
        float calculatedSpacing = standardSpacing;
        float totalWidth = (totalSlotsToCreate - 1) * standardSpacing;

        if (totalWidth > maxGroupWidth)
        {
            calculatedSpacing = maxGroupWidth / (totalSlotsToCreate - 1);
        }

        // Generate coordinate entries from left-to-right (centered around groupCenterOffset)
        for (int i = 0; i < totalSlotsToCreate; i++)
        {
            // Normalize spacing indexes between -0.5 and 0.5 to keep everything centered
            // Change based on vibes
            float t = totalSlotsToCreate > 1 ? (float)i / (totalSlotsToCreate - 1) : 0.5f;
            float normalizedHorizontal = t - 0.5f;

            float xPos = groupCenterOffset.x + (normalizedHorizontal * (totalSlotsToCreate - 1) * calculatedSpacing);
            // Stagger alternate slots slightly on the Y-axis to give it depth, NOTE: change these values to be dynamic
            float yPos = groupCenterOffset.y + (i % 2 == 0 ? 0.5f : -0.5f);

            Vector2 spawnCoords = new Vector2(xPos, yPos);

            // Determine size constraints for this position index
            EnemySize sizeForSlot = EnemySize.Medium;
            SlotState initialState = SlotState.Empty;

            if (i < enemySizesInEncounter.Count)
            {
                sizeForSlot = enemySizesInEncounter[i];
            }
            else
            {
                // Extra spaces generated on the end are locked open automatically for minions
                initialState = SlotState.ReservedForMinion;
            }

            ProceduralSlot newSlot = new ProceduralSlot(spawnCoords, sizeForSlot);
            newSlot.currentState = initialState;
            activeSlots.Add(newSlot);
        }

        UnityEngine.Debug.Log($"[ENCOUNTER GENERATED] Successfully built a dynamic {totalSlotsToCreate}-slot grid layout.");
    }

    //EXECUTION: SPAWN PREFABS ---
    public GameObject SpawnEnemyInSlot(GameObject enemyPrefab, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= activeSlots.Count)
        {
            UnityEngine.Debug.LogError($"Slot index {slotIndex} is outside the generated active grid layout bounds!");
            return null;
        }

        ProceduralSlot targetSlot = activeSlots[slotIndex];

        // Translate local coordinates cleanly into world vectors matching the manager transform position
        Vector3 worldSpawnPosition = transform.TransformPoint(new Vector3(targetSlot.localPosition.x, targetSlot.localPosition.y, 0f));

        GameObject spawnedEnemyInstance = Instantiate(enemyPrefab, worldSpawnPosition, Quaternion.identity, transform);

        targetSlot.activeEnemy = spawnedEnemyInstance.GetComponent<CombatEnemy>();
        targetSlot.currentState = SlotState.Occupied;

        return spawnedEnemyInstance;
    }

    // MID-COMBAT MINION GENERATION API ---
    public GameObject SpawnMinionMidCombat(GameObject minionPrefab)
    {
        CombatEnemy minionComponent = minionPrefab.GetComponent<CombatEnemy>();

        // Find a slot that was explicitly held open for a minion, or find any empty slot
        ProceduralSlot targetSlot = activeSlots.Find(s => s.currentState == SlotState.ReservedForMinion);
        if (targetSlot == null) targetSlot = activeSlots.Find(s => s.currentState == SlotState.Empty);

        if (targetSlot == null)
        {
            UnityEngine.Debug.LogWarning("[SPAWN BLOCKED] No dynamic slot space left in this encounter grid for minions!");
            return null;
        }

        Vector3 worldSpawnPosition = transform.TransformPoint(new Vector3(targetSlot.localPosition.x, targetSlot.localPosition.y, 0f));
        GameObject spawnedEnemyInstance = Instantiate(minionPrefab, worldSpawnPosition, Quaternion.identity, transform);

        targetSlot.activeEnemy = spawnedEnemyInstance.GetComponent<CombatEnemy>();
        targetSlot.currentState = SlotState.Occupied;

        return spawnedEnemyInstance;
    }

    public void FreeUpSlot(CombatEnemy deadEnemy)
    {
        ProceduralSlot slot = activeSlots.Find(s => s.activeEnemy == deadEnemy);
        if (slot != null)
        {
            slot.activeEnemy = null;
            slot.currentState = SlotState.Empty;
        }
    }

    private void OnEnable()
    {
        CombatEnemy.OnEnemyDefeated += HandleEnemyDeath;
    }

    private void OnDisable()
    {

        CombatEnemy.OnEnemyDefeated -= HandleEnemyDeath;

    }

    private void HandleEnemyDeath(CombatEnemy deadEnemy)
    {

        FreeUpSlot(deadEnemy);


    }


}
