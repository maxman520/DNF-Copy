using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 프로필 창 전용 장비 슬롯: 보기 전용 (드래그/장착/해제 불가)
public class ProfileEquipmentSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("프로필 장비 슬롯")]
    public EquipmentType EquipType;
    [SerializeField] private Image icon;

    private Inventory inventory;
    private ItemData currentItem;

    private void Awake()
    {
        HideIcon();
    }

    private void OnEnable()
    {
        CacheInventory();
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
        UIManager.Instance?.HideItemDescription();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void CacheInventory()
    {
        if (Player.Instance != null)
        {
            inventory = Player.Instance.PlayerInventory;
        }
    }

    private void Subscribe()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged += Refresh;
        }
    }

    private void Unsubscribe()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= Refresh;
        }
    }

    // 현재 장착된 아이템을 읽어 아이콘을 갱신
    private void Refresh()
    {
        if (inventory == null)
        {
            HideIcon();
            currentItem = null;
            return;
        }

        EquipmentData equipped = null;
        if (inventory.EquippedItems != null && inventory.EquippedItems.ContainsKey(EquipType))
        {
            equipped = inventory.EquippedItems[EquipType];
        }

        if (equipped != null)
        {
            currentItem = equipped;
            if (icon != null)
            {
                icon.sprite = equipped.ItemIcon;
                var c = icon.color; c.a = 1f; icon.color = c;
            }
        }
        else
        {
            currentItem = null;
            HideIcon();
        }
    }

    private void HideIcon()
    {
        if (icon != null)
        {
            icon.sprite = null;
            var c = icon.color; c.a = 0f; icon.color = c;
        }
    }

    // ---- Pointer Events: 설명창만 표시 ----
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null)
        {
            UIManager.Instance?.ShowItemDescription(currentItem, transform as RectTransform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance?.HideItemDescription();
    }
}
