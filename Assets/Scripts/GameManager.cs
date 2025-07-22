using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Town,
    Dungeon,
    Loading
}

public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; } = GameState.Town;

    // --- Dungeon Logic --- (임시)
    private Dungeon currentDungeon;
    private float dungeonStartTime;
    private int totalHuntExp;
    // ---

    private void Start()
    {
        // 게임 시작 시 스탯 초기화
        InitializePlayerState();
    }

    public void InitializePlayerState()
    {
        // UI 매니저에게 초기 UI 업데이트 요청
        UIManager.Instance.UpdateHP(Player.Instance.MaxHP, Player.Instance.CurrentHP);
        UIManager.Instance.UpdateMP(Player.Instance.MaxMP, Player.Instance.CurrentMP);
        UIManager.Instance.UpdateEXP(Player.Instance.RequiredEXP, Player.Instance.CurrentEXP);
    }

    public void AddExp(int expAmount)
    {
        Player.Instance.AddExp(expAmount);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;
            
        CurrentState = GameState.Loading;
        // UIManager.Instance.ShowLoadingScreen();
        SceneManager.LoadScene(sceneName);
    }

    // 씬이 로드된 '후'에 호출되는 정리 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 로딩 UI 끄기
        // UIManager.Instance.HideLoadingScreen();
        switch(scene.name)
        {
            case "Dungeon1_Scene":
            // case Dungeon2, Dungeon3, ...
                CurrentState = GameState.Dungeon;
                Player.Instance.OnEnterDungeon();
                break;
            case "Town_Scene":
            // case Town2, Town3, ...
                CurrentState = GameState.Town;
                Player.Instance.OnEnterTown();
                RoomManager.Instance.UpdateCurrentRoomByPlayerPosition(); // 씬 로드 후 플레이어 위치 기반으로 카메라 경계 업데이트
                break;
        }
    }

    // Portal에 Player가 닿으면 Portal이 호출하는 함수
    public void EnterRoom(Room targetRoom, Portal targetPortal)
    {
        RoomManager.Instance.EnterRoom(targetRoom, targetPortal);
    }

    public void StartDungeon(Dungeon dungeonToStart)
    {
        Debug.Log($"새로운 던전 '{dungeonToStart.DungeonName}'을 시작");
        this.currentDungeon = dungeonToStart;

        // 던전 관련 정보 초기화
        dungeonStartTime = Time.time;
        totalHuntExp = 0;

        // 맵 이름 설정
        if (UIManager.Instance != null)
            UIManager.Instance.SetMapName(dungeonToStart.DungeonName);

        if (currentDungeon.Rooms == null)
            Debug.LogError("Dungeon_Data에 Room들이 할당되지 않았음");
        
        // 모든 방을 일단 끈다
        foreach (var room in currentDungeon.Rooms)
        {
            room.gameObject.SetActive(false);
        }

        // 플레이어 위치를 던전 시작 지점으로 설정
        Player.Instance.transform.position = currentDungeon.StartPosition;

        // 첫 번째 방을 활성화
        Room startRoom = currentDungeon.Rooms[0];
        startRoom.OnEnterRoom();

        // 플레이어 위치 기반으로 카메라 경계 업데이트
        RoomManager.Instance.UpdateCurrentRoomByPlayerPosition(); 

        // 미니맵 생성 요청 & 플레이어 미니맵 위치 업데이트
        if (UIManager.Instance != null)
        {
            UIManager.Instance.GenerateMinimap(dungeonToStart);
            UIManager.Instance.UpdateMinimapPlayerPosition(0);
        }
    }

    public void AddHuntExp(int exp)
    {
        totalHuntExp += exp;
    }

    public void ShowResultPanel()
    {
        DungeonResultData resultData = CalculateDungeonResult();
        UIManager.Instance.ShowResultPanel(resultData);
    }

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

        Player.Instance.transform.position = currentDungeon.TownSpawnPosition; // 플레이어 좌표 이동
        UIManager.Instance.HideResultPanel(); // 던전 결과 창 숨김
        currentDungeon = null;
        LoadScene(townToReturn); // 돌아가야할 마을 씬으로 변경
    }

    // "다음 던전 시작" 버튼이 호출할 함수
    public void GoToNextDungeon()
    {
        Debug.Log("결과를 확인했습니다. 다음 던전으로 이동합니다.");

        string nextDungeonSceneName = currentDungeon.NextDungeonName; // 임시 값. 현재 던전 데이터에 다음 던전 이름도 추가할 것

        // 다음 던전 씬 로드
        LoadScene(nextDungeonSceneName);
    }

    private bool isSlowing = false;

    // 슬로우 모션을 요청하는 메인 함수
    public UniTask DoSlowMotion(float duration, float slowFactor)
    {
        // 이미 다른 슬로우 모션이 진행 중이면 무시
        if (isSlowing) return UniTask.CompletedTask;
        return SlowMotionSequence(duration, slowFactor);
    }

    private async UniTask SlowMotionSequence(float duration, float slowFactor)
    {
        isSlowing = true;
        try
        {
            // 1. 시간을 느리게 만듦
            Time.timeScale = slowFactor;
            Debug.Log($"슬로우 모션 시작. TimeScale: {slowFactor}");

            // duration을 초 단위로 직접 사용
            await UniTask.Delay(TimeSpan.FromSeconds(duration), ignoreTimeScale: true);
        }
        finally
        {
            // 작업이 성공적으로 끝나든, 중간에 취소되든 항상 시간을 복원
            Time.timeScale = 1f;
            isSlowing = false;
            Debug.Log("슬로우 모션 종료. TimeScale: 1.0");
        }
    }
}
