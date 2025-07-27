using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestEntryUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("REQUIRED: Text component to display quest title")]
    public TextMeshProUGUI questTitleText;
    
    [Header("Optional UI Elements")]
    [Tooltip("Optional: Text to show quest type (Main, Side, etc.)")]
    public TextMeshProUGUI questTypeText;
    [Tooltip("Optional: Text to show quest progress (2/3 objectives)")]
    public TextMeshProUGUI questProgressText;
    [Tooltip("Optional: Image to show quest icon")]
    public Image questIconImage;
    [Tooltip("Optional: Image fill for progress bar visualization")]
    public Image progressBarFill;
    [Tooltip("Optional: Button for quest selection (if null, uses whole GameObject as button)")]
    public Button selectButton;
    [Tooltip("Optional: Background image for visual feedback")]
    public Image backgroundImage;
    
    [Header("Visual Settings")]
    public Color selectedColor = new Color(1f, 0.84f, 0f, 0.3f); // Gold tint
    public Color normalColor = Color.white;
    public Color completedColor = Color.green;
    public Color failedColor = Color.red;
    
    private QuestData questData;
    private QuestJournalUI journalUI;
    private bool isSelected = false;
    
    public void Initialize(QuestData quest, QuestJournalUI journal)
    {
        questData = quest;
        journalUI = journal;
        
        // Validate required components
        if (questTitleText == null)
        {
            Debug.LogError($"QuestEntryUI on {gameObject.name}: questTitleText is required but not assigned!");
            return;
        }
        
        SetupUI();
        SetupEvents();
        UpdateDisplay();
    }
    
    private void SetupUI()
    {
        // Setup button listener
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnQuestSelected);
        }
        else
        {
            // If no select button, make the whole entry clickable
            Button entryButton = GetComponent<Button>();
            if (entryButton == null)
                entryButton = gameObject.AddComponent<Button>();
            entryButton.onClick.AddListener(OnQuestSelected);
        }
    }
    
    private void SetupEvents()
    {
        if (journalUI != null)
        {
            journalUI.OnQuestSelected += OnAnyQuestSelected;
        }
    }
    
    private void UpdateDisplay()
    {
        if (questData == null) return;
        
        // Update quest title
        if (questTitleText != null)
        {
            questTitleText.text = questData.questTitle;
            questTitleText.color = GetQuestTextColor();
        }
        
        // Update quest type
        if (questTypeText != null)
        {
            questTypeText.text = questData.questType.ToString();
        }
        
        // Update progress
        if (questProgressText != null)
        {
            UpdateProgressText();
        }
        
        // Update quest icon
        if (questIconImage != null)
        {
            if (questData.questIcon != null)
            {
                questIconImage.sprite = questData.questIcon;
                questIconImage.gameObject.SetActive(true);
            }
            else
            {
                questIconImage.gameObject.SetActive(false);
            }
        }
        
        // Update progress bar
        if (progressBarFill != null)
        {
            UpdateProgressBar();
        }
        
        // Update background color
        UpdateBackgroundColor();
    }
    
    private void UpdateProgressText()
    {
        if (questData.status == QuestStatus.Completed)
        {
            questProgressText.text = "COMPLETED";
        }
        else if (questData.status == QuestStatus.Failed)
        {
            questProgressText.text = "FAILED";
        }
        else if (questData.status == QuestStatus.Active)
        {
            int completed = questData.GetCompletedObjectiveCount();
            int total = questData.GetTotalObjectiveCount();
            questProgressText.text = $"{completed}/{total} Objectives";
        }
        else
        {
            questProgressText.text = "";
        }
    }
    
    private void UpdateProgressBar()
    {
        float progress = questData.GetProgressPercentage();
        progressBarFill.fillAmount = progress;
        
        // Color the progress bar based on quest status
        switch (questData.status)
        {
            case QuestStatus.Active:
                progressBarFill.color = Color.yellow;
                break;
            case QuestStatus.Completed:
                progressBarFill.color = completedColor;
                break;
            case QuestStatus.Failed:
                progressBarFill.color = failedColor;
                break;
            default:
                progressBarFill.color = Color.gray;
                break;
        }
    }
    
    private Color GetQuestTextColor()
    {
        switch (questData.status)
        {
            case QuestStatus.Completed:
                return completedColor;
            case QuestStatus.Failed:
                return failedColor;
            case QuestStatus.Active:
                return Color.white;
            default:
                return Color.gray;
        }
    }
    
    private void UpdateBackgroundColor()
    {
        if (backgroundImage == null) return;
        
        if (isSelected)
        {
            backgroundImage.color = selectedColor;
        }
        else
        {
            switch (questData.status)
            {
                case QuestStatus.Active:
                    backgroundImage.color = normalColor;
                    break;
                case QuestStatus.Completed:
                    backgroundImage.color = Color.Lerp(normalColor, completedColor, 0.2f);
                    break;
                case QuestStatus.Failed:
                    backgroundImage.color = Color.Lerp(normalColor, failedColor, 0.2f);
                    break;
                default:
                    backgroundImage.color = normalColor;
                    break;
            }
        }
    }
    
    private void OnQuestSelected()
    {
        if (journalUI != null)
        {
            journalUI.SelectQuest(questData);
        }
    }
    
    private void OnAnyQuestSelected(QuestData selectedQuest)
    {
        isSelected = (selectedQuest == questData);
        UpdateBackgroundColor();
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateBackgroundColor();
    }
    
    private void OnDestroy()
    {
        if (journalUI != null)
        {
            journalUI.OnQuestSelected -= OnAnyQuestSelected;
        }
    }
}