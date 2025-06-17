using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // �ܺο��� ���� ���� ���� �� �ֵ��� private set ���
    public float currentHealth { get; private set; }
    public float currentMana { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ���� �ٲ� �ı����� �ʰ� ����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // ���� ���� �� ���� �ʱ�ȭ
        InitializePlayerState();
    }

    public void InitializePlayerState()
    {
        currentHealth = PlayerStats.Instance.MaxHealth;
        currentMana = PlayerStats.Instance.MaxMana;

        // UI �Ŵ������� �ʱ� UI ������Ʈ ��û
        UIManager.Instance.UpdateHP(PlayerStats.Instance.MaxHealth, currentHealth);
        UIManager.Instance.UpdateMP(PlayerStats.Instance.MaxMana, currentMana);
    }

    // �������� �޴� ���� �޼���
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // UI ������Ʈ ��û
        UIManager.Instance.UpdateHP(PlayerStats.Instance.MaxHealth, currentHealth);

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
        UIManager.Instance.UpdateMP(PlayerStats.Instance.MaxMana, currentMana);
    }
}