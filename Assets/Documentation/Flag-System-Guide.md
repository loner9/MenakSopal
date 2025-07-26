# Flag System Guide - Complete Reference

## 🚩 **What Are Flags?**

**Flags** are like **memory tags** for your game. They remember what the player has done, what they know, and what state the game world is in.

Think of flags as **simple true/false switches**:
- Flag exists = TRUE
- Flag doesn't exist = FALSE

---

## 🎯 **Simple Example**

```
Player talks to Blacksmith:
Blacksmith: "I need help with my forge"
Player chooses: "I'll help you"
→ FLAG GETS ADDED: "HELPED_BLACKSMITH"

Later, player talks to Blacksmith again:
- IF "HELPED_BLACKSMITH" flag exists:
  Blacksmith: "Thanks again for helping with my forge!"
- IF flag doesn't exist:
  Blacksmith: "I still need help with my forge..."
```

---

## 🧠 **Where Are Flags Actually Stored?**

### **Current Implementation Location:**
```cs
// File: NPCInteractionSystem.cs (line ~80)
private List<string> gameFlags = new List<string>();
```

**This is where your game's memory lives!** The `NPCInteractionSystem` is the **flag manager**.

### **Flag Management Methods:**
```cs
// File: NPCInteractionSystem.cs, Lines ~912-933

// ADD flag (when choice is selected)
public void AddGameFlag(string flag)
{
    if (!gameFlags.Contains(flag))
        gameFlags.Add(flag);  // Stored in the list!
}

// CHECK if flag exists (when showing choices) 
public bool HasGameFlag(string flag)
{
    return gameFlags.Contains(flag);  // Checks the list!
}

// REMOVE flag (when choice consequence removes it)
public void RemoveGameFlag(string flag)
{
    gameFlags.Remove(flag);  // Removes from list!
}

// GET all flags (for save system)
public List<string> GetGameFlags()
{
    return new List<string>(gameFlags);
}

// SET flags (for load system)
public void SetGameFlags(List<string> flags)
{
    gameFlags = new List<string>(flags);
}
```

---

## 🔄 **System Architecture: Who Does What?**

### **DialogueData (ScriptableObject) = Configuration/Template**
```cs
// This is just configuration - like a recipe!
DialogueChoice choice = new DialogueChoice
{
    requiredFlags = new string[] { "COMPLETED_QUEST" },  // ← USER sets this
    flagsToAdd = new string[] { "TALKED_TO_BLACKSMITH" } // ← USER sets this
};
```

**Role:** User defines what flags are needed and what consequences happen

### **NPCInteractionSystem = Actual Flag Manager**
```cs
// This actually manages the flags at runtime
private List<string> gameFlags = new List<string>();  // ← SYSTEM stores flags here

// When choice is selected, SYSTEM processes the consequences:
private void ProcessChoiceConsequences(DialogueChoice choice)
{
    // SYSTEM reads user configuration and applies it:
    if (choice.flagsToAdd != null)
        foreach (string flag in choice.flagsToAdd)
            AddGameFlag(flag);  // ← SYSTEM adds to memory
}
```

**Role:** Stores, checks, adds, removes flags at runtime

### **Responsibility Table**

| **Component** | **Role** | **What They Handle** |
|---------------|----------|----------------------|
| **DialogueData** | Configuration | User defines what flags are needed/added |
| **NPCInteractionSystem** | Flag Manager | Stores, checks, adds, removes flags |
| **Unity Inspector** | Setup Tool | User configures flag requirements |
| **Runtime System** | Executor | Processes flag logic automatically |

---

## 📋 **Complete Flag Flow Example**

### **Step 1: User Configuration (DialogueData)**
```cs
// User creates this in Unity Inspector:
DialogueChoice helpChoice = new DialogueChoice
{
    choiceText = "I'll help you",
    requiredFlags = new string[] { "MET_BLACKSMITH" },     // User requirement
    flagsToAdd = new string[] { "AGREED_TO_HELP" },       // User consequence
    targetDialogueIndex = 1
};
```

### **Step 2: Runtime Flag Checking (System)**
```cs
// When showing dialogue, SYSTEM checks requirements:
private void ShowChoices(DialogueEntry entry)
{
    foreach(var choice in entry.choices)
    {
        // SYSTEM checks if choice should be available:
        bool available = IsChoiceAvailable(choice, currentTime, gameFlags);
        //                                                      ↑
        //                          SYSTEM's flag storage
    }
}

public bool IsChoiceAvailable(DialogueChoice choice, TimeOfDay currentTime, List<string> gameFlags)
{
    // Check if player has required flags:
    if (choice.requiredFlags != null)
    {
        foreach (string flag in choice.requiredFlags)
        {
            if (!gameFlags.Contains(flag))  // ← Checking SYSTEM's memory
                return false;
        }
    }
    return true;
}
```

### **Step 3: Flag Consequences (System)**
```cs
// When player clicks choice, SYSTEM processes consequences:
private void OnChoiceSelected(DialogueChoice choice)
{
    ProcessChoiceConsequences(choice);  // ← SYSTEM handles this
}

private void ProcessChoiceConsequences(DialogueChoice choice)
{
    // SYSTEM reads user configuration and applies it:
    if (choice.flagsToAdd != null)
    {
        foreach (string flag in choice.flagsToAdd)
        {
            AddGameFlag(flag);  // ← SYSTEM adds to its memory
            //    ↓
            //  gameFlags.Add(flag);  ← Stored here!
        }
    }
    
    if (choice.flagsToRemove != null)
    {
        foreach (string flag in choice.flagsToRemove)
        {
            RemoveGameFlag(flag);  // ← SYSTEM removes from memory
        }
    }
}
```

---

## 🔄 **How Flags Work in Your System**

### **1. Adding Flags (Consequences)**
When player makes a choice, flags get added:

```cs
DialogueChoice:
├── Choice Text: "I'll help you with that quest"
├── Flags To Add: ["QUEST_STARTED", "HELPING_BLACKSMITH"]
└── Target Index: 1
```

After player clicks this choice:
- Game remembers: "QUEST_STARTED" ✅
- Game remembers: "HELPING_BLACKSMITH" ✅

### **2. Checking Flags (Requirements)**
Some choices only appear if certain flags exist:

```cs
DialogueChoice:
├── Choice Text: "About that quest you gave me..."
├── Required Flags: ["QUEST_STARTED"] 
└── Target Index: 2
```

This choice ONLY appears if player has "QUEST_STARTED" flag.

### **3. Removing Flags**
Sometimes you want to "forget" something:

```cs
DialogueChoice:
├── Choice Text: "I completed the quest!"
├── Flags To Add: ["QUEST_COMPLETED"]
├── Flags To Remove: ["QUEST_STARTED"]  // Remove old flag
└── Target Index: 3
```

---

## 🎭 **Real-World Examples**

### **Example 1: Character Relationships**
```
First meeting:
Player: "Nice to meet you"
→ Adds flag: "MET_SARAH"

Later conversations can check:
- IF "MET_SARAH" exists: "Good to see you again!"  
- IF "MET_SARAH" missing: "Who are you, stranger?"
```

### **Example 2: Story Progress**
```
Player discovers a secret:
→ Adds flag: "KNOWS_MAYORS_SECRET"

Later dialogue with Mayor:
- Choice: "I know your secret..." 
- Required Flags: ["KNOWS_MAYORS_SECRET"]
- This choice ONLY appears if player found the secret!
```

### **Example 3: Quest States**
```
Quest Flow:
1. "I need help" → Adds: "QUEST_OFFERED"
2. "I'll help" → Adds: "QUEST_ACCEPTED", Removes: "QUEST_OFFERED"  
3. "I'm done" → Adds: "QUEST_COMPLETED", Removes: "QUEST_ACCEPTED"

Different NPCs can react based on quest state:
- Blacksmith checks "QUEST_ACCEPTED" → "Good luck with that!"
- Merchant checks "QUEST_COMPLETED" → "I heard you helped the blacksmith!"
```

---

## 💡 **Flag Naming Best Practices**

### **Good Flag Names:**
```
✅ "COMPLETED_TUTORIAL"       (clear and specific)
✅ "KNOWS_MAYORS_SECRET"      (describes what player knows)
✅ "HOSTILE_TO_BANDITS"       (describes relationship)
✅ "VISITED_FOREST_TEMPLE"    (describes player action)
```

### **Bad Flag Names:**
```
❌ "flag1"                    (meaningless)
❌ "tutorial_done_yes_no"     (confusing)
❌ "mayor_secret_thing"       (vague)
❌ "ABCDEFGHIJKLMNOP"         (unreadable)
```

### **Flag Categories & Naming Conventions**

#### **1. Story Progress Flags**
```
"INTRO_COMPLETED"
"CHAPTER_1_FINISHED"
"FINAL_BOSS_DEFEATED"
"ENDING_A_UNLOCKED"
```

#### **2. Character Relationship Flags**
```
"LIKES_PLAYER"
"TRUSTS_PLAYER"  
"ROMANTIC_INTEREST"
"CONSIDERS_ENEMY"
"MET_[CHARACTER_NAME]"
"HELPED_[CHARACTER_NAME]"
```

#### **3. World State Flags**
```
"BRIDGE_REPAIRED"
"TOWN_UNDER_ATTACK"
"HARVEST_FESTIVAL_ACTIVE"
"WINTER_SEASON"
```

#### **4. Player Knowledge Flags**
```
"KNOWS_ASSASSINATION_PLOT"
"LEARNED_MAGIC_WORDS"
"DISCOVERED_HIDDEN_PASSAGE"
"READ_ANCIENT_BOOK"
```

#### **5. Quest State Flags**
```
"QUEST_[NAME]_STARTED"
"QUEST_[NAME]_COMPLETED"
"QUEST_[NAME]_FAILED"
"QUEST_[NAME]_REWARD_CLAIMED"
```

---

## 💾 **Save/Load System Integration**

The flag system includes automatic save/load support:

```cs
// File: NPCInteractionSystem.cs, Lines ~720-734

[System.Serializable]
public class DialogueSystemSaveData
{
    public List<string> gameFlags;  // ← Flags get saved here!
}

public DialogueSystemSaveData GetSaveData()
{
    return new DialogueSystemSaveData
    {
        gameFlags = this.gameFlags  // ← Exports current flags
    };
}

public void LoadSaveData(DialogueSystemSaveData data)
{
    if (data != null)
        gameFlags = data.gameFlags ?? new List<string>();  // ← Restores flags
}
```

**Usage:**
```cs
// Save flags to file
var saveData = npcInteractionSystem.GetSaveData();
// ... save saveData to file

// Load flags from file
npcInteractionSystem.LoadSaveData(loadedSaveData);
```

---

## 🌐 **Accessing Flags from Other Systems**

If other systems (QuestManager, InventorySystem, etc.) need to check/modify flags:

### **Method 1: Direct Reference**
```cs
// In another script:
public class QuestManager : MonoBehaviour 
{
    private NPCInteractionSystem dialogueSystem;
    
    void Start()
    {
        dialogueSystem = FindObjectOfType<NPCInteractionSystem>();
    }
    
    void CompleteQuest()
    {
        // Add flag from other system
        dialogueSystem.AddGameFlag("QUEST_COMPLETED");  
        
        // Check flag from other system
        if (dialogueSystem.HasGameFlag("ALL_QUESTS_DONE"))  
        {
            // Unlock ending
        }
    }
}
```

### **Method 2: Singleton Pattern** (Recommended for larger games)
```cs
// Extend NPCInteractionSystem to be a singleton:
public class NPCInteractionSystem : MonoBehaviour
{
    public static NPCInteractionSystem Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

// Then anywhere in your game:
NPCInteractionSystem.Instance.AddGameFlag("SOMETHING_HAPPENED");
bool hasFlag = NPCInteractionSystem.Instance.HasGameFlag("SOME_CONDITION");
```

### **Method 3: Events System** (Most flexible)
```cs
// In NPCInteractionSystem, add events:
public static System.Action<string> OnFlagAdded;
public static System.Action<string> OnFlagRemoved;

public void AddGameFlag(string flag)
{
    if (!gameFlags.Contains(flag))
    {
        gameFlags.Add(flag);
        OnFlagAdded?.Invoke(flag);  // Notify other systems
    }
}

// Other systems can listen:
public class QuestManager : MonoBehaviour
{
    void Start()
    {
        NPCInteractionSystem.OnFlagAdded += OnFlagChanged;
    }
    
    void OnFlagChanged(string flag)
    {
        if (flag == "QUEST_COMPLETED")
        {
            // React to quest completion
        }
    }
}
```

---

## 🔍 **Finding Flag System References in Code**

### **Flag Storage Location:**
```
File: Assets/Script/NPC/NPCInteractionSystem.cs
Line: ~80
Code: private List<string> gameFlags = new List<string>();
```

### **Flag Management Methods:**
```
File: Assets/Script/NPC/NPCInteractionSystem.cs
Lines: ~912-933
Methods: AddGameFlag(), RemoveGameFlag(), HasGameFlag(), SetGameFlags(), GetGameFlags()
```

### **Flag Processing Logic:**
```
File: Assets/Script/NPC/NPCInteractionSystem.cs
Lines: ~876-894
Method: ProcessChoiceConsequences()
```

### **Flag Availability Checking:**
```
File: Assets/Script/NPC/DialogueData.cs
Lines: ~220-254
Method: IsChoiceAvailable()
```

---

## 🚀 **The Magic of Flags**

Flags turn your dialogue from:
- **Static scripts** → **Dynamic conversations**
- **One-size-fits-all** → **Personalized experiences**  
- **Forgettable interactions** → **Meaningful choices**

### **Why Flags Are Powerful:**

#### **1. Branching Stories**
```
Without Flags (boring):
NPC: "Hello" 
Player: "Hi"
NPC: "Hello" (same every time)

With Flags (dynamic):
First time: NPC: "Hello, stranger"
After helping: NPC: "Hello, my friend!" (remembers you helped)
After betrayal: NPC: "You... I don't trust you" (remembers betrayal)
```

#### **2. Consequences That Matter**
```
Player is rude to Guard:
→ Adds flag: "RUDE_TO_GUARDS"

Later effects:
- Guards are less helpful
- Some shop owners won't serve you  
- Different story paths open/close
```

#### **3. Character Development**
```
Player shows wisdom in conversations:
→ Adds flags: "WISE_CHOICE_1", "WISE_CHOICE_2", "WISE_CHOICE_3"

Later dialogue:
- Choice: "[WISE] Share your wisdom"
- Required Flags: ["WISE_CHOICE_1", "WISE_CHOICE_2", "WISE_CHOICE_3"]
- Only appears if player has been consistently wise!
```

---

## 🎯 **Quick Reference Summary**

- **User sets flag requirements/consequences** in DialogueData (Unity Inspector)
- **NPCInteractionSystem stores and manages** the actual flags in `gameFlags` list
- **System automatically processes** flag checking and consequences
- **Other systems can access** flags through NPCInteractionSystem reference
- **Flags persist** through save/load system
- **DialogueData is the blueprint**, **NPCInteractionSystem is the memory bank**

Your NPCs will remember everything the player does, creating a living, breathing world that reacts to player choices! 

**That's the power of the flag system** - it's like giving your game a memory and making every conversation feel personal and consequential! 🎭✨