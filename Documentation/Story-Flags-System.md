# Story Flags System Documentation

This document provides a comprehensive reference for all story flags used in the Trenggalek folklore game, their purposes, dependencies, and implementation guidelines.

## Flag System Overview

The story flags system serves as the backbone for:
- **Story Progression** - Tracking major narrative milestones
- **Dialogue Availability** - Controlling when specific conversations are accessible
- **Quest Flow** - Managing quest prerequisites and completion states
- **Character Development** - Recording player choices and relationship changes
- **World State** - Maintaining persistent changes to the game world

### Document Navigation
**Related Documents:**
- [Story Progression Guide](Story-Progression.md) - Complete narrative flow and quest integration
- [Chapter Progression Guide](Chapter-Progression.md) - Chapter structure and character development
- [Story NPCs Dialogues (Indonesian)](NPCs/Story-NPCs-Dialogues_ID.md) | [English](NPCs/Story-NPCs-Dialogues_EN.md)
- [Village NPCs Dialogues (Indonesian)](NPCs/Village-NPCs-Dialogues_ID.md) | [English](NPCs/Village-NPCs-Dialogues_EN.md)

---

## Main Story Progression Flags

### Phase 1: Discovery & Commitment

#### `story_started`
**Type:** Core Progression
**Set By:** Game initialization
**Purpose:** Indicates the player has begun the main story
**Dependencies:** None
**Unlocks:** Initial NPC interactions, world exploration

#### `water_crisis_discovered`
**Type:** Core Progression
**Set By:** Dialogue with Warga Haus 1 (villager at well)
**Purpose:** Player becomes aware of the village's water shortage
**Dependencies:** `story_started`
**Dialogue References:**
- [Pak Darmo - Crisis discovery](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `warga_haus_1`)
- [Bu Siti - Community conflict](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `warga_haus_2`)
**Unlocks:** 
- Crisis-related dialogue options with all NPCs
- Quest: `seek_guru_guidance`
- Modified NPC schedules reflecting crisis

#### `committed_to_help`
**Type:** Player Choice
**Set By:** Choosing "I want to help solve this water problem"
**Purpose:** Player makes moral commitment to assistance
**Dependencies:** `water_crisis_discovered`
**Alternative:** `avoided_responsibility`
**Dialogue References:**
- Choice available in all Warga Haus conversations (Village NPCs)
**Unlocks:** Positive reputation with villagers, certain dialogue paths

#### `avoided_responsibility`
**Type:** Player Choice
**Set By:** Choosing "This isn't my responsibility"
**Purpose:** Player initially avoids involvement
**Dependencies:** `water_crisis_discovered`
**Alternative:** `committed_to_help`
**Effects:** Some NPCs remember this choice in later dialogues

### Phase 2: Planning & Construction

#### `guru_guidance_received`
**Type:** Story Milestone
**Set By:** Completion of dialogue with Ki Ageng Sinawang about crisis
**Purpose:** Player has consulted their spiritual teacher
**Dependencies:** `water_crisis_discovered`
**Dialogue References:**
- [Ki Ageng Sinawang - Crisis guidance](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `ki_ageng_sinawang` - Fase Cerita 1)
**Unlocks:** Access to padepokan resources and students

#### `asked_permission_water_project`
**Type:** Story Progression
**Set By:** Requesting Ki Ageng's permission to help with water project
**Purpose:** Formal authorization to proceed with dam construction
**Dependencies:** `guru_guidance_received`
**Dialogue References:**
- Choice: "Guru, saya ingin membantu mengatasi kekurangan air ini" in Ki Ageng dialogue
**Unlocks:** Quest: `dam_construction_project`

#### `students_permission_granted`
**Type:** Resource Availability
**Set By:** Ki Ageng agreeing to provide student helpers
**Purpose:** Unlocks assistance from padepokan students
**Dependencies:** `asked_permission_water_project`
**Unlocks:** Student NPC availability for construction quests

#### `student_helpers_recruited`
**Type:** Quest Progress
**Set By:** Successfully recruiting padepokan students
**Purpose:** Indicates construction team is assembled
**Dependencies:** `students_permission_granted`
**Dialogue References:**
- [Andi - Enthusiasm to help](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `murid_padepokan_1`)
- [Candra - Dedication](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `murid_padepokan_3`)
**Unlocks:** Construction material gathering phase

#### `materials_gathered`
**Type:** Quest Progress
**Set By:** Collecting sufficient construction materials
**Purpose:** Prerequisites for dam building are met
**Dependencies:** `student_helpers_recruited`
**Unlocks:** Actual dam construction phase

#### `dam_construction_started`
**Type:** Core Progression
**Set By:** Beginning dam building work
**Purpose:** Major world state change - construction is underway
**Dependencies:** `materials_gathered`
**Effects:** 
- Modified NPC schedules (farmers check progress)
- New dialogue options referencing the project
- Environmental changes at dam site

#### `initial_dam_built`
**Type:** Story Milestone
**Set By:** Completion of first dam construction
**Purpose:** Dam structure is complete and functional
**Dependencies:** `dam_construction_started`
**Unlocks:** Temporary success period, village celebration

#### `initial_dam_success`
**Type:** Temporary State
**Set By:** Water flowing successfully after initial construction
**Purpose:** Brief period of apparent success
**Dependencies:** `initial_dam_built`
**Duration:** Temporary (overridden by destruction events)

### Phase 3: Supernatural Opposition

#### `dam_repeatedly_destroyed`
**Type:** Core Progression
**Set By:** Pattern of mysterious dam destructions
**Purpose:** Supernatural opposition becomes clear
**Dependencies:** `initial_dam_built`
**Dialogue References:**
- [Budi - Supernatural witness](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `murid_padepokan_2`)
- [Ki Ageng - Spiritual consultation](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `ki_ageng_sinawang` - Fase Cerita 3)
- [Raden Ayu - Crisis support](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `raden_ayu_saraswati`)
**Unlocks:** 
- Quest: `investigate_dam_destruction`
- Supernatural-themed dialogue options
- Access to spiritual guidance NPCs

#### `spiritual_interference_confirmed`
**Type:** Story Revelation
**Set By:** Investigation revealing supernatural cause
**Purpose:** Player understands the nature of the opposition
**Dependencies:** `dam_repeatedly_destroyed`
**Unlocks:** Quest: `spiritual_vision_encounter`

#### `spiritual_vision_active`
**Type:** Mystical State
**Set By:** Completing ritual for spiritual communication
**Purpose:** Player can interact with spirit realm
**Dependencies:** `spiritual_interference_confirmed`
**Unlocks:** Dialogue with Buaya Putih (White Crocodile Spirit)

#### `river_spirit_encountered`
**Type:** Story Milestone
**Set By:** First dialogue with Buaya Putih
**Purpose:** Direct contact with the supernatural antagonist
**Dependencies:** `spiritual_vision_active`
**Dialogue References:**
- [Buaya Putih - First contact](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `buaya_putih_spirit` - Kontak Spiritual Pertama)
**Unlocks:** Understanding of spirit's demands

#### `tribute_demand_received`
**Type:** Quest Trigger
**Set By:** Buaya Putih explaining sacrifice requirement
**Purpose:** Player learns what the spirit wants
**Dependencies:** `river_spirit_encountered`
**Unlocks:** White elephant quest line

#### `accepted_spirit_demand`
**Type:** Player Choice
**Set By:** Agreeing to find the white elephant
**Purpose:** Player commits to the sacrifice path
**Dependencies:** `tribute_demand_received`
**Dialogue References:**
- [Buaya Putih - The demand](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `buaya_putih_spirit` - Tuntutan)
- Choice: "Saya akan mencari gajah putih ini"
**Unlocks:** Quest: `find_white_elephant`

### Phase 4: The Sacred Quest

#### `heard_white_elephant_legend`
**Type:** Information Gathered
**Set By:** Learning folklore about white elephants from Nenek Bijak
**Purpose:** Player gains cultural/mystical context
**Dependencies:** `accepted_spirit_demand`
**Unlocks:** Enhanced dialogue options about sacred animals

#### `krandon_location_discovered`
**Type:** Information Gathered
**Set By:** Learning where the white elephant can be found
**Purpose:** Player knows where to go next
**Dependencies:** `heard_white_elephant_legend`
**Unlocks:** Travel options to Desa Krandon

#### `guide_hired`
**Type:** Resource Secured
**Set By:** Arranging guide to Desa Krandon
**Purpose:** Safe travel to destination is arranged
**Dependencies:** `krandon_location_discovered`
**Unlocks:** Quest: `journey_to_krandon`

#### `arrived_desa_krandon`
**Type:** Location Progress
**Set By:** Successfully reaching Desa Krandon
**Purpose:** Player is in position to find the white elephant
**Dependencies:** `guide_hired`
**Dialogue References:**
- [Joko - Guide services](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `pemandu_jalan`)
- [Mbok Randa - Suspicious greeting](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `mbok_randa_krandon` - Pertemuan Pertama)
**Unlocks:** Interaction with Mbok Randa Krandon

#### `explained_water_crisis`
**Type:** Communication Progress
**Set By:** Telling Mbok Randa about the village's situation
**Purpose:** Establishing context for elephant request
**Dependencies:** `arrived_desa_krandon`
**Unlocks:** Negotiation options for borrowing elephant

#### `promised_safe_return`
**Type:** Player Commitment (Deceptive)
**Set By:** Promising to return the elephant safely
**Purpose:** Gaining Mbok Randa's trust through false promise
**Dependencies:** `explained_water_crisis`
**Moral Weight:** High - this is the key deception
**Dialogue References:**
- [Mbok Randa - The negotiation](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `mbok_randa_krandon` - Negosiasi)
- Choice: "Saya berjanji akan mengembalikannya dengan selamat"
**Unlocks:** Access to the white elephant

#### `white_elephant_borrowed`
**Type:** Resource Obtained
**Set By:** Successfully convincing Mbok Randa to lend elephant
**Purpose:** Player has the required sacrifice
**Dependencies:** `promised_safe_return`
**Unlocks:** Return journey and sacrifice quest

#### `mbok_randa_trusts_player`
**Type:** Relationship State
**Set By:** Successful negotiation with Mbok Randa
**Purpose:** Records positive relationship (soon to be betrayed)
**Dependencies:** `white_elephant_borrowed`
**Makes Betrayal More Meaningful:** Yes

### Phase 5: The Sacrifice

#### `elephant_sacrifice_complete`
**Type:** Core Progression (Moral Crisis)
**Set By:** Completing the ritual sacrifice of white elephant
**Purpose:** The required tribute has been paid
**Dependencies:** `white_elephant_borrowed`
**Moral Weight:** Extreme
**Dialogue References:**
- [Buaya Putih - Accepting sacrifice](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `buaya_putih_spirit` - Setelah Pengorbanan)
**Unlocks:** Spirit cooperation, dam functionality

#### `spirit_pact_complete`
**Type:** Supernatural Agreement
**Set By:** Buaya Putih accepting the sacrifice
**Purpose:** River spirit will no longer destroy the dam
**Dependencies:** `elephant_sacrifice_complete`
**Unlocks:** Permanent dam functionality

#### `dam_construction_complete`
**Type:** Core Progression
**Set By:** Dam now permanently functional with spirit blessing
**Purpose:** Original goal achieved, but at great cost
**Dependencies:** `spirit_pact_complete`
**Effects:** 
- Village water crisis resolved
- Agricultural recovery begins
- Modified NPC dialogue reflecting success

#### `village_water_restored`
**Type:** World State
**Set By:** Water flowing to village due to functional dam
**Purpose:** Positive consequences are visible
**Dependencies:** `dam_construction_complete`
**Effects:** Happy villagers, recovered agriculture, celebration

#### `white_elephant_taken`
**Type:** Consequence Flag
**Set By:** The elephant is no longer available/alive
**Purpose:** Tracks the loss that will cause conflict
**Dependencies:** `elephant_sacrifice_complete`
**Unlocks:** Eventual discovery and anger

### Phase 6: Discovery & Pursuit

#### `elephant_sacrifice_revealed`
**Type:** Truth Exposure
**Set By:** Mbok Randa discovering what happened to her elephant
**Purpose:** The deception is exposed
**Dependencies:** `white_elephant_taken`
**Dialogue References:**
- [Mbok Randa - The fury](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `mbok_randa_krandon` - Penemuan Pengkhianatan)
**Unlocks:** Anger, pursuit, confrontation

#### `mbok_randa_angry`
**Type:** Relationship State
**Set By:** Mbok Randa learning of the betrayal
**Purpose:** Tracks antagonistic relationship state
**Dependencies:** `elephant_sacrifice_revealed`
**Effects:** Hostile dialogue, pursuit quest activation

#### `chase_sequence_active`
**Type:** Gameplay State
**Set By:** Villagers of Krandon pursuing Menak Sopal
**Purpose:** Player must escape or face capture
**Dependencies:** `mbok_randa_angry`
**Dialogue References:**
- [Pak Gunawan - Chase leader](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `warga_krandon_1`)
**Unlocks:** Chase mechanics, escape quest

#### `reached_river_escape`
**Type:** Progress Marker
**Set By:** Player reaching river during chase
**Purpose:** Chase sequence reaches climax location
**Dependencies:** `chase_sequence_active`
**Unlocks:** River crossing attempt

#### `drowning_in_river`
**Type:** Crisis State
**Set By:** Failed river crossing during escape
**Purpose:** Player faces mortal danger
**Dependencies:** `reached_river_escape`
**Unlocks:** Potential rescue by Buaya Putih

#### `rescued_by_crocodile`
**Type:** Supernatural Intervention
**Set By:** Buaya Putih saving Menak Sopal from drowning
**Purpose:** Spirit honors the pact by protecting the player
**Dependencies:** `drowning_in_river`
**Dialogue References:**
- [Buaya Putih - The rescue](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `buaya_putih_spirit` - Penyelamatan)
- [Buaya Putih - Final wisdom](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `buaya_putih_spirit` - Pemahaman Akhir)
- [Raden Ayu - Divine protection](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `raden_ayu_saraswati` - Setelah Cerita)
**Unlocks:** Safe return, spiritual protection confirmed

#### `spirit_protection_granted`
**Type:** Supernatural Blessing
**Set By:** Evidence of continued spirit favor
**Purpose:** Player has earned supernatural ally
**Dependencies:** `rescued_by_crocodile`
**Effects:** Potential future spiritual assistance

### Phase 7: Truth & Reconciliation

#### `returned_home_safely`
**Type:** Location & Safety
**Set By:** Successfully returning to padepokan after ordeal
**Purpose:** Player is safe and can tell their story
**Dependencies:** `rescued_by_crocodile`
**Unlocks:** Debriefing with guru and mother

#### `story_events_reported`
**Type:** Communication Complete
**Set By:** Telling Ki Ageng and Raden Ayu what happened
**Purpose:** Padepokan leadership knows the full story
**Dependencies:** `returned_home_safely`
**Unlocks:** Guidance for handling consequences

#### `mbok_randa_visits_padepokan`
**Type:** Confrontation Setup
**Set By:** Mbok Randa arriving at padepokan to confront player
**Purpose:** Final confrontation will happen in safe space
**Dependencies:** `story_events_reported`
**Unlocks:** Formal accusation and truth-telling opportunity

#### `confronted_at_padepokan`
**Type:** Confrontation Active
**Set By:** Mbok Randa making formal accusations
**Purpose:** All parties present for resolution
**Dependencies:** `mbok_randa_visits_padepokan`
**Dialogue References:**
- [Mbok Randa - At padepokan](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `mbok_randa_krandon` - Di Padepokan)
- [Raden Ayu - Conflict advice](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `raden_ayu_saraswati` - Fase Cerita)
**Unlocks:** Opportunity for complete truth telling

#### `full_truth_explained`
**Type:** Communication Milestone
**Set By:** Complete, honest explanation of all events
**Purpose:** All parties understand the full situation
**Dependencies:** `confronted_at_padepokan`
**Unlocks:** Possibility for understanding and forgiveness

#### `attempted_explanation`
**Type:** Player Choice
**Set By:** Trying to explain during angry confrontation
**Purpose:** Player attempts justification
**Dependencies:** `elephant_sacrifice_revealed`
**Alternative:** `justified_actions`
**Effects:** Different dialogue paths for reconciliation

#### `justified_actions`
**Type:** Player Choice
**Set By:** Defending actions as necessary for greater good
**Purpose:** Utilitarian moral stance
**Dependencies:** `elephant_sacrifice_revealed`
**Alternative:** `attempted_explanation`
**Effects:** Affects reconciliation difficulty

#### `sincere_apology_given`
**Type:** Moral Choice
**Set By:** Offering genuine remorse and apology
**Purpose:** Player takes responsibility and shows growth
**Dependencies:** `full_truth_explained`
**Dialogue References:**
- [Mbok Randa - Understanding begins](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `mbok_randa_krandon` - Pemahaman dan Pengampunan)
- Choice: "Ya, Mbok. Dan saya benar-benar minta maaf karena menipu Mbok"
**Unlocks:** Path to forgiveness and reconciliation

#### `remorse_expressed`
**Type:** Character Development
**Set By:** Demonstrating genuine regret for deception
**Purpose:** Shows player character growth
**Dependencies:** `sincere_apology_given`
**Unlocks:** Advanced reconciliation options

### Phase 8: Resolution & Legacy

#### `reconciliation_complete`
**Type:** Core Resolution
**Set By:** Achieving mutual understanding and forgiveness
**Purpose:** Main conflict resolved peacefully
**Dependencies:** `remorse_expressed`
**Unlocks:** Final ceremony and naming event

#### `mutual_understanding_achieved`
**Type:** Relationship Healing
**Set By:** Both parties accepting and forgiving
**Purpose:** Positive relationship restored
**Dependencies:** `reconciliation_complete`
**Effects:** Peaceful post-story interactions

#### `teranging_galih_named`
**Type:** Cultural Legacy
**Set By:** Mbok Randa's declaration of the land name
**Purpose:** Story becomes part of local legend
**Dependencies:** `reconciliation_complete`
**Dialogue References:**
- [Mbok Randa - Naming declaration](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `mbok_randa_krandon` - Rekonsiliasi Selesai)
**Unlocks:** Final story completion

#### `land_naming_complete`
**Type:** Ceremony Finished
**Set By:** Completion of naming ceremony
**Purpose:** Formal recognition of the story's resolution
**Dependencies:** `teranging_galih_named`
**Unlocks:** Final character interactions

#### `story_completed`
**Type:** Main Story Complete
**Set By:** All major story beats concluded
**Purpose:** Player has experienced complete narrative arc
**Dependencies:** `land_naming_complete`
**Dialogue References:**
- [Ki Ageng - Final wisdom](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `ki_ageng_sinawang` - Kesimpulan Cerita)
- [Raden Ayu - Mother's pride](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `raden_ayu_saraswati` - Kesimpulan Cerita)
- [Mbok Randa - Peaceful reflection](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `mbok_randa_krandon` - Dialog Pasca-Cerita)
**Effects:** 
- Unlocks post-story content
- Achievement/completion recognition
- Modified world state reflecting story conclusion

#### `wisdom_gained`
**Type:** Character Development
**Set By:** Final reflection on lessons learned
**Purpose:** Player character has grown through experience
**Dependencies:** `story_completed`
**Effects:** Enhanced dialogue options, reputation changes

---

## Supporting Character Flags

### Teacher Relationship (Ki Ageng Sinawang)

#### `guru_advice_reconciliation`
**Type:** Guidance Received
**Set By:** Ki Ageng providing wisdom about making amends
**Purpose:** Player has spiritual guidance for reconciliation
**Dependencies:** `mbok_randa_angry`
**Dialogue References:**
- [Ki Ageng - Reconciliation advice](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `ki_ageng_sinawang` - Fase Cerita 4)
**Unlocks:** Better reconciliation dialogue options

#### `padepokan_life_established`
**Type:** Tutorial Complete
**Set By:** Initial interactions with padepokan life
**Purpose:** Player understands their background
**Dependencies:** `story_started`
**Unlocks:** Advanced padepokan interactions

### Village Relationships

#### `established_village_reputation`
**Type:** Community Standing
**Set By:** Completing multiple village quests/interactions
**Purpose:** Player is known and trusted in community
**Dependencies:** Multiple village quest completions
**Unlocks:** Advanced village quests, chief trust

#### `pak_tani_harvest_accepted`
**Type:** Side Quest Flag
**Set By:** Agreeing to help with rice harvest
**Purpose:** Tracking agricultural assistance
**Dependencies:** Meeting Pak Tani
**Unlocks:** Agricultural knowledge, village reputation

#### `village_rice_harvest_complete`
**Type:** Side Quest Complete
**Set By:** Successfully helping with harvest
**Purpose:** Positive village relationship
**Dependencies:** `pak_tani_harvest_accepted`
**Effects:** Better prices, village support

### Spiritual/Mystical Flags

#### `showed_respect_to_spirit`
**Type:** Interaction Choice
**Set By:** Respectful dialogue with Buaya Putih
**Purpose:** Player demonstrates proper spiritual etiquette
**Dependencies:** `river_spirit_encountered`
**Dialogue References:**
- [Buaya Putih - Respectful response](NPCs/Story-NPCs-Dialogues_ID.md) (ID: `buaya_putih_spirit`)
- Choice: "Roh agung, saya tidak bermaksud menyinggung"
**Effects:** Better relationship with spiritual entities

#### `spiritual_ritual_accepted`
**Type:** Mystical Engagement
**Set By:** Agreeing to participate in cleansing ritual
**Purpose:** Player engages with traditional spiritual practices
**Dependencies:** Dukun Kampung interaction
**Unlocks:** Enhanced spiritual knowledge

---

## Flag Categories & Management

### Core Progression Flags
**Purpose:** Essential story advancement
**Characteristics:** 
- Cannot be unset once achieved
- Required for story completion
- Trigger major quest unlocks
**Examples:** `water_crisis_discovered`, `dam_construction_complete`, `story_completed`

### Player Choice Flags
**Purpose:** Track moral decisions and player agency
**Characteristics:**
- Mutually exclusive options (A or B, not both)
- Permanent consequences
- Affect dialogue and relationship options
**Examples:** `committed_to_help` vs `avoided_responsibility`

### Relationship State Flags
**Purpose:** Track NPC relationship conditions
**Characteristics:**
- Can change over time
- Affect available interactions
- Influence story resolution difficulty
**Examples:** `mbok_randa_trusts_player` → `mbok_randa_angry` → `mutual_understanding_achieved`

### World State Flags
**Purpose:** Track persistent world changes
**Characteristics:**
- Affect environment and NPC behavior
- Persist across game sessions
- Visible in gameplay and dialogue
**Examples:** `village_water_restored`, `dam_construction_complete`

### Temporary State Flags
**Purpose:** Short-term conditions and transitions
**Characteristics:**
- May be overridden by other flags
- Used for specific sequences
- Often have time or event limits
**Examples:** `chase_sequence_active`, `spiritual_vision_active`

---

## Flag Dependencies & Conflicts

### Linear Progression Dependencies
```
story_started → water_crisis_discovered → dam_construction_started → 
dam_repeatedly_destroyed → river_spirit_encountered → accepted_spirit_demand → 
white_elephant_borrowed → elephant_sacrifice_complete → story_completed
```

### Branching Choice Points
```
water_crisis_discovered → [committed_to_help | avoided_responsibility]
elephant_sacrifice_revealed → [attempted_explanation | justified_actions]
full_truth_explained → [sincere_apology_given | continued_defiance]
```

### Mutually Exclusive Flags
- `committed_to_help` ⊥ `avoided_responsibility`
- `attempted_explanation` ⊥ `justified_actions`
- `mbok_randa_trusts_player` ⊥ `mbok_randa_angry` (different phases)

### Prerequisite Chains
- `reconciliation_complete` requires: `sincere_apology_given` + `full_truth_explained` + `remorse_expressed`
- `spirit_pact_complete` requires: `accepted_spirit_demand` + `white_elephant_borrowed` + `elephant_sacrifice_complete`

---

## Implementation Guidelines

### Flag Naming Conventions
- **Past tense verbs** for completed actions: `water_crisis_discovered`, `elephant_sacrifice_complete`
- **Present tense** for ongoing states: `chase_sequence_active`, `mbok_randa_angry`
- **Descriptive phrases** for choices: `committed_to_help`, `sincere_apology_given`

### Flag Storage Requirements
- Persistent storage across game sessions
- Boolean values (true/false or present/absent)
- Efficient lookup for dialogue and quest systems
- Save game compatibility

### Testing Considerations
- Debug console commands to set/unset flags for testing
- Flag state visualization for development
- Automated testing of flag dependency chains
- Save game corruption prevention

### Performance Optimization
- Efficient flag checking in dialogue system
- Minimal flag evaluations during gameplay
- Cached flag states for frequently accessed flags
- Cleanup of temporary flags when no longer needed

---

## Cultural & Educational Notes

### Indonesian Language Integration
Some flags incorporate Indonesian concepts:
- `teranging_galih_named` - "Teranging Galih" means "brightness of understanding"
- Cultural authenticity in flag naming where appropriate

### Moral Education Elements
Flag system tracks moral development:
- Responsibility vs avoidance
- Truth vs deception  
- Utilitarian vs empathetic choices
- Growth through consequence

### Folklore Preservation
Flags ensure story maintains cultural authenticity:
- Respect for spiritual elements
- Traditional conflict resolution patterns
- Community values and relationships
- Educational value of origin legends