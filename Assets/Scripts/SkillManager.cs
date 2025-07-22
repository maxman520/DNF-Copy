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

        // 이 스킬이 이미 다른 슬롯에 할당되어 있는지 확인
        int oldSlotIndex = -1;
        for (int i = 0; i < SKILL_SLOT_COUNT; i++)
        {
            if (AssignedSkills[i] == skillData)
            {
                oldSlotIndex = i;
                break;
            }
        }
         
        float coolTmp = 0;// 예전 슬롯의 쿨타임 기억용 변수

        // 만약 이미 다른 슬롯에 있었다면
        if (oldSlotIndex != -1)
        {
            SkillData skillInNewSlot = AssignedSkills[slotIndex];
            coolTmp = coolTimers[oldSlotIndex];

            // 할당하려는 슬롯에 스킬이 존재하지 않음
            if (skillInNewSlot == null)
            { 
                // 예전 슬롯을 비움
                AssignedSkills[oldSlotIndex] = null;
                coolTimers[oldSlotIndex] = 0;
            }
            else // 할당하려는 슬롯에 스킬이 이미 존재
            {
                // 할당하려는 슬롯의 스킬과 예전 슬롯을 스왑
                AssignedSkills[oldSlotIndex] = skillInNewSlot;
                coolTimers[oldSlotIndex] = coolTimers[slotIndex];
            }

            OnSkillSlotChanged?.Invoke(oldSlotIndex, skillInNewSlot);
        }

        // 새로운 슬롯에 스킬 할당
        AssignedSkills[slotIndex] = skillData;
        coolTimers[slotIndex] = coolTmp;
        Debug.Log($"{slotIndex}번 슬롯에 '{skillData.skillName}' 스킬 할당됨.");

        // UI 업데이트 신호 보내기 (이벤트 호출)
        OnSkillSlotChanged?.Invoke(slotIndex, skillData);
    }


    // 스킬 사용 가능 여부를 체크하는 함수
    public bool IsSkillReady(int slotIndex, out SkillData skill)
    {
        skill = null;
        if (slotIndex < 0 || slotIndex >= SKILL_SLOT_COUNT) return false;

        skill = AssignedSkills[slotIndex];

        // < 조건 체크 >
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

        // 모든 조건을 통과하면 true 반환
        return true;
    }

    // 스킬의 쿨타임을 시작시키는 함수
    public void StartCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SKILL_SLOT_COUNT) return;

        SkillData skill = AssignedSkills[slotIndex];
        if (skill != null)
        {
            coolTimers[slotIndex] = skill.coolTime;
        }
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