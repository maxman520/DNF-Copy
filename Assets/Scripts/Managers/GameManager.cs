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

    public Dungeon CurrentDungeon { get; private set; }
    private float dungeonStartTime;
    private int totalHuntExp;
    private Vector3? nextSpawnPosition = null;
    
    // 보스 처치(클리어) 시 BGM 임시 감쇄를 위한 백업
    private float? preBgmVolume = null;

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
            Instantiate(playerPrefab);
            Player.Instance.transform.position = new Vector3(0, 0, 0);
            Player.Instance.name = "Player"; // 인스턴스 이름 설정
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.OnRoomEntered += OnRoomEntered; // 방 전환 시 BGM 제어
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.OnRoomEntered -= OnRoomEntered;
        }
    }

    public async void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;
        
        // 로딩 씬 Additive 비동기 로드
        var loadLoadingScene = SceneManager.LoadSceneAsync("Loading_Scene", LoadSceneMode.Additive);
        await UniTask.WaitUntil(() => loadLoadingScene.isDone);
        Debug.Log("Loading_Scene 로드 완료");

        // 메인 씬을 비동기적으로 로드하고 완료될 때까지 기다립니다.
        var loadSceneOperation = SceneManager.LoadSceneAsync(sceneName);
        loadSceneOperation.allowSceneActivation = false; // 씬 활성화를 수동으로 제어
        Debug.Log("메인 Scene 로드 후 비활성화");

        // !! Unity에서 씬 비동기 로드의 progress는 최대 0.9까지만 오르고, 실제 씬 활성화는 allowSceneActivation = true가 되었을 때 이뤄짐 !!
        // 로딩 진행 상황 업데이트 (필요시)
        while (loadSceneOperation.progress < 0.9f)
        {
            // 구현 예: 로딩 UI에 진행 상황 표시
            Debug.Log($"로딩 진행 중: {loadSceneOperation.progress * 100}%");
            await UniTask.Yield();
        }

        // 씬 활성화
        loadSceneOperation.allowSceneActivation = true;
        Debug.Log("메인 Scene 활성화");
    }

    // 씬이 로드된 '후'에 호출되는 정리 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 바뀔 때마다 RoomManager 이벤트 구독 재확인
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.OnRoomEntered -= OnRoomEntered;
            RoomManager.Instance.OnRoomEntered += OnRoomEntered;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideDungeonUI(); // 던전 관련 UI를 모두 숨김
        }

        switch(scene.name)
        {
            case "SelectDungeon_Granfloris_Scene":
                // 던전 선택 씬 진입 시 맵 등장 사운드
                AudioManager.Instance.PlaySFX("Map_Appear");
                break;
            case "Loading_Scene":
                CurrentState = GameState.Loading;
                break;
            case "Dungeon1_Scene":
            case "Dungeon2_Scene":
            case "Dungeon3_Scene":
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

    // 방 입장 시 BGM 전환 처리(던전/타운 공통)
    private void OnRoomEntered(Room room)
    {
        if (room == null)
        {
            Debug.LogWarning("방 전환 이벤트 수신: room이 없습니다");
            return;
        }

        // 방 오버라이드 키 우선: 동일 키면 재시작 없이 그대로 유지
        if (!string.IsNullOrEmpty(room.BgmKeyOverride))
        {
            Debug.Log($"방 전환으로 BGM 변경: {room.BgmKeyOverride}");
            AudioManager.Instance.PlayBGMIfChanged(room.BgmKeyOverride, true, 1.0f);
            return;
        }

        // 보스 방인데 키가 비어있으면 알림
        if (room.roomType == Room.RoomType.Boss)
        {
            Debug.LogWarning("보스 방 BGM 키가 설정되지 않았습니다. Room의 BgmKeyOverride를 지정하세요");
            return;
        }
        // 오버라이드가 없으면 BGM 변경 없음(이전 재생 유지)
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

        // 시작 방에 대한 BGM 규칙 적용(오버라이드가 있으면 반영)
        OnRoomEntered(startRoom);
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
        CurrentDungeon = dungeonToStart;

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
        // BGM 볼륨을 일시적으로 낮춤(던전 클리어 연출)
        if (AudioManager.Instance != null && preBgmVolume == null)
        {
            preBgmVolume = AudioManager.Instance.GetCurrentBgmVolume();
            var ducked = (preBgmVolume ?? 0f) * 0.3f; // 30%로 감쇄
            AudioManager.Instance.SetCurrentBgmVolume(ducked);
        }

        DungeonResultData resultData = CalculateDungeonResult();
        UIManager.Instance.SetResultPanelData(resultData);
        UIManager.Instance.ToggleResultPanel();
    }

    private DungeonResultData CalculateDungeonResult()
    {
        float clearTime = Time.time - dungeonStartTime;
        Sprite rankSprite = null;

        // 랭크 계산
        if (CurrentDungeon.RankTimeThresholds != null && CurrentDungeon.RankSprites != null &&
            CurrentDungeon.RankTimeThresholds.Count == CurrentDungeon.RankSprites.Count)
        {
            // 가장 좋은 랭크(가장 짧은 시간)부터 순회
            for (int i = 0; i < CurrentDungeon.RankTimeThresholds.Count; i++)
            {
                if (clearTime <= CurrentDungeon.RankTimeThresholds[i])
                {
                    rankSprite = CurrentDungeon.RankSprites[i];
                    break; // 조건에 맞는 첫 랭크를 찾으면 중단
                }
            }

            // 모든 시간 기준을 초과했다면 가장 낮은 랭크를 부여
            if (rankSprite == null && CurrentDungeon.RankSprites.Count > 0)
            {
                rankSprite = CurrentDungeon.RankSprites[CurrentDungeon.RankSprites.Count - 1];
            }
        }
        return new DungeonResultData
        {
            ClearTime = clearTime,
            HuntEXP = totalHuntExp,
            ClearEXP = CurrentDungeon.ClearEXP,
            RankSprite = rankSprite
        };
    }

    // 던전 결과 창 "마을로 돌아가기" 버튼이 호출
    public void ReturnToTown()
    {
        Debug.Log("마을로 돌아갑니다.");
        // 감쇄된 BGM을 원래대로 복원
        if (AudioManager.Instance != null && preBgmVolume.HasValue)
        {
            AudioManager.Instance.SetCurrentBgmVolume(preBgmVolume.Value);
            preBgmVolume = null;
        }
        string townToReturn = CurrentDungeon.TownToReturn;

        // 다음 씬에서 사용할 스폰 위치 저장
        nextSpawnPosition = CurrentDungeon.TownSpawnPosition;

        CurrentDungeon = null;
        LoadScene(townToReturn); // 돌아가야할 마을 씬으로 변경
    }

    // "다음 던전 시작" 버튼이 호출할 함수
    public void GoToNextDungeon()
    {
        Debug.Log("결과를 확인했습니다. 다음 던전으로 이동합니다.");

        // 감쇄된 BGM을 원래대로 복원
        if (AudioManager.Instance != null && preBgmVolume.HasValue)
        {
            AudioManager.Instance.SetCurrentBgmVolume(preBgmVolume.Value);
            preBgmVolume = null;
        }

        string nextDungeonSceneName = CurrentDungeon.NextDungeonName; // 임시 값. 현재 던전 데이터에 다음 던전 이름도 추가할 것

        // 다음 던전 씬 로드
        LoadScene(nextDungeonSceneName);
    }
    #endregion Result Panel

    // 메뉴 창 "마을로 가기" 버튼이 호출
    public void GoToTown()
    {
        ReturnToTown();
    }

    // 임의의 마을로 이동(스폰 위치 지정)
    public void GoToTown(string townSceneName, Vector3 spawnPosition)
    {
        if (string.IsNullOrEmpty(townSceneName))
        {
            Debug.LogWarning("GoToTown: 이동할 마을 씬 이름이 비어있습니다");
            return;
        }

        // 감쇄된 BGM을 원래대로 복원 (메뉴 등으로 마을 이동 시)
        if (AudioManager.Instance != null && preBgmVolume.HasValue)
        {
            AudioManager.Instance.SetCurrentBgmVolume(preBgmVolume.Value);
            preBgmVolume = null;
        }

        // 다음 씬에서 사용할 스폰 위치 저장
        nextSpawnPosition = spawnPosition;
        CurrentDungeon = null;
        LoadScene(townSceneName);
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
        LoadScene("CharacterSelect_Scene");
    }

    public void SavePlayerData()
    {
        if (Player.Instance == null || DataManager.Instance == null) return;

        Inventory inventory = Player.Instance.PlayerInventory;
        if (inventory == null) return;

        // 현재 플레이어의 데이터를 기반으로 새로운 CharacterData 생성
        CharacterData currentData = new CharacterData
        {
            // 기본 정보 유지
            CharacterName = DataManager.Instance.SelectedCharacter.CharacterName,
            JobName = DataManager.Instance.SelectedCharacter.JobName,
            PreviewPrefabName = DataManager.Instance.SelectedCharacter.PreviewPrefabName,

            // 스탯 정보 업데이트
            Level = Player.Instance.Level,
            CurrentEXP = Player.Instance.CurrentEXP,
            RequiredEXP = Player.Instance.RequiredEXP,
            baseAtk = Player.Instance.baseAtk,
            baseDef = Player.Instance.baseDef,
            MaxHP = Player.Instance.MaxHP,
            MaxMP = Player.Instance.MaxMP,
            MoveSpeed = Player.Instance.WalkSpeed,
            Gold = inventory.Gold,
            Coin = inventory.Coin
        };

        // 인벤토리 아이템 저장
        currentData.inventoryItems.Clear();
        foreach (var item in inventory.Items)
        {
            if (item != null)
            {
                currentData.inventoryItems.Add(item);
            }
        }

        // 장착 아이템 저장
        currentData.equippedItemIDs.Clear();
        foreach (var item in inventory.EquippedItems.Values)
        {
            if (item != null)
            {
                currentData.equippedItemIDs.Add(item.itemID);
            }
        }

        // 퀵슬롯 아이템 저장
        currentData.quickSlotItemIDs.Clear();
        foreach (var itemID in inventory.QuickSlotItemIDs)
        {
            currentData.quickSlotItemIDs.Add(itemID);
        }

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
