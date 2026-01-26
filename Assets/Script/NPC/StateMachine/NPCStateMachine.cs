using UnityEngine;

public class NPCStateMachine
{
    public NPCState CurrentNPCState { get; private set; }

    // Reference to owner NPC for event firing
    private NPC owner;

    public void SetOwner(NPC npc)
    {
        owner = npc;
    }

    public void Initialize(NPCState startingState)
    {
        CurrentNPCState = startingState;
        CurrentNPCState?.EnterState();
    }

    public void ChangeState(NPCState newState)
    {
        if (newState == null) return;

        string oldStateName = CurrentNPCState?.GetType().Name ?? "None";
        string newStateName = newState.GetType().Name;

        CurrentNPCState?.ExitState();
        CurrentNPCState = newState;
        CurrentNPCState.EnterState();

        // Fire state change event
        if (owner != null && oldStateName != newStateName)
        {
            NPCEvents.RaiseNPCStateChanged(owner, oldStateName, newStateName);
        }
    }
}
