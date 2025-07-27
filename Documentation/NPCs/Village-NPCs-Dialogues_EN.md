# Village NPCs Dialogue Data

This document contains comprehensive dialogue data for all gameplay-focused NPCs in the Trenggalek folklore game.

## Village NPCs Overview

### Farmers & Agriculture
- **Pak Tani (Farmer)** - Rice farming and agriculture
- **Bu Tani (Farmer's Wife)** - Crop advice and local wisdom
- **Anak Gembala (Shepherd Boy)** - Livestock and village news

### Merchants & Trade
- **Pak Pedagang (Merchant)** - General goods trader
- **Bu Penjual (Vendor)** - Food and daily necessities
- **Pengrajin Kayu (Woodcrafter)** - Tools and wooden items

### Village Life
- **Pak Lurah (Village Chief)** - Village leadership and problems
- **Bu Guru (Teacher)** - Education and village children
- **Dukun Kampung (Village Shaman)** - Traditional healing and mysticism
- **Pemuda Desa (Village Youth)** - Energy and village activities
- **Nenek Bijak (Wise Elder)** - Traditional stories and wisdom

### Utility NPCs
- **Penjaga Gerbang (Gate Keeper)** - Village entrance security
- **Pemburu (Hunter)** - Forest knowledge and hunting
- **Nelayan (Fisherman)** - River and fishing information

---

## Pak Tani (Farmer)

**NPC ID:** `pak_tani`
**Role:** Village farmer, provides agricultural quests and rice farming information

### Dialogue Entries

#### Morning Greeting
```yaml
speakerName: "Pak Tani"
dialogueText: "Selamat pagi, anak muda! The morning dew is perfect for planting today. Are you here to learn about farming?"
availableTimesOfDay: [Morning]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "Can you teach me about rice farming?"
    response:
      speakerName: "Pak Tani"
      responseText: "Rice needs water, patience, and respect for the land. If you help me in the fields, I'll teach you everything!"
  - choiceText: "I'm just passing through"
    response:
      speakerName: "Pak Tani"
      responseText: "Safe travels, child. Remember, a full stomach makes for a happy journey!"
```

#### Working Hours Conversation
```yaml
speakerName: "Pak Tani"
dialogueText: "These fields have fed our village for generations. Hard work and good harvests keep everyone fed."
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
isRepeatable: true
```

#### Quest: Help with Harvest
```yaml
speakerName: "Pak Tani"
dialogueText: "Harvest season is upon us! I could use young, strong hands to help gather the rice. Will you assist me?"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
choices:
  - choiceText: "I'll help with the harvest"
    flagsToAdd: ["pak_tani_harvest_accepted"]
    questToStart: "village_rice_harvest"
    response:
      speakerName: "Pak Tani"
      responseText: "Excellent! Meet me in the eastern rice fields. I'll show you how to cut rice properly without damaging the grain."
  - choiceText: "I'm too busy right now"
    response:
      speakerName: "Pak Tani"
      responseText: "I understand. If you change your mind, I'll be in the fields until sunset."
```

#### After Helping Quest
```yaml
speakerName: "Pak Tani"
dialogueText: "You're a natural at this! Your help made the harvest much easier. Take some rice for your journey."
availableTimesOfDay: [Any]
requiredFlags: ["village_rice_harvest_complete"]
isRepeatable: true
```

#### Water Crisis Period
```yaml
speakerName: "Pak Tani"
dialogueText: "The drought is terrible for crops. If this continues, we'll have no rice for the next planting season."
availableTimesOfDay: [Any]
requiredFlags: ["water_crisis_discovered"]
```

#### Post-Dam Construction
```yaml
speakerName: "Pak Tani"
dialogueText: "Praise be! Water flows to our fields again! The young man from the padepokan truly saved our livelihood."
availableTimesOfDay: [Any]
requiredFlags: ["dam_construction_complete"]
isRepeatable: true
```

---

## Bu Tani (Farmer's Wife)

**NPC ID:** `bu_tani`
**Role:** Farming wisdom, herbal knowledge, village recipes

### Dialogue Entries

#### Daily Wisdom
```yaml
speakerName: "Bu Tani"
dialogueText: "A good harvest starts with good seeds, but it's completed with good cooking. Would you like to learn some village recipes?"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "Please teach me to cook rice properly"
    response:
      speakerName: "Bu Tani"
      responseText: "The secret is in the water ratio and knowing when the rice is singing. Listen carefully to the bubbling!"
  - choiceText: "Do you know any herbal remedies?"
    response:
      speakerName: "Bu Tani"
      responseText: "Lemongrass for fever, ginger for stomach ache, and turmeric for wounds. Nature provides all we need!"
```

#### Quest: Gather Herbs
```yaml
speakerName: "Bu Tani"
dialogueText: "I'm preparing medicine for the village children. Could you help me gather some herbs from the forest?"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
choices:
  - choiceText: "What herbs do you need?"
    flagsToAdd: ["herb_gathering_quest_available"]
    questToStart: "gather_healing_herbs"
    response:
      speakerName: "Bu Tani"
      responseText: "I need kunyit (turmeric), jahe (ginger), and serai (lemongrass). Be careful in the forest - wild animals guard the best plants!"
```

#### Cooking Lessons
```yaml
speakerName: "Bu Tani"
dialogueText: "Would you like to learn how to cook gudeg? It's our village's specialty dish, passed down for generations."
availableTimesOfDay: [Afternoon, Evening]
requiredFlags: ["helped_with_harvest"]
choices:
  - choiceText: "Yes, please teach me!"
    flagsToAdd: ["cooking_lessons_started"]
    response:
      speakerName: "Bu Tani"
      responseText: "Wonderful! First, we need young jackfruit, coconut milk, and palm sugar. Cooking is about patience and love."
```

---

## Anak Gembala (Shepherd Boy)

**NPC ID:** `anak_gembala`
**Role:** Village news source, animal care, energetic helper

### Dialogue Entries

#### Energetic Greeting
```yaml
speakerName: "Anak Gembala"
dialogueText: "Hey there! I'm taking care of the village goats today! Have you seen any strange animals in the forest?"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "What kind of strange animals?"
    response:
      speakerName: "Anak Gembala"
      responseText: "Some of the goats have been acting scared lately. They won't go near the river! Animals sense things we humans can't."
  - choiceText: "Can you tell me about the village?"
    response:
      speakerName: "Anak Gembala"
      responseText: "Our village is the best! Everyone helps everyone. But lately, people have been worried about water..."
```

#### Animal Quest - Lost Goat
```yaml
speakerName: "Anak Gembala"
dialogueText: "Oh no! One of my goats wandered off into the forest! She's white with black spots. Can you help me find her?"
availableTimesOfDay: [Afternoon]
requiredFlags: []
choices:
  - choiceText: "I'll help you find your goat"
    flagsToAdd: ["lost_goat_quest_accepted"]
    questToStart: "find_lost_goat"
    response:
      speakerName: "Anak Gembala"
      responseText: "Thank you so much! Her name is Putih. She loves to eat young bamboo shoots, so check near the bamboo groves!"
```

#### Information Source
```yaml
speakerName: "Anak Gembala"
dialogueText: "I see everything from up here on the hill! Want to know what's happening in the village today?"
availableTimesOfDay: [Any]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "What's the latest village news?"
    response:
      speakerName: "Anak Gembala"
      responseText: "Pak Lurah had a meeting with the elders this morning. They looked very serious. And Bu Guru is teaching the children a new song!"
```

#### During Water Crisis
```yaml
speakerName: "Anak Gembala"
dialogueText: "The goats are so thirsty! Usually, they drink from the stream, but it's almost dry now. I have to carry water for them."
availableTimesOfDay: [Any]
requiredFlags: ["water_crisis_discovered"]
```

---

## Pak Pedagang (Merchant)

**NPC ID:** `pak_pedagang`
**Role:** Item vendor, trade information, economic quest giver

### Dialogue Entries

#### Shop Welcome
```yaml
speakerName: "Pak Pedagang"
dialogueText: "Welcome to my humble shop! I have goods from three villages. What can I help you find today?"
availableTimesOfDay: [Morning, Afternoon, Evening]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "What do you have for sale?"
    response:
      speakerName: "Pak Pedagang"
      responseText: "I have tools, cloth, spices, and sometimes magical items from traveling mystics. Depends on what the traders bring!"
  - choiceText: "Do you need any help with your business?"
    response:
      speakerName: "Pak Pedagang"
      responseText: "Funny you ask! I could use someone to deliver goods to the neighboring village. Interested in earning some coins?"
```

#### Trade Quest
```yaml
speakerName: "Pak Pedagang"
dialogueText: "I have a delivery that needs to reach Desa Krandon. It's valuable goods, so I need someone trustworthy. Are you interested?"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
choices:
  - choiceText: "What kind of delivery?"
    response:
      speakerName: "Pak Pedagang"
      responseText: "Herbal medicines for their village healer. The path is safe during the day, but be careful of wild animals at night."
  - choiceText: "I'll take the delivery job"
    flagsToAdd: ["delivery_quest_accepted"]
    questToStart: "merchant_delivery_krandon"
    response:
      speakerName: "Pak Pedagang"
      responseText: "Excellent! Here's the package. Deliver it to Dukun Krandon and bring back the payment. Safe travels!"
```

#### Economic Information
```yaml
speakerName: "Pak Pedagang"
dialogueText: "Trade has been difficult lately. The drought affects everyone - farmers have less to sell, people have less to buy."
availableTimesOfDay: [Any]
requiredFlags: ["water_crisis_discovered"]
```

#### Special Items (Post-Quest Rewards)
```yaml
speakerName: "Pak Pedagang"
dialogueText: "Ah, my reliable delivery person! I have some special items that just arrived. Would you like to see my premium collection?"
availableTimesOfDay: [Any]
requiredFlags: ["merchant_delivery_complete"]
isRepeatable: true
```

---

## Bu Penjual (Food Vendor)

**NPC ID:** `bu_penjual`
**Role:** Food vendor, local recipes, community gatherer

### Dialogue Entries

#### Food Welcome
```yaml
speakerName: "Bu Penjual"
dialogueText: "Fresh food! Warm rice, spicy sambal, and sweet treats! A full belly makes for a happy heart!"
availableTimesOfDay: [Morning, Afternoon, Evening]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "What's your specialty dish?"
    response:
      speakerName: "Bu Penjual"
      responseText: "My nasi gudeg is famous throughout the region! The recipe came from my grandmother's grandmother."
  - choiceText: "Can I buy some food?"
    response:
      speakerName: "Bu Penjual"
      responseText: "Of course! A growing young person needs good food. That'll be 5 copper coins for a full meal."
```

#### Community Gathering Quest
```yaml
speakerName: "Bu Penjual"
dialogueText: "The village festival is coming! I need help gathering ingredients for the community feast. Will you help me?"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
choices:
  - choiceText: "What ingredients do you need?"
    flagsToAdd: ["festival_cooking_quest_available"]
    questToStart: "gather_festival_ingredients"
    response:
      speakerName: "Bu Penjual"
      responseText: "I need fish from the river, vegetables from the farms, and spices from the forest. It'll be the best feast ever!"
```

#### Cooking Wisdom
```yaml
speakerName: "Bu Penjual"
dialogueText: "Cooking is like life - you need the right balance of sweet, salty, sour, and spicy. Too much of any one thing ruins the dish."
availableTimesOfDay: [Any]
requiredFlags: []
isRepeatable: true
```

---

## Pak Lurah (Village Chief)

**NPC ID:** `pak_lurah`
**Role:** Village leadership, major quest giver, problem solver

### Dialogue Entries

#### Formal Greeting
```yaml
speakerName: "Pak Lurah"
dialogueText: "Greetings, young one. I am the village chief. How can I serve you today?"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "I want to help the village"
    response:
      speakerName: "Pak Lurah"
      responseText: "Noble intent! A village prospers when its people work together. There are always tasks that need capable hands."
  - choiceText: "What challenges does the village face?"
    response:
      speakerName: "Pak Lurah"
      responseText: "Every village has its struggles. Currently, we worry about the dry season and keeping our people fed and healthy."
```

#### Water Crisis Leadership
```yaml
speakerName: "Pak Lurah"
dialogueText: "The water situation is becoming critical. I've called a village meeting to discuss solutions. We need immediate action."
availableTimesOfDay: [Any]
requiredFlags: ["water_crisis_discovered"]
isImportantDialogue: true
choices:
  - choiceText: "I may have a solution"
    flagsToAdd: ["offered_help_to_lurah"]
    response:
      speakerName: "Pak Lurah"
      responseText: "Any help would be greatly appreciated. The wellbeing of our people is my greatest responsibility."
```

#### Major Village Quest
```yaml
speakerName: "Pak Lurah"
dialogueText: "There's a matter of great importance to our village. Bandits have been threatening our trade routes. We need someone brave to investigate."
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: ["established_village_reputation"]
choices:
  - choiceText: "I'll investigate the bandit problem"
    flagsToAdd: ["bandit_quest_accepted"]
    questToStart: "investigate_bandit_threat"
    response:
      speakerName: "Pak Lurah"
      responseText: "I was hoping you would volunteer. You've proven yourself trustworthy. Be careful - these bandits are dangerous."
```

#### Post-Dam Gratitude
```yaml
speakerName: "Pak Lurah"
dialogueText: "Thanks to your efforts with the dam, our village has water again. You will always be welcome here, young hero."
availableTimesOfDay: [Any]
requiredFlags: ["dam_construction_complete"]
isRepeatable: true
```

---

## Bu Guru (Teacher)

**NPC ID:** `bu_guru`
**Role:** Village education, children's welfare, cultural preservation

### Dialogue Entries

#### Educational Welcome
```yaml
speakerName: "Bu Guru"
dialogueText: "Education is the light that brightens young minds! Are you here to learn, or perhaps to help teach the children?"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "Can you teach me about local history?"
    response:
      speakerName: "Bu Guru"
      responseText: "Our village has rich traditions! Every stone and tree has a story. Would you like to hear about the ancient legends?"
  - choiceText: "How can I help with the children?"
    response:
      speakerName: "Bu Guru"
      responseText: "The children love to hear stories of adventure! If you have tales to share, they would be delighted."
```

#### Quest: School Supplies
```yaml
speakerName: "Bu Guru"
dialogueText: "The children need new writing materials. Could you help me gather palm leaves and make charcoal for writing?"
availableTimesOfDay: [Morning]
requiredFlags: []
choices:
  - choiceText: "I'll help gather school supplies"
    flagsToAdd: ["school_supplies_quest_accepted"]
    questToStart: "gather_school_supplies"
    response:
      speakerName: "Bu Guru"
      responseText: "Wonderful! We need large palm leaves from the forest and burnt wood for charcoal. Education must continue!"
```

#### Cultural Preservation
```yaml
speakerName: "Bu Guru"
dialogueText: "I'm teaching the children traditional songs and stories. It's important to preserve our culture for future generations."
availableTimesOfDay: [Afternoon]
requiredFlags: []
isRepeatable: true
```

#### Children's Wellbeing Concern
```yaml
speakerName: "Bu Guru"
dialogueText: "The children are worried about the water shortage. I try to keep them hopeful, but it's difficult when parents are stressed."
availableTimesOfDay: [Any]
requiredFlags: ["water_crisis_discovered"]
```

---

## Dukun Kampung (Village Shaman)

**NPC ID:** `dukun_kampung`
**Role:** Traditional healing, spiritual guidance, mystical quests

### Dialogue Entries

#### Mystical Greeting
```yaml
speakerName: "Dukun Kampung"
dialogueText: "The spirits whisper of your arrival, young one. You carry an aura of destiny. What brings you to seek the old ways?"
availableTimesOfDay: [Any]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "I seek spiritual guidance"
    response:
      speakerName: "Dukun Kampung"
      responseText: "The path of wisdom is walked with humble steps. Meditate by the sacred banyan tree when the moon is full."
  - choiceText: "Can you teach me about traditional healing?"
    response:
      speakerName: "Dukun Kampung"
      responseText: "Healing comes from understanding the balance between body, mind, and spirit. Plants are our allies in this sacred work."
```

#### Spiritual Quest
```yaml
speakerName: "Dukun Kampung"
dialogueText: "I sense a disturbance in the spiritual realm. The river spirits are restless. Would you help me perform a cleansing ritual?"
availableTimesOfDay: [Evening, Night]
requiredFlags: ["dam_repeatedly_destroyed"]
choices:
  - choiceText: "What kind of ritual?"
    response:
      speakerName: "Dukun Kampung"
      responseText: "We must gather sacred herbs and offer prayers at the river shrine. The spirits demand respect for their domain."
  - choiceText: "I'll help with the ritual"
    flagsToAdd: ["spiritual_ritual_accepted"]
    questToStart: "river_spirit_cleansing"
    response:
      speakerName: "Dukun Kampung"
      responseText: "Good. Bring white flowers, burning incense, and a pure heart. We perform the ritual at midnight."
```

#### Healing Services
```yaml
speakerName: "Dukun Kampung"
dialogueText: "Your energy seems imbalanced. Perhaps you need spiritual cleansing? I can prepare healing herbs for you."
availableTimesOfDay: [Any]
requiredFlags: []
choices:
  - choiceText: "What kind of healing do you offer?"
    response:
      speakerName: "Dukun Kampung"
      responseText: "I heal both body and spirit. Herbal medicine for physical ailments, prayers and rituals for spiritual troubles."
```

#### Mystical Knowledge
```yaml
speakerName: "Dukun Kampung"
dialogueText: "The old spirits remember when this land was young. They speak of a great white elephant and a wise crocodile. Do these visions mean anything to you?"
availableTimesOfDay: [Night]
requiredFlags: ["seeking_white_elephant"]
isImportantDialogue: true
```

---

## Pemuda Desa (Village Youth)

**NPC ID:** `pemuda_desa`
**Role:** Energetic helper, physical quests, village activities

### Dialogue Entries

#### Enthusiastic Greeting
```yaml
speakerName: "Pemuda Desa"
dialogueText: "Hey there! You look strong! Want to join us for some village work? We're always looking for extra hands!"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "What kind of work needs doing?"
    response:
      speakerName: "Pemuda Desa"
      responseText: "Repair work, moving heavy things, clearing paths - anything that needs muscle and energy! Plus it's fun working together!"
  - choiceText: "I'm interested in helping"
    response:
      speakerName: "Pemuda Desa"
      responseText: "Excellent! Meet us at the village center after morning prayers. We'll put you to work right away!"
```

#### Construction Quest
```yaml
speakerName: "Pemuda Desa"
dialogueText: "We're building a new storage house for the village grain. Could use someone with your skills to help with the heavy lifting!"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
choices:
  - choiceText: "I'll help with construction"
    flagsToAdd: ["construction_quest_accepted"]
    questToStart: "village_construction_project"
    response:
      speakerName: "Pemuda Desa"
      responseText: "Perfect! We need to gather wood from the forest and carry stones from the quarry. Hard work, but we'll have fun!"
```

#### Athletic Challenge
```yaml
speakerName: "Pemuda Desa"
dialogueText: "Hey, you look pretty athletic! Want to race me to the old bridge? Winner gets bragging rights!"
availableTimesOfDay: [Afternoon]
requiredFlags: []
choices:
  - choiceText: "You're on! Let's race!"
    flagsToAdd: ["racing_challenge_accepted"]
    response:
      speakerName: "Pemuda Desa"
      responseText: "Haha! That's the spirit! Ready? Three, two, one... GO!"
```

#### Community Spirit
```yaml
speakerName: "Pemuda Desa"
dialogueText: "This village raised me, so I give back however I can. Everyone should contribute to their community!"
availableTimesOfDay: [Any]
requiredFlags: []
isRepeatable: true
```

---

## Nenek Bijak (Wise Elder)

**NPC ID:** `nenek_bijak`
**Role:** Traditional wisdom, folklore stories, cultural knowledge

### Dialogue Entries

#### Wise Greeting
```yaml
speakerName: "Nenek Bijak"
dialogueText: "Come here, child. These old eyes have seen many seasons, and my ears have heard countless stories. What wisdom do you seek?"
availableTimesOfDay: [Any]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "Tell me a traditional story"
    response:
      speakerName: "Nenek Bijak"
      responseText: "Ah, stories! They carry the wisdom of generations. Would you like to hear about the time the moon fell in love with a mountain?"
  - choiceText: "What advice do you have for young people?"
    response:
      speakerName: "Nenek Bijak"
      responseText: "Listen more than you speak, help more than you take, and remember that every ending is also a beginning."
```

#### Folklore Knowledge
```yaml
speakerName: "Nenek Bijak"
dialogueText: "I know the old stories of this land - tales of spirits, brave heroes, and magical animals. Which story calls to your heart?"
availableTimesOfDay: [Evening, Night]
requiredFlags: []
choices:
  - choiceText: "Tell me about the white elephant"
    flagsToAdd: ["heard_white_elephant_legend"]
    response:
      speakerName: "Nenek Bijak"
      responseText: "Ah, the sacred white elephant! Legend says it appears only to those with pure hearts and great need. It is both blessing and test."
  - choiceText: "What about river spirits?"
    response:
      speakerName: "Nenek Bijak"
      responseText: "River spirits are ancient and proud. They remember when this valley was all wild forest. Respect them, and they may help you."
```

#### Life Wisdom
```yaml
speakerName: "Nenek Bijak"
dialogueText: "Life is like weaving - individual threads may seem weak, but together they create something strong and beautiful."
availableTimesOfDay: [Any]
requiredFlags: []
isRepeatable: true
```

#### Cultural Teaching Quest
```yaml
speakerName: "Nenek Bijak"
dialogueText: "The young ones should learn the old ways before they are forgotten. Would you help me gather the children for storytelling?"
availableTimesOfDay: [Evening]
requiredFlags: []
choices:
  - choiceText: "I'll help gather the children"
    flagsToAdd: ["storytelling_quest_accepted"]
    questToStart: "gather_children_storytelling"
    response:
      speakerName: "Nenek Bijak"
      responseText: "Bless you, child. Stories are the roots that keep culture alive. Bring them to the old banyan tree when the sun sets."
```

---

## Supporting Village NPCs

### Penjaga Gerbang (Gate Keeper)

**NPC ID:** `penjaga_gerbang`

```yaml
speakerName: "Penjaga Gerbang"
dialogueText: "Halt! State your business in our village. We welcome honest travelers but watch for troublemakers."
availableTimesOfDay: [Any]
requiredFlags: []
choices:
  - choiceText: "I come in peace to help the village"
    flagsToAdd: ["peaceful_intentions_declared"]
    response:
      speakerName: "Penjaga Gerbang"
      responseText: "Good to hear. We can always use helpful hands. Go speak to Pak Lurah at the village center."
  - choiceText: "I'm just passing through"
    response:
      speakerName: "Penjaga Gerbang"
      responseText: "Safe travels then. Be careful on the forest roads - wild animals are more active lately."
```

### Pemburu (Hunter)

**NPC ID:** `pemburu`

```yaml
speakerName: "Pemburu"
dialogueText: "The forest has been strange lately. Animals are restless, and I've seen tracks I don't recognize. Something has them spooked."
availableTimesOfDay: [Morning, Afternoon, Evening]
requiredFlags: []
choices:
  - choiceText: "What kind of strange tracks?"
    response:
      speakerName: "Pemburu"
      responseText: "Large, unusual prints near the river. Not from any animal I know. Could be from the spirit realm."
  - choiceText: "Can you teach me about hunting?"
    response:
      speakerName: "Pemburu"
      responseText: "Hunting is about patience, respect, and taking only what you need. The forest provides, but it expects gratitude."
```

### Nelayan (Fisherman)

**NPC ID:** `nelayan`

```yaml
speakerName: "Nelayan"
dialogueText: "The fish are acting odd lately - swimming in circles, jumping out of water. Something's disturbing the river spirits."
availableTimesOfDay: [Morning, Evening]
requiredFlags: []
choices:
  - choiceText: "Have you seen anything unusual in the river?"
    response:
      speakerName: "Nelayan"
      responseText: "Strange ripples, shadows that move against the current. And sometimes, I swear I see white scales shimmering in the deep water."
```

---

## Implementation Notes

### Quest Integration
All village NPCs are designed to integrate with your existing QuestData system:
- Multiple quest-giving NPCs for variety
- Progressive difficulty in quest types
- Community-building objectives
- Resource gathering missions

### Flag Dependencies
Village NPCs respond to major story events:
- Water crisis affects farmer and food vendor dialogues
- Dam completion changes village mood
- Player reputation unlocks advanced quests

### Schedule Compatibility
Dialogues consider time-of-day availability:
- Farmers active during work hours
- Merchants available during trade times
- Wise elder available for evening stories
- Night-time spiritual encounters

### Community Building
NPCs create sense of living village:
- Interconnected relationships between NPCs
- Seasonal and situational dialogue variations
- Progressive familiarity with repeated interactions
- Community celebration events and festivals

### Cultural Authenticity
All dialogue maintains Indonesian cultural elements:
- Traditional greetings and respectful language
- References to authentic activities and foods
- Incorporation of local wisdom and values
- Authentic village social structures