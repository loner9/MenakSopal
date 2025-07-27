# Side Quests Documentation

This document outlines all side quests for the Trenggalek folklore game, designed to enrich the player experience, build village relationships, and provide additional gameplay content alongside the main story.

## Side Quest Philosophy

Side quests in this game serve multiple purposes:
- **Community Building** - Strengthen relationships with village NPCs
- **Cultural Education** - Showcase Indonesian village life and traditions
- **Moral Development** - Provide additional choices that reflect character growth
- **Resource Management** - Allow players to gather items and build reputation
- **World Building** - Expand understanding of the game world beyond main story

---

## Agricultural & Farming Quests

### Rice Harvest Assistance
**Quest ID:** `village_rice_harvest`
**Giver:** Pak Tani (Farmer)
**Type:** Collection/Work
**Difficulty:** Easy
**Estimated Duration:** 15-20 minutes

#### Quest Details
```yaml
questTitle: "Helping Hands in the Fields"
questDescription: "Pak Tani needs assistance with the rice harvest. Help gather the golden grain that feeds the village."
questType: Side
requiredFlags: []
availableTimesOfDay: [Morning, Afternoon]

objectives:
  - objectiveID: "talk_to_pak_tani"
    description: "Speak with Pak Tani about harvest help"
    type: TalkToNPC
    targetNPC: "pak_tani"
    
  - objectiveID: "gather_rice_bundles"
    description: "Collect rice bundles from the fields"
    type: CollectItems
    targetItem: "rice_bundles"
    targetAmount: 8
    showProgress: true
    
  - objectiveID: "deliver_to_storage"
    description: "Bring harvested rice to village storage"
    type: VisitLocation
    targetLocation: "VillageGranary"

rewards:
  - type: Item
    itemID: "cooked_rice"
    amount: 3
  - type: Flags
    flagsToAdd: ["helped_with_harvest", "pak_tani_grateful"]

flagsOnComplete: ["village_rice_harvest_complete"]
```

#### Cultural Learning Elements
- Traditional rice harvesting techniques
- Community cooperation in agriculture
- Seasonal timing and planning
- Respect for food sources

#### Integration with Main Story
- If completed during water crisis: Enhanced dialogue about drought impact
- Post-dam completion: Celebration of water's return to agriculture
- Builds village reputation for advanced quests

---

### Herbal Medicine Gathering
**Quest ID:** `gather_healing_herbs`
**Giver:** Bu Tani (Farmer's Wife)
**Type:** Collection/Exploration
**Difficulty:** Medium
**Estimated Duration:** 20-25 minutes

#### Quest Details
```yaml
questTitle: "Nature's Pharmacy"
questDescription: "Bu Tani needs medicinal herbs from the forest to prepare traditional remedies for village children."
questType: Side
requiredFlags: []
availableTimesOfDay: [Morning, Afternoon]

objectives:
  - objectiveID: "learn_about_herbs"
    description: "Learn which herbs to gather from Bu Tani"
    type: TalkToNPC
    targetNPC: "bu_tani"
    
  - objectiveID: "find_turmeric"
    description: "Gather turmeric root (kunyit) from forest"
    type: CollectItems
    targetItem: "turmeric_root"
    targetAmount: 3
    
  - objectiveID: "find_ginger"
    description: "Collect fresh ginger (jahe) from hillside"
    type: CollectItems
    targetItem: "fresh_ginger"
    targetAmount: 4
    
  - objectiveID: "find_lemongrass"
    description: "Cut lemongrass (serai) from riverside"
    type: CollectItems
    targetItem: "lemongrass"
    targetAmount: 5
    
  - objectiveID: "return_herbs"
    description: "Bring all herbs back to Bu Tani"
    type: TalkToNPC
    targetNPC: "bu_tani"

rewards:
  - type: Item
    itemID: "herbal_medicine"
    amount: 2
  - type: Flags
    flagsToAdd: ["herbal_knowledge_gained", "bu_tani_grateful"]

flagsOnComplete: ["herb_gathering_quest_complete"]
```

#### Educational Value
- Traditional Indonesian medicinal plants
- Sustainable foraging practices
- Community healthcare traditions
- Plant identification skills

#### Special Features
- Different herbs found in different biomes
- Seasonal availability variations
- Risk/reward with rare herbs in dangerous areas

---

## Community Service Quests

### School Supply Collection
**Quest ID:** `gather_school_supplies`
**Giver:** Bu Guru (Teacher)
**Type:** Collection/Crafting
**Difficulty:** Easy
**Estimated Duration:** 15-20 minutes

#### Quest Details
```yaml
questTitle: "Tools for Learning"
questDescription: "The village children need writing materials. Help gather palm leaves and make charcoal for their education."
questType: Side
requiredFlags: []
availableTimesOfDay: [Morning, Afternoon]

objectives:
  - objectiveID: "collect_palm_leaves"
    description: "Gather large palm leaves suitable for writing"
    type: CollectItems
    targetItem: "palm_leaves"
    targetAmount: 10
    
  - objectiveID: "make_charcoal"
    description: "Prepare charcoal sticks from burnt wood"
    type: CollectItems
    targetItem: "charcoal_sticks"
    targetAmount: 6
    
  - objectiveID: "test_materials"
    description: "Help test the writing materials with children"
    type: TalkToNPC
    targetNPC: "bu_guru"

rewards:
  - type: Experience
    amount: 50
  - type: Flags
    flagsToAdd: ["education_supporter", "children_grateful"]

flagsOnComplete: ["school_supplies_quest_complete"]
```

#### Cultural Elements
- Traditional writing materials before modern paper
- Community investment in children's education
- Sustainable resource use
- Intergenerational cooperation

---

### Village Construction Project
**Quest ID:** `village_construction_project`
**Giver:** Pemuda Desa (Village Youth)
**Type:** Collection/Work
**Difficulty:** Medium
**Estimated Duration:** 25-30 minutes

#### Quest Details
```yaml
questTitle: "Building Together"
questDescription: "Help construct a new storage house for the village grain supply. Many hands make light work!"
questType: Side
requiredFlags: []
availableTimesOfDay: [Morning, Afternoon]

objectives:
  - objectiveID: "gather_wood"
    description: "Collect wooden beams from the forest"
    type: CollectItems
    targetItem: "wooden_beams"
    targetAmount: 8
    
  - objectiveID: "gather_stones"
    description: "Bring foundation stones from quarry"
    type: CollectItems
    targetItem: "foundation_stones"
    targetAmount: 12
    
  - objectiveID: "assist_construction"
    description: "Help with the building work"
    type: Custom
    
  - objectiveID: "celebrate_completion"
    description: "Join the completion celebration"
    type: VisitLocation
    targetLocation: "NewStorageHouse"

rewards:
  - type: Item
    itemID: "building_tools"
    amount: 1
  - type: Flags
    flagsToAdd: ["construction_helper", "village_builder"]

flagsOnComplete: ["construction_project_complete"]
```

---

## Merchant & Trade Quests

### Delivery to Neighboring Village
**Quest ID:** `merchant_delivery_krandon`
**Giver:** Pak Pedagang (Merchant)
**Type:** Delivery/Travel
**Difficulty:** Medium
**Estimated Duration:** 30-35 minutes

#### Quest Details
```yaml
questTitle: "Trusted Courier"
questDescription: "Deliver valuable herbal medicines to the healer in Desa Krandon and return with payment."
questType: Side
requiredFlags: []
availableTimesOfDay: [Morning]

objectives:
  - objectiveID: "receive_package"
    description: "Get the delivery package from Pak Pedagang"
    type: TalkToNPC
    targetNPC: "pak_pedagang"
    
  - objectiveID: "travel_to_krandon"
    description: "Safely reach Desa Krandon"
    type: VisitLocation
    targetLocation: "DesaKrandon"
    
  - objectiveID: "find_healer"
    description: "Locate Dukun Krandon (village healer)"
    type: TalkToNPC
    targetNPC: "dukun_krandon"
    
  - objectiveID: "complete_delivery"
    description: "Deliver package and collect payment"
    type: Custom
    
  - objectiveID: "return_safely"
    description: "Return to home village with payment"
    type: TalkToNPC
    targetNPC: "pak_pedagang"

rewards:
  - type: Gold
    amount: 25
  - type: Item
    itemID: "travel_provisions"
    amount: 3
  - type: Flags
    flagsToAdd: ["reliable_courier", "merchant_trust"]

flagsOnComplete: ["merchant_delivery_complete"]
```

#### Special Considerations
- Random encounter possibilities during travel
- Weather/time of day affects difficulty
- Builds relationship for better shop prices
- May overlap with main story Krandon visits

---

### Festival Food Preparation
**Quest ID:** `gather_festival_ingredients`
**Giver:** Bu Penjual (Food Vendor)
**Type:** Collection/Celebration
**Difficulty:** Medium
**Estimated Duration:** 25-30 minutes

#### Quest Details
```yaml
questTitle: "Feast for the Community"
questDescription: "Help prepare special foods for the village festival by gathering ingredients from land, river, and forest."
questType: Side
requiredFlags: []
availableTimesOfDay: [Morning, Afternoon]
seasonalAvailability: [Festival_Season]

objectives:
  - objectiveID: "catch_fresh_fish"
    description: "Catch fish from the river"
    type: CollectItems
    targetItem: "fresh_fish"
    targetAmount: 4
    
  - objectiveID: "gather_vegetables"
    description: "Collect vegetables from farms"
    type: CollectItems
    targetItem: "fresh_vegetables"
    targetAmount: 6
    
  - objectiveID: "find_forest_spices"
    description: "Gather spices from forest plants"
    type: CollectItems
    targetItem: "forest_spices"
    targetAmount: 3
    
  - objectiveID: "help_cooking"
    description: "Assist with festival food preparation"
    type: Custom
    
  - objectiveID: "enjoy_festival"
    description: "Participate in the community feast"
    type: VisitLocation
    targetLocation: "FestivalArea"

rewards:
  - type: Experience
    amount: 75
  - type: Flags
    flagsToAdd: ["festival_contributor", "community_member"]

flagsOnComplete: ["festival_cooking_complete"]
```

---

## Spiritual & Cultural Quests

### River Spirit Cleansing Ritual
**Quest ID:** `river_spirit_cleansing`
**Giver:** Dukun Kampung (Village Shaman)
**Type:** Spiritual/Ritual
**Difficulty:** Hard
**Estimated Duration:** 20-25 minutes

#### Quest Details
```yaml
questTitle: "Appeasing the Waters"
questDescription: "Perform a traditional cleansing ritual to calm the restless river spirits."
questType: Side
requiredFlags: ["dam_repeatedly_destroyed"]
availableTimesOfDay: [Evening, Night]

objectives:
  - objectiveID: "gather_white_flowers"
    description: "Collect pure white flowers for offering"
    type: CollectItems
    targetItem: "white_flowers"
    targetAmount: 7
    
  - objectiveID: "prepare_incense"
    description: "Create sacred incense from aromatic woods"
    type: CollectItems
    targetItem: "sacred_incense"
    targetAmount: 3
    
  - objectiveID: "midnight_ritual"
    description: "Perform cleansing ritual at river shrine"
    type: Custom
    requiredTime: "Midnight"
    
  - objectiveID: "meditation_period"
    description: "Complete post-ritual meditation"
    type: Custom

rewards:
  - type: Item
    itemID: "spiritual_blessing"
    amount: 1
  - type: Flags
    flagsToAdd: ["spiritual_knowledge", "ritual_participant"]

flagsOnComplete: ["river_cleansing_complete"]
```

#### Special Features
- Must be performed at specific time (midnight)
- Requires preparation during day hours
- May provide alternative insight into main story conflict
- Enhances relationship with spiritual NPCs

---

### Children's Storytelling Circle
**Quest ID:** `gather_children_storytelling`
**Giver:** Nenek Bijak (Wise Elder)
**Type:** Cultural/Educational
**Difficulty:** Easy
**Estimated Duration:** 15-20 minutes

#### Quest Details
```yaml
questTitle: "Preserving Ancient Wisdom"
questDescription: "Help gather village children for traditional storytelling to preserve cultural heritage."
questType: Side
requiredFlags: []
availableTimesOfDay: [Evening]

objectives:
  - objectiveID: "invite_children"
    description: "Convince children to attend storytelling"
    type: TalkToNPC
    targetNPC: "village_children"
    targetAmount: 5
    
  - objectiveID: "prepare_seating"
    description: "Arrange seating at the banyan tree"
    type: VisitLocation
    targetLocation: "BanyanTreeCircle"
    
  - objectiveID: "listen_to_stories"
    description: "Participate in traditional storytelling"
    type: Custom
    
  - objectiveID: "share_own_story"
    description: "Tell children about your adventures"
    type: Custom

rewards:
  - type: Experience
    amount: 40
  - type: Flags
    flagsToAdd: ["storytelling_participant", "cultural_preserver"]

flagsOnComplete: ["storytelling_circle_complete"]
```

---

## Animal Care & Nature Quests

### Lost Goat Recovery
**Quest ID:** `find_lost_goat`
**Giver:** Anak Gembala (Shepherd Boy)
**Type:** Search/Rescue
**Difficulty:** Medium
**Estimated Duration:** 20-25 minutes

#### Quest Details
```yaml
questTitle: "Putih the Wanderer"
questDescription: "Help find Putih, the shepherd boy's missing goat who wandered into the forest."
questType: Side
requiredFlags: []
availableTimesOfDay: [Afternoon, Evening]

objectives:
  - objectiveID: "investigate_pen"
    description: "Examine the goat pen for clues"
    type: VisitLocation
    targetLocation: "GoatPen"
    
  - objectiveID: "track_through_village"
    description: "Follow goat tracks through village"
    type: Custom
    
  - objectiveID: "search_bamboo_grove"
    description: "Check bamboo grove where goats like to eat"
    type: VisitLocation
    targetLocation: "BambooGrove"
    
  - objectiveID: "rescue_goat"
    description: "Find and safely retrieve Putih"
    type: Custom
    
  - objectiveID: "return_to_shepherd"
    description: "Bring Putih back to Anak Gembala"
    type: TalkToNPC
    targetNPC: "anak_gembala"

rewards:
  - type: Item
    itemID: "fresh_milk"
    amount: 2
  - type: Flags
    flagsToAdd: ["animal_friend", "shepherd_helper"]

flagsOnComplete: ["lost_goat_quest_complete"]
```

#### Special Features
- Tracking mini-game elements
- Environmental clues and observation
- Animal behavior education
- Time pressure (goat gets farther away)

---

## Security & Protection Quests

### Bandit Investigation
**Quest ID:** `investigate_bandit_threat`
**Giver:** Pak Lurah (Village Chief)
**Type:** Investigation/Combat
**Difficulty:** Hard
**Estimated Duration:** 35-40 minutes

#### Quest Details
```yaml
questTitle: "Threat to Trade Routes"
questDescription: "Investigate reports of bandits threatening village trade routes and merchants."
questType: Side
requiredFlags: ["established_village_reputation"]
availableTimesOfDay: [Morning, Afternoon]

objectives:
  - objectiveID: "interview_merchants"
    description: "Speak with affected merchants about attacks"
    type: TalkToNPC
    targetNPC: "pak_pedagang"
    
  - objectiveID: "examine_attack_sites"
    description: "Investigate locations where attacks occurred"
    type: VisitLocation
    targetLocation: "TradeRoute1"
    
  - objectiveID: "gather_evidence"
    description: "Collect clues about bandit activities"
    type: CollectItems
    targetItem: "bandit_evidence"
    targetAmount: 3
    
  - objectiveID: "track_bandit_camp"
    description: "Follow clues to bandit hideout"
    type: VisitLocation
    targetLocation: "BanditCamp"
    
  - objectiveID: "confront_bandits"
    description: "Deal with the bandit threat"
    type: Custom
    
  - objectiveID: "report_to_chief"
    description: "Report mission results to Pak Lurah"
    type: TalkToNPC
    targetNPC: "pak_lurah"

rewards:
  - type: Gold
    amount: 50
  - type: Item
    itemID: "village_recognition"
    amount: 1
  - type: Flags
    flagsToAdd: ["village_protector", "bandit_threat_resolved"]

flagsOnComplete: ["bandit_investigation_complete"]
```

#### Player Choice Elements
- Negotiate vs Combat resolution options
- Evidence gathering affects available approaches
- Moral choices about bandit motivations
- Community impact of chosen solution

---

## Competitive & Skill Quests

### Village Athletics Competition
**Quest ID:** `village_athletic_competition`
**Giver:** Pemuda Desa (Village Youth)
**Type:** Competition/Skill
**Difficulty:** Medium
**Estimated Duration:** 20-25 minutes

#### Quest Details
```yaml
questTitle: "Festival of Strength and Speed"
questDescription: "Participate in traditional village athletic competitions during festival season."
questType: Side
requiredFlags: []
availableTimesOfDay: [Afternoon]
seasonalAvailability: [Festival_Season]

objectives:
  - objectiveID: "racing_competition"
    description: "Compete in village foot race"
    type: Custom
    
  - objectiveID: "strength_challenge"
    description: "Participate in stone lifting contest"
    type: Custom
    
  - objectiveID: "agility_course"
    description: "Complete traditional obstacle course"
    type: Custom
    
  - objectiveID: "team_events"
    description: "Join group competitions"
    type: Custom

rewards:
  - type: Experience
    amount: 60
  - type: Item
    itemID: "athletic_recognition"
    amount: 1
  - type: Flags
    flagsToAdd: ["athletic_competitor", "festival_participant"]

flagsOnComplete: ["athletic_competition_complete"]
```

---

## Quest Integration Strategies

### Main Story Integration
**Parallel Progression:**
- Side quests available throughout main story
- Some unlock based on story progress
- Others provide alternative perspectives on main events

**Character Development:**
- Side quest choices affect main story dialogue options
- Reputation system influences main story difficulty
- Moral choices in side quests reflect on main character arc

### Village Reputation System
**Reputation Levels:**
1. **Stranger** (0-2 quests) - Basic interactions only
2. **Friend** (3-5 quests) - Enhanced dialogue, better prices
3. **Trusted Member** (6-8 quests) - Advanced quests, special access
4. **Village Hero** (9+ quests) - Best outcomes, community respect

**Reputation Benefits:**
- Better merchant prices and selection
- Access to exclusive quests and areas
- Enhanced dialogue options in main story
- Community support during crises

### Seasonal & Time-Based Availability
**Festival Season Quests:**
- Special celebration quests
- Community gathering events
- Cultural preservation activities

**Agricultural Season Quests:**
- Planting and harvest assistance
- Seasonal resource gathering
- Weather-dependent activities

**Crisis Period Modifications:**
- During water crisis: Modified quest objectives
- Post-resolution: Celebration and recovery quests
- Story-sensitive quest availability

### Educational Value Integration
**Cultural Learning:**
- Traditional Indonesian practices
- Village community structures
- Agricultural and crafting knowledge
- Spiritual and philosophical concepts

**Moral Development:**
- Community service emphasis
- Environmental stewardship
- Intergenerational respect
- Collective vs individual benefit

### Technical Implementation Notes
**Quest System Requirements:**
- Flag-based availability control
- Time and season dependency tracking
- Reputation point calculation
- Reward distribution system

**Performance Considerations:**
- Efficient quest state tracking
- Minimal save game impact
- Scalable for additional content
- Cross-platform compatibility

**Testing Framework:**
- Individual quest completion testing
- Integration with main story testing
- Reputation system validation
- Seasonal transition testing