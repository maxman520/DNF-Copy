using UnityEngine;
using System;

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

        AssignedSkills[slotIndex] = skillData;
        coolTimers[slotIndex] = 0; // ���� ���� �� ��Ÿ�� �ʱ�ȭ
        Debug.Log($"{slotIndex}�� ���Կ� '{skillData.skillName}' ��ų �Ҵ��.");

        // UI ������Ʈ ��ȣ ������ (�̺�Ʈ ȣ��)
        OnSkillSlotChanged?.Invoke(slotIndex, skillData);
    }

    // Ŀ�ǵ� Excute���� ȣ��
    public bool TryExecuteSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SKILL_SLOT_COUNT) return false;

        SkillData skill = AssignedSkills[slotIndex];

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

        // ������ �� �� �ִ� ��������
        if (!player.CanAttack) return false;

        // ��� ������ �����ϸ� ��ų ����
        Debug.Log($"'{skill.skillName}' ��ų ����");

        // ��ų ����
        player.Anim.Play(skill.animName);

        // ��Ÿ�� ����
        coolTimers[slotIndex] = skill.coolTime;

        return true;
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