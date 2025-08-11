
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuickSlotUI : MonoBehaviour
{
    [SerializeField] private QuickSlot[] quickSlots;
    private Inventory inventory;

    // 아이템 쿨타임 관리
    private Dictionary<string, float> itemCooldowns = new Dictionary<string, float>();

    private void Awake()
    {
        if (quickSlots == null || quickSlots.Length == 0)
        {
            quickSlots = GetComponentsInChildren<QuickSlot>();
        }

        for (int i = 0; i < quickSlots.Length; i++)
        {
            quickSlots[i].SetIndex(i);
        }
    }

    private void Start()
    {
        if (Player.Instance != null)
        {
            inventory = Player.Instance.PlayerInventory;
            inventory.OnInventoryChanged += UpdateAllSlots;
            UpdateAllSlots(); // 초기 업데이트
        }
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateAllSlots;
        }
    }

    private void Update()
    {
        // 쿨타임 UI 업데이트
        foreach (var slot in quickSlots)
        {
            if (slot.GetItemData() != null)
            {
                slot.UpdateCooldown(GetCooldownRemaining(slot.GetItemData().itemID));
            }
        }
    }

    public void UpdateAllSlots()
    {
        for (int i = 0; i < quickSlots.Length; i++)
        {
            quickSlots[i].SetItem(inventory.QuickSlotItemIDs[i]);
        }
    }

    // 인벤토리에서 아이템을 퀵슬롯에 등록
    public void RegisterItem(ItemData itemToRegister)
    {
        if (itemToRegister == null || itemToRegister.ItemKind != ItemData.Kind.Consume)
        {
            Debug.Log("소비 아이템만 퀵슬롯에 등록할 수 있습니다.");
            return;
        }

        // 이미 등록된 아이템인지 확인
        if (inventory.QuickSlotItemIDs.Contains(itemToRegister.itemID))
        {
            Debug.Log("이미 퀵슬롯에 등록된 아이템입니다.");
            return;
        }

        // 빈 슬롯을 찾아 등록
        for (int i = 0; i < inventory.QuickSlotItemIDs.Length; i++)
        {
            if (string.IsNullOrEmpty(inventory.QuickSlotItemIDs[i]))
            {
                inventory.QuickSlotItemIDs[i] = itemToRegister.itemID;
                UpdateAllSlots();
                return;
            }
        }

        Debug.Log("퀵슬롯이 가득 찼습니다.");
    }
    
    // 특정 위치의 퀵슬롯에 아이템 등록
    public void RegisterItem(ItemData itemToRegister, int slotIndex)
    {
        if (itemToRegister == null || itemToRegister.ItemKind != ItemData.Kind.Consume) return;

        // 다른 슬롯에 이미 등록된 아이템인지 확인하고, 있다면 위치를 교환
        if (inventory.QuickSlotItemIDs.Contains(itemToRegister.itemID))
        {
            int existingIndex = System.Array.IndexOf(inventory.QuickSlotItemIDs, itemToRegister.itemID);
            // 현재 슬롯에 있던 아이템을 기존 슬롯으로 옮김
            inventory.QuickSlotItemIDs[existingIndex] = inventory.QuickSlotItemIDs[slotIndex];
        }
        
        inventory.QuickSlotItemIDs[slotIndex] = itemToRegister.itemID;
        UpdateAllSlots();
    }


    // 퀵슬롯에서 아이템 제거
    public void UnregisterItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventory.QuickSlotItemIDs.Length) return;

        inventory.QuickSlotItemIDs[slotIndex] = "";
        UpdateAllSlots();
    }

    // 퀵슬롯 아이템 사용. 실패하면 false 반환
    public bool UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventory.QuickSlotItemIDs.Length) return false;

        string itemID = inventory.QuickSlotItemIDs[slotIndex];
        if (string.IsNullOrEmpty(itemID)) return false;

        // 쿨타임 확인
        if (IsOnCooldown(itemID))
        {
            Debug.Log($"{DataManager.Instance.GetItemByID(itemID).ItemName}은(는) 아직 쿨타임입니다.");
            return false;
        }

        // 인벤토리에서 아이템 실제 사용 (수량 감소)
        ConsumableData consumable = DataManager.Instance.GetItemByID(itemID) as ConsumableData;
        if (consumable != null)
        {
            // 아이템 효과 적용
            consumable.Use(Player.Instance);
            
            // 인벤토리에서 아이템 제거
            inventory.RemoveItem(consumable);

            // 쿨타임 적용
            SetCooldown(consumable);

            UpdateAllSlots();
        }

        return true;
    }

    private void SetCooldown(ConsumableData item)
    {
        if (item.Cooldown > 0)
        {
            itemCooldowns[item.itemID] = Time.time + item.Cooldown;
        }
    }

    private bool IsOnCooldown(string itemID)
    {
        return itemCooldowns.ContainsKey(itemID) && Time.time < itemCooldowns[itemID];
    }

    // itemID 에 해당하는 아이템의 남은 쿨타임(FillAmount)을 반환
    private float GetCooldownRemaining(string itemID)
    {
        if (!IsOnCooldown(itemID)) return 0;
        
        float remaining = itemCooldowns[itemID] - Time.time;
        ConsumableData consumable = DataManager.Instance.GetItemByID(itemID) as ConsumableData;
        if (consumable != null && consumable.Cooldown > 0)
        {
            return remaining / consumable.Cooldown; // 0과 1 사이의 값으로 정규화
        }
        return 0;
    }
}
