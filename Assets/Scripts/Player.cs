using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerStateInterface currentState; // ���� ����
    public readonly PlayerStateInterface townState = new InTownState(); // ���� ���� �ν��Ͻ�
    // public readonly IPlayerState dungeonState = new InDungeonState(); // ���߿� �߰��� ���� ����

    public Rigidbody2D Rigidbody { get; private set; }
    public Animator Animator { get; private set; }
    public float walkSpeed = 3f;
    public float runSpeed = 6f; // ���߿� ���

    [HideInInspector] public Vector2 moveInput;

    // === Input System ===
    [HideInInspector] public PlayerInputActions inputActions;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Animator = GetComponentInChildren<Animator>();

        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
    }

    private void Start()
    {
        // PlayerStats�κ��� �ൿ�� �ʿ��� ������ ����
        if (PlayerStats.Instance != null)
        {
            this.walkSpeed = PlayerStats.Instance.walkSpeed;
            this.runSpeed = PlayerStats.Instance.runSpeed;
        }
        // ���� ���� �� �ʱ� ���¸� '����'�� ����
        ChangeState(townState);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("'1' Ű �Է�: HP 10 ���� �׽�Ʈ");
            GameManager.Instance.TakeDamage(10);
        }
        // ��� ������Ʈ ������ ���� ���� ��ü���� ����
        currentState?.Update(this);
    }

    private void FixedUpdate()
    {
        // ��� ���� ������ ���� ���� ��ü���� ����
        currentState?.FixedUpdate(this);
    }

    // ���¸� �����ϴ� ���� �޼���
    public void ChangeState(PlayerStateInterface newState)
    {
        // ���� ������ Exit ���� ����
        currentState?.Exit(this);

        // ���ο� ���·� ��ü�ϰ� Enter ���� ����
        currentState = newState;
        currentState.Enter(this);
    }

    // ������Ʈ�� ��Ȱ��ȭ�� �� Input Actions�� ��Ȱ��ȭ
    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

}