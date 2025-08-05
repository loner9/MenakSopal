using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// UI controller for save/load functionality.
/// Provides interface for manual saving/loading and save file management.
/// </summary>
public class SaveLoadUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject saveLoadPanel;
    public Transform saveSlotContainer;
    public GameObject saveSlotPrefab;
    public Button saveButton;
    public Button loadButton;
    public Button deleteButton;
    public Button backButton;
    
    [Header("New Save UI")]
    public InputField saveNameInput;
    public Button createSaveButton;
    
    [Header("Save Info Display")]
    public Text selectedSaveInfo;
    public Text saveTimeText;
    public Text sceneText;
    public Text playTimeText;
    public Text flagCountText;
    
    private List<SaveSlotUI> saveSlots = new List<SaveSlotUI>();
    private SaveFileInfo selectedSave;
    
    void Start()
    {
        SetupUI();
        RefreshSaveList();
    }
    
    void SetupUI()
    {
        // Setup button events
        if (saveButton) saveButton.onClick.AddListener(OnSaveButtonClicked);
        if (loadButton) loadButton.onClick.AddListener(OnLoadButtonClicked);
        if (deleteButton) deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        if (backButton) backButton.onClick.AddListener(OnBackButtonClicked);
        if (createSaveButton) createSaveButton.onClick.AddListener(OnCreateSaveClicked);
        
        // Setup save manager events
        GameSaveManager.OnGameSaved += OnGameSaved;
        GameSaveManager.OnGameLoaded += OnGameLoaded;
        GameSaveManager.OnSaveError += OnSaveError;
        
        // Initially disable action buttons
        if (loadButton) loadButton.interactable = false;
        if (deleteButton) deleteButton.interactable = false;
    }
    
    #region Save List Management
    
    void RefreshSaveList()
    {
        // Clear existing slots
        ClearSaveSlots();
        
        // Get save files
        var saveFiles = GameSaveManager.Instance.GetSaveFiles();
        
        // Create slot UI for each save
        foreach (var saveFile in saveFiles)
        {
            CreateSaveSlot(saveFile);
        }
        
        // Update UI state
        UpdateButtonStates();
    }
    
    void CreateSaveSlot(SaveFileInfo saveFile)
    {
        if (saveSlotPrefab == null || saveSlotContainer == null) return;
        
        GameObject slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
        SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();
        
        if (slotUI == null)
        {
            slotUI = slotObj.AddComponent<SaveSlotUI>();
        }
        
        slotUI.Initialize(saveFile, OnSaveSlotSelected);
        saveSlots.Add(slotUI);
    }
    
    void ClearSaveSlots()
    {
        foreach (var slot in saveSlots)
        {
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }
        saveSlots.Clear();
        selectedSave = null;
    }
    
    #endregion
    
    #region UI Events
    
    void OnSaveSlotSelected(SaveFileInfo saveFile)
    {
        selectedSave = saveFile;
        UpdateSelectedSaveInfo();
        UpdateButtonStates();
        
        // Update visual selection
        foreach (var slot in saveSlots)
        {
            slot.SetSelected(slot.SaveFile == saveFile);
        }
    }
    
    void OnSaveButtonClicked()
    {
        if (selectedSave != null)
        {
            // Overwrite existing save
            bool success = GameSaveManager.Instance.SaveGame(selectedSave.slotName);
            if (success)
            {
                RefreshSaveList();
            }
        }
    }
    
    void OnLoadButtonClicked()
    {
        if (selectedSave != null)
        {
            bool success = GameSaveManager.Instance.LoadGame(selectedSave.slotName);
            if (success)
            {
                CloseSaveLoadPanel();
            }
        }
    }
    
    void OnDeleteButtonClicked()
    {
        if (selectedSave != null)
        {
            // Show confirmation dialog
            ShowDeleteConfirmation(selectedSave.slotName);
        }
    }
    
    void OnCreateSaveClicked()
    {
        string saveName = saveNameInput?.text?.Trim();
        
        if (string.IsNullOrEmpty(saveName))
        {
            saveName = $"Save_{System.DateTime.Now:yyyyMMdd_HHmmss}";
        }
        
        bool success = GameSaveManager.Instance.SaveGame(saveName);
        if (success)
        {
            if (saveNameInput) saveNameInput.text = "";
            RefreshSaveList();
        }
    }
    
    void OnBackButtonClicked()
    {
        CloseSaveLoadPanel();
    }
    
    #endregion
    
    #region Save Manager Events
    
    void OnGameSaved(string slotName)
    {
        RefreshSaveList();
        ShowNotification($"Game saved: {slotName}");
    }
    
    void OnGameLoaded(string slotName)
    {
        ShowNotification($"Game loaded: {slotName}");
    }
    
    void OnSaveError(string error)
    {
        ShowNotification($"Error: {error}");
    }
    
    #endregion
    
    #region UI Updates
    
    void UpdateSelectedSaveInfo()
    {
        if (selectedSave == null)
        {
            if (selectedSaveInfo) selectedSaveInfo.text = "No save selected";
            if (saveTimeText) saveTimeText.text = "";
            if (sceneText) sceneText.text = "";
            if (playTimeText) playTimeText.text = "";
            if (flagCountText) flagCountText.text = "";
            return;
        }
        
        if (selectedSaveInfo) selectedSaveInfo.text = selectedSave.slotName;
        if (saveTimeText) saveTimeText.text = $"Saved: {selectedSave.saveTime:yyyy/MM/dd HH:mm}";
        if (sceneText) sceneText.text = $"Scene: {selectedSave.currentScene}";
        if (playTimeText) playTimeText.text = $"Play Time: {selectedSave.GetFormattedPlayTime()}";
        if (flagCountText) flagCountText.text = $"Progress: {selectedSave.totalFlags} flags";
    }
    
    void UpdateButtonStates()
    {
        bool hasSelection = selectedSave != null;
        
        if (loadButton) loadButton.interactable = hasSelection;
        if (deleteButton) deleteButton.interactable = hasSelection;
        if (saveButton) saveButton.interactable = hasSelection;
    }
    
    #endregion
    
    #region Public Interface
    
    public void OpenSaveLoadPanel()
    {
        if (saveLoadPanel) saveLoadPanel.SetActive(true);
        RefreshSaveList();
    }
    
    public void CloseSaveLoadPanel()
    {
        if (saveLoadPanel) saveLoadPanel.SetActive(false);
    }
    
    public void QuickSave()
    {
        GameSaveManager.Instance.TriggerManualSave();
    }
    
    public void QuickLoad()
    {
        GameSaveManager.Instance.TriggerQuickLoad();
    }
    
    #endregion
    
    #region Helper Methods
    
    void ShowDeleteConfirmation(string slotName)
    {
        // Simple confirmation - you can replace with a proper dialog
        if (UnityEngine.Application.isEditor)
        {
            if (UnityEditor.EditorUtility.DisplayDialog("Delete Save", 
                $"Are you sure you want to delete '{slotName}'?", "Delete", "Cancel"))
            {
                ConfirmDelete(slotName);
            }
        }
        else
        {
            // For builds, you'd want to implement a proper in-game confirmation dialog
            Debug.Log($"Delete confirmation needed for: {slotName}");
            // For now, directly delete (implement proper dialog later)
            ConfirmDelete(slotName);
        }
    }
    
    void ConfirmDelete(string slotName)
    {
        bool success = GameSaveManager.Instance.DeleteSave(slotName);
        if (success)
        {
            RefreshSaveList();
            ShowNotification($"Deleted save: {slotName}");
        }
    }
    
    void ShowNotification(string message)
    {
        // Simple notification - integrate with your UI notification system
        Debug.Log($"[Save/Load] {message}");
        
        // You can implement this with your existing UI message system
        var gameSystemsManager = FindObjectOfType<GameSystemsManager>();
        if (gameSystemsManager != null)
        {
            // Use your existing message display if available
        }
    }
    
    #endregion
    
    void OnDestroy()
    {
        // Cleanup events
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.OnGameSaved -= OnGameSaved;
            GameSaveManager.OnGameLoaded -= OnGameLoaded;
            GameSaveManager.OnSaveError -= OnSaveError;
        }
    }
}

/// <summary>
/// Individual save slot UI component
/// </summary>
public class SaveSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public Text slotNameText;
    public Text saveTimeText;
    public Text sceneText;
    public Button selectButton;
    public Image backgroundImage;
    
    [Header("Visual States")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.cyan;
    
    private SaveFileInfo saveFile;
    private System.Action<SaveFileInfo> onSelected;
    
    public SaveFileInfo SaveFile => saveFile;
    
    public void Initialize(SaveFileInfo saveFile, System.Action<SaveFileInfo> onSelected)
    {
        this.saveFile = saveFile;
        this.onSelected = onSelected;
        
        UpdateDisplay();
        SetupButton();
    }
    
    void UpdateDisplay()
    {
        if (slotNameText) slotNameText.text = saveFile.slotName;
        if (saveTimeText) saveTimeText.text = saveFile.saveTime.ToString("MM/dd HH:mm");
        if (sceneText) sceneText.text = saveFile.currentScene;
    }
    
    void SetupButton()
    {
        if (selectButton)
        {
            selectButton.onClick.AddListener(() => onSelected?.Invoke(saveFile));
        }
        else
        {
            // If no button component, add one to this gameobject
            Button btn = GetComponent<Button>();
            if (btn == null) btn = gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() => onSelected?.Invoke(saveFile));
        }
    }
    
    public void SetSelected(bool selected)
    {
        if (backgroundImage)
        {
            backgroundImage.color = selected ? selectedColor : normalColor;
        }
    }
}