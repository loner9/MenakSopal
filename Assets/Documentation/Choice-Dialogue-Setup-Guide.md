# Choice-Based Dialogue System Setup Guide

## 🎯 Overview

This comprehensive guide will walk you through setting up the choice-based dialogue system in Unity, from basic setup to advanced chain dialogues. Every field and setting is explained in detail for designers and developers.

---

## Part I: Initial Setup

## Step 1: Create Choice Button Prefab

### 1.1 Create the Button Prefab
1. In Unity, right-click in Project → **Create → UI → Button - TextMeshPro**
2. Name it **"ChoiceButton"**
3. Configure the button components as follows:

### 1.2 Configure Button Component
```
Button Component Settings:
├── Navigation: None (or Automatic)
├── Interactable: ✓ Checked
├── Transition: Color Tint
└── Colors:
    ├── Normal Color: White (#FFFFFF)
    ├── Highlighted Color: Light Yellow (#FFFFAA)
    ├── Pressed Color: Gold (#FFD700)
    └── Selected Color: Dark Gold (#B8860B)
```

### 1.3 Style for Adventure Book Theme
```
Image Component (Button Background):
├── Source Image: Your Adventure Book button sprite
├── Image Type: Sliced
├── Fill Center: ✓ Checked
└── Preserve Aspect: ✗ Unchecked

TextMeshPro - Text (Child Object):
├── Font: Your Adventure Book font
├── Font Size: 14-16
├── Color: Dark Brown (#2D1C14)
├── Alignment: Center & Middle
├── Auto Size: Best Fit (optional)
├── Margins: 10px all sides
└── Overflow: Ellipsis
```

### 1.4 Make it a Prefab
- Drag the configured button from Hierarchy to Project folder
- Delete the button from the scene
- The prefab is now ready to use

---

## Step 2: Setup Dialogue UI Panel

### 2.1 Find Your Existing Dialogue Panel
- Locate your current dialogue UI (should have Adventure Book frame)
- Should contain: Speaker Name Text, Dialogue Text, Continue Button, End Button

### 2.2 Add Choice Container
1. Right-click on dialogue panel → **Create Empty**
2. Name it **"ChoiceContainer"**
3. Add **Vertical Layout Group** component:

```
Vertical Layout Group Settings:
├── Padding:
│   ├── Left: 15
│   ├── Right: 15
│   ├── Top: 10
│   └── Bottom: 10
├── Spacing: 8
├── Child Alignment: Upper Center
├── Control Child Size:
│   ├── Width: ✓ Checked
│   └── Height: ✓ Checked
└── Use Child Scale: ✓ Checked
└── Child Force Expand:
    ├── Width: ✓ Checked
    └── Height: ✗ Unchecked
```

4. Add **Content Size Fitter** component:
```
Content Size Fitter Settings:
├── Horizontal Fit: Unconstrained
└── Vertical Fit: Preferred Size
```

### 2.3 Position Choice Container
- Place below the dialogue text area
- Use RectTransform anchors to position properly within Adventure Book frame
- Recommended: Anchor to bottom of dialogue text with some padding

---

## Step 3: Configure NPCInteractionSystem

### 3.1 Inspector Configuration
Find your NPCInteractionSystem component and populate these fields:

```
Choice System UI Section:
├── Choice Container: [Drag ChoiceContainer GameObject]
├── Choice Button Prefab: [Drag ChoiceButton prefab]
└── Choice Button Sprite: [Adventure Book button sprite]

Choice System Audio Section:
├── Choice Hover Sound: [Optional hover SFX]
├── Choice Select Sound: [Button click SFX]
└── Important Choice Sound: [Special choice SFX]
```

---

## Part II: Creating Dialogues

## Step 4: Basic Choice Dialogue

### 4.1 Create DialogueData Asset
1. Right-click in Project → **Create → NPC → Dialogue Data**
2. Name it **"BasicChoiceDialogue"**

### 4.2 DialogueData Structure Overview
```
DialogueData Asset Contains:
├── NPC Information (Basic info about the dialogue)
├── Dialogue Entries (Array of dialogue steps)
├── Greetings (Optional opening dialogues)
├── Farewells (Optional closing dialogues)
└── Visual/Audio Settings (Bubbles, sounds, etc.)
```

### 4.3 Configure Basic Dialogue
```
NPC Information:
├── NPC Name: "Village Guard"
│   └── Purpose: Identifies the NPC in the system
├── Dialogue Description: "Basic choice dialogue example"
│   └── Purpose: Documentation for designers
└── Default Conversation Bubble: [Optional sprite]
    └── Purpose: Default bubble sprite for all entries
```

### 4.4 Dialogue Entries Configuration

#### Entry [0] - Main Choice Entry
```
Dialogue Content:
├── Speaker Name: "Village Guard"
│   └── Purpose: Who is speaking (appears in UI)
├── Dialogue Text: "Halt! What brings you to our village?"
│   └── Purpose: What the NPC says (shows with typewriter effect)
├── Available Times Of Day: [All times] (optional)
│   └── Purpose: When this dialogue can appear
├── Is Repeatable: ✓ Checked
│   └── Purpose: Can this dialogue be seen multiple times?
└── Required Flags: [] (empty)
    └── Purpose: Flags needed for this dialogue to appear

Choice System:
├── Has Choices: ✓ CHECKED ← CRITICAL: Enables choice system
│   └── Purpose: Tells system to show choices after text
└── Choices (Size: 3):
    └── Purpose: Array of player choice options
    
    Choice [0] - Peaceful Option:
    ├── Choice Text: "I'm just passing through"
    │   └── Purpose: Text shown on choice button
    ├── Required Flags: [] (empty)
    │   └── Purpose: Flags needed for this choice to appear
    ├── Available Times Of Day: [] (empty = always)
    │   └── Purpose: When this choice is available
    ├── Is Repeatable: ✓ Checked
    │   └── Purpose: Can select this choice multiple times?
    ├── Target Dialogue Index: 1
    │   └── Purpose: Jump to dialogue entry [1] when selected
    ├── Response: null (empty)
    │   └── Purpose: NPC reaction before continuing (optional)
    ├── Flags To Add: [] (empty)
    │   └── Purpose: Flags added when choice is selected
    ├── Flags To Remove: [] (empty)
    │   └── Purpose: Flags removed when choice is selected
    ├── Is Important Choice: ✗ Unchecked
    │   └── Purpose: Special styling and audio for key choices
    └── Choice Color: Default (White)
        └── Purpose: Button color tint
    
    Choice [1] - Quest Hook:
    ├── Choice Text: "I'm looking for work"
    ├── Target Dialogue Index: 2
    ├── Flags To Add: ["ASKED_ABOUT_WORK"]
    │   └── Purpose: Remember player asked about work
    ├── Is Important Choice: ✓ Checked
    │   └── Purpose: Uses special audio and bold text
    └── Choice Color: Light Blue (#87CEEB)
        └── Purpose: Makes this choice stand out
    
    Choice [2] - Rude Option:
    ├── Choice Text: "None of your business!"
    ├── Target Dialogue Index: -1
    │   └── Purpose: -1 means END DIALOGUE
    ├── Flags To Add: ["RUDE_TO_GUARD"]
    │   └── Purpose: Remember player was rude
    ├── Choice Color: Red (#FF6B6B)
    │   └── Purpose: Visual indication of negative choice
    └── Is Repeatable: ✓ Checked
```

#### Entry [1] - Passing Through Response
```
Dialogue Content:
├── Speaker Name: "Village Guard"
├── Dialogue Text: "Very well, safe travels, stranger!"
└── Conversation Bubble Sprite: [Optional custom bubble]

Choice System:
├── Has Choices: ✗ UNCHECKED ← Regular dialogue, no choices
│   └── Purpose: Shows Continue/End buttons instead
└── Choices: [] (empty)
    └── Purpose: No choices = traditional dialogue flow
```

#### Entry [2] - Work Response
```
Dialogue Content:
├── Speaker Name: "Village Guard"
├── Dialogue Text: "Ah, a worker! See the blacksmith near the forge. He always needs help."
└── Pause After Dialogue: 1.0f
    └── Purpose: Wait 1 second before showing Continue button

Choice System:
└── Has Choices: ✗ UNCHECKED
```

---

## Step 5: Advanced Choice Features

### 5.1 Flag-Based Conditional Choices
Choices that only appear under certain conditions:

```
Advanced Choice Configuration:
├── Choice Text: "About that secret mission..."
├── Required Flags: ["KNOWS_SECRET", "TRUSTED_BY_CAPTAIN"]
│   └── Purpose: BOTH flags must exist for choice to appear
├── Available Times Of Day: [Day, Sunset]
│   └── Purpose: Choice only available during these times
├── Target Dialogue Index: 5
├── Flags To Add: ["DISCUSSED_SECRET"]
├── Flags To Remove: ["KEEPING_SECRET"]
│   └── Purpose: Player no longer keeping it secret
├── Is Repeatable: ✗ Unchecked
│   └── Purpose: Can only discuss secret once
└── Is Important Choice: ✓ Checked
    └── Purpose: This is a significant story moment
```

### 5.2 Choice with Response System
Instead of jumping directly to another dialogue entry:

```
Choice with NPC Response:
├── Choice Text: "I have something to confess..."
├── Target Dialogue Index: -1
│   └── Purpose: Not used when Response is set
├── Response: ← NPC REACTS FIRST
│   ├── Speaker Name: "Village Priest"
│   │   └── Purpose: Who responds (can be different from main speaker)
│   ├── Response Text: "Tell me, my child. You are safe here."
│   │   └── Purpose: What NPC says in reaction
│   ├── Conversation Bubble Sprite: [Caring bubble sprite]
│   │   └── Purpose: Special bubble for this response
│   ├── Pause After Response: 2.0f
│   │   └── Purpose: Dramatic pause before continuing
│   ├── Continue To Next: ✓ Checked
│   │   └── Purpose: Continue dialogue after response
│   └── Next Dialogue Index: 8
│       └── Purpose: Jump to confession dialogue at entry [8]
├── Flags To Add: ["DECIDED_TO_CONFESS"]
└── Is Important Choice: ✓ Checked

Flow:
Player clicks → "I have something to confess..."
→ NPC responds: "Tell me, my child..."
→ Wait 2 seconds
→ Jump to dialogue entry [8] (confession scene)
```

### 5.3 Multiple Response Navigation Options

#### Option A: Continue to Next Entry in Sequence
```
Response Configuration:
├── Response Text: "Interesting..."
├── Continue To Next: ✓ Checked
├── Next Dialogue Index: -1 (default)
└── Purpose: Goes to dialogueEntries[currentIndex + 1]
```

#### Option B: Jump to Specific Entry
```
Response Configuration:
├── Response Text: "That changes everything!"
├── Continue To Next: ✓ Checked
├── Next Dialogue Index: 15
└── Purpose: Goes directly to dialogueEntries[15]
```

#### Option C: End Dialogue After Response
```
Response Configuration:
├── Response Text: "We'll speak no more of this."
├── Continue To Next: ✗ Unchecked
├── Next Dialogue Index: (ignored)
└── Purpose: Dialogue ends after response is shown
```

---

## Step 6: Chain Choice Dialogues

### 6.1 Simple Chain Example
Create a dialogue that flows through multiple choice points:

```
DialogueData Setup for Chain:
├── Dialogue Entries (Size: 6):
    ├── [0] Opening with choices
    ├── [1] Branch A with more choices
    ├── [2] Branch B with more choices
    ├── [3] Sub-choice from Branch A
    ├── [4] Sub-choice from Branch B
    └── [5] Conclusion entry
```

#### Entry [0] - Chain Starting Point
```
Dialogue Content:
├── Speaker Name: "Wise Elder"
├── Dialogue Text: "I can teach you about three paths of wisdom."

Choice System:
├── Has Choices: ✓ Checked
└── Choices (Size: 3):
    
    Choice [0] - Path of Knowledge:
    ├── Choice Text: "Tell me about the Path of Knowledge"
    ├── Target Dialogue Index: 1
    │   └── Purpose: Goes to knowledge explanation
    └── Flags To Add: ["INTERESTED_IN_KNOWLEDGE"]
    
    Choice [1] - Path of Strength:
    ├── Choice Text: "What is the Path of Strength?"
    ├── Target Dialogue Index: 2
    │   └── Purpose: Goes to strength explanation  
    └── Flags To Add: ["INTERESTED_IN_STRENGTH"]
    
    Choice [2] - I need time to think:
    ├── Choice Text: "I need more time to decide"
    ├── Target Dialogue Index: -1
    └── Purpose: Ends dialogue, preserves state for later
```

#### Entry [1] - Knowledge Path (Has More Choices!)
```
Dialogue Content:
├── Speaker Name: "Wise Elder"
├── Dialogue Text: "The Path of Knowledge requires dedication to learning and wisdom."

Choice System:
├── Has Choices: ✓ Checked ← CHAIN CONTINUES!
└── Choices (Size: 3):
    
    Choice [0] - Accept Path:
    ├── Choice Text: "I choose the Path of Knowledge"
    ├── Target Dialogue Index: 3
    ├── Flags To Add: ["CHOSE_KNOWLEDGE_PATH"]
    └── Is Important Choice: ✓ Checked
    
    Choice [1] - Ask More:
    ├── Choice Text: "What would I learn?"
    ├── Target Dialogue Index: 4
    └── Flags To Add: ["ASKED_ABOUT_KNOWLEDGE_DETAILS"]
    
    Choice [2] - Go Back:
    ├── Choice Text: "Tell me about the other paths"
    ├── Target Dialogue Index: 0
    └── Purpose: Returns to main menu!
```

### 6.2 Hub-and-Spoke Chain Pattern
Perfect for shop menus, information gathering, or quest hubs:

```
Entry [0] - Central Hub:
├── "What would you like to know about?"
└── Choices lead to [1], [2], [3], each with "Return to main menu" choice → [0]

Entry [1] - Information Branch:
├── "Here's what you need to know about topic A..."
└── Choices: ["Tell me more" → [4], "Ask about something else" → [0]]

Entry [2] - Quest Branch:
├── "I have work available..."  
└── Choices: ["Accept quest" → [5], "Decline" → [0], "Ask about reward" → [6]]

This creates a persistent menu system that players can navigate freely!
```

### 6.3 Progressive Chain with Memory
Chain that changes based on previous choices:

```
Entry [0] - Meeting Again:
├── Speaker: "Blacksmith"
├── Text: "You're back!"
└── Choices:
    ├── "How's business?" (always available) → [1]
    ├── "About that armor..." (requires: "DISCUSSED_ARMOR") → [2] 
    ├── "I brought the materials" (requires: "ACCEPTED_COMMISSION") → [3]
    └── "I have your payment" (requires: "ARMOR_COMPLETED") → [4]

Each return visit shows different options based on your history!
```

---

## Step 7: Complex Response Chains

### 7.1 Multi-Stage Response Chain
```
Entry [0] - Dramatic Revelation:
├── Text: "I have something shocking to tell you..."
└── Choice:
    ├── Text: "What is it?"
    ├── Response:
    │   ├── Text: "Your father... he's alive."
    │   ├── Pause: 3.0f ← Long pause for impact
    │   └── Next Dialogue Index: 1
    └── Flags To Add: ["LEARNED_FATHER_ALIVE"]

Entry [1] - Player's Reaction (More Choices!):
├── Text: "I can see you're shocked. What do you want to know?"
└── Choices:
    ├── "Where is he?" → Response → Entry [2]
    ├── "Why didn't you tell me?" → Response → Entry [3]  
    └── "I don't believe you!" → Response → Entry [4]
```

### 7.2 Conditional Response Content
```
Choice with Variable Response:
├── Choice Text: "How do you know me?"
├── Response (changes based on flags):
│   ├── IF player has "MET_BEFORE" flag:
│   │   └── Response Text: "We met at the tavern last week!"
│   ├── IF player has "FAMOUS_HERO" flag:
│   │   └── Response Text: "Everyone knows the great hero!"
│   └── Default:
│       └── Response Text: "Word travels fast in small towns."
└── Next Dialogue Index: 5

Note: This requires multiple DialogueChoice entries with different Required Flags
```

---

## Step 8: Field Reference Guide

### 8.1 DialogueEntry Fields Explained

```
Basic Content:
├── Speaker Name: Who is talking (shows in name UI)
├── Dialogue Text: What they say (typewriter effect)
├── Available Times Of Day: When this entry can appear
├── Is Repeatable: Can this entry be shown multiple times?
└── Required Flags: Flags needed for this entry to appear

Choice System:
├── Has Choices: ✓ = Show choices, ✗ = Show Continue/End buttons
└── Choices: Array of player choice options

Visual & Audio:
├── Conversation Bubble Sprite: Custom bubble for this entry
├── Is Important Dialogue: Special styling for key moments
└── Pause After Dialogue: Wait time before showing choices/buttons
```

### 8.2 DialogueChoice Fields Explained

```
Content:
└── Choice Text: Text shown on the choice button (keep under 60 chars)

Availability:
├── Required Flags: All flags must exist for choice to appear
├── Available Times Of Day: When this choice can be selected
└── Is Repeatable: Can this choice be selected multiple times?

Consequences:
├── Flags To Add: Flags added to game state when selected
└── Flags To Remove: Flags removed from game state when selected

Navigation (Pick ONE):
├── Target Dialogue Index: Jump directly to this entry (-1 = end dialogue)
└── Response: Show NPC response first, then navigate

Visual & Audio:
├── Is Important Choice: Bold text, special audio, yellow color
└── Choice Color: Button tint color
```

### 8.3 DialogueResponse Fields Explained

```
Content:
├── Speaker Name: Who responds (can be different from main speaker)
└── Response Text: What they say in reaction

Visual & Timing:
├── Conversation Bubble Sprite: Special bubble for this response
└── Pause After Response: Wait time before continuing

Navigation:
├── Continue To Next: ✓ = Continue dialogue, ✗ = End after response
└── Next Dialogue Index: Specific entry to jump to (-1 = next in sequence)
```

---

## Step 9: Testing Your Dialogues

### 9.1 Assign to NPC
1. Find an NPC in your scene
2. In the **NPC component**, locate **Dialogue Data** field
3. Assign your DialogueData asset

### 9.2 Test Flow Checklist
```
Basic Testing:
├── ✅ Dialogue starts when pressing E
├── ✅ Text types out correctly
├── ✅ Choice buttons appear after typing
├── ✅ Choice buttons have correct text
├── ✅ Clicking choices navigates correctly
├── ✅ Flags are added/removed as expected
└── ✅ Dialogue ends or continues appropriately

Chain Testing:
├── ✅ All navigation paths work
├── ✅ Return-to-menu choices function
├── ✅ Flag-dependent choices appear/disappear correctly
├── ✅ No dead-end conversations
└── ✅ Complex chains don't break

Advanced Testing:
├── ✅ Response system works with proper timing
├── ✅ Important choices have special styling/audio
├── ✅ Time-based restrictions function
├── ✅ Save/load preserves dialogue state
└── ✅ Multiple NPCs don't interfere with each other
```

### 9.3 Debug Tools
```
Unity Console Debugging:
├── Look for "[NPC MANAGER DEBUG]" messages
├── Check "[SCHEDULE DATA DEBUG]" for flag issues
├── Watch for null reference exceptions
└── Verify dialogue index out-of-bounds errors

Flag State Checking:
├── Use NPCInteractionSystem.GetGameFlags() in debugger
├── Check flag spelling and capitalization
├── Verify Required Flags vs Available Flags
└── Test flag addition/removal consequences
```

---

## Step 10: Common Issues & Solutions

### ❌ Choice buttons don't appear
```
Troubleshooting Checklist:
├── ✅ Choice Container assigned in NPCInteractionSystem?
├── ✅ Choice Button Prefab assigned in NPCInteractionSystem?
├── ✅ DialogueEntry has "Has Choices" checked?
├── ✅ Choices array has at least one entry?
├── ✅ Choice entries have Choice Text filled in?
├── ✅ Required Flags are present (if any)?
└── ✅ Time-based restrictions satisfied (if any)?
```

### ❌ Buttons appear but clicking does nothing
```
Solutions:
├── ✅ Check Target Dialogue Index values (-1 to 999 are valid)
├── ✅ Verify dialogue entries exist at target indices
├── ✅ Look for Unity Console errors
├── ✅ Check if flags prevent choice execution
├── ✅ Ensure button prefab has working Button component
└── ✅ Verify NPCInteractionSystem is active and enabled
```

### ❌ Chain dialogues break or loop infinitely
```
Solutions:
├── ✅ Map out your dialogue flow on paper first
├── ✅ Check for circular references (A→B→A→B...)
├── ✅ Ensure exit conditions exist (choices that end dialogue)
├── ✅ Verify all Target Dialogue Index values point to valid entries
├── ✅ Test all possible navigation paths
└── ✅ Add bounds checking for dialogue array access
```

### ❌ Flags not working correctly
```
Solutions:
├── ✅ Check flag spelling and capitalization exactly
├── ✅ Verify flags are added before they're checked
├── ✅ Use meaningful flag names (not "flag1", "temp", etc.)
├── ✅ Check Required Flags vs Flags To Add consistency
├── ✅ Test flag persistence through save/load
└── ✅ Use NPCInteractionSystem debug methods
```

### ❌ Response system not working
```
Solutions:
├── ✅ Ensure Target Dialogue Index is -1 when using Response
├── ✅ Check Response Text is not empty
├── ✅ Verify Continue To Next setting matches intent
├── ✅ Check Next Dialogue Index is valid (if specified)
├── ✅ Test pause timing (not too long/short)
└── ✅ Verify speaker names are correct
```

---

## Step 11: Design Best Practices

### 11.1 Choice Design Guidelines
```
Writing Effective Choices:
├── Keep choice text under 60 characters for UI layout
├── Make choices feel distinct and meaningful
├── Use active voice: "I'll help you" not "You will be helped"
├── Indicate tone: [Aggressive] "Back off!" or [Polite] "Excuse me"
├── Preview consequences when appropriate: "This might be dangerous..."
└── Avoid obvious "correct" answers unless intentional
```

### 11.2 Flag Management Strategy
```
Flag Naming Convention:
├── Story Progress: "CHAPTER_1_COMPLETE", "QUEST_MAIN_STARTED"
├── Character Relations: "LIKES_SARAH", "TRUSTS_MARCUS", "ENEMY_OF_BANDITS"
├── Player Knowledge: "KNOWS_SECRET", "LEARNED_PASSWORD", "DISCOVERED_PLOT"
├── World State: "BRIDGE_REPAIRED", "FESTIVAL_ACTIVE", "WINTER_SEASON"
└── Choices Made: "CHOSE_DIPLOMATIC", "HELPED_BLACKSMITH", "REJECTED_OFFER"

Flag Cleanup:
├── Remove flags when no longer needed
├── Use flag removal to reset states
├── Group related flags with common prefixes
└── Document flag dependencies for team members
```

### 11.3 Dialogue Flow Design
```
Conversation Structure:
├── Opening: Hook the player's interest
├── Body: Present meaningful choices
├── Branching: Each path should feel distinct
├── Consequences: Choices should matter
└── Closing: Satisfying resolution or cliffhanger

Chain Design Principles:
├── Provide clear navigation options
├── Always offer a way back or out
├── Don't make chains too deep (3-4 levels max usually)
├── Test all paths to ensure they work
└── Consider player fatigue in long conversations
```

### 11.4 Technical Performance
```
Optimization Tips:
├── Limit choices to 6 or fewer for UI readability
├── Use Required Flags sparingly (each check has cost)
├── Cache frequently used components
├── Test on target hardware for performance
└── Use object pooling for frequently spawned choice buttons

Content Organization:
├── Group related dialogue entries together
├── Use consistent dialogue index numbering
├── Document complex chains with flowcharts
├── Keep DialogueData assets organized in folders
└── Name assets descriptively: "MerchantShop_Dialogue", "GuardGate_Dialogue"
```

---

## 🎉 Mastery Complete!

You now have complete knowledge of the choice-based dialogue system! You can create:

- ✅ **Basic choice dialogues** with simple branching
- ✅ **Advanced conditional choices** using flags and time
- ✅ **Response-based interactions** with NPC reactions
- ✅ **Complex chain dialogues** with multiple decision points
- ✅ **Hub-and-spoke menus** for persistent interaction systems
- ✅ **Flag-driven story progression** that remembers player choices

### Next Steps:
1. **Start Simple**: Create basic choice dialogues first
2. **Add Complexity Gradually**: Introduce flags and responses
3. **Build Chains**: Connect multiple choice points
4. **Test Thoroughly**: Verify all paths work correctly
5. **Polish Experience**: Add audio, visual feedback, and animations

Your dialogue system can now rival the complexity of major RPGs while remaining easy to create and maintain! 🎭✨

---

**Remember**: The key to great dialogue is not just the technical implementation, but meaningful choices that make players feel their decisions matter in your game world.