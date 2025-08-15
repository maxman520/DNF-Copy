using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
    public event Action<float, float> OnHPChanged;
    public event Action<float, float> OnMPChanged;
    public event Action<float, float> OnEXPChanged;

    [Header("플레이어 스탯")]
    // 최종 스탯 (기본 + 장비)
    public float Atk;
    public float Def;
    
    // 기본 스탯 (레벨업, 캐릭터 고유값)
    public float baseAtk { get; private set; }
    public float baseDef { get; private set; }

    public float MaxHP;
    public float MaxMP;

    private float _currentHP;
    public float CurrentHP
    {
        get => _currentHP;
        set
        {
            _currentHP = Mathf.Clamp(value, 0, MaxHP);
            OnHPChanged?.Invoke(MaxHP, _currentHP);
        }
    }

    private float _currentMP;
    public float CurrentMP
    {
        get => _currentMP;
        set
        {
            _currentMP = Mathf.Clamp(value, 0, MaxMP);
            OnMPChanged?.Invoke(MaxMP, _currentMP);
        }
    }

    public float WalkSpeed;
    public float RunSpeed;

    public int CurrentEXP; // 레벨업 로직 때문에 EXP는 AddExp에서 별도 처리
    public int Level = 1;
    public int RequiredEXP = 1000; // 1레벨에서 2레벨로 가는 데 필요한 경험치


    // 상태 변수
    public bool IsGrounded = true;
    public bool IsRunning = false;
    public bool IsMoving = false;
    public bool IsDead = false;

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
    public Inventory PlayerInventory { get; private set; }

    [Header("SFX 설정")]
    [SerializeField] private float runSfxInterval = 0.28f; // 달리기 발소리 간격(초)

    protected override void Awake()
    {
        // 싱글턴 패턴
        base.Awake();

        CurrentAnimState = PlayerAnimState.Idle;

        // 컴포넌트 참조 변수 초기화
        Rb = GetComponent<Rigidbody2D>();
        Anim = GetComponentInChildren<Animator>();
        PlayerInventory = GetComponent<Inventory>();
        PlayerInventory.SetPlayer(this);
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
    private void Start()
    {
        InitializeStats(DataManager.Instance.SelectedCharacter);
        // 달리기 발소리 루프 시작
        RunRunSfxLoop(this.GetCancellationTokenOnDestroy()).Forget();
    }
    public void InitializeStats(CharacterData data)
    {
        Debug.Log("플레이어 데이터 초기화 시작");
        // 기본 스탯 설정
        this.baseAtk = data.baseAtk;
        this.baseDef = data.baseDef;

        // 최종 스탯 초기화 (장비 미적용 상태)
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
        OnEXPChanged?.Invoke(RequiredEXP, CurrentEXP);

        // 재화 로드
        PlayerInventory.Gold = data.Gold;
        PlayerInventory.Coin = data.Coin;

        // 인벤토리 및 장비 로드
        if (PlayerInventory != null && DataManager.Instance != null)
        {
            // 1. 저장된 아이템으로 인벤토리 채우기
            for (int i = 0; i < data.inventoryItems.Count; i++)
            {
                if (i < PlayerInventory.Items.Length)
                {
                    PlayerInventory.Items[i] = data.inventoryItems[i];
                }
            }

            // 2. 퀵슬롯 아이템 로드
            for (int i = 0; i < data.quickSlotItemIDs.Count; i++)
            {
                if (i < PlayerInventory.QuickSlotItemIDs.Length)
                {
                    PlayerInventory.QuickSlotItemIDs[i] = data.quickSlotItemIDs[i];
                }
            }

            // 3. 저장된 장비 아이템 장착
            foreach (string itemID in data.equippedItemIDs)
            {
                EquipmentData equipment = DataManager.Instance.GetItemByID(itemID) as EquipmentData;
                if (equipment != null)
                {
                    PlayerInventory.Equip(equipment); // 인덱스가 아닌 EquipmentData로 장착
                }
            }
        }
        
        Debug.Log("플레이어 데이터 초기화 완료");
       

    }

    private void Update()
    {
        HandleCommands(); // 입력된 커맨드 처리
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

        // 플레이어 피격 SFX 재생 (Sm_Dmg_01 ~ 03 중 랜덤)
        {
            int idx = UnityEngine.Random.Range(1, 4);
            string sfxKey = $"Sm_Dmg_{idx:00}";
            AudioManager.Instance.PlaySFX(sfxKey);
        }

        // 피격 반응
        behaviourController.HandleHurt(attackDetails, attackPosition);

        // 이미 죽었다면 데미지 적용X. return
        if (IsDead) return;

        // 데미지 적용
        CurrentHP -= damage;
        Debug.Log($"플레이어가 {damage}의 데미지를 입었습니다. 현재 체력: {CurrentHP}");

        if (CurrentHP <= 0 && !IsDead)
        {
            IsDead = true;
            DieSequence().Forget();
        }
    }

    private async UniTask DieSequence()
    {
        Debug.Log("플레이어 사망 시퀀스 시작");
        CanMove = false;
        CanAttack = false;

        // 플레이어 사망 SFX 재생
        AudioManager.Instance.PlaySFX("Sm_Die");

        // 만약 땅에 붙어있다면, GetDown 애니메이션이 보이도록 살짝 띄움
        if (IsGrounded)
        {
            // 공중에 뜨는 힘 적용
            behaviourController.ApplyVerticalForce(4f);
            animController.PlayAirborne();
        }

        // 땅에 착지할 때까지 대기
        await UniTask.WaitUntil(() => IsGrounded, cancellationToken: this.GetCancellationTokenOnDestroy());

        // 유령 상태 UI 표시
        UIManager.Instance.ShowGhostStatePanel();

        // 3초 동안 0.5초 간격으로 깜빡임
        await BlinkPlayerVisuals(3f, 0.5f, this.GetCancellationTokenOnDestroy());

        // 카운트다운 시작
        UIManager.Instance.ShowCountdown();
    }

    private async UniTask BlinkPlayerVisuals(float duration, float interval, CancellationToken token)
    {
        await UniTask.Delay(500);
        var renderers = VisualsTransform.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning("깜빡일 Renderer가 VisualsTransform 아래에 없습니다.");
            return;
        }

        float endTime = Time.time + duration;
        while (Time.time < endTime)
        {
            foreach (var renderer in renderers)
            {
                renderer.enabled = !renderer.enabled;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token);
        }

        // 시퀀스가 끝나면 모든 렌더러를 반드시 보이도록 설정
        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }
    }

    private float CalculateDamage(AttackDetails attackDetails)
    {
        // !! 데미지 배율에 몬스터의 공격력이 이미 곱해져있음 !!
        float finalDamage = (attackDetails.damageRate) - (Def * 0.5f);
        finalDamage = Mathf.Max(1, Mathf.RoundToInt(finalDamage * UnityEngine.Random.Range(0.8f, 1.2f)));

        return finalDamage;
    }

    public void Revive()
    {
        Debug.Log("플레이어 부활!");

        IsDead = false;
        OnEnterDungeon(); // 던전 입장 시의 상태로 초기화

        // 부활 이펙트 재생
        EffectManager.Instance.PlayEffect("Revive", transform.position, Quaternion.identity, transform);
    }


    // 던전 입장 시 GameManager에 의해 호출
    public void OnEnterDungeon()
    {
        Anim.Play("Idle_Battle");
        animController.ResetAnimations();
        
        // 던전으로 이동시 체력, 마나 회복 (던전에서 바로 다음 던전으로 이동 시 대비)
        CurrentHP = MaxHP;
        CurrentMP = MaxMP;

        // 상태 초기화
        CanMove = true;
        CanAttack = true;
    }
    // 던전 퇴장, 마을 입장 시 GameManager에 의해 호출
    public void OnEnterTown()
    {
        Anim.Play("Idle");
        animController.ResetAnimations();

        // 마을로 이동시 체력, 마나 회복
        CurrentHP = MaxHP;
        CurrentMP = MaxMP;

        // 상태 초기화
        CanMove = true;
        CanAttack = false;
    }

    // Inventory 에서 호출하여 장비로 인한 스탯 변동을 최종 스탯에 반영
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
        OnEXPChanged?.Invoke(RequiredEXP, CurrentEXP); // UI 업데이트 요청

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
        EffectManager.Instance.PlayEffect("LevelUp", transform.position, Quaternion.identity, transform);

        // 레벨업에 따른 스탯 증가 로직 (MaxHP, Atk 등)
        MaxHP = Mathf.RoundToInt(MaxHP * 1.1f);
        MaxMP = Mathf.RoundToInt(MaxMP * 1.1f);
        baseAtk = Mathf.RoundToInt(baseAtk * 1.1f);
        baseDef = Mathf.RoundToInt(baseDef * 1.1f);
        // 체력, 마나 회복 및 UI 업데이트
        CurrentHP = MaxHP; 
        CurrentMP = MaxMP;
        OnEXPChanged?.Invoke(RequiredEXP, CurrentEXP);
        
        // 장비로 인한 추가 스탯을 다시 계산하여 최종 스탯에 반영
        int totalAttackFromItems = PlayerInventory.GetCurrentTotalAttack();
        int totalDefenseFromItems = PlayerInventory.GetCurrentTotalDefense();
        UpdateEquipmentStats(totalAttackFromItems, totalDefenseFromItems);
        
    }

    public void HealHP(int amount)
    {
        int healAmount = Mathf.RoundToInt(Mathf.Min(MaxHP, CurrentHP + amount) - CurrentHP);
        CurrentHP += amount;
        EffectManager.Instance.PlayEffect("Heal", HurtboxTransform.transform.position, Quaternion.identity);
        EffectManager.Instance.PlayEffect("HealDamageText",HurtboxTransform.position, Quaternion.identity, healAmount);
    }

    public void HealMP(int amount)
    {
        CurrentMP += amount;
    }


    private void OnDisable()
    {
        inputHandler?.Dispose();

    }
    private void OnDestroy()
    {
        inputHandler?.Dispose();
    }

    // 달리기 발소리 루프: 달리는 중/이동 중/지면에서만 주기적으로 재생
    private async UniTask RunRunSfxLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.WaitUntil(() => IsRunning && IsMoving && IsGrounded && CanMove && !HasState(PlayerAnimState.Attack), cancellationToken: token);
            AudioManager.Instance.PlaySFX("Pub_Run_01");
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0.05f, runSfxInterval)), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
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
