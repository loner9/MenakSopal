using UnityEngine;

public class FlagEventTester : MonoBehaviour
{
    void OnEnable()
    {
        FlagManager.OnFlagAdded += OnFlagAdded;
        FlagManager.OnFlagRemoved += OnFlagRemoved;
    }
    
    void OnDisable()
    {
        FlagManager.OnFlagAdded -= OnFlagAdded;
        FlagManager.OnFlagRemoved -= OnFlagRemoved;
    }
    
    void OnFlagAdded(string flag)
    {
        Debug.Log($"[EVENT] Flag added: {flag}");
    }
    
    void OnFlagRemoved(string flag)
    {
        Debug.Log($"[EVENT] Flag removed: {flag}");
    }
}