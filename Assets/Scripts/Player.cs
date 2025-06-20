using UnityEngine;
using System.Collections;

public class Player : Singleton<Player>
{
    public float walkSpeed;
    public float runSpeed;

    // 상태 관리
    private PlayerStateInterface currentState;
    public InTownState townState { get; private set; }
    public InDungeonState dungeonState { get; private set; }

    // 컴포넌트 참조
    public Rigidbody2D rb { get; private set; }
    public Animator anim { get; private set; }
    public Collider2D playerGround { get; private set; }
    public Transform visualsTransform { get; private set; }

    // 상태 변수
    public bool isGrounded { get; set; } = true;
    public bool isRunning { get; set; } = false;
    public bool isJumping { get; set; } = false;

    // 입력 관리
    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public PlayerInputActions inputActions;


    protected override void Awake()
    {
        // 싱글턴 패턴
        base.Awake();

        // 컴포넌트 참조 변수 초기화
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        playerGround = transform.Find("PlayerGround").GetComponent<BoxCollider2D>();
        visualsTransform = transform.Find("Visuals");

        if (playerGround == null)
            Debug.LogError("PlayerGround를 찾을 수 없습니다.", this);
        if (visualsTransform == null)
            Debug.LogError("Visuals Transform을 찾을 수 없습니다.", this);

        // 상태 초기화
        townState = new InTownState(this);
        dungeonState = new InDungeonState(this);
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
    }

    private void Start()
    {
        // PlayerStats 로드
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
            Debug.LogError("새로운 상태가 null입니다.");
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