using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public enum PortalType
    {
        SameScene,      // ���� �� ������ �̵�
        DifferentScene  // �ٸ� ������ �̵�
    }

    [Header("��Ż Ÿ��")]
    [Tooltip("��Ż�� �̵� ��� - ���� �� ������ �̵� / �ٸ� ������ �̵�")]
    [SerializeField] private PortalType portalType = PortalType.SameScene;

    [Header("����")]
    [SerializeField] private GameObject visualInactive; // ��Ȱ�� ���� ����
    [SerializeField] private GameObject visualActive;   // Ȱ�� ���� ����

    [Header("SameScene Ÿ�� ����")]
    [Tooltip("���� �� ������ �̵��� �� ���")]
    [SerializeField] private int targetRoomIndex;
    [SerializeField] private Portal targetPortal;

    [Header("DifferentScene Ÿ�� ����")]
    [Tooltip("�ٸ� ������ �̵��� �� ���")]
    [SerializeField] private string destinationSceneName;

    // ������Ʈ ����
    private Collider2D portalCollider;

    private void Awake()
    {
        portalCollider = GetComponent<Collider2D>();
        // ���� �ÿ��� ��Ȱ��ȭ ���·� ����
        Deactivate();
    }

    // ��Ż Ȱ��ȭ
    public void Activate()
    {
        visualInactive?.SetActive(false);
        visualActive?.SetActive(true);

        portalCollider.enabled = true; // �浹 ������ Ȱ��ȭ
        Debug.Log($"��Ż({this.name})�� Ȱ��ȭ��");
    }

    // ��Ż ��Ȱ��ȭ
    public void Deactivate()
    {
        visualInactive?.SetActive(true);
        visualActive?.SetActive(false);

        portalCollider.enabled = false; // �浹 ������ ��Ȱ��ȭ
        Debug.Log($"��Ż({this.name})�� ��Ȱ��ȭ��");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // �÷��̾ �ƴϸ� ����
        if (!other.CompareTag("PlayerGround")) return;

        Debug.Log("�÷��̾ ��Ż�� ����");

        // ������ ��Ż Ÿ�Կ� ���� �ٸ� �ൿ ����
        switch (portalType)
        {
            case PortalType.SameScene:
                if (DungeonManager.Instance != null)
                {
                    Debug.Log($"{targetRoomIndex}�� ������ �̵�");
                    DungeonManager.Instance.EnterRoom(targetRoomIndex, targetPortal);
                }
                break;

            case PortalType.DifferentScene:
                if (!string.IsNullOrEmpty(destinationSceneName))
                {
                    Debug.Log($"{destinationSceneName} ������ �̵�");
                    SceneManager.LoadScene(destinationSceneName);
                }
                else
                {
                    Debug.LogError($"'{this.name}' ��Ż�� destinationSceneName�� �������� �ʾ���");
                }
                break;
        }
    }
}