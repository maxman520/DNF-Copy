using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 외부에서 현재 값만 읽을 수 있도록 private set 사용
    public float currentHealth { get; private set; }
    public float currentMana { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않게 설정
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 게임 시작 시 스탯 초기화
        InitializePlayerState();
    }

    public void InitializePlayerState()
    {
        currentHealth = PlayerStats.Instance.MaxHealth;
        currentMana = PlayerStats.Instance.MaxMana;

        // UI 매니저에게 초기 UI 업데이트 요청
        UIManager.Instance.UpdateHP(PlayerStats.Instance.MaxHealth, currentHealth);
        UIManager.Instance.UpdateMP(PlayerStats.Instance.MaxMana, currentMana);
    }

    // 데미지를 받는 공용 메서드
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // UI 업데이트 요청
        UIManager.Instance.UpdateHP(PlayerStats.Instance.MaxHealth, currentHealth);

        if (currentHealth <= 0)
        {
            // Die(); // 사망 처리 로직 (나중에 구현)
            Debug.Log("플레이어 사망!");
        }
    }

    // 마나를 사용하는 공용 메서드
    public void UseMana(float amount)
    {
        currentMana -= amount;
        if (currentMana < 0) currentMana = 0;

        // UI 업데이트 요청
        UIManager.Instance.UpdateMP(PlayerStats.Instance.MaxMana, currentMana);
    }
}