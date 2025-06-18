using UnityEngine;
using UnityEngine.UI; // Image, Slider ���� ����ϱ� ���� �ʼ�

public class UIManager : Singleton<UIManager>
{

    [Header("HP Gauge")]
    [SerializeField] private Image hpGauge;

    [Header("MP Gauge")]
    [SerializeField] private Image mpGauge;

    // HP ������ ������Ʈ
    public void UpdateHP(float maxHealth, float currentHealth)
    {
        if (hpGauge != null)
        {
            hpGauge.fillAmount = currentHealth / maxHealth;
        }
    }

    // MP ������ ������Ʈ
    public void UpdateMP(float maxMana, float currentMana)
    {
        if (mpGauge != null)
        {
            mpGauge.fillAmount = currentMana / maxMana;
        }
    }
}