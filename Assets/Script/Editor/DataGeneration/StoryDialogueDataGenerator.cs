using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR

/// <summary>
/// Generates DialogueData ScriptableObjects for story NPCs based on the in-game assets and narrative flow
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
            dialogueText = "Ah, Menak Sopal. Kekeringan ini membuat kita dan para warga dalam selimut kegelisahan. Bercengkeramalah dengan warga desa, mereka membutuhkan bantuanmu...",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { "story_started" },
            flagsToAdd = new string[] { "story_started", "first_contact" },
            flagsToRemove = new string[] { },
            isRepeatable = false,
            isImportantDialogue = false
        });
        
        // Fase Cerita 1 - Setelah Penemuan Krisis Air
        var crisisDialogue = new DialogueEntry
        {
            speakerName = "Ki Ageng Sinawang",
            dialogueText = "Penderitaan warga disekitar kita memberatkan hatimu, muridku. Terkadang perbuatan mulia yang terbesar memerlukan pengorbanan yang besar pula.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day, TimeOfDay.Sunset },
            requiredFlags = new string[] { "water_crisis_discovered" },
            isRepeatable = false,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Ingin membantu",
                    flagsToAdd = new string[] { "asked_permission_water_project" },
                    targetDialogueIndex = 3,
                    questToStart = "gather_construction_helpers",
                    objectiveToComplete = "receive_permission",
                    questForObjective = "seek_guru_guidance",
                    isRepeatable = true,
                    choiceColor = new Color(1f, 1f, 1f, 1f),
                    response = new DialogueResponse
                    {
                        speakerName = "Menak Sopal",
                        responseText = "Ki Ageng, atas izinmu, izinkanlah aku untuk membantu tentang masalah ini. Diriku terpanggil untuk membantu tentang masalah ini",
                        continueToNext = true,
                        nextDialogueIndex = 3
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
            isRepeatable = false,
            isImportantDialogue = true
        });

        // Izin Diberikan
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Ki Ageng Sinawang",
            dialogueText = "Belas kasihanmu menghormati ajaran kita, muridku. Bergegaslah, restuku membersamai niat baikmu.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { "asked_permission_water_project" },
            isRepeatable = false,
            isImportantDialogue = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();

        dialogueData.greetings = new DialogueEntry[]
        {
            new DialogueEntry
            {
                speakerName = "Ki Ageng Sinawang",
                dialogueText = "Pagi, muridku",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise },
                isRepeatable = true
            },
            new DialogueEntry
            {
                speakerName = "Ki Ageng Sinawang",
                dialogueText = "Berlatihlah dengan sungguh sungguh nak",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day },
                isRepeatable = true
            },
            new DialogueEntry
            {
                speakerName = "Ki Ageng Sinawang",
                dialogueText = "Hmmm...",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Night },
                isRepeatable = true
            }
        };
        
        return dialogueData;
    }
    
    private DialogueData CreateKiAgengDialogue_English()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Ki Ageng Sinawang";
        dialogueData.dialogueDescription = "Padepokan leader, spiritual teacher of Menak Sopal";
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
            dialogueText = "Selamat pagi, menak sopal anakku. Ibu bermimpi tentang air yang mengalir tadi malam. Mungkin itu pertanda keberuntungan akan datang.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise },
            requiredFlags = new string[] { },
            isRepeatable = false
        });
        
        // Kekhawatiran Seorang Ibu
        var motherConcern = new DialogueEntry
        {
            speakerName = "Raden Ayu Saraswati",
            dialogueText = "Ibu khawatir dengan proyek bendunganmu ini, nak. Roh-roh sungai tidak boleh dianggap enteng.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunset },
            requiredFlags = new string[] { "dam_construction_started" },
            isRepeatable = false,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Jangan khawatir, Ibu. Saya akan berhati-hati",
                    isRepeatable = true,
                    choiceColor = new Color(1f, 1f, 1f, 1f),
                    response = new DialogueResponse
                    {
                        speakerName = "Raden Ayu Saraswati",
                        responseText = "Ayahmu dulu memiliki semangat yang sama. Ingatlah saja, keberanian tanpa kebijaksanaan adalah kecerobohan.",
                        continueToNext = true,
                        nextDialogueIndex = -1
                    }
                },
                new DialogueChoice
                {
                    choiceText = "Apakah Ibu melihat pertanda tentang sungai?",
                    isRepeatable = true,
                    choiceColor = new Color(1f, 1f, 1f, 1f),
                    response = new DialogueResponse
                    {
                        speakerName = "Raden Ayu Saraswati",
                        responseText = "Burung-burung gelisah di dekat air. Dan pelita kelahiranmu berkedip-kedip tadi malam - ada sesuatu yang bergerak di alam spiritual.",
                        continueToNext = true,
                        nextDialogueIndex = -1
                    }
                }
            }
        };
        dialogueEntries.Add(motherConcern);
        
        // Ibu yang Bangga
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Raden Ayu Saraswati",
            dialogueText = "Anakku telah beranjak dewasa hari ini. Bukan karena dia memecahkan masalah, tapi karena dia belajar menghadapi konsekuensi dari pilihannya.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise, TimeOfDay.Night },
            requiredFlags = new string[] { "story_completed" },
            isRepeatable = false,
            isImportantDialogue = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();

        dialogueData.greetings = new DialogueEntry[]
        {
            new DialogueEntry
            {
                speakerName = "Raden Ayu Saraswati",
                dialogueText = "Pagi, anakku...",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise },
                isRepeatable = true
            },
            new DialogueEntry
            {
                speakerName = "Raden Ayu Saraswati",
                dialogueText = "Hari ini begitu terik, dan keadaan air ini...",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day },
                isRepeatable = true
            },
            new DialogueEntry
            {
                speakerName = "Raden Ayu Saraswati",
                dialogueText = "Segeralah beristirahat, menak sopal...",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Night },
                isRepeatable = true
            }
        };

        return dialogueData;
    }
    
    private DialogueData CreateRadenAyuDialogue_English()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Raden Ayu Saraswati";
        dialogueData.dialogueDescription = "Menak Sopal's mother, supportive maternal figure";
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
        
        // 0. Pertemuan Pertama
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Mbok Randa Krandon",
            dialogueText = "Siang. Siapa kalian datang ke gubukku yang sederhana ini, wahai anak muda?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
            requiredFlags = new string[] { },
            isRepeatable = false,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Menjelaskan",
                    targetDialogueIndex = 0,
                    response = new DialogueResponse
                    {
                        speakerName = "Menak Sopal",
                        responseText = "Kami dari Padepokan Sinawang mbok. Saya Menak Sopal, dan itu rekan saya, paman Joko.",
                        continueToNext = true,
                        nextDialogueIndex = 1
                    }
                }
            }
        });
        
        // 1. Maksud Kedatangan
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Mbok Randa Krandon",
            dialogueText = "Padepokan Sinawang, jauh juga. Ada maksud apa kalian kesini?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
            requiredFlags = new string[] { },
            isRepeatable = false,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Meminjam Gajah",
                    targetDialogueIndex = 0,
                    response = new DialogueResponse
                    {
                        speakerName = "Menak Sopal",
                        responseText = "Begini mbok, kami ingin meminjam Gajah Putih panjengengan.",
                        continueToNext = true,
                        nextDialogueIndex = 2
                    }
                }
            }
        });
        
        // 2. Alasan Peminjaman
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Mbok Randa Krandon",
            dialogueText = "Untuk apa, biasanya Ki Ageng bersurat dulu kepadaku jika ada sesuatu",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
            requiredFlags = new string[] { },
            isRepeatable = false,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Berbohong",
                    targetDialogueIndex = 0,
                    response = new DialogueResponse
                    {
                        speakerName = "Menak Sopal",
                        responseText = "Umm, anu mbok di, di dekat padepokan ada festival. Kami ingin menggunakan Gajah Putih untuk pertunjukan. Hanya 3 hari saja...",
                        continueToNext = true,
                        nextDialogueIndex = 3
                    }
                }
            }
        });

        // 3. Izin Mbok Randa
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Mbok Randa Krandon",
            dialogueText = "Baiklah, karena aku kenal baik dengan Ki Ageng, bawalah gajahku.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
            requiredFlags = new string[] { },
            isRepeatable = false
        });

        // 4. Pesan Mbok Randa
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Mbok Randa Krandon",
            dialogueText = "Sebenarnya aku ingin ikut, tapi ada urusan lain yang mendesak. Jaga Gajahku dengan baik ya!!!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { },
            flagsToAdd = new string[] { "white_elephant_borrowed" },
            flagsToRemove = new string[] { "joko_in_mbr" },
            isRepeatable = false
        });

        // 5. Kemarahan Mbok Randa 1
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Mbok Randa Krandon",
            dialogueText = "Sudah lebih dari satu bulan berlalu, dan Gajahku belum juga dikembalikan!!!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "mbok_randa_angry" },
            isRepeatable = false
        });

        // 6. Kemarahan Mbok Randa 2
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Mbok Randa Krandon",
            dialogueText = "Dasar bocah B******n!!!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "mbok_randa_angry" },
            isRepeatable = false
        });

        // 7. Mbok Randa di Padepokan 1
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Mbok Randa Krandon",
            dialogueText = "Ki Ageng!!, dimana murid badungmu itu berada!!!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "mbok_in_padepokan" },
            isRepeatable = false
        });

        // 8. Mbok Randa di Padepokan 2
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Mbok Randa Krandon",
            dialogueText = "Aku ingin menghukum murid badungmu itu, Menak Sopal!!!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { "mbok_in_padepokan_a" },
            isRepeatable = false
        });

        // 9. Mbok Randa Memanggil Pengawal
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Mbok Randa Krandon",
            dialogueText = "Itu dia bocah badung itu, pengawal tangkap dia!!!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "menak_in_padepokan" },
            isRepeatable = false
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        
        return dialogueData;
    }
    
    private DialogueData CreateMbokRandaDialogue_English()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Mbok Randa Krandon";
        dialogueData.dialogueDescription = "White elephant owner, represents conflict and eventual understanding";
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
        
        // 0. Kontak Pertama
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Buaya Putih",
            dialogueText = "Siapa yang berani beraninya mengganggu istirahatku!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { "spiritual_vision_active" },
            isRepeatable = false,
            isImportantDialogue = true,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Menak Sopal",
                    isRepeatable = true,
                    choiceColor = new Color(1f, 1f, 1f, 1f),
                    response = new DialogueResponse
                    {
                        speakerName = "Menak Sopal",
                        responseText = "Aku, Menak Sopal. Murid dari padepokan Sinawang!. Hendak mencari alasan bendunganku hancur berkali kali",
                        continueToNext = true,
                        nextDialogueIndex = 4
                    }
                }
            }
        });
        
        // 1. Tuntutan Kepala Gajah Putih
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Buaya Putih",
            dialogueText = "Jika kamu ingin bendunganmu berdiri, kamu harus menawarkan persembahan yang layak. Bawakan aku kepala gajah putih, dan aku akan menghentikan kerusakanku.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { },
            isRepeatable = false,
            isImportantDialogue = true,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Menerima",
                    flagsToAdd = new string[] { "accepted_spirit_demand" },
                    isRepeatable = true,
                    choiceColor = new Color(1f, 1f, 1f, 1f),
                    response = new DialogueResponse
                    {
                        speakerName = "Menak Sopal",
                        responseText = "Baiklah, saya akan mencari gajah putih ini"
                    }
                }
            }
        });
        
        // 2. Perjanjian Selesai
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Buaya Putih",
            dialogueText = "Permintaanku telah terpenuhi. Bendunganmu akan berdiri, dan aku tak akan menggagu lagi.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { "elephant_sacrifice_complete" },
            flagsToAdd = new string[] { "spirit_pact_complete" },
            isRepeatable = true,
            isImportantDialogue = true
        });
        
        // 3. Penyelamatan di Sungai
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Buaya Putih",
            dialogueText = "Anak muda yang menghormati cara-cara kuno, aku tidak akan membiarkanmu tenggelam. Hatimu yang murni telah mendapat perlindunganku.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { "drowning_in_river" },
            flagsToAdd = new string[] { "rescued_by_crocodile" },
            isRepeatable = true,
            isImportantDialogue = true
        });

        // 4. Konfrontasi Spiritual
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Buaya Putih",
            dialogueText = "Hmm, jadi muara gemuruh gaduh dari tempat istirahatku akhir akhir ini adalah ulahmu!. Berulang kali ku redam namun tak kunjung padam juga, apa kau ingin menantangku nak?!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { },
            isRepeatable = true,
            isImportantDialogue = true,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Menjelaskan",
                    response = new DialogueResponse
                    {
                        speakerName = "Menak Sopal",
                        responseText = "Tidak, aku tidak berniat menggagumu wahai penunggu tempat ini. Aku hanya ingin membantu hajat banyak orang dengan membangun bendungan ini.",
                        continueToNext = true,
                        nextDialogueIndex = 1
                    }
                }
            }
        });

        // 5. Kesepakatan Lanjutan
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Buaya Putih",
            dialogueText = "Begitu rupanya. Kali ini aku sedang dalam suasana baik. Tapi ingat, lain kali izinlah terlebih dahulu dimanapun kau berpijak. ",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night },
            requiredFlags = new string[] { },
            isRepeatable = true,
            isImportantDialogue = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        
        return dialogueData;
    }
    
    private DialogueData CreateBuayaPutihDialogue_English()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Buaya Putih";
        dialogueData.dialogueDescription = "Mystical guardian, represents nature's demands and final cooperation";
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

            // Warga Haus 3 - Karto
            var kartoData = CreateWargaHaus3_Indonesian();
            string pathKarto = Path.Combine(outputPath, "WargaHaus3_ID.asset");
            AssetDatabase.CreateAsset(kartoData, pathKarto);
            Debug.Log($"✅ Created: {pathKarto}");
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
        
        // 0. Rekrut Murid
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Andi (Murid Padepokan)",
            dialogueText = "Menak Sopal! Aku dengar tentang rencana pembangungan bendungan. Bisakah aku ikut membantumu?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { "committed_to_help", "guru_guidance_received" },
            isRepeatable = false,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Tentu",
                    flagsToAdd = new string[] { "student_helpers_recruited" },
                    objectiveToComplete = "gather_students",
                    isRepeatable = true,
                    choiceColor = new Color(1f, 1f, 1f, 1f),
                    response = new DialogueResponse
                    {
                        responseText = "Benar sekali kak Andi, bantuanmu akan sangat membantu ku!",
                        continueToNext = true,
                        nextDialogueIndex = -1
                    }
                }
            }
        });

        // 1. Di Sungai
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Andi (Murid Padepokan)",
            dialogueText = "Hmm, tempat ini memang agak berbahaya. Aku akan membangun pagar dan membersihkan tempat ini agar lebih aman. Jika ada sesuatu berkabar saja ya!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { "npc_to_river" },
            isRepeatable = false
        });

        // 2. Komentar Bendungan
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Andi (Murid Padepokan)",
            dialogueText = "Syukurlah bendungan ini akhirnya selesai kita bangun. Walau entah mengapa tadi bangunan ini roboh terus menerus. Semoga kali ini kita benar benar menyelesaikan bangunan ini, Menak Sopal...",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { "andi_comment_after_dam" },
            flagsToAdd = new string[] { "dam_broken" },
            isRepeatable = false
        });

        // 3. Info Gajah Putih
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Andi (Murid Padepokan)",
            dialogueText = "hmm, Gajah Putih. Setahuku itu ada di desa Krandon, tapi aku tidak tahu untuk kesana...",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { "keberadaan_gajah_putih" },
            isRepeatable = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();

        dialogueData.greetings = new DialogueEntry[]
        {
            new DialogueEntry
            {
                speakerName = "Andi (Murid Padepokan)",
                dialogueText = "Pagi menak sopal!",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise },
                isRepeatable = true
            },
            new DialogueEntry
            {
                speakerName = "Andi (Murid Padepokan)",
                dialogueText = "Terik matahari bukan alasan untuk bermalas malasan!",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day },
                isRepeatable = true
            },
            new DialogueEntry
            {
                speakerName = "Andi (Murid Padepokan)",
                dialogueText = "Ah, malam menak sopal",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Night },
                isRepeatable = true
            }
        };

        return dialogueData;
    }
    
    private DialogueData CreateMuridPadepokan2_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Budi (Murid Padepokan)";
        dialogueData.dialogueDescription = "Murid yang mengamati fenomena aneh";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        // 0. Bahan Terkumpul
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Budi (Murid Padepokan)",
            dialogueText = "Sekarang semua bahan meterial terkumpul, mari kita selesaikan bendungan ini, Menak Sopal!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "materials_collected" },
            flagsToAdd = new string[] { "dam_dialog_built" },
            isRepeatable = false
        });

        // 1. Dam Rusak
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Budi (Murid Padepokan)",
            dialogueText = "Bendungan ini terus rusak! Ada sesuatu yang tidak wajar tentang ini. Aku melihat riak aneh di air saat bendungan runtuh.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "dam_broken" },
            isRepeatable = false
        });

        // 2. Siap Membantu
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Budi (Murid Padepokan)",
            dialogueText = "Kabar berlalu cepat nak, dan aku disini siap untuk membantu niat baikmu!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "committed_to_help", "guru_guidance_received" },
            isRepeatable = false
        });

        // 3. Info Gajah Putih
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Budi (Murid Padepokan)",
            dialogueText = "Kalau tentang jurus, aku pasti akan memberitahumu nak. Tapi Gajah Putih?, aku bahkan pertama kali ini mendegarnya",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "keberadaan_gajah_putih" },
            isRepeatable = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();

        dialogueData.greetings = new DialogueEntry[]
        {
            new DialogueEntry
            {
                speakerName = "Budi (Murid Padepokan)",
                dialogueText = "Pagi saudara seperguruanku!",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise },
                isRepeatable = true
            },
            new DialogueEntry
            {
                speakerName = "Budi (Murid Padepokan)",
                dialogueText = "Hari ini begitu terik, aku jadi semakin bersemangat. Bukankah seperti itu adik tingkat?!!",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day },
                isRepeatable = true
            },
            new DialogueEntry
            {
                speakerName = "Budi (Murid Padepokan)",
                dialogueText = "Aku begitu lelah, menak sopal...",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Night },
                isRepeatable = true
            }
        };

        return dialogueData;
    }
    
    private DialogueData CreateMuridPadepokan3_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Candra (Murid Padepokan)";
        dialogueData.dialogueDescription = "Murid yang loyal dan mendukung";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        // 0. Dukungan
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Candra (Murid Padepokan)",
            dialogueText = "Kakak Menak, kami percaya pada visimu. Jika kakak bilang bendungan ini akan membantu orang, maka kami akan bekerja siang malam untuk membangunnya!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "committed_to_help", "guru_guidance_received" },
            isRepeatable = false
        });

        // 1. Gajah Putih
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Candra (Murid Padepokan)",
            dialogueText = "Gajah Putih?, aku kurang tahu kak...",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "keberadaan_gajah_putih" },
            isRepeatable = false
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();

        dialogueData.greetings = new DialogueEntry[]
        {
            new DialogueEntry
            {
                speakerName = "Candra (Murid Padepokan)",
                dialogueText = "Selamat pagi kak Menak Sopal",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise },
                isRepeatable = true
            },
            new DialogueEntry
            {
                speakerName = "Candra (Murid Padepokan)",
                dialogueText = "Ah, terik sekali. Rasanya aku ingin tidur tiduran sajaa",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day },
                isRepeatable = true
            },
            new DialogueEntry
            {
                speakerName = "Candra (Murid Padepokan)",
                dialogueText = "Malam kak Menak Sopal",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Night },
                isRepeatable = true
            }
        };

        return dialogueData;
    }
    
    private DialogueData CreateWargaHaus1_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Pak Darmo";
        dialogueData.dialogueDescription = "Warga yang menderita krisis air";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        // 0. Permintaan Tolong
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Pak Darmo",
            dialogueText = "Tolong, nak muda! sudah berhari-hari kami kesulitan mendapat air bersih! Sumur ini hampir kering!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Night, TimeOfDay.Sunrise },
            requiredFlags = new string[] { },
            flagsToAdd = new string[] { "water_crisis_discovered" },
            isRepeatable = false,
            isImportantDialogue = true,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Bertanya tentang sungai",
                    flagsToAdd = new string[] { "river_asked" },
                    response = new DialogueResponse
                    {
                        speakerName = "Menak Sopal",
                        responseText = "Saya dengar ada sungai disekitar desa ini pak, mengapa tidak mengambil air dari sana??",
                        continueToNext = true,
                        nextDialogueIndex = 1
                    }
                }
            }
        });

        // 1. Jawaban Pak Darmo tentang Sungai
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Pak Darmo",
            dialogueText = "Kami sudah mencoba mengambil air, tetapi sungai mulai kering. Air dari hulu tidak sampai ke tempat kami biasa mengambil air. Banyak bahaya di tempat tersebut.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Sunrise },
            requiredFlags = new string[] { },
            flagsToAdd = new string[] { "water_crisis_discovered" },
            isRepeatable = false,
            isImportantDialogue = true,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Aku paham",
                    objectiveToComplete = "witness_crisis",
                    questForObjective = "water_crisis_discovery",
                    response = new DialogueResponse
                    {
                        speakerName = "Menak Sopal",
                        responseText = "Kami dari padepokan belum bisa banyak membantu karena juga kesulitan air. Terima kasih atas informasinya, pak.",
                        continueToNext = false,
                        nextDialogueIndex = -1
                    }
                }
            }
        });

        // 2. Ratapan
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Pak Darmo",
            dialogueText = "Iya nak, kasihanilah orang tua renta seperti kami ini...",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "kegaduhan" },
            isRepeatable = false
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
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day, TimeOfDay.Sunset },
            requiredFlags = new string[] { "water_crisis_discovered" },
            isRepeatable = false
        });

        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Bu Siti",
            dialogueText = "Tolong, berikanlah air ini kepada kami...",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "kegaduhan" },
            isRepeatable = false
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();

        dialogueData.greetings = new DialogueEntry[]
        {
            new DialogueEntry
            {
                speakerName = "Bu Siti",
                dialogueText = "Krisis air ini benar benar mengganggu kami, nak",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunset, TimeOfDay.Day },
                isRepeatable = false
            },
            new DialogueEntry
            {
                speakerName = "Bu Siti",
                dialogueText = "Pagi, nak",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
                isRepeatable = true
            }
        };

        return dialogueData;
    }

    private DialogueData CreateWargaHaus3_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Karto";
        dialogueData.dialogueDescription = "Warga desa yang diajak membantu proyek dam";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Karto",
            dialogueText = "Ah, terik sekali. Air semakin menipis tiap hari. Ada perlu apa nak denganku?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "guru_guidance_received" },
            isRepeatable = false,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Meminta bantuan",
                    response = new DialogueResponse
                    {
                        speakerName = "Menak Sopal",
                        responseText = "Begini kang, apakah dirimu bersedia untuk membantuku dalam membangun dam ...",
                        continueToNext = true,
                        nextDialogueIndex = 1
                    }
                }
            }
        });

        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Karto",
            dialogueText = "Membangun dam?, di tengah terik matahari seperti ini?. Tentu saja!!. Lagipula kalau bendungan ini berhasil kita bangun, pasti dapat mengatasi krisis air ini.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "guru_guidance_received" },
            isRepeatable = false
        });

        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Karto",
            dialogueText = "Cukup kisah asmaraku saja yang kering, tak perlu sumber sumber air disekitar pemukiman ini yang kering. Hal ini membuatku benar benar frustasi nak!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Sunrise, TimeOfDay.Day },
            requiredFlags = new string[] { "guru_guidance_received" },
            isRepeatable = false
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
            isRepeatable = true,
            isImportantDialogue = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();
        return dialogueData;
    }
    
    private DialogueData CreatePemanduJalan_Indonesian()
    {
        var dialogueData = ScriptableObject.CreateInstance<DialogueData>();
        dialogueData.npcName = "Joko";
        dialogueData.dialogueDescription = "Pemandu perjalanan ke Desa Krandon";
        
        var dialogueEntries = new List<DialogueEntry>();
        
        // 0. Penawaran Pemandu
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Joko",
            dialogueText = "Desa Krandon?, aku tahu tempat. Apa kamu perlu kesana nak?, aku bisa menemanimu kesana kalau perlu.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "tribute_demand_received" },
            flagsToRemove = new string[] { "keberadaan_gajah_putih" },
            isRepeatable = false,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Meng-iyakan",
                    response = new DialogueResponse
                    {
                        speakerName = "Menak Sopal",
                        responseText = "Iya paman, benar sekali. Tolong bantu aku"
                    }
                }
            }
        });

        // 1. Meminta Bantuan Dam
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Joko",
            dialogueText = "Hei nak, kau seperti dalam masalah. Ada hal yang bisa ku bantu?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "committed_to_help", "guru_guidance_received" },
            isRepeatable = false,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Meminta bantuan",
                    questToStart = "journey_to_krandon",
                    choiceColor = new Color(1f, 1f, 1f, 1f),
                    response = new DialogueResponse
                    {
                        speakerName = "Menak Sopal",
                        responseText = "Paman Joko, bersediakah engkau untuk membantuku dalam membangun dam untuk membantu krisis air di pemukiman ini?",
                        continueToNext = true,
                        nextDialogueIndex = 2
                    }
                }
            }
        });

        // 2. Persetujuan Joko
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Joko",
            dialogueText = "Oh, tentu saja nak!. Krisis air ini telah membuat gaduh kehidupan orang orang disini!. Pun ke sungai, air tidak sampai di tempat yang aman bagi kami. Bersama keahlianmu, pastinya membangun dam ini menjadi lebih aman hahaha.",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "committed_to_help", "guru_guidance_received" },
            isRepeatable = false
        });

        // 3. Menuju Sungai
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Joko",
            dialogueText = "Ayo nak, saatnya kita bangun dam ini!",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "to_river" },
            isRepeatable = false
        });

        // 4. Setelah Hutan
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Joko",
            dialogueText = "Dari mana saja kau tadi nak?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "finish_forest" },
            isRepeatable = false,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Tersesat",
                    isRepeatable = true,
                    choiceColor = new Color(1f, 1f, 1f, 1f),
                    response = new DialogueResponse
                    {
                        speakerName = "Menak Sopal",
                        responseText = "Tadi aku sempat tersesat paman",
                        continueToNext = true,
                        nextDialogueIndex = 5
                    }
                }
            }
        });

        // 5. Ke Teras Mbok Randa
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Joko",
            dialogueText = "Baiklah, ayo kita langsung ke rumah mbok Randa kalau begitu",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "finish_forest", "mc_answer" },
            flagsToAdd = new string[] { "mbr_terrace" },
            isRepeatable = false
        });

        // 6. Pertanyaan Kebohongan
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Joko",
            dialogueText = "Nak, mengapa harus dirimu tadi berbohong?",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "white_elephant_borrowed" },
            isRepeatable = false,
            hasChoices = true,
            choices = new DialogueChoice[]
            {
                new DialogueChoice
                {
                    choiceText = "Menjelaskan",
                    flagsToAdd = new string[] { "sopal_explain" },
                    targetDialogueIndex = 7,
                    isRepeatable = true,
                    choiceColor = new Color(1f, 1f, 1f, 1f),
                    response = new DialogueResponse
                    {
                        speakerName = "Menak Sopal",
                        responseText = "Akan sulit jika mengatakan aku akan menyembelih gajah ini paman. Tenang, aku akan bertanggung jawab penuh.",
                        continueToNext = true,
                        nextDialogueIndex = 7
                    }
                }
            }
        });

        // 7. Respon Pasrah Joko
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Joko",
            dialogueText = "Baiklah kalau begitu, aku tidak ikut ikutan ya...",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "white_elephant_borrowed", "sopal_explain" },
            isRepeatable = false
        });

        // 8. Menunggu di Teras
        dialogueEntries.Add(new DialogueEntry
        {
            speakerName = "Joko",
            dialogueText = "Aku akan menunggu disini nak, sampaikan pesanmu ke mbok randa",
            availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunset, TimeOfDay.Sunrise },
            requiredFlags = new string[] { "joko_in_mbr" },
            isRepeatable = true
        });
        
        dialogueData.dialogueEntries = dialogueEntries.ToArray();

        dialogueData.greetings = new DialogueEntry[]
        {
            new DialogueEntry
            {
                speakerName = "Joko",
                dialogueText = "Ah, halo nak",
                availableTimesOfDay = new TimeOfDay[] { TimeOfDay.Day, TimeOfDay.Sunrise, TimeOfDay.Sunset },
                isRepeatable = false
            }
        };

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