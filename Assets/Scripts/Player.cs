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
    [Header("��Ʈ�ڽ� ����")]
    [SerializeField] private PlayerHitbox comboHitbox; // �Ϲ� �޺� ���ݿ� ����� ��Ʈ�ڽ�


    private InputHandler inputHandler;
    private BehaviourController behaviourController;
    private AnimController animController;
    private SkillManager skillManager;

    public PlayerAnimState CurrentAnimState { get; set; }

    // ������Ʈ ����
    public Rigidbody2D Rb { get; private set; }
    public Animator Anim { get; private set; }
    public Collider2D PlayerGround { get; private set; }
    public Transform VisualsTransform { get; private set; }
    public Transform HurtboxTransform { get; private set; }

    protected override void Awake()
    {
        // �̱��� ����
        base.Awake();

        CurrentAnimState = PlayerAnimState.Idle;

        // ������Ʈ ���� ���� �ʱ�ȭ
        Rb = GetComponent<Rigidbody2D>();
        Anim = GetComponentInChildren<Animator>();
        PlayerGround = transform.Find("Player_Ground").GetComponent<BoxCollider2D>();
        VisualsTransform = transform.Find("Visuals");
        HurtboxTransform = VisualsTransform.Find("Hurtbox");

        // ��Ʈ�ѷ� �ʱ�ȭ
        inputHandler = new InputHandler();
        animController = new AnimController(this);
        skillManager = GetComponent<SkillManager>();
        behaviourController = new BehaviourController(this, inputHandler, animController, skillManager);

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
        HandleCommands();
        behaviourController.Flip(); // ���� ��ȯ ó��
        behaviourController.HandleGravity(); // �÷��̾� visuals�� localPosition.y�� ���� �߷� ó��
        animController.UpdateAnimations(); // �ִϸ��̼� ó��
    }

    private void FixedUpdate()
    {
        behaviourController.ApplyMovement(); // �÷��̾� �̵� ó��
    }
    private void HandleCommands()
    {
        // ���ۿ��� Ŀ�ǵ带 �ϳ� ������
        ICommand command = inputHandler.PeekCommand();

        if (command != null)
        {
            // Ŀ�ǵ带 ����, �����ϸ� Ŀ�ǵ带 ���ۿ��� ����
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
        // ���� ������ ���
        float damage = CalculateDamage(attackDetails);

        // ������ �ؽ�Ʈ ���
        EffectManager.Instance.PlayEffect("HurtDamageText",HurtboxTransform.position, Quaternion.identity, damage);

        // �ǰ� ����
        behaviourController.HandleHurt(attackDetails, attackPosition);

        // �̹� �׾��ٸ� ������ ����X. return

        // ������ ����
        // ex) health -= damage;
        // Debug.Log(damage + " ��ŭ�� ���ظ� ����");
        // ex) behaviourController.ApplyKnockback(...); // �˹� �� �ǰ� ���� ó��
        // BehaviourController�� �����ϴ� ���� ����
    }
    private float CalculateDamage(AttackDetails attackDetails)
    {
        // !! ������ ������ ������ ���ݷ��� �̹� ���������� !!
        float finalDamage = (attackDetails.damageRate) - (Def * 0.5f);
        finalDamage = Mathf.Max(1, Mathf.RoundToInt(finalDamage * UnityEngine.Random.Range(0.8f, 1.2f)));

        return finalDamage;
    }


    // ���� ���� �� GameManager�� ���� ȣ��
    public void OnEnterDungeon()
    {
        Anim.Play("Idle_Battle");
    }
    // ���� ���� �� GameManager�� ���� ȣ��
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
        // 1. ���� ���� ��������
        AttackDetails details = AttackDetails[index];

        // 2. Y�� ���� ������ ����
        //    �Ϲ� ������ �׻� �÷��̾��� �߹��� �������� ��
        float originY = this.transform.position.y;

        // 3. ��Ʈ�ڽ� �ʱ�ȭ
        if (comboHitbox != null)
        {
            comboHitbox.Initialize(details, originY);
        }
    }
    #endregion Animation Event Receivers
#if UNITY_EDITOR // �����Ϳ����� ����ǵ��� ��ó���� ��� (���� �� ����)
    // ����� UI�� ���� ���� �����ϴ� ����
    [Header("����� �ɼ�")]
    [SerializeField] private bool showInputBufferUI = true;

    private void OnGUI()
    {
        if (!showInputBufferUI) return;

        

        // UI ��Ÿ�� ����
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;

        // ȭ�� �»�ܿ� UI ������ ����
        GUILayout.BeginArea(new Rect(10, 10, 300, 500));

        GUILayout.Label("--- Input Buffer ---", style);

        if (inputHandler != null)
        {
            // InputHandler�κ��� ���ۿ� �ִ� Ŀ�ǵ� �̸� ����� ������
            List<string> bufferedCommands = inputHandler.GetBufferedCommandNames();

            if (bufferedCommands.Count == 0)
            {
                GUILayout.Label("(Empty)", style);
            }
            else
            {
                // ���ۿ� �ִ� �� Ŀ�ǵ带 ȭ�鿡 ǥ��
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
