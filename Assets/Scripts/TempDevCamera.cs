using UnityEngine;

// 개발 빌드(에디터)가 아닐 때는 스스로를 파괴하고, 자신을 비활성화
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