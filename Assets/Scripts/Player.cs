using UnityEngine;

public class Player : Singleton<Player>
{
    private PlayerStateInterface currentState; // 현재 상태
    public readonly PlayerStateInterface townState = new InTownState(); // 마을 상태
    public readonly PlayerStateInterface dungeonState = new InDungeonState(); // 던전 상태

    public Rigidbody2D rb { get; private set; }
    public Animator anim { get; private set; }
    public float walkSpeed;
    public float runSpeed;

    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public PlayerInputActions inputActions;

    protected override void Awake()
    {
        // 싱글턴 패턴
        base.Awake();

        // 컴포넌트 초기화
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
    }

    private void Start()
    {
        // PlayerStats로부터 행동에 필요한 데이터 복사
        if (PlayerStats.Instance != null)
        {
            this.walkSpeed = PlayerStats.Instance.walkSpeed;
            this.runSpeed = PlayerStats.Instance.runSpeed;
        }
    }

    private void Update()
    {
        // 모든 업데이트 로직을 현재 상태 객체에게 위임
        currentState?.Update(this);

    }

    private void FixedUpdate()
    {
        // 모든 물리 로직을 현재 상태 객체에게 위임
        currentState?.FixedUpdate(this);
    }

    // 상태를 변경하는 메소드
    public void ChangeState(PlayerStateInterface newState)
    {
        // 이전 상태의 Exit 로직 실행
        currentState?.Exit(this);

        // 새로운 상태로 교체하고 Enter 로직 실행
        currentState = newState;
        currentState.Enter(this);
    }

    // 오브젝트가 비활성화될 때 Input Actions도 비활성화
    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

}