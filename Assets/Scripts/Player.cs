using UnityEngine;
using System.Collections;

public class Player : Singleton<Player>
{
    public float WalkSpeed;
    public float RunSpeed;

    // ���� ����
    private IPlayerState currentState;
    public InTownState TownState { get; private set; }
    public InDungeonState DungeonState { get; private set; }

    // ������Ʈ ����
    public Rigidbody2D Rb { get; private set; }
    public Animator Anim { get; private set; }
    public Collider2D PlayerGround { get; private set; }
    public Transform VisualsTransform { get; private set; }

    // ���� ����
    public bool IsGrounded { get; set; } = true;
    public bool IsRunning { get; set; } = false;
    public bool IsJumping { get; set; } = false;

    // �Է� ����
    [HideInInspector] public Vector2 MoveInput;
    [HideInInspector] public PlayerInputActions InputActions;


    protected override void Awake()
    {
        // �̱��� ����
        base.Awake();

        // ������Ʈ ���� ���� �ʱ�ȭ
        Rb = GetComponent<Rigidbody2D>();
        Anim = GetComponentInChildren<Animator>();
        PlayerGround = transform.Find("PlayerGround").GetComponent<BoxCollider2D>();
        VisualsTransform = transform.Find("Visuals");

        if (PlayerGround == null)
            Debug.LogError("PlayerGround�� ã�� �� �����ϴ�.", this);
        if (VisualsTransform == null)
            Debug.LogError("Visuals Transform�� ã�� �� �����ϴ�.", this);

        // ���� �ʱ�ȭ
        TownState = new InTownState(this);
        DungeonState = new InDungeonState(this);
        InputActions = new PlayerInputActions();
        InputActions.Player.Enable();
    }

    private void Start()
    {
        // PlayerStats �ε�
        if (PlayerStats.Instance != null)
        {
            this.WalkSpeed = PlayerStats.Instance.MoveSpeed * 1.0f;
            this.RunSpeed = PlayerStats.Instance.MoveSpeed * 2.0f;
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

    public void ChangeState(IPlayerState newState)
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
        InputActions?.Player.Disable();
    }

    private void OnDestroy()
    {
        InputActions?.Dispose();
    }
}