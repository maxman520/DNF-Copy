[System.Serializable]
public class CharacterData
{
    // 기본 정보
    public string CharacterName;
    public string JobName;
    public string PreviewPrefabName; // 미리보기 프리팹의 이름

    // 스탯 정보
    public int Level = 1;
    public int CurrentEXP = 0;
    public int RequiredEXP = 100;

    public float Atk = 10f;
    public float Def = 10f;
    public float MoveSpeed = 3f;

    public float MaxHP = 100f;

    public float MaxMP = 100f;
}
