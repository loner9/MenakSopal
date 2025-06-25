using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float maxAttackDuration = 1.0f; // Fallback auto-reset
    [SerializeField] private GameObject attackBox;
    
    [Header("Attack Box Positioning")]
    [SerializeField] private float attackBoxDistance = 1.5f;
    [SerializeField] private Vector2 attackBoxOffset = Vector2.zero;
    
    private PlayerActions playerActions;
    private PlayerAnimation playerAnimation;
    private Player player;
    private bool isAttacking = false;
    private float cooldownTimer = 0f;
    private float attackDurationTimer = 0f;
    private Vector2 lastFacingDirection = Vector2.down; // Default facing down

    private void Awake()
    {
        playerActions = new PlayerActions();
        playerAnimation = GetComponent<PlayerAnimation>();
        player = GetComponent<Player>();
    }

    private void Start()
    {
        if (attackBox == null)
        {
            CreateAttackBox();
        }
        else
        {
            attackBox.SetActive(false);
        }
    }

    private void Update()
    {
        HandleAttackInput();
        UpdateCooldownTimer();
    }

    private void HandleAttackInput()
    {
        if (player.Stats.health <= 0) return;

        if (playerActions.Movement.Attack.WasPressedThisFrame() && !isAttacking && cooldownTimer <= 0f)
        {
            PerformAttack();
        }
    }

    private void UpdateCooldownTimer()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // Fallback auto-reset if attack state gets stuck
        if (isAttacking)
        {
            attackDurationTimer -= Time.deltaTime;
            if (attackDurationTimer <= 0f)
            {
                Debug.LogWarning("Attack auto-reset triggered - animation event may be missing");
                EndAttack();
            }
        }
    }

    private void PerformAttack()
    {
        isAttacking = true;
        cooldownTimer = attackCooldown;
        attackDurationTimer = maxAttackDuration; // Start fallback timer

        Debug.Log("Player performing attack!");

        if (playerAnimation != null)
        {
            playerAnimation.setAttackAnimation();
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
        Debug.Log("Player attack ended");
    }

    // Methods to be called from animation events
    public void EnableAttackBox()
    {
        if (attackBox != null)
        {
            // Update position based on current facing direction before enabling
            UpdateAttackBoxPosition();
            attackBox.SetActive(true);
            Debug.Log("Attack box enabled");
        }
    }

    public void DisableAttackBox()
    {
        if (attackBox != null)
        {
            attackBox.SetActive(false);
            Debug.Log("Attack box disabled");
        }
    }

    // Method to be called from PlayerMovements to update facing direction
    public void UpdateFacingDirection(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            lastFacingDirection = direction.normalized;
        }
    }

    private void UpdateAttackBoxPosition()
    {
        if (attackBox != null)
        {
            Vector3 attackPosition = transform.position + (Vector3)(lastFacingDirection * attackBoxDistance) + (Vector3)attackBoxOffset;
            attackBox.transform.position = attackPosition;
        }
    }

    private void CreateAttackBox()
    {
        attackBox = new GameObject("PlayerAttackBox");
        attackBox.transform.SetParent(transform);
        
        // Set initial position based on facing direction
        UpdateAttackBoxPosition();

        // Add collider for attack detection
        CircleCollider2D attackCollider = attackBox.AddComponent<CircleCollider2D>();
        attackCollider.isTrigger = true;
        attackCollider.radius = 1.5f;

        // Add attack box handler
        AttackBoxHandler attackHandler = attackBox.AddComponent<AttackBoxHandler>();

        // Initially disable the attack box
        attackBox.SetActive(false);
    }

    public bool IsAttacking => isAttacking;

    public void SetAttackBox(GameObject newAttackBox)
    {
        if (attackBox != null && attackBox != newAttackBox)
        {
            attackBox.SetActive(false);
        }
        attackBox = newAttackBox;
        if (attackBox != null)
        {
            attackBox.SetActive(false);
        }
    }

    private void OnEnable()
    {
        playerActions.Enable();
    }

    private void OnDisable()
    {
        playerActions.Disable();
    }
}