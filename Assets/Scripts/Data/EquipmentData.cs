using UnityEngine;

// 장비 종류를 정의하는 열거형
public enum EquipmentType
{
    Shoulder,
    Body,
    Pants,
    Belt,
    Shoes,
    Weapon,
    Title,
    Wrist,
    Necklace,
    Ring
}

[CreateAssetMenu(fileName = "New Equipment Data", menuName = "Data/Equipment Data")]
public class EquipmentData : ItemData
{
    [Header("장비 정보")]
    public EquipmentType EquipType;

    [Header("공통 스탯")]
    public int AttackPower; // 공격력
    public int DefensePower; // 방어력
}
