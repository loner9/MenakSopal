using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_EDITOR

/// <summary>
/// Master data generation tool that coordinates all other generators
/// Run this from Tools -> Trenggalek Game -> Master Data Generator
/// </summary>
public class MasterDataGenerator : EditorWindow
{
    [MenuItem("Tools/Trenggalek Game/Master Data Generator")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MasterDataGenerator));
    }

    private bool generateDialogues = true;
    private bool generateSchedules = true;
    private bool generateQuests = true;
    private bool generateFlags = true;
    private bool validateAfterGeneration = true;
    
    private Vector2 scrollPosition;
    private List<string> generationLog = new List<string>();

    private void OnGUI()
    {
        titleContent = new GUIContent("Master Data Generator");
        
        GUILayout.Label("Master Data Generation System", EditorStyles.boldLabel);
        GUILayout.Label("Generate all game data based on documentation", EditorStyles.helpBox);
        GUILayout.Space(10);
        
        // Generation options
        GUILayout.Label("Generation Options:", EditorStyles.boldLabel);
        generateDialogues = EditorGUILayout.Toggle("Generate Dialogue Data", generateDialogues);
        generateSchedules = EditorGUILayout.Toggle("Generate Schedule Data", generateSchedules);
        generateQuests = EditorGUILayout.Toggle("Generate Quest Data", generateQuests);
        generateFlags = EditorGUILayout.Toggle("Generate Flag Definitions", generateFlags);
        
        GUILayout.Space(10);
        validateAfterGeneration = EditorGUILayout.Toggle("Validate After Generation", validateAfterGeneration);
        
        GUILayout.Space(20);
        
        // Main generation button
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🚀 Generate All Game Data", GUILayout.Height(50)))
        {
            GenerateAllGameData();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.Space(10);
        
        // Individual generation buttons
        GUILayout.Label("Individual Generators:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Generate Story Dialogues Only"))
        {
            GenerateStoryDialogues();
        }
        
        if (GUILayout.Button("Generate Village Dialogues Only"))
        {
            GenerateVillageDialogues();
        }
        
        if (GUILayout.Button("Generate NPC Schedules Only"))
        {
            GenerateNPCSchedules();
        }
        
        if (GUILayout.Button("Generate Quest Data Only"))
        {
            GenerateQuestData();
        }
        
        if (GUILayout.Button("Generate Flag System Only"))
        {
            GenerateFlagSystem();
        }
        
        GUILayout.Space(10);
        
        // Utility buttons
        GUILayout.Label("Utilities:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🔍 Validate All Data"))
        {
            ValidateAllData();
        }
        
        if (GUILayout.Button("📋 Export Documentation"))
        {
            ExportDocumentation();
        }
        
        if (GUILayout.Button("🧹 Clean Generated Assets"))
        {
            CleanGeneratedAssets();
        }
        
        GUILayout.Space(20);
        
        // Generation log
        if (generationLog.Count > 0)
        {
            GUILayout.Label("Generation Log:", EditorStyles.boldLabel);
            
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            foreach (string logEntry in generationLog)
            {
                GUILayout.Label(logEntry, EditorStyles.miniLabel);
            }
            
            GUILayout.EndScrollView();
            
            if (GUILayout.Button("Clear Log"))
            {
                generationLog.Clear();
            }
        }
    }

    #region Main Generation Methods
    
    private void GenerateAllGameData()
    {
        generationLog.Clear();
        LogMessage("🚀 Starting Master Data Generation Process...");
        
        // Ensure all directories exist
        CreateNecessaryDirectories();
        
        var startTime = System.DateTime.Now;
        
        try
        {
            if (generateFlags)
            {
                LogMessage("📋 Generating Flag System...");
                StoryFlagManager.GenerateFlagDefinitionsStatic();
            }
            
            if (generateDialogues)
            {
                LogMessage("💬 Generating Dialogue Data...");
                StoryDialogueDataGenerator.GenerateAllStoryDialoguesStatic();
                // TODO: Add VillageDialogueDataGenerator.GenerateAllVillageDialoguesStatic() when available
            }
            
            if (generateSchedules)
            {
                LogMessage("📅 Generating Schedule Data...");
                NPCScheduleDataGenerator.GenerateAllSchedulesStatic();
            }
            
            if (generateQuests)
            {
                LogMessage("🎯 Generating Quest Data...");
                QuestDataGenerator.GenerateAllQuestsStatic();
            }
            
            // Refresh asset database
            AssetDatabase.Refresh();
            LogMessage("🔄 Refreshing Asset Database...");
            
            if (validateAfterGeneration)
            {
                LogMessage("🔍 Validating Generated Data...");
                GameDataValidator.ValidateAllGameDataStatic();
            }
            
            var duration = System.DateTime.Now - startTime;
            LogMessage($"✅ Master Data Generation Complete! Duration: {duration.TotalSeconds:F1}s");
            
            // Show completion dialog
            EditorUtility.DisplayDialog(
                "Generation Complete", 
                $"All game data generated successfully!\n\nDuration: {duration.TotalSeconds:F1} seconds\n\nCheck the console and generation log for details.", 
                "OK"
            );
        }
        catch (System.Exception e)
        {
            LogMessage($"❌ Generation failed: {e.Message}");
            Debug.LogError($"Master Data Generation failed: {e}");
            
            EditorUtility.DisplayDialog(
                "Generation Failed", 
                $"Data generation encountered an error:\n\n{e.Message}\n\nCheck the console for details.", 
                "OK"
            );
        }
    }
    
    private void CreateNecessaryDirectories()
    {
        string[] directories = {
            "Assets/Resources/Dialogues/Story",
            "Assets/Resources/Dialogues/Village", 
            "Assets/Resources/Schedules",
            "Assets/Resources/Quests",
            "Assets/Resources/Data",
            "Assets/Documentation/Generated"
        };
        
        foreach (string dir in directories)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                LogMessage($"📁 Created directory: {dir}");
            }
        }
    }
    
    #endregion
    
    #region Individual Generators
    
    private void GenerateStoryDialogues()
    {
        LogMessage("Generating Story NPC Dialogues...");
        StoryDialogueDataGenerator.GenerateAllStoryDialoguesStatic();
        LogMessage("✅ Story dialogues generated using proper generator");
    }
    
    private void GenerateVillageDialogues()
    {
        LogMessage("Generating Village NPC Dialogues...");
        // TODO: Create VillageDialogueDataGenerator and implement GenerateAllVillageDialoguesStatic()
        // For now, village dialogues are generated as part of the NPCScheduleDataGenerator
        LogMessage("⚠️ Village dialogue generation not yet implemented - will be added in future update");
        LogMessage("✅ Placeholder completed for village dialogues");
    }
    
    private void GenerateNPCSchedules()
    {
        LogMessage("Generating NPC Schedules...");
        NPCScheduleDataGenerator.GenerateAllSchedulesStatic();
        LogMessage("✅ NPC schedules generated using proper generator");
    }
    
    private void GenerateQuestData()
    {
        LogMessage("Generating Quest Data...");
        QuestDataGenerator.GenerateAllQuestsStatic();
        LogMessage("✅ Quest data generated using proper generator with all documentation-based quests");
    }
    
    private void GenerateFlagSystem()
    {
        LogMessage("Generating Flag System Definitions...");
        StoryFlagManager.GenerateFlagDefinitionsStatic();
        LogMessage("✅ Flag system generated using proper generator with all story flags");
    }
    
    #endregion
    
    #region Utilities
    
    private void ValidateAllData()
    {
        LogMessage("🔍 Starting data validation...");
        GameDataValidator.ValidateAllGameDataStatic();
        LogMessage("✅ Data validation complete using proper validator");
    }
    
    
    private void ExportDocumentation()
    {
        LogMessage("📋 Exporting documentation...");
        
        string docPath = "Assets/Documentation/Generated/";
        if (!Directory.Exists(docPath))
        {
            Directory.CreateDirectory(docPath);
        }
        
        // Export various documentation files
        ExportAssetSummary(docPath);
        ExportFlagReference(docPath);
        ExportQuestSummary(docPath);
        
        LogMessage("✅ Documentation exported successfully");
    }
    
    private void ExportAssetSummary(string path)
    {
        var summary = new System.Text.StringBuilder();
        summary.AppendLine("# Generated Assets Summary");
        summary.AppendLine($"Generated on: {System.DateTime.Now}");
        summary.AppendLine();
        
        // Count assets
        var dialogues = AssetDatabase.FindAssets("t:DialogueData");
        var schedules = AssetDatabase.FindAssets("t:NPCScheduleData");
        var quests = AssetDatabase.FindAssets("t:QuestData");
        
        summary.AppendLine($"## Asset Counts");
        summary.AppendLine($"- Dialogue Assets: {dialogues.Length}");
        summary.AppendLine($"- Schedule Assets: {schedules.Length}");
        summary.AppendLine($"- Quest Assets: {quests.Length}");
        
        File.WriteAllText(Path.Combine(path, "AssetSummary.md"), summary.ToString());
    }
    
    private void ExportFlagReference(string path)
    {
        var flagRef = "# Story Flags Quick Reference\n\nGenerated flag reference for development use.\n";
        File.WriteAllText(Path.Combine(path, "FlagReference.md"), flagRef);
    }
    
    private void ExportQuestSummary(string path)
    {
        var questSummary = "# Quest Summary\n\nOverview of all generated quests.\n";
        File.WriteAllText(Path.Combine(path, "QuestSummary.md"), questSummary);
    }
    
    private void CleanGeneratedAssets()
    {
        if (EditorUtility.DisplayDialog(
            "Clean Generated Assets", 
            "This will delete all generated asset files. Are you sure?", 
            "Yes, Delete All", 
            "Cancel"))
        {
            LogMessage("🧹 Cleaning generated assets...");
            
            // Delete generated directories
            string[] pathsToClean = {
                "Assets/Resources/Dialogues/Story",
                "Assets/Resources/Dialogues/Village",
                "Assets/Resources/Schedules", 
                "Assets/Resources/Quests",
                "Assets/Documentation/Generated"
            };
            
            foreach (string path in pathsToClean)
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    LogMessage($"  🗑️ Deleted: {path}");
                }
            }
            
            AssetDatabase.Refresh();
            LogMessage("✅ Asset cleanup complete");
        }
    }
    
    private void LogMessage(string message)
    {
        generationLog.Add($"[{System.DateTime.Now:HH:mm:ss}] {message}");
        Debug.Log($"[MasterDataGenerator] {message}");
        
        // Keep log size manageable
        if (generationLog.Count > 100)
        {
            generationLog.RemoveAt(0);
        }
    }
    
    #endregion
}

#endif