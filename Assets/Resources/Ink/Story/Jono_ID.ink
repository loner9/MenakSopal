// Jono - Simple NPC with time-based greetings
// Migrated from: Jono.asset

// ============================================
// VARIABLES
// ============================================
// Time of day: 0=Morning, 1=Day, 2=Afternoon, 3=Night
VAR time_of_day = 0
VAR dialogue_index = 0

// ============================================
// MAIN ENTRY POINT
// ============================================
=== main ===
-> greetings

=== greetings ===
{time_of_day:
    - 0: # speaker: Jono
        Berlatihlah dengan sungguh sungguh nak -> main_dialogue
    - 1: # speaker: Jono
        Pagi yang cerah untuk beraktivitas! -> main_dialogue
    - 2: # speaker: Jono
        Sore kawan! -> main_dialogue
    - else: # speaker: Jono
        Selamat malam -> main_dialogue
}

=== main_dialogue ===
// Cycle through different random dialogues
~ dialogue_index = RANDOM(0, 2)

{dialogue_index:
    - 0: -> dialogue_motivational
    - 1: -> dialogue_philosophical  
    - 2: -> dialogue_humorous
}

=== dialogue_motivational ===
# speaker: Jono
We walk the talk, not only talk the talk
-> farewell

=== dialogue_philosophical ===
# speaker: Jono
Percayalah, semua ini akan berlalu kawan
-> farewell

=== dialogue_humorous ===
# speaker: Jono
Kerja mulu kaya kagak
-> farewell

=== farewell ===
# speaker: Jono
Baiklah, hati hati dijalan!
-> DONE
