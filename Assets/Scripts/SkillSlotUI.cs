using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillSlotUI : MonoBehaviour, IDropHandler
{
    public int slotIndex; // �ν����Ϳ��� 0~13���� ����

    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownImage;

    private SkillManager skillManager;

    private void Start()
    {
        // �Ŵ����� ã�� ����
        skillManager = Player.Instance.GetComponent<SkillManager>();

        // SkillManager�� �̺�Ʈ�� UpdateSlot �Լ��� ����
        if (skillManager != null)
        {
            skillManager.OnSkillSlotChanged += HandleSkillSlotChanged;
        }

        // �ʱ� ������ ���� ������Ʈ
        UpdateSlot(slotIndex, skillManager.AssignedSkills[slotIndex]);
    }

    private void Update()
    {
        // ��Ÿ�� UI ������Ʈ
        if (skillManager != null)
        {
            float progress = skillManager.GetCooldownProgress(slotIndex);
            cooldownImage.fillAmount = progress;
        }
    }
    // ������Ʈ�� �ı��� �� �̺�Ʈ ������ ����
    private void OnDestroy()
    {
        if (skillManager != null)
        {
            skillManager.OnSkillSlotChanged -= HandleSkillSlotChanged;
        }
    }
    // ����� �޾��� �� ȣ��
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        SkillIconDraggable draggableIcon = droppedObject.GetComponent<SkillIconDraggable>();

        if (draggableIcon != null)
        {
            // SkillManager�� ��ų �Ҵ��� ��û
            skillManager.AssignSkill(this.slotIndex, draggableIcon.skillData);
        }
    }

    // skillManager���� OnSkillSlotChanged �̺�Ʈ�� �߻��ϸ� ȣ��� �Լ�
    private void HandleSkillSlotChanged(int updatedSlotIndex, SkillData skillData)
    {
        // ����� ������ �ٷ� �� �ڽ��� ����, �� �������� ������Ʈ
        if (this.slotIndex == updatedSlotIndex)
        {
            UpdateSlot(updatedSlotIndex, skillData);
        }
    }

    // SkillManager�� ȣ���Ͽ� UI�� ������Ʈ
    public void UpdateSlot(int index, SkillData skillData)
    {
        Debug.Log("UpdateSlot ȣ��");

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