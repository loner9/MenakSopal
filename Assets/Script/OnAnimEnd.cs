using UnityEngine;

public class OnAnimEnd : MonoBehaviour
{
    public bool animationEnded = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void AnimStart()
    {
        animationEnded = false;
    }

    public void AnimEnd()
    {
        animationEnded = true;
    }
}
