using UnityEngine;
using UnityEngine.U2D.Animation;
using System.Collections.Generic;
using System.Linq;

// 각 장비 부위와 그에 해당하는 SpriteLibrary 컴포넌트를 연결하는 헬퍼 클래스
[System.Serializable]
public class EquipmentPart
{
    public EquipmentType EquipmentType;
    public SpriteLibrary SpriteLibraryComponent;
    public SpriteLibraryAsset DefaultSpriteLibraryAsset;
}

public class PlayerEquipment : MonoBehaviour
{
    [Header("장비 부위별 라이브러리 설정")]
    // 인스펙터에서 각 부위(무기, 헤어 등)의 SpriteLibrary 컴포넌트와 기본 에셋을 설정
    [SerializeField] private List<EquipmentPart> equipmentParts;

    private Player playerStats;
    private Inventory inventory;

    // 장비로 인해 추가된 총 스탯
    private int currentTotalAttack = 0;
    private int currentTotalDefense = 0;

    private void Awake()
    {
        playerStats = GetComponent<Player>();
        inventory = GetComponent<Inventory>();

        // 게임 시작 시 모든 장비 부위를 기본 에셋으로 초기화
        foreach (var part in equipmentParts)
        {
            if (part.SpriteLibraryComponent != null && part.DefaultSpriteLibraryAsset != null)
            {
                part.SpriteLibraryComponent.spriteLibraryAsset = part.DefaultSpriteLibraryAsset;
            }
        }
    }

    // Inventory.cs에서 호출
    public void Equip(EquipmentData itemToEquip)
    {
        // 1. 스탯 변경
        currentTotalAttack += itemToEquip.AttackPower;
        currentTotalDefense += itemToEquip.DefensePower;
        playerStats.UpdateEquipmentStats(currentTotalAttack, currentTotalDefense);

        // 2. 외형 변경
        EquipmentPart partToEquip = equipmentParts.FirstOrDefault(p => p.EquipmentType == itemToEquip.EquipType);
        if (partToEquip == null)
        {
            Debug.LogWarning($"{itemToEquip.EquipType}에 해당하는 장비 부위가 PlayerEquipment에 설정되지 않았음");
            return;
        }
        // 실제 장비 데이터 타입에 따라 분기 처리
        if (itemToEquip is WeaponData weaponData && weaponData.WeaponSpriteLibrary != null)
        {
            partToEquip.SpriteLibraryComponent.spriteLibraryAsset = weaponData.WeaponSpriteLibrary;
        }
        // 다른 부위 외형 변경 로직 추가...

        Debug.Log($"{itemToEquip.ItemName} 장착. 추가 공/방: {itemToEquip.AttackPower}/{itemToEquip.DefensePower}");
    }

    // Inventory.cs에서 호출
    public void UnEquip(EquipmentType typeToUnEquip)
    {
        // 장착 해제할 아이템 정보를 Inventory에서 가져옴
        EquipmentData itemToUnEquip = inventory.EquippedItems[typeToUnEquip];
        if (itemToUnEquip == null) return;

        // 1. 스탯 변경
        currentTotalAttack -= itemToUnEquip.AttackPower;
        currentTotalDefense -= itemToUnEquip.DefensePower;
        playerStats.UpdateEquipmentStats(currentTotalAttack, currentTotalDefense);

        // 2. 외형 변경
        EquipmentPart partToUnEquip = equipmentParts.FirstOrDefault(p => p.EquipmentType == typeToUnEquip);
        if (partToUnEquip == null) return;

        partToUnEquip.SpriteLibraryComponent.spriteLibraryAsset = partToUnEquip.DefaultSpriteLibraryAsset;

        Debug.Log($"{itemToUnEquip.ItemName} 해제. 감소 공/방: {itemToUnEquip.AttackPower}/{itemToUnEquip.DefensePower}");
    }
}
