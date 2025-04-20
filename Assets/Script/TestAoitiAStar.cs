using System.Collections.Generic;
using Aoiti.Pathfinding;
using UnityEngine;

public class TestAoitiAStar : MonoBehaviour
{
    Pathfinder<Vector2> pathfinder;
    [SerializeField] float detectRadius = 1f;
    [SerializeField] LayerMask obstacles;
    [SerializeField] float speed = 2.5f;
    [SerializeField] float gridSize = 0.5f;
    [SerializeField] bool searchShortcut = false;
    [SerializeField] bool snapToGrid = false;
    List<Vector2> path;
    List<Vector2> pathLeftToGo = new List<Vector2>();
    bool shouldMove = false;
    [SerializeField] Transform initialPosition;
    [SerializeField] float stoppingDistance = 0.5f;
    Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        pathfinder = new Pathfinder<Vector2>(GetDistance, GetNeighbourNodes, 1000);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Set z to 0 for 2D

        float distance = Vector3.Distance(transform.position, mousePosition);
        if (distance < detectRadius)
        {
            GetMoveCommand(mousePosition);
            shouldMove = true;
        }
        else
        {
            GetMoveCommand(initialPosition.position);
            shouldMove = false;
        }

        if (pathLeftToGo.Count > 0)
        {
            Vector3 dir = (Vector3)pathLeftToGo[0] - transform.position;
            Vector2 newPosition = rb.position + (Vector2)(dir.normalized * speed);

            // rb.MovePosition(Vector3.MoveTowards(transform.position, newPosition, Time.deltaTime * speed)); // Move the Rigidbody2D to the new position
            rb.MovePosition(newPosition); // Move the Rigidbody2D to the new position

            if (((Vector2)transform.position - pathLeftToGo[0]).sqrMagnitude < speed * speed)
            {
                rb.MovePosition(pathLeftToGo[0]);
                pathLeftToGo.RemoveAt(0);
            }
        }

        for (int i = 0; i < pathLeftToGo.Count - 1; i++) //visualize your path in the sceneview
        {
            Debug.DrawLine(pathLeftToGo[i], pathLeftToGo[i + 1]);
        }

    }

    void GetMoveCommand(Vector2 target)
    {
        Vector2 closestNode = GetClosestNode(transform.position);

        // Adjust the target position to account for the stopping distance
        Vector2 directionToTarget = (target - (Vector2)transform.position).normalized;
        target -= directionToTarget * stoppingDistance;

        if (pathfinder.GenerateAstarPath(closestNode, GetClosestNode(target), out path))
        {
            if (searchShortcut && path.Count > 0)
                pathLeftToGo = ShortenPath(path);
            else
            {
                pathLeftToGo = new List<Vector2>(path);
                if (!snapToGrid) pathLeftToGo.Add(target);
            }
        }
    }

    float GetDistance(Vector2 A, Vector2 B)
    {
        return (A - B).sqrMagnitude; //Uses square magnitude to lessen the CPU time.
    }

    Vector2 GetClosestNode(Vector2 target)
    {
        return new Vector2(Mathf.Round(target.x / gridSize) * gridSize, Mathf.Round(target.y / gridSize) * gridSize);
    }

    Dictionary<Vector2, float> GetNeighbourNodes(Vector2 pos)
    {
        Dictionary<Vector2, float> neighbours = new Dictionary<Vector2, float>();
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0) continue;

                Vector2 dir = new Vector2(i, j) * gridSize;
                if (!Physics2D.Linecast(pos, pos + dir, obstacles))
                {
                    neighbours.Add(GetClosestNode(pos + dir), dir.magnitude);
                }
            }

        }
        return neighbours;
    }

    List<Vector2> ShortenPath(List<Vector2> path)
    {
        List<Vector2> newPath = new List<Vector2>();

        for (int i = 0; i < path.Count; i++)
        {
            newPath.Add(path[i]);
            for (int j = path.Count - 1; j > i; j--)
            {
                if (!Physics2D.Linecast(path[i], path[j], obstacles))
                {

                    i = j;
                    break;
                }
            }
            newPath.Add(path[i]);
        }
        newPath.Add(path[path.Count - 1]);
        return newPath;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}
