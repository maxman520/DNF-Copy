using System.Collections.Generic;
using UnityEngine;

public class MinimapUI : MonoBehaviour
{
    [SerializeField] private GameObject roomIconPrefab;
    [SerializeField] private Transform gridParent; // Grid Layout Group이 있는 Transform

    // 2차원 딕셔너리로 방 아이콘들을 관리
    private Dictionary<Vector2Int, MinimapRoomIconController> roomIcons = new Dictionary<Vector2Int, MinimapRoomIconController>();
    private Dungeon currentDungeon;
    private int currentRoomIndex = -1; // 현재 방 인덱스를 추적

    // 던전이 시작될 때 DungeonManager가 호출
    public void GenerateMap(Dungeon dungeon)
    {
        if (dungeon == null) return;
        this.currentDungeon = dungeon;
        ClearMap();

        // 1. 맵 크기에 맞춰 빈 격자판(아이콘)을 생성
        for (int y = 0; y < dungeon.mapSize.y; y++)
        {
            for (int x = 0; x < dungeon.mapSize.x; x++)
            {
                GameObject iconObj = Instantiate(roomIconPrefab, gridParent);
                MinimapRoomIconController controller = iconObj.GetComponent<MinimapRoomIconController>();
                Vector2Int coords = new Vector2Int(x, y);

                // 생성된 아이콘을 좌표를 키로 하여 딕셔너리에 저장
                roomIcons[coords] = controller;

                // 일단 모든 칸을 숨김 상태로
                controller.SetState(MinimapRoomState.Hidden);
            }
        }

        // 2. 실제 존재하는 방들의 정보를 좌표에 맞는 아이콘에 할당
        foreach (Room room in dungeon.Rooms)
        {
            if (roomIcons.ContainsKey(room.coordinates))
            {
                MinimapRoomIconController icon = roomIcons[room.coordinates];
                icon.AssignRoom(room);
                // 아직 방문 전이므로 Empty 상태로 설정 (보스방은 알아서 보스 아이콘으로 표시)
                icon.SetState(MinimapRoomState.Empty);
            }
        }
    }

    // 플레이어의 위치가 바뀔 때 DungeonManager가 호출
    public void UpdatePlayerPosition(int newRoomIndex)
    {
        if (currentDungeon == null || newRoomIndex < 0 || newRoomIndex >= currentDungeon.Rooms.Count) return;

        // 1. 이전에 있던 방을 Discovered 상태로 변경
        if (currentRoomIndex != -1)
        {
            Room oldRoom = currentDungeon.Rooms[currentRoomIndex];
            if (roomIcons.ContainsKey(oldRoom.coordinates))
            {
                roomIcons[oldRoom.coordinates].SetState(MinimapRoomState.Discovered);
            }
        }

        // 2. 새로 들어온 방을 Current 상태로 변경
        Room newRoom = currentDungeon.Rooms[newRoomIndex];
        if (roomIcons.ContainsKey(newRoom.coordinates))
        {
            roomIcons[newRoom.coordinates].SetState(MinimapRoomState.Current);
        }

        // 3. 현재 방 인덱스 갱신
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