using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

// 던전 결과 데이터를 담을 간단한 구조체
public struct DungeonResultData
{
    public int HuntEXP;
    public int ClearEXP;
}

[RequireComponent(typeof(CanvasGroup))]
public class ResultPanel : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private TextMeshProUGUI totalExpText;
    [SerializeField] private TextMeshProUGUI huntExpText;
    [SerializeField] private TextMeshProUGUI clearExpText;
    [SerializeField] private Button returnTownButton;
    [SerializeField] private Button nextDungeonButton;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        
        // 처음에는 상호작용 불가
        gameObject.SetActive(false);
        canvasGroup.interactable = false;
    }
    private void Start()
    {   
        // 버튼 클릭 이벤트에 DungeonManager의 함수들을 연결
        if (DungeonManager.Instance != null)
        {
            returnTownButton.onClick.AddListener(DungeonManager.Instance.ReturnToTown);
            nextDungeonButton.onClick.AddListener(DungeonManager.Instance.GoToNextDungeon);
        }
    }

    // 결과 데이터를 받아 UI를 채우고, 패널을 보여주는 메인 함수
    public void Show(DungeonResultData resultData)
    {
        // 1. 데이터로 UI 텍스트 업데이트
        huntExpText.text = $"{resultData.HuntEXP:N0}"; // N0는 천 단위 콤마
        clearExpText.text = $"{resultData.ClearEXP:N0}";
        totalExpText.text = $"{resultData.HuntEXP+resultData.ClearEXP:N0}";

        // 2. 활성화
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
    }
}