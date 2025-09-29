using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MovePlayerTo : MonoBehaviour
{
    GameObject player;
    Transform defaultTarget;
    Transform target;
    Animator fadeAnimator;
    [SerializeField] float desiredTime = 0f;
    public static MovePlayerTo Instance { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        GameObject ImageFadeGO = GameObject.FindGameObjectWithTag("FadeImage");
        fadeAnimator = ImageFadeGO.GetComponent<Animator>();
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

    public void voidDelayCall(string name, float delay)
    {
        Invoke(name, delay);
    }

    public void movePlayerWithDelay()
    {
        desiredTime = 5;
        if (fadeAnimator != null)
        {
            fadeAnimator.Play("fade", 0, 0f);
        }
        else
        {
            Debug.Log("Error gess");
        }
        voidDelayCall("MovePlayer", 0.8f);
    }

    public void movePlayerWithDestinationFade(string destination)
    {
        desiredTime = 5;
        if (fadeAnimator != null)
        {
            fadeAnimator.Play("fade", 0, 0f);
        }
        else
        {
            Debug.Log("Error gess");
        }
        MovePlayerDestination(destination);
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

    public void MovePlayerDestination(string destination)
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("NPCTarget");
        Transform currentTarget = null;
        foreach (GameObject p in gameObjects)
        {
            if (p.name == destination)
            {
                currentTarget = p.transform;
                break;
            }
        }

        if (currentTarget != null)
        {
            try
            {
                // Stop player movement first
                stopPlayerMovement();

                // Get player components that might interfere
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

                player.transform.position = currentTarget.position;
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
    }
    private System.Collections.IEnumerator ResumeMovementAfterDelay()
    {
        yield return new WaitForEndOfFrame();
        resumePlayerMovement();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneChanged;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene arg0, LoadSceneMode arg1)
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
            if (p.name == "Player")
            {
                player = p;
                break;
            }
        }
    }
}
