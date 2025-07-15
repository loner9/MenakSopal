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

// PATROL STATE - For walking between points
public class NPCPatrolState : NPCState
{
    private int currentPatrolIndex = 0;
    private float pauseTimer = 0f;
    private bool isPaused = false;
    private PatrolPoint[] currentPatrolPoints;
    
    public NPCPatrolState(NPC npc, NPCStateMachine npcStateMachine) : base(npc, npcStateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        
        // Get current patrol points based on time of day
        currentPatrolPoints = npc.GetCurrentPatrolPoints();
        
        if (currentPatrolPoints == null || currentPatrolPoints.Length == 0)
        {
            // No patrol points, go to idle
            npcStateMachine.ChangeState(npc.IdleState);
            return;
        }
        
        // Start at first patrol point or find nearest
        currentPatrolIndex = FindNearestPatrolPoint();
        isPaused = false;
        pauseTimer = 0f;
        
        // Move to first destination
        MoveToCurrentPatrolPoint();
        
        Debug.Log($"NPC {npc.npcName}: Starting patrol with {currentPatrolPoints.Length} points");
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
        
        // Check if we should stop patrolling
        if (npc.GetCurrentActivity() != NPCBehavior.Walk)
        {
            npcStateMachine.ChangeState(npc.IdleState);
            return;
        }
        
        if (npc.ShouldGoHome())
        {
            npcStateMachine.ChangeState(npc.GoHomeState);
            return;
        }
        
        if (isPaused)
        {
            // Handle pause at patrol point
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                // Move to next patrol point
                currentPatrolIndex = (currentPatrolIndex + 1) % currentPatrolPoints.Length;
                MoveToCurrentPatrolPoint();
                isPaused = false;
            }
        }
        else
        {
            // Check if we've reached the current patrol point
            if (!npc.hasDestination)
            {
                // Reached destination, start pause
                PatrolPoint currentPoint = currentPatrolPoints[currentPatrolIndex];
                pauseTimer = currentPoint.pauseDuration;
                isPaused = true;
                
                // Show activity bubble for this point
                npc.ShowStatusBubble(currentPoint.activityAtPoint);
                
                // Set animation to idle during pause
                if (npc.animator != null)
                {
                    npc.animator.SetFloat("speed", 0f);
                }
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        if (!isPaused && npc.hasDestination)
        {
            Vector2 movement = npc.GetMovementToDestination();
            npc.MoveNPC(movement);
        }
        else
        {
            npc.MoveNPC(Vector2.zero);
        }
    }
    
    private void MoveToCurrentPatrolPoint()
    {
        if (currentPatrolPoints != null && currentPatrolIndex < currentPatrolPoints.Length)
        {
            Vector2 destination = currentPatrolPoints[currentPatrolIndex].position;
            npc.GetMoveCommand(destination);
            
            // Show walking bubble
            npc.ShowStatusBubble(NPCBehavior.Walk);
        }
    }
    
    private int FindNearestPatrolPoint()
    {
        if (currentPatrolPoints == null || currentPatrolPoints.Length == 0)
            return 0;
        
        int nearestIndex = 0;
        float nearestDistance = Vector2.Distance(npc.transform.position, currentPatrolPoints[0].position);
        
        for (int i = 1; i < currentPatrolPoints.Length; i++)
        {
            float distance = Vector2.Distance(npc.transform.position, currentPatrolPoints[i].position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }
        
        return nearestIndex;
    }
}

// GO HOME STATE - For going home before despawning
public class NPCGoHomeState : NPCState
{
    private bool reachedHome = false;
    
    public NPCGoHomeState(NPC npc, NPCStateMachine npcStateMachine) : base(npc, npcStateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        
        reachedHome = false;
        
        // Get home position from schedule
        Vector2 homePosition = npc.GetHomePosition();
        npc.GetMoveCommand(homePosition);
        
        // Show walking home bubble (could be same as walk or different)
        npc.ShowStatusBubble(NPCBehavior.Walk);
        
        Debug.Log($"NPC {npc.npcName}: Going home for the night");
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
        
        // Check if we've reached home
        if (!npc.hasDestination && !reachedHome)
        {
            reachedHome = true;
            
            // Notify NPC Manager to despawn this NPC
            NPCManager npcManager = UnityEngine.Object.FindObjectOfType<NPCManager>();
            if (npcManager != null)
            {
                npcManager.DespawnNPC(npc);
            }
            else
            {
                // Fallback - just disable the NPC
                npc.gameObject.SetActive(false);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        
        if (npc.hasDestination && !reachedHome)
        {
            Vector2 movement = npc.GetMovementToDestination();
            npc.MoveNPC(movement);
        }
        else
        {
            npc.MoveNPC(Vector2.zero);
        }
    }
}