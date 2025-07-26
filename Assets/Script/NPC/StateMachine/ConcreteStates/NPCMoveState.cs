using UnityEngine;

public class NPCMoveState : NPCState
{
    private Vector2 targetDestination;
    private bool shouldIdleWhenReached;
    private bool shouldMoveAroundDestination;
    
    public NPCMoveState(NPC npc, NPCStateMachine npcStateMachine) : base(npc, npcStateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        
        // Get movement parameters from the NPC
        targetDestination = npc.GetCurrentDestination();
        shouldIdleWhenReached = npc.ShouldIdleWhenReached();
        shouldMoveAroundDestination = npc.ShouldMoveAroundWhenIdle();
        
        // Show walking bubble
        npc.ShowStatusBubble(NPCBehavior.Walk);
    }

    public override void ExitState()
    {
        base.ExitState();
        npc.MoveNPC(Vector2.zero);
        npc.HideStatusBubble();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        
        // Check if we have a valid destination
        if (!npc.hasDestination)
        {
            // Reached destination, determine next behavior
            HandleDestinationReached();
            return;
        }
        
        // Continue moving towards destination
        Vector2 movement = npc.GetMovementToDestination();
        npc.MoveNPC(movement);
        
        // Check if NPC Manager has given us new instructions while moving
        if (npc.HasNewScheduleCommand())
        {
            npc.ProcessScheduleCommand();
        }
    }
    
    private void HandleDestinationReached()
    {
        // Notify NPC Manager that we've reached the destination
        npc.NotifyDestinationReached();
        
        // Check if this was a GoHome command (meaning we should despawn)
        if (npc.ShouldDespawnAfterReachingDestination())
        {
            npc.RequestDespawn();
            return;
        }
        
        // Determine next state based on schedule
        if (shouldIdleWhenReached)
        {
            // Set up idle position at current location
            npc.SetIdlePosition(npc.transform.position);
            npcStateMachine.ChangeState(npc.IdleState);
        }
        else
        {
            // Check if we have another movement command queued
            if (npc.HasNewScheduleCommand())
            {
                npc.ProcessScheduleCommand();
            }
            else
            {
                // Default to idle if no further instructions
                npc.SetIdlePosition(npc.transform.position);
                npcStateMachine.ChangeState(npc.IdleState);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        // Movement is handled in FrameUpdate to ensure proper pathfinding
        if (npc.hasDestination)
        {
            Vector2 movement = npc.GetMovementToDestination();
            npc.MoveNPC(movement);
        }
    }
}