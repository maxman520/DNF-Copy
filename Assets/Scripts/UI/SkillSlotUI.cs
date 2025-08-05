using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillSlotUI : MonoBehaviour, IDropHandler
{
    public int slotIndex; // 인스펙터에서 0~13까지 설정

    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownImage;

    private SkillManager skillManager;

    private void Start()
    {
        // 매니저를 찾아 연결
        skillManager = Player.Instance.GetComponent<SkillManager>();

        // OnSkillSlotChanged 이벤트에 UpdateSlot 함수를 구독
        if (skillManager != null)
        {
            skillManager.OnSkillSlotChanged += HandleSkillSlotChanged;
        }

        // 초기 아이콘 상태 업데이트
        UpdateSlot(slotIndex, skillManager.AssignedSkills[slotIndex]);
    }

    private void Update()
    {
        // 쿨타임 UI 업데이트
        if (skillManager != null)
        {
            float progress = skillManager.GetCooldownProgress(slotIndex);
            cooldownImage.fillAmount = progress;
        }
    }
    // 오브젝트가 파괴될 때 이벤트 구독을 해제
    private void OnDestroy()
    {
        if (skillManager != null)
        {
            skillManager.OnSkillSlotChanged -= HandleSkillSlotChanged;
        }
    }
    // 드롭을 받았을 때 호출
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        SkillIconDraggable draggableIcon = droppedObject.GetComponent<SkillIconDraggable>();

        if (draggableIcon != null)
        {
            // SkillManager에 스킬 할당을 요청
            skillManager.AssignSkill(this.slotIndex, draggableIcon.skillData);
        }
    }

    // OnSkillSlotChanged 이벤트 처리
    private void HandleSkillSlotChanged(int updatedSlotIndex, SkillData skillData)
    {
        // 변경된 슬롯이 바로 나 자신일 때만, 내 아이콘을 업데이트
        if (this.slotIndex == updatedSlotIndex)
        {
            UpdateSlot(updatedSlotIndex, skillData);
        }
    }

    // SkillManager가 호출하여 슬롯 UI를 업데이트
    public void UpdateSlot(int index, SkillData skillData)
    {
        if (skillData != null)
        {
            iconImage.sprite = skillData.skillIcon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }
}