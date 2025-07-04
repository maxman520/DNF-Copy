using System;
using UnityEngine;

[Flags]
public enum PlayerAnimState
{
    None = 0,
    Idle = 1 << 0,
    Move = 1 << 1,
    Run = 1 << 2,
    Jump = 1 << 3,
    Attack = 1 << 4,
    Hurt = 1 << 5,
    Airborne = 1 << 6,
}

public class Player : Singleton<Player>
{
    public PlayerStat Stats { get; private set; }
    [Header("플레이어 스탯")]
    public float Atk;
    public float Def;
    public float MaxHP;
    public float MaxMP;
    public float CurrentHP;
    public float CurrentMP;
    public float WalkSpeed;
    public float RunSpeed;


    // 상태 변수
    public bool IsGrounded = true;
    public bool IsRunning = false;
    public bool IsMoving { get; set; } = false;

    public bool CanMove { get; set; } = true;
    public bool CanAttack { get; set; } = true;
    public bool CanContinueAttack { get; set; } = false;
    public int AttackCounter = 0;

    [Header("공격 정보")]
    public AttackDetails[] AttackDetails;


    private InputHandler inputHandler;
    private BehaviourController behaviourController;
    private AnimController animController;


    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), "IsRunning: " + IsRunning);
        GUI.Label(new Rect(10, 20, 200, 20), "CanMove: " + CanMove);
        GUI.Label(new Rect(10, 30, 200, 20), "CanAttack: " + CanAttack);
        GUI.Label(new Rect(10, 40, 200, 20), "CanContinueAttack: " + CanContinueAttack);
        GUI.Label(new Rect(10, 50, 200, 20), "AttackCounter: " + AttackCounter);
    }
    public PlayerAnimState CurrentAnimState { get; set; }

    // 컴포넌트 참조
    public Rigidbody2D Rb { get; private set; }
    public Animator Anim { get; private set; }
    public Collider2D PlayerGround { get; private set; }
    public Transform VisualsTransform { get; private set; }

    protected override void Awake()
    {
        // 싱글턴 패턴
        base.Awake();


        CurrentAnimState = PlayerAnimState.Idle;

        // 컴포넌트 참조 변수 초기화
        Rb = GetComponent<Rigidbody2D>();
        Anim = GetComponentInChildren<Animator>();
        PlayerGround = transform.Find("PlayerGround").GetComponent<BoxCollider2D>();
        VisualsTransform = transform.Find("Visuals");

        // 컨트롤러 초기화
        inputHandler = new InputHandler();
        animController = new AnimController(this);
        behaviourController = new BehaviourController(this, inputHandler, animController);

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

    private void Update()
    {
        inputHandler.ReadInput(); // 입력을 읽고
        behaviourController.Flip(); // 입력을 바탕으로 방향 전환 처리
        behaviourController.HandleGravity();
        animController.UpdateAnimations(); // 애니메이션 처리
    }

    private void FixedUpdate()
    {
        behaviourController.ApplyMovement(); // 플레이어 이동 처리
    }
    public bool HasState(PlayerAnimState state)
    {
        return (CurrentAnimState.HasFlag(state));
    }

    public void OnDamaged(float monsterAtk)
    {

        float damage = (monsterAtk - (this.Def * 0.5f));
        damage = Mathf.RoundToInt(damage * UnityEngine.Random.Range(0.8f, 1.2f));

        Debug.Log(damage + " 만큼의 피해를 입음");
        Anim.SetTrigger("hurt");

        // 여기에 추가로 체력 감소, 넉백 등의 로직
        // ex) health -= damage;
        // ex) behaviourController.ApplyKnockback(...);
    }

    // 던전 입장 시 GameManager에 의해 호출
    public void OnEnterDungeon()
    {
        behaviourController.SubscribeToEvents();
        Anim.Play("Idle_Battle");
    }
    // 던전 퇴장 시 GameManager에 의해 호출
    public void OnExitDungeon()
    {
        behaviourController.UnsubscribeFromEvents();
        animController.ResetAnimations();
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
    public void AnimEvent_OnComboWindowOpen()
    {
        behaviourController?.OnComboWindowOpen();
    }

    public void AnimEvent_OnComboWindowClose()
    {
        behaviourController?.OnComboWindowClose();
    }
    #endregion
}
