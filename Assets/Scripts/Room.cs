using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;


public enum RoomType { Normal, Start, Boss } // ���� ����

public class Room : MonoBehaviour
{
    [Header("�� ����")]
    [SerializeField] public RoomType roomType;
    [SerializeField] public CinemachineCamera virtualCamera; // �� �濡�� ����� ���� ī�޶�
    [SerializeField] private List<Monster> monsters; // �� �濡 �ִ� ��� ���� ����Ʈ
    [SerializeField] private List<Portal> portals; // �� ���� ��� ��Ż ����Ʈ

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