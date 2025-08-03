using UnityEngine;



// 모든 아이템 데이터의 기반이 될 클래스
[CreateAssetMenu(fileName = "New Item Data", menuName = "Data/Item Data")]
public class ItemData : ScriptableObject
{
    public enum Kind {
    Equipment,
    Consume
    }

    [Header("기본 정보")]
    public string ItemName;
    public Sprite ItemIcon;
    public Kind ItemKind;
    [TextArea]
    public string ItemDescription;
}
