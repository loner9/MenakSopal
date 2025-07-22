using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class NPCInteractionSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Button continueButton;
    public Button endButton;

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
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;

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

    private NPC currentNPC;
    private DialogueData currentDialogue;
    private int currentDialogueIndex = 0;
    private bool isInDialogue = false;
    private bool isTyping = false;
    private Transform player;
    private List<string> gameFlags = new List<string>(); // Simple flag system
    private Sprite originalNPCBubble; // Store original bubble to restore later
    private Coroutine typingCoroutine;
    private DayNightCycle dayNightCycle;

    public bool IsInDialogue => isInDialogue;

    public System.Action<NPC> OnDialogueStart;
    public System.Action<NPC> OnDialogueEnd;

    private void Start()
    {
        // Find required components
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            player = playerGO.transform;

        dayNightCycle = FindObjectOfType<DayNightCycle>();

        // Setup UI
        // SetupAdventureBookUI();

        // Setup buttons
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueDialogue);

        if (endButton != null)
            endButton.onClick.AddListener(EndDialogue);
    }

    private void SetupAdventureBookUI()
    {
        // Hide dialogue initially
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

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
        if (!isInDialogue)
        {
            CheckForNearbyNPCs();

            if (Input.GetKeyDown(interactKey) && currentNPC != null)
            {
                StartDialogue(currentNPC);
            }
        }
        else
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
    }

    private void CheckForNearbyNPCs()
    {
        if (player == null) return;

        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(player.position, interactionRange, npcLayerMask);
        NPC nearestNPC = null;
        float nearestDistance = float.MaxValue;

        foreach (var collider in nearbyColliders)
        {
            NPC npc = collider.GetComponent<NPC>();
            if (npc != null && npc.canInteract)
            {
                float distance = Vector2.Distance(player.position, npc.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestNPC = npc;
                }
            }
        }

        // Update current NPC and prompt
        if (nearestNPC != currentNPC)
        {
            currentNPC = nearestNPC;
            UpdateInteractionPrompt();
        }
    }

    private void UpdateInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            if (currentNPC != null)
            {
                interactionPrompt.SetActive(true);
                if (promptText != null)
                {
                    promptText.text = $"Press {interactKey} to talk to {currentNPC.npcName}";
                }
            }
            else
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    public void StartDialogue(NPC npc)
    {
        if (npc == null || isInDialogue) return;

        // Get dialogue data from NPC
        DialogueData dialogueData = GetDialogueForNPC(npc);
        if (dialogueData == null)
        {
            Debug.LogWarning($"No dialogue data found for NPC: {npc.npcName}");
            return;
        }

        currentNPC = npc;
        currentDialogue = dialogueData;
        currentDialogueIndex = 0;
        isInDialogue = true;

        // Store original bubble state
        originalNPCBubble = GetCurrentNPCBubbleSprite();

        // Hide interaction prompt
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Play dialogue start sound
        PlayAudioClip(dialogueData.dialogueStartSound ?? bookOpenSound);

        // Show dialogue with adventure book animation
        if (enableBookAnimations)
        {
            StartCoroutine(OpenBookAnimation());
        }
        else
        {
            // Show dialogue immediately
            if (dialogueCanvas != null)
                dialogueCanvas.SetActive(true);
            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);
            DisplayFirstDialogue();
        }

        // Notify systems
        OnDialogueStart?.Invoke(npc);

        // Force NPC to interaction state if not already
        if (npc.StateMachine.CurrentNPCState != npc.InteractionState)
        {
            npc.StateMachine.ChangeState(npc.InteractionState);
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
        DisplayFirstDialogue();
    }

    private void DisplayFirstDialogue()
    {
        TimeOfDay currentTime = dayNightCycle != null ? dayNightCycle.CurrentTimeOfDay : TimeOfDay.Day;

        // Start with greeting if available
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
                return;
            }
        }
    }

    public void ContinueDialogue()
    {
        if (!isInDialogue || currentDialogue == null || isTyping) return;

        // Play page flip sound
        PlayAudioClip(pageFlipSound);

        currentDialogueIndex++;

        TimeOfDay currentTime = dayNightCycle != null ? dayNightCycle.CurrentTimeOfDay : TimeOfDay.Day;

        // Get available dialogues for current time
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
                // End dialogue after farewell
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
        currentDialogueIndex = 0;

        // Stop any typing animation
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;
        }

        // Play book close sound
        PlayAudioClip(bookCloseSound);

        // Adventure book closing animation
        if (enableBookAnimations)
        {
            StartCoroutine(CloseBookAnimation());
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

        // Hide UI
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        FinishEndDialogue();
    }

    private void FinishEndDialogue()
    {
        // Restore original NPC bubble
        RestoreOriginalNPCBubble();

        // Show interaction prompt again if still near NPC
        UpdateInteractionPrompt();

        // Notify systems
        OnDialogueEnd?.Invoke(currentNPC);

        Debug.Log($"Ended dialogue with {(currentNPC != null ? currentNPC.npcName : "unknown NPC")}");

        currentNPC = null;
        currentDialogue = null;
    }

    private IEnumerator EndDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndDialogue();
    }

    private void DisplayDialogue(DialogueEntry entry)
    {
        if (entry == null) return;

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
        dialogueText.text = "";

        foreach (char letter in text)
        {
            dialogueText.text += letter;

            // Play typewriter sound for certain characters
            if (currentDialogue != null && currentDialogue.typewriterSound != null)
            {
                if (letter != ' ' && Random.Range(0f, 1f) > 0.7f) // Random typing sounds
                {
                    PlayAudioClip(currentDialogue.typewriterSound);
                }
            }

            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;

        // Optional pause after dialogue
        if (pauseAfter > 0f)
        {
            yield return new WaitForSeconds(pauseAfter);
        }
    }

    private void SkipTypewriter()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;

            // Show full text immediately
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

    private void UpdateDialogueButtons()
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

    private DialogueData GetDialogueForNPC(NPC npc)
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

    private void RestoreOriginalNPCBubble()
    {
        if (currentNPC == null) return;

        // Restore to the state-appropriate bubble
        currentNPC.UpdateBubbleForCurrentState();
    }

    private void PlayAudioClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Public methods for external systems
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
    }

    public DialogueSystemSaveData GetSaveData()
    {
        return new DialogueSystemSaveData
        {
            gameFlags = this.gameFlags
        };
    }

    public void LoadSaveData(DialogueSystemSaveData data)
    {
        if (data != null)
        {
            gameFlags = data.gameFlags ?? new List<string>();
        }
    }
}