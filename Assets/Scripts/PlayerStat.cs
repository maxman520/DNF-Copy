using UnityEngine;

public class PlayerStat : Singleton<PlayerStat>
{

    [Header("Player Stats")]
    public float Atk = 10f;
    public float Def = 10f;
    public float MoveSpeed = 3f;
    public float MaxHP = 100f;
    public float MaxMP = 100f;

}