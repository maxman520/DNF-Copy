// 스킬샵 창의 드래그 가능한 스킬 아이콘에 붙일 스크립트
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillIconDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public SkillData skillData; // 이 아이콘이 어떤 스킬인지
    private SkillShopUI skillShopUI;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 startPosition;
    private Transform startParent;

    private Canvas parentCanvas; // 자신을 담고 있는 최상위 캔버스를 저장할 변수

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();
        skillShopUI = GetComponentInParent<SkillShopUI>();
    }

    // 아이콘이 클릭되었을 때 호출
    public void OnPointerClick(PointerEventData eventData)
    {
        // 드래그 중인 상태에서는 클릭으로 처리하지 않음 (선택적)
        if (eventData.dragging)
        {
            return;
        }

        if (skillShopUI != null)
        {
            // 스킬샵 매니저의 ShowDescription 함수를 호출
            skillShopUI.ShowDescription(this.skillData);
            Debug.Log($"'{skillData.skillName}' 아이콘 클릭됨. 설명 표시 요청");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 시작 시 상태 저장
        startPosition = rectTransform.position;
        startParent = transform.parent;

        canvasGroup.blocksRaycasts = false; // 드롭 지점을 감지할 수 있도록 레이캐스트를 통과시킴

        // 드래그 중에는 최상위 캔버스 자식으로 옮겨서 다른 UI에 가려지지 않게 함
        transform.SetParent(parentCanvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 마우스 위치를 따라 아이콘 이동
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 원래 위치와 부모로 되돌아감
        rectTransform.position = startPosition;
        transform.SetParent(startParent);
    }
}