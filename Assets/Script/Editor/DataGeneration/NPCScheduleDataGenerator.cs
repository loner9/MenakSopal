using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR

/// <summary>
/// Generates NPCScheduleData ScriptableObjects for all NPCs based on the documentation
/// Run this from Tools -> Trenggalek Game -> Generate NPC Schedule Data
/// </summary>
public class NPCScheduleDataGenerator : EditorWindow
{
    [MenuItem("Tools/Trenggalek Game/Generate NPC Schedule Data")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(NPCScheduleDataGenerator));
    }

    /// <summary>
    /// Static method to generate all NPC schedules without opening the window
    /// </summary>
    public static void GenerateAllSchedulesStatic()
    {
        var generator = CreateInstance<NPCScheduleDataGenerator>();
        generator.outputPath = "Assets/Resources/Schedules/";
        generator.generateStoryNPCs = true;
        generator.generateVillageNPCs = true;
        generator.GenerateAllSchedules();
        DestroyImmediate(generator);
    }

    private string outputPath = "Assets/Resources/Schedules/";
    private bool generateStoryNPCs = true;
    private bool generateVillageNPCs = true;

    private void OnGUI()
    {
        titleContent = new GUIContent("NPC Schedule Generator");
        
        GUILayout.Label("NPC Schedule Data Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        generateStoryNPCs = EditorGUILayout.Toggle("Generate Story NPCs", generateStoryNPCs);
        generateVillageNPCs = EditorGUILayout.Toggle("Generate Village NPCs", generateVillageNPCs);
        
        GUILayout.Space(10);
        outputPath = EditorGUILayout.TextField("Output Path:", outputPath);
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Generate All NPC Schedules", GUILayout.Height(40)))
        {
            GenerateAllSchedules();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate Story NPC Schedules Only"))
        {
            GenerateStoryNPCSchedules();
        }
        
        if (GUILayout.Button("Generate Village NPC Schedules Only"))
        {
            GenerateVillageNPCSchedules();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Conditional Schedule Examples:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Generate Student Conditional Schedules (Example)"))
        {
            GenerateStudentConditionalSchedules();
        }
    }

    private void GenerateAllSchedules()
    {
        Debug.Log("Starting NPC Schedule Generation...");
        
        // Ensure output directory exists
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        
        if (generateStoryNPCs)
        {
            GenerateStoryNPCSchedules();
        }
        
        if (generateVillageNPCs)
        {
            GenerateVillageNPCSchedules();
        }
        
        AssetDatabase.Refresh();
        Debug.Log("✅ NPC Schedule Generation Complete!");
    }

    #region Story NPC Schedules
    
    private void GenerateStoryNPCSchedules()
    {
        GenerateKiAgengSchedule();
        GenerateRadenAyuSchedule();
        GenerateMbokRandaSchedule();
        GenerateStudentSchedules();
        GenerateGuideSchedule();
    }
    
    private void GenerateKiAgengSchedule()
    {
        var schedule = ScriptableObject.CreateInstance<NPCScheduleData>();
        schedule.scheduleName = "Ki Ageng Sinawang Schedule";
        schedule.scheduleDescription = "Spiritual teacher and padepokan leader schedule";
        schedule.spawnHour = 5; // Early morning for meditation
        
        // Home location
        schedule.homeObjectTag = NPCScheduleData.CommonTags.House;
        schedule.homeObjectName = "PadepokanTeacherQuarters";
        schedule.homePosition = new Vector2(0, 0); // Fallback position
        
        // Movement settings
        schedule.walkSpeed = 1.0f; // Slow, dignified pace
        schedule.pauseAtDestination = 3f;
        schedule.moveAroundWhenIdle = false; // Stays in place when meditating/teaching
        
        var events = new List<ScheduleEvent>();
        
        // 05:00 - Morning Meditation
        events.Add(new ScheduleEvent
        {
            hour = 5,
            targetObjectTag = "NPCTarget",
            targetObjectName = "MeditationGarden",
            targetPosition = new Vector2(-5, 2),
            behavior = NPCBehavior.Idle,
            shouldIdleWhenReached = true,
            customDialogue = new string[] 
            { 
                "Gunung mengajarkan kita kesabaran, sungai mengajarkan kita ketekunan." 
            }
        });
        
        // 07:00 - Morning Teaching
        events.Add(new ScheduleEvent
        {
            hour = 7,
            targetObjectTag = "NPCTarget",
            targetObjectName = "TeachingPavilion",
            targetPosition = new Vector2(0, 0),
            behavior = NPCBehavior.Interact,
            shouldIdleWhenReached = true
        });
        
        // 12:00 - Midday Rest
        events.Add(new ScheduleEvent
        {
            hour = 12,
            targetObjectTag = NPCScheduleData.CommonTags.House,
            targetObjectName = "PadepokanTeacherQuarters",
            targetPosition = new Vector2(2, 1),
            behavior = NPCBehavior.Idle,
            shouldIdleWhenReached = true
        });
        
        // 14:00 - Afternoon Consultation
        events.Add(new ScheduleEvent
        {
            hour = 14,
            targetObjectTag = "NPCTarget",
            targetObjectName = "TeachingPavilion",
            targetPosition = new Vector2(0, 0),
            behavior = NPCBehavior.Interact,
            shouldIdleWhenReached = true
        });
        
        // 18:00 - Evening Meditation
        events.Add(new ScheduleEvent
        {
            hour = 18,
            targetObjectTag = "NPCTarget",
            targetObjectName = "MeditationGarden",
            targetPosition = new Vector2(-5, 2),
            behavior = NPCBehavior.Idle,
            shouldIdleWhenReached = true
        });
        
        // 21:00 - Rest
        events.Add(new ScheduleEvent
        {
            hour = 21,
            targetObjectTag = NPCScheduleData.CommonTags.House,
            targetObjectName = "PadepokanTeacherQuarters",
            targetPosition = new Vector2(2, 1),
            behavior = NPCBehavior.Sleep,
            shouldIdleWhenReached = true,
            shouldDespawn = true
        });
        
        schedule.scheduleEvents = events.ToArray();
        
        string path = Path.Combine(outputPath, "KiAgengSinawang_Schedule.asset");
        AssetDatabase.CreateAsset(schedule, path);
        Debug.Log($"✅ Created: {path}");
    }
    
    private void GenerateRadenAyuSchedule()
    {
        var schedule = ScriptableObject.CreateInstance<NPCScheduleData>();
        schedule.scheduleName = "Raden Ayu Saraswati Schedule";
        schedule.scheduleDescription = "Nurturing mother figure and herbalist";
        schedule.spawnHour = 6;
        
        schedule.homeObjectTag = NPCScheduleData.CommonTags.House;
        schedule.homeObjectName = "PadepokanFamilyQuarters";
        schedule.homePosition = new Vector2(3, 0);
        
        schedule.walkSpeed = 1.2f;
        schedule.pauseAtDestination = 2f;
        schedule.moveAroundWhenIdle = true;
        schedule.idleMovementRange = 1.5f;
        
        var events = new List<ScheduleEvent>();
        
        // 06:00 - Morning Preparation
        events.Add(new ScheduleEvent
        {
            hour = 6,
            targetObjectTag = NPCScheduleData.CommonTags.House,
            targetObjectName = "PadepokanFamilyQuarters",
            targetPosition = new Vector2(3, 0),
            behavior = NPCBehavior.Work,
            customDialogue = new string[] 
            { 
                "Sudah cukupkah kamu makan hari ini? Seorang ibu selalu khawatir anaknya tidak makan dengan baik." 
            }
        });
        
        // 08:00 - Herb Garden Work
        events.Add(new ScheduleEvent
        {
            hour = 8,
            targetObjectTag = "NPCTarget",
            targetObjectName = "HerbGarden",
            targetPosition = new Vector2(5, -2),
            behavior = NPCBehavior.Work,
            customDialogue = new string[] 
            { 
                "Ibu sedang menyiapkan ramuan penyembuhan untuk desa. Serai di tepi sungai tumbuh sangat baik musim ini." 
            }
        });
        
        // 11:00 - Cooking Preparation
        events.Add(new ScheduleEvent
        {
            hour = 11,
            targetObjectTag = NPCScheduleData.CommonTags.House,
            targetObjectName = "PadepokanKitchen",
            targetPosition = new Vector2(1, -1),
            behavior = NPCBehavior.Work
        });
        
        // 15:00 - Afternoon Care
        events.Add(new ScheduleEvent
        {
            hour = 15,
            targetObjectTag = "NPCTarget",
            targetObjectName = "PadepokanCourtyard",
            targetPosition = new Vector2(0, -1),
            behavior = NPCBehavior.Interact,
            shouldIdleWhenReached = true
        });
        
        // 19:00 - Evening Family Time
        events.Add(new ScheduleEvent
        {
            hour = 19,
            targetObjectTag = NPCScheduleData.CommonTags.House,
            targetObjectName = "PadepokanFamilyQuarters",
            targetPosition = new Vector2(3, 0),
            behavior = NPCBehavior.Idle,
            shouldIdleWhenReached = true
        });
        
        // 22:00 - Sleep
        events.Add(new ScheduleEvent
        {
            hour = 22,
            targetObjectTag = NPCScheduleData.CommonTags.House,
            targetObjectName = "PadepokanFamilyQuarters",
            targetPosition = new Vector2(3, 0),
            behavior = NPCBehavior.Sleep,
            shouldDespawn = true
        });
        
        schedule.scheduleEvents = events.ToArray();
        
        string path = Path.Combine(outputPath, "RadenAyuSaraswati_Schedule.asset");
        AssetDatabase.CreateAsset(schedule, path);
        Debug.Log($"✅ Created: {path}");
    }
    
    private void GenerateMbokRandaSchedule()
    {
        var schedule = ScriptableObject.CreateInstance<NPCScheduleData>();
        schedule.scheduleName = "Mbok Randa Krandon Schedule";
        schedule.scheduleDescription = "White elephant owner in Desa Krandon";
        schedule.spawnHour = 6;
        
        schedule.homeObjectTag = NPCScheduleData.CommonTags.House;
        schedule.homeObjectName = "KrandonElderHouse";
        schedule.homePosition = new Vector2(10, 5); // Different village coordinates
        
        schedule.walkSpeed = 1.0f; // Elderly pace
        schedule.pauseAtDestination = 4f;
        schedule.moveAroundWhenIdle = false;
        
        var events = new List<ScheduleEvent>();
        
        // 06:00 - Morning Elephant Care
        events.Add(new ScheduleEvent
        {
            hour = 6,
            targetObjectTag = "NPCTarget",
            targetObjectName = "ElephantEnclosure",
            targetPosition = new Vector2(8, 3),
            behavior = NPCBehavior.Work,
            customDialogue = new string[] 
            { 
                "Gajah putihku adalah harta paling berharga. Dia seperti putri bagiku." 
            }
        });
        
        // 09:00 - Village Council
        events.Add(new ScheduleEvent
        {
            hour = 9,
            targetObjectTag = "NPCTarget",
            targetObjectName = "KrandonVillageCenter",
            targetPosition = new Vector2(12, 5),
            behavior = NPCBehavior.Interact
        });
        
        // 12:00 - Midday Rest
        events.Add(new ScheduleEvent
        {
            hour = 12,
            targetObjectTag = NPCScheduleData.CommonTags.House,
            targetObjectName = "KrandonElderHouse",
            targetPosition = new Vector2(10, 5),
            behavior = NPCBehavior.Idle
        });
        
        // 15:00 - Afternoon Elephant Care
        events.Add(new ScheduleEvent
        {
            hour = 15,
            targetObjectTag = "NPCTarget",
            targetObjectName = "ElephantEnclosure",
            targetPosition = new Vector2(8, 3),
            behavior = NPCBehavior.Work
        });
        
        // 18:00 - Evening at Home
        events.Add(new ScheduleEvent
        {
            hour = 18,
            targetObjectTag = NPCScheduleData.CommonTags.House,
            targetObjectName = "KrandonElderHouse",
            targetPosition = new Vector2(10, 5),
            behavior = NPCBehavior.Idle,
            shouldIdleWhenReached = true
        });
        
        // 21:00 - Sleep
        events.Add(new ScheduleEvent
        {
            hour = 21,
            targetObjectTag = NPCScheduleData.CommonTags.House,
            targetObjectName = "KrandonElderHouse",
            targetPosition = new Vector2(10, 5),
            behavior = NPCBehavior.Sleep,
            shouldDespawn = true
        });
        
        schedule.scheduleEvents = events.ToArray();
        
        string path = Path.Combine(outputPath, "MbokRandaKrandon_Schedule.asset");
        AssetDatabase.CreateAsset(schedule, path);
        Debug.Log($"✅ Created: {path}");
    }
    
    private void GenerateStudentSchedules()
    {
        // Generate schedules for padepokan students
        string[] studentNames = { "AndiStudent", "BudiStudent", "CandraStudent" };
        
        for (int i = 0; i < studentNames.Length; i++)
        {
            var schedule = ScriptableObject.CreateInstance<NPCScheduleData>();
            schedule.scheduleName = $"{studentNames[i]} Schedule";
            schedule.scheduleDescription = $"Padepokan student {i + 1} daily routine";
            schedule.spawnHour = 6;
            
            schedule.homeObjectTag = NPCScheduleData.CommonTags.House;
            schedule.homeObjectName = $"StudentDormitory{i + 1}";
            schedule.homePosition = new Vector2(-2 + i, -3);
            
            schedule.walkSpeed = 1.8f; // Young and energetic
            schedule.pauseAtDestination = 1.5f;
            schedule.moveAroundWhenIdle = true;
            schedule.idleMovementRange = 2f;
            
            var events = new List<ScheduleEvent>();
            
            // 06:00 - Morning Practice
            events.Add(new ScheduleEvent
            {
                hour = 6,
                targetObjectTag = "NPCTarget",
                targetObjectName = "TrainingGrounds",
                targetPosition = new Vector2(-1, 1),
                behavior = NPCBehavior.Work
            });
            
            // 08:00 - Studies
            events.Add(new ScheduleEvent
            {
                hour = 8,
                targetObjectTag = "NPCTarget",
                targetObjectName = "StudyHall",
                targetPosition = new Vector2(1, 2),
                behavior = NPCBehavior.Idle
            });
            
            // 12:00 - Lunch Break
            events.Add(new ScheduleEvent
            {
                hour = 12,
                targetObjectTag = "NPCTarget",
                targetObjectName = "PadepokanDiningHall",
                targetPosition = new Vector2(0, -2),
                behavior = NPCBehavior.Idle
            });
            
            // 14:00 - Practical Work
            events.Add(new ScheduleEvent
            {
                hour = 14,
                targetObjectTag = "NPCTarget",
                targetObjectName = "WorkArea",
                targetPosition = new Vector2(2, 0),
                behavior = NPCBehavior.Work
            });
            
            // 17:00 - Free Time
            events.Add(new ScheduleEvent
            {
                hour = 17,
                targetObjectTag = "NPCTarget",
                targetObjectName = "PadepokanCourtyard",
                targetPosition = new Vector2(0, -1),
                behavior = NPCBehavior.Interact
            });
            
            // 20:00 - Evening Rest
            events.Add(new ScheduleEvent
            {
                hour = 20,
                targetObjectTag = NPCScheduleData.CommonTags.House,
                targetObjectName = $"StudentDormitory{i + 1}",
                targetPosition = new Vector2(-2 + i, -3),
                behavior = NPCBehavior.Idle
            });
            
            // 22:00 - Sleep
            events.Add(new ScheduleEvent
            {
                hour = 22,
                targetObjectTag = NPCScheduleData.CommonTags.House,
                targetObjectName = $"StudentDormitory{i + 1}",
                targetPosition = new Vector2(-2 + i, -3),
                behavior = NPCBehavior.Sleep,
                shouldDespawn = true
            });
            
            schedule.scheduleEvents = events.ToArray();
            
            string path = Path.Combine(outputPath, $"{studentNames[i]}_Schedule.asset");
            AssetDatabase.CreateAsset(schedule, path);
            Debug.Log($"✅ Created: {path}");
        }
    }
    
    private void GenerateGuideSchedule()
    {
        var schedule = ScriptableObject.CreateInstance<NPCScheduleData>();
        schedule.scheduleName = "Joko Guide Schedule";
        schedule.scheduleDescription = "Village guide who leads travelers to Desa Krandon";
        schedule.spawnHour = 7;
        
        schedule.homeObjectTag = NPCScheduleData.CommonTags.House;
        schedule.homeObjectName = "GuideHut";
        schedule.homePosition = new Vector2(-8, 0);
        
        schedule.walkSpeed = 1.5f;
        schedule.pauseAtDestination = 2f;
        schedule.moveAroundWhenIdle = true;
        schedule.idleMovementRange = 3f;
        
        var events = new List<ScheduleEvent>();
        
        // 07:00 - Morning at Village Entrance
        events.Add(new ScheduleEvent
        {
            hour = 7,
            targetObjectTag = "NPCTarget",
            targetObjectName = "VillageEntrance",
            targetPosition = new Vector2(-10, 2),
            behavior = NPCBehavior.Idle,
            customDialogue = new string[] 
            { 
                "Aku tahu jalan ke Desa Krandon, Kakak. Perjalanan dua hari melewati hutan." 
            }
        });
        
        // 12:00 - Midday Break
        events.Add(new ScheduleEvent
        {
            hour = 12,
            targetObjectTag = NPCScheduleData.CommonTags.Tavern,
            targetObjectName = "VillageTavern",
            targetPosition = new Vector2(-6, 1),
            behavior = NPCBehavior.Idle
        });
        
        // 15:00 - Afternoon Preparation
        events.Add(new ScheduleEvent
        {
            hour = 15,
            targetObjectTag = NPCScheduleData.CommonTags.House,
            targetObjectName = "GuideHut",
            targetPosition = new Vector2(-8, 0),
            behavior = NPCBehavior.Work
        });
        
        // 18:00 - Evening at Tavern
        events.Add(new ScheduleEvent
        {
            hour = 18,
            targetObjectTag = NPCScheduleData.CommonTags.Tavern,
            targetObjectName = "VillageTavern",
            targetPosition = new Vector2(-6, 1),
            behavior = NPCBehavior.Interact
        });
        
        // 22:00 - Sleep
        events.Add(new ScheduleEvent
        {
            hour = 22,
            targetObjectTag = NPCScheduleData.CommonTags.House,
            targetObjectName = "GuideHut",
            targetPosition = new Vector2(-8, 0),
            behavior = NPCBehavior.Sleep,
            shouldDespawn = true
        });
        
        schedule.scheduleEvents = events.ToArray();
        
        string path = Path.Combine(outputPath, "JokoGuide_Schedule.asset");
        AssetDatabase.CreateAsset(schedule, path);
        Debug.Log($"✅ Created: {path}");
    }
    
    #endregion
    
    #region Village NPC Schedules
    
    private void GenerateVillageNPCSchedules()
    {
        GenerateFarmerSchedules();
        GenerateThirstyVillagersSchedules();
        GenerateTraderSchedules();
        GenerateElderSchedules();
        GenerateChildrenSchedules();
    }
    
    private void GenerateFarmerSchedules()
    {
        string[] farmerNames = { "PakTani", "BuTani", "MudaFarmer" };
        
        for (int i = 0; i < farmerNames.Length; i++)
        {
            var schedule = ScriptableObject.CreateInstance<NPCScheduleData>();
            schedule.scheduleName = $"{farmerNames[i]} Schedule";
            schedule.scheduleDescription = $"Farmer {i + 1} - Agricultural work schedule";
            schedule.spawnHour = 5; // Early morning for farm work
            
            schedule.homeObjectTag = NPCScheduleData.CommonTags.House;
            schedule.homeObjectName = $"FarmHouse{i + 1}";
            schedule.homePosition = new Vector2(8 + i * 2, -5);
            
            schedule.walkSpeed = 1.3f;
            schedule.pauseAtDestination = 2f;
            schedule.moveAroundWhenIdle = true;
            schedule.idleMovementRange = 4f; // Large area for farm work
            
            var events = new List<ScheduleEvent>();
            
            // 05:00 - Early Farm Work
            events.Add(new ScheduleEvent
            {
                hour = 5,
                targetObjectTag = NPCScheduleData.CommonTags.Farm,
                targetObjectName = $"RiceField{i + 1}",
                targetPosition = new Vector2(10 + i * 3, -8),
                behavior = NPCBehavior.Work,
                customDialogue = new string[] 
                { 
                    "Padi tumbuh dengan baik setelah bendungan selesai. Air mengalir lancar sekarang." 
                }
            });
            
            // 08:00 - Check Irrigation
            events.Add(new ScheduleEvent
            {
                hour = 8,
                targetObjectTag = "NPCTarget",
                targetObjectName = "IrrigationChannel",
                targetPosition = new Vector2(6, -6),
                behavior = NPCBehavior.Work
            });
            
            // 12:00 - Midday Rest
            events.Add(new ScheduleEvent
            {
                hour = 12,
                targetObjectTag = NPCScheduleData.CommonTags.House,
                targetObjectName = $"FarmHouse{i + 1}",
                targetPosition = new Vector2(8 + i * 2, -5),
                behavior = NPCBehavior.Idle
            });
            
            // 14:00 - Afternoon Farm Work
            events.Add(new ScheduleEvent
            {
                hour = 14,
                targetObjectTag = NPCScheduleData.CommonTags.Farm,
                targetObjectName = $"RiceField{i + 1}",
                targetPosition = new Vector2(10 + i * 3, -8),
                behavior = NPCBehavior.Work
            });
            
            // 17:00 - Market Visit
            events.Add(new ScheduleEvent
            {
                hour = 17,
                targetObjectTag = NPCScheduleData.CommonTags.Market,
                targetObjectName = "VillageMarket",
                targetPosition = new Vector2(0, -4),
                behavior = NPCBehavior.Interact
            });
            
            // 19:00 - Evening Home
            events.Add(new ScheduleEvent
            {
                hour = 19,
                targetObjectTag = NPCScheduleData.CommonTags.House,
                targetObjectName = $"FarmHouse{i + 1}",
                targetPosition = new Vector2(8 + i * 2, -5),
                behavior = NPCBehavior.Idle
            });
            
            // 21:00 - Sleep
            events.Add(new ScheduleEvent
            {
                hour = 21,
                targetObjectTag = NPCScheduleData.CommonTags.House,
                targetObjectName = $"FarmHouse{i + 1}",
                targetPosition = new Vector2(8 + i * 2, -5),
                behavior = NPCBehavior.Sleep,
                shouldDespawn = true
            });
            
            schedule.scheduleEvents = events.ToArray();
            
            string path = Path.Combine(outputPath, $"{farmerNames[i]}_Schedule.asset");
            AssetDatabase.CreateAsset(schedule, path);
            Debug.Log($"✅ Created: {path}");
        }
    }
    
    private void GenerateThirstyVillagersSchedules()
    {
        string[] villagerNames = { "PakDarmo", "BuSiti", "WargaHaus3", "WargaHaus4" };
        
        for (int i = 0; i < villagerNames.Length; i++)
        {
            var schedule = ScriptableObject.CreateInstance<NPCScheduleData>();
            schedule.scheduleName = $"{villagerNames[i]} Schedule";
            schedule.scheduleDescription = $"Villager affected by water crisis";
            schedule.spawnHour = 6;
            
            schedule.homeObjectTag = NPCScheduleData.CommonTags.House;
            schedule.homeObjectName = $"VillageHouse{i + 1}";
            schedule.homePosition = new Vector2(-5 + i * 2, -8);
            
            schedule.walkSpeed = 1.0f; // Slower due to hardship
            schedule.pauseAtDestination = 3f;
            schedule.moveAroundWhenIdle = false; // Conserving energy
            
            var events = new List<ScheduleEvent>();
            
            // 06:00 - Early Water Search
            events.Add(new ScheduleEvent
            {
                hour = 6,
                targetObjectTag = NPCScheduleData.CommonTags.Well,
                targetObjectName = "VillageWell",
                targetPosition = new Vector2(0, -6),
                behavior = NPCBehavior.Interact,
                customDialogue = new string[] 
                { 
                    "Tolong, anak muda! Anak-anakku sudah berhari-hari tidak mendapat air bersih!" 
                }
            });
            
            // 09:00 - Rest at Home
            events.Add(new ScheduleEvent
            {
                hour = 9,
                targetObjectTag = NPCScheduleData.CommonTags.House,
                targetObjectName = $"VillageHouse{i + 1}",
                targetPosition = new Vector2(-5 + i * 2, -8),
                behavior = NPCBehavior.Idle
            });
            
            // 15:00 - Afternoon Water Check
            events.Add(new ScheduleEvent
            {
                hour = 15,
                targetObjectTag = NPCScheduleData.CommonTags.Well,
                targetObjectName = "VillageWell",
                targetPosition = new Vector2(0, -6),
                behavior = NPCBehavior.Interact
            });
            
            // 18:00 - Evening Home
            events.Add(new ScheduleEvent
            {
                hour = 18,
                targetObjectTag = NPCScheduleData.CommonTags.House,
                targetObjectName = $"VillageHouse{i + 1}",
                targetPosition = new Vector2(-5 + i * 2, -8),
                behavior = NPCBehavior.Idle,
                shouldIdleWhenReached = true
            });
            
            // 20:00 - Sleep
            events.Add(new ScheduleEvent
            {
                hour = 20,
                targetObjectTag = NPCScheduleData.CommonTags.House,
                targetObjectName = $"VillageHouse{i + 1}",
                targetPosition = new Vector2(-5 + i * 2, -8),
                behavior = NPCBehavior.Sleep,
                shouldDespawn = true
            });
            
            schedule.scheduleEvents = events.ToArray();
            
            string path = Path.Combine(outputPath, $"{villagerNames[i]}_Schedule.asset");
            AssetDatabase.CreateAsset(schedule, path);
            Debug.Log($"✅ Created: {path}");
        }
    }
    
    private void GenerateTraderSchedules()
    {
        // Implementation for trader NPCs
        Debug.Log("Generating trader schedules...");
    }
    
    private void GenerateElderSchedules()
    {
        // Implementation for elder NPCs (Nenek Bijak, etc.)
        Debug.Log("Generating elder schedules...");
    }
    
    private void GenerateChildrenSchedules()
    {
        // Implementation for children NPCs
        Debug.Log("Generating children schedules...");
    }
    
    #endregion
    
    #region Conditional Schedule Examples
    
    /// <summary>
    /// Generate example conditional schedules for padepokan students to demonstrate the system
    /// </summary>
    public void GenerateStudentConditionalSchedules()
    {
        Debug.Log("Generating conditional schedules for padepokan students...");
        
        string[] studentNames = { "AndiStudent", "BudiStudent", "CandraStudent" };
        
        for (int i = 0; i < studentNames.Length; i++)
        {
            // Create normal student schedule
            var normalSchedule = CreateNormalStudentSchedule(studentNames[i], i);
            string normalPath = Path.Combine(outputPath, $"{studentNames[i]}_Normal_Schedule.asset");
            AssetDatabase.CreateAsset(normalSchedule, normalPath);
            Debug.Log($"✅ Created normal schedule: {normalPath}");
            
            // Create dam construction schedule
            var damSchedule = CreateDamConstructionSchedule(studentNames[i], i);
            string damPath = Path.Combine(outputPath, $"{studentNames[i]}_DamConstruction_Schedule.asset");
            AssetDatabase.CreateAsset(damSchedule, damPath);
            Debug.Log($"✅ Created dam construction schedule: {damPath}");
        }
    }
    
    private NPCScheduleData CreateNormalStudentSchedule(string studentName, int index)
    {
        var schedule = ScriptableObject.CreateInstance<NPCScheduleData>();
        schedule.scheduleName = $"{studentName} Normal Schedule";
        schedule.scheduleDescription = $"Normal padepokan study routine for {studentName}";
        schedule.spawnHour = 6;
        
        schedule.homeObjectTag = NPCScheduleData.CommonTags.House;
        schedule.homeObjectName = $"StudentDormitory{index + 1}";
        schedule.homePosition = new Vector2(-2 + index, -3);
        
        schedule.walkSpeed = 1.8f;
        schedule.pauseAtDestination = 1.5f;
        schedule.moveAroundWhenIdle = true;
        schedule.idleMovementRange = 2f;
        
        var events = new List<ScheduleEvent>();
        
        // 06:00 - Morning Practice
        events.Add(new ScheduleEvent
        {
            hour = 6,
            targetObjectTag = "NPCTarget",
            targetObjectName = "TrainingGrounds",
            targetPosition = new Vector2(-1, 1),
            behavior = NPCBehavior.Work,
            customDialogue = new string[] { $"Latihan pagi membantu konsentrasi sepanjang hari." }
        });
        
        // 08:00 - Studies
        events.Add(new ScheduleEvent
        {
            hour = 8,
            targetObjectTag = "NPCTarget",
            targetObjectName = "StudyHall",
            targetPosition = new Vector2(1, 2),
            behavior = NPCBehavior.Idle,
            customDialogue = new string[] { $"Kami sedang belajar tentang kebijaksanaan kuno." }
        });
        
        // 12:00 - Lunch
        events.Add(new ScheduleEvent
        {
            hour = 12,
            targetObjectTag = "NPCTarget",
            targetObjectName = "PadepokanDiningHall",
            targetPosition = new Vector2(0, -2),
            behavior = NPCBehavior.Idle
        });
        
        // 14:00 - Practical Work
        events.Add(new ScheduleEvent
        {
            hour = 14,
            targetObjectTag = "NPCTarget",
            targetObjectName = "WorkArea",
            targetPosition = new Vector2(2, 0),
            behavior = NPCBehavior.Work,
            customDialogue = new string[] { $"Bekerja dengan tangan sama pentingnya dengan belajar dari buku." }
        });
        
        // 17:00 - Free Time
        events.Add(new ScheduleEvent
        {
            hour = 17,
            targetObjectTag = "NPCTarget",
            targetObjectName = "PadepokanCourtyard",
            targetPosition = new Vector2(0, -1),
            behavior = NPCBehavior.Interact,
            customDialogue = new string[] { $"Waktu istirahat untuk bersosialisasi dengan teman-teman." }
        });
        
        // 20:00 - Evening Rest
        events.Add(new ScheduleEvent
        {
            hour = 20,
            targetObjectTag = NPCScheduleData.CommonTags.House,
            targetObjectName = $"StudentDormitory{index + 1}",
            targetPosition = new Vector2(-2 + index, -3),
            behavior = NPCBehavior.Idle
        });
        
        // 22:00 - Sleep
        events.Add(new ScheduleEvent
        {
            hour = 22,
            targetObjectTag = NPCScheduleData.CommonTags.House,
            targetObjectName = $"StudentDormitory{index + 1}",
            targetPosition = new Vector2(-2 + index, -3),
            behavior = NPCBehavior.Sleep,
            shouldDespawn = true
        });
        
        schedule.scheduleEvents = events.ToArray();
        return schedule;
    }
    
    private NPCScheduleData CreateDamConstructionSchedule(string studentName, int index)
    {
        var schedule = ScriptableObject.CreateInstance<NPCScheduleData>();
        schedule.scheduleName = $"{studentName} Dam Construction Schedule";
        schedule.scheduleDescription = $"Special schedule for {studentName} during dam construction project";
        schedule.spawnHour = 5; // Earlier start for construction work
        
        schedule.homeObjectTag = NPCScheduleData.CommonTags.House;
        schedule.homeObjectName = $"StudentDormitory{index + 1}";
        schedule.homePosition = new Vector2(-2 + index, -3);
        
        schedule.walkSpeed = 2.0f; // Faster movement for work
        schedule.pauseAtDestination = 1.0f;
        schedule.moveAroundWhenIdle = true;
        schedule.idleMovementRange = 4f; // Larger work area
        
        var events = new List<ScheduleEvent>();
        
        // 05:00 - Early preparation
        events.Add(new ScheduleEvent
        {
            hour = 5,
            targetObjectTag = "NPCTarget",
            targetObjectName = "PadepokanCourtyard",
            targetPosition = new Vector2(0, -1),
            behavior = NPCBehavior.Work,
            customDialogue = new string[] { $"Kami bangun lebih awal untuk membantu proyek bendungan." }
        });
        
        // 06:00 - Travel to dam site
        events.Add(new ScheduleEvent
        {
            hour = 6,
            targetObjectTag = "NPCTarget",
            targetObjectName = "DamConstructionSite",
            targetPosition = new Vector2(15, -10), // Different location for dam
            behavior = NPCBehavior.Work,
            customDialogue = new string[] { $"Proyek ini akan membantu seluruh desa mendapatkan air bersih." }
        });
        
        // 08:00 - Heavy construction work
        events.Add(new ScheduleEvent
        {
            hour = 8,
            targetObjectTag = "NPCTarget",
            targetObjectName = "DamConstructionSite",
            targetPosition = new Vector2(16, -11),
            behavior = NPCBehavior.Work,
            customDialogue = new string[] { $"Kerja sama adalah kunci keberhasilan proyek besar seperti ini." }
        });
        
        // 12:00 - Lunch break at site
        events.Add(new ScheduleEvent
        {
            hour = 12,
            targetObjectTag = "NPCTarget",
            targetObjectName = "ConstructionCamp",
            targetPosition = new Vector2(14, -9),
            behavior = NPCBehavior.Idle,
            customDialogue = new string[] { $"Istirahat sejenak, lalu kembali bekerja." }
        });
        
        // 13:00 - Afternoon construction
        events.Add(new ScheduleEvent
        {
            hour = 13,
            targetObjectTag = "NPCTarget",
            targetObjectName = "DamConstructionSite",
            targetPosition = new Vector2(17, -12),
            behavior = NPCBehavior.Work
        });
        
        // 17:00 - Return to padepokan
        events.Add(new ScheduleEvent
        {
            hour = 17,
            targetObjectTag = "NPCTarget",
            targetObjectName = "PadepokanCourtyard",
            targetPosition = new Vector2(0, -1),
            behavior = NPCBehavior.Interact,
            customDialogue = new string[] { $"Hari yang melelahkan tapi memuaskan. Proyek berjalan lancar." }
        });
        
        // 19:00 - Evening rest
        events.Add(new ScheduleEvent
        {
            hour = 19,
            targetObjectTag = NPCScheduleData.CommonTags.House,
            targetObjectName = $"StudentDormitory{index + 1}",
            targetPosition = new Vector2(-2 + index, -3),
            behavior = NPCBehavior.Idle
        });
        
        // 21:00 - Early sleep (tired from construction)
        events.Add(new ScheduleEvent
        {
            hour = 21,
            targetObjectTag = NPCScheduleData.CommonTags.House,
            targetObjectName = $"StudentDormitory{index + 1}",
            targetPosition = new Vector2(-2 + index, -3),
            behavior = NPCBehavior.Sleep,
            shouldDespawn = true
        });
        
        schedule.scheduleEvents = events.ToArray();
        return schedule;
    }
    
    #endregion
}

#endif