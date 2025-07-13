using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DialogueEntry
{
    public string speakerName;
    [TextArea(3, 5)]
    public string dialogueText;
    public TimeOfDay[] availableTimesOfDay;
    public bool isRepeatable = true;
    public string[] requiredFlags; // For quest system integration
    
    [Header("Conversation Bubble")]
    [Tooltip("Specific bubble sprite to show during this dialogue. If null, uses default conversation bubble.")]
    public Sprite conversationBubbleSprite;
}

[CreateAssetMenu(fileName = "New Dialogue", menuName = "NPC/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string npcName;
    public DialogueEntry[] dialogueEntries;
    public DialogueEntry[] greetings;
    public DialogueEntry[] farewells;
    
    [Header("Default Conversation Bubble")]
    [Tooltip("Default bubble sprite for this NPC's conversations if no specific bubble is set.")]
    public Sprite defaultConversationBubble;
}

public class NPCInteractionSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Button continueButton;
    public Button endButton;
    
    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public LayerMask npcLayerMask = -1;
    public float interactionRange = 2f;
    
    [Header("Visual Feedback")]
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;
    
    [Header("Default Conversation Bubbles")]
    [Tooltip("Default bubble sprite shown when talking to NPCs if no specific bubble is defined.")]
    public Sprite defaultInteractionBubble;
    [Tooltip("Alternative conversation bubbles for different types of interactions.")]
    public Sprite questionBubbleSprite;
    public Sprite exclamationBubbleSprite;
    public Sprite heartBubbleSprite;
    public Sprite angerBubbleSprite;
    
    private NPC currentNPC;
    private DialogueData currentDialogue;
    private int currentDialogueIndex = 0;
    private bool isInDialogue = false;
    private Transform player;
    private List<string> gameFlags = new List<string>(); // Simple flag system
    private Sprite originalNPCBubble; // Store original bubble to restore later
    
    public System.Action<NPC> OnDialogueStart;
    public System.Action<NPC> OnDialogueEnd;
    
    private void Start()
    {
        // Find player
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            player = playerGO.transform;
        
        // Setup UI
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
        
        // Setup buttons
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
            
            if (Input.GetKeyDown(interactKey) && currentNPC != null)
            {
                StartDialogue(currentNPC);
            }
        }
        else
        {
            // Handle dialogue input
            if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.Space))
            {
                ContinueDialogue();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
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
        
        // Show dialogue panel
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
        
        // Start with greeting if available
        DialogueEntry[] availableGreetings = GetAvailableDialogues(currentDialogue.greetings);
        if (availableGreetings.Length > 0)
        {
            DisplayDialogue(availableGreetings[0]);
        }
        else
        {
            // Start with first available dialogue
            DialogueEntry[] availableDialogues = GetAvailableDialogues(currentDialogue.dialogueEntries);
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
        
        // Notify systems
        OnDialogueStart?.Invoke(npc);
        
        // Force NPC to interaction state if not already
        if (npc.StateMachine.CurrentNPCState != npc.InteractState)
        {
            npc.StateMachine.ChangeState(npc.InteractState);
        }
    }
    
    public void ContinueDialogue()
    {
        if (!isInDialogue || currentDialogue == null) return;
        
        currentDialogueIndex++;
        
        // Get available dialogues for current time
        DialogueEntry[] availableDialogues = GetAvailableDialogues(currentDialogue.dialogueEntries);
        
        if (currentDialogueIndex < availableDialogues.Length)
        {
            DisplayDialogue(availableDialogues[currentDialogueIndex]);
        }
        else
        {
            // Check for farewell
            DialogueEntry[] availableFarewells = GetAvailableDialogues(currentDialogue.farewells);
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
        
        // Hide dialogue panel
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        // Restore original NPC bubble
        RestoreOriginalNPCBubble();
        
        // Show interaction prompt again if still near NPC
        UpdateInteractionPrompt();
        
        // Notify systems
        OnDialogueEnd?.Invoke(currentNPC);
        
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
            speakerNameText.text = entry.speakerName;
        
        if (dialogueText != null)
        {
            StartCoroutine(TypewriterEffect(entry.dialogueText));
        }
        
        // Update button visibility
        UpdateDialogueButtons();
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
    
    private IEnumerator TypewriterEffect(string text)
    {
        if (dialogueText == null) yield break;
        
        dialogueText.text = "";
        
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.02f); // Adjust speed as needed
        }
    }
    
    private void UpdateDialogueButtons()
    {
        DialogueEntry[] availableDialogues = GetAvailableDialogues(currentDialogue.dialogueEntries);
        bool hasMoreDialogue = currentDialogueIndex < availableDialogues.Length - 1;
        
        if (continueButton != null)
            continueButton.gameObject.SetActive(hasMoreDialogue);
        
        if (endButton != null)
            endButton.gameObject.SetActive(!hasMoreDialogue);
    }
    
    private DialogueEntry[] GetAvailableDialogues(DialogueEntry[] dialogues)
    {
        if (dialogues == null) return new DialogueEntry[0];
        
        List<DialogueEntry> available = new List<DialogueEntry>();
        
        foreach (var dialogue in dialogues)
        {
            if (IsDialogueAvailable(dialogue))
            {
                available.Add(dialogue);
            }
        }
        
        return available.ToArray();
    }
    
    private bool IsDialogueAvailable(DialogueEntry dialogue)
    {
        // Check time of day availability
        if (dialogue.availableTimesOfDay != null && dialogue.availableTimesOfDay.Length > 0)
        {
            DayNightCycle dayNightCycle = FindObjectOfType<DayNightCycle>();
            if (dayNightCycle != null)
            {
                bool timeMatches = false;
                foreach (var timeOfDay in dialogue.availableTimesOfDay)
                {
                    if (dayNightCycle.CurrentTimeOfDay == timeOfDay)
                    {
                        timeMatches = true;
                        break;
                    }
                }
                if (!timeMatches) return false;
            }
        }
        
        // Check required flags
        if (dialogue.requiredFlags != null && dialogue.requiredFlags.Length > 0)
        {
            foreach (var flag in dialogue.requiredFlags)
            {
                if (!gameFlags.Contains(flag))
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    private DialogueData GetDialogueForNPC(NPC npc)
    {
        // Try to get dialogue from NPC's schedule data first
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
            // Set default conversation bubble if available
            tempDialogue.defaultConversationBubble = defaultInteractionBubble;
            return tempDialogue;
        }
        
        // Fallback: Look for DialogueData in Resources or attached to NPC
        DialogueData dialogueData = npc.GetComponent<DialogueData>();
        if (dialogueData == null)
        {
            // Try loading from Resources
            dialogueData = Resources.Load<DialogueData>($"Dialogues/{npc.npcName}");
        }
        
        return dialogueData;
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