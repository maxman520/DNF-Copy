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
    [SerializeField] private float duration = 0.2f;     // 전체 지속 시간
    [SerializeField] private float fadeOutDelay = 0.15f; // 사라지기 시작하는 시간

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

    // 위로 떠오르다 사라지는 애니메이션
    private async UniTask Animate()
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

        // 애니메이션이 끝나면 풀에 반납
        EffectManager.Instance.ReturnEffectToPool("DamageText", this.gameObject);
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