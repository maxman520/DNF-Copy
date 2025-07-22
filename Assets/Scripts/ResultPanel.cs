using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// 던전 결과 데이터를 담을 간단한 구조체
public struct DungeonResultData
{
    public float ClearTime;
    public int HuntEXP;
    public int ClearEXP;
    public Sprite RankSprite;
}

[RequireComponent(typeof(CanvasGroup))]
public class ResultPanel : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Image rankImage;
    [SerializeField] private TextMeshProUGUI clearTimeText;
    [SerializeField] private TextMeshProUGUI totalExpText;
    [SerializeField] private TextMeshProUGUI huntExpText;
    [SerializeField] private TextMeshProUGUI clearExpText;
    [SerializeField] private Button returnTownButton;
    [SerializeField] private Button nextDungeonButton;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        
        // 처음에는 투명하고 상호작용 불가
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        this.gameObject.SetActive(false);
    }

    private void Start()
    {   
        // 버튼 클릭 이벤트에 DungeonManager의 함수들을 연결
        if (GameManager.Instance != null)
        {
            returnTownButton.onClick.AddListener(GameManager.Instance.ReturnToTown);
            nextDungeonButton.onClick.AddListener(GameManager.Instance.GoToNextDungeon);
        }
    }

    // 결과 데이터를 받아 UI를 채우는 함수
    public void SetResultData(DungeonResultData resultData)
    {
        // 1. 랭크 이미지 설정
        if (rankImage != null)
        {
            if (resultData.RankSprite != null)
            {
                rankImage.sprite = resultData.RankSprite;
                rankImage.SetNativeSize();
                rankImage.gameObject.SetActive(true);
            }
            else
            {
                // 표시할 랭크가 없으면 이미지를 비활성화
                Debug.LogError("표시할 랭크 스프라이트가 없음!");
                rankImage.gameObject.SetActive(false);
            }
        }

        // 2. 데이터로 UI 텍스트 업데이트
        TimeSpan timeSpan = TimeSpan.FromSeconds(resultData.ClearTime);
        clearTimeText.text = $"{timeSpan.Minutes}분 {timeSpan.Seconds}초 {timeSpan.Milliseconds}";
        huntExpText.text = $"{resultData.HuntEXP:N0}"; // N0는 천 단위 콤마
        clearExpText.text = $"{resultData.ClearEXP:N0}";
        totalExpText.text = $"{resultData.HuntEXP + resultData.ClearEXP:N0}";

        // 3. 활성화
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
    }
}