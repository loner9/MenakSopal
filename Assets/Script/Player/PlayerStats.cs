using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("Config")]
    public float maxHealth = 10f;
    public float health;
    public float maxStamina = 100f;
    public float stamina;
    public float maxMana = 100f;
    public float mana;

    public int level;
    public int currentExp;
    public int nextLevelExp;
    public int initiaNextLevelExp;
    public float expMultiplier;
    public float playerStage;

    public void resetPlayerStats()
    {
        health = maxHealth;
        stamina = maxStamina;
        mana = maxMana;
    }
}
