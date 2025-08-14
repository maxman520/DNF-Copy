using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 웰컴 메시지 이미지가 나타났다가 서서히 사라지도록 처리하는 스크립트
// - 씬 진입 시 자동 재생 옵션
// - 대기 시간, 페이드 시간 조절 가능
// - 페이드 완료 후 비활성화 옵션
[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class WelcomeMessageFader : MonoBehaviour
{
    [SerializeField, Tooltip("씬 시작 시 자동으로 재생할지 여부")]
    private bool autoPlayOnStart = true;

    [SerializeField, Tooltip("표시 유지 시간(초)")]
    private float holdSeconds = 1.0f;

    [SerializeField, Tooltip("서서히 사라지는 시간(초)")]
    private float fadeSeconds = 0.75f;

    [SerializeField, Tooltip("페이드 완료 후 GameObject를 비활성화할지 여부")]
    private bool deactivateAfterFade = true;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        // 필수 컴포넌트 참조 확보
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnValidate()
    {
        // 에디터에서 추가 시 자동으로 CanvasGroup 보장
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        if (autoPlayOnStart)
        {
            // 씬 시작 시 자동 재생
            Play().Forget();
        }
    }

    // 외부에서 호출 가능한 진입점: 나왔다가 사라짐
    public async UniTask Play(CancellationToken externalToken = default)
    {
        var ct = this.GetCancellationTokenOnDestroy();
        using (var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, externalToken))
        {
            try
            {
                await ShowAndFadeOutAsync(holdSeconds, fadeSeconds, linked.Token);
            }
            catch (OperationCanceledException)
            {
                // "오브젝트가 파괴되어 페이드가 취소됨"
            }
        }
    }

    // 표시 후 페이드아웃 실행 (원하는 값으로 직접 호출 가능)
    public async UniTask ShowAndFadeOutAsync(float hold, float fade, CancellationToken ct = default)
    {
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        // 시작 상태: 완전 표시
        gameObject.SetActive(true);
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = false;

        // 유지 시간 대기
        if (hold > 0f)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(hold), cancellationToken: ct);
        }

        // 페이드 아웃
        if (fade <= 0f)
        {
            _canvasGroup.alpha = 0f;
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < fade)
            {
                ct.ThrowIfCancellationRequested();
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fade);
                _canvasGroup.alpha = 1f - t;
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            _canvasGroup.alpha = 0f;
        }

        // 완료 처리
        _canvasGroup.blocksRaycasts = false;
        if (deactivateAfterFade)
        {
            gameObject.SetActive(false);
        }
    }
}

