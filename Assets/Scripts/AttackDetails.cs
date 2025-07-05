using UnityEngine;

[System.Serializable] // �ν����� â�� ����
public struct AttackDetails
{
    public string attackName;
    public float damageRate;            // �� ������ ������ ����. �⺻ 1
    public float knockbackForce;    // ���� �˹��� Rigidbody �ӵ��� ������ ��ȿ
    public float launchForce;    // ���߿� ���� �� (���� ������ �ƴϸ� 0)
    public float yOffset; // y�� ���� ����
    // �ʿ��ϴٸ� ���߿� ���� �ð�, �Ӽ� � �߰� ����
}