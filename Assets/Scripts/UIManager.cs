using UnityEngine;
using UnityEngine.UI; // Image, Slider ���� ����ϱ� ���� �ʼ�

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HP Gauge")]
    [SerializeField] private Image hpGauge;

    [Header("MP Gauge")]
    [SerializeField] private Image mpGauge;

    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // �̹� �ν��Ͻ��� �ִٸ� �� ������Ʈ�� �ı�
        }
    }

    // HP �������� ������Ʈ�ϴ� ���� �޼���
    public void UpdateHP(float maxHealth, float currentHealth)
    {
        if (hpGauge != null)
        {
            hpGauge.fillAmount = currentHealth / maxHealth;
        }
    }

    // MP �������� ������Ʈ�ϴ� ���� �޼���
    public void UpdateMP(float maxMana, float currentMana)
    {
        if (mpGauge != null)
        {
            mpGauge.fillAmount = currentMana / maxMana;
        }
    }
}