using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Player : Singleton<Player>
{
    public float WalkSpeed;
    public float RunSpeed;

    private InputHandler inputHandler;
    private MoveController moveController;
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

    protected override void Awake()
    {
        // �̱��� ����
        base.Awake();

        // ��Ʈ�ѷ� �ʱ�ȭ
        inputHandler = new InputHandler();
        moveController = new MoveController(this, inputHandler);
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
    }

    private void Start()
    {
        // PlayerStats �ε�
        if (PlayerStats.Instance != null)
        {
            this.WalkSpeed = PlayerStats.Instance.MoveSpeed * 1.0f;
            this.RunSpeed = PlayerStats.Instance.MoveSpeed * 2.0f;
        }
    }

    private void Update()
    {
        inputHandler.ReadInput(); // 1. ���� �Է��� �а�
        moveController.Tick(); // 2. �Է��� �������� ���� ����(����, ���� ��ȯ) ó��
        animController.UpdateAnimations(); // 3. �ִϸ��̼� ó��           
    }

    private void FixedUpdate()
    {
        moveController.ApplyMovement(); // ���� ���
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
                moveController.SubscribeToEvents();
                break;
        }
        IsRunning = false;
        // moveController.SubscribeToEvents();
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
                moveController.ForceStopJump();
                break;
        }
        moveController.UnsubscribeFromEvents();
        animController.ResetAnimations();
    }

    public void SetState(PlayerState newState)
    {
        if (CurrentState == newState) return;

        ExitCurrentState(CurrentState);
        CurrentState = newState;
        EnterNewState(CurrentState);
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
}