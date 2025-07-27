# NPC Schedules Documentation

This document contains comprehensive schedule data for all NPCs in the Trenggalek folklore game using the existing NPCScheduleData system.

## Schedule Overview

NPCs are categorized by their primary roles and importance to story progression:

### Story-Essential NPCs
- **Ki Ageng Sinawang** - Padepokan leader, available for teaching and guidance
- **Raden Ayu Saraswati** - Mother figure, home-based with specific story interactions
- **Mbok Randa Krandon** - Key antagonist, village-based with travel events

### Village Life NPCs
- **Pak Tani & Bu Tani** - Farmers with agricultural schedules
- **Pak Pedagang** - Merchant with business hours
- **Bu Penjual** - Food vendor with meal-time focus
- **Pak Lurah** - Village chief with formal schedule
- **Bu Guru** - Teacher with education hours
- **Dukun Kampung** - Shaman with mystical schedule

### Community NPCs
- **Anak Gembala** - Shepherd with pastoral duties
- **Pemuda Desa** - Village youth with flexible schedule
- **Nenek Bijak** - Elder with storytelling times

---

## Story-Essential NPC Schedules

### Ki Ageng Sinawang Schedule

**NPC ID:** `ki_ageng_sinawang`
**Schedule Name:** "Padepokan Leader Daily Routine"

```yaml
scheduleName: "Padepokan Master Schedule"
scheduleDescription: "Daily routine of the padepokan spiritual leader"
spawnHour: 5
homeObjectTag: "NPCTarget"
homeObjectName: "PadepokanMasterQuarters"
walkSpeed: 1.2
pauseAtDestination: 3.0
moveAroundWhenIdle: true
idleMovementRange: 2.0

scheduleEvents:
  - hour: 5
    targetObjectTag: "NPCTarget"
    targetObjectName: "MeditationSpot"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["The dawn brings clarity to the mind and peace to the soul."]
    
  - hour: 7
    targetObjectTag: "NPCTarget"
    targetObjectName: "PadepokanMainHall"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Come, students. Today we learn about balance in all things."]
    
  - hour: 12
    targetObjectTag: "NPCTarget"
    targetObjectName: "PadepokanCourtyard"
    behavior: Walk
    shouldIdleWhenReached: true
    customDialogue: ["Midday reflection helps center the spirit."]
    
  - hour: 15
    targetObjectTag: "NPCTarget"
    targetObjectName: "PadepokanGarden"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Tending the garden teaches patience and care."]
    
  - hour: 18
    targetObjectTag: "NPCTarget"
    targetObjectName: "PadepokanMainHall"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Evening discussions bring wisdom through shared thoughts."]
    
  - hour: 21
    targetObjectTag: "NPCTarget"
    targetObjectName: "PadepokanMasterQuarters"
    behavior: Sleep
    shouldIdleWhenReached: true
    shouldDespawn: false
```

### Raden Ayu Saraswati Schedule

**NPC ID:** `raden_ayu_saraswati`
**Schedule Name:** "Caring Mother Daily Routine"

```yaml
scheduleName: "Mother's Daily Care Schedule"
scheduleDescription: "Nurturing mother with household and family duties"
spawnHour: 5
homeObjectTag: "House"
homeObjectName: "SaraswatiHouse"
walkSpeed: 1.0
pauseAtDestination: 2.5
moveAroundWhenIdle: true
idleMovementRange: 1.5

scheduleEvents:
  - hour: 5
    targetObjectTag: "NPCTarget"
    targetObjectName: "KitchenArea"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Preparing breakfast brings joy to a mother's heart."]
    
  - hour: 8
    targetObjectTag: "NPCTarget"
    targetObjectName: "HerbGarden"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["These healing herbs will help the village children."]
    
  - hour: 11
    targetObjectTag: "NPCTarget"
    targetObjectName: "WashingArea"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Clean clothes reflect a clean heart."]
    
  - hour: 14
    targetObjectTag: "NPCTarget"
    targetObjectName: "SaraswatiHouse"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Afternoon rest helps restore energy for evening duties."]
    
  - hour: 17
    targetObjectTag: "NPCTarget"
    targetObjectName: "KitchenArea"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Evening meal preparation is time for family bonding."]
    
  - hour: 20
    targetObjectTag: "NPCTarget"
    targetObjectName: "SaraswatiHouse"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Evening prayers bring peace to the household."]
    
  - hour: 22
    targetObjectTag: "House"
    targetObjectName: "SaraswatiHouse"
    behavior: Sleep
    shouldIdleWhenReached: true
    shouldDespawn: false
```

### Mbok Randa Krandon Schedule

**NPC ID:** `mbok_randa_krandon`
**Schedule Name:** "Village Elder with White Elephant"

```yaml
scheduleName: "Krandon Village Elder Schedule"
scheduleDescription: "Daily routine of the white elephant owner"
spawnHour: 6
homeObjectTag: "House"
homeObjectName: "MbokRandaHouse"
walkSpeed: 0.8
pauseAtDestination: 4.0
moveAroundWhenIdle: false
idleMovementRange: 1.0

scheduleEvents:
  - hour: 6
    targetObjectTag: "NPCTarget"
    targetObjectName: "ElephantEnclosure"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["My precious elephant needs morning care and feeding."]
    
  - hour: 9
    targetObjectTag: "NPCTarget"
    targetObjectName: "VillageWell"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Drawing water is harder work than it used to be."]
    
  - hour: 11
    targetObjectTag: "House"
    targetObjectName: "MbokRandaHouse"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Household chores never end for a village woman."]
    
  - hour: 14
    targetObjectTag: "NPCTarget"
    targetObjectName: "ElephantEnclosure"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Watching my elephant brings me such peace and joy."]
    
  - hour: 17
    targetObjectTag: "NPCTarget"
    targetObjectName: "VillageCenter"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Evening gossip keeps me informed of village happenings."]
    
  - hour: 19
    targetObjectTag: "House"
    targetObjectName: "MbokRandaHouse"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Evening prayers for family and elephant's wellbeing."]
    
  - hour: 21
    targetObjectTag: "House"
    targetObjectName: "MbokRandaHouse"
    behavior: Sleep
    shouldIdleWhenReached: true
    shouldDespawn: false
```

---

## Village Life NPC Schedules

### Pak Tani (Farmer) Schedule

**NPC ID:** `pak_tani`
**Schedule Name:** "Agricultural Worker Daily Routine"

```yaml
scheduleName: "Farmer's Agricultural Schedule"
scheduleDescription: "Rice farmer with seasonal agricultural duties"
spawnHour: 5
homeObjectTag: "House"
homeObjectName: "FarmerHouse"
walkSpeed: 1.5
pauseAtDestination: 2.0
moveAroundWhenIdle: true
idleMovementRange: 3.0

scheduleEvents:
  - hour: 5
    targetObjectTag: "Farm"
    targetObjectName: "RiceField1"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Early morning is best for rice field work."]
    
  - hour: 9
    targetObjectTag: "Farm"
    targetObjectName: "RiceField2"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["These fields feed our entire village."]
    
  - hour: 12
    targetObjectTag: "NPCTarget"
    targetObjectName: "FieldShelter"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Midday rest prevents heat exhaustion."]
    
  - hour: 15
    targetObjectTag: "Farm"
    targetObjectName: "RiceField3"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Afternoon work while the sun is still strong."]
    
  - hour: 18
    targetObjectTag: "House"
    targetObjectName: "FarmerHouse"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Home for evening meal with family."]
    
  - hour: 21
    targetObjectTag: "House"
    targetObjectName: "FarmerHouse"
    behavior: Sleep
    shouldIdleWhenReached: true
    shouldDespawn: false
```

### Bu Tani (Farmer's Wife) Schedule

**NPC ID:** `bu_tani`
**Schedule Name:** "Farm Household Manager"

```yaml
scheduleName: "Farm Wife Household Schedule"
scheduleDescription: "Household management and herbal knowledge keeper"
spawnHour: 5
homeObjectTag: "House"
homeObjectName: "FarmerHouse"
walkSpeed: 1.0
pauseAtDestination: 3.0
moveAroundWhenIdle: true
idleMovementRange: 2.0

scheduleEvents:
  - hour: 5
    targetObjectTag: "NPCTarget"
    targetObjectName: "KitchenGarden"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Morning vegetables are the freshest for cooking."]
    
  - hour: 8
    targetObjectTag: "NPCTarget"
    targetObjectName: "HerbPatch"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Gathering healing herbs for village medicine."]
    
  - hour: 10
    targetObjectTag: "House"
    targetObjectName: "FarmerHouse"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Household chores keep the family healthy."]
    
  - hour: 13
    targetObjectTag: "NPCTarget"
    targetObjectName: "CookingArea"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Preparing lunch for hardworking family."]
    
  - hour: 16
    targetObjectTag: "Well"
    targetObjectName: "VillageWell"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Fetching water for evening cooking."]
    
  - hour: 18
    targetObjectTag: "House"
    targetObjectName: "FarmerHouse"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Family dinner time is sacred."]
    
  - hour: 21
    targetObjectTag: "House"
    targetObjectName: "FarmerHouse"
    behavior: Sleep
    shouldIdleWhenReached: true
    shouldDespawn: false
```

### Pak Pedagang (Merchant) Schedule

**NPC ID:** `pak_pedagang`
**Schedule Name:** "Village Trading Business"

```yaml
scheduleName: "Merchant Business Schedule"
scheduleDescription: "Trade and commerce with flexible business hours"
spawnHour: 7
homeObjectTag: "Shop"
homeObjectName: "MerchantShop"
walkSpeed: 1.3
pauseAtDestination: 1.5
moveAroundWhenIdle: false
idleMovementRange: 1.5

scheduleEvents:
  - hour: 7
    targetObjectTag: "Shop"
    targetObjectName: "MerchantShop"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Opening shop for early morning customers."]
    
  - hour: 12
    targetObjectTag: "NPCTarget"
    targetObjectName: "MarketSquare"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Midday market is busiest trading time."]
    
  - hour: 15
    targetObjectTag: "Shop"
    targetObjectName: "MerchantShop"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Afternoon inventory and customer service."]
    
  - hour: 18
    targetObjectTag: "NPCTarget"
    targetObjectName: "TradingPost"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Evening negotiations with traveling traders."]
    
  - hour: 20
    targetObjectTag: "Shop"
    targetObjectName: "MerchantShop"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Closing shop and counting day's earnings."]
    
  - hour: 22
    targetObjectTag: "House"
    targetObjectName: "MerchantHouse"
    behavior: Sleep
    shouldIdleWhenReached: true
    shouldDespawn: false
```

### Bu Penjual (Food Vendor) Schedule

**NPC ID:** `bu_penjual`
**Schedule Name:** "Village Food Service"

```yaml
scheduleName: "Food Vendor Schedule"
scheduleDescription: "Meal preparation and food service for village"
spawnHour: 5
homeObjectTag: "House"
homeObjectName: "VendorHouse"
walkSpeed: 1.1
pauseAtDestination: 2.0
moveAroundWhenIdle: true
idleMovementRange: 2.5

scheduleEvents:
  - hour: 5
    targetObjectTag: "NPCTarget"
    targetObjectName: "FoodPrepArea"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Early morning food preparation for breakfast service."]
    
  - hour: 7
    targetObjectTag: "NPCTarget"
    targetObjectName: "FoodStall"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Breakfast service for early working villagers."]
    
  - hour: 10
    targetObjectTag: "NPCTarget"
    targetObjectName: "FoodPrepArea"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Preparing lunch specialties and snacks."]
    
  - hour: 12
    targetObjectTag: "NPCTarget"
    targetObjectName: "FoodStall"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Busy lunch service for hungry villagers."]
    
  - hour: 15
    targetObjectTag: "House"
    targetObjectName: "VendorHouse"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Afternoon rest before evening service."]
    
  - hour: 17
    targetObjectTag: "NPCTarget"
    targetObjectName: "FoodStall"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Evening food service for family dinners."]
    
  - hour: 20
    targetObjectTag: "House"
    targetObjectName: "VendorHouse"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Cleaning up and preparing for tomorrow."]
    
  - hour: 22
    targetObjectTag: "House"
    targetObjectName: "VendorHouse"
    behavior: Sleep
    shouldIdleWhenReached: true
    shouldDespawn: false
```

### Pak Lurah (Village Chief) Schedule

**NPC ID:** `pak_lurah`
**Schedule Name:** "Village Leadership Duties"

```yaml
scheduleName: "Village Chief Administrative Schedule"
scheduleDescription: "Leadership duties and village administration"
spawnHour: 6
homeObjectTag: "House"
homeObjectName: "ChiefHouse"
walkSpeed: 1.0
pauseAtDestination: 4.0
moveAroundWhenIdle: false
idleMovementRange: 1.0

scheduleEvents:
  - hour: 6
    targetObjectTag: "NPCTarget"
    targetObjectName: "ChiefOffice"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Morning administrative duties and planning."]
    
  - hour: 9
    targetObjectTag: "NPCTarget"
    targetObjectName: "VillageCenter"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Meeting with villagers about their concerns."]
    
  - hour: 12
    targetObjectTag: "House"
    targetObjectName: "ChiefHouse"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Midday break for family meal."]
    
  - hour: 14
    targetObjectTag: "NPCTarget"
    targetObjectName: "VillageCenter"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Afternoon consultations and decision making."]
    
  - hour: 17
    targetObjectTag: "NPCTarget"
    targetObjectName: "ChiefOffice"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Evening paperwork and planning tomorrow."]
    
  - hour: 19
    targetObjectTag: "House"
    targetObjectName: "ChiefHouse"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Family time and personal reflection."]
    
  - hour: 22
    targetObjectTag: "House"
    targetObjectName: "ChiefHouse"
    behavior: Sleep
    shouldIdleWhenReached: true
    shouldDespawn: false
```

### Bu Guru (Teacher) Schedule

**NPC ID:** `bu_guru`
**Schedule Name:** "Village Education Schedule"

```yaml
scheduleName: "Teacher's Educational Schedule"
scheduleDescription: "Teaching children and preserving cultural knowledge"
spawnHour: 6
homeObjectTag: "House"
homeObjectName: "TeacherHouse"
walkSpeed: 1.1
pauseAtDestination: 2.5
moveAroundWhenIdle: true
idleMovementRange: 2.0

scheduleEvents:
  - hour: 6
    targetObjectTag: "NPCTarget"
    targetObjectName: "SchoolPrep"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Preparing lessons and materials for children."]
    
  - hour: 8
    targetObjectTag: "NPCTarget"
    targetObjectName: "SchoolArea"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Morning lessons - reading and writing practice."]
    
  - hour: 12
    targetObjectTag: "NPCTarget"
    targetObjectName: "SchoolArea"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Midday break - children play while teacher rests."]
    
  - hour: 14
    targetObjectTag: "NPCTarget"
    targetObjectName: "SchoolArea"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Afternoon cultural lessons and storytelling."]
    
  - hour: 16
    targetObjectTag: "House"
    targetObjectName: "TeacherHouse"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Preparing tomorrow's lessons and grading work."]
    
  - hour: 18
    targetObjectTag: "NPCTarget"
    targetObjectName: "CommunityArea"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Evening community engagement and parent meetings."]
    
  - hour: 21
    targetObjectTag: "House"
    targetObjectName: "TeacherHouse"
    behavior: Sleep
    shouldIdleWhenReached: true
    shouldDespawn: false
```

### Dukun Kampung (Village Shaman) Schedule

**NPC ID:** `dukun_kampung`
**Schedule Name:** "Spiritual Healer Routine"

```yaml
scheduleName: "Village Shaman Spiritual Schedule"
scheduleDescription: "Traditional healing and spiritual guidance"
spawnHour: 5
homeObjectTag: "NPCTarget"
homeObjectName: "ShamanHut"
walkSpeed: 0.9
pauseAtDestination: 5.0
moveAroundWhenIdle: true
idleMovementRange: 1.5

scheduleEvents:
  - hour: 5
    targetObjectTag: "NPCTarget"
    targetObjectName: "SacredGrove"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Dawn meditation connects with spiritual realm."]
    
  - hour: 8
    targetObjectTag: "NPCTarget"
    targetObjectName: "HerbGathering"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Gathering sacred herbs for healing preparations."]
    
  - hour: 11
    targetObjectTag: "NPCTarget"
    targetObjectName: "ShamanHut"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Preparing healing potions and spiritual remedies."]
    
  - hour: 14
    targetObjectTag: "NPCTarget"
    targetObjectName: "ShamanHut"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Available for healing consultations and guidance."]
    
  - hour: 17
    targetObjectTag: "NPCTarget"
    targetObjectName: "SacredShrine"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Evening prayers and spiritual communication."]
    
  - hour: 20
    targetObjectTag: "NPCTarget"
    targetObjectName: "ShamanHut"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Night consultations for serious spiritual matters."]
    
  - hour: 23
    targetObjectTag: "NPCTarget"
    targetObjectName: "ShamanHut"
    behavior: Sleep
    shouldIdleWhenReached: true
    shouldDespawn: false
```

---

## Community NPC Schedules

### Anak Gembala (Shepherd Boy) Schedule

**NPC ID:** `anak_gembala`
**Schedule Name:** "Shepherd Pastoral Duties"

```yaml
scheduleName: "Young Shepherd Schedule"
scheduleDescription: "Energetic boy caring for village livestock"
spawnHour: 6
homeObjectTag: "House"
homeObjectName: "ShepherdHouse"
walkSpeed: 1.6
pauseAtDestination: 1.5
moveAroundWhenIdle: true
idleMovementRange: 4.0

scheduleEvents:
  - hour: 6
    targetObjectTag: "Farm"
    targetObjectName: "GoatPen"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Morning feeding and health check for goats."]
    
  - hour: 8
    targetObjectTag: "NPCTarget"
    targetObjectName: "PastureField1"
    behavior: Walk
    shouldIdleWhenReached: true
    customDialogue: ["Leading goats to fresh pasture for grazing."]
    
  - hour: 12
    targetObjectTag: "NPCTarget"
    targetObjectName: "PastureField2"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Midday rest while goats graze in shade."]
    
  - hour: 15
    targetObjectTag: "NPCTarget"
    targetObjectName: "WateringSpot"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Taking goats to water source for drinking."]
    
  - hour: 17
    targetObjectTag: "Farm"
    targetObjectName: "GoatPen"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Evening return to pen and final feeding."]
    
  - hour: 19
    targetObjectTag: "House"
    targetObjectName: "ShepherdHouse"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Home for family dinner and evening chores."]
    
  - hour: 21
    targetObjectTag: "House"
    targetObjectName: "ShepherdHouse"
    behavior: Sleep
    shouldIdleWhenReached: true
    shouldDespawn: false
```

### Pemuda Desa (Village Youth) Schedule

**NPC ID:** `pemuda_desa`
**Schedule Name:** "Energetic Village Helper"

```yaml
scheduleName: "Village Youth Activity Schedule"
scheduleDescription: "Flexible helping and community activities"
spawnHour: 7
homeObjectTag: "House"
homeObjectName: "YouthHouse"
walkSpeed: 1.8
pauseAtDestination: 1.0
moveAroundWhenIdle: true
idleMovementRange: 3.5

scheduleEvents:
  - hour: 7
    targetObjectTag: "NPCTarget"
    targetObjectName: "VillageCenter"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Morning gathering to plan daily village work."]
    
  - hour: 9
    targetObjectTag: "NPCTarget"
    targetObjectName: "ConstructionSite"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Morning construction and heavy lifting work."]
    
  - hour: 12
    targetObjectTag: "NPCTarget"
    targetObjectName: "VillageCenter"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Midday break and socializing with friends."]
    
  - hour: 14
    targetObjectTag: "NPCTarget"
    targetObjectName: "WorkArea"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Afternoon community projects and repairs."]
    
  - hour: 17
    targetObjectTag: "NPCTarget"
    targetObjectName: "RecreationArea"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Evening games and physical activities."]
    
  - hour: 20
    targetObjectTag: "House"
    targetObjectName: "YouthHouse"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Family time and preparing for tomorrow."]
    
  - hour: 22
    targetObjectTag: "House"
    targetObjectName: "YouthHouse"
    behavior: Sleep
    shouldIdleWhenReached: true
    shouldDespawn: false
```

### Nenek Bijak (Wise Elder) Schedule

**NPC ID:** `nenek_bijak`
**Schedule Name:** "Elder Wisdom Keeper"

```yaml
scheduleName: "Wise Elder Cultural Schedule"
scheduleDescription: "Traditional wisdom and storytelling keeper"
spawnHour: 6
homeObjectTag: "House"
homeObjectName: "ElderHouse"
walkSpeed: 0.7
pauseAtDestination: 6.0
moveAroundWhenIdle: false
idleMovementRange: 1.0

scheduleEvents:
  - hour: 6
    targetObjectTag: "NPCTarget"
    targetObjectName: "ElderGarden"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Morning reflection and communion with nature."]
    
  - hour: 9
    targetObjectTag: "NPCTarget"
    targetObjectName: "CommunityArea"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Morning consultations and village wisdom sharing."]
    
  - hour: 12
    targetObjectTag: "House"
    targetObjectName: "ElderHouse"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Midday rest and preparation of traditional crafts."]
    
  - hour: 15
    targetObjectTag: "NPCTarget"
    targetObjectName: "CommunityArea"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Afternoon guidance for village problems."]
    
  - hour: 18
    targetObjectTag: "NPCTarget"
    targetObjectName: "StorytellingSpot"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Evening storytelling for children and adults."]
    
  - hour: 21
    targetObjectTag: "House"
    targetObjectName: "ElderHouse"
    behavior: Sleep
    shouldIdleWhenReached: true
    shouldDespawn: false
```

---

## Supporting NPC Schedules

### Penjaga Gerbang (Gate Keeper) Schedule

**NPC ID:** `penjaga_gerbang`

```yaml
scheduleName: "Village Gate Security Schedule"
scheduleDescription: "24/7 village entrance security (shift rotation)"
spawnHour: 0
homeObjectTag: "NPCTarget"
homeObjectName: "VillageGate"
walkSpeed: 1.0
pauseAtDestination: 8.0
moveAroundWhenIdle: true
idleMovementRange: 2.0

scheduleEvents:
  - hour: 0
    targetObjectTag: "NPCTarget"
    targetObjectName: "VillageGate"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Night watch protects village from danger."]
    
  - hour: 6
    targetObjectTag: "NPCTarget"
    targetObjectName: "VillageGate"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Morning shift begins - watching for travelers."]
    
  - hour: 12
    targetObjectTag: "NPCTarget"
    targetObjectName: "GuardHouse"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Midday break but always alert for visitors."]
    
  - hour: 18
    targetObjectTag: "NPCTarget"
    targetObjectName: "VillageGate"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Evening watch - securing village for night."]
```

### Pemburu (Hunter) Schedule

**NPC ID:** `pemburu`

```yaml
scheduleName: "Forest Hunter Schedule"
scheduleDescription: "Forest hunting and wildlife management"
spawnHour: 4
homeObjectTag: "House"
homeObjectName: "HunterCabin"
walkSpeed: 1.4
pauseAtDestination: 3.0
moveAroundWhenIdle: true
idleMovementRange: 5.0

scheduleEvents:
  - hour: 4
    targetObjectTag: "NPCTarget"
    targetObjectName: "ForestEdge"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Pre-dawn hunting is most successful."]
    
  - hour: 8
    targetObjectTag: "NPCTarget"
    targetObjectName: "HuntingGrounds"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Morning tracking and forest patrol."]
    
  - hour: 12
    targetObjectTag: "NPCTarget"
    targetObjectName: "ForestClearing"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Midday rest and animal observation."]
    
  - hour: 16
    targetObjectTag: "NPCTarget"
    targetObjectName: "VillageCenter"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Returning to village with forest reports."]
    
  - hour: 19
    targetObjectTag: "House"
    targetObjectName: "HunterCabin"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Evening equipment maintenance."]
    
  - hour: 22
    targetObjectTag: "House"
    targetObjectName: "HunterCabin"
    behavior: Sleep
    shouldIdleWhenReached: true
    shouldDespawn: false
```

### Nelayan (Fisherman) Schedule

**NPC ID:** `nelayan`

```yaml
scheduleName: "River Fisherman Schedule"
scheduleDescription: "River fishing and water knowledge"
spawnHour: 4
homeObjectTag: "House"
homeObjectName: "FishermanHut"
walkSpeed: 1.2
pauseAtDestination: 4.0
moveAroundWhenIdle: true
idleMovementRange: 3.0

scheduleEvents:
  - hour: 4
    targetObjectTag: "NPCTarget"
    targetObjectName: "FishingSpot1"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Early morning fishing when fish are active."]
    
  - hour: 8
    targetObjectTag: "NPCTarget"
    targetObjectName: "FishingSpot2"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Moving to different spots for variety."]
    
  - hour: 11
    targetObjectTag: "NPCTarget"
    targetObjectName: "RiverBank"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Processing and preparing morning catch."]
    
  - hour: 14
    targetObjectTag: "House"
    targetObjectName: "FishermanHut"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Midday rest and net repair."]
    
  - hour: 17
    targetObjectTag: "NPCTarget"
    targetObjectName: "FishingSpot3"
    behavior: Work
    shouldIdleWhenReached: true
    customDialogue: ["Evening fishing session."]
    
  - hour: 20
    targetObjectTag: "House"
    targetObjectName: "FishermanHut"
    behavior: Idle
    shouldIdleWhenReached: true
    customDialogue: ["Cleaning equipment and preparing for tomorrow."]
    
  - hour: 22
    targetObjectTag: "House"
    targetObjectName: "FishermanHut"
    behavior: Sleep
    shouldIdleWhenReached: true
    shouldDespawn: false
```

---

## Special Event Schedules

### Water Crisis Modified Schedules

During the water crisis period (when `water_crisis_discovered` flag is active), several NPCs have modified schedules:

#### Modified Pak Tani Schedule (Water Crisis)
```yaml
# Additional event during water crisis
- hour: 10
  targetObjectTag: "Well"
  targetObjectName: "VillageWell"
  behavior: Work
  shouldIdleWhenReached: true
  customDialogue: ["Checking well water levels for irrigation."]
  requiredFlags: ["water_crisis_discovered"]
```

#### Modified Nelayan Schedule (Water Crisis)
```yaml
# Modified fishing during water crisis
- hour: 6
  targetObjectTag: "NPCTarget"
  targetObjectName: "FishingSpot1"
  behavior: Idle
  shouldIdleWhenReached: true
  customDialogue: ["River levels too low for proper fishing."]
  requiredFlags: ["water_crisis_discovered"]
```

### Festival Period Schedules

During village festivals (when `festival_active` flag is set):

#### Festival Bu Penjual Schedule
```yaml
# Extended food service during festivals
- hour: 5
  targetObjectTag: "NPCTarget"
  targetObjectName: "FestivalFoodArea"
  behavior: Work
  shouldIdleWhenReached: true
  customDialogue: ["Preparing special festival foods for celebration."]
  requiredFlags: ["festival_active"]
```

---

## Implementation Notes

### Tag Requirements
All schedules require these Unity GameObject tags to be set up in the scene:

**Essential Tags:**
- `House` - For NPC homes and sleeping locations
- `NPCTarget` - For general NPC activity locations
- `Farm` - For agricultural areas
- `Shop` - For merchant and trading areas
- `Well` - For water-related activities

**Specific Object Names Required:**
- `PadepokanMasterQuarters`, `PadepokanMainHall`, `PadepokanCourtyard`
- `SaraswatiHouse`, `KitchenArea`, `HerbGarden`
- `MbokRandaHouse`, `ElephantEnclosure`, `VillageWell`
- `FarmerHouse`, `RiceField1`, `RiceField2`, `RiceField3`
- `MerchantShop`, `MarketSquare`, `TradingPost`
- `ChiefOffice`, `VillageCenter`, `SchoolArea`
- `ShamanHut`, `SacredGrove`, `SacredShrine`

### Schedule Validation
Each schedule includes validation for:
- Required GameObject existence in scene
- Proper tag assignment
- Logical time progression
- Behavior consistency

### Dynamic Schedule Modifications
Schedules can be modified based on:
- Story progression flags
- Seasonal changes
- Special events (festivals, crises)
- Player actions and choices

### Performance Considerations
- NPC schedules use caching system for GameObject lookups
- Idle movement ranges are optimized for performance
- Schedule events are triggered only when NPCs are active
- Despawn/spawn system reduces memory usage during off-hours