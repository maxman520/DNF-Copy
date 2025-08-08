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
    [Header("플레이어 스탯")]
    // 최종 스탯 (기본 + 장비)
    public float Atk;
    public float Def;
    
    // 기본 스탯 (레벨업, 캐릭터 고유값)
    private float baseAtk;
    private float baseDef;

    public float MaxHP;
    public float MaxMP;
    public float CurrentHP;
    public float CurrentMP;
    public float WalkSpeed;
    public float RunSpeed;
    public int CurrentEXP;
    public int Level = 1;
    public int RequiredEXP = 1000; // 1레벨에서 2레벨로 가는 데 필요한 경험치


    // 상태 변수
    public bool IsGrounded = true;
    public bool IsRunning = false;
    public bool IsMoving { get; set; } = false;

    public bool CanMove { get; set; } = true;
    public bool CanAttack { get; set; } = true;
    public bool CanContinueAttack { get; set; } = false;
    public int AttackCounter = 0;

    public DropItem ItemToPickUp { get; set; }

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
        animController.ResetAnimations();
        
        // 던전으로 이동시 체력, 마나 회복 (던전에서 바로 다음 던전으로 이동 시 대비)
        CurrentHP = MaxHP;
        CurrentMP = MaxMP;
    }
    // 던전 퇴장, 마을 입장 시 GameManager에 의해 호출
    public void OnEnterTown()
    {
        Anim.Play("Idle");
        animController.ResetAnimations();

        // 마을로 이동시 체력, 마나 회복
        CurrentHP = MaxHP;
        CurrentMP = MaxMP;
    }

    public void InitializeStats(CharacterData data)
    {
        // 기본 스탯 설정
        this.baseAtk = data.Atk;
        this.baseDef = data.Def;

        // 최종 스탯 초기화 (처음엔 장비가 없으므로 기본 스탯과 동일)
        this.Atk = this.baseAtk;
        this.Def = this.baseDef;

        this.MaxHP = data.MaxHP;
        this.CurrentHP = this.MaxHP;
        this.MaxMP = data.MaxMP;
        this.CurrentMP = this.MaxMP;
        this.WalkSpeed = data.MoveSpeed * 1.0f;
        this.RunSpeed = data.MoveSpeed * 2.0f;
        this.Level = data.Level;
        this.CurrentEXP = data.CurrentEXP;
        this.RequiredEXP = data.RequiredEXP;

        // UI 업데이트 요청
        UIManager.Instance.UpdateHP(MaxHP, CurrentHP);
        UIManager.Instance.UpdateMP(MaxMP, CurrentMP);
        UIManager.Instance.UpdateEXP(RequiredEXP, CurrentEXP);
    }

    // PlayerEquipment에서 호출하여 장비로 인한 스탯 변동을 최종 스탯에 반영
    public void UpdateEquipmentStats(int totalAttack, int totalDefense)
    {
        Atk = baseAtk + totalAttack;
        Def = baseDef + totalDefense;

        Debug.Log($"장비 스탯 적용 완료. 최종 공격력: {Atk}, 최종 방어력: {Def}");
        // TODO: 여기에 스탯 변경 시 업데이트가 필요한 UI 호출 (캐릭터 정보 창 등)
    }

    public void AddExp(int expAmount)
    {
        CurrentEXP += expAmount;
        Debug.Log($"경험치 {expAmount} 획득! 현재 경험치: {CurrentEXP}");
        // UI 업데이트 요청
        UIManager.Instance.UpdateEXP(RequiredEXP, CurrentEXP);

        while (CurrentEXP >= RequiredEXP)
        {
            CurrentEXP -= RequiredEXP;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Level++;
        // 다음 레벨업에 필요한 경험치 계산 (예: 이전 경험치의 1.25배)
        RequiredEXP = Mathf.RoundToInt(RequiredEXP * 1.25f);
        Debug.Log($"레벨업! 현재 레벨: {Level}, 다음 레벨업까지 필요한 경험치: {RequiredEXP}");
        // 레벨업 이펙트 호출
        // ..

        // 레벨업에 따른 스탯 증가 로직 (MaxHP, Atk 등)
        MaxHP = Mathf.RoundToInt(MaxHP * 1.1f);
        MaxMP = Mathf.RoundToInt(MaxMP * 1.1f);
        Atk = Mathf.RoundToInt(Atk * 1.1f);
        Def = Mathf.RoundToInt(Def * 1.1f);
        CurrentHP = MaxHP; // 레벨업 시 체력, 마나 회복
        CurrentMP = MaxMP;

        // UI 업데이트 요청
        UIManager.Instance.UpdateHP(MaxHP, CurrentHP);
        UIManager.Instance.UpdateMP(MaxMP, CurrentMP);
        UIManager.Instance.UpdateEXP(RequiredEXP, CurrentEXP);
    }

    public void HealHP(int amount)
    {
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        UIManager.Instance.UpdateHP(MaxHP, CurrentHP);
    }

    public void HealMP(int amount)
    {
        CurrentMP = Mathf.Min(MaxMP, CurrentMP + amount);
        UIManager.Instance.UpdateMP(MaxMP, CurrentMP);
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
        //    공격은 항상 플레이어의 발밑을 기준으로 함
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
