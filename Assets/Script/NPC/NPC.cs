using UnityEngine;
using System.Collections.Generic;
using Aoiti.Pathfinding;

public class NPC : MonoBehaviour
{
    [Header("NPC Basic Info")]
    public string npcName = "NPC";
    public NPCType npcType = NPCType.Villager;
    
    [Header("Day/Night Schedule")]
    public NPCScheduleData scheduleData;
    
    [Header("Interaction")]
    public float interactionRange = 2f;
    public bool canInteract = true;
    
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float runSpeed = 3f;
    
    [Header("Animation")]
    public Animator animator;
    
    [Header("Pathfinding")]
    public LayerMask obstacleLayerMask = -1;
    
    // Animation parameter hashes
    private readonly int IsMoving = Animator.StringToHash("IsMoving");
    private readonly int XValue = Animator.StringToHash("X");
    private readonly int YValue = Animator.StringToHash("Y");
    private readonly int NPCState = Animator.StringToHash("NPCState");
    
    // Components
    public Rigidbody2D rb { get; private set; }
    
    // State Machine
    public NPCStateMachine StateMachine { get; private set; }
    public NPCIdleState IdleState { get; private set; }
    public NPCWalkState WalkState { get; private set; }
    public NPCWorkState WorkState { get; private set; }
    public NPCInteractState InteractState { get; private set; }
    public NPCSleepState SleepState { get; private set; }
    public NPCFleeState FleeState { get; private set; }
    
    // Pathfinding
    public Pathfinder<Vector2> pathfinder;
    public List<Vector2> pathLeftToGo = new List<Vector2>();
    
    // Current behavior data
    public Vector2 currentDestination;
    public bool hasDestination = false;
    public Transform player;
    public bool isPlayerInRange = false;
    public TimeOfDay currentTimeOfDay;
    public DayNightCycle dayNightCycle;
    
    // Interaction system
    public System.Action<NPC> OnInteractionStart;
    public System.Action<NPC> OnInteractionEnd;
    
    public enum NPCType
    {
        Villager,
        Merchant,
        Guard,
        Worker,
        Child,
        Elder
    }
    
    public enum AnimationTriggerType
    {
        InteractionStart,
        InteractionEnd,
        WorkStart,
        WorkEnd,
        SleepStart,
        SleepEnd
    }
    
    private void Awake()
    {
        StateMachine = new NPCStateMachine();
        IdleState = new NPCIdleState(this, StateMachine);
        WalkState = new NPCWalkState(this, StateMachine);
        WorkState = new NPCWorkState(this, StateMachine);
        InteractState = new NPCInteractState(this, StateMachine);
        SleepState = new NPCSleepState(this, StateMachine);
        FleeState = new NPCFleeState(this, StateMachine);
    }
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (animator == null)
            animator = GetComponent<Animator>();
        
        // Find player
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            player = playerGO.transform;
        
        // Find day/night cycle
        dayNightCycle = FindObjectOfType<DayNightCycle>();
        if (dayNightCycle != null)
        {
            dayNightCycle.OnTimeOfDayChanged += OnTimeOfDayChanged;
            currentTimeOfDay = dayNightCycle.CurrentTimeOfDay;
        }
        
        // Initialize pathfinder
        pathfinder = new Pathfinder<Vector2>(GetDistance, GetNeighbourNodes, 1000);
        
        // Start with appropriate state based on time of day
        StateMachine.Initialize(GetStateForTimeOfDay(currentTimeOfDay));
    }
    
    private void Update()
    {
        if (StateMachine?.CurrentNPCState != null)
        {
            StateMachine.CurrentNPCState.FrameUpdate();
        }
        
        CheckPlayerProximity();
        
        // Debug pathfinding
        for (int i = 0; i < pathLeftToGo.Count - 1; i++)
        {
            Debug.DrawLine(pathLeftToGo[i], pathLeftToGo[i + 1], Color.green);
        }
    }
    
    private void FixedUpdate()
    {
        if (StateMachine?.CurrentNPCState != null)
        {
            StateMachine.CurrentNPCState.PhysicsUpdate();
        }
    }
    
    #region Pathfinding
    public float GetDistance(Vector2 A, Vector2 B)
    {
        return (A - B).sqrMagnitude;
    }
    
    public Dictionary<Vector2, float> GetNeighbourNodes(Vector2 currentTile)
    {
        Dictionary<Vector2, float> neighbours = new Dictionary<Vector2, float>();
        
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        
        foreach (Vector2 direction in directions)
        {
            Vector2 neighbourPos = currentTile + direction;
            
            // Check if the position is walkable (no obstacles)
            if (!Physics2D.OverlapCircle(neighbourPos, 0.3f, obstacleLayerMask))
            {
                neighbours.Add(neighbourPos, 1f);
            }
        }
        
        return neighbours;
    }
    
    public void GetMoveCommand(Vector2 target)
    {
        Vector2 startPos = (Vector2)transform.position;
        
        if (pathfinder.GenerateAstarPath(startPos, target, out pathLeftToGo))
        {
            currentDestination = target;
            hasDestination = true;
        }
        else
        {
            Debug.LogWarning($"NPC {npcName}: Could not find path to target");
            hasDestination = false;
        }
    }
    #endregion
    
    #region Movement
    public void MoveNPC(Vector2 velocity)
    {
        rb.linearVelocity = velocity;
        
        // Update animation parameters
        if (animator != null)
        {
            bool isMoving = velocity.sqrMagnitude > 0.01f;
            animator.SetBool(IsMoving, isMoving);
            
            if (isMoving)
            {
                animator.SetFloat(XValue, velocity.normalized.x);
                animator.SetFloat(YValue, velocity.normalized.y);
            }
        }
    }
    
    public Vector2 GetMovementToDestination()
    {
        if (pathLeftToGo.Count > 0)
        {
            Vector2 direction = (pathLeftToGo[0] - (Vector2)transform.position).normalized;
            
            // Remove waypoint if we're close enough
            if (Vector2.Distance(transform.position, pathLeftToGo[0]) < 0.5f)
            {
                pathLeftToGo.RemoveAt(0);
            }
            
            return direction * moveSpeed;
        }
        
        hasDestination = false;
        return Vector2.zero;
    }
    #endregion
    
    #region Day/Night System Integration
    private void OnTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        currentTimeOfDay = newTimeOfDay;
        
        // Change behavior based on time of day
        NPCState newState = GetStateForTimeOfDay(newTimeOfDay);
        if (newState != StateMachine.CurrentNPCState)
        {
            StateMachine.ChangeState(newState);
        }
    }
    
    public NPCState GetStateForTimeOfDay(TimeOfDay timeOfDay)
    {
        if (scheduleData == null)
            return IdleState;
        
        switch (timeOfDay)
        {
            case TimeOfDay.Day:
                return scheduleData.dayBehavior == NPCBehavior.Work ? WorkState : 
                       scheduleData.dayBehavior == NPCBehavior.Walk ? WalkState : IdleState;
                       
            case TimeOfDay.Night:
                return scheduleData.nightBehavior == NPCBehavior.Sleep ? SleepState :
                       scheduleData.nightBehavior == NPCBehavior.Walk ? WalkState : IdleState;
                       
            case TimeOfDay.Sunrise:
            case TimeOfDay.Sunset:
                return WalkState; // Transition periods
                
            default:
                return IdleState;
        }
    }
    
    public Vector2 GetScheduledPosition()
    {
        if (scheduleData == null)
            return transform.position;
        
        switch (currentTimeOfDay)
        {
            case TimeOfDay.Day:
            case TimeOfDay.Sunrise:
                return scheduleData.dayPosition;
                
            case TimeOfDay.Night:
            case TimeOfDay.Sunset:
                return scheduleData.nightPosition;
                
            default:
                return transform.position;
        }
    }
    #endregion
    
    #region Interaction System
    private void CheckPlayerProximity()
    {
        if (player == null) return;
        
        float distance = Vector2.Distance(transform.position, player.position);
        bool wasInRange = isPlayerInRange;
        isPlayerInRange = distance <= interactionRange;
        
        // Trigger interaction state change if needed
        if (isPlayerInRange && !wasInRange && canInteract)
        {
            // Player entered range - could switch to interaction state
            if (StateMachine.CurrentNPCState != InteractState && StateMachine.CurrentNPCState != FleeState)
            {
                // Only interact if not in important states
                if (StateMachine.CurrentNPCState == IdleState || StateMachine.CurrentNPCState == WalkState)
                {
                    StateMachine.ChangeState(InteractState);
                }
            }
        }
        else if (!isPlayerInRange && wasInRange)
        {
            // Player left range - return to scheduled behavior
            if (StateMachine.CurrentNPCState == InteractState)
            {
                StateMachine.ChangeState(GetStateForTimeOfDay(currentTimeOfDay));
            }
        }
    }
    
    public void StartInteraction()
    {
        if (!canInteract) return;
        
        OnInteractionStart?.Invoke(this);
        Debug.Log($"Started interaction with {npcName}");
    }
    
    public void EndInteraction()
    {
        OnInteractionEnd?.Invoke(this);
        Debug.Log($"Ended interaction with {npcName}");
    }
    #endregion
    
    #region Animation Events
    public void AnimationTriggerEvent(AnimationTriggerType triggerType)
    {
        StateMachine.CurrentNPCState?.AnimationTriggerEvent(triggerType);
    }
    
    public void SetAnimationState(string stateName)
    {
        if (animator != null)
        {
            animator.SetTrigger(stateName);
        }
    }
    #endregion
    
    #region Public Methods
    public void ForceBehavior(NPCState state)
    {
        StateMachine.ChangeState(state);
    }
    
    public void ResetToScheduledBehavior()
    {
        StateMachine.ChangeState(GetStateForTimeOfDay(currentTimeOfDay));
    }
    
    public void FleeFromThreat(Vector2 threatPosition)
    {
        // Calculate flee direction
        Vector2 fleeDirection = ((Vector2)transform.position - threatPosition).normalized;
        Vector2 fleeTarget = (Vector2)transform.position + fleeDirection * 10f;
        
        GetMoveCommand(fleeTarget);
        StateMachine.ChangeState(FleeState);
    }
    #endregion
    
    private void OnDestroy()
    {
        if (dayNightCycle != null)
        {
            dayNightCycle.OnTimeOfDayChanged -= OnTimeOfDayChanged;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw scheduled positions
        if (scheduleData != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(scheduleData.dayPosition, 0.5f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(scheduleData.nightPosition, 0.5f);
        }
    }
}