using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerMovements : MonoBehaviour, IKnockbackable
{
    [Header("Config")]
    [SerializeField]
    public float walkSpeed;
    public float runSpeed;

    private float currentSpeed;

    private PlayerActions playerActions;
    private Rigidbody2D rb2d;
    private Vector2 moveDirection;
    private float lastX;
    private float lastY;
    private Vector2 lastCardinalDirection = Vector2.down; // Store the actual cardinal direction
    private float StaminaRegenTimer = 0.0f;
    private const float StaminaDecreasePerFrame = 55.0f;
    private const float StaminaIncreasePerFrame = 25.0f;
    private float StaminaTimeToRegen = 3.0f;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackResistance = 1f;

    PlayerAnimation playerAnimation;
    Player player;
    private KnockbackHandler knockbackHandler;
    private PlayerAttack playerAttack;


    private void Awake()
    {
        player = GetComponent<Player>();
        playerActions = new PlayerActions();
        rb2d = GetComponent<Rigidbody2D>();
        playerAnimation = GetComponent<PlayerAnimation>();
        playerAttack = GetComponent<PlayerAttack>();
        
        // Initialize or add KnockbackHandler component
        knockbackHandler = GetComponent<KnockbackHandler>();
        if (knockbackHandler == null)
        {
            knockbackHandler = gameObject.AddComponent<KnockbackHandler>();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        lastX = 1;
        lastY = -1; // Default facing down
        currentSpeed = walkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        ReadMovement();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if (player.Stats.health <= 0)
        {
            return;
        }
        
        // If player is being knocked back, don't apply normal movement
        if (knockbackHandler != null && knockbackHandler.IsKnockedBack)
        {
            // KnockbackHandler will handle the movement
            return;
        }
        
        rb2d.MovePosition(rb2d.position + moveDirection * (currentSpeed * Time.fixedDeltaTime));
    }

    private void ReadMovement()
    {
        // Don't read input if being knocked back or attacking
        if ((knockbackHandler != null && knockbackHandler.IsKnockedBack) || 
            (playerAttack != null && playerAttack.IsAttacking))
        {
            moveDirection = Vector2.zero;
            playerAnimation.setMovingAnimation(false);
            return;
        }
        
        moveDirection = playerActions.Movement.Move.ReadValue<Vector2>().normalized;

        bool isRunning;
        
        if (player.Stats.stamina <= 0)
        {
            isRunning = false;
        }else
        {
            isRunning = playerActions.Movement.Run.IsPressed();
        }

        currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (isRunning && moveDirection != Vector2.zero)
        {
            player.Stats.stamina = Mathf.Clamp(player.Stats.stamina - (StaminaDecreasePerFrame * Time.unscaledDeltaTime), 0.0f, player.Stats.maxStamina);
            StaminaRegenTimer = 0.0f;
        }
        else if (player.Stats.stamina < player.Stats.maxStamina)
        {
            if (StaminaRegenTimer >= StaminaTimeToRegen)
            {
                player.Stats.stamina = Mathf.Clamp(player.Stats.stamina + (StaminaIncreasePerFrame * Time.unscaledDeltaTime), 0.0f, player.Stats.maxStamina);
            }
            else
            {
                StaminaRegenTimer += Time.deltaTime;
            }
        }

        if (moveDirection.x != 0)
        {
            if (player.Stats.health <= 0)
            {
                return;
            }
            lastX = moveDirection.x;
        }

        if (moveDirection.y != 0)
        {
            if (player.Stats.health <= 0)
            {
                return;
            }
            lastY = moveDirection.y;
        }

        // Update attack system with facing direction (cardinal directions only)
        if (playerAttack != null)
        {
            if (moveDirection != Vector2.zero)
            {
                // Calculate and store the cardinal direction when moving
                lastCardinalDirection = GetCardinalDirection(moveDirection);
                playerAttack.UpdateFacingDirection(lastCardinalDirection);
            }
            else
            {
                // Use stored cardinal direction when not moving (no reprocessing)
                playerAttack.UpdateFacingDirection(lastCardinalDirection);
            }
        }

        if (moveDirection == Vector2.zero)
        {
            playerAnimation.setMovingAnimation(false);
            return;
        }

        playerAnimation.setMovingAnimation(true);
        playerAnimation.setMovingAnimation(moveDirection.x, moveDirection.y);
        playerAnimation.setRunningAnimation(isRunning);
    }

    private void OnEnable()
    {
        playerActions.Enable();
    }

    private void OnDisable()
    {
        playerActions.Disable();
    }

    #region IKnockbackable Implementation

    public bool IsKnockedBack => knockbackHandler != null && knockbackHandler.IsKnockedBack;
    
    public float KnockbackResistance => knockbackResistance;

    public void ApplyKnockback(Vector2 knockbackDirection, float knockbackForce, float duration)
    {
        if (knockbackHandler == null) return;

        // Apply knockback through handler
        knockbackHandler.ApplyKnockback(knockbackDirection, knockbackForce, duration);
        
        // Update player animation to show knockback direction
        if (knockbackDirection.sqrMagnitude > 0.1f)
        {
            playerAnimation.setMovingAnimation(knockbackDirection.x, knockbackDirection.y);
        }
    }

    #endregion

    private Vector2 GetCardinalDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return Vector2.down; // Default fallback
        
        // Get absolute values to determine which axis is stronger
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);
        
        // Special case: prioritize horizontal when moving down diagonally
        if (direction.y < 0 && absX > 0)
        {
            return direction.x > 0 ? Vector2.right : Vector2.left;
        }
        
        // Normal prioritization for other directions
        if (absX > absY)
        {
            // Horizontal movement is stronger
            return direction.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            // Vertical movement is stronger (or equal)
            return direction.y > 0 ? Vector2.up : Vector2.down;
        }
    }
}
