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
        if (collision.CompareTag("Player"))
        {
            if (DayNightCycle.Instance.CurrentTime >= 7f && DayNightCycle.Instance.CurrentTime <= 19f)
            {
                CutsceneController.Instance.PlayCutscene("sumurCutscene");
                Destroy(this);
            }
        }
    }
}
