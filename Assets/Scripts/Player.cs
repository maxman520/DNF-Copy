using UnityEngine;

public class Player : Singleton<Player>
{
    private PlayerStateInterface currentState; // ���� ����
    public readonly PlayerStateInterface townState = new InTownState(); // ���� ����
    public readonly PlayerStateInterface dungeonState = new InDungeonState(); // ���� ����

    public Rigidbody2D Rigidbody { get; private set; }
    public Animator Animator { get; private set; }
    public float walkSpeed;
    public float runSpeed;

    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public PlayerInputActions inputActions;

    protected override void Awake()
    {
        // �̱��� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        // ������Ʈ �ʱ�ȭ
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
    }

    private void Update()
    {
        // ��� ������Ʈ ������ ���� ���� ��ü���� ����
        currentState?.Update(this);

        // === UI �۵� �׽�Ʈ�� �ڵ� ===
        // '1'�� Ű�� ������ GameManager���� �������� �޾Ҵٰ� �˸�
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("HP -10 �׽�Ʈ");
            GameManager.Instance.TakeDamage(10);
        }

        // '2'�� Ű�� ������ GameManager���� ������ ����ߴٰ� �˸�
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("MP -10 �׽�Ʈ");
            GameManager.Instance.UseMana(10);
        }
        // === �׽�Ʈ�� �ڵ� ===

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