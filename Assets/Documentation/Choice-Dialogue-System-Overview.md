# Choice-Based Dialogue System - Technical Overview

## 🔧 System Architecture

This document provides a technical overview of the choice-based dialogue system implementation, including how it extends the existing linear dialogue system.

---

## Core Components

### 1. Data Structures (`DialogueData.cs`)

#### `DialogueChoice` Class
```csharp
[System.Serializable]
public class DialogueChoice
{
    // Content
    public string choiceText;
    
    // Availability Conditions
    public string[] requiredFlags;
    public TimeOfDay[] availableTimesOfDay;
    public bool isRepeatable;
    
    // Consequences
    public string[] flagsToAdd;
    public string[] flagsToRemove;
    
    // Navigation
    public int targetDialogueIndex;
    public DialogueResponse response;
    
    // Visual
    public bool isImportantChoice;
    public Color choiceColor;
}
```

#### `DialogueResponse` Class
```csharp
[System.Serializable]
public class DialogueResponse
{
    // Content
    public string speakerName;
    public string responseText;
    
    // Visual & Timing
    public Sprite conversationBubbleSprite;
    public float pauseAfterResponse;
    
    // Navigation
    public bool continueToNext;
    public int nextDialogueIndex;
}
```

#### Extended `DialogueEntry` Class
```csharp
[System.Serializable]
public class DialogueEntry
{
    // Existing fields remain unchanged...
    
    // NEW: Choice System
    public bool hasChoices = false;
    public DialogueChoice[] choices;
}
```

### 2. UI Controller (`NPCInteractionSystem.cs`)

#### New UI References
```csharp
[Header("Choice System UI")]
public Transform choiceContainer;
public Button choiceButtonPrefab;
public Sprite choiceButtonSprite;

[Header("Choice System Audio")]
public AudioClip choiceHoverSound;
public AudioClip choiceSelectSound;
public AudioClip importantChoiceSound;
```

#### State Management
```csharp
// Choice system state tracking
private bool isShowingChoices = false;
private List<Button> activeChoiceButtons = new List<Button>();
private DialogueEntry currentDialogueEntry;
private bool waitingForChoiceResponse = false;
```

---

## System Flow

### 1. Linear Dialogue Flow (Unchanged)
```
Start Dialogue → Show Text → Continue Button → Next Entry → End
```

### 2. Choice Dialogue Flow (New)
```
Start Dialogue → Show Text → Check for Choices → Show Choice Buttons
                                                      ↓
Player Selects Choice → Process Consequences → Show Response (optional)
                                                      ↓
Navigate to Target Entry or End Dialogue
```

### 3. Detailed Flow Diagram
```
DisplayDialogue(entry)
         ↓
   Clear existing choices
         ↓
   Show dialogue text with typewriter
         ↓
   Text complete? → hasChoices?
         ↓ YES              ↓ NO
   ShowChoices()         Show Continue/End buttons
         ↓
   Create choice buttons dynamically
         ↓
   Player clicks choice
         ↓
   OnChoiceSelected()
         ↓
   Process flag consequences
         ↓
   Has response? → Show response → Navigate to target
         ↓ NO
   Navigate directly to target or end
```

---

## Key Methods

### Choice Management
```csharp
// Display available choices for a dialogue entry
private void ShowChoices(DialogueEntry entry)

// Create a single choice button with styling and events  
private void CreateChoiceButton(DialogueChoice choice, int choiceIndex)

// Handle player choice selection
private void OnChoiceSelected(DialogueChoice choice)

// Show NPC response to choice before continuing
private IEnumerator ShowChoiceResponse(DialogueChoice choice)

// Clean up choice buttons
private void ClearChoiceButtons()
```

### Navigation & Flow
```csharp
// Jump to specific dialogue entry by index
private void NavigateToDialogueEntry(int index)

// Process flag additions/removals from choice
private void ProcessChoiceConsequences(DialogueChoice choice)

// Check if choice is available based on flags/time
public bool IsChoiceAvailable(DialogueChoice choice, TimeOfDay currentTime, List<string> gameFlags)
```

---

## Backward Compatibility

### How Existing Dialogues Still Work
1. **Existing `DialogueEntry`** objects have `hasChoices = false` by default
2. **Choice-specific code** only executes when `hasChoices = true`  
3. **Traditional flow** (Continue/End buttons) remains unchanged
4. **All existing features** (flags, time restrictions, bubbles) work as before

### Migration Path
```csharp
// Old dialogue entry (still works)
var oldEntry = new DialogueEntry
{
    speakerName = "NPC",
    dialogueText = "Hello!",
    hasChoices = false  // Default - traditional behavior
};

// New choice dialogue entry
var newEntry = new DialogueEntry  
{
    speakerName = "NPC",
    dialogueText = "What do you want?",
    hasChoices = true,  // Enable choice system
    choices = new DialogueChoice[] { /* choices here */ }
};
```

---

## Flag System Integration

### Flag Processing
```csharp
// When choice is selected:
ProcessChoiceConsequences(choice)
├── Add flags from choice.flagsToAdd[]
├── Remove flags from choice.flagsToRemove[]  
└── Update game state immediately
```

### Availability Checking
```csharp
// Before showing choice:
IsChoiceAvailable(choice, currentTime, gameFlags)
├── Check required flags exist in gameFlags
├── Check current time matches availableTimesOfDay
└── Return true only if all conditions met
```

### Quest System Hooks
The flag system is designed for easy quest integration:
```csharp
// Example quest-triggering choice
var questChoice = new DialogueChoice
{
    choiceText = "I'll help you with that",
    flagsToAdd = new string[] { "QUEST_STARTED", "HELPING_BLACKSMITH" },
    flagsToRemove = new string[] { "LOOKING_FOR_WORK" },
    targetDialogueIndex = 5  // Quest acceptance dialogue
};
```

---

## Audio System Integration

### Sound Hierarchy
```
Choice Audio Priority:
├── Important Choice Sound (highest priority)
├── Regular Choice Select Sound  
├── Choice Hover Sound (subtle)
└── Existing dialogue sounds (preserved)
```

### Implementation
```csharp
// In OnChoiceSelected():
AudioClip soundToPlay = choice.isImportantChoice ? 
    importantChoiceSound : choiceSelectSound;
PlayAudioClip(soundToPlay);
```

---

## UI System Details

### Dynamic Button Creation
```csharp
// For each available choice:
1. Instantiate choiceButtonPrefab in choiceContainer
2. Configure button sprite and colors
3. Set choice text in TextMeshPro component
4. Apply special styling for important choices
5. Add click listener: () => OnChoiceSelected(choice)
6. Add hover listener for audio feedback
7. Store in activeChoiceButtons list for cleanup
```

### Layout Management
```csharp
// ChoiceContainer setup:
Vertical Layout Group:
├── Auto-sizes based on number of choices
├── Maintains consistent spacing
├── Handles different text lengths
└── Integrates with Adventure Book styling

Content Size Fitter:
├── Expands vertically as needed
└── Maintains fixed width
```

---

## Performance Considerations

### Memory Management
- **Choice buttons** are instantiated only when needed
- **Buttons destroyed** immediately after choice selection
- **No persistent choice UI** elements
- **Pooling not required** due to infrequent usage

### Optimization Features
- **Lazy evaluation** - choices checked only when dialogue entry has `hasChoices = true`
- **Early termination** - availability checking stops at first failed condition
- **Minimal UI updates** - only affected elements are modified

---

## Extension Points

### For Future Quest System
```csharp
// Planned extensions:
public class DialogueChoice
{
    // Quest integration (future)
    public string questID;
    public QuestAction[] questActions;
    public NPCRelationshipChange[] relationshipChanges;
    
    // Advanced conditions (future)  
    public InventoryRequirement[] itemRequirements;
    public StatRequirement[] statRequirements;
}
```

### For Advanced Features
- **Custom choice types** (trade, combat, skill checks)
- **Conditional response text** based on player stats
- **Multi-stage choice sequences**
- **Choice memory system** for complex narratives

---

## Testing & Validation

### Automated Tests
The system includes validation tests in `DialogueSystemValidator.cs`:
- **Backward compatibility** verification
- **Choice availability** logic testing
- **Flag system** integration testing
- **Navigation** system validation

### Manual Testing Checklist
- ✅ Linear dialogues still work unchanged
- ✅ Choice buttons appear correctly
- ✅ Flag requirements work properly
- ✅ Time-based restrictions function
- ✅ Audio feedback plays correctly
- ✅ UI layout adapts to choice count
- ✅ Navigation works for all target indices

---

## 🚀 Ready for Production

The choice-based dialogue system is fully implemented, tested, and ready for use. It maintains 100% backward compatibility while adding powerful branching dialogue capabilities that will seamlessly extend to support quest systems and advanced NPC interactions.