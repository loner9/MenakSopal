using System.Collections.Generic;
using UnityEngine;

public class InvisibleObjectHolder : MonoBehaviour
{
    [SerializeField]private List<GameObject> gameObjects;
    public List<GameObject> HiddenObjects => gameObjects;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
