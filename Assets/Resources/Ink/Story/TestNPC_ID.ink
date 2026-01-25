// New Dialogue - Test NPC with branching choices
// Migrated from: New Dialogue.asset

// ============================================
// VARIABLES
// ============================================
VAR player_name = ""

// ============================================
// MAIN ENTRY POINT
// ============================================
=== main ===
# speaker: Test NPC
Siapa namamu tuan?

+ [Namaku adalah sigma, kau]
    ~ player_name = "sigma"
    # speaker: Test NPC
    # pause: 1.2
    Ah seperti itu, baiklah tuan sigma
    -> second_question

+ [Aku seorang pengembara dari barat, panggil saja west]
    ~ player_name = "west"
    # speaker: Test NPC
    Wah, baiklah tuan {player_name}
    -> second_question

=== second_question ===
# speaker: Test NPC
Apa yang membawamu kemari?

+ [Aku ingin melihat lihat kota ini]
    # speaker: Test NPC
    # pause: 2
    Wah, baiklah tuan {player_name}
    -> DONE
