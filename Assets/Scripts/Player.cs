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
    [Header("�÷��̾� ����")]
    public float Atk;
    public float Def;
    public float MaxHP;
    public float MaxMP;
    public float CurrentHP;
    public float CurrentMP;
    public float WalkSpeed;
    public float RunSpeed;


    // ���� ����
    public bool IsGrounded = true;
    public bool IsRunning = false;
    public bool IsMoving { get; set; } = false;

    public bool CanMove { get; set; } = true;
    public bool CanAttack { get; set; } = true;
    public bool CanContinueAttack { get; set; } = false;
    public int AttackCounter = 0;

    [Header("���� ����")]
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

    // ������Ʈ ����
    public Rigidbody2D Rb { get; private set; }
    public Animator Anim { get; private set; }
    public Collider2D PlayerGround { get; private set; }
    public Transform VisualsTransform { get; private set; }

    protected override void Awake()
    {
        // �̱��� ����
        base.Awake();


        CurrentAnimState = PlayerAnimState.Idle;

        // ������Ʈ ���� ���� �ʱ�ȭ
        Rb = GetComponent<Rigidbody2D>();
        Anim = GetComponentInChildren<Animator>();
        PlayerGround = transform.Find("PlayerGround").GetComponent<BoxCollider2D>();
        VisualsTransform = transform.Find("Visuals");

        // ��Ʈ�ѷ� �ʱ�ȭ
        inputHandler = new InputHandler();
        animController = new AnimController(this);
        behaviourController = new BehaviourController(this, inputHandler, animController);

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

    private void Update()
    {
        inputHandler.ReadInput(); // �Է��� �а�
        behaviourController.Flip(); // �Է��� �������� ���� ��ȯ ó��
        behaviourController.HandleGravity();
        animController.UpdateAnimations(); // �ִϸ��̼� ó��
    }

    private void FixedUpdate()
    {
        behaviourController.ApplyMovement(); // �÷��̾� �̵� ó��
    }
    public bool HasState(PlayerAnimState state)
    {
        return (CurrentAnimState.HasFlag(state));
    }

    public void OnDamaged(float monsterAtk)
    {

        float damage = (monsterAtk - (this.Def * 0.5f));
        damage = Mathf.RoundToInt(damage * UnityEngine.Random.Range(0.8f, 1.2f));

        Debug.Log(damage + " ��ŭ�� ���ظ� ����");
        Anim.SetTrigger("hurt");

        // ���⿡ �߰��� ü�� ����, �˹� ���� ����
        // ex) health -= damage;
        // ex) behaviourController.ApplyKnockback(...);
    }

    // ���� ���� �� GameManager�� ���� ȣ��
    public void OnEnterDungeon()
    {
        behaviourController.SubscribeToEvents();
        Anim.Play("Idle_Battle");
    }
    // ���� ���� �� GameManager�� ���� ȣ��
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
