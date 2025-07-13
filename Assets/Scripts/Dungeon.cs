using UnityEngine;
using System.Collections.Generic;

public class Dungeon : MonoBehaviour
{
    [Header("���� ����")]
    [SerializeField] public string DungeonName;
    [SerializeField] public List<Room> Rooms; // �� ������ �����ϴ� �� ���
    [SerializeField] public Vector3 StartPosition; // ���� ���� �� �÷��̾� ���� ��ġ

    private void Start()
    {
        // �� ���� ���� ���۵� ��, DungeonManager���� �ڽ��� ����ϰ� ������ ��û
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.StartDungeon(this);
        }
        else
        {
            Debug.LogError("DungeonManager�� ���� �������� ����");
        }
    }
}