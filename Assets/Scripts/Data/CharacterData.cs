using System.Collections.Generic;

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
    public int RequiredEXP = 1000;

    public float baseAtk = 10f;
    public float baseDef = 10f;
    public float MoveSpeed = 3f;

    public float MaxHP = 100f;
    public float MaxMP = 100f;

    // 재화 정보
    public int Gold = 0;

    // 인벤토리 정보
    public List<SavedItem> inventoryItems = new List<SavedItem>();
    public List<string> equippedItemIDs = new List<string>();
}
