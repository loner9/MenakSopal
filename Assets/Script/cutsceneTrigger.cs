using MenakSopal.Cutscenes;
using UnityEngine;

public class cutsceneTrigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (NPCInteractionSystem.Instance.HasGameFlag("kegaduhan"))
        {
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        CutsceneController.Instance.PlayCutscene("sumurCutscene");
        Destroy(this);
    }
}
