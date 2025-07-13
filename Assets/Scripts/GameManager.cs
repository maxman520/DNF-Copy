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
        // ���� ���� �� ���� �ʱ�ȭ
        InitializePlayerState();
    }
    public void InitializePlayerState()
    {
        CurrentHealth = Player.Instance.MaxHP;
        CurrentMana = Player.Instance.MaxMP;

        // UI �Ŵ������� �ʱ� UI ������Ʈ ��û
        UIManager.Instance.UpdateHP(Player.Instance.MaxHP, CurrentHealth);
        UIManager.Instance.UpdateMP(Player.Instance.MaxMP, CurrentMana);
    }

    private void OnEnable()
    {
        // ���� �ε�� ������ SceneLoaded �Լ��� ȣ���ϵ��� �̺�Ʈ�� ���
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
            Debug.LogError("�÷��̾ NULL�Դϴ�");
            return;
        }

        switch(scene.name)
        {
            case "Dungeon1_Scene":
            // case Dungeon2, Dungeon3, ...
                Debug.Log("������ ����. ���� �ý��� Ȱ��ȭ.");
                player.OnEnterDungeon();
                break;

            case "Town_Scene":
                Debug.Log("���� �� �ε�");
                player.OnExitDungeon();
                // ������ �̵��� ü��, ���� ȸ��
                CurrentHealth = player.MaxHP;
                CurrentMana = player.MaxMP;
                break;
        }

    }
}