using UnityEngine;

[CreateAssetMenu(fileName = "New Monster Data", menuName = "Data/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("기본 정보")]
    public string monsterName;
    public GameObject prefab; // 몬스터의 외형 프리팹

    [Header("핵심 스탯")]
    public float maxHP;
    public float atk;
    public float moveSpeed;

    [Header("AI 관련 스탯")]
    public float recognitionRange; // 플레이어 인식 범위
    public float attackRange;      // 공격 가능 범위
}