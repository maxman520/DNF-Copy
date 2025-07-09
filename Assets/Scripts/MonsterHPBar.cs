using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class MonsterHPBar : MonoBehaviour
{
    [SerializeField] private Image faceImage;
    [SerializeField] private Image hpOverallImage; // 전체 HP 바
    [SerializeField] private Image hpFillFrontImage; // 앞쪽 HP 바
    [SerializeField] private Image hpFillBackImage;  // 뒤쪽 HP 바
    [SerializeField] private Image hpFlashImage;   // 전체 HP 점멸 이미지

    [Header("HP 바 색상 (스프라이트)")]
    [SerializeField] private List<Sprite> hpBarSprites;

    [Header("애니메이션 설정")]
    [SerializeField] private GameObject hpFlashPrefab;
    [SerializeField] private Transform hpFlashParent;
    [SerializeField] private float flashDuration = 0.4f;       // 전체 HP 애니메이션 점멸이 사라지는 시간

    [Header("잔상 효과 설정")]
    [SerializeField] private float animDuration = 0.5f; // 잔상이 따라잡는 데 걸리는 시간
    
    private CanvasGroup canvasGroup;
    private CancellationTokenSource fillCts; // 내용물 애니메이션 취소용 토큰
    
    private Monster currentTarget; // 현재 HP바의 주인이 누구인지

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // 초상화 설정
    public void SetFace(Sprite portraitSprite)
    {
        if (faceImage != null && portraitSprite != null)
        {
            faceImage.sprite = portraitSprite;
        }
    }

    
    // HP 업데이트
    public void UpdateHP(float maxHP,  float previousHP, float currentHP,  float hpPerLine)
    {
        if (hpOverallImage == null) return;
        if (hpFillFrontImage == null || hpFillBackImage == null || hpBarSprites.Count == 0) return;

        // --- 1. 전체 HP 바 크기 및 점멸 처리 ---
        float currentTotalHPRatio = currentHP / maxHP;
        float previousTotalHPRatio = previousHP / maxHP;

        // 전체 HP 바 fillAmount 즉시 조절
        hpOverallImage.fillAmount = currentTotalHPRatio;

        // 전체 HP 점멸 애니메이션 시작
        OverallFlashAsync(previousTotalHPRatio).Forget();

        // currentHP가 0일 때 FloorToInt가 -1을 반환하는 것을 방지
        currentHP = Mathf.Max(0, currentHP - 0.001f);

        // 1. 현재 몇 번째 줄인지 계산 (0부터 시작. 0 = 빨강, 1 = 주황, ...)
        int currentLineIndex = Mathf.FloorToInt(currentHP / hpPerLine);

        // 2. 현재 줄의 남은 HP 비율 계산
        float hpInCurrentLine = currentHP % hpPerLine;
        if (hpInCurrentLine == 0)
        {
            // 나머지 연산이 0이라는 것은 해당 줄의 체력이 꽉 찼다는 의미
            hpInCurrentLine = hpPerLine;
        }
        float newFillAmount = hpInCurrentLine / hpPerLine;

        // 3. 색상 순서를 계산
        int colorCount = hpBarSprites.Count;
        int frontColorIndex = currentLineIndex % colorCount;
        int backColorIndex = (currentLineIndex - 1) >= 0 ? (currentLineIndex - 1) % colorCount : 0;

        // 현재 Front 바의 fillAmount를 Flash 바에 먼저 설정
        hpFillFrontImage.sprite = hpBarSprites[frontColorIndex];

        // 뒷쪽 바 설정 (마지막 줄일 때는 뒷배경이 보여야 함)
        if (currentLineIndex > 0) // 현재 줄의 인덱스가 0보다 크다면 (즉, 마지막 줄이 아니라면)
        {
            hpFillBackImage.enabled = true;
            // 다음 줄(인덱스 - 1)의 색상을 배경으로 사용
            hpFillBackImage.sprite = hpBarSprites[backColorIndex];
        }
        else
        {
            // 마지막 줄(인덱스 0)일 때는 뒷 바를 비활성화
            hpFillBackImage.enabled = false;
        }

        // 애니메이션 시작
        FillAnimationAsync(newFillAmount).Forget();

        // 체력이 0이 되면 앞쪽 바도 비활성화
        if (currentHP <= 0)
        {
            hpFillFrontImage.enabled = false;
        }
        else
        {
            hpFillFrontImage.enabled = true;
        }
    }

    // 플레이어에게 데미지를 받아 해당 줄의 몬스터 HP바가 줄어드는 애니메이션
    private async UniTask FillAnimationAsync(float newFillAmount)
    {
        fillCts?.Cancel();
        fillCts?.Dispose();

        var destroyToken = this.GetCancellationTokenOnDestroy();
        fillCts = CancellationTokenSource.CreateLinkedTokenSource(destroyToken);
        var token = fillCts.Token;

        float oldFillAmount = hpFillFrontImage.fillAmount;
        
        try
        {
            // 점차 줄어들게 함
            float elapsedTime = 0f;
            while (elapsedTime < animDuration && !token.IsCancellationRequested)
            {
                // 이전 줄에서 다음 줄로 넘어갈 때, startFillAmount가 targetFillAmount보다 작을 수 있음
                // 이 경우 애니메이션 없이 즉시 반영해야 자연스러움
                if (oldFillAmount < newFillAmount)
                    oldFillAmount = newFillAmount;

                float tmp = Mathf.Lerp(oldFillAmount, newFillAmount, elapsedTime / animDuration);

                // FrontImage의 Color 프로퍼티를 변경
                hpFillFrontImage.fillAmount = tmp;

                elapsedTime += Time.deltaTime;
                await UniTask.Yield(token);
            }
        }
        catch (OperationCanceledException)
        {
            // 예외 처리
        }
        finally
        {
            hpFillFrontImage.fillAmount = newFillAmount;
        }
    }
    // 플레이어에게 데미지를 받아 전체 몬스터 HP바가 줄어드는 애니메이션
    private async UniTaskVoid OverallFlashAsync(float previousFillRatio)
    {
        // 1. 점멸용 이미지 복제
        var flash = EffectManager.Instance.PlayEffect("MonsterHPBarFlash", hpFlashParent);

        Image flashImage = flash.GetComponent<Image>();
        flashImage.fillAmount = previousFillRatio;
        flashImage.enabled = true;
        flashImage.color = Color.white;

        // 2. 점멸 애니메이션
        float elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / flashDuration);
            flashImage.color = new Color(1, 1, 1, alpha);
            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }

        // 3. 정리
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.ReturnEffectToPool("MonsterHPBarFlash", flash);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 모든 활성 플래시 오브젝트를 파괴하는 헬퍼 함수

    // UI 보이기
    public void Show(Monster newTarget)
    {
        // 두 가지 조건을 모두 만족할 때만 flash 이펙트 정리
        bool targetChanged = (currentTarget != null && currentTarget != newTarget);
        bool hasActiveFlashes = EffectManager.Instance.GetActiveEffectCount("MonsterHPBarFlash") > 0;
        
        if (targetChanged && hasActiveFlashes)
            EffectManager.Instance.ClearEffectsByName("MonsterHPBarFlash");

        // 새로운 타겟 설정
        this.currentTarget = newTarget;

        canvasGroup.alpha = 1f;
    }
    

    // UI 숨기기
    public void Hide()
    {
        canvasGroup.alpha = 0f;
    }
}