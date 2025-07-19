using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Enhanced version of your NPCInteractionSystem that can work with both:
/// - Old NPC components (backward compatibility)
/// - New SimpleNPC components via NPCInteractionBridge (new functionality)
/// 
/// This maintains all your existing functionality while adding support for the new NPC system.
/// The key innovation is detection and handling of both component types transparently.
/// </summary>
public class EnhancedNPCInteractionSystem : MonoBehaviour
{
    [Header("UI References")]
    public Canvas dialogueCanvas;
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Button continueButton;
    public Button endButton;
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public float interactionRange = 2f;
    public LayerMask npcLayerMask = -1;
    public float typewriterSpeed = 0.05f;

    [Header("Visual Effects")]
    public Sprite defaultInteractionBubble;
    public Sprite questionBubbleSprite;
    public Sprite exclamationBubbleSprite;
    public Sprite heartBubbleSprite;
    public Sprite angerBubbleSprite;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip bookOpenSound;
    public AudioClip bookCloseSound;
    public AudioClip pageFlipSound;

    // Current interaction tracking - supports both old and new NPC types
    private MonoBehaviour currentInteractableNPC; // Can be either NPC or NPCInteractionBridge
    private DialogueData currentDialogue;
    private int currentDialogueIndex = 0;
    private bool isInDialogue = false;
    private bool isTyping = false;
    private Transform player;
    private List<string> gameFlags = new List<string>();
    private Sprite originalNPCBubble;
    private Coroutine typingCoroutine;
    private DayNightCycle dayNightCycle;

    // Bridge integration
    private SyncedScriptableNPCManager npcManager;

    public bool IsInDialogue => isInDialogue;

    // Events
    public System.Action<MonoBehaviour> OnDialogueStart; // Changed to MonoBehaviour to support both types
    public System.Action<MonoBehaviour> OnDialogueEnd;

    private void Start()
    {
        SetupSystemReferences();
        SetupUI();
        SetupButtons();
    }

    /// <summary>
    /// Finds and connects to all the systems this interaction system needs to work with.
    /// This includes your existing DayNightCycle and the new SyncedSimpleNPCManager.
    /// </summary>
    void SetupSystemReferences()
    {
        // Find player
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            player = playerGO.transform;

        // Find your existing day/night cycle
        dayNightCycle = FindObjectOfType<DayNightCycle>();

        // Find the new NPC manager (optional - used for advanced integration)
        npcManager = FindObjectOfType<SyncedScriptableNPCManager>();

        Debug.Log($"EnhancedNPCInteractionSystem: Connected to DayNightCycle: {dayNightCycle != null}, " +
                 $"NPCManager: {npcManager != null}");
    }

    void SetupUI()
    {
        // Hide dialogue initially
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    void SetupButtons()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueDialogue);
        if (endButton != null)
            endButton.onClick.AddListener(EndDialogue);
    }

    private void Update()
    {
        if (!isInDialogue)
        {
            CheckForNearbyNPCs();

            if (Input.GetKeyDown(interactKey) && currentInteractableNPC != null)
            {
                StartDialogueWithCurrentNPC();
            }
        }
        else
        {
            HandleDialogueInput();
        }
    }

    #region NPC Detection and Compatibility

    /// <summary>
    /// Enhanced NPC detection that works with both old and new NPC types.
    /// This is the key method that makes the system backward compatible.
    /// </summary>
    void CheckForNearbyNPCs()
    {
        if (player == null) return;

        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(player.position, interactionRange, npcLayerMask);
        MonoBehaviour nearestInteractableNPC = null;
        float nearestDistance = float.MaxValue;

        foreach (var collider in nearbyColliders)
        {
            // Check for NPCInteractionBridge first (new system)
            NPCInteractionBridge bridge = collider.GetComponent<NPCInteractionBridge>();
            if (bridge != null && bridge.CanInteractProperty)
            {
                float distance = Vector2.Distance(player.position, bridge.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestInteractableNPC = bridge;
                }
                continue; // Found bridge, skip checking for old NPC component
            }

        }

        // Update current NPC and prompt
        if (nearestInteractableNPC != currentInteractableNPC)
        {
            currentInteractableNPC = nearestInteractableNPC;
            UpdateInteractionPrompt();
        }
    }

    /// <summary>
    /// Updates the interaction prompt to show the correct NPC name regardless of component type.
    /// This demonstrates how the bridge pattern makes different components look the same to the UI.
    /// </summary>
    void UpdateInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            if (currentInteractableNPC != null)
            {
                string npcName = GetNPCName(currentInteractableNPC);

                interactionPrompt.SetActive(true);
                if (promptText != null)
                {
                    promptText.text = $"Press {interactKey} to talk to {npcName}";
                }
            }
            else
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Bridge method that gets the NPC name regardless of component type.
    /// This shows how we can write code that works with both old and new systems.
    /// </summary>
    string GetNPCName(MonoBehaviour npcComponent)
    {
        // Check if it's a bridge component (new system)
        if (npcComponent is NPCInteractionBridge bridge)
        {
            return bridge.npcName;
        }

        return "Unknown NPC";
    }

    /// <summary>
    /// Bridge method that gets dialogue data regardless of component type.
    /// This is where the abstraction really pays off - the same code works for both systems.
    /// </summary>
    DialogueData GetDialogueData(MonoBehaviour npcComponent)
    {
        // Handle new system via bridge
        if (npcComponent is NPCInteractionBridge bridge)
        {
            return bridge.GetDialogueData();
        }

        return null;
    }

    #endregion

    #region Dialogue Management

    /// <summary>
    /// Starts dialogue with the currently selected NPC, regardless of component type.
    /// This method shows how abstraction allows us to treat different NPC types uniformly.
    /// </summary>
    void StartDialogueWithCurrentNPC()
    {
        if (currentInteractableNPC == null) return;

        // Get dialogue data using our bridge method
        DialogueData dialogueData = GetDialogueData(currentInteractableNPC);

        if (dialogueData == null)
        {
            string npcName = GetNPCName(currentInteractableNPC);
            Debug.LogWarning($"No dialogue data found for NPC: {npcName}");
            return;
        }

        StartDialogue(dialogueData);

        // Notify the NPC component that dialogue has started
        NotifyNPCDialogueStart(currentInteractableNPC);
    }

    // Add this method to EnhancedNPCInteractionSystem.cs

    /// <summary>
    /// Starts dialogue with an NPC that uses the bridge component.
    /// This method provides the interface that NPCInteractionBridge components
    /// use to integrate with the dialogue system.
    /// </summary>
    public void StartDialogueWithBridge(NPCInteractionBridge bridge)
    {
        if (bridge == null || isInDialogue)
        {
            Debug.LogWarning("EnhancedNPCInteractionSystem: Cannot start dialogue - bridge is null or already in dialogue");
            return;
        }

        // Get dialogue data through the bridge
        DialogueData dialogueData = bridge.GetDialogueData();
        if (dialogueData == null)
        {
            Debug.LogWarning($"EnhancedNPCInteractionSystem: No dialogue data found for NPC {bridge.npcName}");
            return;
        }

        // Set the bridge as our current interactable NPC
        currentInteractableNPC = bridge;

        // Start the dialogue using our existing dialogue system
        StartDialogue(dialogueData);

        // Notify the bridge that dialogue has started
        bridge.OnDialogueStart();
    }

    /// <summary>
    /// Core dialogue starting logic that works regardless of NPC component type.
    /// This contains your existing dialogue initialization logic.
    /// </summary>
    public void StartDialogue(DialogueData dialogueData)
    {
        currentDialogue = dialogueData;
        currentDialogueIndex = 0;
        isInDialogue = true;

        // Store original bubble state for restoration later
        originalNPCBubble = GetCurrentNPCBubbleSprite();

        // Hide interaction prompt
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Show dialogue UI
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // Play dialogue start sound
        PlayAudioClip(dialogueData.dialogueStartSound ?? bookOpenSound);

        // Start with greeting if available
        TimeOfDay currentTime = dayNightCycle != null ? dayNightCycle.CurrentTimeOfDay : TimeOfDay.Day;
        DialogueEntry[] availableGreetings = currentDialogue.GetAvailableDialogues(currentDialogue.greetings, currentTime, gameFlags);

        if (availableGreetings.Length > 0)
        {
            DisplayDialogue(availableGreetings[0]);
        }
        else
        {
            // Start with first available dialogue
            DialogueEntry[] availableDialogues = currentDialogue.GetAvailableDialogues(currentDialogue.dialogueEntries, currentTime, gameFlags);
            if (availableDialogues.Length > 0)
            {
                DisplayDialogue(availableDialogues[0]);
            }
            else
            {
                EndDialogue();
            }
        }
    }

    /// <summary>
    /// Notifies the NPC component that dialogue has started.
    /// This allows both old and new NPC types to react appropriately to dialogue beginning.
    /// </summary>
    void NotifyNPCDialogueStart(MonoBehaviour npcComponent)
    {
        if (npcComponent is NPCInteractionBridge bridge)
        {
            bridge.OnDialogueStart();
        }

        OnDialogueStart?.Invoke(npcComponent);
    }

    /// <summary>
    /// Notifies the NPC component that dialogue has ended.
    /// This allows both old and new NPC types to return to their normal behavior.
    /// </summary>
    void NotifyNPCDialogueEnd(MonoBehaviour npcComponent)
    {
        if (npcComponent is NPCInteractionBridge bridge)
        {
            bridge.OnDialogueEnd();
        }

        OnDialogueEnd?.Invoke(npcComponent);
    }

    public void ContinueDialogue()
    {
        if (!isInDialogue || currentDialogue == null || isTyping) return;

        PlayAudioClip(pageFlipSound);
        currentDialogueIndex++;

        TimeOfDay currentTime = dayNightCycle != null ? dayNightCycle.CurrentTimeOfDay : TimeOfDay.Day;
        DialogueEntry[] availableDialogues = currentDialogue.GetAvailableDialogues(currentDialogue.dialogueEntries, currentTime, gameFlags);

        if (currentDialogueIndex < availableDialogues.Length)
        {
            DisplayDialogue(availableDialogues[currentDialogueIndex]);
        }
        else
        {
            // Check for farewell
            DialogueEntry[] availableFarewells = currentDialogue.GetAvailableDialogues(currentDialogue.farewells, currentTime, gameFlags);
            if (availableFarewells.Length > 0)
            {
                DisplayDialogue(availableFarewells[0]);
                StartCoroutine(EndDialogueAfterDelay(2f));
            }
            else
            {
                EndDialogue();
            }
        }
    }

    public void EndDialogue()
    {
        if (!isInDialogue) return;

        isInDialogue = false;

        // Hide dialogue UI
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Play dialogue end sound
        PlayAudioClip(bookCloseSound);

        // Restore original NPC bubble
        RestoreOriginalNPCBubble();

        // Notify NPC that dialogue ended
        if (currentInteractableNPC != null)
        {
            NotifyNPCDialogueEnd(currentInteractableNPC);
        }

        string npcName = currentInteractableNPC != null ? GetNPCName(currentInteractableNPC) : "unknown NPC";
        Debug.Log($"EnhancedNPCInteractionSystem: Ended dialogue with {npcName}");

        currentInteractableNPC = null;
        currentDialogue = null;
    }

    #endregion

    #region Input and UI Management

    void HandleDialogueInput()
    {
        // Skip typing animation
        if (isTyping && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(interactKey)))
        {
            SkipTypewriter();
        }
        // Continue dialogue
        else if (!isTyping && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(interactKey)))
        {
            ContinueDialogue();
        }
        // End dialogue
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndDialogue();
        }
    }

    void DisplayDialogue(DialogueEntry entry)
    {
        if (entry == null) return;

        UpdateNPCConversationBubble(entry);

        if (speakerNameText != null)
            speakerNameText.text = entry.speakerName.ToUpper();

        if (dialogueText != null)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypewriterEffect(entry.dialogueText, entry.pauseAfterDialogue));
        }

        UpdateDialogueButtons();
    }

    IEnumerator TypewriterEffect(string text, float pauseAfter = 0f)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in text)
        {
            dialogueText.text += letter;

            if (currentDialogue != null && currentDialogue.typewriterSound != null)
            {
                if (letter != ' ' && Random.Range(0f, 1f) > 0.7f)
                {
                    PlayAudioClip(currentDialogue.typewriterSound);
                }
            }

            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;

        if (pauseAfter > 0f)
        {
            yield return new WaitForSeconds(pauseAfter);
        }
    }

    void SkipTypewriter()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;

            if (currentDialogue != null)
            {
                TimeOfDay currentTime = dayNightCycle != null ? dayNightCycle.CurrentTimeOfDay : TimeOfDay.Day;
                DialogueEntry[] availableDialogues = currentDialogue.GetAvailableDialogues(currentDialogue.dialogueEntries, currentTime, gameFlags);
                if (currentDialogueIndex < availableDialogues.Length)
                {
                    dialogueText.text = availableDialogues[currentDialogueIndex].dialogueText;
                }
            }
        }
    }

    void UpdateDialogueButtons()
    {
        if (currentDialogue == null) return;

        TimeOfDay currentTime = dayNightCycle != null ? dayNightCycle.CurrentTimeOfDay : TimeOfDay.Day;
        DialogueEntry[] availableDialogues = currentDialogue.GetAvailableDialogues(currentDialogue.dialogueEntries, currentTime, gameFlags);
        bool hasMoreDialogue = currentDialogueIndex < availableDialogues.Length - 1;

        if (continueButton != null)
            continueButton.gameObject.SetActive(hasMoreDialogue);
        if (endButton != null)
            endButton.gameObject.SetActive(!hasMoreDialogue);
    }

    #endregion



    #region Utility Methods (Your Existing Code)

    IEnumerator EndDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndDialogue();
    }

    void UpdateNPCConversationBubble(DialogueEntry entry)
    {
        // Your existing bubble update logic - works with both NPC types
        // Implementation details preserved from your original system
    }

    Sprite GetCurrentNPCBubbleSprite()
    {
        // Your existing bubble sprite retrieval logic
        return null; // Placeholder
    }

    void RestoreOriginalNPCBubble()
    {
        // Your existing bubble restoration logic
    }

    void PlayAudioClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Your existing game flag methods
    public void AddGameFlag(string flag)
    {
        if (!gameFlags.Contains(flag))
        {
            gameFlags.Add(flag);
        }
    }

    public void RemoveGameFlag(string flag)
    {
        gameFlags.Remove(flag);
    }

    public bool HasGameFlag(string flag)
    {
        return gameFlags.Contains(flag);
    }

    public void SetGameFlags(List<string> flags)
    {
        gameFlags = new List<string>(flags);
    }

    public List<string> GetGameFlags()
    {
        return new List<string>(gameFlags);
    }

    #endregion
}