using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    // 이 슬롯에 마우스가 올라왔을 때, 아이템 종류를 외부에 알리기 위한 정적 이벤트
    public static event System.Action<EquipmentType> OnHoverEquipmentItem;
    public static event System.Action OnExitHover;

    [Header("UI 구성요소")]
    public Image Icon;
    [SerializeField] protected Image foreground;
    [SerializeField] private TextMeshProUGUI quantityText;

    public int Index { get; private set; }
    private SavedItem savedItem;
    private ItemData itemData;

    private Transform originalParent;
    private Inventory inventory;
    private Canvas parentCanvas;
    protected static InventorySlot draggedSlot;

    private void Awake()
    {
        originalParent = transform; 
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        if (Player.Instance != null)
            inventory = Player.Instance.PlayerInventory;
        
        if (foreground != null) foreground.gameObject.SetActive(false);
        if (quantityText != null) quantityText.gameObject.SetActive(false);
    }

    public void SetItemData(SavedItem newItem)
    {
        savedItem = newItem;
        itemData = DataManager.Instance.GetItemByID(savedItem.ItemID);

        if (itemData == null)
        {
            ClearSlot();
            return;
        }

        Icon.sprite = itemData.ItemIcon;
        
        // 아이콘이 보이도록 처리
        var tempColor = Icon.color;
        tempColor.a = 1f;
        Icon.color = tempColor;

        if (quantityText != null)
        {
            if (itemData is ConsumableData && savedItem.Quantity > 1)
            {
                quantityText.text = savedItem.Quantity.ToString();
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }
    }

    public void ClearSlot()
    {
        savedItem = null;
        itemData = null;
        Icon.sprite = null;

        // 아이템 아이콘 숨김 처리
        var tempColor = Icon.color;
        tempColor.a = 0f;
        Icon.color = tempColor;

        if (foreground != null) foreground.gameObject.SetActive(false);
        if (quantityText != null) quantityText.gameObject.SetActive(false);
    }

    public void SetIndex(int index)
    {
        Index = index;
    }

    public ItemData GetItemData()
    {
        return itemData;
    }

    #region Pointer Events
    // 슬롯에서 우클릭 시 호출
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (itemData == null) return;

            if (itemData is EquipmentData)
            {
                inventory.Equip(Index);
                UIManager.Instance.HideItemDescription();
                if (foreground != null) foreground.gameObject.SetActive(false);
            }
            else if (itemData is ConsumableData)
            {
                inventory.RegisterItemToEmptyQuickSlot(itemData);
                // 우클릭으로 퀵슬롯 등록 시 장착감 사운드 재생
                AudioManager.Instance.PlaySFX("Scrap_Touch");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemData != null)
        {
            if (foreground != null) foreground.gameObject.SetActive(true);

            // UIManager에 아이템 설명창 표시를 요청
            UIManager.Instance.ShowItemDescription(itemData, transform as RectTransform);

            // 아이템이 장비라면, 해당하는 장비 종류를 이벤트로 외부에 알림
            if (itemData is EquipmentData equipmentData) OnHoverEquipmentItem?.Invoke(equipmentData.EquipType);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (foreground != null) foreground.gameObject.SetActive(false);
        UIManager.Instance.HideItemDescription();
        // 아이템 종류와 상관없이, 마우스가 슬롯을 벗어나면 이벤트 발생
        OnExitHover?.Invoke();
    }
    #endregion

    #region Drag and Drop Events
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemData == null) return;
        // 드래그 시작 사운드
        AudioManager.Instance.PlaySFX("Iconpick");
        draggedSlot = this;
        // 아이콘을 최상위로 옮겨 다른 UI 위에 그려지게 함
        Icon.transform.SetParent(parentCanvas.transform);

        // 드래그 중인 아이콘이 마우스 이벤트를 통과시키도록 설정
        Icon.raycastTarget = false;
        if (foreground != null) foreground.gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (itemData == null) return;
        Icon.transform.position = eventData.position; // 마우스 위치로 아이콘 이동
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그가 끝났으므로, 공용 변수를 비워줌
        draggedSlot = null;
        // 아이콘을 원래 슬롯으로 되돌림
        Icon.transform.SetParent(originalParent);
        Icon.transform.localPosition = Vector3.zero;

        // 아이콘이 다시 마우스 이벤트를 받을 수 있도록 복원
        Icon.raycastTarget = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot == null) return;

        // 1) 장비 슬롯에서 인벤토리 슬롯으로 드롭 → 탈착만 수행, 드롭 위치 무시
        if (draggedSlot is EquipmentSlot eqSlot)
        {
            inventory.UnEquip(eqSlot.EquipType);
            return;
        }

        // 2) 인벤토리 슬롯 ↔ 인벤토리 슬롯 → 교환
        if (draggedSlot is InventorySlot invSlot)
        {
            if (invSlot != this)
            {
                inventory.SwapItems(invSlot.Index, this.Index);
                // 유효한 슬롯에 드롭 사운드
                AudioManager.Instance.PlaySFX("Iconset");
            }
        }
    }
    #endregion
}
