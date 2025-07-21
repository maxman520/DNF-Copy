using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        // 플레이어 미니맵 위치 업데이트
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateMinimapPlayerPosition(targetRoomIndex);

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


        if (UIManager.Instance != null) 
            UIManager.Instance.SetMapName(dungeonToStart.DungeonName);

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

        // 미니맵 생성 요청 & 플레이어 미니맵 위치 업데이트
        if (UIManager.Instance != null)
        {
            UIManager.Instance.GenerateMinimap(dungeonToStart);
            UIManager.Instance.UpdateMinimapPlayerPosition(0);
        }

        Player.Instance.transform.position = currentDungeon.StartPosition;
        
    }

    // 보스 몬스터가 호출할 함수
    public void ShowResultPanel()
    {
        // 던전 클리어 보상 계산
        DungeonResultData resultData = CalculateDungeonResult();

        // 결과창 표시
        UIManager.Instance.ShowResultPanel(resultData);
    }

    // 보상 계산 로직
    private DungeonResultData CalculateDungeonResult()
    {
        // 실제로는 던전 데이터나 처치한 몬스터 리스트 등을 기반으로 계산해야 함
        // 임시 값. 나중에 수정할 것
        return new DungeonResultData
        {
            // clearTime도 추가해야 함
            HuntEXP = 12345,
            ClearEXP = 67890
        };
    }

    // "마을로 돌아가기" 버튼이 호출할 함수
    public void ReturnToTown()
    {
        Debug.Log("결과를 확인했습니다. 마을로 돌아갑니다.");

        string townToReturn = currentDungeon.TownToReturn;

        // 던전 관련 데이터 초기화
        currentDungeon = null;

        // 마을 씬 로드
        SceneManager.LoadScene(townToReturn);
    }

    // "다음 던전 시작" 버튼이 호출할 함수
    public void GoToNextDungeon()
    {
        Debug.Log("결과를 확인했습니다. 다음 던전으로 이동합니다.");

        string nextDungeonSceneName = currentDungeon.NextDungeonName; // 임시 값. 현재 던전 데이터에 다음 던전 이름도 추가할 것

        SceneManager.LoadScene(nextDungeonSceneName);
    }
}