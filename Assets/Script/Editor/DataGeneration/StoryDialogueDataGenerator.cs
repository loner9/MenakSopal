using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR

/// <summary>
/// Generates DialogueData ScriptableObjects for story NPCs based on the documentation
/// Run this from Tools -> Trenggalek Game -> Generate Story Dialogue Data
/// </summary>
public class StoryDialogueDataGenerator : EditorWindow
{
    [MenuItem("Tools/Trenggalek Game/Generate Story Dialogue Data")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(StoryDialogueDataGenerator));
    }

    /// <summary>
    /// Static method to generate all story dialogues without opening the window
    /// </summary>
    public static void GenerateAllStoryDialoguesStatic()
    {
        var generator = CreateInstance<StoryDialogueDataGenerator>();
        generator.outputPath = "Assets/Resources/Dialogues/Story/";
        generator.generateIndonesian = true;
        generator.generateEnglish = true;
        generator.GenerateAllStoryDialogues();
        DestroyImmediate(generator);
    }

    private bool generateIndonesian = true;
    private bool generateEnglish = true;
    private string outputPath = "Assets/Resources/Dialogues/Story/";

    private void OnGUI()
    {
        titleContent = new GUIContent("Story Dialogue Generator");
        
        GUILayout.Label("Story NPC Dialogue Data Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        generateIndonesian = EditorGUILayout.Toggle("Generate Indonesian Dialogues", generateIndonesian);
        generateEnglish = EditorGUILayout.Toggle("Generate English Dialogues", generateEnglish);
        
        GUILayout.Space(10);
        outputPath = EditorGUILayout.TextField("Output Path:", outputPath);
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Generate All Story Dialogues", GUILayout.Height(40)))
        {
            GenerateAllStoryDialogues();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate Ki Ageng Sinawang Only"))
        {
            GenerateKiAgengDialogue();
        }
        
        if (GUILayout.Button("Generate Mbok Randa Krandon Only"))
        {
            GenerateMbokRandaDialogue();
        }
        
        if (GUILayout.Button("Generate Buaya Putih Only"))
        {
            GenerateBuayaPutihDialogue();
        }
        
        if (GUILayout.Button("Generate Raden Ayu Saraswati Only"))
        {
            GenerateRadenAyuDialogue();
        }
    }

    private void GenerateAllStoryDialogues()
    {
        Debug.Log("Starting Story Dialogue Generation...");
        
        // Ensure output directory exists
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        
        GenerateKiAgengDialogue();
        GenerateRadenAyuDialogue();
        GenerateMbokRandaDialogue();
        GenerateBuayaPutihDialogue();
        GenerateSupportingCharacters();
        
        AssetDatabase.Refresh();
        Debug.Log("✅ Story Dialogue Generation Complete!");
    }

    #region Ki Ageng Sinawang Generation
    
    private void GenerateKiAgengDialogue()
    {
        if (generateIndonesian)
        {
            var dialogueData = CreateKiAgengDialogue_Indonesian();
            string path = Path.Combine(outputPath, "KiAgengSinawang_ID.asset");
            AssetDatabase.CreateAsset(dialogueData, path);
            Debug.Log($"✅ Created: {path}");
        }
        
        if (generateEnglish)
        {
            var dialogueData = CreateKiAgengDialogue_English();
            string path = Path.Combine(outputPath, "KiAgengSinawang_EN.asset");
            AssetDatabase.CreateAsset(dialogueData, path);
            Debug.Log($"✅ Created: {path}");
        }
    }
    
    private DialogueData CreateKiAgengDialogue_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Ki Ageng Sinawang";
        dialogueData.dialogueDescription = "Pemimpin padepokan, guru spiritual Menak Sopal";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        // Salam Awal (Pre-Story)
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Ki Ageng Sinawang",
            dialogueText = "Ah, Menak Sopal. Aku merasakan hatimu gelisah hari ini. Angin bercerita tentang perubahan yang akan datang ke tanah kita.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { "story_started" },
            isRepeatable = true,
            isImportantDialogue = false
        });
        
        // Fase Cerita 1 - Setelah Penemuan Krisis Air
        var crisisDialogue = new DialogueEntry
        {
            speakerName = "Ki Ageng Sinawang",
            dialogueText = "Penderitaan rakyat kita memberatkan hatimu, anakku. Terkadang perbuatan mulia yang terbesar memerlukan pengorbanan yang besar pula.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day, TimeOfDay.Sunset },
            requiredFlags = new string[] { "water_crisis_discovered" },
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Guru, saya ingin membantu mengatasi kekurangan air ini",
                    flagsToAdd = new string[] { "asked_permission_water_project" },
                    questToStart = "dam_construction_project",
                    response = new DialogueResponse
                    {
                        speakerName = "Ki Ageng Sinawang",
                        responseText = "Belas kasihanmu menghormati ajaran kita. Pergilah, tapi ingatlah - kebijaksanaan sejati terletak pada pemahaman semua konsekuensi dari tindakan kita."
                    }
                },
                new DialogueChoice
                {
                    choiceText = "Menurut Guru, apa yang harus saya lakukan?",
                    response = new DialogueResponse
                    {
                        speakerName = "Ki Ageng Sinawang",
                        responseText = "Jawabannya ada dalam dirimu, nak. Dengarkan hatimu, tapi tempa dengan kebijaksanaan. Jalan seorang penolong tidak pernah sederhana."
                    }
                }
            }
        };
        dialogueEntries.Add(crisisDialogue);
        
        // Fase Cerita 2 - Bantuan Pembangunan Bendungan
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Ki Ageng Sinawang",
            dialogueText = "Ajaklah beberapa murid kita untuk membantumu. Tangan-tangan muda yang bekerja bersama dapat memindahkan gunung - atau dalam hal ini, membangun sungai.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { "dam_construction_started" },
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Terima kasih, Guru. Kebijaksanaan Guru membimbing saya",
                    flagsToAdd = new string[] { "students_permission_granted" },
                    questToStart = "gather_construction_helpers"
                }
            }
        });
        
        // Fase Cerita 3 - Konsultasi Mistis
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Ki Ageng Sinawang",
            dialogueText = "Aku merasakan kekuatan spiritual gelap sedang bekerja. Roh-roh sungai itu kuno dan angkuh. Mereka tidak suka dengan pembangunan yang tidak diundang.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { "dam_repeatedly_destroyed" },
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Bagaimana aku bisa menenangkan roh-roh sungai?",
                    response = new DialogueResponse
                    {
                        speakerName = "Ki Ageng Sinawang",
                        responseText = "Roh-roh sering menuntut persembahan atau penghormatan. Carilah komunikasi dulu, nak. Kekerasan haruslah jalan terakhir."
                    }
                },
                new DialogueChoice
                {
                    choiceText = "Apakah ada bahaya dalam menghadapi roh-roh ini?",
                    response = new DialogueResponse
                    {
                        speakerName = "Ki Ageng Sinawang",
                        responseText = "Semua urusan spiritual mengandung risiko. Tapi niat sucimu mungkin akan melindungimu. Percayalah pada latihanmu."
                    }
                }
            }
        });
        
        // Fase Cerita 4 - Dilema Gajah Putih
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Ki Ageng Sinawang",
            dialogueText = "Mbok Randa Krandon berhati baik, meski temperamental. Dia akan mengerti jika kamu menjelaskan kebaikan yang lebih besar yang dilayani tindakanmu.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day, TimeOfDay.Sunset },
            requiredFlags = new string[] { "white_elephant_taken", "mbok_randa_angry" },
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Dia sangat marah padaku. Bagaimana aku bisa memperbaiki ini?",
                    flagsToAdd = new string[] { "guru_advice_reconciliation" },
                    response = new DialogueResponse
                    {
                        speakerName = "Ki Ageng Sinawang",
                        responseText = "Kebenaran yang diucapkan dengan penyesalan tulus dapat menyembuhkan banyak luka. Tunjukkan padanya kebaikan yang datang dari tindakanmu."
                    }
                }
            }
        });
        
        // Kesimpulan Cerita - Refleksi Kebijaksanaan
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Ki Ageng Sinawang",
            dialogueText = "Kamu telah belajar bahwa bahkan niat mulia pun dapat menyebabkan rasa sakit. Tapi dari rasa sakit ini, pemahaman tumbuh. Desa kini memiliki air, dan kamu memiliki kebijaksanaan.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { "story_completed", "reconciliation_complete" },
            isRepeatable = true,
            isImportantDialogue = true
        });
        
        // Add casual dialogues
        var greetings = new List<DialogueEntry>();
        greetings.Add(new DialogueEntry
        {
            speakerName = "Ki Ageng Sinawang",
            dialogueText = "Gunung mengajarkan kita kesabaran, sungai mengajarkan kita ketekunan. Pelajaran apa yang akan kamu pilih untuk dipelajari hari ini?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise },
            isRepeatable = true
        });
        
        greetings.Add(new DialogueEntry
        {
            speakerName = "Ki Ageng Sinawang",
            dialogueText = "Duduklah dengan tenang di bawah pohon beringin tua saat matahari terbenam. Dengarkan apa yang dikatakan angin tentang hari esok.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day },
            isRepeatable = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        dialogueData.greetings = greetings.ToArray();
        dialogueData.loopDialogue = true;
        
        return dialogueData;
    }
    
    private DialogueData CreateKiAgengDialogue_English()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Ki Ageng Sinawang";
        dialogueData.dialogueDescription = "Padepokan leader, spiritual teacher of Menak Sopal";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        // Initial Greeting (Pre-Story)
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Ki Ageng Sinawang",
            dialogueText = "Ah, Menak Sopal. I sense your heart is restless today. The wind speaks of changes coming to our land.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { "story_started" },
            isRepeatable = true,
            isImportantDialogue = false
        });
        
        // Story Phase 1 - After Water Crisis Discovery
        var crisisDialogue = new DialogueEntry
        {
            speakerName = "Ki Ageng Sinawang",
            dialogueText = "The suffering of our people weighs on your heart, my child. Sometimes the greatest noble deeds require great sacrifice.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day, TimeOfDay.Sunset },
            requiredFlags = new string[] { "water_crisis_discovered" },
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Teacher, I wish to help solve the water shortage",
                    flagsToAdd = new string[] { "asked_permission_water_project" },
                    questToStart = "dam_construction_project",
                    response = new DialogueResponse
                    {
                        speakerName = "Ki Ageng Sinawang",
                        responseText = "Your compassion honors our teachings. Go forth, but remember - true wisdom lies in understanding all consequences of our actions."
                    }
                },
                new DialogueChoice
                {
                    choiceText = "What do you think I should do?",
                    response = new DialogueResponse
                    {
                        speakerName = "Ki Ageng Sinawang",
                        responseText = "The answer lies within you, child. Listen to your heart, but temper it with wisdom. The path of the helper is never simple."
                    }
                }
            }
        };
        dialogueEntries.Add(crisisDialogue);
        
        // Continue with other story phases...
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        
        return dialogueData;
    }
    
    #endregion
    
    #region Mbok Randa Krandon Generation
    
    private void GenerateMbokRandaDialogue()
    {
        if (generateIndonesian)
        {
            var dialogueData = CreateMbokRandaDialogue_Indonesian();
            string path = Path.Combine(outputPath, "MbokRandaKrandon_ID.asset");
            AssetDatabase.CreateAsset(dialogueData, path);
            Debug.Log($"✅ Created: {path}");
        }
        
        if (generateEnglish)
        {
            var dialogueData = CreateMbokRandaDialogue_English();
            string path = Path.Combine(outputPath, "MbokRandaKrandon_EN.asset");
            AssetDatabase.CreateAsset(dialogueData, path);
            Debug.Log($"✅ Created: {path}");
        }
    }
    
    private DialogueData CreateMbokRandaDialogue_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Mbok Randa Krandon";
        dialogueData.dialogueDescription = "Pemilik gajah putih, mewakili konflik dan pemahaman akhir";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        // Pertemuan Pertama - Sambutan Mencurigakan
        var firstMeeting = new DialogueEntry
        {
            speakerName = "Mbok Randa Krandon",
            dialogueText = "Pemuda dari padepokan? Apa yang membawamu sejauh ini dari rumah, nak?",
            requiredFlags = new string[] { "arrived_desa_krandon" },
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Saya datang mencari gajah putih Mbok",
                    flagsToAdd = new string[] { "requested_elephant_directly" },
                    response = new DialogueResponse
                    {
                        speakerName = "Mbok Randa Krandon",
                        responseText = "Gajahku? Itu permintaan yang aneh. Mengapa murid padepokan membutuhkan gajah berhargaku?"
                    }
                },
                new DialogueChoice
                {
                    choiceText = "Saya datang membawa salam dari Ki Ageng Sinawang",
                    response = new DialogueResponse
                    {
                        speakerName = "Mbok Randa Krandon",
                        responseText = "Ah, Ki Ageng! Aku mengenalnya ketika dia masih guru muda. Orang baik. Apa yang dia butuhkan?"
                    }
                }
            }
        };
        dialogueEntries.Add(firstMeeting);
        
        // Negosiasi
        var negotiation = new DialogueEntry
        {
            speakerName = "Mbok Randa Krandon",
            dialogueText = "Kamu ingin meminjam gajah putihku? Selama tiga hari? Itu cukup tidak biasa... tapi Ki Ageng menjaminmu.",
            requiredFlags = new string[] { "explained_water_crisis" },
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Saya berjanji akan mengembalikannya dengan selamat",
                    flagsToAdd = new string[] { "promised_safe_return" },
                    response = new DialogueResponse
                    {
                        speakerName = "Mbok Randa Krandon",
                        responseText = "Baiklah. Tapi jika ada bahaya yang menimpanya, padepokanmu akan bertanggung jawab. Tiga hari, tidak lebih."
                    }
                },
                new DialogueChoice
                {
                    choiceText = "Bagaimana jika sesuatu terjadi pada gajah itu?",
                    response = new DialogueResponse
                    {
                        speakerName = "Mbok Randa Krandon",
                        responseText = "Maka kamu akan membuat musuh yang sangat kuat. Tapi... aku percaya pada penilaian Ki Ageng terhadap karakter."
                    }
                }
            }
        };
        dialogueEntries.Add(negotiation);
        
        // Penemuan Pengkhianatan
        var betrayalDiscovery = new DialogueEntry
        {
            speakerName = "Mbok Randa Krandon",
            dialogueText = "KAMU! Kamu menipu aku! Di mana gajah putihku? Apa yang telah kamu lakukan padanya?",
            requiredFlags = new string[] { "elephant_sacrifice_revealed" },
            isImportantDialogue = true,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Saya bisa menjelaskan semuanya...",
                    flagsToAdd = new string[] { "attempted_explanation" },
                    response = new DialogueResponse
                    {
                        speakerName = "Mbok Randa Krandon",
                        responseText = "Menjelaskan? MENJELASKAN?! Kamu mengambil gajah kesayanganku dan... dan... Aku tidak seharusnya mempercayai murid padepokan!"
                    }
                },
                new DialogueChoice
                {
                    choiceText = "Ini demi kebaikan banyak orang",
                    flagsToAdd = new string[] { "justified_actions" },
                    response = new DialogueResponse
                    {
                        speakerName = "Mbok Randa Krandon",
                        responseText = "Kebaikan banyak orang? Bagaimana dengan kehilanganKU? Bagaimana dengan rasa sakitKU? Tangkap dia! Jangan biarkan dia kabur!"
                    }
                }
            }
        };
        dialogueEntries.Add(betrayalDiscovery);
        
        // Continue with other dialogue phases...
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        
        return dialogueData;
    }
    
    private DialogueData CreateMbokRandaDialogue_English()
    {
        // Similar structure but in English
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Mbok Randa Krandon";
        dialogueData.dialogueDescription = "White elephant owner, represents conflict and eventual understanding";
        
        // Implementation continues...
        return dialogueData;
    }
    
    #endregion
    
    #region Other Characters
    
    private void GenerateBuayaPutihDialogue()
    {
        // Buaya Putih (White Crocodile Spirit) dialogue generation
        Debug.Log("Generating Buaya Putih dialogue...");
    }
    
    private void GenerateRadenAyuDialogue()
    {
        // Raden Ayu Saraswati (Mother) dialogue generation
        Debug.Log("Generating Raden Ayu dialogue...");
    }
    
    private void GenerateSupportingCharacters()
    {
        // Generate dialogues for supporting characters
        // Murid Padepokan 1-3, Warga Haus 1-4, etc.
        Debug.Log("Generating supporting character dialogues...");
    }
    
    #endregion
}

#endif