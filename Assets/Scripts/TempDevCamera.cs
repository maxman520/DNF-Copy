using UnityEngine;

// ���� ����(������)�� �ƴ� ���� �����θ� �ı��ϰ�, �ڽ��� ��Ȱ��ȭ
public class TempDevCamera : MonoBehaviour
{
    void Awake()
    {
#if !UNITY_EDITOR
        Destroy(gameObject);
#else
        gameObject.SetActive(false);
#endif
    }
}