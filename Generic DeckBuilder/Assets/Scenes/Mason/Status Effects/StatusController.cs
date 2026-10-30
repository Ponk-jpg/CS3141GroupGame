using System;
using System.Collections.Generic;
using UnityEngine;

// controller that is assigned to each entity that can take status effects
public class StatusController : MonoBehaviour
{
    // A clean runtime struct pairing the static data with its current runtime stack count
    [System.Serializable]
    public class ActiveStatus
    {
        public StatusEffectData data;
        public int stacks;

        public ActiveStatus(StatusEffectData data, int initialStacks)
        {
            this.data = data;
            this.stacks = initialStacks;
        }
    }

    // Broadcasts an update passing the active statuses list whenever a stack counts shift
    public Action<List<ActiveStatus>> OnStatusesChanged;
    // global static event that any card can listen to to update.
    public static Action OnAnyStatusChanged;
    // public hook for damage calculator
    public List<ActiveStatus> GetActiveStatusesList() => activeStatuses;

    private List<ActiveStatus> activeStatuses = new List<ActiveStatus>();
    



    // UNIVERSAL API: Call this from cards or intents to apply a buff/debuff
    public void ApplyStatus(StatusEffectData statusData, int amount)
    {
        ActiveStatus existingStatus = activeStatuses.Find(s => s.data == statusData);

        if (existingStatus != null)
        {
            existingStatus.stacks += amount;
        }
        else
        {
            existingStatus = new ActiveStatus(statusData, amount);
            activeStatuses.Add(existingStatus);
        }
        
        UnityEngine.Debug.Log($"[STATUS APPLIED] {gameObject.name} now has {existingStatus.stacks} stacks of {statusData.effectName}");
        OnStatusesChanged?.Invoke(activeStatuses);

        OnAnyStatusChanged?.Invoke();
    }

    // universal function to just clear ALL status effects on this given entity. mainly used for the end of combat clears
    public void ClearAllStatuses()
    {
        activeStatuses.Clear();

        // Broadcast the update so the visual UI icon grids instantly vanish
        OnStatusesChanged?.Invoke(activeStatuses);

        // Notify the global card network to update hand descriptions back to default white values
        OnAnyStatusChanged?.Invoke();

        UnityEngine.Debug.Log($"[STATUS CLEANUP] All active buffs and debuffs successfully purged from {gameObject.name}.");
    }


    // Call this during turn transitions to decrement ticking status loops
    public void TickEndOfTurnStatuses()
    {
        for (int i = activeStatuses.Count - 1; i >= 0; i--)
        {
            if (activeStatuses[i].data.decaysAtEndOfTurn)
            {
                activeStatuses[i].stacks--;

                if (activeStatuses[i].stacks <= 0 && activeStatuses[i].data.hideAtZeroStacks)
                {
                    activeStatuses.RemoveAt(i);
                }
            }
        }
        OnStatusesChanged?.Invoke(activeStatuses);

        OnAnyStatusChanged?.Invoke();

    }

    public int GetStatusStacks(StatusEffectData statusData)
    {
        ActiveStatus status = activeStatuses.Find(s => s.data == statusData);
        return status != null ? status.stacks : 0;
    }
}
