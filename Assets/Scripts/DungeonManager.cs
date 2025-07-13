using UnityEngine;

public class DungeonManager : Singleton<DungeonManager>
{
    private Dungeon currentDungeon; // ���� �÷��� ���� ������ ����
    private Room currentRoom;

    protected override void Awake()
    {
        base.Awake();
    }

    // ��ǥ ��Ż �������� �̵��ϴ� �Լ�
    public void EnterRoom(int targetRoomIndex, Portal targetPortal)
    {

        if (targetPortal == null)
        {
            Debug.LogError("targetPortal�� �Ҵ���� �ʾ���");
            return;
        }

        if (targetRoomIndex < 0 || targetRoomIndex >= currentDungeon.Rooms.Count)
        {
            Debug.LogError("targetRoomIndex�� �߸��Ǿ���");
            return;
        }

        // ���� ���� �־��ٸ� ���� ó��
        currentRoom?.OnExitRoom();

        // currentRoom�� ���ο� ������ ����
        currentRoom = currentDungeon.Rooms[targetRoomIndex];

        // ���ο� ���� ���� ����
        currentRoom.OnEnterRoom();

        // �÷��̾� ��ġ �̵�
        Player.Instance.transform.position = targetPortal.transform.position;
    }

    // ���ο� ������ �����ϴ� �Լ�
    public void StartDungeon(Dungeon dungeonToStart)
    {
        Debug.Log($"���ο� ���� '{dungeonToStart.DungeonName}'�� ����");
        this.currentDungeon = dungeonToStart;

        if (currentDungeon.Rooms == null)
            Debug.LogError("Dungeon_Data�� Room���� �Ҵ���� �ʾ���");

        // ��� ���� �ϴ� ����
        foreach (var room in currentDungeon.Rooms)
        {
            room.gameObject.SetActive(false);
        }

        // ù ��° ����� ����
        currentRoom = currentDungeon.Rooms[0];
        currentRoom.OnEnterRoom();


        Player.Instance.transform.position = currentDungeon.StartPosition;
        
    }
}