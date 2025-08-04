using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    // 이 슬롯에 마우스가 올라왔을 때, 아이템 종류를 외부에 알리기 위한 정적 이벤트
    public static event System.Action<EquipmentType> OnHoverEquipmentItem;
    public static event System.Action OnExitHover;

    [Header("UI 구성요소")]
    public Image Icon;
    [SerializeField] protected Image forground;

    private ItemData item;

    private void Awake()
    {
        if (forground != null)
            forground.gameObject.SetActive(false);
    }

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

        if (forground != null)
            forground.gameObject.SetActive(false);
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
                forground.gameObject.SetActive(false);
                UIManager.Instance.HideItemDescription(); // UIManager에 아이템 설명창 숨김을 요청
            }
        }
    }

    // 마우스 포인터를 올렸을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null)
        {
            if (forground != null) forground.gameObject.SetActive(true);

            // UIManager에 아이템 설명창 표시를 요청
            UIManager.Instance.ShowItemDescription(item, transform as RectTransform);

            // 아이템이 장비라면, 해당하는 장비 종류를 이벤트로 외부에 알림
            if (item is EquipmentData equipmentData)
            {
                OnHoverEquipmentItem?.Invoke(equipmentData.EquipType);
            }
        }
    }

    // 마우스 포인터를 내렸을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        if (forground != null) forground.gameObject.SetActive(false);

        UIManager.Instance.HideItemDescription(); // UIManager에 아이템 설명창 숨김을 요청

        // 아이템 종류와 상관없이, 마우스가 슬롯을 벗어나면 무조건 이벤트 발생
        OnExitHover?.Invoke();
    }
}