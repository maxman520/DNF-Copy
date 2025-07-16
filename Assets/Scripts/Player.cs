using System;
using System.Collections.Generic;
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
    [Header("히트박스 참조")]
    [SerializeField] private PlayerHitbox comboHitbox; // 일반 콤보 공격에 사용할 히트박스


    private InputHandler inputHandler;
    private BehaviourController behaviourController;
    private AnimController animController;
    private SkillManager skillManager;

    public PlayerAnimState CurrentAnimState { get; set; }

    // 컴포넌트 참조
    public Rigidbody2D Rb { get; private set; }
    public Animator Anim { get; private set; }
    public Collider2D PlayerGround { get; private set; }
    public Transform VisualsTransform { get; private set; }
    public Transform HurtboxTransform { get; private set; }

    protected override void Awake()
    {
        // 싱글턴 패턴
        base.Awake();

        CurrentAnimState = PlayerAnimState.Idle;

        // 컴포넌트 참조 변수 초기화
        Rb = GetComponent<Rigidbody2D>();
        Anim = GetComponentInChildren<Animator>();
        PlayerGround = transform.Find("Player_Ground").GetComponent<BoxCollider2D>();
        VisualsTransform = transform.Find("Visuals");
        HurtboxTransform = VisualsTransform.Find("Hurtbox");

        // 컨트롤러 초기화
        inputHandler = new InputHandler();
        animController = new AnimController(this);
        skillManager = GetComponent<SkillManager>();
        behaviourController = new BehaviourController(this, inputHandler, animController, skillManager);

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
        HandleCommands();
        behaviourController.Flip(); // 방향 전환 처리
        behaviourController.HandleGravity(); // 플레이어 visuals의 localPosition.y에 따라 중력 처리
        animController.UpdateAnimations(); // 애니메이션 처리
    }

    private void FixedUpdate()
    {
        behaviourController.ApplyMovement(); // 플레이어 이동 처리
    }
    private void HandleCommands()
    {
        // 버퍼에서 커맨드를 하나 가져옴
        ICommand command = inputHandler.PeekCommand();

        if (command != null)
        {
            // 커맨드를 실행, 성공하면 커맨드를 버퍼에서 제거
            if (command.Execute(behaviourController))
            {
                inputHandler.RemoveCommand();
            }
        }
    }
    public bool HasState(PlayerAnimState state)
    {
        return (CurrentAnimState.HasFlag(state));
    }

    public void OnDamaged(AttackDetails attackDetails, Vector3 attackPosition)
    {
        // 입을 데미지 계산
        float damage = CalculateDamage(attackDetails);

        // 데미지 텍스트 출력
        EffectManager.Instance.PlayEffect("HurtDamageText",HurtboxTransform.position, Quaternion.identity, damage);

        // 피격 반응
        behaviourController.HandleHurt(attackDetails, attackPosition);

        // 이미 죽었다면 데미지 적용X. return

        // 데미지 적용
        // ex) health -= damage;
        // Debug.Log(damage + " 만큼의 피해를 입음");
        // ex) behaviourController.ApplyKnockback(...); // 넉백 및 피격 반응 처리
        // BehaviourController에 위임하는 것이 좋음
    }
    private float CalculateDamage(AttackDetails attackDetails)
    {
        // !! 데미지 배율에 몬스터의 공격력이 이미 곱해져있음 !!
        float finalDamage = (attackDetails.damageRate) - (Def * 0.5f);
        finalDamage = Mathf.Max(1, Mathf.RoundToInt(finalDamage * UnityEngine.Random.Range(0.8f, 1.2f)));

        return finalDamage;
    }


    // 던전 입장 시 GameManager에 의해 호출
    public void OnEnterDungeon()
    {
        Anim.Play("Idle_Battle");
    }
    // 던전 퇴장 시 GameManager에 의해 호출
    public void OnExitDungeon()
    {
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
    public void SetAttackDetails(int index)
    {
        // 1. 공격 정보 가져오기
        AttackDetails details = AttackDetails[index];

        // 2. Y축 판정 기준점 결정
        //    일반 공격은 항상 플레이어의 발밑을 기준으로 함
        float originY = this.transform.position.y;

        // 3. 히트박스 초기화
        if (comboHitbox != null)
        {
            comboHitbox.Initialize(details, originY);
        }
    }
    #endregion Animation Event Receivers
#if UNITY_EDITOR // 에디터에서만 실행되도록 전처리기 사용 (빌드 시 제외)
    // 디버깅 UI를 켤지 말지 결정하는 변수
    [Header("디버그 옵션")]
    [SerializeField] private bool showInputBufferUI = true;

    private void OnGUI()
    {
        if (!showInputBufferUI) return;

        

        // UI 스타일 설정
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;

        // 화면 좌상단에 UI 영역을 만듦
        GUILayout.BeginArea(new Rect(10, 10, 300, 500));

        GUILayout.Label("--- Input Buffer ---", style);

        if (inputHandler != null)
        {
            // InputHandler로부터 버퍼에 있는 커맨드 이름 목록을 가져옴
            List<string> bufferedCommands = inputHandler.GetBufferedCommandNames();

            if (bufferedCommands.Count == 0)
            {
                GUILayout.Label("(Empty)", style);
            }
            else
            {
                // 버퍼에 있는 각 커맨드를 화면에 표시
                foreach (string commandName in bufferedCommands)
                {
                    GUILayout.Label(commandName, style);
                }
            }
        }

        int i = 100;
        GUI.Label(new Rect(10, i+10, 200, 20), "IsRunning: " + IsRunning);
        GUI.Label(new Rect(10, i+20, 200, 20), "CanMove: " + CanMove);
        GUI.Label(new Rect(10, i+30, 200, 20), "CanAttack: " + CanAttack);
        GUI.Label(new Rect(10, i+40, 200, 20), "CanContinueAttack: " + CanContinueAttack);
        GUI.Label(new Rect(10, i+50, 200, 20), "AttackCounter: " + AttackCounter);


        GUILayout.EndArea();

    }
#endif // UNITY_EDITOR
}
