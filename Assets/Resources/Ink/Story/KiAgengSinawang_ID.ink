// Ki Ageng Sinawang - Spiritual Leader and Menak Sopal's Teacher
// Migrated from: KiAgengSinawang_ID.asset

// ============================================
// VARIABLES (synced with game flag system)
// ============================================
VAR story_started = false
VAR first_contact = false
VAR water_crisis_discovered = false
VAR asked_permission_water_project = false
VAR story_completed = false
VAR reconciliation_complete = false
VAR talked_with_witness = false
VAR hearing_situation = false
VAR talk_ki_ageng = false

// Time of day: 0=Morning, 1=Day, 2=Afternoon, 3=Night
VAR time_of_day = 0

// External functions for quest integration
EXTERNAL startQuest(questId)
EXTERNAL completeQuest(questId)
EXTERNAL completeObjective(questId, objectiveId)
EXTERNAL addFlag(flagName)
EXTERNAL removeFlag(flagName)

// ============================================
// MAIN ENTRY POINT
// ============================================
=== main ===
// Greeting based on time of day
-> greetings

=== greetings ===
{time_of_day:
    - 0: # speaker: Ki Ageng Sinawang
        Berlatihlah dengan sungguh sungguh nak
    - 1: # speaker: Ki Ageng Sinawang  
        Pagi, muridku
    - 3: # speaker: Ki Ageng Sinawang
        Hmmm...
}

// After greeting, go to main dialogue
-> story_dialogue

// ============================================
// STORY DIALOGUES
// ============================================
=== story_dialogue ===

// Story completion dialogue (highest priority)
{story_completed && reconciliation_complete:
    -> story_completed_dialogue
}

// After meditation results
{talk_ki_ageng:
    -> meditation_results_dialogue
}

// After witnessing dam destruction  
{talked_with_witness:
    -> witness_dialogue
}

// Water crisis discovered - offer help
{water_crisis_discovered:
    -> water_crisis_dialogue
}

// Initial story start
{story_started:
    -> initial_dialogue
}

// Default - generic conversation
-> generic_dialogue

=== initial_dialogue ===
# speaker: Ki Ageng Sinawang
# important
Ah, Menak Sopal. Kekeringan ini membuat kita dan para warga dalam selimut kegelisahan. Bercengkeramalah dengan warga desa, mereka membutuhkan bantuanmu...
~ story_started = true
~ first_contact = true
~ addFlag("story_started")
~ addFlag("first_contact")
-> DONE

=== water_crisis_dialogue ===
# speaker: Ki Ageng Sinawang
Penderitaan warga disekitar kita memberatkan hatimu, muridku. Terkadang perbuatan mulia yang terbesar memerlukan pengorbanan yang besar pula.

+ [Ingin membantu]
    # speaker: Menak Sopal
    Ki Ageng, atas izinmu, izinkanlah aku untuk membantu tentang masalah ini. Diriku terpanggil untuk membantu tentang masalah ini
    ~ asked_permission_water_project = true
    ~ addFlag("asked_permission_water_project")
    ~ startQuest("gather_construction_helpers")
    ~ completeObjective("seek_guru_guidance", "receive_permission")
    -> guru_blessing

=== guru_blessing ===
# speaker: Ki Ageng Sinawang
# important
Belas kasihanmu menghormati ajaran kita, muridku. Bergegaslah, restuku membersamai niat baikmu.
-> DONE

=== witness_dialogue ===
# speaker: Ki Ageng Sinawang
# important
Menak Sopal, perihal apa yang menimpamu. Sehingga wajahmu terlihat begitu murung sedari gerbang padepokan kita?

+ [Menjelaskan Situasi]
    # speaker: Menak Sopal
    Jadi seperti ini guru, kami telah berhasil membangun dam untuk membendung air agar dapat digunakan oleh warga sekitar dan juga padepokan. Namun entah mengapa, dam ini selalu hancur berkali kali ketika kami selesai membangunnya. Aku sungguh frustasi guru...
    ~ hearing_situation = true
    ~ addFlag("hearing_situation")
    -> guru_advice

=== guru_advice ===
# speaker: Ki Ageng Sinawang
# important
Seperti itu rupanya. Agaknya terdapat kekuatan asing tak terlihat yang mengganggu niat baik kalian. Cobalah bertapa disekitar tempat yang kau bangun dam itu muridku. Persiapkanlah dirimu, hal ini memerlukan ketenangan dan kesiapan yang matang.
-> DONE

=== meditation_results_dialogue ===
# speaker: Ki Ageng Sinawang
# important
Bagaimana muridku, apakah pertapaanmu menghasilkan sesuatu?

+ [Menjelaskan Situasi]
    # speaker: Menak Sopal
    Aku menemukan sesuatu yang menyebabkan hancurnya dam yang kami bangun berkali kali guru. Ternyata terdapat penunggu sungai yang mengingkan sesuatu dari kami, sebuah kepala gajah putih!
    ~ hearing_situation = true
    ~ addFlag("hearing_situation")
    -> white_elephant_dialogue

=== white_elephant_dialogue ===
# speaker: Ki Ageng Sinawang
# important
Hmm, gajah putih. Sebuah permintaan yang cukup sulit. Aku kenal dengan seseorang dari desa Krandon yang memilikinya. Namun aku tidak yakin ia akan memberikan gajah itu dengan begitu saja. Apakah tidak ada cara lain?

+ [Berfikir]
    # speaker: Menak Sopal
    Aku kurang tau guru. Ada baiknya aku pergi menemui penunggu sungai lagi untuk memastikannya.
    ~ hearing_situation = true
    ~ addFlag("hearing_situation")
    -> DONE

=== story_completed_dialogue ===
# speaker: Ki Ageng Sinawang
# important
Kamu telah belajar bahwa bahkan niat mulia pun dapat menyebabkan rasa sakit. Tapi dari rasa sakit ini, pemahaman tumbuh. Desa kini memiliki air, dan kamu memiliki kebijaksanaan.
-> DONE

=== generic_dialogue ===
# speaker: Ki Ageng Sinawang
Muridku, semoga harimu menyenangkan.
-> DONE
