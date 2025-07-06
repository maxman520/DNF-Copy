using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(CanvasGroup))]
public class MonsterHPBar : MonoBehaviour
{
    [SerializeField] private Image faceImage;
    [SerializeField] private Image hpFillImage;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // 초상화 설정
    public void SetPortrait(Sprite portraitSprite)
    {
        if (faceImage != null && portraitSprite != null)
        {
            faceImage.sprite = portraitSprite;
        }
    }

    // HP 업데이트
    public void UpdateHP(float maxHP, float currentHP)
    {
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = currentHP / maxHP;
        }
    }

    // UI 보이기
    public void Show(float duration = 0.2f)
    {
        canvasGroup.alpha = 1f;
    }

    // UI 숨기기 (부드럽게)
    public async UniTask Hide(float duration = 0.2f)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }
        canvasGroup.alpha = 0f;
    }
}