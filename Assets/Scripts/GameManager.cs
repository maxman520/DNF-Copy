using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    // --- 게임 플로우 상태 ---
    // MainMenu,           // 메인 메뉴 화면 (게임 시작 화면)
    CharacterSelect,    // 캐릭터 선택창
    Loading,            // 로딩 중

    // --- 인게임 상태 (In-Game) ---
    Town,               // 마을
    Dungeon,            // 던전
}

public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; } = GameState.Town;

    private Player player;

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
        // 씬이 로드될 때마다 SceneLoaded 함수를 호출하도록 이벤트에 등록
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

        Debug.Log($"{sceneName} 씬 로드를 시작합니다...");
        CurrentState = GameState.Loading;
        // UIManager.Instance.ShowLoadingScreen();

        SceneManager.LoadScene(sceneName);
    }

    // 씬이 로드된 '후'에 호출되는 정리 함수
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"'{scene.name}' 씬 로드 완료.");

        // 로딩 UI 끄기
        // UIManager.Instance.HideLoadingScreen();

        switch(scene.name)
        {
            case "Dungeon1_Scene":
            // case Dungeon2, Dungeon3, ...
                CurrentState = GameState.Dungeon;
                Player.Instance?.OnEnterDungeon();
                break;

            case "Town_Scene":
            // case Town2, Town3, ...
                CurrentState = GameState.Town;
                Player.Instance?.OnEnterTown();
                break;
        }

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

            // duration을 초 단위로 직접 사용 (더 간단함)
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