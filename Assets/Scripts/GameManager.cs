using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public float CurrentHealth { get; private set; }
    public float CurrentMana { get; private set; }

    private void Start()
    {
        // ���� ���� �� ���� �ʱ�ȭ
        InitializePlayerState();
    }
    public void InitializePlayerState()
    {
        CurrentHealth = PlayerStats.Instance.MaxHP;
        CurrentMana = PlayerStats.Instance.MaxMP;

        // UI �Ŵ������� �ʱ� UI ������Ʈ ��û
        UIManager.Instance.UpdateHP(PlayerStats.Instance.MaxHP, CurrentHealth);
        UIManager.Instance.UpdateMP(PlayerStats.Instance.MaxMP, CurrentMana);
    }

    private void OnEnable()
    {
        // ���� �ε�� ������ SceneLoaded �Լ��� ȣ���ϵ��� �̺�Ʈ�� ���
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // ������Ʈ�� �ı��� �� �̺�Ʈ ��� ����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Player player = Player.Instance;
        if (player == null) return;


        if (scene.name == "Dungeon1_Scene")
        {
            Debug.Log("���� �� �ε�");
            player.SetState(Player.PlayerState.Dungeon);
        }
        else if (scene.name == "Town_Scene")
        {
            Debug.Log("���� �� �ε�");
            player.SetState(Player.PlayerState.Town);
            // ������ �̵��� ü��, ���� ȸ��
            CurrentHealth = PlayerStats.Instance.MaxHP;
            CurrentMana = PlayerStats.Instance.MaxMP;
        }

        // �÷��̾ ���� �������� �̵�
        GameObject startPoint = GameObject.FindWithTag("Respawn");
        if (startPoint != null)
        {
            player.transform.position = startPoint.transform.position;
        }

        // --- ī�޶� ���, y ��ǥ �缳�� ---
        GameObject newBoundsObject = GameObject.FindWithTag("CameraBound");
        if (newBoundsObject != null)
        {
            Collider2D newBoundsCollider = newBoundsObject.GetComponent<Collider2D>();
            CinemachineCamera vcam = FindFirstObjectByType<CinemachineCamera>();
            if (vcam != null)
            {
                // ���� ī�޶��� Confiner 2D�� ���ο� ��踦 �������ش�.
                CinemachineConfiner2D confiner = vcam.GetComponent<CinemachineConfiner2D>();
                if (confiner != null)
                {
                    confiner.BoundingShape2D = newBoundsCollider;
                    // ĳ�ø� ��ȿȭ�Ͽ� ���ο� ��踦 ��� ����
                    confiner.InvalidateBoundingShapeCache();
                    Debug.Log("ī�޶� ��� �缳��");
                }
                vcam.OnTargetObjectWarped(player.transform, new Vector3(0, 0, -10));
                Debug.Log("ī�޶� Y ��ǥ �缳��");
            }
        }
    }


    // �������� �޴� ���� �޼���
    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth < 0) CurrentHealth = 0;

        // UI ������Ʈ ��û
        UIManager.Instance.UpdateHP(PlayerStats.Instance.MaxHP, CurrentHealth);

        if (CurrentHealth <= 0)
        {
            // Die(); // ��� ó�� ���� (���߿� ����)
            Debug.Log("�÷��̾� ���!");
        }
    }

    // ������ ����ϴ� ���� �޼���
    public void UseMana(float amount)
    {
        CurrentMana -= amount;
        if (CurrentMana < 0) CurrentMana = 0;

        // UI ������Ʈ ��û
        UIManager.Instance.UpdateMP(PlayerStats.Instance.MaxMP, CurrentMana);
    }
}