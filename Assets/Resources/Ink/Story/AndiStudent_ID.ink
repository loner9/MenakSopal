// Andi (Murid Padepokan) - Student helper for dam project
// Migrated from: AndiStudent_ID.asset

// ============================================
// VARIABLES (synced with game flag system)
// ============================================
VAR committed_to_help = false
VAR guru_guidance_received = false
VAR student_helpers_recruited = false
VAR npc_to_river = false
VAR andi_comment_after_dam = false
VAR dam_broken = false

// Time of day: 0=Morning, 1=Day, 2=Afternoon, 3=Night
VAR time_of_day = 0

// External functions for quest integration
EXTERNAL completeObjective(questId, objectiveId)
EXTERNAL addFlag(flagName)

// ============================================
// MAIN ENTRY POINT
// ============================================
=== main ===
-> greetings

=== greetings ===
{time_of_day:
    - 0: # speaker: Andi (Murid Padepokan)
        Terik matahari bukan alasan untuk bermalas malasan!
    - 1: # speaker: Andi (Murid Padepokan)
        Pagi menak sopal!
    - 3: # speaker: Andi (Murid Padepokan)
        Ah, malam menak sopal
}

-> story_dialogue

// ============================================
// STORY DIALOGUES
// ============================================
=== story_dialogue ===

// Comment after dam completion
{andi_comment_after_dam:
    -> after_dam_dialogue
}

// At the river site
{npc_to_river:
    -> river_site_dialogue
}

// Recruitment dialogue
{committed_to_help && guru_guidance_received:
    -> recruitment_dialogue
}

// Default
-> generic_dialogue

=== recruitment_dialogue ===
# speaker: Andi (Murid Padepokan)
Menak Sopal! Aku dengar tentang proyek bendunganmu. Bisakah kami membantu? Kami kuat dan bersemangat melayani masyarakat!

+ [Ya, saya sangat membutuhkan tenaga bantuan tambahan!]
    ~ student_helpers_recruited = true
    ~ addFlag("student_helpers_recruited")
    ~ completeObjective("gather_construction_helpers", "gather_students")
    # speaker: Andi (Murid Padepokan)
    Baiklah! Kami siap membantu, Menak Sopal!
    -> DONE

=== river_site_dialogue ===
# speaker: Andi (Murid Padepokan)
Hmm, tempat ini memang agak berbahaya. Aku akan membangun pagar dan membersihkan tempat ini agar lebih aman. Jika ada sesuatu berkabar saja ya!
-> DONE

=== after_dam_dialogue ===
# speaker: Andi (Murid Padepokan)
Syukurlah bendungan ini akhirnya selesai kita bangun. Walau entah mengapa tadi bangunan ini roboh terus menerus. Semoga kali ini kita benar benar menyelesaikan bangunan ini, Menak Sopal...
~ dam_broken = true
~ addFlag("dam_broken")
-> DONE

=== generic_dialogue ===
# speaker: Andi (Murid Padepokan)
Semangat, Menak Sopal!
-> DONE
