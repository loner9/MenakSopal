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
    [Header("Stamina Settings")]
    [SerializeField] private float staminaDrainRate = 55f;
    [SerializeField] private float regenRateStill = 40f;
    [SerializeField] private float regenRateWalking = 20f;
    [SerializeField] private float windedDuration = 1.5f;
    [SerializeField] private float regenSmoothing = 4f;

    private float currentRegenRate = 0f;
    private bool isWinded = false;
    private float windedTimer = 0f;

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

        // 1. Read New Input System (Keyboard / Gamepad)
        Vector2 inputSystemMove = playerActions.Movement.Move.ReadValue<Vector2>();

        // 2. Read Control Freak 2 Touch Joystick / Axes
        float cfX = ControlFreak2.CF2Input.GetAxis("Horizontal");
        float cfY = ControlFreak2.CF2Input.GetAxis("Vertical");
        Vector2 cfMove = new Vector2(cfX, cfY);

        // Combine inputs (prioritize CF2 if touched/moved, otherwise use Input System)
        Vector2 rawMove = cfMove.sqrMagnitude > 0.001f ? cfMove : inputSystemMove;
        moveDirection = rawMove.normalized;

        bool isRunning;

        if (player.Stats.stamina <= 0 || isWinded)
        {
            isRunning = false;
        }
        else
        {
            isRunning = playerActions.Movement.Run.IsPressed() ||
                        (ControlFreak2.CF2Input.activeRig != null && ControlFreak2.CF2Input.activeRig.GetButton("Run")) ||
                        ControlFreak2.CF2Input.GetKey(KeyCode.LeftShift);
        }

        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // --- Action-Sensitive Stamina System ---
        if (isRunning && moveDirection != Vector2.zero)
        {
            // Draining: actively sprinting
            player.Stats.stamina = Mathf.Clamp(
                player.Stats.stamina - (staminaDrainRate * Time.deltaTime),
                0f, player.Stats.maxStamina);

            // Trigger winded state the moment stamina bottoms out
            if (player.Stats.stamina <= 0f && !isWinded)
            {
                isWinded = true;
                windedTimer = 0f;
            }

            currentRegenRate = 0f; // No regen while draining
        }
        else
        {
            // Recovering: determine target regen rate by player state
            float targetRegenRate;

            if (isWinded)
            {
                // Count through the winded lockout before recovering
                windedTimer += Time.deltaTime;
                if (windedTimer >= windedDuration)
                {
                    isWinded = false;
                }
                targetRegenRate = 0f;
            }
            else if (playerAttack != null && playerAttack.IsAttacking)
            {
                targetRegenRate = 0f; // No regen while attacking
            }
            else if (moveDirection != Vector2.zero)
            {
                targetRegenRate = regenRateWalking; // Slow regen while walking
            }
            else
            {
                targetRegenRate = regenRateStill; // Full regen while idle
            }

            // Smooth the regen rate transition
            currentRegenRate = Mathf.Lerp(currentRegenRate, targetRegenRate, regenSmoothing * Time.deltaTime);

            if (player.Stats.stamina < player.Stats.maxStamina)
            {
                player.Stats.stamina = Mathf.Clamp(
                    player.Stats.stamina + (currentRegenRate * Time.deltaTime),
                    0f, player.Stats.maxStamina);
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
