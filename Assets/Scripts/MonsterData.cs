using UnityEngine;

[CreateAssetMenu(fileName = "New Monster Data", menuName = "Data/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("�⺻ ����")]
    public string MonsterName;
    public Sprite FaceSprite; // ���� �ʻ�ȭ ��������Ʈ
    public GameObject Prefab; // ������ ���� ������
    public bool isBoss = false; // ���� ���� ����

    [Header("�ٽ� ����")]
    public float MaxHP;
    public float HpPerLine; // HP ������ �� �ٴ� ü��
    public float Atk;
    public float Def;
    public float MoveSpeed;

    [Header("AI ���� ����")]
    public float RecognitionRange; // �÷��̾� �ν� ����
    public float AttackRange;      // ���� ���� ����

    [Header("���� ����")]
    public AttackDetails[] attackDetails; // ������ ���� ������ ���� ����ü

}