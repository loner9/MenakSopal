using Unity.Behavior;
using UnityEngine;

public class WayPointFiller : MonoBehaviour
{
    BehaviorGraphAgent agent;
    GameObject[] wayPoints;

    void Awake()
    {
        agent = GetComponent<BehaviorGraphAgent>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // agent.SetVariableValue<GameObject>("Agent", gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
