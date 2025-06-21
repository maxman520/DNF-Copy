using UnityEngine;
using System.Collections;

public class Player : Singleton<Player>
{
    public float WalkSpeed;
    public float RunSpeed;

    // 상태 관리
    private IPlayerState currentState;
    public InTownState TownState { get; private set; }
    public InDungeonState DungeonState { get; private set; }

    // 컴포넌트 참조
    public Rigidbody2D Rb { get; private set; }
    public Animator Anim { get; private set; }
    public Collider2D PlayerGround { get; private set; }
    public Transform VisualsTransform { get; private set; }

    // 상태 변수
    public bool IsGrounded { get; set; } = true;
    public bool IsRunning { get; set; } = false;
    public bool IsJumping { get; set; } = false;

    // 입력 관리
    [HideInInspector] public Vector2 MoveInput;
    [HideInInspector] public PlayerInputActions InputActions;


    protected override void Awake()
    {
        // 싱글턴 패턴
        base.Awake();

        // 컴포넌트 참조 변수 초기화
        Rb = GetComponent<Rigidbody2D>();
        Anim = GetComponentInChildren<Animator>();
        PlayerGround = transform.Find("PlayerGround").GetComponent<BoxCollider2D>();
        VisualsTransform = transform.Find("Visuals");

        if (PlayerGround == null)
            Debug.LogError("PlayerGround를 찾을 수 없습니다.", this);
        if (VisualsTransform == null)
            Debug.LogError("Visuals Transform을 찾을 수 없습니다.", this);

        // 상태 초기화
        TownState = new InTownState(this);
        DungeonState = new InDungeonState(this);
        InputActions = new PlayerInputActions();
        InputActions.Player.Enable();
    }

    private void Start()
    {
        // PlayerStats 로드
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
        InputActions?.Player.Disable();
    }

    private void OnDestroy()
    {
        InputActions?.Dispose();
    }
}