using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler // 우클릭 감지를 위해 추가
{
    public Image Icon;
    private ItemData item;

    public void AddItem(ItemData newItem)
    {
        item = newItem;
        Icon.sprite = item.ItemIcon;
        Icon.enabled = true;
    }

    public void ClearSlot()
    {
        item = null;
        Icon.sprite = null;
        Icon.enabled = false;
    }

    // 슬롯에서 우클릭 시 호출
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (item != null && item is EquipmentData)
            {
                // 아이템을 장착
                FindFirstObjectByType<Inventory>().Equip(item as EquipmentData);
            }
        }
    }
}
