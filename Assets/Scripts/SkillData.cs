using UnityEngine;

[CreateAssetMenu(fileName = "New Skill Data", menuName = "Data/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("�⺻ ����")]
    public string skillName;
    [TextArea] public string description;
    public Sprite skillIcon;

    [Header("�ٽ� �ɷ�ġ")]
    public float coolTime = 1f;
    public float manaCost = 10f;

    public GameObject skillEffectPrefab;
    public string animName;
}