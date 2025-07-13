using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
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
                Debug.Log("던전에 입장. 전투 시스템 활성화.");
                player.OnEnterDungeon();
                break;

            case "Town_Scene":
                Debug.Log("마을 씬 로드");
                player.OnExitDungeon();
                // 마을로 이동시 체력, 마나 회복
                CurrentHealth = player.MaxHP;
                CurrentMana = player.MaxMP;
                break;
        }

    }
}