using UnityEngine;

[CreateAssetMenu(fileName = "New Skill Data", menuName = "Data/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    public string skillName;
    [TextArea] public string description;
    public Sprite skillIcon;

    [Header("핵심 능력치")]
    public float coolTime = 1f;
    public float manaCost = 10f;

    public GameObject skillEffectPrefab;
    public string animName;

    [Header("보이스 설정")]
    [Tooltip("스킬 시전 시 재생할 보이스 SFX 키들(랜덤 선택)")]
    public string[] voiceKeys;
    [Tooltip("보이스 재생 지연(초). 0이면 즉시 재생")]
    public float voiceDelay = 0f;
}
