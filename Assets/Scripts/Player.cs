using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Player : Singleton<Player>
{
    public PlayerStat Stats { get; private set; }
    [HideInInspector] public float Atk;
    [HideInInspector] public float Def;
    [HideInInspector] public float MaxHP;
    [HideInInspector] public float MaxMP;
    [HideInInspector] public float CurrentHP;
    [HideInInspector] public float CurrentMP;
    [HideInInspector] public float WalkSpeed;
    [HideInInspector] public float RunSpeed;
    
    private InputHandler inputHandler;
    private BehaviourController behaviourController;
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
    public bool IsAttacking { get; set; } = false;
    public bool IsJumpAttacking { get; set; } = false;
    public bool IsHurt { get; set; } = false;

    protected override void Awake()
    {
        // 싱글턴 패턴
        base.Awake();

        // 컨트롤러 초기화
        inputHandler = new InputHandler();
        behaviourController = new BehaviourController(this, inputHandler);
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

        // PlayerStat 로드
        Stats = GetComponent<PlayerStat>();
        if (Stats != null)
        {
            this.Atk = Stats.Atk;
            this.Def = Stats.Def;
            this.MaxHP = Stats.MaxHP;
            this.MaxMP = Stats.MaxMP;
            this.CurrentHP = Stats.MaxHP;
            this.CurrentMP = Stats.MaxMP;
            this.WalkSpeed = Stats.MoveSpeed * 1.0f;
            this.RunSpeed = Stats.MoveSpeed * 2.0f;
        }
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        inputHandler.ReadInput(); // 1. 먼저 입력을 읽고
        behaviourController.Flip(); // 3. 입력을 바탕으로 게임 로직(점프, 방향 전환) 처리
        animController.UpdateAnimations(); // 4. 애니메이션 처리
    }

    private void FixedUpdate()
    {
        behaviourController.ApplyMovement(); // 물리 계산
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
                behaviourController.SubscribeToEvents();
                break;
        }
        IsRunning = false;
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
                behaviourController.ForceStopJump();
                break;
        }
        behaviourController.UnsubscribeFromEvents();
        animController.ResetAnimations();
    }

    public void SetState(PlayerState newState)
    {
        if (CurrentState == newState) return;

        ExitCurrentState(CurrentState);
        CurrentState = newState;
        EnterNewState(CurrentState);
    }

    public void TakeDamage(float monsterAtk)
    {

        float damage = (monsterAtk - (this.Def * 0.5f));
        damage = Mathf.RoundToInt(damage * Random.Range(0.9f, 1.1f));

        Debug.Log(damage + " 만큼의 피해를 입음");
        if (IsGrounded) Anim.SetBool("isGrounded", true);
        IsHurt = true;
        Anim.SetTrigger("isHurt");

        // 여기에 추가로 체력 감소, 넉백 등의 로직
        // ex) health -= damage;
        // ex) behaviourController.ApplyKnockback(...);
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
    #region Animation Event Receivers
    public void AnimEvent_OnAttackStart()
    {
        behaviourController?.OnAttackStart();
    }

    public void AnimEvent_OnComboWindowOpen()
    {
        behaviourController?.OnComboWindowOpen();
    }

    public void AnimEvent_OnComboWindowClose()
    {
        behaviourController?.OnComboWindowClose();
    }


    public void AnimEvent_OnAttackEnd()
    {
        behaviourController?.OnAttackEnd();
    }

    public void AnimEvent_OnHurtEnd()
    {
        behaviourController?.OnHurtEnd();
    }
    #endregion
}
