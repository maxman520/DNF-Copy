using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public enum PortalType
    {
        SameScene,      // 같은 씬 내에서 이동
        DifferentScene  // 다른 씬으로 이동
    }

    [Header("포탈 타입")]
    [Tooltip("포탈의 이동 방식 - 같은 씬 내에서 이동 / 다른 씬으로 이동")]
    [SerializeField] private PortalType portalType = PortalType.SameScene;

    [Header("참조")]
    [SerializeField] private GameObject visualInactive; // 비활성 상태 외형
    [SerializeField] private GameObject visualActive;   // 활성 상태 외형

    [Header("SameScene 타입 설정")]
    [Tooltip("같은 씬 내에서 이동할 때 사용")]
    [SerializeField] private int targetRoomIndex;
    [SerializeField] private Portal targetPortal;

    [Header("DifferentScene 타입 설정")]
    [Tooltip("다른 씬으로 이동할 때 사용")]
    [SerializeField] private string destinationSceneName;

    // 컴포넌트 참조
    private Collider2D portalCollider;

    private void Awake()
    {
        portalCollider = GetComponent<Collider2D>();
        // 시작 시에는 비활성화 상태로 시작
        Deactivate();
    }

    // 포탈 활성화
    public void Activate()
    {
        visualInactive?.SetActive(false);
        visualActive?.SetActive(true);

        portalCollider.enabled = true; // 충돌 감지도 활성화
        Debug.Log($"포탈({this.name})이 활성화됨");
    }

    // 포탈 비활성화
    public void Deactivate()
    {
        visualInactive?.SetActive(true);
        visualActive?.SetActive(false);

        portalCollider.enabled = false; // 충돌 감지도 비활성화
        Debug.Log($"포탈({this.name})이 비활성화됨");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어가 아니면 무시
        if (!other.CompareTag("PlayerGround")) return;

        Debug.Log("플레이어가 포탈에 진입");

        // 설정된 포탈 타입에 따라 다른 행동 수행
        switch (portalType)
        {
            case PortalType.SameScene:
                if (DungeonManager.Instance != null)
                {
                    Debug.Log($"{targetRoomIndex}번 방으로 이동");
                    DungeonManager.Instance.EnterRoom(targetRoomIndex, targetPortal);
                }
                break;

            case PortalType.DifferentScene:
                if (!string.IsNullOrEmpty(destinationSceneName))
                {
                    Debug.Log($"{destinationSceneName} 씬으로 이동");
                    GameManager.Instance.LoadScene(destinationSceneName);
                }
                else
                {
                    Debug.LogError($"'{this.name}' 포탈에 destinationSceneName이 설정되지 않았음");
                }
                break;
        }
    }
}