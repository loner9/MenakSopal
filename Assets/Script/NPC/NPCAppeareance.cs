using UnityEngine;

public class NPCAppearance : MonoBehaviour
{
    [Header("Appearance")]
    [Tooltip("Drag and drop any sprite from your sprite sheets here")]
    public Sprite npcSprite;
    
    [Header("Components")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    
    [Header("Animation Settings")]
    public bool enableMovementAnimation = true;
    
    // Private members
    private Vector2 previousPosition;
    
    void Awake()
    {
        // Get components if not assigned
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
            
        if (animator == null)
            animator = GetComponent<Animator>();
        
        previousPosition = transform.position;
        
        // Apply the selected sprite
        ApplySprite();
        
        // Set initial animation state
        if (animator != null)
        {
            animator.SetFloat("speed", 0);
            animator.SetInteger("orientation", 4); // Default facing down
        }
    }
    
    void Update()
    {
        if (enableMovementAnimation)
        {
            // UpdateMovementAnimation();
        }
    }
    
    void OnValidate()
    {
        // Apply sprite changes in editor
        if (Application.isPlaying)
        {
            ApplySprite();
        }
    }
    
    private void ApplySprite()
    {
        if (spriteRenderer != null && npcSprite != null)
        {
            spriteRenderer.sprite = npcSprite;
        }
    }
    
    // Public methods for external control
    public void ChangeSprite(Sprite newSprite)
    {
        npcSprite = newSprite;
        ApplySprite();
    }
    
    public void SetOrientation(Vector2 direction)
    {
        if (animator == null) return;
        
        if (direction.x > 0.01f)
            animator.SetInteger("orientation", 6); // Right
        else if (direction.x < -0.01f)
            animator.SetInteger("orientation", 2); // Left
        else if (direction.y > 0.01f)
            animator.SetInteger("orientation", 0); // Up
        else if (direction.y < -0.01f)
            animator.SetInteger("orientation", 4); // Down
    }
}