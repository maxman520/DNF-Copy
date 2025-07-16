using UnityEngine;
using System;
using Cysharp.Threading.Tasks.Triggers;

public class SkillManager : MonoBehaviour
{
    private const int SKILL_SLOT_COUNT = 14;

    // ���� ���Կ� �Ҵ�� ��ų �����͵�
    public SkillData[] AssignedSkills { get; private set; } = new SkillData[SKILL_SLOT_COUNT];


    // ��ų ������ ����Ǿ����� �˸��� �̺�Ʈ
    public event Action<int, SkillData> OnSkillSlotChanged;

    // �� ��ų�� ���� ��Ÿ��
    private float[] coolTimers = new float[SKILL_SLOT_COUNT];

    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        // ��� ��ų ��Ÿ�� ����
        for (int i = 0; i < SKILL_SLOT_COUNT; i++)
        {
            if (coolTimers[i] > 0)
            {
                coolTimers[i] -= Time.deltaTime;
            }
        }
    }

    // UI���� ��ų�� ���Կ� �Ҵ��� �� ȣ��
    public void AssignSkill(int slotIndex, SkillData skillData)
    {
        if (slotIndex < 0 || slotIndex >= SKILL_SLOT_COUNT) return;

        // �� ��ų�� �̹� �ٸ� ���Կ� �Ҵ�Ǿ� �ִ��� Ȯ��
        int oldSlotIndex = -1;
        for (int i = 0; i < SKILL_SLOT_COUNT; i++)
        {
            if (AssignedSkills[i] == skillData)
            {
                oldSlotIndex = i;
                break;
            }
        }
         
        float coolTmp = 0;// ���� ������ ��Ÿ�� ���� ����

        // ���� �̹� �ٸ� ���Կ� �־��ٸ�
        if (oldSlotIndex != -1)
        {
            SkillData skillInNewSlot = AssignedSkills[slotIndex];
            coolTmp = coolTimers[oldSlotIndex];

            // �Ҵ��Ϸ��� ���Կ� ��ų�� �������� ����
            if (skillInNewSlot == null)
            { 
                // ���� ������ ���
                AssignedSkills[oldSlotIndex] = null;
                coolTimers[oldSlotIndex] = 0;
            }
            else // �Ҵ��Ϸ��� ���Կ� ��ų�� �̹� ����
            {
                // �Ҵ��Ϸ��� ������ ��ų�� ���� ������ ����
                AssignedSkills[oldSlotIndex] = skillInNewSlot;
                coolTimers[oldSlotIndex] = coolTimers[slotIndex];
            }

            OnSkillSlotChanged?.Invoke(oldSlotIndex, skillInNewSlot);
        }

        // ���ο� ���Կ� ��ų �Ҵ�
        AssignedSkills[slotIndex] = skillData;
        coolTimers[slotIndex] = coolTmp;
        Debug.Log($"{slotIndex}�� ���Կ� '{skillData.skillName}' ��ų �Ҵ��.");

        // UI ������Ʈ ��ȣ ������ (�̺�Ʈ ȣ��)
        OnSkillSlotChanged?.Invoke(slotIndex, skillData);
    }


    // ��ų ��� ���� ���θ� üũ�ϴ� �Լ�
    public bool IsSkillReady(int slotIndex, out SkillData skill)
    {
        skill = null;
        if (slotIndex < 0 || slotIndex >= SKILL_SLOT_COUNT) return false;

        skill = AssignedSkills[slotIndex];

        // < ���� üũ >
        // �ش� ���Կ� ��ų �Ҵ� üũ
        if (skill == null)
        {
            Debug.Log($"{slotIndex}�� ���Կ� �Ҵ�� ��ų �� �����ϴ�.");
            return false;
        }
        // ��Ÿ�� üũ
        if (coolTimers[slotIndex] > 0)
        {
            Debug.Log($"'{skill.skillName}' ��Ÿ�� ({coolTimers[slotIndex]:F1}�� ����)");
            return false;
        }
        // ���� üũ
        if (player.CurrentMP < skill.manaCost) return false;

        // ��� ������ ����ϸ� true ��ȯ
        return true;
    }

    // ��ų�� ��Ÿ���� ���۽�Ű�� �Լ�
    public void StartCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SKILL_SLOT_COUNT) return;

        SkillData skill = AssignedSkills[slotIndex];
        if (skill != null)
        {
            coolTimers[slotIndex] = skill.coolTime;
        }
    }

    // ��ų ���� UI���� ��Ÿ�� ������ �������� ���� �Լ�
    public float GetCooldownProgress(int slotIndex)
    {
        if (AssignedSkills[slotIndex] == null || AssignedSkills[slotIndex].coolTime <= 0)
        {
            return 0;
        }
        return coolTimers[slotIndex] / AssignedSkills[slotIndex].coolTime;
    }
}