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

    public void Equip(EquipmentData itemToEquip)
    {
        EquipmentType type = itemToEquip.EquipType;

        // 1. 이미 해당 부위에 다른 아이템을 장착 중인지 확인
        if (EquippedItems[type] != null)
        {
            UnEquip(type); // 기존 아이템을 인벤토리로 돌려보냄 (장비 교체)
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