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
    [SerializeField] float gridSize = 0.5f;
    [SerializeField] bool searchShortcut = false;
    [SerializeField] bool snapToGrid = false;
    List<Vector2> path;

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
    public NPCMoveState MoveState { get; private set; }
    public NPCInteractionState InteractionState { get; private set; }

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

    // Schedule management
    private ScheduleCommand? pendingScheduleCommand;
    private Vector2 currentIdlePosition;
    private bool shouldIdleWhenReached = true;
    private bool shouldMoveAroundWhenIdle = false;
    private bool shouldDespawnOnReachingDestination = false;

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
        MoveState = new NPCMoveState(this, StateMachine);
        InteractionState = new NPCInteractionState(this, StateMachine);

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

        // Initialize with idle state - NPCManager will send commands as needed
        currentIdlePosition = transform.position;
        StateMachine.Initialize(IdleState);
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

        // Choose direction based on which axis has larger absolute value
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal movement is stronger - face left or right
            if (direction.x > 0.01f)
                animator.SetInteger("orientation", 6); // Right
            else if (direction.x < -0.01f)
                animator.SetInteger("orientation", 2); // Left
        }
        else
        {
            // Vertical movement is stronger or equal - face up or down
            if (direction.y > 0.01f)
                animator.SetInteger("orientation", 0); // Up
            else if (direction.y < -0.01f)
                animator.SetInteger("orientation", 4); // Down
        }
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

    public Dictionary<Vector2, float> GetNeighbourNodes(Vector2 pos)
    {
        Dictionary<Vector2, float> neighbours = new Dictionary<Vector2, float>();
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0) continue;

                Vector2 dir = new Vector2(i, j) * gridSize;
                if (!Physics2D.Linecast(pos, pos + dir, obstacleLayerMask))
                {
                    neighbours.Add(GetClosestNode(pos + dir), dir.magnitude);
                }
            }
        }
        return neighbours;
    }
    
    Vector2 GetClosestNode(Vector2 target)
    {
        return new Vector2(Mathf.Round(target.x / gridSize) * gridSize, Mathf.Round(target.y / gridSize) * gridSize);
    }
    
    List<Vector2> ShortenPath(List<Vector2> path)
    {
        List<Vector2> newPath = new List<Vector2>();

        for (int i = 0; i < path.Count; i++)
        {
            newPath.Add(path[i]);
            for (int j = path.Count - 1; j > i; j--)
            {
                if (!Physics2D.Linecast(path[i], path[j], obstacleLayerMask))
                {
                    i = j;
                    break;
                }
            }
            newPath.Add(path[i]);
        }
        newPath.Add(path[path.Count - 1]);
        return newPath;
    }

    public void GetMoveCommand(Vector2 target)
    {
        Vector2 startPos = (Vector2)transform.position;
        
        Vector2 closestStartNode = GetClosestNode(startPos);
        Vector2 closestTargetNode = GetClosestNode(target);

        if (pathfinder.GenerateAstarPath(closestStartNode, closestTargetNode, out path))
        {
            if (searchShortcut && path.Count > 0)
                pathLeftToGo = ShortenPath(path);
            else
            {
                pathLeftToGo = new List<Vector2>(path);
                if (!snapToGrid) pathLeftToGo.Add(target);
            }
            
            currentDestination = target;
            hasDestination = true;
        }
        else
        {
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

    #region Schedule Command System
    public void ReceiveScheduleCommand(ScheduleCommand command)
    {
        pendingScheduleCommand = command;
    }

    public bool HasNewScheduleCommand()
    {
        return pendingScheduleCommand.HasValue;
    }

    public void ProcessScheduleCommand()
    {
        if (!pendingScheduleCommand.HasValue)
        {
            Debug.Log($"[NPC DEBUG] {npcName}: No pending schedule command to process");
            return;
        }

        var command = pendingScheduleCommand.Value;
        pendingScheduleCommand = null;

        Debug.Log($"[NPC DEBUG] {npcName}: Processing schedule command {command.commandType} to position {command.targetPosition}");

        switch (command.commandType)
        {
            case ScheduleCommandType.Move:
                Debug.Log($"[NPC DEBUG] {npcName}: Executing MOVE command to {command.targetPosition}");
                shouldIdleWhenReached = command.shouldIdleWhenReached;
                canInteract = command.canInteract;
                shouldDespawnOnReachingDestination = false;
                GetMoveCommand(command.targetPosition);
                StateMachine.ChangeState(MoveState);
                break;

            case ScheduleCommandType.Idle:
                Debug.Log($"[NPC DEBUG] {npcName}: Executing IDLE command at {command.targetPosition}");
                currentIdlePosition = command.targetPosition;
                shouldMoveAroundWhenIdle = ShouldMoveAroundWhenIdle();
                shouldDespawnOnReachingDestination = false;
                StateMachine.ChangeState(IdleState);
                break;

            case ScheduleCommandType.GoHome:
                Debug.Log($"[NPC DEBUG] {npcName}: Executing GO HOME command to {command.targetPosition} (will despawn on arrival)");
                shouldIdleWhenReached = command.shouldIdleWhenReached;
                canInteract = command.canInteract;
                shouldDespawnOnReachingDestination = true; // Mark for despawn on arrival
                GetMoveCommand(command.targetPosition);
                StateMachine.ChangeState(MoveState);
                break;

            case ScheduleCommandType.Despawn:
                Debug.Log($"[NPC DEBUG] {npcName}: Executing DESPAWN command");
                RequestDespawn();
                break;
        }
    }

    public Vector2 GetCurrentDestination()
    {
        return currentDestination;
    }

    public bool ShouldIdleWhenReached()
    {
        return shouldIdleWhenReached;
    }

    public bool ShouldMoveAroundWhenIdle()
    {
        if (scheduleData != null)
            return scheduleData.moveAroundWhenIdle;
        return shouldMoveAroundWhenIdle;
    }

    public void SetIdlePosition(Vector2 position)
    {
        currentIdlePosition = position;
    }

    public bool ShouldDespawnAfterReachingDestination()
    {
        return shouldDespawnOnReachingDestination;
    }

    public void NotifyDestinationReached()
    {
        // Notify NPCManager that we've reached our destination
        NPCManager npcManager = FindObjectOfType<NPCManager>();
        if (npcManager != null)
        {
            npcManager.NotifyNPCDestinationReached(this);
        }
    }

    public void RequestDespawn()
    {
        NPCManager npcManager = FindObjectOfType<NPCManager>();
        if (npcManager != null)
        {
            npcManager.RequestNPCDespawn(this);
        }
        else
        {
            // Fallback
            gameObject.SetActive(false);
        }
    }
    #endregion

    #region Day/Night System Integration
    private void OnTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        currentTimeOfDay = newTimeOfDay;
        // Schedule updates are now handled by NPCManager
        // This method is kept for compatibility but NPCs now receive commands from NPCManager
    }

    // Legacy method kept for compatibility
    public NPCState GetStateForTimeOfDay(TimeOfDay timeOfDay)
    {
        // This method is no longer used in the new system
        // All state transitions are now handled via schedule commands from NPCManager
        return IdleState;
    }

    public Vector2 GetScheduledPosition()
    {
        if (scheduleData == null)
            return transform.position;

        // Use the new event-based system
        return scheduleData.GetPositionForTime(dayNightCycle?.CurrentTime ?? 12f);
    }

    // Legacy methods kept for compatibility but simplified
    public Vector2 GetHomePosition()
    {
        if (scheduleData == null)
            return transform.position;

        return scheduleData.GetHomePosition();
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
            if (StateMachine.CurrentNPCState != InteractionState)
            {
                // Only interact if not in important states
                if (StateMachine.CurrentNPCState == IdleState || StateMachine.CurrentNPCState == MoveState)
                {
                    StateMachine.ChangeState(InteractionState);
                }
            }
        }
        else if (!isPlayerInRange && wasInRange)
        {
            // Player left range - return to previous behavior
            if (StateMachine.CurrentNPCState == InteractionState)
            {
                // Check if we have a pending command first
                if (HasNewScheduleCommand())
                {
                    ProcessScheduleCommand();
                }
                else
                {
                    StateMachine.ChangeState(IdleState);
                }
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
        else if (StateMachine.CurrentNPCState == MoveState)
            ShowStatusBubble(NPCBehavior.Walk);
        else if (StateMachine.CurrentNPCState == InteractionState)
            HideStatusBubble(); // Don't show bubble during interaction
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
        // In the new system, simply return to idle and let NPCManager handle scheduling
        StateMachine.ChangeState(IdleState);
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
            // No longer drawing legacy day/night positions since we use schedule events

            // Home position
            Gizmos.color = Color.red;
            Vector2 homePos = scheduleData.GetHomePosition();
            Gizmos.DrawWireSphere(homePos, 0.3f);

            // Draw home object connection if using object reference
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(scheduleData.homeObjectName))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(homePos, homePos + Vector2.up * 0.7f);
                string homeLabel = $"HOME→{scheduleData.homeObjectName}";
                if (!string.IsNullOrEmpty(scheduleData.homeObjectTag))
                    homeLabel += $"\n[{scheduleData.homeObjectTag}]";
                UnityEditor.Handles.Label(homePos + Vector2.up * 0.8f, homeLabel);
            }
            else
            {
                UnityEditor.Handles.Label(homePos + Vector2.up * 0.8f, "HOME");
            }
#endif

            // Draw schedule events
            if (scheduleData.scheduleEvents != null && scheduleData.scheduleEvents.Length > 0)
            {
                for (int i = 0; i < scheduleData.scheduleEvents.Length; i++)
                {
                    var scheduleEvent = scheduleData.scheduleEvents[i];
                    if (scheduleEvent == null) continue;

                    // Color based on time of day
                    if (scheduleEvent.hour >= 6 && scheduleEvent.hour < 12)
                        Gizmos.color = Color.yellow; // Morning
                    else if (scheduleEvent.hour >= 12 && scheduleEvent.hour < 18)
                        Gizmos.color = Color.green; // Afternoon
                    else if (scheduleEvent.hour >= 18 && scheduleEvent.hour < 22)
                        Gizmos.color = new Color32(255, 165, 0, 255); // Orange - Evening
                    else
                        Gizmos.color = Color.blue; // Night

                    // Get smart target position
                    Vector2 targetPos = scheduleEvent.GetTargetPosition();

                    // Draw position for this event
                    Gizmos.DrawWireSphere(targetPos, 0.3f);

                    // Draw object reference indicator
#if UNITY_EDITOR
                    if (!string.IsNullOrEmpty(scheduleEvent.targetObjectName))
                    {
                        // Draw connection line to show it's using an object reference
                        Gizmos.color = Color.white;
                        Gizmos.DrawLine(targetPos, targetPos + Vector2.up * 0.4f);
                    }
#endif

                    // Draw connections between events (chronological order)
                    if (i < scheduleData.scheduleEvents.Length - 1)
                    {
                        var nextEvent = scheduleData.scheduleEvents[i + 1];
                        if (nextEvent != null)
                        {
                            Gizmos.color = Color.white;
                            Gizmos.DrawLine(targetPos, nextEvent.GetTargetPosition());
                        }
                    }

                    // Draw hour label (in editor only)
#if UNITY_EDITOR
                    string label = $"{scheduleEvent.hour}:00\n{scheduleEvent.behavior}";
                    if (scheduleEvent.shouldDespawn)
                        label += "\n[DESPAWN]";
                    if (!string.IsNullOrEmpty(scheduleEvent.targetObjectName))
                    {
                        label += $"\n→{scheduleEvent.targetObjectName}";
                        if (!string.IsNullOrEmpty(scheduleEvent.targetObjectTag))
                            label += $"\n[{scheduleEvent.targetObjectTag}]";
                    }
                    UnityEditor.Handles.Label(targetPos + Vector2.up * 0.5f, label);
#endif
                }
            }
        }

        // Draw bubble position
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position + bubbleOffset, 0.2f);
    }
}