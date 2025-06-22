using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PlayerStats stats;
    public PlayerStats Stats => stats;
    private PlayerAnimation anim;

    private void Awake()
    {
        anim = GetComponent<PlayerAnimation>();
    }

    // public void resetPlayer(){
    //     stats.resetPlayerStats();
    //     anim.resetPlayer();
    // }
}
