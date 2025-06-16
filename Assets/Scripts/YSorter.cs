using UnityEngine;

// ������ ��忡���� �ǽð����� Ȯ���ϱ� ���� �߰�
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
        // Y ��ǥ�� �������� sortingOrder ���� �۾����� �ڿ� �׷����� ��
        // !! ������Ʈ�� Pivot��ġ�� �߹�(Bottom)���� �����ؾ���!!!!!!!!!!!!
        // �����ִ� ���ڴ� ���е��� ���� (�Ҽ��� ��ǥ�� ������ ��ȯ)
        spriteRenderer.sortingOrder = -(int)(transform.position.y * 100);
    }
}