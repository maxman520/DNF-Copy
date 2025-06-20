using UnityEngine;

public interface PlayerStateInterface
{
    // �� ���¿� �������� �� ȣ��� �Լ�
    void Enter();

    // �� ���¿��� �� ������ ����� �Լ� (�Է� ó��, ���� ������Ʈ ��)
    void Update();

    // �� ���¿��� ���� �����Ӹ��� ����� �Լ� (���� ó��)
    void FixedUpdate();

    // �� ���¸� �������� �� ȣ��� �Լ�
    void Exit();
}