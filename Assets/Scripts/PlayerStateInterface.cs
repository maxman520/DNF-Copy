using UnityEngine;

public interface PlayerStateInterface
{
    // �� ���¿� �������� �� ȣ��� �Լ�
    void Enter(Player player);

    // �� ���¿��� �� ������ ����� �Լ� (�Է� ó��, ���� ������Ʈ ��)
    void Update(Player player);

    // �� ���¿��� ���� �����Ӹ��� ����� �Լ� (���� ó��)
    void FixedUpdate(Player player);

    // �� ���¸� �������� �� ȣ��� �Լ�
    void Exit(Player player);
}