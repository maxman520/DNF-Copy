using UnityEngine;

public class PlayerStats : Singleton<PlayerStats>
{

    [Header("Player Stats")]
    public int Atk = 10;
    public int Def = 10;
    public float MoveSpeed = 3;
    public float MaxHP = 100f;
    public float MaxMP = 100f;

}