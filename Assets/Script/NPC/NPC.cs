using UnityEngine;
using System.Collections.Generic;
using Aoiti.Pathfinding;
using UnityEngine.UI;

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

    [Header("Animation & Appearance")]
    public Animator animator;
    public string SpriteSheetName = "chara_01"; // Default sprite sheet
    public SpriteRenderer spriteRenderer;

    [Header("Status Bubble System")]
    public GameObject bubblePrefab; // Assign a UI bubble prefab
    public Transform bubbleParent; // Parent transform for bubble (usually Canvas)
    public Vector3 bubbleOffset = new Vector3(0, 1.5f, 0); // Offset above NPC head

    [Header("Bubble Status Images")]
    public Sprite workingBubbleSprite;
    public Sprite sleepingBubbleSprite;
    public Sprite idleBubbleSprite;
    public Sprite walkingBubbleSprite;
    public Sprite fleeingBubbleSprite;

    [Header("Pathfinding")]
    public LayerMask obstacleLayerMask = -1;

    [Header("Dialogue System")]
    [Tooltip("Optional DialogueData asset for advanced dialogue features. If not assigned, will use NPCScheduleData.dialogues or Resources folder.")]
    public DialogueData dialogueData;

    [Header("Dialogue Fallback")]
    [Tooltip("Simple dialogue lines if no DialogueData is assigned (legacy support)")]
    [TextArea(3, 5)]
    public string[] simpleDialogues;

    // Animation system variables
    private Vector2 movement;
    private Vector2 previousPosition;
    private string LoadedSpriteSheetName;
    private Dictionary<string, Sprite> spriteSheet;

    // Bubble system variables  
    public GameObject currentBubble { get; private set; } // Public access for interaction system
    private Image bubbleImage;
    private bool bubbleVisible = false;

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
    public NPCPatrolState PatrolState { get; private set; }
    public NPCGoHomeState GoHomeState { get; private set; }

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
        PatrolState = new NPCPatrolState(this, StateMachine);
        GoHomeState = new NPCGoHomeState(this, StateMachine);

        previousPosition = transform.position;
        LoadSpriteSheet();
        InitializeBubbleSystem();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize animation
        if (animator != null)
        {
            animator.SetFloat("speed", 0);
            animator.SetInteger("orientation", 4); // Default facing down
        }

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

        // Start with appropriate state based on current activity
        NPCBehavior currentActivity = GetCurrentActivity();
        if (currentActivity == NPCBehavior.Walk)
        {
            StateMachine.Initialize(PatrolState);
        }
        else if (ShouldGoHome())
        {
            StateMachine.Initialize(GoHomeState);
        }
        else
        {
            StateMachine.Initialize(GetStateForTimeOfDay(currentTimeOfDay));
        }
    }

    private void Update()
    {
        if (StateMachine?.CurrentNPCState != null)
        {
            StateMachine.CurrentNPCState.FrameUpdate();
        }

        CheckPlayerProximity();
        UpdateBubblePosition();

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

        // Calculate movement for animation
        movement.x = transform.position.x - previousPosition.x;
        movement.y = transform.position.y - previousPosition.y;
        previousPosition = transform.position;

        UpdateAnimation();
    }

    private void LateUpdate()
    {
        // Check if sprite sheet name changed
        if (LoadedSpriteSheetName != SpriteSheetName)
        {
            LoadSpriteSheet();
        }

        this.spriteRenderer.sprite = this.spriteSheet[this.spriteRenderer.sprite.name];
    }

    #region Animation System
    private void UpdateAnimation()
    {
        if (animator == null) return;

        // Set speed parameter
        float speed = Mathf.Abs(movement.x) + Mathf.Abs(movement.y);
        animator.SetFloat("speed", speed);

        // Set orientation parameter based on movement direction
        if (movement.x > 0.01f)
            animator.SetInteger("orientation", 6); // Right
        else if (movement.x < -0.01f)
            animator.SetInteger("orientation", 2); // Left
        else if (movement.y > 0.01f)
            animator.SetInteger("orientation", 0); // Up
        else if (movement.y < -0.01f)
            animator.SetInteger("orientation", 4); // Down
        // If no movement, keep current orientation
    }

    public void SetAnimationDirection(Vector2 direction)
    {
        if (animator == null) return;

        // Force set direction without movement (for interactions, work, etc.)
        if (direction.x > 0.01f)
            animator.SetInteger("orientation", 6); // Right
        else if (direction.x < -0.01f)
            animator.SetInteger("orientation", 2); // Left
        else if (direction.y > 0.01f)
            animator.SetInteger("orientation", 0); // Up
        else if (direction.y < -0.01f)
            animator.SetInteger("orientation", 4); // Down
    }

    private void LoadSpriteSheet()
    {
        // Load sprites from Resources folder
        string spritesheetfolder = "Characters/";
        string spritesheetfilepath = spritesheetfolder + SpriteSheetName + "/spritesheet";
        var sprites = Resources.LoadAll<Sprite>(spritesheetfilepath);

        if (sprites.Length == 0)
        {
            Debug.LogWarning($"Could not load sprite sheet: {spritesheetfilepath}. Using default.");
            spritesheetfilepath = spritesheetfolder + "chara_01/spritesheet";
            sprites = Resources.LoadAll<Sprite>(spritesheetfilepath);
        }

        if (sprites.Length > 0)
        {
            spriteSheet = new Dictionary<string, Sprite>();
            foreach (var sprite in sprites)
            {
                spriteSheet[sprite.name] = sprite;
            }
        }

        LoadedSpriteSheetName = SpriteSheetName;
    }
    #endregion

    #region Bubble Status System
    private void InitializeBubbleSystem()
    {
        if (bubbleParent == null)
        {
            // Try to find canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                bubbleParent = canvas.transform;
        }

        CreateBubble();
    }

    private void CreateBubble()
    {
        if (bubblePrefab == null || bubbleParent == null) return;

        currentBubble = Instantiate(bubblePrefab, bubbleParent);
        bubbleImage = currentBubble.GetComponent<Image>();

        if (bubbleImage == null)
            bubbleImage = currentBubble.GetComponentInChildren<Image>();

        currentBubble.SetActive(false);
    }

    private void UpdateBubblePosition()
    {
        if (currentBubble == null || bubbleParent == null) return;

        // Convert world position to screen position
        Vector3 worldPos = transform.position + bubbleOffset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // Convert screen position to canvas position
        RectTransform canvasRect = bubbleParent.GetComponent<RectTransform>();
        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, Camera.main, out canvasPos);

        currentBubble.transform.localPosition = canvasPos;
    }

    public void ShowStatusBubble(NPCBehavior behavior)
    {
        if (currentBubble == null || bubbleImage == null) return;

        Sprite bubbleSprite = GetBubbleSpriteForBehavior(behavior);
        if (bubbleSprite != null)
        {
            bubbleImage.sprite = bubbleSprite;
            currentBubble.SetActive(true);
            bubbleVisible = true;
        }
    }

    public void HideStatusBubble()
    {
        if (currentBubble != null)
        {
            currentBubble.SetActive(false);
            bubbleVisible = false;
        }
    }

    private Sprite GetBubbleSpriteForBehavior(NPCBehavior behavior)
    {
        switch (behavior)
        {
            case NPCBehavior.Work:
                return workingBubbleSprite;
            case NPCBehavior.Sleep:
                return sleepingBubbleSprite;
            case NPCBehavior.Idle:
                return idleBubbleSprite;
            case NPCBehavior.Walk:
                return walkingBubbleSprite;
            case NPCBehavior.Flee:
                return fleeingBubbleSprite;
            case NPCBehavior.Interact:
                return null; // Don't show bubble during interaction
            default:
                return idleBubbleSprite;
        }
    }
    #endregion

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

        // Check if we need to change behavior
        if (ShouldGoHome() && StateMachine.CurrentNPCState != GoHomeState)
        {
            StateMachine.ChangeState(GoHomeState);
        }
        else
        {
            // Update activity based on new time
            NPCBehavior newActivity = GetCurrentActivity();
            if (newActivity == NPCBehavior.Walk && StateMachine.CurrentNPCState != PatrolState)
            {
                StateMachine.ChangeState(PatrolState);
            }
            else if (newActivity != NPCBehavior.Walk && StateMachine.CurrentNPCState == PatrolState)
            {
                StateMachine.ChangeState(IdleState);
            }
            else
            {
                // For other state changes, use the original logic
                NPCState newState = GetStateForTimeOfDay(newTimeOfDay);
                if (newState != StateMachine.CurrentNPCState && StateMachine.CurrentNPCState != InteractState)
                {
                    StateMachine.ChangeState(newState);
                }
            }
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

    // New helper methods for simplified system
    public NPCBehavior GetCurrentActivity()
    {
        if (scheduleData == null)
            return NPCBehavior.Idle;

        // Check if we should be active at night
        if (IsNightTime() && !scheduleData.activeAtNight)
        {
            return NPCBehavior.Walk; // Will trigger going home
        }

        // Get activity based on time of day
        if (IsNightTime() && scheduleData.activeAtNight)
        {
            return scheduleData.nightBehavior;
        }
        else
        {
            return scheduleData.dayBehavior;
        }
    }

    public PatrolPoint[] GetCurrentPatrolPoints()
    {
        if (scheduleData == null)
            return null;

        if (IsNightTime() && scheduleData.activeAtNight)
        {
            return scheduleData.nightPatrolPoints;
        }
        else
        {
            return scheduleData.dayPatrolPoints;
        }
    }

    public Vector2 GetCurrentPosition()
    {
        if (scheduleData == null)
            return transform.position;

        if (IsNightTime() && scheduleData.activeAtNight)
        {
            return scheduleData.nightPosition;
        }
        else
        {
            return scheduleData.dayPosition;
        }
    }

    public Vector2 GetHomePosition()
    {
        if (scheduleData == null)
            return transform.position;

        return scheduleData.homePosition;
    }

    public bool ShouldGoHome()
    {
        return IsNightTime() && !scheduleData.activeAtNight;
    }

    private bool IsNightTime()
    {
        if (dayNightCycle == null)
            return false;

        float currentHour = dayNightCycle.CurrentTime;
        return currentHour >= scheduleData.nightStartHour || currentHour < scheduleData.dayStartHour;
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

        // Don't hide bubble during interaction - let interaction system handle it

        OnInteractionStart?.Invoke(this);
        Debug.Log($"Started interaction with {npcName}");
    }

    public void EndInteraction()
    {
        OnInteractionEnd?.Invoke(this);
        Debug.Log($"Ended interaction with {npcName}");

        // Show appropriate bubble after interaction ends
        UpdateBubbleForCurrentState();
    }

    // Method to show conversation bubble during dialogue
    public void ShowConversationBubble(Sprite conversationSprite)
    {
        if (conversationSprite != null)
        {
            ShowStatusBubble(conversationSprite);
        }
    }

    // Overloaded method to show bubble with sprite directly
    public void ShowStatusBubble(Sprite bubbleSprite)
    {
        if (currentBubble == null || bubbleImage == null || bubbleSprite == null) return;

        bubbleImage.sprite = bubbleSprite;
        currentBubble.SetActive(true);
        bubbleVisible = true;
    }

    // Make UpdateBubbleForCurrentState public for interaction system access
    public void UpdateBubbleForCurrentState()
    {
        if (StateMachine.CurrentNPCState == IdleState)
            ShowStatusBubble(NPCBehavior.Idle);
        else if (StateMachine.CurrentNPCState == WorkState)
            ShowStatusBubble(NPCBehavior.Work);
        else if (StateMachine.CurrentNPCState == SleepState)
            ShowStatusBubble(NPCBehavior.Sleep);
        else if (StateMachine.CurrentNPCState == WalkState)
            ShowStatusBubble(NPCBehavior.Walk);
        else if (StateMachine.CurrentNPCState == FleeState)
            ShowStatusBubble(NPCBehavior.Flee);
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

        if (currentBubble != null)
        {
            Destroy(currentBubble);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw interaction range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Draw scheduled positions if available
        if (scheduleData != null)
        {
            // Day position
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(scheduleData.dayPosition, 0.5f);

            // Night position (if active at night)
            if (scheduleData.activeAtNight)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(scheduleData.nightPosition, 0.5f);
            }

            // Home position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(scheduleData.homePosition, 0.3f);

            // Day patrol points
            if (scheduleData.dayPatrolPoints != null && scheduleData.dayPatrolPoints.Length > 0)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < scheduleData.dayPatrolPoints.Length; i++)
                {
                    Gizmos.DrawWireSphere(scheduleData.dayPatrolPoints[i].position, 0.3f);

                    // Draw connections between patrol points
                    if (i < scheduleData.dayPatrolPoints.Length - 1)
                    {
                        Gizmos.DrawLine(scheduleData.dayPatrolPoints[i].position, scheduleData.dayPatrolPoints[i + 1].position);
                    }
                    else
                    {
                        // Connect last to first
                        Gizmos.DrawLine(scheduleData.dayPatrolPoints[i].position, scheduleData.dayPatrolPoints[0].position);
                    }
                }
            }

            // Night patrol points (if active at night)
            if (scheduleData.activeAtNight && scheduleData.nightPatrolPoints != null && scheduleData.nightPatrolPoints.Length > 0)
            {
                Gizmos.color = Color.magenta;
                for (int i = 0; i < scheduleData.nightPatrolPoints.Length; i++)
                {
                    Gizmos.DrawWireSphere(scheduleData.nightPatrolPoints[i].position, 0.25f);

                    if (i < scheduleData.nightPatrolPoints.Length - 1)
                    {
                        Gizmos.DrawLine(scheduleData.nightPatrolPoints[i].position, scheduleData.nightPatrolPoints[i + 1].position);
                    }
                    else
                    {
                        Gizmos.DrawLine(scheduleData.nightPatrolPoints[i].position, scheduleData.nightPatrolPoints[0].position);
                    }
                }
            }
        }

        // Draw bubble position
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position + bubbleOffset, 0.2f);
    }
}