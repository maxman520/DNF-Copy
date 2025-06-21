using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Player : Singleton<Player>
{
    public float WalkSpeed;
    public float RunSpeed;

    private InputHandler inputHandler;
    private MoveController moveController;
    private AnimController animController;

    // 상태 관리
    public enum PlayerState
    {
        Town,
        Dungeon
    }
    public PlayerState CurrentState { get; private set; } = PlayerState.Town;

    // 컴포넌트 참조
    public Rigidbody2D Rb { get; private set; }
    public Animator Anim { get; private set; }
    public Collider2D PlayerGround { get; private set; }
    public Transform VisualsTransform { get; private set; }

    // 상태 변수
    public bool IsGrounded { get; set; } = true;
    public bool IsRunning { get; set; } = false;
    public bool IsJumping { get; set; } = false;

    protected override void Awake()
    {
        // 싱글턴 패턴
        base.Awake();

        // 컨트롤러 초기화
        inputHandler = new InputHandler();
        moveController = new MoveController(this, inputHandler);
        animController = new AnimController(this, inputHandler);

        // 컴포넌트 참조 변수 초기화
        Rb = GetComponent<Rigidbody2D>();
        Anim = GetComponentInChildren<Animator>();
        PlayerGround = transform.Find("PlayerGround").GetComponent<BoxCollider2D>();
        VisualsTransform = transform.Find("Visuals");

        if (PlayerGround == null)
            Debug.LogError("PlayerGround를 찾을 수 없습니다.", this);
        if (VisualsTransform == null)
            Debug.LogError("Visuals Transform을 찾을 수 없습니다.", this);
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
        inputHandler.ReadInput(); // 1. 먼저 입력을 읽고
        moveController.Tick(); // 2. 입력을 바탕으로 게임 로직(점프, 방향 전환) 처리
        animController.UpdateAnimations(); // 3. 애니메이션 처리           
    }

    private void FixedUpdate()
    {
        moveController.ApplyMovement(); // 물리 계산
    }
    public void EnterNewState(PlayerState currentState)
    {
        switch (CurrentState)
        {
            case PlayerState.Town:
                Debug.Log("마을 상태에 진입");
                Anim.Play("Idle_Town");
                break;
            case PlayerState.Dungeon:
                Debug.Log("던전 상태에 진입");
                Anim.Play("Idle_Dungeon");
                moveController.SubscribeToEvents();
                break;
        }
        IsRunning = false;
        // moveController.SubscribeToEvents();
    }
    public void ExitCurrentState(PlayerState currentState)
    {
        switch (CurrentState)
        {
            case PlayerState.Town:
                Debug.Log("마을 상태를 벗어납니다.");
                // 다음 상태로 가기 전, 걷기 애니메이션 상태를 초기화
                Anim.SetBool("isWalking", false);
                break;
            case PlayerState.Dungeon:
                Debug.Log("던전 상태를 벗어남");
                IsRunning = false;
                moveController.ForceStopJump();
                break;
        }
        moveController.UnsubscribeFromEvents();
        animController.ResetAnimations();
    }

    public void SetState(PlayerState newState)
    {
        if (CurrentState == newState) return;

        ExitCurrentState(CurrentState);
        CurrentState = newState;
        EnterNewState(CurrentState);
    }

    public Coroutine StartCoroutineFromController(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }

    private void OnDisable()
    {
        inputHandler?.Dispose();

    }
    private void OnDestroy()
    {
        inputHandler?.Dispose();
    }
}