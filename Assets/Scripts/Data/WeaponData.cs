using UnityEngine;
using UnityEngine.U2D.Animation;

public enum WeaponKind
{
    None,
    Blunt,   // 둔기
    Katana,  // 카타나
    Sword,   // 소검, 대검
    BeamSword // 광검
}

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Data/Weapon Data")]
public class WeaponData : EquipmentData
{
    [Header("무기 고유 정보")]
    public SpriteLibraryAsset WeaponSpriteLibrary;
    public WeaponKind WeaponKind = WeaponKind.None;

    // 생성자에서 장비 타입을 무기로 고정
    public WeaponData()
    {
        EquipType = EquipmentType.Weapon;
    }
}
