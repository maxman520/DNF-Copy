using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용 시

public class SkillShopUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject skillShopPanel;
    [SerializeField] private Transform skillIconParent; // 스킬 아이콘들이 생성될 부모 (Content 오브젝트)
    [SerializeField] private GameObject skillIconPrefab;

    [Header("설명창 참조")]
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;
    [SerializeField] private GameObject descriptionPanel;

    [Header("배울 수 있는 스킬 목록")]
    [SerializeField] private SkillData[] availableSkills;

    void Start()
    {
        InitializeShop();
        descriptionPanel.SetActive(false); // 시작시엔 스킬 설명창은 꺼둠
    }

    // 스킬샵 UI를 토글하는 함수
    public void ToggleShop()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    // 스킬샵 초기화
    private void InitializeShop()
    {
        // 배울 수 있는 모든 스킬에 대해 아이콘을 생성
        foreach (var skill in availableSkills)
        {
            GameObject iconObj = Instantiate(skillIconPrefab, skillIconParent);
            iconObj.GetComponent<Image>().sprite = skill.skillIcon;
            iconObj.GetComponent<SkillIconDraggable>().skillData = skill;
        }
    }

    // 설명창 보이기
    public void ShowDescription(SkillData skill)
    {
        if (skill == null) return;
        skillNameText.text = skill.skillName;
        skillDescriptionText.text = skill.description;
        descriptionPanel.SetActive(true);
    }

    // 설명창 숨기기
    public void HideDescription()
    {
        descriptionPanel.SetActive(false);
    }
}
