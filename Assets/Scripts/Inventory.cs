using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // --- 인벤토리 데이터 ---
    public List<ItemData> items = new List<ItemData>();
    public Dictionary<EquipmentType, EquipmentData> EquippedItems = new Dictionary<EquipmentType, EquipmentData>();

    // --- 재화 ---
    public int Gold = 0;
    public int Coin = 0;

    // --- 이벤트 ---
    public event System.Action OnInventoryChanged; // 인벤토리에 변화가 생길 때 UI를 업데이트하기 위한 이벤트

    private void Awake()
    {
        // 모든 장비 슬롯을 null로 초기화
        foreach (EquipmentType type in System.Enum.GetValues(typeof(EquipmentType)))
        {
            EquippedItems.Add(type, null);
        }
    }

    public void AddItem(ItemData itemToAdd)
    {
        items.Add(itemToAdd);
        OnInventoryChanged?.Invoke(); // 인벤토리 변경 이벤트 호출
    }

    public void RemoveItem(ItemData itemToRemove)
    {
        items.Remove(itemToRemove);
        OnInventoryChanged?.Invoke();
    }

    public void Equip(EquipmentData itemToEquip)
    {
        EquipmentType type = itemToEquip.EquipType;

        // 1. 이미 해당 부위에 다른 아이템을 장착 중인지 확인
        if (EquippedItems[type] != null)
        {
            // 기존 아이템을 인벤토리로 돌려보냄 (장비 교체)
            UnEquip(type);
        }

        // 2. 인벤토리에서 새 아이템 제거
        RemoveItem(itemToEquip);

        // 3. 장비 슬롯에 새 아이템 등록
        EquippedItems[type] = itemToEquip;

        // 4. 실제 플레이어에게 능력치 및 외형 적용
        GetComponent<PlayerEquipment>()?.Equip(itemToEquip);

        OnInventoryChanged?.Invoke();
    }

    public void UnEquip(EquipmentType type)
    {
        EquipmentData itemToUnEquip = EquippedItems[type];
        if (itemToUnEquip == null) return;

        // 1. 플레이어에게 적용된 능력치 및 외형 먼저 해제
        GetComponent<PlayerEquipment>()?.UnEquip(type);

        // 2. 장비 슬롯에서 아이템 제거
        EquippedItems[type] = null;

        // 3. 인벤토리에 아이템 추가
        AddItem(itemToUnEquip);

        OnInventoryChanged?.Invoke();
    }
}
