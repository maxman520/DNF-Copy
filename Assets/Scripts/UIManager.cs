using UnityEngine;
using UnityEngine.UI; // Image, Slider 등을 사용하기 위해 필수

public class UIManager : Singleton<UIManager>
{

    [Header("HP Gauge")]
    [SerializeField] private Image hpGauge;

    [Header("MP Gauge")]
    [SerializeField] private Image mpGauge;

    // HP 게이지 업데이트
    public void UpdateHP(float maxHealth, float currentHealth)
    {
        if (hpGauge != null)
        {
            hpGauge.fillAmount = currentHealth / maxHealth;
        }
    }

    // MP 게이지 업데이트
    public void UpdateMP(float maxMana, float currentMana)
    {
        if (mpGauge != null)
        {
            mpGauge.fillAmount = currentMana / maxMana;
        }
    }
}