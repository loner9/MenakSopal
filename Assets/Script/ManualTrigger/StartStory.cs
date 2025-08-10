using UnityEngine;

public class StartStory : MonoBehaviour
{
    GameObject parentGameObject;
    void Awake()
    {
        GameObject childGameObject = this.gameObject; // Assuming this script is attached to the child
        Transform parentTransform = childGameObject.transform.parent;
        if (parentTransform != null)
        {
            parentGameObject = parentTransform.gameObject;
            Debug.Log("Parent GameObject: " + parentGameObject.name);
        }

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (NPCInteractionSystem.Instance.HasGameFlag("story_started"))
        {
            if (parentGameObject != null)
            {
                parentGameObject.SetActive(false);
            }
        }
        else
        {
            DayNightCycle.Instance.PauseTime();
            DayNightCycle.Instance.SetTime(8);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            NPCInteractionSystem.Instance.AddGameFlag("story_started");
            if (parentGameObject != null)
            {
                parentGameObject.SetActive(false);
            }
        }
    }
}
