using UnityEngine;

public class PlayerStats : Singleton<PlayerStats>
{

    [Header("Player Stats")]
    public int atk = 10;
    public int def = 10;
    public float walkSpeed = 3f;
    public float runSpeed = 6f;

    public float MaxHealth = 100f;
    public float MaxMana = 100f;

}