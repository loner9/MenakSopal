using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NPCInteractionSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Button continueButton;
    public Button endButton;

    [Header("Choice System UI")]
    [Tooltip("Container for choice buttons")]
    public Transform choiceContainer;
    [Tooltip("Prefab for choice buttons")]
    public Button choiceButtonPrefab;
    [Tooltip("Adventure Book styled choice button sprite")]
    public Sprite choiceButtonSprite;

    [Header("Adventure Book UI")]
    [Tooltip("Main dialogue frame from your Adventure Book assets (2_0 sprite)")]
    public Image dialogueBoxImage;
    [Tooltip("Canvas for dialogue UI (for animations)")]
    public GameObject dialogueCanvas;

    [Header("Adventure Book Sprites")]
    [Tooltip("Main dialogue frame from 2.png sprite 2_0")]
    public Sprite adventureBookFrame;
    [Tooltip("Button sprites from your UI assets")]
    public Sprite continueButtonSprite;
    public Sprite endButtonSprite;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public LayerMask npcLayerMask = -1;
    public float interactionRange = 2f;

    [Header("Visual Feedback")]
    // public GameObject interactionPrompt;
    // public TextMeshProUGUI promptText;

    [Header("Adventure Book Settings")]
    public float typewriterSpeed = 0.03f;
    public float bookOpenSpeed = 0.5f;
    public bool enableBookAnimations = true;

    [Header("Default Conversation Bubbles")]
    [Tooltip("Default bubble sprite shown when talking to NPCs if no specific bubble is defined.")]
    public Sprite defaultInteractionBubble;
    [Tooltip("Alternative conversation bubbles for different types of interactions.")]
    public Sprite questionBubbleSprite;
    public Sprite exclamationBubbleSprite;
    public Sprite heartBubbleSprite;
    public Sprite angerBubbleSprite;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip bookOpenSound;
    public AudioClip bookCloseSound;
    public AudioClip pageFlipSound;

    [Header("Choice System Audio")]
    [Tooltip("Sound when hovering over choice buttons")]
    public AudioClip choiceHoverSound;
    [Tooltip("Sound when selecting a choice")]
    public AudioClip choiceSelectSound;
    [Tooltip("Sound for important choices")]
    public AudioClip importantChoiceSound;

    private NPC currentNPC;
    private DialogueData currentDialogue;
    private int currentDialogueIndex = 0;
    private bool isInDialogue = false;
    private bool isTyping = false;
    private float typewriterProgress = 0f;
    private Transform player;
    private GameObject playerGO;
    private List<string> gameFlags = new List<string>(); // Simple flag system
    private Sprite originalNPCBubble; // Store original bubble to restore later
    private Coroutine typingCoroutine;
    private Coroutine bookAnimationCoroutine; // Track opening/closing animation
    private DayNightCycle dayNightCycle;

    // Track used non-repeatable dialogue entries per NPC
    private Dictionary<string, HashSet<int>> usedDialogueEntries = new Dictionary<string, HashSet<int>>();
    private Dictionary<string, HashSet<int>> queuedUsedDialogueEntries = new Dictionary<string, HashSet<int>>();

    // Choice response continuation tracking
    private bool hasPendingNavigation = false;
    private int pendingNavigationIndex = -1;
    private bool shouldEndAfterResponse = false;

    // Flag queueing system - flags to add when dialogue ends
    private HashSet<string> queuedFlagsToAdd = new HashSet<string>();

    // Choice system state
    private bool isShowingChoices = false;
    private List<Button> activeChoiceButtons = new List<Button>();
    private DialogueEntry currentDialogueEntry;
    private bool waitingForChoiceResponse = false;

    public bool _IsInDialogue => isInDialogue;

    public bool IsInDialogue()
    {
        return isInDialogue;
    }
    public NPC CurrentDialogueNPC => currentNPC;

    public System.Action<NPC> OnDialogueStart;
    public System.Action<NPC> OnDialogueEnd;

    private static NPCInteractionSystem instance;

    public static NPCInteractionSystem Instance
    {
        get => instance;
    }

    /// <summary>
    /// Check if the system is currently in dialogue with a specific NPC
    /// </summary>
    public bool IsInDialogueWith(NPC npc)
    {
        return isInDialogue && currentNPC == npc;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Transfer any local flags to existing instance if needed
            foreach (string flag in gameFlags)
            {
                if (!instance.gameFlags.Contains(flag))
                    instance.gameFlags.Add(flag);
            }
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Setup buttons
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueDialogue);

        if (endButton != null)
            endButton.onClick.AddListener(OnEndButtonClicked);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneChanged;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene arg0, LoadSceneMode arg1)
    {
        // Find required components
        playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            player = playerGO.transform;

        Debug.Log($"[NPCInteraction] Player found: {player != null}");

        dayNightCycle = FindObjectOfType<DayNightCycle>();
    }

    private void SetupAdventureBookUI()
    {
        // Hide dialogue initially
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // if (interactionPrompt != null)
        //     interactionPrompt.SetActive(false);

        // Apply Adventure Book styling
        // if (dialogueBoxImage != null && adventureBookFrame != null)
        // {
        //     dialogueBoxImage.sprite = adventureBookFrame;
        //     dialogueBoxImage.type = Image.Type.Sliced;
        // }

        // Style buttons if sprites provided
        if (continueButton != null && continueButtonSprite != null)
        {
            continueButton.image.sprite = continueButtonSprite;
        }

        if (endButton != null && endButtonSprite != null)
        {
            endButton.image.sprite = endButtonSprite;
        }

        // Style speaker name text for adventure theme
        if (speakerNameText != null)
        {
            speakerNameText.color = new Color(1f, 0.84f, 0f, 1f); // Gold color
            speakerNameText.fontStyle = FontStyles.Bold;
        }

        // Style dialogue text
        if (dialogueText != null)
        {
            dialogueText.color = new Color(0.18f, 0.11f, 0.08f, 1f); // Dark brown
        }
    }

    private void Update()
    {
        // Don't check for nearby NPCs if we are in dialogue OR currently playing the book animation (opening or closing)
        if (!isInDialogue && bookAnimationCoroutine == null)
        {
            CheckForNearbyNPCs();

            // Add detailed input debugging
            if (ControlFreak2.CF2Input.GetKeyDown(interactKey))
            {
                Debug.Log($"[NPCInteraction] {interactKey} key pressed! currentNPC: {(currentNPC != null ? currentNPC.npcName : "NULL")}");

                if (currentNPC != null)
                {
                    Debug.Log($"[NPCInteraction] Starting dialogue with {currentNPC.npcName}");
                    StartDialogue(currentNPC);
                }
                else
                {
                    Debug.LogWarning("[NPCInteraction] No current NPC to talk to!");
                }
            }

            // Debug current NPC state every few seconds
            if (Time.time % 2f < 0.1f && currentNPC != null) // Every 2 seconds
            {
                Debug.Log($"[NPCInteraction] Current NPC: {currentNPC.npcName}, canInteract: {currentNPC.canInteract}, isInDialogue: {isInDialogue}");
            }
        }
        else if (isInDialogue && bookAnimationCoroutine == null)
        {
            // Skip typing animation
            if (isTyping && (ControlFreak2.CF2Input.GetKeyDown(KeyCode.Space) || ControlFreak2.CF2Input.GetKeyDown(KeyCode.Return) || ControlFreak2.CF2Input.GetKeyDown(interactKey)))
            {
                SkipTypewriter();
            }
            // Continue dialogue (only if not showing choices and not waiting for response)
            else if (!isTyping && !isShowingChoices && !waitingForChoiceResponse && (ControlFreak2.CF2Input.GetKeyDown(KeyCode.Space) || ControlFreak2.CF2Input.GetKeyDown(KeyCode.Return) || ControlFreak2.CF2Input.GetKeyDown(interactKey)))
            {
                ContinueDialogue();
            }
            // End dialogue
            else if (ControlFreak2.CF2Input.GetKeyDown(KeyCode.Escape))
            {
                EndDialogue();
            }
        }
    }

    private void CheckForNearbyNPCs()
    {
        if (player == null) return;

        // Debug.Log($"[NPCInteraction] Checking for NPCs near player at {player.position}, range: {interactionRange}");

        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(player.position, interactionRange, npcLayerMask);
        // Debug.Log($"[NPCInteraction] Found {nearbyColliders.Length} colliders in range");

        NPC nearestNPC = null;
        float nearestDistance = float.MaxValue;

        foreach (var collider in nearbyColliders)
        {
            // Debug.Log($"[NPCInteraction] Checking collider: {collider.name}");

            NPC npc = collider.GetComponent<NPC>();
            if (npc != null)
            {
                // Debug.Log($"[NPCInteraction] Found NPC: {npc.npcName}, canInteract: {npc.canInteract}");

                if (npc.canInteract)
                {
                    float distance = Vector2.Distance(player.position, npc.transform.position);
                    // Debug.Log($"[NPCInteraction] NPC {npc.npcName} distance: {distance}");

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestNPC = npc;
                        // Debug.Log($"[NPCInteraction] {npc.npcName} is now nearest NPC");
                    }
                }
                else
                {
                    // Debug.LogWarning($"[NPCInteraction] NPC {npc.npcName} cannot interact!");
                }
            }
            else
            {
                // Debug.Log($"[NPCInteraction] Collider {collider.name} has no NPC component");
            }
        }

        // Update current NPC and prompt
        if (nearestNPC != currentNPC)
        {
            // Debug.Log($"[NPCInteraction] Updating current NPC from {(currentNPC?.npcName ?? "NULL")} to {(nearestNPC?.npcName ?? "NULL")}");
            currentNPC = nearestNPC;
            UpdateInteractionPrompt();
        }
    }

    private void UpdateInteractionPrompt()
    {
        // if (interactionPrompt != null)
        // {
        //     if (currentNPC != null)
        //     {
        //         interactionPrompt.SetActive(true);
        //         if (promptText != null)
        //         {
        //             promptText.text = $"Press {interactKey} to talk to {currentNPC.npcName}";
        //         }
        //     }
        //     else
        //     {
        //         interactionPrompt.SetActive(false);
        //     }
        // }
    }

    public void StartDialogue(NPC npc)
    {
        DialogueData data = GetDialogueForNPC(npc);
        StartDialogue(npc, data);
    }

    public void StartDialogue(NPC npc, DialogueData dialogueData)
    {
        Debug.Log($"[NPCInteraction] StartDialogue called for: {npc?.npcName ?? "NULL NPC"}");

        // Force complete previous dialogue if it was still in the middle of closing
        if (bookAnimationCoroutine != null)
        {
            StopCoroutine(bookAnimationCoroutine);
            bookAnimationCoroutine = null;
            if (!isInDialogue && currentDialogue != null)
            {
                FinishEndDialogue();
            }
        }

        if (npc == null || isInDialogue)
        {
            Debug.LogWarning($"[NPCInteraction] Cannot start dialogue - NPC: {npc?.npcName ?? "null"}, isInDialogue: {isInDialogue}");
            return;
        }

        if (dialogueData == null)
        {
            Debug.LogWarning($"No dialogue data found or provided for NPC: {npc.npcName}");
            return;
        }

        Debug.Log($"[NPCInteraction] Dialogue data: {dialogueData.npcName}");

        currentNPC = npc;
        currentDialogue = dialogueData;
        currentDialogueIndex = 0;
        isInDialogue = true;

        // Clear any previously queued flags from previous dialogue
        queuedFlagsToAdd.Clear();
        queuedUsedDialogueEntries.Clear();

        // Clear choice response navigation state
        hasPendingNavigation = false;
        pendingNavigationIndex = -1;
        shouldEndAfterResponse = false;

        // Check and complete TalkToNPC objectives
        CheckTalkToNPCObjectives(npc);

        // Pause game time during dialogue
        if (dayNightCycle != null)
        {
            dayNightCycle.PauseTime();
            if (player != null)
            {
                Debug.Log("Disabling player movement");
                PlayerMovements playerMovements = playerGO.GetComponent<PlayerMovements>();
                if (playerMovements != null)
                {
                    playerMovements.enabled = false;
                }

                // Reset player animation to idle
                PlayerAnimation playerAnimation = playerGO.GetComponent<PlayerAnimation>();
                if (playerAnimation != null)
                {
                    playerAnimation.setMovingAnimation(false);
                    playerAnimation.setRunningAnimation(false);
                }
            }
        }


        // Store original bubble state
        originalNPCBubble = GetCurrentNPCBubbleSprite();

        // Hide interaction prompt
        // if (interactionPrompt != null)
        //     interactionPrompt.SetActive(false);

        // Play dialogue start sound
        PlayAudioClip(dialogueData.dialogueStartSound ?? bookOpenSound);

        // Clear any previous dialogue content before showing UI
        ClearDialogueUI();

        // Show dialogue with adventure book animation
        Debug.Log($"[NPCInteraction] Showing dialogue UI - enableBookAnimations: {enableBookAnimations}");

        if (enableBookAnimations)
        {
            bookAnimationCoroutine = StartCoroutine(OpenBookAnimation());
        }
        else
        {
            // Show dialogue immediately
            Debug.Log("[NPCInteraction] Showing dialogue immediately (no animation)");
            if (dialogueCanvas != null)
            {
                dialogueCanvas.SetActive(true);
                Debug.Log($"[NPCInteraction] dialogueCanvas activated: {dialogueCanvas.activeSelf}");
            }
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
                Debug.Log($"[NPCInteraction] dialoguePanel activated: {dialoguePanel.activeSelf}");
            }
            DisplayFirstDialogue();
        }

        // Notify systems
        OnDialogueStart?.Invoke(npc);

        // Force NPC to interaction state if not already (with null checks)
        if (npc.StateMachine != null && npc.InteractionState != null && npc.StateMachine.CurrentNPCState != npc.InteractionState)
        {
            npc.StateMachine.ChangeState(npc.InteractionState);
        }
        else if (npc.StateMachine == null)
        {
            Debug.LogWarning($"[NPCInteraction] NPC {npc.npcName} has no StateMachine component!");
        }
        else if (npc.InteractionState == null)
        {
            Debug.LogWarning($"[NPCInteraction] NPC {npc.npcName} has no InteractionState!");
        }

        Debug.Log($"Started dialogue with {npc.npcName}");
    }

    private IEnumerator OpenBookAnimation()
    {
        // Show canvas
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);

        // Show panel
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // Animate book opening
        if (dialogueBoxImage != null)
        {
            Transform bookTransform = dialogueBoxImage.transform;

            // Start closed
            bookTransform.localScale = new Vector3(0f, 1f, 1f);
            bookTransform.localRotation = Quaternion.Euler(0, 0, -5f);

            // Animate to open
            float elapsedTime = 0f;
            Vector3 targetScale = Vector3.one;
            Quaternion targetRotation = Quaternion.identity;

            while (elapsedTime < bookOpenSpeed)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / bookOpenSpeed;

                // Ease out animation
                float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

                bookTransform.localScale = Vector3.Lerp(new Vector3(0f, 1f, 1f), targetScale, easedProgress);
                bookTransform.localRotation = Quaternion.Lerp(Quaternion.Euler(0, 0, -5f), targetRotation, easedProgress);

                yield return null;
            }

            bookTransform.localScale = targetScale;
            bookTransform.localRotation = targetRotation;
        }

        // Display first dialogue
        yield return new WaitForSeconds(0.2f);
        
        if (isInDialogue && currentDialogue != null)
        {
            DisplayFirstDialogue();
        }
        else
        {
            Debug.Log("[NPCInteraction] Dialogue ended before book finished opening.");
        }
        
        bookAnimationCoroutine = null;
    }

    private void DisplayFirstDialogue()
    {
        if (!isInDialogue || currentDialogue == null)
        {
            Debug.LogWarning("[NPCInteraction] DisplayFirstDialogue called but no active dialogue found.");
            return;
        }

        TimeOfDay currentTime = dayNightCycle != null ? dayNightCycle.CurrentTimeOfDay : TimeOfDay.Day;
        HashSet<int> usedEntries = GetUsedDialogueEntries(currentDialogue.npcName);

        // First priority: Check for available main dialogues
        DialogueEntry[] availableDialogues = currentDialogue.GetAvailableDialogues(currentDialogue.dialogueEntries, currentTime, gameFlags, usedEntries);

        if (availableDialogues.Length > 0)
        {
            // Main dialogue available - show it (highest priority)
            DisplayDialogue(availableDialogues[0]);
        }
        else
        {
            // No main dialogue available - fallback to greetings/farewells
            DialogueEntry[] availableGreetings = currentDialogue.GetAvailableDialogues(currentDialogue.greetings, currentTime, gameFlags, usedEntries);
            if (availableGreetings.Length > 0)
            {
                DisplayDialogue(availableGreetings[0]);
            }
            else
            {
                // No main dialogue or greeting - check for farewell
                DialogueEntry[] availableFarewells = currentDialogue.GetAvailableDialogues(currentDialogue.farewells, currentTime, gameFlags, usedEntries);
                if (availableFarewells.Length > 0)
                {
                    DisplayDialogue(availableFarewells[0]);
                }
                else
                {
                    // No dialogue available at all
                    Debug.Log($"[NPCInteraction] No available dialogue entries for {currentDialogue.npcName}");
                    EndDialogue();
                    return;
                }
            }
        }
    }

    public void ContinueDialogue()
    {
        if (!isInDialogue || currentDialogue == null || isTyping) return;

        // Play page flip sound
        PlayAudioClip(pageFlipSound);

        // Handle pending navigation from choice response
        if (hasPendingNavigation)
        {
            hasPendingNavigation = false;
            NavigateToDialogueEntry(pendingNavigationIndex);
            pendingNavigationIndex = -1;
            return;
        }

        // Handle end after choice response
        if (shouldEndAfterResponse)
        {
            shouldEndAfterResponse = false;
            EndDialogue();
            return;
        }

        currentDialogueIndex++;

        TimeOfDay currentTime = dayNightCycle != null ? dayNightCycle.CurrentTimeOfDay : TimeOfDay.Day;
        HashSet<int> usedEntries = GetUsedDialogueEntries(currentDialogue.npcName);

        // Get available dialogues for current time
        DialogueEntry[] availableDialogues = currentDialogue.GetAvailableDialogues(currentDialogue.dialogueEntries, currentTime, gameFlags, usedEntries);

        if (currentDialogueIndex < availableDialogues.Length)
        {
            DisplayDialogue(availableDialogues[currentDialogueIndex]);
        }
        else
        {
            // Check for farewell
            DialogueEntry[] availableFarewells = currentDialogue.GetAvailableDialogues(currentDialogue.farewells, currentTime, gameFlags, usedEntries);
            if (availableFarewells.Length > 0)
            {
                DisplayDialogue(availableFarewells[0]);
                // Farewell shown, dialogue will remain open for manual close
            }
            else
            {
                EndDialogue();
            }
        }
    }

    /// <summary>
    /// Start a dialogue using DialogueData directly (for triggers that don't need physical NPC)
    /// </summary>
    public void StartForcedDialogue(DialogueData dialogueData, int specificEntryIndex = -1)
    {
        Debug.Log($"[NPCInteraction] StartForcedDialogue called for: {dialogueData?.npcName ?? "NULL DialogueData"}");

        // Force complete previous dialogue if it was still in the middle of closing
        if (bookAnimationCoroutine != null)
        {
            StopCoroutine(bookAnimationCoroutine);
            bookAnimationCoroutine = null;
            if (!isInDialogue && currentDialogue != null)
            {
                FinishEndDialogue();
            }
        }

        if (dialogueData == null || isInDialogue)
        {
            Debug.LogWarning($"[NPCInteraction] Cannot start forced dialogue - DialogueData: {(dialogueData != null ? "assigned" : "null")}, isInDialogue: {isInDialogue}");
            return;
        }

        // Set dummy NPC for forced dialogue
        currentNPC = null; // No physical NPC
        currentDialogue = dialogueData;
        currentDialogueIndex = specificEntryIndex >= 0 ? specificEntryIndex : 0;
        isInDialogue = true;

        // Clear any previously queued flags from previous dialogue
        queuedFlagsToAdd.Clear();
        queuedUsedDialogueEntries.Clear();

        // Clear choice response navigation state
        hasPendingNavigation = false;
        pendingNavigationIndex = -1;
        shouldEndAfterResponse = false;

        // Pause game time during dialogue
        if (dayNightCycle != null)
        {
            dayNightCycle.PauseTime();
            if (player != null)
            {
                Debug.Log("Disabling player movement for forced dialogue");
                PlayerMovements playerMovements = playerGO.GetComponent<PlayerMovements>();
                if (playerMovements != null)
                {
                    playerMovements.enabled = false;
                }

                // Reset player animation to idle
                PlayerAnimation playerAnimation = playerGO.GetComponent<PlayerAnimation>();
                if (playerAnimation != null)
                {
                    playerAnimation.setMovingAnimation(false);
                    playerAnimation.setRunningAnimation(false);
                    // playerAnimation.setMovingAnimation(0f, 0f); // Reset direction to 0,0
                    Debug.Log("Reset player animation to idle for forced dialogue");
                }
            }
        }

        // Play dialogue start sound
        PlayAudioClip(dialogueData.dialogueStartSound ?? bookOpenSound);

        // Clear any previous dialogue content before showing UI
        ClearDialogueUI();

        // Show dialogue with adventure book animation
        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(true);
            Debug.Log($"[NPCInteraction] dialogueCanvas activated: {dialogueCanvas.activeSelf}");
        }

        if (dialoguePanel != null)
        {

            dialoguePanel.SetActive(true);
            Debug.Log($"[NPCInteraction] dialoguePanel activated: {dialoguePanel.activeSelf}");
        }

        // Show first dialogue or specific entry
        if (specificEntryIndex >= 0)
        {
            ShowSpecificDialogueEntry(specificEntryIndex);
        }
        else
        {
            DisplayFirstDialogue();
        }
    }

    private void ShowSpecificDialogueEntry(int entryIndex)
    {
        if (currentDialogue == null || currentDialogue.dialogueEntries == null ||
            entryIndex >= currentDialogue.dialogueEntries.Length)
        {
            Debug.LogWarning($"[NPCInteraction] Invalid dialogue entry index: {entryIndex}");
            DisplayFirstDialogue();
            return;
        }

        DialogueEntry entry = currentDialogue.dialogueEntries[entryIndex];
        DisplayDialogue(entry);
    }

    /// <summary>
    /// Handle end button clicks - first stops typewriter, then ends dialogue
    /// </summary>
    private void OnEndButtonClicked()
    {
        if (!isInDialogue) return;

        // If typewriter is active, stop it first
        if (isTyping)
        {
            SkipTypewriter();
            Debug.Log("[NPCInteraction] End button: Stopped typewriter effect");
        }
        else
        {
            // If typewriter is not active, end dialogue
            EndDialogue();
            Debug.Log("[NPCInteraction] End button: Ending dialogue");
        }
    }

    public void EndDialogue()
    {
        if (!isInDialogue) return;

        isInDialogue = false;
        currentDialogueIndex = 0;

        // Stop any typing animation
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;
        }

        // Stop any ongoing book animation (opening or closing)
        if (bookAnimationCoroutine != null)
        {
            StopCoroutine(bookAnimationCoroutine);
            bookAnimationCoroutine = null;
        }

        // Clear choice system state
        ClearChoiceButtons();
        isShowingChoices = false;
        waitingForChoiceResponse = false;
        currentDialogueEntry = null;

        // Play book close sound
        PlayAudioClip(bookCloseSound);

        // Adventure book closing animation
        if (enableBookAnimations)
        {
            bookAnimationCoroutine = StartCoroutine(CloseBookAnimation());
        }
        else
        {
            // Hide dialogue immediately
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
            if (dialogueCanvas != null)
                dialogueCanvas.SetActive(false);

            FinishEndDialogue();
        }
    }

    private IEnumerator CloseBookAnimation()
    {
        if (dialogueBoxImage != null)
        {
            Transform bookTransform = dialogueBoxImage.transform;

            float elapsedTime = 0f;
            Vector3 startScale = Vector3.one;
            Quaternion startRotation = Quaternion.identity;

            while (elapsedTime < bookOpenSpeed * 0.7f) // Faster close
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / (bookOpenSpeed * 0.7f);

                bookTransform.localScale = Vector3.Lerp(startScale, new Vector3(0f, 1f, 1f), progress);
                bookTransform.localRotation = Quaternion.Lerp(startRotation, Quaternion.Euler(0, 0, 5f), progress);

                yield return null;
            }
        }

        bookAnimationCoroutine = null;

        // Hide UI
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        FinishEndDialogue();
    }

    private void FinishEndDialogue()
    {
        // Process all queued flags now that dialogue has ended
        ProcessQueuedFlags();

        // Process all queued used dialogue entries now that dialogue has ended
        ProcessQueuedUsedDialogueEntries();

        // Capture the NPC we were just talking to to ensure we restore THE CORRECT NPC's bubble
        // even if Update() somehow managed to change currentNPC (though we added guards now)
        NPC npcToRestore = currentNPC;

        // Resume game time after dialogue
        if (dayNightCycle != null)
        {
            dayNightCycle.ResumeTime();
            if (playerGO != null)
            {
                PlayerMovements playerMovements = playerGO.GetComponent<PlayerMovements>();
                if (playerMovements != null)
                {
                    playerMovements.enabled = true;
                }
            }
        }

        // Restore original NPC bubble
        if (npcToRestore != null)
        {
            npcToRestore.UpdateBubbleForCurrentState();
        }

        // Show interaction prompt again if still near NPC
        UpdateInteractionPrompt();

        // Notify systems
        OnDialogueEnd?.Invoke(npcToRestore);

        Debug.Log($"Ended dialogue with {(npcToRestore != null ? npcToRestore.npcName : "unknown NPC")}");

        // Only clear currentNPC if it's still the one we were talking to
        if (currentNPC == npcToRestore)
        {
            currentNPC = null;
        }

        currentDialogue = null;
    }

    private void ProcessQueuedFlags()
    {
        if (queuedFlagsToAdd.Count == 0) return;

        Debug.Log($"[NPCInteraction] Processing {queuedFlagsToAdd.Count} queued flags after dialogue ended");

        // Add all queued flags
        foreach (string flag in queuedFlagsToAdd)
        {
            AddGameFlag(flag);
            Debug.Log($"[NPCInteraction] Added queued flag: {flag}");
        }

        // Clear the queue
        queuedFlagsToAdd.Clear();
    }

    private void DisplayDialogue(DialogueEntry entry)
    {
        if (entry == null) return;

        currentDialogueEntry = entry;

        // Process dialogue entry consequences (flags, etc.)
        ProcessDialogueEntryConsequences(entry);

        // Clear any existing choices
        ClearChoiceButtons();
        isShowingChoices = false;
        waitingForChoiceResponse = false;

        // Update NPC bubble based on dialogue entry
        UpdateNPCConversationBubble(entry);

        // Update UI
        if (speakerNameText != null)
            speakerNameText.text = entry.speakerName.ToUpper();

        if (dialogueText != null)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypewriterEffect(entry.dialogueText, entry.pauseAfterDialogue));
        }

        // Update button visibility
        UpdateDialogueButtons();
    }

    private IEnumerator TypewriterEffect(string text, float pauseAfter = 0f)
    {
        isTyping = true;
        typewriterProgress = 0f;
        bool buttonsUpdated = false;

        dialogueText.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text += text[i];
            typewriterProgress = (float)(i + 1) / text.Length;

            // Show buttons if we reached 40%
            if (!buttonsUpdated && typewriterProgress >= 0.4f)
            {
                buttonsUpdated = true;
                UpdateDialogueButtons();
            }

            // Play typewriter sound for certain characters
            if (currentDialogue != null && currentDialogue.typewriterSound != null)
            {
                if (text[i] != ' ' && Random.Range(0f, 1f) > 0.7f) // Random typing sounds
                {
                    PlayAudioClip(currentDialogue.typewriterSound);
                }
            }

            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        typewriterProgress = 1f;
        UpdateDialogueButtons();

        // Optional pause after dialogue
        if (pauseAfter > 0f)
        {
            yield return new WaitForSeconds(pauseAfter);
        }

        // Check if this dialogue entry has choices
        if (currentDialogueEntry != null && currentDialogueEntry.hasChoices)
        {
            yield return new WaitForSeconds(0.5f); // Brief pause before showing choices
            ShowChoices(currentDialogueEntry);
        }
    }

    private void SkipTypewriter()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;
            typewriterProgress = 1f;

            // Show full text immediately
            if (currentDialogueEntry != null)
            {
                dialogueText.text = currentDialogueEntry.dialogueText;

                // If this dialogue has choices, show them after skipping
                if (currentDialogueEntry.hasChoices && !isShowingChoices)
                {
                    ShowChoices(currentDialogueEntry);
                }
            }

            UpdateDialogueButtons();
        }
    }

    private void UpdateDialogueButtons()
    {
        if (currentDialogue == null) return;

        TimeOfDay currentTime = dayNightCycle != null ? dayNightCycle.CurrentTimeOfDay : TimeOfDay.Day;
        HashSet<int> usedEntries = GetUsedDialogueEntries(currentDialogue.npcName);
        DialogueEntry[] availableDialogues = currentDialogue.GetAvailableDialogues(currentDialogue.dialogueEntries, currentTime, gameFlags, usedEntries);
        bool hasMoreDialogue = currentDialogueIndex < availableDialogues.Length - 1;

        // Check if we have pending navigation from choice response
        bool hasPendingContinuation = hasPendingNavigation || shouldEndAfterResponse;

        // Hide buttons if showing choices or waiting for choice response
        bool shouldHideButtons = isShowingChoices || waitingForChoiceResponse ||
                               (currentDialogueEntry != null && currentDialogueEntry.hasChoices);

        // NEW: Only show buttons when 40% of the content message is shown
        bool isProgressEnough = !isTyping || typewriterProgress >= 0.4f;

        if (continueButton != null)
        {
            // Show continue button if there's more dialogue OR pending navigation
            bool showContinue = (hasMoreDialogue || hasPendingContinuation) && !shouldHideButtons && isProgressEnough;
            continueButton.gameObject.SetActive(showContinue);
        }

        if (endButton != null)
        {
            // Show end button only if no more dialogue AND no pending navigation
            bool showEnd = !hasMoreDialogue && !hasPendingContinuation && !shouldHideButtons && isProgressEnough;
            endButton.gameObject.SetActive(showEnd);
        }
    }

    public DialogueData GetDialogueForNPC(NPC npc)
    {
        // Priority 1: Check if NPC has DialogueData component attached
        if (npc.dialogueData != null)
        {
            return npc.dialogueData;
        }

        // Priority 2: Try loading from Resources folder
        DialogueData dialogueData = Resources.Load<DialogueData>($"Dialogues/{npc.npcName}");
        if (dialogueData != null)
        {
            return dialogueData;
        }

        // Priority 3: Create from NPCScheduleData if available
        if (npc.scheduleData != null && npc.scheduleData.dialogues != null && npc.scheduleData.dialogues.Length > 0)
        {
            // Create a temporary DialogueData from the schedule
            DialogueData tempDialogue = ScriptableObject.CreateInstance<DialogueData>();
            tempDialogue.npcName = npc.npcName;

            // Convert string array to DialogueEntry array
            List<DialogueEntry> entries = new List<DialogueEntry>();
            foreach (string dialogue in npc.scheduleData.dialogues)
            {
                DialogueEntry entry = new DialogueEntry
                {
                    speakerName = npc.npcName,
                    dialogueText = dialogue,
                    availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Night, TimeOfDay.Sunrise, TimeOfDay.Sunset },
                    isRepeatable = true,
                    conversationBubbleSprite = null // Will use default
                };
                entries.Add(entry);
            }

            tempDialogue.dialogueEntries = entries.ToArray();
            tempDialogue.defaultConversationBubble = defaultInteractionBubble;
            return tempDialogue;
        }

        return null;
    }

    private void UpdateNPCConversationBubble(DialogueEntry entry)
    {
        if (currentNPC == null) return;

        Sprite bubbleToShow = null;

        // Priority order: dialogue-specific > dialogue data default > system default
        if (entry.conversationBubbleSprite != null)
        {
            bubbleToShow = entry.conversationBubbleSprite;
        }
        else if (currentDialogue.defaultConversationBubble != null)
        {
            bubbleToShow = currentDialogue.defaultConversationBubble;
        }
        else if (defaultInteractionBubble != null)
        {
            bubbleToShow = defaultInteractionBubble;
        }

        // Show the conversation bubble
        if (bubbleToShow != null)
        {
            currentNPC.ShowConversationBubble(bubbleToShow);
        }
    }

    private Sprite GetCurrentNPCBubbleSprite()
    {
        if (currentNPC == null || currentNPC.currentBubble == null) return null;

        Image bubbleImage = currentNPC.currentBubble.GetComponent<Image>();
        if (bubbleImage == null)
            bubbleImage = currentNPC.currentBubble.GetComponentInChildren<Image>();

        return bubbleImage?.sprite;
    }


    private void PlayAudioClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    #region Choice System Methods

    private void ShowChoices(DialogueEntry entry)
    {
        if (entry == null || !entry.hasChoices || choiceContainer == null) return;

        // ENSURE CHOICE CONTAINER IS ACTIVE
        choiceContainer.gameObject.SetActive(true);

        TimeOfDay currentTime = dayNightCycle != null ? dayNightCycle.CurrentTimeOfDay : TimeOfDay.Day;
        DialogueChoice[] availableChoices = currentDialogue.GetAvailableChoices(entry, currentTime, gameFlags);

        if (availableChoices.Length == 0)
        {
            // No choices available, continue normally
            Debug.Log("No available choices found for current conditions");
            return;
        }

        isShowingChoices = true;

        Debug.Log($"Showing {availableChoices.Length} choice buttons");

        // Create choice buttons
        for (int i = 0; i < availableChoices.Length; i++)
        {
            CreateChoiceButton(availableChoices[i], i);
        }

        // Update button visibility
        UpdateDialogueButtons();
    }

    private void CreateChoiceButton(DialogueChoice choice, int choiceIndex)
    {
        if (choiceButtonPrefab == null || choiceContainer == null) return;

        Button choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
        activeChoiceButtons.Add(choiceButton);

        // ENSURE BUTTON IS ACTIVE
        choiceButton.gameObject.SetActive(true);

        // Get image component with null check
        Image buttonImage = choiceButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            // Configure button sprite
            if (choiceButtonSprite != null)
            {
                buttonImage.sprite = choiceButtonSprite;
            }

            // ENSURE IMAGE IS VISIBLE - Set default color first
            buttonImage.color = Color.white;

            // Apply choice color tint ONLY if it's not default and has proper alpha
            if (choice.choiceColor != Color.white && choice.choiceColor.a > 0.5f)
            {
                buttonImage.color = choice.choiceColor;
            }
        }

        // Set choice text
        TextMeshProUGUI buttonText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = choice.choiceText;

            // Apply choice styling
            if (choice.isImportantChoice)
            {
                buttonText.fontStyle = FontStyles.Bold;
                buttonText.color = Color.yellow;
            }
        }

        // ENSURE BUTTON IS INTERACTABLE
        choiceButton.interactable = true;

        // Add click listener
        choiceButton.onClick.AddListener(() => OnChoiceSelected(choice));

        // Add hover effects
        var eventTrigger = choiceButton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = choiceButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }

        var hoverEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        hoverEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        hoverEntry.callback.AddListener((data) =>
        {
            PlayAudioClip(choiceHoverSound);
        });
        eventTrigger.triggers.Add(hoverEntry);

        // DEBUG: Log button creation
        Debug.Log($"Created choice button: '{choice.choiceText}' - Active: {choiceButton.gameObject.activeSelf}, Interactable: {choiceButton.interactable}");
    }

    private void OnChoiceSelected(DialogueChoice choice)
    {
        if (choice == null) return;

        // Play selection sound
        AudioClip soundToPlay = choice.isImportantChoice ? importantChoiceSound : choiceSelectSound;
        PlayAudioClip(soundToPlay);

        // Clear choice buttons
        ClearChoiceButtons();
        isShowingChoices = false;
        waitingForChoiceResponse = true;

        // Process choice consequences
        ProcessChoiceConsequences(choice);

        // Handle choice response or navigation
        if (choice.response != null && !string.IsNullOrEmpty(choice.response.responseText))
        {
            StartCoroutine(ShowChoiceResponse(choice));
        }
        else if (choice.targetDialogueIndex >= 0)
        {
            // Navigate to specific dialogue entry
            NavigateToDialogueEntry(choice.targetDialogueIndex);
        }
        else
        {
            // End dialogue
            EndDialogue();
        }
    }

    private IEnumerator ShowChoiceResponse(DialogueChoice choice)
    {
        var response = choice.response;

        // Clear current dialogue entry to prevent it from showing choices after response
        currentDialogueEntry = null;

        // Update NPC bubble if specified
        if (response.conversationBubbleSprite != null)
        {
            currentNPC?.ShowConversationBubble(response.conversationBubbleSprite);
        }

        // Update UI with response
        if (speakerNameText != null)
            speakerNameText.text = response.speakerName.ToUpper();

        if (dialogueText != null)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypewriterEffect(response.responseText, response.pauseAfterResponse));
        }

        // Wait for typing to complete
        while (isTyping)
        {
            yield return null;
        }

        // Wait for additional pause
        if (response.pauseAfterResponse > 0f)
        {
            yield return new WaitForSeconds(response.pauseAfterResponse);
        }

        waitingForChoiceResponse = false;

        // Store navigation info for when user manually continues (BEFORE updating buttons!)
        if (response.continueToNext)
        {
            if (response.nextDialogueIndex >= 0)
            {
                // Store the target dialogue index for manual navigation
                pendingNavigationIndex = response.nextDialogueIndex;
                hasPendingNavigation = true;
            }
            else
            {
                // Will continue to next dialogue in sequence when user presses continue
                hasPendingNavigation = false;
            }
        }
        else
        {
            // Will end dialogue when user presses continue/end
            hasPendingNavigation = false;
            shouldEndAfterResponse = true;
        }

        // Show dialogue buttons for manual continuation (AFTER setting navigation state!)
        UpdateDialogueButtons();
    }

    private void NavigateToDialogueEntry(int index)
    {
        DialogueEntry targetEntry = currentDialogue.GetDialogueEntry(index);
        if (targetEntry != null)
        {
            currentDialogueIndex = index;
            DisplayDialogue(targetEntry);
        }
        else
        {
            EndDialogue();
        }
    }

    private void ProcessChoiceConsequences(DialogueChoice choice)
    {
        // Queue flags to add when dialogue ends
        if (choice.flagsToAdd != null)
        {
            foreach (string flag in choice.flagsToAdd)
            {
                queuedFlagsToAdd.Add(flag);
                Debug.Log($"[NPCInteraction] Queued flag to add when dialogue ends: {flag}");
            }
        }

        // Remove flags immediately (as they may affect dialogue flow)
        if (choice.flagsToRemove != null)
        {
            foreach (string flag in choice.flagsToRemove)
            {
                RemoveGameFlag(flag);
            }
        }

        // Process quest triggers
        ProcessQuestTriggers(choice);
    }

    private void ProcessQuestTriggers(DialogueChoice choice)
    {
        QuestManager questManager = QuestManager.Instance;
        if (questManager == null) return;

        // IMPORTANT: Process completions BEFORE starting new quests
        // This ensures flags from completed quests are available for new quest requirements

        // Complete objective FIRST (may trigger quest completion and add flags)
        if (!string.IsNullOrEmpty(choice.objectiveToComplete))
        {
            string questID = !string.IsNullOrEmpty(choice.questForObjective) ?
                choice.questForObjective : choice.questToStart;

            if (!string.IsNullOrEmpty(questID))
            {
                bool completed = questManager.CompleteObjective(questID, choice.objectiveToComplete);
                if (completed)
                {
                    Debug.Log($"Completed objective '{choice.objectiveToComplete}' in quest '{questID}' from dialogue choice");
                }
            }
        }

        // Complete quest SECOND
        if (!string.IsNullOrEmpty(choice.questToComplete))
        {
            bool completed = questManager.CompleteQuest(choice.questToComplete);
            if (completed)
            {
                Debug.Log($"Completed quest '{choice.questToComplete}' from dialogue choice");
            }
        }

        // Start new quest LAST (after all completions and flag additions)
        if (!string.IsNullOrEmpty(choice.questToStart))
        {
            bool started = questManager.StartQuest(choice.questToStart);
            if (started)
            {
                Debug.Log($"Started quest '{choice.questToStart}' from dialogue choice");
            }
            else
            {
                Debug.LogWarning($"Failed to start quest '{choice.questToStart}' - check if required flags are present");
            }
        }
    }

    private void ClearDialogueUI()
    {
        // Clear previous dialogue content to prevent showing old text
        if (speakerNameText != null)
            speakerNameText.text = "";

        if (dialogueText != null)
            dialogueText.text = "";

        // Hide buttons initially during transition/animation
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);
        if (endButton != null)
            endButton.gameObject.SetActive(false);

        // Clear any existing choices
        ClearChoiceButtons();

        // Stop any ongoing typing animation
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        isShowingChoices = false;
        waitingForChoiceResponse = false;
    }

    private void ProcessDialogueEntryConsequences(DialogueEntry entry)
    {
        if (entry == null) return;

        // Queue this dialogue entry to be marked as used when dialogue finishes if it's not repeatable
        if (!entry.isRepeatable && currentDialogue != null)
        {
            QueueDialogueEntryAsUsed(currentDialogue.npcName, entry);
        }

        // Queue flags to add when dialogue ends
        if (entry.flagsToAdd != null)
        {
            foreach (string flag in entry.flagsToAdd)
            {
                queuedFlagsToAdd.Add(flag);
                Debug.Log($"[NPCInteraction] Queued dialogue entry flag to add when dialogue ends: {flag}");
            }
        }

        // Remove flags immediately (as they may affect dialogue flow)
        if (entry.flagsToRemove != null)
        {
            foreach (string flag in entry.flagsToRemove)
            {
                RemoveGameFlag(flag);
            }
        }
    }

    private void ClearChoiceButtons()
    {
        foreach (Button button in activeChoiceButtons)
        {
            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }
        activeChoiceButtons.Clear();
    }

    #endregion

    #region Used Dialogue Tracking

    /// <summary>
    /// Get the set of used dialogue entry indices for a specific NPC
    /// </summary>
    private HashSet<int> GetUsedDialogueEntries(string npcName)
    {
        if (string.IsNullOrEmpty(npcName)) return new HashSet<int>();

        if (!usedDialogueEntries.ContainsKey(npcName))
        {
            usedDialogueEntries[npcName] = new HashSet<int>();
        }

        return usedDialogueEntries[npcName];
    }

    /// <summary>
    /// Queue a dialogue entry to be marked as used when the current dialogue session ends
    /// </summary>
    private void QueueDialogueEntryAsUsed(string npcName, DialogueEntry entry)
    {
        if (string.IsNullOrEmpty(npcName) || entry == null || currentDialogue == null) return;

        int entryIndex = currentDialogue.GetDialogueEntryIndex(entry);
        if (entryIndex >= 0)
        {
            if (!queuedUsedDialogueEntries.ContainsKey(npcName))
            {
                queuedUsedDialogueEntries[npcName] = new HashSet<int>();
            }
            queuedUsedDialogueEntries[npcName].Add(entryIndex);
            Debug.Log($"[NPCInteraction] Queued dialogue entry {entryIndex} as used for {npcName} (will commit when dialogue ends)");
        }
    }

    /// <summary>
    /// Commit all queued used dialogue entries to persistent tracking
    /// </summary>
    private void ProcessQueuedUsedDialogueEntries()
    {
        if (queuedUsedDialogueEntries.Count == 0) return;

        Debug.Log($"[NPCInteraction] Processing queued used dialogue entries for {queuedUsedDialogueEntries.Count} NPCs");

        foreach (var kvp in queuedUsedDialogueEntries)
        {
            string npcName = kvp.Key;
            HashSet<int> entries = kvp.Value;
            HashSet<int> targetSet = GetUsedDialogueEntries(npcName);

            foreach (int index in entries)
            {
                targetSet.Add(index);
                Debug.Log($"[NPCInteraction] Marked dialogue entry {index} as used for {npcName} (non-repeatable)");
            }
        }

        queuedUsedDialogueEntries.Clear();
    }

    /// <summary>
    /// Mark a dialogue entry as used for a specific NPC immediately
    /// </summary>
    private void MarkDialogueEntryAsUsed(string npcName, DialogueEntry entry)
    {
        if (string.IsNullOrEmpty(npcName) || entry == null || currentDialogue == null) return;

        // Find the index of this entry in the current dialogue
        int entryIndex = currentDialogue.GetDialogueEntryIndex(entry);
        if (entryIndex >= 0)
        {
            HashSet<int> usedEntries = GetUsedDialogueEntries(npcName);
            usedEntries.Add(entryIndex);
            Debug.Log($"[NPCInteraction] Marked dialogue entry {entryIndex} as used for {npcName} (non-repeatable)");
        }
    }

    /// <summary>
    /// Reset used dialogue entries for a specific NPC (useful for testing or story resets)
    /// </summary>
    public void ResetUsedDialogueEntries(string npcName)
    {
        if (!string.IsNullOrEmpty(npcName) && usedDialogueEntries.ContainsKey(npcName))
        {
            usedDialogueEntries[npcName].Clear();
            Debug.Log($"[NPCInteraction] Reset used dialogue entries for {npcName}");
        }
    }

    /// <summary>
    /// Reset all used dialogue entries (useful for new game or story resets)
    /// </summary>
    public void ResetAllUsedDialogueEntries()
    {
        usedDialogueEntries.Clear();
        Debug.Log("[NPCInteraction] Reset all used dialogue entries");
    }

    #endregion

    // Public methods for external systems
    // Syncs with FlagManager when available for centralized flag management
    public void AddGameFlag(string flag)
    {
        if (!gameFlags.Contains(flag))
        {
            gameFlags.Add(flag);

            // Sync with centralized FlagManager
            if (FlagManager.Instance != null)
            {
                FlagManager.Instance.AddFlag(flag);
            }

            // Notify flag monitor system for automatic reactions
            FlagMonitorSystem.NotifyFlagAdded(flag);
        }
    }

    /// <summary>
    /// Removes all flags that were added after the specified target flag.
    /// This is used to reset the story state to a specific checkpoint.
    /// </summary>
    /// <param name="targetFlag">The flag to roll back to. This flag and all before it will be kept.</param>
    public void RollbackFlags(string targetFlag)
    {
        if (!gameFlags.Contains(targetFlag))
        {
            Debug.LogWarning($"[NPCInteractionSystem] Cannot rollback to flag '{targetFlag}' because it doesn't exist!");
            return;
        }

        int targetIndex = gameFlags.IndexOf(targetFlag);
        int flagsToRemoveCount = gameFlags.Count - (targetIndex + 1);

        if (flagsToRemoveCount > 0)
        {
            Debug.Log($"[NPCInteractionSystem] Rolling back flags to '{targetFlag}'. Removing {flagsToRemoveCount} flags.");
            
            // Remove flags from the end backwards
            for (int i = gameFlags.Count - 1; i > targetIndex; i--)
            {
                string flagToRemove = gameFlags[i];
                
                // Sync with centralized FlagManager if available
                if (FlagManager.Instance != null)
                {
                    FlagManager.Instance.RemoveFlag(flagToRemove);
                }
                
                // Notify flag monitor system
                FlagMonitorSystem.NotifyFlagRemoved(flagToRemove);
                
                gameFlags.RemoveAt(i);
            }
        }
    }

    public void RemoveGameFlag(string flag)
    {
        if (gameFlags.Remove(flag))
        {
            // Sync with centralized FlagManager
            if (FlagManager.Instance != null)
            {
                FlagManager.Instance.RemoveFlag(flag);
            }

            // Notify flag monitor system for automatic reactions
            FlagMonitorSystem.NotifyFlagRemoved(flag);
        }
    }

    public bool HasGameFlag(string flag)
    {
        // Check FlagManager first if available, fallback to local
        if (FlagManager.Instance != null)
        {
            return FlagManager.Instance.HasFlag(flag);
        }
        return gameFlags.Contains(flag);
    }

    public void SetGameFlags(List<string> flags)
    {
        gameFlags = new List<string>(flags);

        // Sync with FlagManager
        if (FlagManager.Instance != null)
        {
            FlagManager.Instance.ClearAllFlags();
            FlagManager.Instance.AddFlags(flags);
        }
    }

    public List<string> GetGameFlags()
    {
        // Return from FlagManager if available for consistency
        if (FlagManager.Instance != null)
        {
            return FlagManager.Instance.GetAllFlags();
        }
        return new List<string>(gameFlags);
    }

    /// <summary>
    /// Sync flags from FlagManager to local storage.
    /// Call this during initialization if FlagManager loaded save data first.
    /// </summary>
    public void SyncFromFlagManager()
    {
        if (FlagManager.Instance != null)
        {
            gameFlags = FlagManager.Instance.GetAllFlags();
            Debug.Log($"[NPCInteractionSystem] Synced {gameFlags.Count} flags from FlagManager");
        }
    }

    // Methods for external control of conversation bubbles
    public void ShowConversationBubble(Sprite bubbleSprite)
    {
        if (currentNPC != null && bubbleSprite != null)
        {
            currentNPC.ShowConversationBubble(bubbleSprite);
        }
    }

    public void ShowQuestionBubble()
    {
        ShowConversationBubble(questionBubbleSprite);
    }

    public void ShowExclamationBubble()
    {
        ShowConversationBubble(exclamationBubbleSprite);
    }

    #region TalkToNPC Objective Integration

    /// <summary>
    /// Check and automatically complete TalkToNPC objectives when talking to an NPC
    /// </summary>
    /// <param name="npc">The NPC being talked to</param>
    private void CheckTalkToNPCObjectives(NPC npc)
    {
        if (npc == null) return;

        var questManager = QuestManager.Instance;
        if (questManager == null) return;

        // Get NPC ID from NPCManager spawn data
        string npcID = GetNPCIDFromSpawnData(npc);
        if (string.IsNullOrEmpty(npcID))
        {
            // Fallback to npcName if no ID found
            npcID = npc.npcName?.ToLower().Replace(" ", "_");
        }

        if (string.IsNullOrEmpty(npcID)) return;

        // Get all active quests using the correct property
        var activeQuests = questManager.ActiveQuests;
        if (activeQuests == null) return;

        foreach (var quest in activeQuests)
        {
            if (quest.objectives == null) continue;

            foreach (var objective in quest.objectives)
            {
                // Check if this is a TalkToNPC objective that matches this NPC
                if (objective.type == ObjectiveType.TalkToNPC &&
                    !objective.isCompleted &&
                    !string.IsNullOrEmpty(objective.targetNPC))
                {
                    // Check if the target NPC matches (case-insensitive)
                    if (string.Equals(objective.targetNPC, npcID, System.StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(objective.targetNPC, npc.npcName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        // Check if required flags are present before completing
                        if (!objective.IsAvailable(GetGameFlags()))
                        {
                            Debug.Log($"[TalkToNPC] Objective '{objective.description}' cannot be completed yet - missing required flags");
                            continue;
                        }

                        // Complete the objective
                        bool completed = questManager.CompleteObjective(quest.questID, objective.objectiveID);

                        if (completed)
                        {
                            Debug.Log($"[TalkToNPC] Auto-completed objective '{objective.description}' by talking to {npc.npcName} (ID: {npcID})");

                            // Show feedback to player
                            ShowObjectiveCompleteMessage(objective.description);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Get NPC ID from NPCManager spawn data
    /// </summary>
    /// <param name="npc">The NPC to get ID for</param>
    /// <returns>NPC ID from spawn data, or null if not found</returns>
    private string GetNPCIDFromSpawnData(NPC npc)
    {
        var npcManager = FindObjectOfType<NPCManager>();
        if (npcManager == null) return null;

        // Find matching spawn data based on prefab or name
        foreach (var spawnData in npcManager.npcSpawnList)
        {
            if (spawnData.npcPrefab != null)
            {
                // Check if this NPC matches the prefab
                if (npc.name.Contains(spawnData.npcPrefab.name) ||
                    npc.npcName == spawnData.npcPrefab.GetComponent<NPC>()?.npcName)
                {
                    return spawnData.npcID;
                }
            }

        }

        return null;
    }

    /// <summary>
    /// Show a message when an objective is completed through NPC interaction
    /// </summary>
    /// <param name="objectiveDescription">Description of the completed objective</param>
    private void ShowObjectiveCompleteMessage(string objectiveDescription)
    {
        // You can customize this to integrate with your UI system
        Debug.Log($"Objective Complete: {objectiveDescription}");

        // If you have a UI notification system, integrate it here
        // Example: NotificationManager.ShowMessage($"Objective Complete: {objectiveDescription}");
    }

    /// <summary>
    /// Get all TalkToNPC objectives for a specific NPC (for debugging/UI purposes)
    /// </summary>
    /// <param name="npcID">ID of the NPC to check</param>
    /// <returns>List of relevant objectives</returns>
    public System.Collections.Generic.List<QuestObjective> GetTalkToNPCObjectivesForNPC(string npcID)
    {
        var objectives = new System.Collections.Generic.List<QuestObjective>();
        var questManager = QuestManager.Instance;

        if (questManager == null || string.IsNullOrEmpty(npcID)) return objectives;

        var activeQuests = questManager.ActiveQuests;
        if (activeQuests == null) return objectives;

        foreach (var quest in activeQuests)
        {
            if (quest.objectives == null) continue;

            foreach (var objective in quest.objectives)
            {
                if (objective.type == ObjectiveType.TalkToNPC &&
                    !objective.isCompleted &&
                    string.Equals(objective.targetNPC, npcID, System.StringComparison.OrdinalIgnoreCase))
                {
                    objectives.Add(objective);
                }
            }
        }

        return objectives;
    }

    #endregion

    public void ShowHeartBubble()
    {
        ShowConversationBubble(heartBubbleSprite);
    }

    public void ShowAngerBubble()
    {
        ShowConversationBubble(angerBubbleSprite);
    }

    // For save/load system
    [System.Serializable]
    public class DialogueSystemSaveData
    {
        public List<string> gameFlags;
        public Dictionary<string, List<int>> usedDialogueEntries;
    }

    public DialogueSystemSaveData GetSaveData()
    {
        // Convert HashSet<int> to List<int> for serialization
        Dictionary<string, List<int>> serializedUsedEntries = new Dictionary<string, List<int>>();
        foreach (var kvp in usedDialogueEntries)
        {
            serializedUsedEntries[kvp.Key] = new List<int>(kvp.Value);
        }

        return new DialogueSystemSaveData
        {
            gameFlags = this.gameFlags,
            usedDialogueEntries = serializedUsedEntries
        };
    }

    public void LoadSaveData(DialogueSystemSaveData data)
    {
        if (data != null)
        {
            gameFlags = data.gameFlags ?? new List<string>();

            // Convert List<int> back to HashSet<int>
            usedDialogueEntries.Clear();
            if (data.usedDialogueEntries != null)
            {
                foreach (var kvp in data.usedDialogueEntries)
                {
                    usedDialogueEntries[kvp.Key] = new HashSet<int>(kvp.Value);
                }
            }
        }
    }
}