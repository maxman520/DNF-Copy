using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup))]
public class YSorter : MonoBehaviour
{
    private SortingGroup sortingGroup;
    private Transform rootTransform; // 이 오브젝트의 최상위 Transform 참조

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