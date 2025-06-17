using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerStateInterface currentState; // 현재 상태
    public readonly PlayerStateInterface townState = new InTownState(); // 마을 상태 인스턴스
    // public readonly IPlayerState dungeonState = new InDungeonState(); // 나중에 추가할 던전 상태

    public Rigidbody2D Rigidbody { get; private set; }
    public Animator Animator { get; private set; }
    public float walkSpeed = 3f;
    public float runSpeed = 6f; // 나중에 사용

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
        // PlayerStats로부터 행동에 필요한 데이터 복사
        if (PlayerStats.Instance != null)
        {
            this.walkSpeed = PlayerStats.Instance.walkSpeed;
            this.runSpeed = PlayerStats.Instance.runSpeed;
        }
        // 게임 시작 시 초기 상태를 '마을'로 설정
        ChangeState(townState);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("'1' 키 입력: HP 10 감소 테스트");
            GameManager.Instance.TakeDamage(10);
        }
        // 모든 업데이트 로직을 현재 상태 객체에게 위임
        currentState?.Update(this);
    }

    private void FixedUpdate()
    {
        // 모든 물리 로직을 현재 상태 객체에게 위임
        currentState?.FixedUpdate(this);
    }

    // 상태를 변경하는 공용 메서드
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