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
    public float CurrentHealth { get; private set; }
    public float CurrentMana { get; private set; }

    private void Start()
    {
        // 게임 시작 시 스탯 초기화
        InitializePlayerState();
    }
    public void InitializePlayerState()
    {
        CurrentHealth = Player.Instance.MaxHP;
        CurrentMana = Player.Instance.MaxMP;

        // UI 매니저에게 초기 UI 업데이트 요청
        UIManager.Instance.UpdateHP(Player.Instance.MaxHP, CurrentHealth);
        UIManager.Instance.UpdateMP(Player.Instance.MaxMP, CurrentMana);
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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = Player.Instance;

        if (player == null) {
            Debug.LogError("플레이어가 NULL입니다");
            return;
        }

        switch(scene.name)
        {
            case "Dungeon1_Scene":
            // case Dungeon2, Dungeon3, ...

                Debug.Log("게임 상태: Dungeon");
                CurrentState = GameState.Dungeon;

                player.OnEnterDungeon();

                break;

            case "Town_Scene":
                Debug.Log("게임 상태: Town");
                CurrentState = GameState.Town;

                player.OnExitDungeon();

                // 마을로 이동시 체력, 마나 회복
                CurrentHealth = player.MaxHP;
                CurrentMana = player.MaxMP;
                break;
        }

    }
}