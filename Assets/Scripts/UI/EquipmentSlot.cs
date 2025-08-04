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
            Unhighlight();
            UIManager.Instance.HideItemDescription(); // UIManager에 아이템 설명창 숨김을 요청
        }
    }

    public void Highlight()
    {
        if (forground != null)
            forground.gameObject.SetActive(true);
    }

    public void Unhighlight()
    {
        if (forground != null)
            forground.gameObject.SetActive(false);
    }
}