using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDescriptionPanel : MonoBehaviour
{
    [Header("UI 구성요소")][Space(10f)]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI equipmentTypeText; // 장비 타입 텍스트

    [Header("위치 오프셋")]
    [SerializeField] private Vector2 offset = new Vector2(0, 100f);

    private RectTransform panelRectTransform;
    private Canvas rootCanvas;

    private void Awake()
    {
        panelRectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        Hide();
    }

    public void Show(ItemData data, RectTransform slotRectTransform)
    {
        if (data == null) return;

        // 1. 내용 채우기
        itemIcon.sprite = data.ItemIcon;
        itemNameText.text = data.ItemName;
        itemDescriptionText.text = data.ItemDescription;
        // 2. 장비 타입 텍스트 처리
        if (data is EquipmentData equipmentData)
        {
            equipmentTypeText.gameObject.SetActive(true);
            equipmentTypeText.text = equipmentData.EquipType.ToString();
        }
        else
        {
            equipmentTypeText.gameObject.SetActive(false);
        }

        // 위치 계산을 위해 패널 활성화
        descriptionPanel.SetActive(true);

        // 3. 위치 설정 (슬롯 기준, 위쪽으로 먼저 시도)
        panelRectTransform.position = slotRectTransform.position;
        panelRectTransform.anchoredPosition += offset;

        // 4. 화면 상단 경계 체크
        Vector3[] corners = new Vector3[4];
        panelRectTransform.GetWorldCorners(corners);
        // corners[1]은 좌상단, corners[2]는 우상단 꼭짓점
        if (corners[1].y > Screen.height)
        {
            // 화면을 벗어났다면, offset을 빼서 아래쪽으로 위치를 재설정
            panelRectTransform.anchoredPosition -= offset * 2; // 위로 더한것을 취소하고 아래로 빼야하므로 * 2
        }
    }

    public void Hide()
    {
        descriptionPanel.SetActive(false);
    }
}