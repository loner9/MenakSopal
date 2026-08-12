using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace MenakSopal.ChaseMinigame
{
    /// <summary>
    /// Manages UI panels for pre-game instructions, victory achievements, and defeat retry menus.
    /// </summary>
    public class ChaseUIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ChaseLevelManager levelManager;

        [Header("Scene Transition Settings")]
        [Tooltip("Scene to load when player completes level or exits after defeat")]
        [SerializeField] private string returnSceneName = "SceneAwal";

        [Header("Instruction Panel")]
        [SerializeField] private GameObject instructionPanel;
        [SerializeField] private TMP_Text instructionTitleTMP;
        [SerializeField] private TMP_Text instructionMessageTMP;
        [SerializeField] private Text instructionTitleText;
        [SerializeField] private Text instructionMessageText;
        [SerializeField] private Button startButton;

        [Header("Default Instructions")]
        [SerializeField] private string defaultTitle = "Lari dari Kejaran!";
        [TextArea(3, 5)]
        [SerializeField] private string defaultMessage = "Gunakan tombol W / S atau Panah Atas / Bawah untuk berpindah di antara 4 jalur.\n\nHindari rintangan di jalanan! Jika terkena rintangan, kamu akan terdorong mendekati pengejar.\n\nCapai Garis Finish untuk menang!";

        [Header("Victory Panel")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TMP_Text victoryTitleTMP;
        [SerializeField] private TMP_Text victoryMessageTMP;
        [SerializeField] private Text victoryTitleText;
        [SerializeField] private Text victoryMessageText;
        [SerializeField] private Button victoryContinueButton;

        [Header("Default Victory Messages")]
        [SerializeField] private string defaultVictoryTitle = "Berhasil Lolos!";
        [TextArea(2, 4)]
        [SerializeField] private string defaultVictoryMessage = "Hebat! Kamu berhasil menghindari pengejar dan mencapai Garis Finish dengan selamat!";

        [Header("Defeat Panel")]
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private TMP_Text defeatTitleTMP;
        [SerializeField] private TMP_Text defeatMessageTMP;
        [SerializeField] private Text defeatTitleText;
        [SerializeField] private Text defeatMessageText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button defeatExitButton;

        [Header("Default Defeat Messages")]
        [SerializeField] private string defaultDefeatTitle = "Tertangkap!";
        [TextArea(2, 4)]
        [SerializeField] private string defaultDefeatMessage = "Pengejar berhasil menangkapmu! Coba lagi dan perhatikan rintangan di depan.";

        public event Action OnStartButtonClicked;
        public event Action OnVictoryContinueClicked;
        public event Action OnRetryButtonClicked;

        private void Awake()
        {
            if (levelManager == null)
            {
                levelManager = FindFirstObjectByType<ChaseLevelManager>();
            }

            SetupButtons();
        }

        private void Start()
        {
            if (levelManager != null)
            {
                levelManager.OnGameStateChanged += HandleGameStateChanged;
            }

            // Display instruction panel at start if state is Ready
            if (levelManager == null || levelManager.CurrentState == ChaseGameState.Ready)
            {
                ShowInstructions(defaultTitle, defaultMessage);
            }
            else
            {
                HideAllPanels();
            }
        }

        private void OnDestroy()
        {
            if (levelManager != null)
            {
                levelManager.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void SetupButtons()
        {
            if (startButton != null) startButton.onClick.AddListener(OnClickStart);
            if (victoryContinueButton != null) victoryContinueButton.onClick.AddListener(OnClickVictoryContinue);
            if (retryButton != null) retryButton.onClick.AddListener(OnClickRetry);
            if (defeatExitButton != null) defeatExitButton.onClick.AddListener(OnClickExit);
        }

        public void ShowInstructions(string title, string message)
        {
            HideAllPanels();

            SetText(instructionTitleTMP, instructionTitleText, title);
            SetText(instructionMessageTMP, instructionMessageText, message);

            if (instructionPanel != null) instructionPanel.SetActive(true);
        }

        public void ShowVictory(string title, string message)
        {
            HideAllPanels();

            SetText(victoryTitleTMP, victoryTitleText, title);
            SetText(victoryMessageTMP, victoryMessageText, message);

            if (victoryPanel != null) victoryPanel.SetActive(true);
        }

        public void ShowDefeat(string title, string message)
        {
            HideAllPanels();

            SetText(defeatTitleTMP, defeatTitleText, title);
            SetText(defeatMessageTMP, defeatMessageText, message);

            if (defeatPanel != null) defeatPanel.SetActive(true);
        }

        public void HideAllPanels()
        {
            if (instructionPanel != null) instructionPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);
        }

        private void HandleGameStateChanged(ChaseGameState newState)
        {
            switch (newState)
            {
                case ChaseGameState.Ready:
                    ShowInstructions(defaultTitle, defaultMessage);
                    break;
                case ChaseGameState.Playing:
                    HideAllPanels();
                    break;
                case ChaseGameState.Victory:
                    ShowVictory(defaultVictoryTitle, defaultVictoryMessage);
                    break;
                case ChaseGameState.Defeat:
                    ShowDefeat(defaultDefeatTitle, defaultDefeatMessage);
                    break;
            }
        }

        private void OnClickStart()
        {
            HideAllPanels();
            OnStartButtonClicked?.Invoke();

            if (levelManager != null)
            {
                levelManager.StartLevel();
            }
        }

        private void OnClickVictoryContinue()
        {
            OnVictoryContinueClicked?.Invoke();
            ReturnToMainScene();
        }

        private void OnClickRetry()
        {
            HideAllPanels();
            OnRetryButtonClicked?.Invoke();

            if (levelManager != null)
            {
                levelManager.StartLevel();
            }
        }

        private void OnClickExit()
        {
            ReturnToMainScene();
        }

        private void ReturnToMainScene()
        {
            if (SceneTransitionWithSave.Instance != null && !string.IsNullOrEmpty(returnSceneName))
            {
                SceneTransitionWithSave.Instance.LoadScene(returnSceneName);
            }
            else if (!string.IsNullOrEmpty(returnSceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(returnSceneName);
            }
        }

        private void SetText(TMP_Text tmpComponent, Text textComponent, string content)
        {
            if (tmpComponent != null) tmpComponent.text = content;
            if (textComponent != null) textComponent.text = content;
        }
    }
}
