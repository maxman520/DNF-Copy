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
}