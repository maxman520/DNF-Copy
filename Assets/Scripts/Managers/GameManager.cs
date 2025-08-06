using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
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
    [Header("플레이어 설정")]
    public GameObject playerPrefab; // 인스펙터에서 할당할 플레이어 프리팹

    public GameState CurrentState { get; private set; } = GameState.Town;

    private Dungeon currentDungeon;
    private float dungeonStartTime;
    private int totalHuntExp;
    private Vector3? nextSpawnPosition = null;

    private void InitializePlayerStats(CharacterData data)
    {
        Player.Instance.InitializeStats(data);
        Debug.Log($"플레이어 스탯 초기화");
    }

    public void AddExp(int expAmount)
    {
        Player.Instance.AddExp(expAmount);
    }

    protected override void Awake()
    {
        base.Awake();
        // Player 인스턴스가 없으면 생성 (Ex. 게임 시작)
        if (Player.Instance == null)
        {
            GameObject playerObject = Instantiate(playerPrefab);
            playerObject.transform.position = new Vector3(0, 0, 0);
            playerObject.name = "Player"; // 인스턴스 이름 설정
            Player player = playerObject.GetComponent<Player>();
            if (player != null && DataManager.Instance != null)
            {
                InitializePlayerStats(DataManager.Instance.SelectedCharacter);
            }
            else
            {
                Debug.LogError("소환된 플레이어 프리팹에 Player 컴포넌트가 없습니다.");
            }   
        }
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
                break;
        }
    }

    private void InitializeMap(string mapName, List<Room> rooms, Vector3 startPosition)
    {
        // 맵 이름 설정
        UIManager.Instance.SetMapName(mapName);

        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogError("InitializeMap: rooms 리스트가 비어있거나 할당되지 않았음");
            return;
        }

        // 플레이어 위치 설정
        Player.Instance.transform.position = startPosition;

        // 플레이어가 위치한 방 찾기
        Room startRoom = null;
        foreach (var room in rooms)
        {
            // Room의 CameraBound를 기준으로 현재 위치가 방 안에 있는지 확인
            if (room.CameraBound != null && room.CameraBound.bounds.Contains(startPosition))
            {
                startRoom = room;
                break;
            }
        }

        // 만약 시작 위치에 해당하는 방을 찾지 못했다면, 첫 번째 방을 시작 방으로 사용
        if (startRoom == null)
        {
            Debug.LogWarning("시작 위치에 해당하는 방을 찾지 못했습니다. 첫 번째 방을 대신 활성화합니다.");
            startRoom = rooms[0];
        }

        // RoomManager의 currentRoom 변수 업데이트, 카메라 경계 설정
        RoomManager.Instance.UpdateCurrentRoom(startRoom);
        RoomManager.Instance.ChangeCameraConfiner(startRoom.CameraBound);
        
        // startRoom 제외 모든 방을 비활성화
        foreach (var room in rooms)
        {
            if (room != startRoom)
                room.gameObject.SetActive(false);
        }
    }

    // 마을 씬 시작 시 Town.cs가 호출
    public void StartTown(Town townToStart)
    {
        Debug.Log($"마을 '{townToStart.TownName}'에 입장");
        
        // 지정된 스폰 위치가 있으면 사용하고, 없으면 타운의 기본 시작 위치 사용
        Vector3 startPosition = nextSpawnPosition.HasValue ? nextSpawnPosition.Value : townToStart.StartPosition;
        nextSpawnPosition = null; // 사용 후 초기화

        InitializeMap(townToStart.TownName, townToStart.Rooms, startPosition);
        
        // 미니맵 비활성화
        UIManager.Instance.HideMinimap();
    }

    // 던전 씬 시작 시 Dungeon.cs가 호출
    public void StartDungeon(Dungeon dungeonToStart)
    {
        Debug.Log($"새로운 던전 '{dungeonToStart.DungeonName}'을 시작");
        this.currentDungeon = dungeonToStart;

        // 던전 관련 정보 초기화
        dungeonStartTime = Time.time;
        totalHuntExp = 0;

        InitializeMap(dungeonToStart.DungeonName, dungeonToStart.Rooms, dungeonToStart.StartPosition);

        // 미니맵 생성 요청 & 플레이어 미니맵 위치 업데이트
        UIManager.Instance.GenerateMinimap(dungeonToStart);
        UIManager.Instance.UpdateMinimapPlayerPosition(0);
    }

    #region Result Panel
    public void AddHuntExp(int exp)
    {
        totalHuntExp += exp;
    }

    public void ShowResultPanel()
    {
        DungeonResultData resultData = CalculateDungeonResult();
        UIManager.Instance.SetResultPanelData(resultData);
        UIManager.Instance.ToggleResultPanel();
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
        Debug.Log("마을로 돌아갑니다.");
        string townToReturn = currentDungeon.TownToReturn;

        // 다음 씬에서 사용할 스폰 위치 저장
        nextSpawnPosition = currentDungeon.TownSpawnPosition;

        UIManager.Instance.ToggleResultPanel(); // 던전 결과 창 숨김
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
    #endregion Result Panel

    public void GoToTown()
    {
        ReturnToTown();
    }

    public void ResetGameAndGoToCharacterSelect()
    {
        SavePlayerData();

        // 다른 싱글톤 인스턴스 파괴
        if (MainCamera.Instance != null) Destroy(MainCamera.Instance.gameObject);
        if (VirtualCamera.Instance != null) Destroy(VirtualCamera.Instance.gameObject);
        if (RoomManager.Instance != null) Destroy(RoomManager.Instance.gameObject);
        if (UIManager.Instance != null) Destroy(UIManager.Instance.gameObject);
        if (EffectManager.Instance != null) Destroy(EffectManager.Instance.gameObject);
        if (Player.Instance != null) Destroy(Player.Instance.gameObject);

        // 자기 자신 파괴
        Destroy(gameObject);

        // 캐릭터 선택 씬 로드
        SceneManager.LoadScene("CharacterSelect_Scene");
    }

    public void SavePlayerData()
    {
        if (Player.Instance == null || DataManager.Instance == null) return;

        // 현재 플레이어의 데이터를 기반으로 새로운 CharacterData 생성
        CharacterData currentData = new CharacterData
        {
            // CharacterID, CharacterName, JobName 등은 기존 정보를 유지해야 함
            CharacterName = DataManager.Instance.SelectedCharacter.CharacterName,
            JobName = DataManager.Instance.SelectedCharacter.JobName,
            PreviewPrefabName = DataManager.Instance.SelectedCharacter.PreviewPrefabName,

            // 업데이트가 필요한 정보들
            Level = Player.Instance.Level,
            CurrentEXP = Player.Instance.CurrentEXP,
            RequiredEXP = Player.Instance.RequiredEXP,
            Atk = Player.Instance.Atk,
            Def = Player.Instance.Def,
            MaxHP = Player.Instance.MaxHP,
            MaxMP = Player.Instance.MaxMP,
            MoveSpeed = Player.Instance.WalkSpeed // WalkSpeed를 기본 이동속도로 저장
        };

        // DataManager에 업데이트 요청
        DataManager.Instance.UpdateAndSaveCurrentCharacter(currentData);
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
