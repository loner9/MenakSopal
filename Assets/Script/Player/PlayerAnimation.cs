using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private readonly int xHash = Animator.StringToHash("x");
    private readonly int yHash = Animator.StringToHash("y");
    private readonly int velocityHash = Animator.StringToHash("velocity");
    private readonly int movingHash = Animator.StringToHash("moving");
    private readonly int runningHash = Animator.StringToHash("running");
    private readonly int attackHash = Animator.StringToHash("attack");
    private readonly int hitHash = Animator.StringToHash("hit");
    private readonly int deadHash = Animator.StringToHash("dead");
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setMovingAnimation(bool isMoving)
    {
        animator.SetBool(movingHash, isMoving);
    }

    public void setMovingAnimation(float x, float y)
    {
        animator.SetFloat(xHash, x);
        animator.SetFloat(yHash, y);
    }

    public void setRunningAnimation(bool isRunning)
    {
        animator.SetBool(runningHash, isRunning);
    }

    public void setRunningAnimation(float x, float y)
    {
        animator.SetFloat(xHash, x);
        animator.SetFloat(yHash, y);
    }

    public void setAttackAnimation()
    {
        animator.SetTrigger(attackHash);
    }

    public void showDeadAnimation()
    {
        animator.SetTrigger(deadHash);
    }

    public void showHitAnimation()
    {
        animator.SetTrigger(hitHash);
    }


}
