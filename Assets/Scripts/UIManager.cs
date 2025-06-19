using UnityEngine;
using UnityEngine.UI; // Image, Slider ���� ����ϱ� ���� �ʼ�

public class UIManager : Singleton<UIManager>
{

    [Header("HP Gauge")]
    [SerializeField] private Image hpGauge;

    [Header("MP Gauge")]
    [SerializeField] private Image mpGauge;

    // HP ������ ������Ʈ
    public void UpdateHP(float maxHP, float currentHP)
    {
        if (hpGauge != null)
        {
            hpGauge.fillAmount = currentHP / maxHP;
        }
    }

    // MP ������ ������Ʈ
    public void UpdateMP(float maxMP, float currentMP)
    {
        if (mpGauge != null)
        {
            mpGauge.fillAmount = currentMP / maxMP;
        }
    }
}