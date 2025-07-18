using System.Collections.Generic;
using UnityEngine;

public class MinimapUI : MonoBehaviour
{
    [SerializeField] private GameObject roomIconPrefab;
    [SerializeField] private Transform gridParent; // Grid Layout Group�� �ִ� Transform

    // 2���� ��ųʸ��� �� �����ܵ��� ����
    private Dictionary<Vector2Int, MinimapRoomIconController> roomIcons = new Dictionary<Vector2Int, MinimapRoomIconController>();
    private Dungeon currentDungeon;
    private int currentRoomIndex = -1; // ���� �� �ε����� ����

    // ������ ���۵� �� DungeonManager�� ȣ��
    public void GenerateMap(Dungeon dungeon)
    {
        if (dungeon == null) return;
        this.currentDungeon = dungeon;
        ClearMap();

        // 1. �� ũ�⿡ ���� �� ������(������)�� ����
        for (int y = 0; y < dungeon.mapSize.y; y++)
        {
            for (int x = 0; x < dungeon.mapSize.x; x++)
            {
                GameObject iconObj = Instantiate(roomIconPrefab, gridParent);
                MinimapRoomIconController controller = iconObj.GetComponent<MinimapRoomIconController>();
                Vector2Int coords = new Vector2Int(x, y);

                // ������ �������� ��ǥ�� Ű�� �Ͽ� ��ųʸ��� ����
                roomIcons[coords] = controller;

                // �ϴ� ��� ĭ�� ���� ���·�
                controller.SetState(MinimapRoomState.Hidden);
            }
        }

        // 2. ���� �����ϴ� ����� ������ ��ǥ�� �´� �����ܿ� �Ҵ�
        foreach (Room room in dungeon.Rooms)
        {
            if (roomIcons.ContainsKey(room.coordinates))
            {
                MinimapRoomIconController icon = roomIcons[room.coordinates];
                icon.AssignRoom(room);
                // ���� �湮 ���̹Ƿ� Empty ���·� ���� (�������� �˾Ƽ� ���� ���������� ǥ��)
                icon.SetState(MinimapRoomState.Empty);
            }
        }
    }

    // �÷��̾��� ��ġ�� �ٲ� �� DungeonManager�� ȣ��
    public void UpdatePlayerPosition(int newRoomIndex)
    {
        if (currentDungeon == null || newRoomIndex < 0 || newRoomIndex >= currentDungeon.Rooms.Count) return;

        // 1. ������ �ִ� ���� Discovered ���·� ����
        if (currentRoomIndex != -1)
        {
            Room oldRoom = currentDungeon.Rooms[currentRoomIndex];
            if (roomIcons.ContainsKey(oldRoom.coordinates))
            {
                roomIcons[oldRoom.coordinates].SetState(MinimapRoomState.Discovered);
            }
        }

        // 2. ���� ���� ���� Current ���·� ����
        Room newRoom = currentDungeon.Rooms[newRoomIndex];
        if (roomIcons.ContainsKey(newRoom.coordinates))
        {
            roomIcons[newRoom.coordinates].SetState(MinimapRoomState.Current);
        }

        // 3. ���� �� �ε��� ����
        currentRoomIndex = newRoomIndex;
    }

    private void ClearMap()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }
        roomIcons.Clear();
        currentRoomIndex = -1;
    }
}