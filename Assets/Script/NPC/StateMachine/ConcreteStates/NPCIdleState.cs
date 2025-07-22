using UnityEngine;

public class NPCIdleState : NPCState
{
    private float idleTimer;
    private float randomMovementTimer;
    private Vector2 currentIdlePosition;
    private readonly float maxIdleTime = 5f;
    private readonly float randomMovementInterval = 3f;
    private readonly float randomMovementRange = 2f;
    
    public NPCIdleState(NPC npc, NPCStateMachine npcStateMachine) : base(npc, npcStateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        idleTimer = 0f;
        randomMovementTimer = 0f;
        currentIdlePosition = npc.transform.position;
        
        // Stop movement and set animation
        npc.MoveNPC(Vector2.zero);
        
        // Show idle bubble
        npc.ShowStatusBubble(NPCBehavior.Idle);
        
        // Set animation speed to 0 for idle
        if (npc.animator != null)
        {
            npc.animator.SetFloat("speed", 0f);
        }
    }

    public override void ExitState()
    {
        base.ExitState();
        npc.HideStatusBubble();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        
        idleTimer += Time.deltaTime;
        randomMovementTimer += Time.deltaTime;
        
        // Check if we should move around the idle area (if enabled in schedule)
        if (npc.scheduleData != null && npc.ShouldMoveAroundWhenIdle())
        {
            HandleRandomMovement();
        }
        
        // Check if NPC Manager has given us new instructions
        if (npc.HasNewScheduleCommand())
        {
            npc.ProcessScheduleCommand();
        }
    }
    
    private void HandleRandomMovement()
    {
        if (randomMovementTimer >= randomMovementInterval)
        {
            // Get a random point near the idle position
            Vector2 randomOffset = Random.insideUnitCircle * randomMovementRange;
            Vector2 targetPosition = currentIdlePosition + randomOffset;
            
            // Move to the random position
            npc.GetMoveCommand(targetPosition);
            npcStateMachine.ChangeState(npc.MoveState);
            
            randomMovementTimer = 0f;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        // Stay stationary during idle
        npc.MoveNPC(Vector2.zero);
    }
}