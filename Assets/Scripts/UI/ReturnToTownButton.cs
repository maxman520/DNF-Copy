using UnityEngine;
using UnityEngine.UI;

// 돌아가기 버튼: 인스펙터에서 씬 이름과 플레이어 좌표를 설정하고,
// 버튼을 누르면 해당 씬으로 이동한 뒤 플레이어 위치를 지정
[RequireComponent(typeof(Button))]
public class ReturnToTownButton : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Town_Scene"; // 이동할 씬 이름
    [SerializeField] private Vector3 targetPlayerPosition;            // 해당 씬에서의 플레이어 좌표

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
        {
            _button.onClick.AddListener(OnClick);
        }
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnClick);
        }
    }

    // UI Button에 연결할 메서드
    public void OnClick()
    {
        // 인자 검증
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("[ReturnToTownButton] 이동할 씬 이름이 비어있습니다");
            return;
        }
        GameManager.Instance.GoToTown(targetSceneName, targetPlayerPosition);
    }
}
