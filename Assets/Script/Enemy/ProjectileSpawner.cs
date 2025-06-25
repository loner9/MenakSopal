using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileSpawner : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform[] firePoints;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileDamage = 20f;
    
    [Header("Firing Pattern")]
    [SerializeField] private FiringPattern firingPattern = FiringPattern.Single;
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private float spreadAngle = 15f;
    [SerializeField] private float burstDelay = 0.1f;
    
    [Header("Pooling (Optional)")]
    [SerializeField] private bool usePooling = false;
    [SerializeField] private int poolSize = 10;
    
    private Queue<GameObject> projectilePool;
    private GameObject player;
    
    public enum FiringPattern
    {
        Single,      // Fire one projectile
        Burst,       // Fire multiple projectiles in sequence
        Spread,      // Fire multiple projectiles simultaneously with spread
        MultiPoint   // Fire from multiple fire points
    }
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        // Initialize projectile pool if enabled
        if (usePooling)
        {
            InitializePool();
        }
        
        // Validate fire points
        if (firePoints == null || firePoints.Length == 0)
        {
            // Create default fire point
            GameObject firePointGO = new GameObject("FirePoint");
            firePointGO.transform.SetParent(transform);
            firePointGO.transform.localPosition = Vector3.zero;
            firePoints = new Transform[] { firePointGO.transform };
        }
    }
    
    private void InitializePool()
    {
        projectilePool = new Queue<GameObject>();
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject pooledProjectile = Instantiate(projectilePrefab);
            pooledProjectile.SetActive(false);
            projectilePool.Enqueue(pooledProjectile);
        }
    }
    
    public void FireAtTarget(Vector3 targetPosition)
    {
        Vector2 direction = (targetPosition - transform.position).normalized;
        FireInDirection(direction);
    }
    
    public void FireAtPlayer()
    {
        if (player != null)
        {
            FireAtTarget(player.transform.position);
        }
    }
    
    public void FireInDirection(Vector2 direction)
    {
        switch (firingPattern)
        {
            case FiringPattern.Single:
                FireSingleProjectile(direction, firePoints[0]);
                break;
                
            case FiringPattern.Burst:
                StartCoroutine(FireBurst(direction));
                break;
                
            case FiringPattern.Spread:
                FireSpread(direction);
                break;
                
            case FiringPattern.MultiPoint:
                FireMultiPoint(direction);
                break;
        }
    }
    
    private void FireSingleProjectile(Vector2 direction, Transform firePoint)
    {
        GameObject projectile = GetProjectile();
        if (projectile != null)
        {
            projectile.transform.position = firePoint.position;
            projectile.transform.rotation = Quaternion.identity;
            projectile.SetActive(true);
            
            // Initialize projectile
            EnemyProjectile projectileScript = projectile.GetComponent<EnemyProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(direction, projectileSpeed, projectileDamage);
            }
        }
    }
    
    private IEnumerator FireBurst(Vector2 direction)
    {
        for (int i = 0; i < projectileCount; i++)
        {
            FireSingleProjectile(direction, firePoints[0]);
            
            if (i < projectileCount - 1) // Don't wait after last projectile
            {
                yield return new WaitForSeconds(burstDelay);
            }
        }
    }
    
    private void FireSpread(Vector2 direction)
    {
        float angleStep = spreadAngle / (projectileCount - 1);
        float startAngle = -spreadAngle / 2f;
        
        for (int i = 0; i < projectileCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 spreadDirection = RotateVector(direction, currentAngle);
            
            FireSingleProjectile(spreadDirection, firePoints[0]);
        }
    }
    
    private void FireMultiPoint(Vector2 direction)
    {
        foreach (Transform firePoint in firePoints)
        {
            if (firePoint != null)
            {
                FireSingleProjectile(direction, firePoint);
            }
        }
    }
    
    private Vector2 RotateVector(Vector2 vector, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        
        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }
    
    private GameObject GetProjectile()
    {
        if (usePooling && projectilePool.Count > 0)
        {
            return projectilePool.Dequeue();
        }
        else
        {
            return Instantiate(projectilePrefab);
        }
    }
    
    public void ReturnProjectileToPool(GameObject projectile)
    {
        if (usePooling && projectilePool != null)
        {
            projectile.SetActive(false);
            projectilePool.Enqueue(projectile);
        }
        else
        {
            Destroy(projectile);
        }
    }
    
    // Public methods for external configuration
    public void SetProjectileSettings(float speed, float damage)
    {
        projectileSpeed = speed;
        projectileDamage = damage;
    }
    
    public void SetFiringPattern(FiringPattern pattern, int count = 1, float spread = 15f)
    {
        firingPattern = pattern;
        projectileCount = count;
        spreadAngle = spread;
    }
    
    public void AddFirePoint(Transform firePoint)
    {
        if (firePoint != null)
        {
            List<Transform> firePointList = new List<Transform>(firePoints);
            firePointList.Add(firePoint);
            firePoints = firePointList.ToArray();
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw fire points
        if (firePoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform firePoint in firePoints)
            {
                if (firePoint != null)
                {
                    Gizmos.DrawWireSphere(firePoint.position, 0.15f);
                    Gizmos.DrawRay(firePoint.position, firePoint.right * 1f);
                }
            }
        }
        
        // Draw spread angle visualization
        if (firingPattern == FiringPattern.Spread && projectileCount > 1)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = transform.position;
            Vector3 forward = transform.right;
            
            float halfSpread = spreadAngle / 2f;
            Vector3 leftBound = RotateVector(forward, -halfSpread);
            Vector3 rightBound = RotateVector(forward, halfSpread);
            
            Gizmos.DrawRay(center, leftBound * 2f);
            Gizmos.DrawRay(center, rightBound * 2f);
        }
    }
    
    // Public getters
    public GameObject ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileDamage => projectileDamage;
    public FiringPattern CurrentFiringPattern => firingPattern;
}