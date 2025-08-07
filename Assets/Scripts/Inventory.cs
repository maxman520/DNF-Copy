using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // --- 인벤토리 데이터 ---
    public ItemData[] Items;
    public Dictionary<EquipmentType, EquipmentData> EquippedItems = new Dictionary<EquipmentType, EquipmentData>();

    // --- 재화 ---
    public int Gold = 0;
    public int Coin = 0;

    // --- 이벤트 ---
    public event System.Action OnInventoryChanged; // 인벤토리에 변화가 생길 때 UI를 업데이트하기 위한 이벤트

    public void Initialize()
    {
        // 모든 장비 슬롯을 null로 초기화
        foreach (EquipmentType type in System.Enum.GetValues(typeof(EquipmentType)))
        {
            EquippedItems.Add(type, null);
        }
    }

    // 빈 슬롯을 찾아 아이템 추가
    public void AddItem(ItemData itemToAdd)
    {
        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i] == null)
            {
                Items[i] = itemToAdd;
                OnInventoryChanged?.Invoke();
                return; // 아이템 추가 후 함수 종료
            }
        }
        Debug.LogWarning("인벤토리가 가득 찼습니다.");
    }

    // 특정 아이템을 찾아 제거
    public void RemoveItem(ItemData itemToRemove)
    {
        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i] == itemToRemove)
            {
                Items[i] = null;
                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }

    // 인덱스를 명확히 지정해서 장착
    public void Equip(int index)
    {
        if (index < 0 || index >= Items.Length) return;
        var equip = Items[index] as EquipmentData;
        if (equip == null) return;

        EquipmentType type = equip.EquipType;
        var currentlyEquipped = EquippedItems.ContainsKey(type) ? EquippedItems[type] : null;

        // 선택 슬롯 비우기
        Items[index] = null;

        var pe = GetComponent<PlayerEquipment>();
        if (currentlyEquipped != null)
        {
            // 기존 장비의 능력치/외형 해제
            pe?.UnEquip(type);
        }

        // 새 장비 장착
        EquippedItems[type] = equip;
        pe?.Equip(equip);

        // 기존 장비는 인벤토리에 반환
        if (currentlyEquipped != null)
        {
            AddItem(currentlyEquipped);
        }

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

    // 두 인덱스의 아이템을 교환
    public void SwapItems(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= Items.Length || indexB < 0 || indexB >= Items.Length) return;

        (Items[indexA], Items[indexB]) = (Items[indexB], Items[indexA]);

        OnInventoryChanged?.Invoke(); // UI 업데이트
    }

    public void AddGold(int amount)
    {
        Gold += Mathf.Max(0, amount);
        OnInventoryChanged?.Invoke();
    }
}