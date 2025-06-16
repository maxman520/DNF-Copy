using UnityEngine;

// 에디터 모드에서도 실시간으로 확인하기 위해 추가
[ExecuteInEditMode]
public class YSorter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Y 좌표가 높을수록 sortingOrder 값이 작아져서 뒤에 그려지게 함
        // !! 오브젝트의 Pivot위치를 발밑(Bottom)으로 통일해야함!!!!!!!!!!!!
        // 곱해주는 숫자는 정밀도를 위함 (소수점 좌표를 정수로 변환)
        spriteRenderer.sortingOrder = -(int)(transform.position.y * 100);
    }
}