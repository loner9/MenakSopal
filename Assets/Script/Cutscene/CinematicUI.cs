using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenakSopal.Cutscenes
{
    /// <summary>
    /// Handles the cinematic presentation of cutscenes (letterboxing, UI hiding, skip prompts).
    /// </summary>
    public class CinematicUI : MonoBehaviour
    {
        public static CinematicUI Instance { get; private set; }

        [Header("Letterbox")]
        [SerializeField] private RectTransform topBar;
        [SerializeField] private RectTransform bottomBar;
        [SerializeField] private float barHeight = 100f;
        [SerializeField] private float transitionSpeed = 5f;

        [Header("Skip Prompt")]
        [SerializeField] private CanvasGroup skipGroup;
        [SerializeField] private TextMeshProUGUI skipText;

        [Header("Gameplay HUD")]
        [SerializeField] private CanvasGroup gameplayHUD;

        private bool isCinematic = false;
        private Coroutine barCoroutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // Ensure bars start off-screen
            if (topBar) topBar.anchoredPosition = new Vector2(0, barHeight);
            if (bottomBar) bottomBar.anchoredPosition = new Vector2(0, -barHeight);
            if (skipGroup) skipGroup.alpha = 0;

            // Auto-find HUD if not assigned
            if (gameplayHUD == null)
            {
                GameObject hud = GameObject.Find("HUD") ?? GameObject.Find("GameplayUI") ?? GameObject.Find("Canvas_InGame");
                if (hud != null) gameplayHUD = hud.GetComponent<CanvasGroup>();
            }
        }

        public void ShowCinematicMode(bool show, bool canSkip = true)
        {
            isCinematic = show;

            if (barCoroutine != null) StopCoroutine(barCoroutine);
            barCoroutine = StartCoroutine(AnimateCinematicMode(show));

            if (show && canSkip)
            {
                StartCoroutine(FadeSkipPrompt(true));
            }
            else
            {
                StartCoroutine(FadeSkipPrompt(false));
            }

            // Hide/Show Gameplay HUD
            if (gameplayHUD != null)
            {
                LeanTweenFadeHUD(show ? 0 : 1);
            }
        }

        private IEnumerator AnimateCinematicMode(bool show)
        {
            float targetTop = show ? 0 : barHeight;
            float targetBottom = show ? 0 : -barHeight;

            Vector2 topPos = topBar.anchoredPosition;
            Vector2 bottomPos = bottomBar.anchoredPosition;

            while (Mathf.Abs(topPos.y - targetTop) > 0.1f)
            {
                topPos.y = Mathf.Lerp(topPos.y, targetTop, Time.deltaTime * transitionSpeed);
                bottomPos.y = Mathf.Lerp(bottomPos.y, targetBottom, Time.deltaTime * transitionSpeed);

                topBar.anchoredPosition = topPos;
                bottomBar.anchoredPosition = bottomPos;

                yield return null;
            }

            topBar.anchoredPosition = new Vector2(0, targetTop);
            bottomBar.anchoredPosition = new Vector2(0, targetBottom);
        }

        private IEnumerator FadeSkipPrompt(bool show)
        {
            if (!skipGroup) yield break;

            float target = show ? 1 : 0;
            float start = skipGroup.alpha;
            float elapsed = 0;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                skipGroup.alpha = Mathf.Lerp(start, target, elapsed / duration);
                yield return null;
            }
            skipGroup.alpha = target;
        }

        private void LeanTweenFadeHUD(float targetAlpha)
        {
            // Fallback to simple lerp if LeanTween isn't available
            StartCoroutine(SimpleHUDAlpha(targetAlpha));
        }

        private IEnumerator SimpleHUDAlpha(float target)
        {
            if (!gameplayHUD) yield break;

            float start = gameplayHUD.alpha;
            float elapsed = 0;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                gameplayHUD.alpha = Mathf.Lerp(start, target, elapsed / 0.5f);
                yield return null;
            }
            gameplayHUD.alpha = target;
            gameplayHUD.interactable = (target > 0.5f);
            gameplayHUD.blocksRaycasts = (target > 0.5f);
        }
    }
}
