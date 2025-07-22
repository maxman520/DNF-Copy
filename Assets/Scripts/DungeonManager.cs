using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonManager : Singleton<DungeonManager>
{
    private Dungeon currentDungeon; // 현재 플레이 중인 던전의 정보
    private Room currentRoom;
    private float dungeonStartTime; // 던전 시작 시간
    private int totalHuntExp; // 누적된 사냥 경험치

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

        // 던전 관련 정보 초기화
        dungeonStartTime = Time.time;
        totalHuntExp = 0;


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

    // 몬스터가 죽었을 때 경험치를 추가하는 함수
    public void AddHuntExp(int exp)
    {
        totalHuntExp += exp;
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
        float clearTime = Time.time - dungeonStartTime;
        Sprite rankSprite = null;

        // 랭크 계산
        if (currentDungeon.RankTimeThresholds != null && currentDungeon.RankSprites != null &&
            currentDungeon.RankTimeThresholds.Count == currentDungeon.RankSprites.Count)
        {
            // 가장 좋은 랭크(가장 짧은 시간)부터 순회
            for (int i = 0; i < currentDungeon.RankTimeThresholds.Count; i++)
            {
                if (clearTime <= currentDungeon.RankTimeThresholds[i])
                {
                    rankSprite = currentDungeon.RankSprites[i];
                    break; // 조건에 맞는 첫 랭크를 찾으면 중단
                }
            }

            // 모든 시간 기준을 초과했다면 가장 낮은 랭크를 부여
            if (rankSprite == null && currentDungeon.RankSprites.Count > 0)
            {
                rankSprite = currentDungeon.RankSprites[currentDungeon.RankSprites.Count - 1];
            }
        }

        return new DungeonResultData
        {
            ClearTime = clearTime,
            HuntEXP = totalHuntExp,
            ClearEXP = currentDungeon.ClearEXP,
            RankSprite = rankSprite
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
        GameManager.Instance.LoadScene(townToReturn);
    }

    // "다음 던전 시작" 버튼이 호출할 함수
    public void GoToNextDungeon()
    {
        Debug.Log("결과를 확인했습니다. 다음 던전으로 이동합니다.");

        string nextDungeonSceneName = currentDungeon.NextDungeonName; // 임시 값. 현재 던전 데이터에 다음 던전 이름도 추가할 것

        // 다음 던전 씬 로드
        GameManager.Instance.LoadScene(nextDungeonSceneName);
    }
}