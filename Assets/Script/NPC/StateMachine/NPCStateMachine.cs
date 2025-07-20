using UnityEngine;

public class NPCStateMachine
{
    public NPCState CurrentNPCState { get; private set; }

    public void Initialize(NPCState startingState)
    {
        CurrentNPCState = startingState;
        CurrentNPCState?.EnterState();
    }

    public void ChangeState(NPCState newState)
    {
        if (newState == null) return;
        
        CurrentNPCState?.ExitState();
        CurrentNPCState = newState;
        CurrentNPCState.EnterState();
    }
}