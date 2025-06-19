using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public float currentHealth { get; private set; }
    public float currentMana { get; private set; }

    private void Start()
    {
        // ���� ���� �� ���� �ʱ�ȭ
        InitializePlayerState();
    }
    public void InitializePlayerState()
    {
        currentHealth = PlayerStats.Instance.MaxHP;
        currentMana = PlayerStats.Instance.MaxMP;

        // UI �Ŵ������� �ʱ� UI ������Ʈ ��û
        UIManager.Instance.UpdateHP(PlayerStats.Instance.MaxHP, currentHealth);
        UIManager.Instance.UpdateMP(PlayerStats.Instance.MaxMP, currentMana);
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
            player.ChangeState(player.dungeonState);
        }
        else if (scene.name == "Town_Scene")
        {
            Debug.Log("���� �� �ε�");
            player.ChangeState(player.townState);
            // ������ �̵��� ü��, ���� ȸ��
            currentHealth = PlayerStats.Instance.MaxHP;
            currentMana = PlayerStats.Instance.MaxMP;
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
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // UI ������Ʈ ��û
        UIManager.Instance.UpdateHP(PlayerStats.Instance.MaxHP, currentHealth);

        if (currentHealth <= 0)
        {
            // Die(); // ��� ó�� ���� (���߿� ����)
            Debug.Log("�÷��̾� ���!");
        }
    }

    // ������ ����ϴ� ���� �޼���
    public void UseMana(float amount)
    {
        currentMana -= amount;
        if (currentMana < 0) currentMana = 0;

        // UI ������Ʈ ��û
        UIManager.Instance.UpdateMP(PlayerStats.Instance.MaxMP, currentMana);
    }
}