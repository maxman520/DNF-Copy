
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;

public class QuickSlot : MonoBehaviour, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 구성요소")]
    [SerializeField] private Image icon;
    [SerializeField] private Image coolTimeImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image foreground;

    public int Index { get; private set; }
    private ItemData itemData;
    private QuickSlotUI controller;

    private void Awake()
    {
        controller = GetComponentInParent<QuickSlotUI>();
        if (foreground != null) foreground.gameObject.SetActive(false);
        if (coolTimeImage != null) coolTimeImage.fillAmount = 0;
    }

    public void SetIndex(int index)
    {
        Index = index;
    }

    public void SetItem(string itemID)
    {
        itemData = DataManager.Instance.GetItemByID(itemID);

        if (itemData == null)
        {
            ClearSlot();
            return;
        }

        icon.sprite = itemData.ItemIcon;
        icon.color = new Color(1, 1, 1, 1);

        if (itemData is ConsumableData)
        {
            int quantity = Player.Instance.PlayerInventory.Items
                                .Where(i => i != null && i.ItemID == itemID)
                                .Sum(i => i.Quantity);

            if (quantity > 1)
            {
                quantityText.text = quantity.ToString();
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }

            if (quantity <= 0)
            {
                icon.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 반투명 회색
                quantityText.text = "0";
                quantityText.gameObject.SetActive(true);
            }
        }
    }

    public void ClearSlot()
    {
        itemData = null;
        icon.sprite = null;
        icon.color = new Color(1, 1, 1, 0);
        quantityText.gameObject.SetActive(false);
        if (coolTimeImage != null) coolTimeImage.fillAmount = 0;
    }

    public ItemData GetItemData()
    {
        return itemData;
    }

    public void UpdateCooldown(float fillAmount)
    {
        if (coolTimeImage != null)
        {
            coolTimeImage.fillAmount = fillAmount;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot draggedSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (draggedSlot != null)
        {
            ItemData droppedItem = draggedSlot.GetItemData(); // InventorySlot에 GetItemData() 필요
            if (droppedItem != null)
            {
                controller.RegisterItem(droppedItem, this.Index);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (itemData != null)
            {
                controller.UnregisterItem(Index);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemData != null)
        {
            foreground.gameObject.SetActive(true);
            UIManager.Instance.ShowItemDescription(itemData, transform as RectTransform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreground.gameObject.SetActive(false);
        UIManager.Instance.HideItemDescription();
    }
}
