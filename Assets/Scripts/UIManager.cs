using UnityEngine;
using UnityEngine.UI; // Image, Slider 등을 사용하기 위해 필수

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HP Gauge")]
    [SerializeField] private Image hpGauge;

    [Header("MP Gauge")]
    [SerializeField] private Image mpGauge;

    private void Awake()
    {
        // 싱글턴 인스턴스 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 있다면 이 오브젝트는 파괴
        }
    }

    // HP 게이지를 업데이트하는 공용 메서드
    public void UpdateHP(float maxHealth, float currentHealth)
    {
        if (hpGauge != null)
        {
            hpGauge.fillAmount = currentHealth / maxHealth;
        }
    }

    // MP 게이지를 업데이트하는 공용 메서드
    public void UpdateMP(float maxMana, float currentMana)
    {
        if (mpGauge != null)
        {
            mpGauge.fillAmount = currentMana / maxMana;
        }
    }
}