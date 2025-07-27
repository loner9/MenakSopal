using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestObjectiveUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI objectiveText;
    public Image checkboxImage;
    public Image progressBarFill;
    public GameObject progressBarContainer;
    
    [Header("Checkbox Sprites")]
    public Sprite uncheckedSprite;
    public Sprite checkedSprite;
    
    [Header("Visual Settings")]
    public Color completedTextColor = Color.green;
    public Color activeTextColor = Color.white;
    public Color optionalTextColor = Color.gray;
    public Color progressBarColor = Color.yellow;
    public Color completedProgressColor = Color.green;
    
    private QuestObjective objectiveData;
    
    public void Initialize(QuestObjective objective)
    {
        objectiveData = objective;
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (objectiveData == null) return;
        
        // Update objective text
        if (objectiveText != null)
        {
            string displayText = objectiveData.GetProgressText();
            
            // Add optional indicator
            if (objectiveData.isOptional)
            {
                displayText = $"(Optional) {displayText}";
            }
            
            objectiveText.text = displayText;
            objectiveText.color = GetObjectiveTextColor();
            
            // Strike through completed objectives
            if (objectiveData.isCompleted)
            {
                objectiveText.fontStyle = FontStyles.Strikethrough;
            }
            else
            {
                objectiveText.fontStyle = FontStyles.Normal;
            }
        }
        
        // Update checkbox
        if (checkboxImage != null)
        {
            UpdateCheckbox();
        }
        
        // Update progress bar
        if (progressBarFill != null && progressBarContainer != null)
        {
            UpdateProgressBar();
        }
    }
    
    private void UpdateCheckbox()
    {
        if (objectiveData.isCompleted)
        {
            checkboxImage.sprite = checkedSprite;
            checkboxImage.color = completedTextColor;
        }
        else
        {
            checkboxImage.sprite = uncheckedSprite;
            checkboxImage.color = activeTextColor;
        }
    }
    
    private void UpdateProgressBar()
    {
        bool showProgressBar = ShouldShowProgressBar();
        progressBarContainer.SetActive(showProgressBar);
        
        if (!showProgressBar) return;
        
        float progress = objectiveData.GetProgressPercentage();
        progressBarFill.fillAmount = progress;
        
        // Color the progress bar
        if (objectiveData.isCompleted)
        {
            progressBarFill.color = completedProgressColor;
        }
        else
        {
            progressBarFill.color = progressBarColor;
        }
    }
    
    private bool ShouldShowProgressBar()
    {
        // Show progress bar for objectives that have measurable progress
        switch (objectiveData.type)
        {
            case ObjectiveType.CollectItems:
            case ObjectiveType.DefeatEnemies:
                return objectiveData.targetAmount > 1;
            case ObjectiveType.TimeDelay:
                return !objectiveData.isCompleted; // Show timer progress
            default:
                return false;
        }
    }
    
    private Color GetObjectiveTextColor()
    {
        if (objectiveData.isCompleted)
        {
            return completedTextColor;
        }
        else if (objectiveData.isOptional)
        {
            return optionalTextColor;
        }
        else
        {
            return activeTextColor;
        }
    }
    
    public void RefreshDisplay()
    {
        UpdateDisplay();
    }
}