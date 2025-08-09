using UnityEngine.EventSystems;
using UnityEngine;

public class EquipmentSlot : InventorySlot, IPointerClickHandler, IDropHandler
{
    public EquipmentType EquipType;

    // 장비 슬롯에서 우클릭 시 호출 (오버라이드)
    public new void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 장착된 아이템을 해제
            FindFirstObjectByType<Inventory>().UnEquip(EquipType);
            Unhighlight();
            UIManager.Instance.HideItemDescription(); // UIManager에 아이템 설명창 숨김을 요청
        }
    }

    // 인벤토리 슬롯에서 장비 슬롯으로 드롭하면 해당 인덱스의 아이템을 장착
    public new void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot == null) return;
        var inv = Player.Instance.GetComponent<Inventory>();
        if (inv == null) return;
        
        // 1) 인벤토리 → 장비: 장착(이미 장착 시 교체)
        if (draggedSlot is InventorySlot invSlot)
        {
            inv.Equip(invSlot.Index);
            return;
        }

        // 2) 장비 ↔ 장비: 아무 것도 하지 않음
        // draggedSlot이 EquipmentSlot인 경우 무시
    }

    public void Highlight()
    {
        if (foreground != null)
            foreground.gameObject.SetActive(true);
    }

    public void Unhighlight()
    {
        if (foreground != null)
            foreground.gameObject.SetActive(false);
    }
}