using UnityEngine;
using System.Collections.Generic;

public class Dungeon : MonoBehaviour
{
    [Header("���� ����")]
    [SerializeField] public string DungeonName;
    [SerializeField] public List<Room> Rooms; // �� ������ �����ϴ� �� ���
    [SerializeField] public Vector3 StartPosition; // ���� ���� �� �÷��̾� ���� ��ġ
    [SerializeField] public Vector2Int mapSize; // ���� ũ�� (��: X=3, Y=3)

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