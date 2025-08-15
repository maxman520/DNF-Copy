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

public class Inventory : MonoBehaviour
{
    [Header("인벤토리 크기 설정 (반드시 인벤토리 아이템 슬롯 개수와 같아야 함)")]
    [SerializeField] private int inventorySize = 16;

    [Header("장비 부위별 스프라이트 라이브러리 설정")]
    [SerializeField] private List<EquipmentPart> equipmentParts;

    // --- 인벤토리 데이터 ---
    public SavedItem[] Items;
    public string[] QuickSlotItemIDs = new string[6];
    public Dictionary<EquipmentType, EquipmentData> EquippedItems = new Dictionary<EquipmentType, EquipmentData>();

    // --- 재화 ---
    public int Gold = 0;
    public int Coin = 0;

    // --- 이벤트 ---
    public event System.Action OnInventoryChanged;

    // --- 내부 참조 ---
    private Player playerStats;
    private int currentTotalAttack = 0;
    private int currentTotalDefense = 0;

    private void Awake()
    {
        // 아이템 칸 초기화
        Items = new SavedItem[inventorySize];
        for (int i = 0; i < inventorySize; i++)
        {
            // 각 슬롯에 비어있는 new SavedItem 인스턴스를 생성하여 할당
            Items[i] = new SavedItem { ItemID = "", Quantity = 0 };
        }

        for (int i = 0; i < QuickSlotItemIDs.Length; i++)
        {
            QuickSlotItemIDs[i] = "";
        }

        // 게임 시작 시 모든 장비 부위를 기본 에셋으로 초기화
        foreach (var part in equipmentParts)
        {
            if (part.SpriteLibraryComponent != null && part.DefaultSpriteLibraryAsset != null)
            {
                part.SpriteLibraryComponent.spriteLibraryAsset = part.DefaultSpriteLibraryAsset;
            }
        }
        EquippedItems.Clear();
        foreach (EquipmentType type in System.Enum.GetValues(typeof(EquipmentType)))
        {
            EquippedItems.Add(type, null);
        }
    }

    // 참조 설정을 위해 Player의 Awake에서 호출. 
    public void SetPlayer(Player player)
    {
        playerStats = player;
    }

    public void AddItem(ItemData itemToAdd)
    {
        if (itemToAdd is ConsumableData)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                if (Items[i] != null && Items[i].ItemID == itemToAdd.itemID)
                {
                    Items[i].Quantity++;
                    OnInventoryChanged?.Invoke();
                    return;
                }
            }
        }

        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i] == null || string.IsNullOrEmpty(Items[i].ItemID))
            {
                Items[i] = new SavedItem { ItemID = itemToAdd.itemID, Quantity = 1 };
                OnInventoryChanged?.Invoke();
                return;
            }
        }
        Debug.LogWarning("인벤토리가 가득 찼습니다.");
    }

    public void RemoveItem(ItemData itemToRemove)
    {
        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i] != null && Items[i].ItemID == itemToRemove.itemID)
            {
                Items[i].Quantity--;
                if (Items[i].Quantity <= 0)
                {
                    Items[i] = null;
                }
                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }
    // 인덱스를 통한 장비 장착
    public void Equip(int index)
    {
        if (index < 0 || index >= Items.Length || Items[index] == null) return;

        ItemData itemToEquip = DataManager.Instance.GetItemByID(Items[index].ItemID);
        var equip = itemToEquip as EquipmentData;
        if (equip == null) return;

        EquipmentType type = equip.EquipType;
        var currentlyEquipped = EquippedItems.ContainsKey(type) ? EquippedItems[type] : null;

        Items[index] = null;

        if (currentlyEquipped != null)
        {
            UnEquip(type, false); // UnEquip 내부에서 AddItem을 호출하지 않도록 false 전달
        }

        Equip(equip, true);

        if (currentlyEquipped != null)
        {
            AddItem(currentlyEquipped);
        }

        OnInventoryChanged?.Invoke();
    }
    // 장비 데이터를 통한 장비 장착
    public void Equip(EquipmentData equip, bool fromInventory = true)
    {
        if (equip == null) return;

        EquipmentType type = equip.EquipType;
        if (fromInventory && EquippedItems.ContainsKey(type) && EquippedItems[type] != null)
        {
            Debug.LogWarning($"{type} 부위에 이미 아이템이 장착되어 있습니다: {EquippedItems[type].ItemName}");
            return;
        }

        EquippedItems[type] = equip;

        // 스탯 변경
        currentTotalAttack += equip.AttackPower;
        currentTotalDefense += equip.DefensePower;
        playerStats.UpdateEquipmentStats(currentTotalAttack, currentTotalDefense);

        // 장착 사운드 재생
        AudioManager.Instance.PlaySFX("Scrap_Touch");

        // 외형 변경
        EquipmentPart partToEquip = equipmentParts.FirstOrDefault(p => p.EquipmentType == equip.EquipType);
        if (partToEquip != null && equip is WeaponData weaponData && weaponData.WeaponSpriteLibrary != null)
        {
            partToEquip.SpriteLibraryComponent.spriteLibraryAsset = weaponData.WeaponSpriteLibrary;
        }

        OnInventoryChanged?.Invoke();
    }

    public void UnEquip(EquipmentType type, bool addToInventory = true)
    {
        EquipmentData itemToUnEquip = EquippedItems[type];
        if (itemToUnEquip == null) return;

        // 스탯 변경
        currentTotalAttack -= itemToUnEquip.AttackPower;
        currentTotalDefense -= itemToUnEquip.DefensePower;
        playerStats.UpdateEquipmentStats(currentTotalAttack, currentTotalDefense);

        // 외형 변경
        EquipmentPart partToUnEquip = equipmentParts.FirstOrDefault(p => p.EquipmentType == type);
        if (partToUnEquip != null)
        {
            partToUnEquip.SpriteLibraryComponent.spriteLibraryAsset = partToUnEquip.DefaultSpriteLibraryAsset;
        }

        EquippedItems[type] = null;

        if (addToInventory)
        {
            AddItem(itemToUnEquip);
        }

        OnInventoryChanged?.Invoke();
    }

    public void SwapItems(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= Items.Length || indexB < 0 || indexB >= Items.Length) return;
        (Items[indexA], Items[indexB]) = (Items[indexB], Items[indexA]);
        OnInventoryChanged?.Invoke();
    }

    public void AddGold(int amount)
    {
        Gold += Mathf.Max(0, amount);
        OnInventoryChanged?.Invoke();
    }

    public bool UseCoin()
    {
        if (Coin > 0)
        {
            Coin--;
            OnInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }

    public int GetCurrentTotalAttack() { return currentTotalAttack; }
    public int GetCurrentTotalDefense() { return currentTotalDefense; }

    public void RefreshUI()
    {
        OnInventoryChanged?.Invoke();
    }

    public void RegisterItemToEmptyQuickSlot(ItemData itemToRegister)
    {
        // 소비 아이템만 등록 가능
        if (itemToRegister == null || !(itemToRegister is ConsumableData))
        {
            return;
        }

        // 이미 퀵슬롯에 등록되어 있는지 확인
        if (QuickSlotItemIDs.Contains(itemToRegister.itemID))
        {
            Debug.Log($"'{itemToRegister.ItemName}'은(는) 이미 퀵슬롯에 등록되어 있습니다.");
            return;
        }

        // 비어있는 퀵슬롯 찾기
        int emptySlotIndex = -1;
        for (int i = 0; i < QuickSlotItemIDs.Length; i++)
        {
            if (string.IsNullOrEmpty(QuickSlotItemIDs[i]))
            {
                emptySlotIndex = i;
                break;
            }
        }

        // 비어있는 슬롯이 있다면 아이템 등록
        if (emptySlotIndex != -1)
        { 
            QuickSlotItemIDs[emptySlotIndex] = itemToRegister.itemID;
            Debug.Log($"'{itemToRegister.ItemName}'을(를) {emptySlotIndex + 1}번 퀵슬롯에 등록했습니다.");
            OnInventoryChanged?.Invoke(); // UI 갱신
        }
        else
        {
            Debug.Log("퀵슬롯이 가득 찼습니다.");
        }
    }
}
