using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR

/// <summary>
/// Generates DialogueData ScriptableObjects for story NPCs based on the Indonesian documentation
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
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate Supporting Characters"))
        {
            GenerateMuridPadepokanDialogues();
            GenerateWargaHausDialogues();
            GenerateWargaKrandonDialogues();
            GeneratePemanduJalanDialogue();
        }
        
        if (GUILayout.Button("Generate Village NPCs"))
        {
            GenerateVillageNPCDialogues();
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
        
        // Story NPCs
        GenerateKiAgengDialogue();
        GenerateRadenAyuDialogue();
        GenerateMbokRandaDialogue();
        GenerateBuayaPutihDialogue();
        
        // Supporting Story Characters
        GenerateMuridPadepokanDialogues();
        GenerateWargaHausDialogues();
        GenerateWargaKrandonDialogues();
        GeneratePemanduJalanDialogue();
        
        // Village NPCs
        GenerateVillageNPCDialogues();
        
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
            flagsToRemove = new string[] { "game_started" },
            isRepeatable = true,
            isImportantDialogue = false
        });
        
        // Fase Cerita 1 - Setelah Penemuan Krisis Air
        var crisisDialogue = new DialogueEntry
        {
            speakerName = "Ki Ageng Sinawang",
            dialogueText = "Penderitaan rakyat kita memberatkan hatimu, muridku. Terkadang perbuatan mulia yang terbesar memerlukan pengorbanan yang besar pula.",
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
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        
        return dialogueData;
    }
    
    private DialogueData CreateKiAgengDialogue_English()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Ki Ageng Sinawang";
        dialogueData.dialogueDescription = "Padepokan leader, spiritual teacher of Menak Sopal";
        
        // Basic English implementation
        dialogueData.dialogueEntries = new DialogueEntry[0];
        
        return dialogueData;
    }
    
    #endregion
    
    #region Raden Ayu Saraswati Generation
    
    private void GenerateRadenAyuDialogue()
    {
        if (generateIndonesian)
        {
            var dialogueData = CreateRadenAyuDialogue_Indonesian();
            string path = Path.Combine(outputPath, "RadenAyuSaraswati_ID.asset");
            AssetDatabase.CreateAsset(dialogueData, path);
            Debug.Log($"✅ Created: {path}");
        }
        
        if (generateEnglish)
        {
            var dialogueData = CreateRadenAyuDialogue_English();
            string path = Path.Combine(outputPath, "RadenAyuSaraswati_EN.asset");
            AssetDatabase.CreateAsset(dialogueData, path);
            Debug.Log($"✅ Created: {path}");
        }
    }
    
    private DialogueData CreateRadenAyuDialogue_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Raden Ayu Saraswati";
        dialogueData.dialogueDescription = "Ibu Menak Sopal, sosok ibu yang mendukung";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        // Berkah Pagi
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Raden Ayu Saraswati",
            dialogueText = "Selamat pagi, menak sopal anakku. Semoga pagi ini membawa keberuntungan.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise },
            requiredFlags = new string[] { },
            isRepeatable = true
        });
        
        // Kekhawatiran Seorang Ibu
        var motherConcern = new DialogueEntry
        {
            speakerName = "Raden Ayu Saraswati",
            dialogueText = "Ibu khawatir dengan proyek bendunganmu ini, nak. Roh-roh sungai tidak boleh dianggap enteng.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunset },
            requiredFlags = new string[] { "dam_construction_started" },
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Jangan khawatir, Ibu. Saya akan berhati-hati",
                    response = new DialogueResponse
                    {
                        speakerName = "Raden Ayu Saraswati",
                        responseText = "Ayahmu dulu memiliki semangat yang sama. Ingatlah saja, keberanian tanpa kebijaksanaan adalah kecerobohan."
                    }
                },
                new DialogueChoice
                {
                    choiceText = "Apakah Ibu melihat pertanda tentang sungai?",
                    response = new DialogueResponse
                    {
                        speakerName = "Raden Ayu Saraswati",
                        responseText = "Burung-burung gelisah di dekat air. Dan pelita kelahiranmu berkedip-kedip tadi malam - ada sesuatu yang bergerak di alam spiritual."
                    }
                }
            }
        };
        dialogueEntries.Add(motherConcern);
        
        // Ibu yang Bangga
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Raden Ayu Saraswati",
            dialogueText = "Anakku telah menjadi pria sejati hari ini. Bukan karena dia memecahkan masalah, tapi karena dia belajar menghadapi konsekuensi dari pilihannya.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { "story_completed" },
            isRepeatable = true,
            isImportantDialogue = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    private DialogueData CreateRadenAyuDialogue_English()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Raden Ayu Saraswati";
        dialogueData.dialogueDescription = "Menak Sopal's mother, supportive maternal figure";
        
        // Basic English implementation
        dialogueData.dialogueEntries = new DialogueEntry[0];
        
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
                }
            }
        };
        dialogueEntries.Add(betrayalDiscovery);
        
        // // Rekonsiliasi Selesai
        // dialogueEntries.Add(new DialogueEntry
        // {
        //     speakerName = "Mbok Randa Krandon",
        //     dialogueText = "Jika tanah ini makmur dari pengorbanan gajahku, maka biarlah disebut 'Teranging Galih' - terangnya pemahaman.",
        //     availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
        //     requiredFlags = new string[] { "reconciliation_complete" },
        //     flagsToAdd = new string[] { "teranging_galih_named" },
        //     isImportantDialogue = true
        // });
        
        // dialogueData.dialogueEntries = dialogueEntries.ToArray();
        
        return dialogueData;
    }
    
    private DialogueData CreateMbokRandaDialogue_English()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Mbok Randa Krandon";
        dialogueData.dialogueDescription = "White elephant owner, represents conflict and eventual understanding";
        
        // Basic English implementation
        dialogueData.dialogueEntries = new DialogueEntry[0];
        
        return dialogueData;
    }
    
    #endregion
    
    #region Buaya Putih Generation
    
    private void GenerateBuayaPutihDialogue()
    {
        if (generateIndonesian)
        {
            var dialogueData = CreateBuayaPutihDialogue_Indonesian();
            string path = Path.Combine(outputPath, "BuayaPutih_ID.asset");
            AssetDatabase.CreateAsset(dialogueData, path);
            Debug.Log($"✅ Created: {path}");
        }
        
        if (generateEnglish)
        {
            var dialogueData = CreateBuayaPutihDialogue_English();
            string path = Path.Combine(outputPath, "BuayaPutih_EN.asset");
            AssetDatabase.CreateAsset(dialogueData, path);
            Debug.Log($"✅ Created: {path}");
        }
    }
    
    private DialogueData CreateBuayaPutihDialogue_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Buaya Putih";
        dialogueData.dialogueDescription = "Penjaga mistis, mewakili tuntutan alam dan kerjasama akhir";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        // Kontak Spiritual Pertama
        var firstContact = new DialogueEntry
        {
            speakerName = "Buaya Putih",
            dialogueText = "Siapa yang berani mengganggu air kuno tanpa meminta izin dari penjaganya?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { "spiritual_vision_active" },
            isImportantDialogue = true,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Saya Menak Sopal. Saya berusaha membantu rakyat saya",
                    response = new DialogueResponse
                    {
                        speakerName = "Buaya Putih",
                        responseText = "Membantu? Dengan membangun bendungan di sungaiKU? Niatmu mungkin murni, tapi caramu menunjukkan ketidakhormatan."
                    }
                },
                new DialogueChoice
                {
                    choiceText = "Roh agung, saya tidak bermaksud menyinggung",
                    flagsToAdd = new string[] { "showed_respect_to_spirit" },
                    response = new DialogueResponse
                    {
                        speakerName = "Buaya Putih",
                        responseText = "Penghormatan ditunjukkan melalui tindakan, bukan kata-kata. Kamu membangun tanpa bertanya, mengambil tanpa memberi."
                    }
                }
            }
        };
        dialogueEntries.Add(firstContact);
        
        // Tuntutan
        var demand = new DialogueEntry
        {
            speakerName = "Buaya Putih",
            dialogueText = "Jika kamu ingin bendunganmu berdiri, kamu harus menawarkan persembahan yang layak. Bawakan aku kepala gajah putih, dan aku akan menghentikan kerusakanku.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { "first_contact_complete" },
            isImportantDialogue = true,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Saya akan mencari gajah putih ini",
                    flagsToAdd = new string[] { "accepted_spirit_demand" },
                    questToStart = "find_white_elephant"
                }
            }
        };
        dialogueEntries.Add(demand);
        
        // Setelah Pengorbanan
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Buaya Putih",
            dialogueText = "Persembahan itu dapat diterima. Bendunganmu akan berdiri, dan air akan mengalir sesuai kebutuhan. Keseimbangan telah dipulihkan.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { "elephant_sacrifice_complete" },
            isImportantDialogue = true,
            flagsToAdd = new string[] { "spirit_pact_complete" }
        });
        
        // Penyelamatan
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Buaya Putih",
            dialogueText = "Anak muda yang menghormati cara-cara kuno, aku tidak akan membiarkanmu tenggelam. Hatimu yang murni telah mendapat perlindunganku.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { "drowning_in_river" },
            isImportantDialogue = true,
            flagsToAdd = new string[] { "rescued_by_crocodile" }
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        
        return dialogueData;
    }
    
    private DialogueData CreateBuayaPutihDialogue_English()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Buaya Putih";
        dialogueData.dialogueDescription = "Mystical guardian, represents nature's demands and final cooperation";
        
        // Basic English implementation
        dialogueData.dialogueEntries = new DialogueEntry[0];
        
        return dialogueData;
    }
    
    #endregion
    
    #region Supporting Characters Generation
    
    private void GenerateMuridPadepokanDialogues()
    {
        if (generateIndonesian)
        {
            // Murid Padepokan 1 - Andi
            var andiData = CreateMuridPadepokan1_Indonesian();
            string pathAndi = Path.Combine(outputPath, "AndiStudent_ID.asset");
            AssetDatabase.CreateAsset(andiData, pathAndi);
            Debug.Log($"✅ Created: {pathAndi}");
            
            // Murid Padepokan 2 - Budi
            var budiData = CreateMuridPadepokan2_Indonesian();
            string pathBudi = Path.Combine(outputPath, "BudiStudent_ID.asset");
            AssetDatabase.CreateAsset(budiData, pathBudi);
            Debug.Log($"✅ Created: {pathBudi}");
            
            // Murid Padepokan 3 - Candra
            var candraData = CreateMuridPadepokan3_Indonesian();
            string pathCandra = Path.Combine(outputPath, "CandraStudent_ID.asset");
            AssetDatabase.CreateAsset(candraData, pathCandra);
            Debug.Log($"✅ Created: {pathCandra}");
        }
    }
    
    private void GenerateWargaHausDialogues()
    {
        if (generateIndonesian)
        {
            // Warga Haus 1 - Pak Darmo
            var pakDarmoData = CreateWargaHaus1_Indonesian();
            string pathPakDarmo = Path.Combine(outputPath, "PakDarmo_ID.asset");
            AssetDatabase.CreateAsset(pakDarmoData, pathPakDarmo);
            Debug.Log($"✅ Created: {pathPakDarmo}");
            
            // Warga Haus 2 - Bu Siti
            var buSitiData = CreateWargaHaus2_Indonesian();
            string pathBuSiti = Path.Combine(outputPath, "BuSiti_ID.asset");
            AssetDatabase.CreateAsset(buSitiData, pathBuSiti);
            Debug.Log($"✅ Created: {pathBuSiti}");
        }
    }
    
    private void GenerateWargaKrandonDialogues()
    {
        if (generateIndonesian)
        {
            var wargaKrandonData = CreateWargaKrandon1_Indonesian();
            string pathWargaKrandon = Path.Combine(outputPath, "WargaKrandon1_ID.asset");
            AssetDatabase.CreateAsset(wargaKrandonData, pathWargaKrandon);
            Debug.Log($"✅ Created: {pathWargaKrandon}");
        }
    }
    
    private void GeneratePemanduJalanDialogue()
    {
        if (generateIndonesian)
        {
            var pemanduData = CreatePemanduJalan_Indonesian();
            string pathPemandu = Path.Combine(outputPath, "JokoGuide_ID.asset");
            AssetDatabase.CreateAsset(pemanduData, pathPemandu);
            Debug.Log($"✅ Created: {pathPemandu}");
        }
    }
    
    private void GenerateVillageNPCDialogues()
    {
        if (generateIndonesian)
        {
            // Pak Tani
            var pakTaniData = CreatePakTani_Indonesian();
            string pathPakTani = Path.Combine(outputPath, "PakTani_ID.asset");
            AssetDatabase.CreateAsset(pakTaniData, pathPakTani);
            Debug.Log($"✅ Created: {pathPakTani}");
            
            // Bu Tani
            var buTaniData = CreateBuTani_Indonesian();
            string pathBuTani = Path.Combine(outputPath, "BuTani_ID.asset");
            AssetDatabase.CreateAsset(buTaniData, pathBuTani);
            Debug.Log($"✅ Created: {pathBuTani}");
            
            // Other village NPCs
            GenerateOtherVillageNPCs();
        }
    }
    
    private void GenerateOtherVillageNPCs()
    {
        // Generate the remaining village NPCs from the documentation
        var npcList = new Dictionary<string, System.Func<DialogueData>>
        {
            {"AnakGembala_ID.asset", () => CreateAnakGembala_Indonesian()},
            {"PakPedagang_ID.asset", () => CreatePakPedagang_Indonesian()},
            {"BuPenjual_ID.asset", () => CreateBuPenjual_Indonesian()},
            {"PakLurah_ID.asset", () => CreatePakLurah_Indonesian()},
            {"BuGuru_ID.asset", () => CreateBuGuru_Indonesian()},
            {"DukunKampung_ID.asset", () => CreateDukunKampung_Indonesian()},
            {"PemudaDesa_ID.asset", () => CreatePemudaDesa_Indonesian()},
            {"NenekBijak_ID.asset", () => CreateNenekBijak_Indonesian()}
        };
        
        foreach (var npc in npcList)
        {
            var dialogueData = npc.Value();
            string path = Path.Combine(outputPath, npc.Key);
            AssetDatabase.CreateAsset(dialogueData, path);
            Debug.Log($"✅ Created: {path}");
        }
    }
    
    #endregion
    
    #region Supporting Characters Implementation
    
    private DialogueData CreateMuridPadepokan1_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Andi (Murid Padepokan)";
        dialogueData.dialogueDescription = "Murid padepokan yang bersemangat membantu";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Andi (Murid Padepokan)",
            dialogueText = "Menak Sopal! Aku dengar tentang proyek bendunganmu. Bisakah kami membantu? Kami kuat dan bersemangat melayani masyarakat!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { "dam_construction_started" },
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Ya, saya butuh bantuan mengangkut batu dan kayu",
                    flagsToAdd = new string[] { "student_helpers_recruited" },
                    questToStart = "gather_construction_materials"
                }
            }
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    private DialogueData CreateMuridPadepokan2_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Budi (Murid Padepokan)";
        dialogueData.dialogueDescription = "Murid yang mengamati fenomena aneh";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Budi (Murid Padepokan)",
            dialogueText = "Bendungan ini terus rusak! Ada sesuatu yang tidak wajar tentang ini. Aku melihat riak aneh di air saat bendungan runtuh.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { "dam_repeatedly_destroyed" }
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    private DialogueData CreateMuridPadepokan3_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Candra (Murid Padepokan)";
        dialogueData.dialogueDescription = "Murid yang loyal dan mendukung";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Candra (Murid Padepokan)",
            dialogueText = "Kakak Menak, kami percaya pada visimu. Jika kakak bilang bendungan ini akan membantu orang, maka kami akan bekerja siang malam untuk membangunnya!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset },
            requiredFlags = new string[] { "students_permission_granted" }
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    private DialogueData CreateWargaHaus1_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Pak Darmo";
        dialogueData.dialogueDescription = "Warga yang menderita krisis air";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Pak Darmo",
            dialogueText = "Tolong, anak muda! sudah berhari-hari kami tidak mendapat air bersih! Sumur ini hampir kering!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { },
            flagsToAdd = new string[] { "water_crisis_discovered" },
            isImportantDialogue = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    private DialogueData CreateWargaHaus2_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Bu Siti";
        dialogueData.dialogueDescription = "Warga yang terkena dampak krisis air";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Bu Siti",
            dialogueText = "Krisis air ini benar benar membuat kami kesusahan nak",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset },
            requiredFlags = new string[] { "water_crisis_discovered" }
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    private DialogueData CreateWargaKrandon1_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Pak Gunawan";
        dialogueData.dialogueDescription = "Warga Krandon yang mengejar";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Pak Gunawan",
            dialogueText = "Itu dia! Itu pemuda yang mencuri gajah Mbok Randa! Jangan biarkan dia kabur!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { "chase_sequence_active" },
            isImportantDialogue = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    private DialogueData CreatePemanduJalan_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Joko (Penunjuk Jalan Desa)";
        dialogueData.dialogueDescription = "Pemandu perjalanan ke Desa Krandon";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        var guideOffer = new DialogueEntry
        {
            speakerName = "Joko (Penunjuk Jalan Desa)",
            dialogueText = "Aku tahu jalan ke Desa Krandon, anak muda. Perjalanan dua hari melewati hutan.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset },
            requiredFlags = new string[] { "seeking_white_elephant" },
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Tolong tunjukkan jalan ke rumah Mbok Randa",
                    flagsToAdd = new string[] { "guide_hired" },
                    questToStart = "journey_to_krandon"
                }
            }
        };
        dialogueEntries.Add(guideOffer);
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    #endregion
    
    #region Village NPCs Implementation
    
    private DialogueData CreatePakTani_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Pak Tani";
        dialogueData.dialogueDescription = "Petani desa, menyediakan quest pertanian dan informasi bercocok tanam padi";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        // Salam Pagi
        var morningGreeting = new DialogueEntry
        {
            speakerName = "Pak Tani",
            dialogueText = "Selamat pagi, anak muda! Embun pagi ini sangat cocok untuk menanam hari ini. Apa kamu ke sini untuk belajar bertani?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise },
            requiredFlags = new string[] { },
            isRepeatable = true,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Bisakah Pak mengajarkan saya bertani padi?",
                    response = new DialogueResponse
                    {
                        speakerName = "Pak Tani",
                        responseText = "Padi butuh air, kesabaran, dan rasa hormat pada tanah. Kalau kamu mau bantu di sawah, Pak akan ajari semuanya!"
                    }
                }
            }
        };
        dialogueEntries.Add(morningGreeting);
        
        // Quest Panen
        var harvestQuest = new DialogueEntry
        {
            speakerName = "Pak Tani",
            dialogueText = "Musim panen sudah tiba! Pak butuh tangan muda yang kuat untuk bantu kumpulkan padi. Mau bantu Pak?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { },
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Saya akan bantu panen",
                    flagsToAdd = new string[] { "pak_tani_harvest_accepted" },
                    questToStart = "village_rice_harvest"
                }
            }
        };
        dialogueEntries.Add(harvestQuest);
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    private DialogueData CreateBuTani_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Bu Tani";
        dialogueData.dialogueDescription = "Kebijaksanaan bertani, pengetahuan herbal, resep desa";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Bu Tani",
            dialogueText = "Panen yang baik dimulai dari benih yang baik, tapi diselesaikan dengan masakan yang baik. Mau belajar resep desa?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { },
            isRepeatable = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    // Additional Village NPCs (simplified implementations)
    private DialogueData CreateAnakGembala_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Anak Gembala";
        dialogueData.dialogueDescription = "Sumber berita desa, perawatan hewan, pembantu energik";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Anak Gembala",
            dialogueText = "Hai! Aku lagi jaga kambing desa hari ini! Kamu lihat hewan aneh di hutan nggak?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { },
            isRepeatable = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    private DialogueData CreatePakPedagang_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Pak Pedagang";
        dialogueData.dialogueDescription = "Penjual barang, informasi perdagangan, pemberi quest ekonomi";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Pak Pedagang",
            dialogueText = "Selamat datang di toko sederhana Pak! Ada barang dari tiga desa. Bisa Pak bantu cariin apa hari ini?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day, TimeOfDay.Sunset },
            requiredFlags = new string[] { },
            isRepeatable = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    private DialogueData CreateBuPenjual_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Bu Penjual";
        dialogueData.dialogueDescription = "Penjual makanan, resep lokal, pengumpul komunitas";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Bu Penjual",
            dialogueText = "Makanan segar! Nasi anget, sambal pedas, sama jajanan manis! Perut kenyang bikin hati senang!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day, TimeOfDay.Sunset },
            requiredFlags = new string[] { },
            isRepeatable = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    private DialogueData CreatePakLurah_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Pak Lurah";
        dialogueData.dialogueDescription = "Kepemimpinan desa, pemberi quest utama, pemecah masalah";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Pak Lurah",
            dialogueText = "Selamat datang, anak muda. Saya kepala desa ini. Ada yang bisa saya bantu hari ini?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { },
            isRepeatable = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    private DialogueData CreateBuGuru_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Bu Guru";
        dialogueData.dialogueDescription = "Pendidikan desa, kesejahteraan anak-anak, pelestarian budaya";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Bu Guru",
            dialogueText = "Pendidikan adalah cahaya yang menerangi pikiran muda! Kamu ke sini mau belajar, atau mungkin bantu ngajar anak-anak?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { },
            isRepeatable = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    private DialogueData CreateDukunKampung_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Dukun Kampung";
        dialogueData.dialogueDescription = "Pengobatan tradisional, bimbingan spiritual, quest mistis";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Dukun Kampung",
            dialogueText = "Roh-roh berbisik tentang kedatanganmu, anak muda. Kamu membawa aura takdir. Apa yang membuatmu mencari cara-cara lama?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { },
            isRepeatable = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    private DialogueData CreatePemudaDesa_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Pemuda Desa";
        dialogueData.dialogueDescription = "Pembantu energik, quest fisik, kegiatan desa";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Pemuda Desa",
            dialogueText = "Eh! Kamu kelihatan kuat! Mau gabung kerja bakti desa? Kita selalu butuh tangan tambahan!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { },
            isRepeatable = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    private DialogueData CreateNenekBijak_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Nenek Bijak";
        dialogueData.dialogueDescription = "Kebijaksanaan tradisional, cerita rakyat, pengetahuan budaya";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Nenek Bijak",
            dialogueText = "Sini, nak. Mata tua ini sudah lihat banyak musim, telinga ini sudah dengar cerita tak terhitung. Kebijaksanaan apa yang kamu cari?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { },
            isRepeatable = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    #endregion
}

#endif