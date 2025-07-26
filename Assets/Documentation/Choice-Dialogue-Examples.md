# Choice-Based Dialogue Examples & Patterns

## 🎭 Common Dialogue Patterns

This guide provides practical examples of different choice dialogue patterns you can implement.

---

## Basic Choice Patterns

### 1. Simple Branching Dialogue

**Use Case:** Basic conversation with different outcomes

```
DialogueData Configuration:
├── Entry [0] - Greeting with choices
├── Entry [1] - Friendly response path
├── Entry [2] - Neutral response path  
└── Entry [3] - Hostile response path
```

#### Entry [0] - Main Choice Entry
```
Speaker: "Village Merchant"
Text: "Welcome to my shop! How can I help you today?"
Has Choices: ✓

Choices:
├── [0] "I'd like to browse your wares"
│   ├── Target Index: 1
│   └── Flags to Add: ["BROWSING_SHOP"]
├── [1] "Just looking around"  
│   ├── Target Index: 2
│   └── Color: Light Gray
└── [2] "Your prices are too high!"
    ├── Target Index: 3  
    ├── Flags to Add: ["COMPLAINED_ABOUT_PRICES"]
    └── Color: Red
```

### 2. Information Gathering

**Use Case:** Let player ask multiple questions

```
DialogueData Configuration:
├── Entry [0] - Main hub with choices
├── Entry [1] - About the town
├── Entry [2] - About current events
├── Entry [3] - About rumors
└── Entry [4] - Farewell
```

#### Entry [0] - Question Hub
```
Speaker: "Town Elder"  
Text: "I have lived here for many years. What would you like to know?"
Has Choices: ✓

Choices:
├── [0] "Tell me about this town"
│   ├── Target Index: 1
│   └── Is Repeatable: ✓
├── [1] "What's happening lately?"
│   ├── Target Index: 2
│   └── Is Repeatable: ✓
├── [2] "Any interesting rumors?"
│   ├── Target Index: 3
│   ├── Required Flags: ["GAINED_TRUST"]
│   └── Is Important: ✓
└── [3] "That's all, thank you"
    └── Target Index: 4
```

#### Entry [1] - Town Info (Returns to Hub)
```
Speaker: "Town Elder"
Text: "Our town was founded 200 years ago by brave settlers..."
Has Choices: ✓

Choices:
└── [0] "I see, what else can you tell me?"
    └── Target Index: 0  // Returns to main hub
```

---

## Advanced Choice Patterns

### 3. Flag-Dependent Choices

**Use Case:** Choices that appear based on player actions or story progress

```
Entry [0] - Context-Sensitive Dialogue
Speaker: "Captain of Guards"
Text: "Ah, you're back. How did things go?"
Has Choices: ✓

Choices:
├── [0] "The mission was successful" (ALWAYS AVAILABLE)
│   ├── Target Index: 1
│   └── Flags to Add: ["REPORTED_SUCCESS"]
├── [1] "I found evidence of corruption" (CONDITIONAL)
│   ├── Target Index: 2
│   ├── Required Flags: ["FOUND_EVIDENCE", "COMPLETED_INVESTIGATION"]
│   ├── Flags to Add: ["REPORTED_CORRUPTION"]
│   └── Is Important: ✓
├── [2] "I need to report a betrayal" (CONDITIONAL)  
│   ├── Target Index: 3
│   ├── Required Flags: ["WITNESSED_BETRAYAL"]
│   ├── Flags to Add: ["REPORTED_BETRAYAL"]
│   ├── Color: Dark Red
│   └── Is Important: ✓
└── [3] "I'd rather not say" (ALWAYS AVAILABLE)
    ├── Target Index: 4
    └── Flags to Add: ["KEPT_SECRETS"]
```

### 4. Time-Based Choices

**Use Case:** Different options available at different times

```
Entry [0] - Time-Sensitive Meeting
Speaker: "Mysterious Stranger"
Text: "Perfect timing. I have a proposition for you..."
Has Choices: ✓

Choices:
├── [0] "I'm listening" (DAY/SUNSET ONLY)
│   ├── Available Times: [Day, Sunset]
│   ├── Target Index: 1
│   └── Flags to Add: ["HEARD_PROPOSITION"]
├── [1] "Let's discuss this privately" (NIGHT ONLY)
│   ├── Available Times: [Night]
│   ├── Target Index: 2
│   ├── Flags to Add: ["SECRET_MEETING"]
│   └── Is Important: ✓
└── [2] "Not interested" (ALWAYS AVAILABLE)
    ├── Target Index: 3
    └── Color: Gray
```

### 5. Skill/Stat Checks (Using Flags)

**Use Case:** Choices that require character progression or abilities

```
Entry [0] - Locked Door Encounter
Speaker: "System"
Text: "You encounter a heavy wooden door with an intricate lock."
Has Choices: ✓

Choices:
├── [0] "Pick the lock" (REQUIRES SKILL)
│   ├── Required Flags: ["LOCKPICKING_SKILL", "HAS_LOCKPICKS"]
│   ├── Target Index: 1
│   ├── Flags to Add: ["PICKED_LOCK"]
│   └── Is Important: ✓
├── [1] "Force the door open" (REQUIRES STRENGTH)
│   ├── Required Flags: ["HIGH_STRENGTH"]
│   ├── Target Index: 2
│   ├── Flags to Add: ["FORCED_DOOR", "MADE_NOISE"]
│   └── Color: Orange
├── [2] "Look for another way" (ALWAYS AVAILABLE)
│   ├── Target Index: 3
│   └── Flags to Add: ["LOOKING_FOR_ALTERNATIVES"]
└── [3] "Leave this place" (ALWAYS AVAILABLE)
    └── Target Index: -1  // End dialogue
```

---

## Complex Dialogue Trees

### 6. Multi-Stage Conversation

**Use Case:** Deep branching conversation with multiple decision points

```
DialogueData Configuration:
├── Entry [0] - Initial meeting
├── Entry [1] - Agreed to help branch
├── Entry [2] - Declined to help branch
├── Entry [3] - Negotiate payment (from Entry 1)
├── Entry [4] - Accept immediately (from Entry 1)
├── Entry [5] - High payment route (from Entry 3)
├── Entry [6] - Standard payment route (from Entry 3)
└── Entry [7] - Final confirmation
```

#### Entry [0] - Initial Offer
```
Speaker: "Desperate Merchant"
Text: "Please, I need someone to retrieve my stolen goods! Bandits took everything!"
Has Choices: ✓

Choices:
├── [0] "I'll help you"
│   ├── Target Index: 1
│   └── Flags to Add: ["AGREED_TO_HELP"]
└── [1] "That's not my problem"
    ├── Target Index: 2
    ├── Flags to Add: ["DECLINED_HELP"]
    └── Color: Red
```

#### Entry [1] - Payment Discussion
```
Speaker: "Desperate Merchant"
Text: "Bless you! Now, about payment... I can offer you a reward."
Has Choices: ✓

Choices:
├── [0] "Let's discuss the payment first"
│   ├── Target Index: 3
│   └── Flags to Add: ["NEGOTIATING_PAYMENT"]
└── [1] "Don't worry about payment, I'll help"
    ├── Target Index: 4
    ├── Flags to Add: ["HELPING_FOR_FREE"]
    └── Is Important: ✓
```

#### Entry [3] - Negotiation
```
Speaker: "Desperate Merchant"  
Text: "I can offer 50 gold pieces, or perhaps some rare items..."
Has Choices: ✓

Choices:
├── [0] "I want 100 gold pieces"
│   ├── Target Index: 5
│   ├── Required Flags: ["NEGOTIATING_PAYMENT"]
│   └── Flags to Add: ["DEMANDED_HIGH_PAYMENT"]
├── [1] "50 gold sounds fair"
│   ├── Target Index: 6
│   └── Flags to Add: ["ACCEPTED_STANDARD_PAYMENT"]
└── [2] "What kind of rare items?"
    ├── Target Index: 7
    └── Flags to Add: ["INTERESTED_IN_ITEMS"]
```

---

## Response-Based Patterns

### 7. Choice with Custom Response

**Use Case:** NPC reacts specifically to choice before continuing

```
Entry [0] - Moral Dilemma
Speaker: "Wounded Bandit"
Text: "Please... don't turn me in. I only stole to feed my family."
Has Choices: ✓

Choice with Response:
├── Choice Text: "I'll give you a second chance"
├── Response:
│   ├── Speaker: "Wounded Bandit"
│   ├── Response Text: "*tears of relief* Thank you! I won't forget this kindness."
│   ├── Pause After: 2.0f
│   ├── Continue To Next: ✓
│   └── Next Dialogue Index: 1
├── Flags to Add: ["SHOWED_MERCY"]
└── Is Important: ✓
```

### 8. Delayed Consequences

**Use Case:** Choice consequences that affect later conversations

```
Entry [0] - First Meeting
Choice: "I don't trust you"
├── Flags to Add: ["DISTRUSTS_MARCUS"]
└── Target Index: 1

// Later in game, different dialogue:
Entry [15] - Later Meeting  
Speaker: "Marcus"
Text: "You again... I remember you didn't trust me before."
Required Flags: ["DISTRUSTS_MARCUS"]
```

---

## UI and UX Patterns

### 9. Visual Choice Styling

```
Important Choices (Quest-related):
├── Is Important Choice: ✓
├── Choice Color: Gold
└── Font Style: Bold

Negative Choices (Rude/Hostile):
├── Choice Color: Red (#FF6B6B)
└── Font Color: Dark Red

Skill-based Choices:
├── Choice Color: Blue (#4ECDC4)
├── Required Flags: ["SKILL_NAME"]
└── Choice Text: "[SKILL] Attempt something"

Unavailable Choices (for reference):
├── Required Flags: ["MISSING_FLAG"]
├── Choice Color: Gray
└── Interactable: ✗ (handled automatically)
```

### 10. Audio Patterns

```
Audio Configuration Examples:

Regular Choices:
├── Choice Select Sound: "button_click.wav"
└── Choice Hover Sound: "button_hover.wav"

Important Choices:
├── Important Choice Sound: "important_decision.wav"
└── Is Important Choice: ✓

Quest Choices:
├── Important Choice Sound: "quest_accepted.wav"  
├── Flags to Add: ["QUEST_STARTED"]
└── Is Important Choice: ✓
```

---

## Best Practices

### Choice Text Guidelines
- **Keep it under 60 characters** for proper button display
- **Use active voice**: "I'll help you" not "You will be helped"
- **Make choices distinct** - avoid similar options
- **Indicate consequences** when appropriate: "[Lie] I didn't see anything"

### Flag Naming Conventions
```
Quest Flags:
├── QUEST_[NAME]_STARTED
├── QUEST_[NAME]_COMPLETED  
└── QUEST_[NAME]_FAILED

Character Relationship:
├── LIKES_[CHARACTER]
├── TRUSTS_[CHARACTER]
└── ENEMY_OF_[CHARACTER]

Story Progress:
├── KNOWS_[SECRET]
├── VISITED_[LOCATION]
└── COMPLETED_[EVENT]
```

### Performance Tips
- **Limit choices to 6 or fewer** for UI readability
- **Use flags sparingly** - only when needed for branching
- **Test all dialogue paths** to ensure they work
- **Keep response text concise** to maintain pacing

---

## 🎯 Quick Reference

### Common Target Index Values
```
-1  = End dialogue immediately
0   = Return to first entry (dialogue loop)
1+  = Go to specific dialogue entry
```

### Flag Best Practices
```
✅ Good: "COMPLETED_TUTORIAL"
❌ Bad: "tutorial_complete_flag_1"

✅ Good: "HOSTILE_TO_GUARDS" 
❌ Bad: "angry_at_guard_captain_because_of_incident"
```

### Choice Color Suggestions
```
Default/Neutral: White
Positive/Helpful: Light Green (#90EE90)
Negative/Rude: Light Red (#FF6B6B)
Important/Quest: Gold (#FFD700)
Skill-based: Light Blue (#87CEEB)
Unavailable: Gray (handled automatically)
```

---

These examples should give you a solid foundation for creating engaging, branching dialogues that take full advantage of the choice system's capabilities!