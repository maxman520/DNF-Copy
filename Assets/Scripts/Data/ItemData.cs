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
    public Sprite DropSprite; // 월드 드랍 시 표시될 스프라이트 (없으면 ItemIcon 사용)
    public Kind ItemKind;
    [TextArea]
    public string ItemDescription;
}
