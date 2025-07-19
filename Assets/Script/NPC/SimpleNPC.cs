using UnityEngine;

public class SimpleNPC : MonoBehaviour
{
    [Header("Current State - Runtime")]
    [SerializeField] private NPCState currentState = NPCState.Idle;
    [SerializeField] private bool isMoving = false;
    [SerializeField] private bool canInteract = true; // Only true when idle
    public bool isInteractable { get; private set; }
    [Header("Movement")]
    public float arrivalThreshold = 0.1f;

    // References
    private SimpleNPCScheduleData scheduleData;
    private SyncedScriptableNPCManager npcManager;
    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;

    // Current schedule tracking
    private HourlyScheduleEntry currentSchedule;
    private Vector2 currentDestination;
    private bool hasDestination = false;
    private float waypointBreakTimer = 0f;

    // Interaction
    private float interactionRange = 1.5f;
    private bool isPlayerNearby = false;

    // Idle walking
    private Vector2 idleCenter;
    private Vector2 idleTarget;
    private float idleWalkTimer = 0f;
    private float idleWalkInterval = 3f; // Change idle target every 3 seconds

    public SimpleNPCScheduleData GetScheduleData()
    {
        return scheduleData;
    }

    public enum NPCState
    {
        Idle,           // Standing still or walking nearby randomly
        MovingToSchedule, // Walking to scheduled destination
        AtWaypoint,     // Paused at a waypoint
        Interacting     // Talking with player
    }

    #region Initialization

    public void Initialize(SimpleNPCScheduleData data, SyncedScriptableNPCManager manager)
    {
        scheduleData = data;
        npcManager = manager;

        // Get components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // 2D top-down game
        }

        // Find player
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.transform;
        }

        // Set up initial schedule for current hour
        int currentHour = npcManager.GetCurrentHour();
        OnHourChanged(currentHour);

        // Set idle center to spawn position
        idleCenter = transform.position;

        Debug.Log($"SimpleNPC {scheduleData.npcName}: Initialized and ready");
    }

    #endregion

    #region Update Methods

    void Update()
    {
        CheckPlayerProximity();
        UpdateCurrentBehavior();
        UpdateAnimator();
    }

    void UpdateCurrentBehavior()
    {
        switch (currentState)
        {
            case NPCState.Idle:
                HandleIdleState();
                break;

            case NPCState.MovingToSchedule:
                HandleMovingToScheduleState();
                break;

            case NPCState.AtWaypoint:
                HandleAtWaypointState();
                break;

            case NPCState.Interacting:
                HandleInteractingState();
                break;
        }
    }

    #endregion

    #region State Handling

    void HandleIdleState()
    {
        // Can interact with player when idle
        canInteract = true;

        // Check if we should be completely idle (not moving at all)
        bool shouldStayStill = (currentSchedule != null &&
                               currentSchedule.destinationType == DestinationType.SinglePoint &&
                               currentSchedule.shouldCompletelyIdle);

        if (shouldStayStill)
        {
            // Completely stationary idle - like working at a specific station
            isMoving = false;
        }
        else
        {
            // Active idle - random walking within radius
            if (scheduleData != null && scheduleData.idleWalkRadius > 0)
            {
                idleWalkTimer += Time.deltaTime;

                if (idleWalkTimer >= idleWalkInterval)
                {
                    SetNewIdleTarget();
                    idleWalkTimer = 0f;
                }

                // Move towards idle target
                if (Vector2.Distance(transform.position, idleTarget) > arrivalThreshold)
                {
                    MoveTowards(idleTarget, scheduleData.walkSpeed * 0.5f); // Slower idle walking
                    isMoving = true;
                }
                else
                {
                    isMoving = false;
                }
            }
            else
            {
                isMoving = false;
            }
        }
    }

    void HandleMovingToScheduleState()
    {
        // Cannot interact while moving to scheduled destination
        canInteract = false;

        if (!hasDestination)
        {
            // No destination set, go back to idle
            ChangeState(NPCState.Idle);
            return;
        }

        // Move towards current destination
        float distance = Vector2.Distance(transform.position, currentDestination);

        if (distance <= arrivalThreshold)
        {
            // Arrived at destination
            ArrivedAtDestination();
        }
        else
        {
            MoveTowards(currentDestination, scheduleData.walkSpeed);
            isMoving = true;
        }
    }

    void HandleAtWaypointState()
    {
        // Cannot interact while taking a break at waypoint
        canInteract = false;
        isMoving = false;

        // Count down break time
        waypointBreakTimer -= Time.deltaTime;

        if (waypointBreakTimer <= 0f)
        {
            // Break time over, continue to next waypoint or finish
            ContinueWaypointRoute();
        }
    }

    void HandleInteractingState()
    {
        // Stop all movement when interacting
        isMoving = false;
        canInteract = true;

        // Face the player if available
        if (player != null)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            // You can add facing logic here if needed
        }

        // Return to previous state when player moves away
        if (!isPlayerNearby)
        {
            ReturnToPreviousState();
        }
    }

    #endregion

    #region Schedule Management

    public void OnHourChanged(int newHour)
    {
        // Get new schedule for this hour
        HourlyScheduleEntry newSchedule = scheduleData.GetScheduleForHour(newHour);

        if (newSchedule != null)
        {
            // We have a schedule for this hour
            currentSchedule = newSchedule;
            StartNewSchedule();
        }
        else
        {
            // No specific schedule for this hour, just idle at current location
            currentSchedule = null;
            ChangeState(NPCState.Idle);
            idleCenter = transform.position; // Update idle center to current position
        }
    }

    void StartNewSchedule()
    {
        if (currentSchedule == null) return;

        Debug.Log($"SimpleNPC {scheduleData.npcName}: Starting new schedule - {currentSchedule.description}");

        // Reset waypoint route if it's a new route
        if (currentSchedule.destinationType == DestinationType.MultiplePoints && currentSchedule.waypointRoute != null)
        {
            currentSchedule.waypointRoute.currentWaypointIndex = 0;
            currentSchedule.waypointRoute.timeAtCurrentWaypoint = 0f;
        }

        // Set destination and start moving
        SetDestinationFromSchedule();

        if (hasDestination)
        {
            ChangeState(NPCState.MovingToSchedule);
        }
        else
        {
            ChangeState(NPCState.Idle);
        }
    }

    void SetDestinationFromSchedule()
    {
        if (currentSchedule == null)
        {
            hasDestination = false;
            return;
        }

        currentDestination = currentSchedule.GetCurrentDestination();
        hasDestination = currentDestination != Vector2.zero;

        if (hasDestination)
        {
            Debug.Log($"SimpleNPC {scheduleData.npcName}: Moving to {currentDestination}");
        }
    }

    void ArrivedAtDestination()
    {
        Debug.Log($"SimpleNPC {scheduleData.npcName}: Arrived at destination {currentDestination}");

        // Check if we should despawn
        if (currentSchedule != null && currentSchedule.shouldDespawn)
        {
            Debug.Log($"SimpleNPC {scheduleData.npcName}: Despawning as scheduled");
            npcManager.RequestNPCDespawn(this);
            return;
        }

        // Handle different destination types
        if (currentSchedule != null && currentSchedule.destinationType == DestinationType.MultiplePoints)
        {
            // This is a waypoint route
            if (currentSchedule.HasMoreWaypoints() || currentSchedule.waypointRoute.isLooped)
            {
                // Take a break at this waypoint
                // Convert game hours to real seconds based on a reasonable default
                // Since we're synced with DayNightCycle, we'll use a standard conversion
                float gameHoursToRealSeconds = 60f; // 1 game hour = 60 real seconds (configurable)
                float breakTimeInSeconds = currentSchedule.waypointRoute.breakTimeAtEachWaypoint * gameHoursToRealSeconds;
                waypointBreakTimer = breakTimeInSeconds;
                ChangeState(NPCState.AtWaypoint);
            }
            else
            {
                // Reached end of non-looped route
                FinishCurrentSchedule();
            }
        }
        else
        {
            // Single destination - we're done with this schedule
            FinishCurrentSchedule();
        }
    }
    void ContinueWaypointRoute()
    {
        if (currentSchedule == null || currentSchedule.waypointRoute == null)
        {
            ChangeState(NPCState.Idle);
            return;
        }

        // Move to next waypoint
        currentSchedule.MoveToNextWaypoint();
        SetDestinationFromSchedule();

        if (hasDestination)
        {
            ChangeState(NPCState.MovingToSchedule);
        }
        else
        {
            FinishCurrentSchedule();
        }
    }

    void FinishCurrentSchedule()
    {
        // Set idle center to current position for random walking
        idleCenter = transform.position;

        // Determine idle behavior based on schedule settings
        if (currentSchedule != null)
        {
            if (currentSchedule.destinationType == DestinationType.SinglePoint && currentSchedule.shouldCompletelyIdle)
            {
                // NPC should stay completely still at this exact position (like working at a station)
                Debug.Log($"SimpleNPC {scheduleData.npcName}: Starting stationary idle at work position");
            }
            else if (currentSchedule.arrivalBehavior == ArrivalBehavior.IdleAndWalkNearby)
            {
                // NPC can walk around the area randomly
                Debug.Log($"SimpleNPC {scheduleData.npcName}: Starting active idle with random walking");
            }
            else
            {
                // Default idle behavior
                Debug.Log($"SimpleNPC {scheduleData.npcName}: Starting idle at location");
            }
        }

        ChangeState(NPCState.Idle);
        hasDestination = false;
    }

    #endregion

    #region Movement

    void MoveTowards(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        Vector2 movement = direction * speed * Time.deltaTime;

        if (rb != null)
        {
            rb.MovePosition(rb.position + movement);
        }
        else
        {
            transform.Translate(movement);
        }
    }

    void SetNewIdleTarget()
    {
        // Pick random position within idle radius
        Vector2 randomOffset = Random.insideUnitCircle * scheduleData.idleWalkRadius;
        idleTarget = idleCenter + randomOffset;
    }

    #endregion

    #region Interaction System

    void CheckPlayerProximity()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        bool wasNearby = isPlayerNearby;
        isPlayerNearby = distance <= interactionRange;

        // Start interaction if player approaches and NPC can interact
        if (isPlayerNearby && !wasNearby && canInteract && currentState != NPCState.Interacting)
        {
            ChangeState(NPCState.Interacting);
        }
    }

    public bool CanInteractWithPlayer()
    {
        return canInteract && isPlayerNearby && currentState == NPCState.Idle;
    }

    public void StartInteraction()
    {
        if (CanInteractWithPlayer())
        {
            ChangeState(NPCState.Interacting);
        }
    }

    void ReturnToPreviousState()
    {
        if (hasDestination)
        {
            ChangeState(NPCState.MovingToSchedule);
        }
        else
        {
            ChangeState(NPCState.Idle);
        }
    }

    #endregion

    #region State Management

    void ChangeState(NPCState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        Debug.Log($"SimpleNPC {scheduleData.npcName}: Changed state to {newState}");
    }

    #endregion

    #region Animation

    void UpdateAnimator()
    {
        if (animator == null) return;

        // Simple animation - just speed for walking
        animator.SetFloat("speed", isMoving ? 1f : 0f);

        // You can add orientation/direction here if needed
    }

    #endregion

    #region Debug

    void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Draw current destination
        if (hasDestination)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentDestination);
            Gizmos.DrawWireSphere(currentDestination, 0.3f);
        }

        // Draw idle area
        if (currentState == NPCState.Idle && scheduleData != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(idleCenter, scheduleData.idleWalkRadius);
        }

        // Draw waypoint route if applicable
        if (currentSchedule != null && currentSchedule.destinationType == DestinationType.MultiplePoints && currentSchedule.waypointRoute != null)
        {
            Gizmos.color = Color.blue;
            Vector2[] waypoints = currentSchedule.waypointRoute.waypoints;

            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
            }

            // Draw loop connection if looped
            if (currentSchedule.waypointRoute.isLooped && waypoints.Length > 1)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(waypoints[waypoints.Length - 1], waypoints[0]);
            }
        }
    }

    #endregion
}