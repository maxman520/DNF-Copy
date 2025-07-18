using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class Room : MonoBehaviour
{
    public enum RoomType { Normal, Start, Boss } // ���� ����

    [Flags]
    public enum HasExit // �� ���� ����
    {
        None = 0,
        Right = 1 << 0,
        Left = 1 << 1,
        Bottom = 1 << 2,
        Top = 1 << 3,
    }

    [Header("�� ����")]
    [SerializeField] public RoomType roomType;
    [SerializeField] public HasExit hasExit;
    [SerializeField] public CinemachineCamera virtualCamera; // �� �濡�� ����� ���� ī�޶�
    [SerializeField] private List<Monster> monsters; // �� �濡 �ִ� ��� ���� ����Ʈ
    [SerializeField] private List<Portal> portals; // �� ���� ��� ��Ż ����Ʈ

    [Header("�̴ϸ� ��ǥ")]
    public Vector2Int coordinates; // �� ���� �̴ϸ� �� ��ǥ

    private bool isCleared = false;

    public void OnEnable()
    {
        // ���� Ȱ��ȭ �Ǵ� ���� Virtual_Camera�� Follow Ÿ���� �÷��̾�� ����
        virtualCamera.Follow = Player.Instance.transform;
    }

    // ���� Ȱ��ȭ�� �� ȣ��
    public void OnEnterRoom()
    {
        Debug.Log($"{this.name}�� ����");

        // �� ���� ��� ���� Ȱ��ȭ
        this.gameObject.SetActive(true);

        // �����ϸ鼭 ī�޶� �켱������ ���� ī�޶� ����
        if (virtualCamera != null)
            virtualCamera.Priority = 10;
    }

    // ���� ���� �� ȣ��
    public void OnExitRoom()
    {
        Debug.Log($"{this.name}���� ����");

        // �� ���� ��� ���� ��Ȱ��ȭ�Ͽ� ������ ����
        this.gameObject.SetActive(false);

        // �ٸ� ī�޶�� �����ϱ� ���� ī�޶� �켱���� ����
        if (virtualCamera != null)
            virtualCamera.Priority = 9;
    }

    void Update()
    {
        // ���� Ŭ������� �ʾ��� ���� ���� ���� ���� üũ
        if (!isCleared)
        {
            CheckClearCondition();
        }
    }

    void CheckClearCondition()
    {
        // ��� ���Ͱ� �׾����� Ȯ��
        foreach (var monster in monsters)
        {
            // ���� ����ִ� ���Ͱ� �� ������ ������ �Լ� ����
            if (monster != null && monster.gameObject.activeSelf) // ������ ��Ȱ��ȭ�ǰų� �ı��ȴٴ� ����
            {
                return;
            }
        }

        // ��� ���Ͱ� �׾���
        isCleared = true;
        OnRoomCleared();
    }

    void OnRoomCleared()
    {
        Debug.Log($"{this.name} Ŭ����!");

        // ��� ��Ż�� Ȱ��ȭ
        foreach (var portal in portals)
        {
            if (portal != null)
                portal.Activate();
        }
    }
}