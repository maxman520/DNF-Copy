using UnityEngine;

[System.Serializable] // �ν����� â�� ����
public struct AttackDetails
{
    public float damageRate;            // �� ������ ������ ����. �⺻ 1
    public float knockbackForce;    // ���� �˹��� Rigidbody �ӵ��� ������ ��ȿ
    public float launchForce;    // ���߿� ���� �� (���� ������ �ƴϸ� 0)
    // �ʿ��ϴٸ� ���߿� ���� �ð�, �Ӽ� � �߰� ����
}