using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GhostStatePanel : MonoBehaviour
{
    [Header("하위 오브젝트 참조")]
    [SerializeField] private GameObject ghostStateObject;
    [SerializeField] private GameObject countdownObject;

    [Header("카운트다운 설정")]
    [SerializeField] private Image countdownImage;
    [SerializeField] private Sprite[] numberSprites; // 인스펙터에서 0부터 9까지 할당

    [Header("코인 개수 설정")]
    [SerializeField] private TextMeshProUGUI CoinQuantityText;


    private CancellationTokenSource countdownCts;

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        countdownCts?.Cancel();
        countdownCts?.Dispose();
        countdownCts = null;
    }

    // UIManager가 호출하여 패널을 초기 상태로 설정
    public void Initialize()
    {
        ghostStateObject.SetActive(true);
        countdownObject.SetActive(false);
        if (Player.Instance != null)
            SetCoinQuantity(Player.Instance.PlayerInventory.Coin);
    }

    // UIManager가 호출하여 카운트다운 시작
    public void StartCountdown()
    {
        countdownObject.SetActive(true);

        // 이전 카운트다운이 있다면 취소
        countdownCts?.Cancel();
        countdownCts?.Dispose();
        
        var destroyToken = this.GetCancellationTokenOnDestroy();
        countdownCts = CancellationTokenSource.CreateLinkedTokenSource(destroyToken);

        CountdownSequence(countdownCts.Token).Forget();
    }

    private async UniTask CountdownSequence(CancellationToken token)
    {
        float timeLeft = 9.99f;

        try
        {
            while (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;

                // 시간에 맞는 숫자 스프라이트 표시
                int digit = Mathf.Max(0, Mathf.FloorToInt(timeLeft));
                if (digit < numberSprites.Length)
                {
                    countdownImage.sprite = numberSprites[digit];
                }

                // 'X' 키 입력 감지하여 부활 처리
                if (Input.GetKeyDown(KeyCode.X))
                {
                    if (Player.Instance != null && Player.Instance.PlayerInventory.UseCoin())
                    {
                        Player.Instance.Revive();
                        gameObject.SetActive(false); // 부활 성공 시 패널 닫기
                        break; // 카운트다운 루프 중단
                    }
                    else
                    {
                        Debug.Log("코인이 부족하여 부활할 수 없습니다.");
                        // TODO: 코인 부족 알림 UI 표시 (예: 화면 흔들림, 사운드)
                    }
                }

                await UniTask.Yield(token);
            }

            // 카운트다운이 정상적으로 끝났다면 (부활하지 않았다면)
            if (timeLeft <= 0)
            {
                // 카운트다운 종료 후 게임 오버 처리
                Debug.Log("부활 시간 초과. 게임 오버 처리.");
                gameObject.SetActive(false);
                GameManager.Instance.GoToTown();
            }
        }
        catch (OperationCanceledException)
        {
            // 카운트다운이 외부에서 취소됨
        }
    }

    public void SetCoinQuantity(int quantity)
    {
        CoinQuantityText.text = "X " + quantity.ToString();
    }

}
