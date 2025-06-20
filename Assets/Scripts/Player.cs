using UnityEngine;
using System.Collections;

public class Player : Singleton<Player>
{
    public float walkSpeed;
    public float runSpeed;

    // ���� ����
    private PlayerStateInterface currentState;
    public InTownState townState { get; private set; }
    public InDungeonState dungeonState { get; private set; }

    // ������Ʈ ����
    public Rigidbody2D rb { get; private set; }
    public Animator anim { get; private set; }
    public Collider2D playerGround { get; private set; }
    public Transform visualsTransform { get; private set; }

    // ���� ����
    public bool isGrounded { get; set; } = true;
    public bool isRunning { get; set; } = false;
    public bool isJumping { get; set; } = false;

    // �Է� ����
    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public PlayerInputActions inputActions;


    protected override void Awake()
    {
        // �̱��� ����
        base.Awake();

        // ������Ʈ ���� ���� �ʱ�ȭ
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        playerGround = transform.Find("PlayerGround").GetComponent<BoxCollider2D>();
        visualsTransform = transform.Find("Visuals");

        if (playerGround == null)
            Debug.LogError("PlayerGround�� ã�� �� �����ϴ�.", this);
        if (visualsTransform == null)
            Debug.LogError("Visuals Transform�� ã�� �� �����ϴ�.", this);

        // ���� �ʱ�ȭ
        townState = new InTownState(this);
        dungeonState = new InDungeonState(this);
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
    }

    private void Start()
    {
        // PlayerStats �ε�
        if (PlayerStats.Instance != null)
        {
            this.walkSpeed = PlayerStats.Instance.moveSpeed * 1.0f;
            this.runSpeed = PlayerStats.Instance.moveSpeed * 2.0f;
        }
    }

    private void Update()
    {
        currentState?.Update();
    }

    private void FixedUpdate()
    {
        currentState?.FixedUpdate();
    }

    public void ChangeState(PlayerStateInterface newState)
    {
        if (newState == null)
        {
            Debug.LogError("���ο� ���°� null�Դϴ�.");
            return;
        }
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }
    public Coroutine StartCoroutineFromState(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }

    private void OnDisable()
    {
        inputActions?.Player.Disable();
    }

    private void OnDestroy()
    {
        inputActions?.Dispose();
    }
}