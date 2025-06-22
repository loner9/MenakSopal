using UnityEngine;

public class EnemyStrikingDistanceCheck : MonoBehaviour
{
    public GameObject playerTarget { get; set; }
    private GameObject attackBox { get; set; }
    private GameObject parent { get; set; }
    private Enemy _enemy;
     private CapsuleCollider2D attackBoxCollider;
    [SerializeField] private float offset = 1f;
    [SerializeField] private Vector2 horizontalColliderSize = new Vector2(1.5f, 0.5f); // wider than tall
    [SerializeField] private Vector2 verticalColliderSize = new Vector2(0.5f, 1.5f);   // taller than wide

    void Awake()
    {
        // Try to find PlayerHBox by tag first
        playerTarget = GameObject.FindGameObjectWithTag("PlayerHBox");
        
        // If PlayerHBox not found, try to find Player tag or PlayerMovements
        if (playerTarget == null)
        {
            playerTarget = GameObject.FindGameObjectWithTag("Player");
            if (playerTarget == null)
            {
                PlayerMovements playerMovements = FindFirstObjectByType<PlayerMovements>();
                if (playerMovements != null)
                {
                    playerTarget = playerMovements.gameObject;
                    Debug.LogWarning($"EnemyStrikingDistanceCheck: PlayerHBox tag not found, using Player GameObject: {playerTarget.name}");
                }
            }
            else
            {
                Debug.LogWarning($"EnemyStrikingDistanceCheck: PlayerHBox tag not found, using Player tag: {playerTarget.name}");
            }
        }
        
        attackBox = GameObject.Find("AttackBox");
        parent = transform.parent.parent.gameObject;
        Debug.Log($"EnemyStrikingDistanceCheck Parent: {parent.name}");

        _enemy = GetComponentInParent<Enemy>();
        
        if (attackBox != null)
        {
            attackBoxCollider = attackBox.GetComponent<CapsuleCollider2D>();
        }
        else
        {
            Debug.LogError("EnemyStrikingDistanceCheck: AttackBox not found!");
        }
        
        if (playerTarget == null)
        {
            Debug.LogError("EnemyStrikingDistanceCheck: Could not find player target!");
        }
        if (_enemy == null)
        {
            Debug.LogError("EnemyStrikingDistanceCheck: Could not find Enemy component in parent!");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == playerTarget)
        {
            _enemy.setAttackRangeStatus(true);
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject == playerTarget)
        {
            // Fix: Calculate direction from parent to player (not attackBox to player)
            Vector2 direction = playerTarget.transform.position - parent.transform.position;
            direction.Normalize();
            
            Vector2 parentSize = Vector2.zero;
            SpriteRenderer parentSprite = parent.GetComponent<SpriteRenderer>();
            if (parentSprite != null)
            {
                parentSize = parentSprite.bounds.size;
            }
            else
            {
                Collider2D parentCollider = parent.GetComponent<Collider2D>();
                if (parentCollider != null)
                {
                    parentSize = parentCollider.bounds.size;
                }
            }

            // Fix: Corrected variable names from abxX/abxY to absX/absY
            float absX = Mathf.Abs(direction.x);
            float absY = Mathf.Abs(direction.y);

            Vector3 newPosition = parent.transform.position;

           // Determine if we're dealing with horizontal or vertical positioning
            bool isHorizontalDirection = absX > absY;

            if (isHorizontalDirection)
            {
                // Horizontal direction (left/right)
                float halfWidth = parentSize.x / 2f;
                if (direction.x > 0)
                {
                    // Player is to the right of parent
                    newPosition.x = parent.transform.position.x + halfWidth + offset;
                }
                else
                {
                    // Player is to the left of parent
                    newPosition.x = parent.transform.position.x - halfWidth - offset;
                }
                newPosition.y = parent.transform.position.y;
                
                // Set capsule collider to vertical orientation
                if (attackBoxCollider != null)
                {
                    // CapsuleDirection2D.Vertical = 0
                    attackBoxCollider.direction = CapsuleDirection2D.Vertical;
                    attackBoxCollider.size = verticalColliderSize;
                }
            }
            else
            {
                // Vertical direction (top/bottom)
                float halfHeight = parentSize.y / 2f;
                if (direction.y > 0)
                {
                    // Player is above parent
                    newPosition.y = parent.transform.position.y + halfHeight + offset;
                }
                else
                {
                    // Player is below parent
                    newPosition.y = parent.transform.position.y - halfHeight - offset;
                }
                newPosition.x = parent.transform.position.x;
                
                // Set capsule collider to horizontal orientation
                if (attackBoxCollider != null)
                {
                    // CapsuleDirection2D.Horizontal = 1
                    attackBoxCollider.direction = CapsuleDirection2D.Horizontal;
                    attackBoxCollider.size = horizontalColliderSize;
                }
            }

            attackBox.transform.position = newPosition;
            
            // Add visual debugging to see the direction
            // Debug.DrawRay(parent.transform.position, direction * 2f, Color.red);
            // Debug.Log("Direction: " + direction + " | New Position: " + newPosition);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == playerTarget)
        {
            _enemy.setAttackRangeStatus(false);
        }
    }
}
