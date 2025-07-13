using UnityEngine;

public class DungeonManager : Singleton<DungeonManager>
{
    private Dungeon currentDungeon; // 현재 플레이 중인 던전의 정보
    private Room currentRoom;

    protected override void Awake()
    {
        base.Awake();
    }

    // 목표 포탈 지점으로 이동하는 함수
    public void EnterRoom(int targetRoomIndex, Portal targetPortal)
    {

        if (targetPortal == null)
        {
            Debug.LogError("targetPortal이 할당되지 않았음");
            return;
        }

        if (targetRoomIndex < 0 || targetRoomIndex >= currentDungeon.Rooms.Count)
        {
            Debug.LogError("targetRoomIndex가 잘못되었음");
            return;
        }

        // 이전 방이 있었다면 퇴장 처리
        currentRoom?.OnExitRoom();

        // currentRoom을 새로운 방으로 설정
        currentRoom = currentDungeon.Rooms[targetRoomIndex];

        // 새로운 방의 로직 시작
        currentRoom.OnEnterRoom();

        // 플레이어 위치 이동
        Player.Instance.transform.position = targetPortal.transform.position;
    }

    // 새로운 던전을 시작하는 함수
    public void StartDungeon(Dungeon dungeonToStart)
    {
        Debug.Log($"새로운 던전 '{dungeonToStart.DungeonName}'을 시작");
        this.currentDungeon = dungeonToStart;

        if (currentDungeon.Rooms == null)
            Debug.LogError("Dungeon_Data에 Room들이 할당되지 않았음");

        // 모든 방을 일단 끈다
        foreach (var room in currentDungeon.Rooms)
        {
            room.gameObject.SetActive(false);
        }

        // 첫 번째 방부터 시작
        currentRoom = currentDungeon.Rooms[0];
        currentRoom.OnEnterRoom();


        Player.Instance.transform.position = currentDungeon.StartPosition;
        
    }
}