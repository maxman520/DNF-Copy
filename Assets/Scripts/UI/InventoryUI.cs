using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("슬롯 (Slots)")]
    [SerializeField] private List<EquipmentSlot> equipmentSlots; // 장비 장착 슬롯 (인스펙터에서 할당)
    [SerializeField] private List<InventorySlot> inventorySlots; // 획득한 아이템 슬롯 (인스펙터에서 할당)

    [Header("재화 텍스트")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI coinText;

    private Inventory inventory;

    void Start()
    {
        // 플레이어의 Inventory 컴포넌트를 찾아서 참조
        inventory = Player.Instance.GetComponent<Inventory>();

        // 장비 장착 슬롯 초기화
        inventory.Initialize();

        // 인벤토리 데이터가 변경될 때마다 UI를 업데이트하도록 이벤트 구독
        inventory.OnInventoryChanged += UpdateUI;

        // 슬롯 이벤트 구독
        InventorySlot.OnHoverEquipmentItem += HighlightEquipmentSlot;
        InventorySlot.OnExitHover += UnhighlightAllEquipmentSlots;

        // 초기 UI 업데이트
        UpdateUI();
    }

    private void OnEnable()
    {
        UIManager.Instance?.OpenUI(this.gameObject);
    }

    private void OnDisable()
    {
        UIManager.Instance?.CloseUI(this.gameObject);
    }

    // 인벤토리 전체 UI를 최신 데이터로 업데이트
    private void UpdateUI()
    {
        // 1. 획득한 아이템 슬롯 업데이트
        for (int i = 0; i < inventory.Items.Length; i++)
        {
            inventorySlots[i].SetIndex(i); // 각 슬롯에 자신의 데이터 인덱스를 알려줌

            if (inventory.Items[i] != null)
            {
                inventorySlots[i].AddItem(inventory.Items[i]);
            }
            else
            {
                inventorySlots[i].ClearSlot();
            }
        }

        // 2. 장착된 아이템 슬롯 업데이트
        foreach (var slot in equipmentSlots)
        {
            EquipmentData equippedItem = inventory.EquippedItems[slot.EquipType];
            if (equippedItem != null)
            {
                slot.AddItem(equippedItem);
            }
            else
            {
                slot.ClearSlot();
            }
        }

        // 3. 재화 텍스트 업데이트
        goldText.text = inventory.Gold.ToString();
        coinText.text = inventory.Coin.ToString();
    }

    private void OnDestroy()
    {
        // 오브젝트 파괴 시 이벤트 구독 해제
        if (inventory != null)
        { 
            inventory.OnInventoryChanged -= UpdateUI;
        }

        InventorySlot.OnHoverEquipmentItem -= HighlightEquipmentSlot;
        InventorySlot.OnExitHover -= UnhighlightAllEquipmentSlots;
    }

    // 특정 타입의 장비 슬롯을 하이라이트하는 함수
    private void HighlightEquipmentSlot(EquipmentType type)
    {
        foreach (var slot in equipmentSlots)
        {
            if (slot.EquipType == type)
            {
                slot.Highlight();
                break; // 하나만 찾으면 되므로 중단
            }
        }
    }

    // 모든 장비 슬롯의 하이라이트를 해제하는 함수
    private void UnhighlightAllEquipmentSlots()
    {
        foreach (var slot in equipmentSlots)
        {
            slot.Unhighlight();
        }
    }
}
