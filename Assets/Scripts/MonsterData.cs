using UnityEngine;

[CreateAssetMenu(fileName = "New Monster Data", menuName = "Data/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("�⺻ ����")]
    public string monsterName;
    public GameObject prefab; // ������ ���� ������

    [Header("�ٽ� ����")]
    public float maxHP;
    public float atk;
    public float moveSpeed;

    [Header("AI ���� ����")]
    public float recognitionRange; // �÷��̾� �ν� ����
    public float attackRange;      // ���� ���� ����
}