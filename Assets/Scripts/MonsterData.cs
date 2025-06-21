using UnityEngine;

[CreateAssetMenu(fileName = "New Monster Data", menuName = "Data/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("�⺻ ����")]
    public string MonsterName;
    public GameObject Prefab; // ������ ���� ������

    [Header("�ٽ� ����")]
    public float MaxHP;
    public float Atk;
    public float Def;
    public float MoveSpeed;

    [Header("AI ���� ����")]
    public float RecognitionRange; // �÷��̾� �ν� ����
    public float AttackRange;      // ���� ���� ����


}