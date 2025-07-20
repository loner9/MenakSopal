using UnityEngine;

public class NPCInteractionState : NPCState
{
    private float interactionTimer;
    private readonly float maxInteractionTime = 10f;
    
    public NPCInteractionState(NPC npc, NPCStateMachine npcStateMachine) : base(npc, npcStateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        
        interactionTimer = 0f;
        
        // Stop movement
        npc.MoveNPC(Vector2.zero);
        
        // Face the player
        if (npc.player != null)
        {
            Vector2 directionToPlayer = (npc.player.position - npc.transform.position).normalized;
            npc.SetAnimationDirection(directionToPlayer);
        }
        
        // Set animation to idle (speed = 0 but facing player)
        if (npc.animator != null)
        {
            npc.animator.SetFloat("speed", 0f);
        }
        
        // Trigger interaction start
        npc.StartInteraction();
        npc.AnimationTriggerEvent(NPC.AnimationTriggerType.InteractionStart);
    }

    public override void ExitState()
    {
        base.ExitState();
        npc.EndInteraction();
        npc.AnimationTriggerEvent(NPC.AnimationTriggerType.InteractionEnd);
        
        // Show appropriate bubble after interaction ends
        npc.UpdateBubbleForCurrentState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        
        interactionTimer += Time.deltaTime;
        
        // End interaction if player moves away or time limit reached
        if (!npc.isPlayerInRange || interactionTimer >= maxInteractionTime)
        {
            // Return to previous state or idle based on schedule
            if (npc.HasNewScheduleCommand())
            {
                npc.ProcessScheduleCommand();
            }
            else
            {
                npcStateMachine.ChangeState(npc.IdleState);
            }
            return;
        }
        
        // Keep facing the player during interaction
        if (npc.player != null && npc.isPlayerInRange)
        {
            Vector2 directionToPlayer = (npc.player.position - npc.transform.position).normalized;
            npc.SetAnimationDirection(directionToPlayer);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        // Stay stationary during interaction
        npc.MoveNPC(Vector2.zero);
    }
}