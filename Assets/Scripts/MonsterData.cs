using UnityEngine;

[CreateAssetMenu(fileName = "New Monster Data", menuName = "Data/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("기본 정보")]
    public string MonsterName;
    public Sprite FaceSprite; // 몬스터 초상화 스프라이트
    public GameObject Prefab; // 몬스터의 외형 프리팹
    public bool isBoss = false; // 보스 몬스터 인지

    [Header("핵심 스탯")]
    public float MaxHP;
    public float HpPerLine; // HP 게이지 한 줄당 체력
    public float Atk;
    public float Def;
    public float MoveSpeed;

    [Header("AI 관련 스탯")]
    public float RecognitionRange; // 플레이어 인식 범위
    public float AttackRange;      // 공격 가능 범위

    [Header("공격 정보")]
    public AttackDetails[] attackDetails; // 몬스터의 공격 정보를 담을 구조체

}