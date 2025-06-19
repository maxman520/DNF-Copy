using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup))]
public class YSorter : MonoBehaviour
{
    private SortingGroup sortingGroup;
    private Transform rootTransform; // �� ������Ʈ�� �ֻ��� Transform ����

    void Awake()
    {
        sortingGroup = GetComponent<SortingGroup>();
        rootTransform = transform.parent;
    }

    void LateUpdate()
    {
        float pivotY = rootTransform.position.y;
        sortingGroup.sortingOrder = -(int)(pivotY * 100);
    }
}