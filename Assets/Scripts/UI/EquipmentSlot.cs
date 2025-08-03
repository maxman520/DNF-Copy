using UnityEngine;
using UnityEngine.EventSystems;

public class EquipmentSlot : InventorySlot, IPointerClickHandler
{
    public EquipmentType EquipType;

    // 장비 슬롯에서 우클릭 시 호출 (오버라이드)
    public new void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 장착된 아이템을 해제
            FindFirstObjectByType<Inventory>().UnEquip(EquipType);
        }
    }
}
