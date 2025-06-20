using UnityEngine;

public class PlayerStats : Singleton<PlayerStats>
{

    [Header("Player Stats")]
    public int atk = 10;
    public int def = 10;
    public float moveSpeed = 3;

    public float MaxHP = 100f;
    public float MaxMP = 100f;

}