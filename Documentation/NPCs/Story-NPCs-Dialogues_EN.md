# Story NPCs Dialogue Data

This document contains comprehensive dialogue data for all story-essential NPCs in the Trenggalek folklore game.

## Story NPCs Overview

### Primary Story Characters
- **Ki Ageng Sinawang** - Mentor and padepokan leader
- **Raden Ayu Saraswati** - Menak Sopal's mother
- **Mbok Randa Krandon** - White elephant owner, key antagonist
- **Buaya Putih** - White crocodile spirit (mystical boss)

### Supporting Story Characters
- **Murid Padepokan 1-3** - Fellow students
- **Warga Krandon 1-5** - Pursuing villagers
- **Pemandu Jalan** - Guide to Desa Krandon
- **Warga Haus 1-4** - Thirsty villagers at the well

---

## Ki Ageng Sinawang (Mentor)

**NPC ID:** `ki_ageng_sinawang`
**Role:** Padepokan leader, Menak Sopal's spiritual teacher

### Dialogue Entries

#### Initial Greeting (Pre-Story)
```yaml
speakerName: "Ki Ageng Sinawang"
dialogueText: "Ah, Menak Sopal. I sense your heart is restless today. The winds speak of change coming to our land."
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
isRepeatable: true
```

#### Story Phase 1 - After Water Crisis Discovery
```yaml
speakerName: "Ki Ageng Sinawang"
dialogueText: "The suffering of our people weighs heavily on your heart, my son. Sometimes the greatest acts of service require great sacrifice."
availableTimesOfDay: [Morning, Afternoon, Evening]
requiredFlags: ["water_crisis_discovered"]
hasChoices: true
choices:
  - choiceText: "Guru, I wish to help solve the water shortage"
    flagsToAdd: ["asked_permission_water_project"]
    response:
      speakerName: "Ki Ageng Sinawang"
      responseText: "Your compassion honors our teachings. Go forth, but remember - true wisdom lies in understanding all consequences of our actions."
  - choiceText: "What do you think I should do?"
    response:
      speakerName: "Ki Ageng Sinawang"
      responseText: "The answer lies within you, child. Listen to your heart, but temper it with wisdom. The path of a helper is never simple."
```

#### Story Phase 2 - Dam Building Assistance
```yaml
speakerName: "Ki Ageng Sinawang"
dialogueText: "Take some of our students to assist you. Young hands working together can move mountains - or in this case, build rivers."
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: ["dam_construction_started"]
choices:
  - choiceText: "Thank you, Guru. Your wisdom guides me"
    flagsToAdd: ["students_permission_granted"]
    questToStart: "gather_construction_helpers"
```

#### Story Phase 3 - Mystical Consultation
```yaml
speakerName: "Ki Ageng Sinawang"
dialogueText: "I sense dark spiritual forces at work. The river spirits are ancient and proud. They do not take kindly to uninvited construction."
availableTimesOfDay: [Evening, Night]
requiredFlags: ["dam_repeatedly_destroyed"]
choices:
  - choiceText: "How can I appease the river spirits?"
    response:
      speakerName: "Ki Ageng Sinawang"
      responseText: "Spirits often demand tribute or respect. Seek communication first, young one. Violence should be the last resort."
  - choiceText: "Is there danger in confronting these spirits?"
    response:
      speakerName: "Ki Ageng Sinawang" 
      responseText: "All spiritual dealings carry risk. But your pure intentions may protect you. Trust in your training."
```

#### Story Phase 4 - White Elephant Dilemma
```yaml
speakerName: "Ki Ageng Sinawang"
dialogueText: "Mbok Randa Krandon is good-hearted, despite her temper. She will understand if you explain the greater good your actions serve."
availableTimesOfDay: [Morning, Afternoon, Evening]
requiredFlags: ["white_elephant_taken", "mbok_randa_angry"]
choices:
  - choiceText: "She's furious with me. How can I make this right?"
    flagsToAdd: ["guru_advice_reconciliation"]
    response:
      speakerName: "Ki Ageng Sinawang"
      responseText: "Truth spoken with genuine remorse can heal many wounds. Show her the good that came from your actions."
```

#### Story Conclusion - Wisdom Reflection
```yaml
speakerName: "Ki Ageng Sinawang"
dialogueText: "You have learned that even noble intentions can cause pain. But from this pain, understanding grows. The village now has water, and you have wisdom."
availableTimesOfDay: [Any]
requiredFlags: ["story_completed", "reconciliation_complete"]
isRepeatable: true
```

### Casual Dialogues (Non-story times)

#### Daily Wisdom
```yaml
speakerName: "Ki Ageng Sinawang"
dialogueText: "The mountains teach us patience, the rivers teach us persistence. What lesson will you choose to learn today?"
availableTimesOfDay: [Morning]
isRepeatable: true
```

#### Meditation Guidance
```yaml
speakerName: "Ki Ageng Sinawang"
dialogueText: "Sit quietly by the old banyan tree when the sun sets. Listen to what the wind tells you about tomorrow."
availableTimesOfDay: [Afternoon]
isRepeatable: true
```

---

## Raden Ayu Saraswati (Mother)

**NPC ID:** `raden_ayu_saraswati`
**Role:** Menak Sopal's mother, supportive maternal figure

### Dialogue Entries

#### Morning Blessing
```yaml
speakerName: "Raden Ayu Saraswati"
dialogueText: "Good morning, my dear son. I dreamed of flowing water last night. Perhaps it's a sign of good fortune coming."
availableTimesOfDay: [Morning]
requiredFlags: []
isRepeatable: true
```

#### Story Phase - Motherly Concern
```yaml
speakerName: "Raden Ayu Saraswati"
dialogueText: "I worry about this dam project of yours, anak. The spirits of the river are not to be taken lightly."
availableTimesOfDay: [Evening]
requiredFlags: ["dam_construction_started"]
choices:
  - choiceText: "Don't worry, Mother. I'll be careful"
    response:
      speakerName: "Raden Ayu Saraswati"
      responseText: "Your father had the same determined spirit. Just remember, courage without wisdom is recklessness."
  - choiceText: "Have you seen omens about the river?"
    response:
      speakerName: "Raden Ayu Saraswati"
      responseText: "The birds have been restless near the water. And your birth light flickered last night - something stirs in the spiritual realm."
```

#### Story Phase - Crisis Support
```yaml
speakerName: "Raden Ayu Saraswati"
dialogueText: "When troubles seem overwhelming, remember that every storm passes. Your good heart will find a way through this."
availableTimesOfDay: [Any]
requiredFlags: ["dam_repeatedly_destroyed"]
```

#### Story Phase - Mbok Randa Conflict
```yaml
speakerName: "Raden Ayu Saraswati"
dialogueText: "Mbok Randa is here, and she's quite upset. But I sense her anger comes from hurt, not hatred. Be gentle with her."
availableTimesOfDay: [Any]
requiredFlags: ["mbok_randa_visits_padepokan"]
choices:
  - choiceText: "What should I say to her?"
    response:
      speakerName: "Raden Ayu Saraswati"
      responseText: "Speak from your heart. Tell her why you did what you did. Sometimes understanding is all someone needs."
```

#### Story Conclusion - Proud Mother
```yaml
speakerName: "Raden Ayu Saraswati"
dialogueText: "My son has become a true man today. Not because he solved a problem, but because he learned to face the consequences of his choices."
availableTimesOfDay: [Any]
requiredFlags: ["story_completed"]
isRepeatable: true
```

#### Story Aftermath - White Crocodile Rescue
```yaml
speakerName: "Raden Ayu Saraswati"
dialogueText: "Do not worry about what happened in the river. The white crocodile is ancient and wise - it would not let harm come to someone pure of heart."
availableTimesOfDay: [Any]
requiredFlags: ["rescued_by_crocodile"]
```

### Casual Dialogues

#### Evening Care
```yaml
speakerName: "Raden Ayu Saraswati"
dialogueText: "Have you eaten enough today? A mother always worries that her child isn't eating properly."
availableTimesOfDay: [Evening]
isRepeatable: true
```

#### Herbal Wisdom
```yaml
speakerName: "Raden Ayu Saraswati"
dialogueText: "I'm preparing healing herbs for the village. The lemongrass by the river grows especially well this season."
availableTimesOfDay: [Afternoon]
isRepeatable: true
```

---

## Mbok Randa Krandon (Antagonist)

**NPC ID:** `mbok_randa_krandon`
**Role:** White elephant owner, represents conflict and eventual understanding

### Dialogue Entries

#### First Meeting - Suspicious Welcome
```yaml
speakerName: "Mbok Randa Krandon"
dialogueText: "A young man from the padepokan? What brings you so far from home, child?"
availableTimesOfDay: [Any]
requiredFlags: ["arrived_desa_krandon"]
choices:
  - choiceText: "I come seeking your white elephant, Mbok"
    flagsToAdd: ["requested_elephant_directly"]
    response:
      speakerName: "Mbok Randa Krandon"
      responseText: "My elephant? That's a strange request. Why would a padepokan student need my precious elephant?"
  - choiceText: "I come with greetings from Ki Ageng Sinawang"
    response:
      speakerName: "Mbok Randa Krandon"
      responseText: "Ah, Ki Ageng! I knew him when he was just a young teacher. A good man. What does he need?"
```

#### The Negotiation
```yaml
speakerName: "Mbok Randa Krandon"
dialogueText: "You want to borrow my white elephant? For three days? That's quite unusual... but Ki Ageng vouches for you."
availableTimesOfDay: [Any]
requiredFlags: ["explained_water_crisis"]
choices:
  - choiceText: "I promise to return her safely"
    flagsToAdd: ["promised_safe_return"]
    response:
      speakerName: "Mbok Randa Krandon"
      responseText: "Very well. But if any harm comes to her, your padepokan will answer for it. Three days, no more."
  - choiceText: "What if something happens to the elephant?"
    response:
      speakerName: "Mbok Randa Krandon"
      responseText: "Then you'll have made a very powerful enemy. But... I trust Ki Ageng's judgment of character."
```

#### The Betrayal Discovery
```yaml
speakerName: "Mbok Randa Krandon"
dialogueText: "YOU! You lied to me! Where is my white elephant? What have you done with her?"
availableTimesOfDay: [Any]
requiredFlags: ["elephant_sacrifice_revealed"]
isImportantDialogue: true
choices:
  - choiceText: "I can explain everything..."
    flagsToAdd: ["attempted_explanation"]
    response:
      speakerName: "Mbok Randa Krandon"
      responseText: "Explain? EXPLAIN?! You took my beloved elephant and... and... I should have never trusted a padepokan student!"
  - choiceText: "It was for the good of many people"
    flagsToAdd: ["justified_actions"]
    response:
      speakerName: "Mbok Randa Krandon"
      responseText: "The good of many? What about MY loss? What about MY pain? Seize him! Don't let him escape!"
```

#### At the Padepokan - Truth Revealed
```yaml
speakerName: "Mbok Randa Krandon"
dialogueText: "Ki Ageng, your student has betrayed my trust! He took my elephant under false pretenses!"
availableTimesOfDay: [Any]
requiredFlags: ["confronted_at_padepokan"]
choices:
  - choiceText: "Please let me explain the whole truth"
    targetDialogueIndex: 1  # Continue to explanation
```

#### Understanding and Forgiveness
```yaml
speakerName: "Mbok Randa Krandon"
dialogueText: "So... my elephant's sacrifice brought water to your village? And saved many from suffering?"
availableTimesOfDay: [Any]
requiredFlags: ["full_truth_explained"]
choices:
  - choiceText: "Yes, Mbok. And I'm truly sorry for deceiving you"
    flagsToAdd: ["sincere_apology_given"]
    response:
      speakerName: "Mbok Randa Krandon"
      responseText: "Your remorse seems genuine. And if my elephant's sacrifice helped so many... then perhaps her death had noble purpose."
  - choiceText: "Will you forgive me?"
    response:
      speakerName: "Mbok Randa Krandon"
      responseText: "Forgiveness is easier when understanding comes first. I forgive you, child. But the pain of loss remains."
```

#### Reconciliation Complete
```yaml
speakerName: "Mbok Randa Krandon"
dialogueText: "If this land prospers from my elephant's sacrifice, then let it be called 'Teranging Galih' - the brightness of understanding."
availableTimesOfDay: [Any]
requiredFlags: ["reconciliation_complete"]
flagsToAdd: ["teranging_galih_named"]
isImportantDialogue: true
```

### Post-Story Dialogues

#### Peaceful Reflection
```yaml
speakerName: "Mbok Randa Krandon"
dialogueText: "I still miss my elephant, but I see children playing by the river again. That brings some comfort to this old heart."
availableTimesOfDay: [Any]
requiredFlags: ["story_completed"]
isRepeatable: true
```

---

## Buaya Putih (White Crocodile Spirit)

**NPC ID:** `buaya_putih_spirit`
**Role:** Mystical guardian, represents nature's demands and eventual cooperation

### Dialogue Entries

#### First Spiritual Contact
```yaml
speakerName: "Buaya Putih"
dialogueText: "Who dares disturb the ancient waters without seeking permission from its guardian?"
availableTimesOfDay: [Any]
requiredFlags: ["spiritual_vision_active"]
isImportantDialogue: true
choices:
  - choiceText: "I am Menak Sopal. I seek to help my people"
    response:
      speakerName: "Buaya Putih"
      responseText: "Help? By building dams in MY river? Your intentions may be pure, but your methods show disrespect."
  - choiceText: "Great spirit, I meant no offense"
    flagsToAdd: ["showed_respect_to_spirit"]
    response:
      speakerName: "Buaya Putih"
      responseText: "Respect is shown through actions, not words. You build without asking, take without giving."
```

#### The Demand
```yaml
speakerName: "Buaya Putih"
dialogueText: "If you wish your dam to stand, you must offer proper tribute. Bring me the head of the white elephant, and I shall cease my destruction."
availableTimesOfDay: [Any]
requiredFlags: ["first_contact_complete"]
isImportantDialogue: true
choices:
  - choiceText: "Why do you require such a sacrifice?"
    response:
      speakerName: "Buaya Putih"
      responseText: "The white elephant is sacred, as am I. Only sacred tribute can balance the cosmic order you've disturbed."
  - choiceText: "There must be another way"
    response:
      speakerName: "Buaya Putih"
      responseText: "There is no other way. The ancient laws demand balance. Disturb the water, pay the price."
  - choiceText: "I will find this white elephant"
    flagsToAdd: ["accepted_spirit_demand"]
    questToStart: "find_white_elephant"
```

#### After Sacrifice
```yaml
speakerName: "Buaya Putih"
dialogueText: "The tribute is acceptable. Your dam shall stand, and the waters shall flow as needed. The balance is restored."
availableTimesOfDay: [Any]
requiredFlags: ["elephant_sacrifice_complete"]
isImportantDialogue: true
flagsToAdd: ["spirit_pact_complete"]
```

#### The Rescue
```yaml
speakerName: "Buaya Putih"
dialogueText: "Young one who honored the ancient ways, I shall not let you drown. Your pure heart has earned my protection."
availableTimesOfDay: [Any]
requiredFlags: ["drowning_in_river"]
isImportantDialogue: true
flagsToAdd: ["rescued_by_crocodile"]
```

#### Final Understanding
```yaml
speakerName: "Buaya Putih"
dialogueText: "Remember this lesson: Nature gives freely to those who approach with respect, but demands payment from those who take without asking."
availableTimesOfDay: [Any]
requiredFlags: ["rescued_by_crocodile"]
isImportantDialogue: true
```

---

## Supporting Characters

### Murid Padepokan (Students)

#### Murid Padepokan 1
**NPC ID:** `murid_padepokan_1`

```yaml
speakerName: "Andi (Padepokan Student)"
dialogueText: "Menak Sopal! I heard about your dam project. Can we help? We're strong and eager to serve the community!"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: ["dam_construction_started"]
choices:
  - choiceText: "Yes, I need help carrying stones and wood"
    flagsToAdd: ["student_helpers_recruited"]
    questToStart: "gather_construction_materials"
```

#### Murid Padepokan 2
**NPC ID:** `murid_padepokan_2`

```yaml
speakerName: "Budi (Padepokan Student)"
dialogueText: "This dam keeps breaking! There's something unnatural about it. I saw strange ripples in the water during the collapse."
availableTimesOfDay: [Any]
requiredFlags: ["dam_repeatedly_destroyed"]
```

#### Murid Padepokan 3
**NPC ID:** `murid_padepokan_3`

```yaml
speakerName: "Candra (Padepokan Student)"
dialogueText: "Senior Menak, we believe in your vision. If you say this dam will help people, then we'll work day and night to build it!"
availableTimesOfDay: [Any]
requiredFlags: ["students_permission_granted"]
```

### Warga Haus (Thirsty Villagers)

#### Warga Haus 1
**NPC ID:** `warga_haus_1`

```yaml
speakerName: "Pak Darmo"
dialogueText: "Please, young man! My children haven't had clean water in days! This well is nearly dry!"
availableTimesOfDay: [Any]
requiredFlags: []
flagsToAdd: ["water_crisis_discovered"]
isImportantDialogue: true
```

#### Warga Haus 2
**NPC ID:** `warga_haus_2`

```yaml
speakerName: "Bu Siti"
dialogueText: "We've been fighting over these last few drops! This isn't right! We're neighbors, not enemies!"
availableTimesOfDay: [Any]
requiredFlags: ["water_crisis_discovered"]
```

### Warga Krandon (Pursuing Villagers)

#### Warga Krandon 1
**NPC ID:** `warga_krandon_1`

```yaml
speakerName: "Pak Gunawan"
dialogueText: "There! That's the young man who stole Mbok Randa's elephant! Don't let him escape!"
availableTimesOfDay: [Any]
requiredFlags: ["chase_sequence_active"]
isImportantDialogue: true
```

### Pemandu Jalan (Guide)

**NPC ID:** `pemandu_jalan`

```yaml
speakerName: "Joko (Village Guide)"
dialogueText: "I know the way to Desa Krandon, Senior. It's a two-day walk through the forest. I'll guide you safely there."
availableTimesOfDay: [Any]
requiredFlags: ["seeking_white_elephant"]
choices:
  - choiceText: "Please guide me to Mbok Randa's house"
    flagsToAdd: ["guide_hired"]
    questToStart: "journey_to_krandon"
```

---

## Implementation Notes

### Flag Dependencies
All dialogues are designed to work with your existing flag system:
- Flags trigger story progression
- Multiple dialogue paths based on player choices
- Repeatable casual dialogues for immersion

### Time of Day Integration
- Morning: Formal greetings, planning discussions
- Afternoon: Work-related conversations
- Evening: Reflection, wisdom sharing
- Night: Mystical/spiritual encounters

### Choice Consequences
- Choices affect flag states and story progression
- Multiple dialogue branches for different player approaches
- Some choices lock/unlock future conversation options

### Quest Integration
- Dialogues integrate with your QuestData system
- NPCs can start, complete, or update quest objectives
- Story-critical quests triggered by key dialogues