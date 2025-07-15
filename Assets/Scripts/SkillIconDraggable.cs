// ��ų�� â�� �巡�� ������ ��ų �����ܿ� ���� ��ũ��Ʈ
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillIconDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public SkillData skillData; // �� �������� � ��ų����

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 startPosition;
    private Transform startParent;
    
    // �ڡڡ� �ڽ��� ��� �ִ� �ֻ��� ĵ������ ������ ����
    [SerializeField]private Canvas parentCanvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // �巡�� ���� �� ���� ����
        startPosition = rectTransform.position;
        startParent = transform.parent;

        canvasGroup.alpha = 0.6f; // �������ϰ�
        canvasGroup.blocksRaycasts = false; // ��� ������ ������ �� �ֵ��� ����ĳ��Ʈ�� �����Ŵ

        // �巡�� �߿��� �ֻ��� ĵ���� �ڽ����� �Űܼ� �ٸ� UI�� �������� �ʰ� ��
        transform.SetParent(parentCanvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // ���콺 ��ġ�� ���� ������ �̵�
        // rectTransform.position = Input.mousePosition;

        // Input.mousePosition ��� eventData.position ���
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // ���� ��ġ�� �θ�� �ǵ��ư�
        rectTransform.position = startPosition;
        transform.SetParent(startParent);
    }
}