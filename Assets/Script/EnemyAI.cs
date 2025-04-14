using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private enum State { Patrol, Chase, Attack }
    [SerializeField] private State currentState = State.Patrol;

    [Header("Patrol Settings")]
    public GameObject[] waypoints;
    public float waypointReachedThreshold = 1f;
    public float waitTime = 2f;
    private int currentWaypointIndex = 0;
    private Coroutine waitCoroutine;
    private Coroutine attackCoroutine;

    [Header("Detection")]
    public FieldOfViewDetector detector;
    private Transform chaseTarget;

    private NavMeshAgent agent;
    public float maxCombatRange = 2f;
    public float attackCooldown = 1f;
    Animator animator;
    private bool isAttacking = false;
    private const string ANIMATOR_MOVE_X = "X";
    private const string ANIMATOR_MOVE_Y = "Y";
    private const string ANIMATOR_IS_MOVING = "isMoving";
    [SerializeField] private float agentSpeed;
    private float currentSpeed;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        if (detector == null)
        {
            detector = GetComponent<FieldOfViewDetector>();
        }

        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
        }
        currentSpeed = agentSpeed;
        if (agent != null)
        {
            agent.speed = currentSpeed;
        }
    }

    void Update()
    {
        agent.speed = agentSpeed;

        UpdateAnimationParameters();
        bool targetInView = detector.FindTargetInView();

        if (targetInView && detector.TargetsInView.Count > 0)
        {
            chaseTarget = detector.TargetsInView[0];
            float distanceToTarget = Vector3.Distance(transform.position, chaseTarget.position);
            currentState = distanceToTarget <= maxCombatRange ? State.Attack : State.Chase;
        }
        else if (currentState != State.Patrol)
        {
            currentState = State.Patrol;
            chaseTarget = null;
            if (waypoints.Length > 0)
            {
                agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
            }
        }

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
        }
    }

    void UpdateAnimationParameters()
    {
        if (animator == null) return;
        Debug.Log("agent.velocity: " + agent.velocity);
        Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
        animator.SetFloat(ANIMATOR_MOVE_X, localVelocity.x);
        animator.SetFloat(ANIMATOR_MOVE_Y, localVelocity.y);
    }

    void Patrol()
    {
        isAttacking = false;
        if (waypoints.Length == 0) return;
        if (!agent.pathPending && agent.remainingDistance < waypointReachedThreshold)
        {
            if (waitCoroutine == null)
            {
                waitCoroutine = StartCoroutine(WaitAtWaypoint());
            }
        }
    }

    private IEnumerator WaitAtWaypoint()
    {
        yield return new WaitForSeconds(waitTime);
        if (currentState == State.Patrol)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
        }
        waitCoroutine = null;
    }

    void Chase()
    {
        // agent.isStopped = false;
        agent.speed = currentSpeed;
        isAttacking = false;
        if (chaseTarget != null)
        {
            agent.SetDestination(chaseTarget.position);
            if (Vector3.Distance(transform.position, chaseTarget.position) <= maxCombatRange)
            {
                currentState = State.Attack;
            }
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        attackCoroutine = null;
    }

    void Attack()
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
        // agent.isStopped = true;
        agent.speed = 0.05f;
        agent.SetDestination(chaseTarget.position);
        if (chaseTarget != null && Vector3.Distance(transform.position, chaseTarget.position) > maxCombatRange)
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            currentState = State.Chase;
            return;
        }
        if (attackCoroutine == null && !isAttacking)
        {
            attackCoroutine = StartCoroutine(PerformAttack());
        }
    }
}
