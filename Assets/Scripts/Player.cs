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

    // ���� ����
    public enum PlayerState
    {
        Town,
        Dungeon
    }
    public PlayerState CurrentState { get; private set; } = PlayerState.Town;

    // ������Ʈ ����
    public Rigidbody2D Rb { get; private set; }
    public Animator Anim { get; private set; }
    public Collider2D PlayerGround { get; private set; }
    public Transform VisualsTransform { get; private set; }

    // ���� ����
    public bool IsGrounded { get; set; } = true;
    public bool IsRunning { get; set; } = false;
    public bool IsJumping { get; set; } = false;
    public bool IsAttacking { get; set; } = false;
    public bool IsJumpAttacking { get; set; } = false;
    public bool IsHurt { get; set; } = false;

    protected override void Awake()
    {
        // �̱��� ����
        base.Awake();

        // ��Ʈ�ѷ� �ʱ�ȭ
        inputHandler = new InputHandler();
        behaviourController = new BehaviourController(this, inputHandler);
        animController = new AnimController(this, inputHandler);

        // ������Ʈ ���� ���� �ʱ�ȭ
        Rb = GetComponent<Rigidbody2D>();
        Anim = GetComponentInChildren<Animator>();
        PlayerGround = transform.Find("PlayerGround").GetComponent<BoxCollider2D>();
        VisualsTransform = transform.Find("Visuals");

        if (PlayerGround == null)
            Debug.LogError("PlayerGround�� ã�� �� �����ϴ�.", this);
        if (VisualsTransform == null)
            Debug.LogError("Visuals Transform�� ã�� �� �����ϴ�.", this);

        // PlayerStat �ε�
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
        inputHandler.ReadInput(); // 1. ���� �Է��� �а�
        behaviourController.Flip(); // 3. �Է��� �������� ���� ����(����, ���� ��ȯ) ó��
        animController.UpdateAnimations(); // 4. �ִϸ��̼� ó��
    }

    private void FixedUpdate()
    {
        behaviourController.ApplyMovement(); // ���� ���
    }
    public void EnterNewState(PlayerState currentState)
    {
        switch (CurrentState)
        {
            case PlayerState.Town:
                Debug.Log("���� ���¿� ����");
                Anim.Play("Idle_Town");
                break;
            case PlayerState.Dungeon:
                Debug.Log("���� ���¿� ����");
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
                Debug.Log("���� ���¸� ����ϴ�.");
                // ���� ���·� ���� ��, �ȱ� �ִϸ��̼� ���¸� �ʱ�ȭ
                Anim.SetBool("isWalking", false);
                break;
            case PlayerState.Dungeon:
                Debug.Log("���� ���¸� ���");
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

        Debug.Log(damage + " ��ŭ�� ���ظ� ����");
        if (IsGrounded) Anim.SetBool("isGrounded", true);
        IsHurt = true;
        Anim.SetTrigger("isHurt");

        // ���⿡ �߰��� ü�� ����, �˹� ���� ����
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
