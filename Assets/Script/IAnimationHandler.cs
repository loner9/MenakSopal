using UnityEngine;
using UnityEngine.Accessibility;

public interface IAnimationHandler
{
    Animator animator { get; set; }

    void SetFloat(string name, float value);
    void SetBool(string name, bool value);
    void SetTrigger(string name);
}
