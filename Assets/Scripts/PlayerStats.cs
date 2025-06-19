using UnityEngine;

public class PlayerStats : Singleton<PlayerStats>
{

    [Header("Player Stats")]
    public int atk = 10;
    public int def = 10;
    public float walkSpeed = 3f;
    public float runSpeed = 6f;

    public float MaxHP = 100f;
    public float MaxMP = 100f;

}