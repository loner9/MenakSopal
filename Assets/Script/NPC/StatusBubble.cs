using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class StatusBubble : MonoBehaviour
{
    [Header("Bubble Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;
    public Vector3 bobAmount = new Vector3(0, 0.1f, 0);
    public float bobSpeed = 2f;
    
    [Header("Scale Animation")]
    public bool useScaleAnimation = true;
    public float scaleInDuration = 0.2f;
    public Vector3 targetScale = Vector3.one;
    
    private Image bubbleImage;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private bool isVisible = false;
    private Coroutine animationCoroutine;
    
    void Awake()
    {
        bubbleImage = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        originalPosition = transform.localPosition;
        originalScale = transform.localScale;
        
        // Start invisible
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (isVisible)
        {
            // Gentle bobbing animation
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount.y;
            transform.localPosition = originalPosition + new Vector3(0, bobOffset, 0);
        }
    }
    
    public void ShowBubble(Sprite sprite)
    {
        if (sprite == null) return;
        
        bubbleImage.sprite = sprite;
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        animationCoroutine = StartCoroutine(ShowAnimation());
    }
    
    public void HideBubble()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        animationCoroutine = StartCoroutine(HideAnimation());
    }
    
    private System.Collections.IEnumerator ShowAnimation()
    {
        gameObject.SetActive(true);
        isVisible = true;
        
        float elapsedTime = 0f;
        
        // Reset position and scale
        transform.localPosition = originalPosition;
        
        if (useScaleAnimation)
        {
            transform.localScale = Vector3.zero;
        }
        
        // Animate in
        while (elapsedTime < Mathf.Max(fadeInDuration, scaleInDuration))
        {
            elapsedTime += Time.deltaTime;
            
            // Fade in
            float fadeProgress = Mathf.Clamp01(elapsedTime / fadeInDuration);
            canvasGroup.alpha = fadeProgress;
            
            // Scale in
            if (useScaleAnimation)
            {
                float scaleProgress = Mathf.Clamp01(elapsedTime / scaleInDuration);
                // Use ease out for nice bounce effect
                scaleProgress = 1f - Mathf.Pow(1f - scaleProgress, 3f);
                transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, scaleProgress);
            }
            
            yield return null;
        }
        
        // Ensure final values
        canvasGroup.alpha = 1f;
        if (useScaleAnimation)
        {
            transform.localScale = targetScale;
        }
        
        animationCoroutine = null;
    }
    
    private System.Collections.IEnumerator HideAnimation()
    {
        isVisible = false;
        
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        Vector3 startScale = transform.localScale;
        
        // Animate out
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeOutDuration;
            
            // Fade out
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            
            // Scale out
            if (useScaleAnimation)
            {
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            }
            
            yield return null;
        }
        
        // Ensure final values
        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
        
        animationCoroutine = null;
    }
    
    public void SetBubbleSprite(Sprite sprite)
    {
        if (bubbleImage != null)
        {
            bubbleImage.sprite = sprite;
        }
    }
    
    public bool IsVisible()
    {
        return isVisible;
    }
    
    public void SetBobAmount(Vector3 amount)
    {
        bobAmount = amount;
    }
    
    public void SetBobSpeed(float speed)
    {
        bobSpeed = speed;
    }
    
    void OnDestroy()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
    }
}