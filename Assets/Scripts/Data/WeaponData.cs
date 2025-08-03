using UnityEngine;
using UnityEngine.U2D.Animation;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Data/Weapon Data")]
public class WeaponData : EquipmentData
{
    [Header("무기 고유 정보")]
    public SpriteLibraryAsset WeaponSpriteLibrary;

    // 생성자에서 장비 타입을 무기로 고정
    public WeaponData()
    {
        EquipType = EquipmentType.Weapon;
    }
}
