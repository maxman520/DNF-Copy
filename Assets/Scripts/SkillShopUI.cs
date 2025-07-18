using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro ��� ��

public class SkillShopUI : MonoBehaviour
{
    [Header("UI ����")]
    [SerializeField] private GameObject skillShopPanel;
    [SerializeField] private Transform skillIconParent; // ��ų �����ܵ��� ������ �θ� (Content ������Ʈ)
    [SerializeField] private GameObject skillIconPrefab;

    [Header("����â ����")]
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;
    [SerializeField] private GameObject descriptionPanel;

    [Header("��� �� �ִ� ��ų ���")]
    [SerializeField] private SkillData[] availableSkills;

    void Start()
    {
        InitializeShop();
        descriptionPanel.SetActive(false); // ���۽ÿ� ��ų ����â�� ����
    }

    // ��ų�� UI�� ����ϴ� �Լ�
    public void ToggleShop()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    // ��ų�� �ʱ�ȭ
    private void InitializeShop()
    {
        // ��� �� �ִ� ��� ��ų�� ���� �������� ����
        foreach (var skill in availableSkills)
        {
            GameObject iconObj = Instantiate(skillIconPrefab, skillIconParent);
            iconObj.GetComponent<Image>().sprite = skill.skillIcon;
            iconObj.GetComponent<SkillIconDraggable>().skillData = skill;
        }
    }

    // ����â ���̱�
    public void ShowDescription(SkillData skill)
    {
        if (skill == null) return;
        skillNameText.text = skill.skillName;
        skillDescriptionText.text = skill.description;
        descriptionPanel.SetActive(true);
    }

    // ����â �����
    public void HideDescription()
    {
        descriptionPanel.SetActive(false);
    }
}
