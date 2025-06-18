using UnityEngine;

public class Player : Singleton<Player>
{
    private PlayerStateInterface currentState; // 현재 상태
    public readonly PlayerStateInterface townState = new InTownState(); // 마을 상태
    public readonly PlayerStateInterface dungeonState = new InDungeonState(); // 던전 상태

    public Rigidbody2D Rigidbody { get; private set; }
    public Animator Animator { get; private set; }
    public float walkSpeed;
    public float runSpeed;

    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public PlayerInputActions inputActions;

    protected override void Awake()
    {
        // 싱글턴 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        // 컴포넌트 초기화
        Rigidbody = GetComponent<Rigidbody2D>();
        Animator = GetComponentInChildren<Animator>();
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

        // === UI 작동 테스트용 코드 ===
        // '1'번 키를 누르면 GameManager에게 데미지를 받았다고 알림
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("HP -10 테스트");
            GameManager.Instance.TakeDamage(10);
        }

        // '2'번 키를 누르면 GameManager에게 마나를 사용했다고 알림
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("MP -10 테스트");
            GameManager.Instance.UseMana(10);
        }
        // === 테스트용 코드 ===

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