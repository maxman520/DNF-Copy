using UnityEngine;
using System;

public class SkillManager : MonoBehaviour
{
    private const int SKILL_SLOT_COUNT = 14;

    // 현재 슬롯에 할당된 스킬 데이터들
    public SkillData[] AssignedSkills { get; private set; } = new SkillData[SKILL_SLOT_COUNT];


    // 스킬 슬롯이 변경되었음을 알리는 이벤트
    public event Action<int, SkillData> OnSkillSlotChanged;

    // 각 스킬의 남은 쿨타임
    private float[] coolTimers = new float[SKILL_SLOT_COUNT];

    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        // 모든 스킬 쿨타임 감소
        for (int i = 0; i < SKILL_SLOT_COUNT; i++)
        {
            if (coolTimers[i] > 0)
            {
                coolTimers[i] -= Time.deltaTime;
            }
        }
    }

    // UI에서 스킬을 슬롯에 할당할 때 호출
    public void AssignSkill(int slotIndex, SkillData skillData)
    {
        if (slotIndex < 0 || slotIndex >= SKILL_SLOT_COUNT) return;

        AssignedSkills[slotIndex] = skillData;
        coolTimers[slotIndex] = 0; // 슬롯 변경 시 쿨타임 초기화
        Debug.Log($"{slotIndex}번 슬롯에 '{skillData.skillName}' 스킬 할당됨.");

        // UI 업데이트 신호 보내기 (이벤트 호출)
        OnSkillSlotChanged?.Invoke(slotIndex, skillData);
    }

    // 커맨드 Excute에서 호출
    public bool TryExecuteSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SKILL_SLOT_COUNT) return false;

        SkillData skill = AssignedSkills[slotIndex];

        // 해당 슬롯에 스킬 할당 체크
        if (skill == null)
        {
            Debug.Log($"{slotIndex}번 슬롯에 할당된 스킬 이 없습니다.");
            return false;
        }

        // 쿨타임 체크
        if (coolTimers[slotIndex] > 0)
        {
            Debug.Log($"'{skill.skillName}' 쿨타임 ({coolTimers[slotIndex]:F1}초 남음)");
            return false;
        }

        // 마나 체크
        if (player.CurrentMP < skill.manaCost) return false;

        // 공격을 할 수 있는 상태인지
        if (!player.CanAttack) return false;

        // 모든 조건을 만족하면 스킬 실행
        Debug.Log($"'{skill.skillName}' 스킬 시전");

        // 스킬 실행
        player.Anim.Play(skill.animName);

        // 쿨타임 시작
        coolTimers[slotIndex] = skill.coolTime;

        return true;
    }

    // 스킬 슬롯 UI에서 쿨타임 정보를 가져가기 위한 함수
    public float GetCooldownProgress(int slotIndex)
    {
        if (AssignedSkills[slotIndex] == null || AssignedSkills[slotIndex].coolTime <= 0)
        {
            return 0;
        }
        return coolTimers[slotIndex] / AssignedSkills[slotIndex].coolTime;
    }
}