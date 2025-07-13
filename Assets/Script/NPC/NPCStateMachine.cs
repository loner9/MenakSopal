using UnityEngine;

// NPC State Machine
public class NPCStateMachine
{
    public NPCState CurrentNPCState { get; private set; }

    public void Initialize(NPCState startingState)
    {
        CurrentNPCState = startingState;
        CurrentNPCState.EnterState();
    }

    public void ChangeState(NPCState newState)
    {
        if (newState == null) return;
        
        CurrentNPCState?.ExitState();
        CurrentNPCState = newState;
        CurrentNPCState.EnterState();
    }
}

// Base NPC State
public class NPCState
{
    protected NPC npc;
    protected NPCStateMachine npcStateMachine;

    public NPCState(NPC npc, NPCStateMachine npcStateMachine)
    {
        this.npc = npc;
        this.npcStateMachine = npcStateMachine;
    }
    
    public virtual void EnterState()
    {
        Debug.Log($"NPC {npc.npcName}: Entering {GetType().Name}");
    }

    public virtual void ExitState()
    {
        Debug.Log($"NPC {npc.npcName}: Exiting {GetType().Name}");
    }

    public virtual void FrameUpdate()
    {
    }

    public virtual void PhysicsUpdate()
    {
    }

    public virtual void AnimationTriggerEvent(NPC.AnimationTriggerType triggerType)
    {
    }
}

// Schedule Data ScriptableObject
[System.Serializable]
public class NPCScheduleData
{
    [Header("Day Schedule")]
    public Vector2 dayPosition;
    public NPCBehavior dayBehavior = NPCBehavior.Work;
    public string dayAnimation = "Work";
    
    [Header("Night Schedule")]
    public Vector2 nightPosition;
    public NPCBehavior nightBehavior = NPCBehavior.Sleep;
    public string nightAnimation = "Sleep";
    
    [Header("Work Settings")]
    public Transform workStation;
    public float workDuration = 5f;
    
    [Header("Interaction Settings")]
    public string[] dialogues;
    public bool availableAtNight = false;
}

public enum NPCBehavior
{
    Idle,
    Walk,
    Work,
    Sleep,
    Interact,
    Flee
}

// Concrete States
public class NPCIdleState : NPCState
{
    private float idleTimer;
    private readonly float maxIdleTime = 5f;
    
    public NPCIdleState(NPC npc, NPCStateMachine npcStateMachine) : base(npc, npcStateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        idleTimer = 0f;
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
        
        // Occasionally look around or move to scheduled position
        if (idleTimer >= maxIdleTime)
        {
            Vector2 scheduledPos = npc.GetScheduledPosition();
            float distanceToScheduled = Vector2.Distance(npc.transform.position, scheduledPos);
            
            if (distanceToScheduled > 1f)
            {
                npc.GetMoveCommand(scheduledPos);
                npcStateMachine.ChangeState(npc.WalkState);
            }
            else
            {
                idleTimer = 0f; // Reset idle timer
            }
        }
    }
}

public class NPCWalkState : NPCState
{
    public NPCWalkState(NPC npc, NPCStateMachine npcStateMachine) : base(npc, npcStateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        
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
        
        if (npc.hasDestination)
        {
            Vector2 movement = npc.GetMovementToDestination();
            npc.MoveNPC(movement);
        }
        else
        {
            // Reached destination, return to appropriate state
            NPCState nextState = GetNextStateAfterWalk();
            npcStateMachine.ChangeState(nextState);
        }
    }
    
    private NPCState GetNextStateAfterWalk()
    {
        if (npc.scheduleData == null)
            return npc.IdleState;
        
        switch (npc.currentTimeOfDay)
        {
            case TimeOfDay.Day:
                return npc.scheduleData.dayBehavior == NPCBehavior.Work ? npc.WorkState : npc.IdleState;
                
            case TimeOfDay.Night:
                return npc.scheduleData.nightBehavior == NPCBehavior.Sleep ? npc.SleepState : npc.IdleState;
                
            default:
                return npc.IdleState;
        }
    }
}

public class NPCWorkState : NPCState
{
    private float workTimer;
    private bool isWorking;
    
    public NPCWorkState(NPC npc, NPCStateMachine npcStateMachine) : base(npc, npcStateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        
        workTimer = 0f;
        isWorking = true;
        
        // Show working bubble
        npc.ShowStatusBubble(NPCBehavior.Work);
        
        // Face work station or work direction
        if (npc.scheduleData?.workStation != null)
        {
            Vector2 workDirection = (npc.scheduleData.workStation.position - npc.transform.position).normalized;
            npc.SetAnimationDirection(workDirection);
        }
        
        // Set animation to idle work animation (speed = 0 but facing direction)
        if (npc.animator != null)
        {
            npc.animator.SetFloat("speed", 0f);
        }
        
        npc.AnimationTriggerEvent(NPC.AnimationTriggerType.WorkStart);
    }

    public override void ExitState()
    {
        base.ExitState();
        isWorking = false;
        npc.HideStatusBubble();
        npc.AnimationTriggerEvent(NPC.AnimationTriggerType.WorkEnd);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        
        if (isWorking)
        {
            workTimer += Time.deltaTime;
            
            // Work for specified duration, then take a break
            if (npc.scheduleData != null && workTimer >= npc.scheduleData.workDuration)
            {
                npcStateMachine.ChangeState(npc.IdleState);
            }
        }
        
        // Check if it's still day time
        if (npc.currentTimeOfDay == TimeOfDay.Night || npc.currentTimeOfDay == TimeOfDay.Sunset)
        {
            npcStateMachine.ChangeState(npc.WalkState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        // Stay stationary while working
        npc.MoveNPC(Vector2.zero);
    }
}

public class NPCSleepState : NPCState
{
    private bool isSleeping;
    
    public NPCSleepState(NPC npc, NPCStateMachine npcStateMachine) : base(npc, npcStateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        
        isSleeping = true;
        
        // Show sleeping bubble
        npc.ShowStatusBubble(NPCBehavior.Sleep);
        
        // Set animation to idle sleeping position (speed = 0, facing down usually)
        if (npc.animator != null)
        {
            npc.animator.SetFloat("speed", 0f);
            npc.animator.SetInteger("orientation", 4); // Facing down for sleep
        }
        
        // Disable interaction during sleep (unless specified otherwise)
        if (npc.scheduleData != null)
        {
            npc.canInteract = npc.scheduleData.availableAtNight;
        }
        
        npc.AnimationTriggerEvent(NPC.AnimationTriggerType.SleepStart);
    }

    public override void ExitState()
    {
        base.ExitState();
        isSleeping = false;
        npc.canInteract = true; // Re-enable interaction
        npc.HideStatusBubble();
        npc.AnimationTriggerEvent(NPC.AnimationTriggerType.SleepEnd);
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        
        // Check if it's day time - wake up
        if (npc.currentTimeOfDay == TimeOfDay.Day || npc.currentTimeOfDay == TimeOfDay.Sunrise)
        {
            npcStateMachine.ChangeState(npc.WalkState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        // Stay stationary while sleeping
        npc.MoveNPC(Vector2.zero);
    }
}

public class NPCInteractState : NPCState
{
    private float interactionTimer;
    private readonly float maxInteractionTime = 10f;
    
    public NPCInteractState(NPC npc, NPCStateMachine npcStateMachine) : base(npc, npcStateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        
        interactionTimer = 0f;
        
        // Keep bubble visible during interaction - interaction system will handle conversation bubbles
        
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
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        
        interactionTimer += Time.deltaTime;
        
        // End interaction if player moves away or time limit reached
        if (!npc.isPlayerInRange || interactionTimer >= maxInteractionTime)
        {
            npcStateMachine.ChangeState(npc.GetStateForTimeOfDay(npc.currentTimeOfDay));
        }
        
        // Keep facing the player
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

public class NPCFleeState : NPCState
{
    private float fleeTimer;
    private readonly float maxFleeTime = 5f;
    
    public NPCFleeState(NPC npc, NPCStateMachine npcStateMachine) : base(npc, npcStateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        
        fleeTimer = 0f;
        
        // Show flee bubble
        npc.ShowStatusBubble(NPCBehavior.Flee);
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
        
        fleeTimer += Time.deltaTime;
        
        if (npc.hasDestination)
        {
            Vector2 movement = npc.GetMovementToDestination();
            // Flee at run speed
            npc.MoveNPC(movement.normalized * npc.runSpeed);
        }
        else
        {
            // Reached safe distance or flee time expired
            if (fleeTimer >= maxFleeTime)
            {
                npcStateMachine.ChangeState(npc.GetStateForTimeOfDay(npc.currentTimeOfDay));
            }
        }
    }
}