using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class DungeonSelectButton : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler, IPointerDownHandler
{
    [SerializeField] private string sceneName;
    [SerializeField] private Button button;

    private bool isSelected;
    private bool highlighted; // 첫 클릭 후 하이라이트(두 번째 클릭부터 실행)
    private int highlightedClickFrame; // 하이라이트된 클릭 프레임
    private int lastClickStartFrame; // 마지막 포인터 다운 프레임
    private int lastSpaceTriggerFrame = -1; // 스페이스 트리거 프레임(Submit과 중복 방지)

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    private void OnEnable()
    {
        // 상태 초기화
        highlighted = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        // 선택 해제 시 하이라이트 해제
        highlighted = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 마우스/터치: 첫 클릭은 선택만, 두 번째 클릭부터 실행
        var es = EventSystem.current;
        if (!highlighted)
        {
            highlighted = true;
            highlightedClickFrame = lastClickStartFrame;
            if (es != null && es.currentSelectedGameObject != gameObject)
            {
                es.SetSelectedGameObject(gameObject);
            }
            return;
        }

        // 같은 클릭 안에서는 실행하지 않음
        if (highlighted && highlightedClickFrame == lastClickStartFrame)
        {
            return;
        }

        TryLoad();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 포인터 입력 시작 프레임 기록
        lastClickStartFrame = Time.frameCount;
    }

    public void OnSubmit(BaseEventData eventData)
    {
        // 같은 프레임에 스페이스 처리된 Submit이면 중복 실행 방지
        if (Time.frameCount == lastSpaceTriggerFrame)
        {
            return;
        }

        // 같은 클릭 사이클(하이라이트된 프레임)에서 발생한 Submit이라면 무시
        if (highlighted && highlightedClickFrame == lastClickStartFrame)
        {
            return;
        }

        // 키보드/패드 Submit 또는 하이라이트 이후의 별도 클릭
        TryLoad();
    }

    private void Update()
    {
        // 하이라이트된 상태에서 스페이스바로 실행 지원
        if (!isSelected || !highlighted)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            lastSpaceTriggerFrame = Time.frameCount;
            TryLoad();
        }
    }

    private void TryLoad()
    {
        // 현재 선택 상태가 아니면 실행하지 않음
        if (!isSelected)
        {
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"[DungeonSelectButton] {name} 에서 씬 이름이 비어있습니다");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
