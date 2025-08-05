using System;
using UnityEngine;

public class MovePlayerTo : MonoBehaviour
{
    GameObject player;
    Transform defaultTarget;
    [SerializeField] Transform target;
    [SerializeField] float desiredTime = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("House");
        foreach (GameObject p in targets)
        {
            if (p.name == "MCHome")
            {
                target = p.transform;
                break;
            }
        }
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            if (p.name == "PlayerCok")
            {
                player = p;
                break;
            }
        }

        if (player != null)
        {
            Debug.Log("Player found: " + player.name);
        }
        else
        {
            Debug.LogError("PlayerCok not found among Player-tagged objects!");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void stopPlayerMovement()
    {
        if (player != null)
        {
            PlayerMovements playerMovements = player.GetComponent<PlayerMovements>();
            if (playerMovements != null)
            {
                playerMovements.enabled = false;
            }
        }
    }

    public void resumePlayerMovement()
    {
        if (player != null)
        {
            PlayerMovements playerMovements = player.GetComponent<PlayerMovements>();
            if (playerMovements != null)
            {
                playerMovements.enabled = true;
            }
        }
    }

    public void MovePlayer()
    {
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        if (target == null)
        {
            Debug.LogError("Target not set!");
            return;
        }

        DayNightCycle.Instance.SetTime(desiredTime);
        DayNightCycle.Instance.ResumeTime();

        try
        {
            Debug.Log("Moving player to " + target.position + " from " + player.transform.position);

            // Stop player movement first
            stopPlayerMovement();

            // Get player components that might interfere
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            CharacterController playerCC = player.GetComponent<CharacterController>();

            // Handle Rigidbody2D movement

            player.transform.position = target.position;
            Debug.Log("Used direct transform movement");


            Debug.Log("Player moved to " + player.transform.position);

            // Wait a frame then resume movement
            StartCoroutine(ResumeMovementAfterDelay());
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            throw;
        }
    }

    private System.Collections.IEnumerator ResumeMovementAfterDelay()
    {
        yield return new WaitForEndOfFrame();
        resumePlayerMovement();
    }
}
