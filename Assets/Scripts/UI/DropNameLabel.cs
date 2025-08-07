using UnityEngine;
using TMPro;

public class DropNameLabel : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private TextMeshPro label;
    [SerializeField] private SpriteRenderer bg; // 단일 배경 스프라이트

    [Header("레이아웃 설정")]
    [SerializeField] private float paddingX = 0.2f; // 좌우 여백
    [SerializeField] private float paddingY = 0.1f; // 상하 여백
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.8f, 0f); // Y 오프셋 유지

    private Transform target; // 따라갈 대상(드랍 루트 등)

    public void Initialize(Transform followTarget, string text)
    {
        target = followTarget;
        if (label != null) label.text = text;
        LayoutNow();
        UpdateTransform();
    }

    public void SetText(string text)
    {
        if (label == null) return;
        label.text = text;
        LayoutNow();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            target = transform.parent;
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }
        }

        UpdateTransform();
        LayoutNow();
    }

    private void UpdateTransform()
    {
        transform.position = target.position + offset;
        var cam = Camera.main;
        if (cam) transform.forward = cam.transform.forward; // 빌보드 유지
    }

    private void LayoutNow()
    {
        if (label == null || bg == null || bg.sprite == null) return;

        label.ForceMeshUpdate();

        float targetW = Mathf.Max(0.0001f, label.preferredWidth  + paddingX * 2f);
        float targetH = Mathf.Max(0.0001f, label.preferredHeight + paddingY * 2f);

        // 스프라이트 원본 월드 크기 대비 스케일 산출
        Vector2 spriteSize = bg.sprite.bounds.size;
        float sx = targetW / Mathf.Max(0.0001f, spriteSize.x);
        float sy = targetH / Mathf.Max(0.0001f, spriteSize.y);

        var ls = bg.transform.localScale;
        bg.transform.localScale = new Vector3(sx, sy, ls.z);

        // 중앙 정렬
        label.transform.localPosition = Vector3.zero;
        bg.transform.localPosition = Vector3.zero;
    }
}
