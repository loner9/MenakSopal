using UnityEngine;

public class EnemyAggroCheck : MonoBehaviour
{
    public GameObject playerTarget {get; set;}
    private Enemy _enemy;
    [SerializeField] private bool debugMode = false;
    
    void Awake()
    {
        // Try to find player by tag first
        playerTarget = GameObject.FindGameObjectWithTag("Player");
        
        // If not found, try alternative methods
        if (playerTarget == null)
        {
            // Look for PlayerMovements component
            PlayerMovements playerMovements = FindFirstObjectByType<PlayerMovements>();
            if (playerMovements != null)
            {
                playerTarget = playerMovements.gameObject;
                Debug.LogWarning($"EnemyAggroCheck: Player tag not found, using PlayerMovements GameObject: {playerTarget.name}");
            }
        }

        _enemy = GetComponentInParent<Enemy>();
        
        if (playerTarget == null)
        {
            Debug.LogError("EnemyAggroCheck: Could not find player target!");
        }
        if (_enemy == null)
        {
            Debug.LogError("EnemyAggroCheck: Could not find Enemy component in parent!");
        }
        
        if (debugMode)
        {
            Debug.Log($"EnemyAggroCheck initialized for {gameObject.name}, Player: {(playerTarget ? playerTarget.name : "NULL")}");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerTarget != null && collision.gameObject == playerTarget)
        {
            if (debugMode) Debug.Log($"{gameObject.name}: Player entered aggro range");
            _enemy?.setAggroStatus(true);
        } 
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (playerTarget != null && collision.gameObject == playerTarget)
        {
            if (debugMode) Debug.Log($"{gameObject.name}: Player exited aggro range");
            _enemy?.setAggroStatus(false);
        }
    }
}
