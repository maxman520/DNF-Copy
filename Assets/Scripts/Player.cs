using UnityEngine;

public class Player : Singleton<Player>
{
    private PlayerStateInterface currentState; // ���� ����
    public readonly PlayerStateInterface townState = new InTownState(); // ���� ����
    public readonly PlayerStateInterface dungeonState = new InDungeonState(); // ���� ����

    public Rigidbody2D rb { get; private set; }
    public Animator anim { get; private set; }
    public float walkSpeed;
    public float runSpeed;

    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public PlayerInputActions inputActions;

    protected override void Awake()
    {
        // �̱��� ����
        base.Awake();

        // ������Ʈ �ʱ�ȭ
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
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
    }

    private void Update()
    {
        // ��� ������Ʈ ������ ���� ���� ��ü���� ����
        currentState?.Update(this);

    }

    private void FixedUpdate()
    {
        // ��� ���� ������ ���� ���� ��ü���� ����
        currentState?.FixedUpdate(this);
    }

    // ���¸� �����ϴ� �޼ҵ�
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