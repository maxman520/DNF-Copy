using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FitImageToSprite : MonoBehaviour
{
    private Image image;
    private Sprite lastSprite;

    void Awake()
    {
        image = GetComponent<Image>();
        // 시작할 때의 스프라이트를 저장
        lastSprite = image.sprite;
    }

    void LateUpdate()
    {
        Sprite currentSprite = image.sprite;

        // 현재 스프라이트가 존재하고, 이전 프레임의 스프라이트와 다르다면
        if (currentSprite != null && currentSprite != lastSprite)
        {
            // 이미지의 크기를 현재 스프라이트의 원본 크기로
            image.SetNativeSize();
            // 현재 스프라이트를 lastSprite로 기록하여 다음 프레임에 비교
            lastSprite = currentSprite;
        }
    }
}
