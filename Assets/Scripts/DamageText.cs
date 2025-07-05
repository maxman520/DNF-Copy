using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class DamageText : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private DamageFontData fontData; // 사용할 폰트 데이터
    [SerializeField] private List<SpriteRenderer> numberRenderers; // 숫자 자릿수 렌더러들

    [Header("애니메이션 설정")]
    [SerializeField] private float moveDistance = 0.5f; // 위로 떠오를 거리
    [SerializeField] private float duration = 0.8f;     // 전체 지속 시간
    [SerializeField] private float fadeOutDelay = 0.5f; // 사라지기 시작하는 시간

    [Header("크기 애니메이션 설정")]
    [SerializeField] private Vector3 startScale = new Vector3(10f, 10f, 1f); // 시작 시 크기 배율
    [SerializeField] private float scaleInDuration = 0.15f; // 원래 크기로 돌아오는 데 걸리는 시간

    [Header("데미지 텍스트 이름 (EffectManager에 등록된 이름과 동일해야 함)")]
    [SerializeField] private string effectName;

    // 데미지 값을 받아와서 스프라이트로 변환하여 표시
    public void SetDamageAndPlay(int damage)
    {
        string damageString = damage.ToString();

        // 모든 숫자 렌더러를 일단 비활성화하고 알파값 초기화
        foreach (var renderer in numberRenderers)
        {
            renderer.gameObject.SetActive(false);
            Color color = renderer.color;
            color.a = 1f; // 알파값 리셋
            renderer.color = color;
        }

        // 데미지 문자열의 각 숫자에 해당하는 스프라이트를 설정하고 활성화
        for (int i = 0; i < damageString.Length; i++)
        {
            if (i >= numberRenderers.Count) break; // 준비된 자릿수를 넘으면 중단

            int number = int.Parse(damageString[i].ToString());
            numberRenderers[i].sprite = fontData.numberSprites[number];
            numberRenderers[i].gameObject.SetActive(true);
        }

        // 애니메이션 시작
        Animate().Forget();
    }

    // 메인 애니메이션 함수: 두 개의 시퀀스를 순차적으로 호출
    private async UniTask Animate()
    {

        // 시퀀스 1: 크기 조절 애니메이션 실행 및 완료 대기
        await ScaleInAnimation();

        // 시퀀스 2: 이동 및 소멸 애니메이션 실행 및 완료 대기
        await MoveAndFadeAnimation();

        // 애니메이션이 끝나면 풀에 반납
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.ReturnEffectToPool(effectName, this.gameObject);
        }
        else
        {
            // 매니저가 없으면 그냥 파괴
            Destroy(gameObject);
        }
    }

    // 시퀀스 1: 큰 크기에서 원래 크기(1,1,1)로 줄어드는 애니메이션
    private async UniTask ScaleInAnimation()
    {
        float elapsedTime = 0f;
        Vector3 originalScale = Vector3.one; // 목표 크기 (1, 1, 1)

        while (elapsedTime < scaleInDuration)
        {
            // 진행률 (0 -> 1)
            float progress = elapsedTime / scaleInDuration;

            // Ease-Out 효과를 위해 진행률을 보정 (처음엔 빠르고 나중에 느리게)
            // 1 - (1-x)^n 공식 사용
            float easedProgress = 1 - Mathf.Pow(1 - progress, 3); // 3은 강도 조절

            // 크기를 시작 크기에서 목표 크기로 보간
            transform.localScale = Vector3.Lerp(startScale, originalScale, easedProgress);

            elapsedTime += Time.deltaTime;
            await UniTask.Yield(this.GetCancellationTokenOnDestroy());
        }

        // 애니메이션이 끝나면 크기를 정확히 원래대로 설정
        transform.localScale = originalScale;
    }

    // 시퀀스 2: 위로 떠오르며 사라지는 애니메이션 (기존 로직)
    private async UniTask MoveAndFadeAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * moveDistance;
        float elapsedTime = 0f;

        // 위로 떠오르는 움직임
        while (elapsedTime < duration)
        {
            float easedProgress = Mathf.Pow(elapsedTime / duration, 2.5f); // 처음엔 느리게 떠오르다 점점 빨리 떠오르도록

            transform.position = Vector3.Lerp(startPos, endPos, easedProgress);

            if (elapsedTime > fadeOutDelay)
            {
                float fadeProgress = (elapsedTime - fadeOutDelay) / (duration - fadeOutDelay);
                SetAlpha(1f - fadeProgress);
            }

            elapsedTime += Time.deltaTime;
            await UniTask.Yield(this.GetCancellationTokenOnDestroy());
        }
    }

    // 모든 숫자 스프라이트의 알파(투명도) 값을 조절
    private void SetAlpha(float alpha)
    {
        foreach (var renderer in numberRenderers)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }
}