using UnityEngine;
using Cysharp.Threading.Tasks; // UniTask
using System.Threading;       // CancellationToken

public class Goblin : Monster
{
    [Header("�̵� ����")]
    public bool IsGrounded = true;
    public bool IsWalking = false;

    [Header("���� ����")]
    private const float ORIGINAL_GRAVITY = 10f;
    public float verticalVelocity; // ���� '��'�� ����� ��Ÿ���� ���� �ӵ�
    private float gravity = ORIGINAL_GRAVITY; // ���� �߷°�
    private int airHitCounter = 0;

    [Header("AI ����")]
    protected bool isActing = false; // ���� � �ൿ(Idle, Move ��)�� �ϰ� �ִ��� ����
    protected bool isAware = false; // �÷��̾ �ν��ߴ°�

    [Header("AI Configuration")]
    [SerializeField] private Vector2 patrolAreaCenter; // ���� ���� �߽� (���� ��ǥ)
    [SerializeField] private Vector2 patrolAreaSize;   // ���� ���� ũ��
    private Vector3 initialPosition; // ������ �ʱ� ��ġ


    private CancellationTokenSource aiLoopCts; // �񵿱� �۾� ����. �ܺο����� CancellationToken�� ���
    private CancellationTokenSource moveCts; // �̵� �۾� ���� ��ū - �̵� �ߴ��� ����

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake(); // �θ��� Awake�� ���� ȣ��
    }

    protected void Start()
    {
        if (Player.Instance != null)
        {
            playerTransform = Player.Instance.transform;
        }
        initialPosition = transform.position; // �ʱ� ��ġ ����
        patrolAreaCenter += (Vector2)initialPosition; // ���� �߽����� ���� ��ǥ�� ��ȯ

        // AI ���� ����
        StartAILoop();
    }
    private void Update()
    {
        HandleGravity();

        // �ִϸ��̼� ������Ʈ
        anim.SetBool("isGrounded", IsGrounded);
        anim.SetBool("isWalking", IsWalking);


        // ��� ó��
        if (currentHP <= 0 && IsGrounded)
        {
            Die();
        }
    }
    void OnGUI()
    {
        GUI.Label(new Rect(10, 80, 200, 20), "goblin IsGrounded: " + IsGrounded);
        GUI.Label(new Rect(10, 90, 200, 20), "goblin IsWalking: " + IsWalking);
        GUI.Label(new Rect(10, 100, 200, 20), "goblin isActing: " + isActing);
        GUI.Label(new Rect(10, 110, 200, 20), "goblin isAware: " + isAware);
    }
    #endregion Unity Lifecycle


    #region AI System
    private void StartAILoop()
    {
        // ���� CancellationTokenSource�� �ִٸ� return
        if (aiLoopCts != null) return;

        // ������Ʈ �ı� �� ��ҵǴ� ��ū�� ����� ���ο� CancellationTokenSource ����
        var destroyToken = this.GetCancellationTokenOnDestroy();
        aiLoopCts = CancellationTokenSource.CreateLinkedTokenSource(destroyToken);

        // AI ���� ����
        AI_Loop(aiLoopCts.Token).Forget();
    }
    private void StopAILoop()
    {
        aiLoopCts?.Cancel();
        aiLoopCts?.Dispose();
        aiLoopCts = null;
    }
    private void StopMovement()
    {
        // �̵� �۾� �ߴ�
        moveCts?.Cancel();
        moveCts?.Dispose();
        moveCts = null;

        // ������ �̵� ��� �ߴ�
        rb.linearVelocity = Vector2.zero;
        IsWalking = false;
    }

    private async UniTask AI_Loop(CancellationToken token)
    {
        while (token.IsCancellationRequested == false)
        {
            // �÷��̾� �ν� ���¿� ���� �ٸ� �ൿ ���� ����
            if (isAware)
                await Pattern_Aware(token);
            else
                await Pattern_UnAware(token);

            // �� ������ ������� �ʰ�, �ణ�� �����̸� �־� ���� ���ϸ� ����
            await UniTask.Delay(100, cancellationToken: token);
        }
    }

    // --- ���� ��: ���� ���� ---
    private async UniTask Pattern_UnAware(CancellationToken token)
    {
        if (isActing) return; // �̹� �ٸ� �ൿ ���̸� �������� ����


        // �÷��̾� ����
        if (IsPlayerInRecognitionRange())
        {
            isAware = true; // ���� ���·� ��ȯ
            Debug.Log("�÷��̾� ����! ���� �¼��� ��ȯ");
            return;
        }

        isActing = true;
        IsWalking = false;

        // 50% Ȯ���� ���. �ƴϸ� �̵�
        if (Random.value < 0.5f)
        {
            // ���� �ð�(1~2��) ���� ���
            float idleTime = Random.Range(1f, 2f);
            Debug.Log($"����: {idleTime:F1}�� ���� ���");
            await UniTask.Delay(System.TimeSpan.FromSeconds(idleTime), cancellationToken: token);
        }
        else
        {
            // ���� ���� �� ���� ������ ����
            Vector2 randomOffset = new Vector2(
                Random.Range(-patrolAreaSize.x / 2, patrolAreaSize.x / 2),
                Random.Range(-patrolAreaSize.y / 2, patrolAreaSize.y / 2)
            );
            Vector2 destination = patrolAreaCenter + randomOffset;

            // ���� ���� ���� ������ ��ǥ �������� �̵�
            Debug.Log($"����: {destination.x}, {destination.y} ��ġ�� �̵�");

            await MoveTo(destination, token);
        }

        isActing = false;
    }


    // --- ���� ��: ���� ���� ---
    private async UniTask Pattern_Aware(CancellationToken token)
    {
        if (isActing) return; // �̹� �ٸ� �ൿ���̸� �������� ����

        // �÷��̾ ���� ���� �ȿ� ������ 70% Ȯ���� ����
        if (IsPlayerInAttackRange()&& Random.value > 0.3f)
        {
            isActing = true;
            IsWalking = false;
            rb.linearVelocity = Vector2.zero;
            FlipTowardsPlayer();

            Attack(); // ���� ����

            // ���� �� ������
            await UniTask.Delay(3000, cancellationToken: token); // ���� ��Ÿ��
        }
        else // ���� ���� �ۿ� ������ ��� �ൿ
        {
            isActing = true;
            IsWalking = false;

            // 3���� �ൿ �� �ϳ��� �����ϰ� ����
            Vector3 destination;
            int action = Random.Range(0, 3);
            switch (action)
            {
                case 0: // ��� ���
                    float idleTime = Random.Range(1f, 2f);
                    Debug.Log($"���: {idleTime:F1}�� ���� ���");
                    await UniTask.Delay(System.TimeSpan.FromSeconds(idleTime), cancellationToken: token);
                    break;
                case 1: // �÷��̾�� ����
                    destination = transform.position + (playerTransform.position - transform.position).normalized * Random.Range(1f, 3f);
                    Debug.Log($"���: �÷��̾�� ����");
                    await MoveTo(destination, token);
                    break;
                case 2: // �÷��̾�Լ� ����
                    destination = transform.position - (playerTransform.position - transform.position).normalized * Random.Range(1f, 3f);
                    Debug.Log($"���: �÷��̾�Լ� ����");
                    await MoveTo(destination, token);
                    break;
            }
        }
        isActing = false;
    }

    // ��ǥ �������� �̵��ϴ� UniTask �Լ�
    private async UniTask MoveTo(Vector3 destination, CancellationToken parentToken)
    {
        // ���� �̵� �۾� �ߴ�
        StopMovement();

        // ���ο� �̵� ��ū ���� (�θ� ��ū�� ����)
        moveCts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
        var moveToken = moveCts.Token;

        IsWalking = true;

        while (Vector2.Distance(transform.position, destination) > 0.1f
            && !moveToken.IsCancellationRequested)
        {
            Vector2 direction = (destination - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;

            if (isAware) FlipTowardsPlayer();
            else Flip(direction.x);

            await UniTask.Yield(PlayerLoopTiming.Update, moveToken);  // ���� �����ӱ��� �̵�
        }

        rb.linearVelocity = Vector2.zero;
        IsWalking = false;
    }
    #endregion AI System

    #region State Behaviour
    // ��� �ִϸ��̼����� ���� �� ȣ��
    public override void OnIdleStateEnter()
    {
        // AI ���� ����� �Լ� ȣ��
        StartAILoop();
    }

    public override void OnWalkStateExit()
    {
        StopMovement();
    }

    // ���� �ִϸ��̼��� ������ �� ȣ��
    public override void OnAttackStateExit()
    {
        isActing = false;
    }

    // �ǰ� �ִϸ��̼��� ������ �� ȣ��
    public override void OnHurtStateExit() {
        // �ǰ��� ������ ���� ���·� �����ϰ� AI ���� �����
        isAware = true; // �ǰݴ������� �÷��̾�� ������ ����
        isActing = false;
    }

    // ��� �ִϸ��̼��� ������ �� ȣ��
    public override void OnGetUpStateExit()
    {
        // �ǰ��� ������ ���� ���·� �����ϰ� AI ���� �����
        isAware = true;
        isActing = false;
    }
    #endregion State Behaviour

    #region Utilities
    private bool IsPlayerInRecognitionRange()
    {
        return playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= recognitionRange;
    }

    private bool IsPlayerInAttackRange()
    {
        return playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= attackRange;
    }

    private void Flip(float directionX)
    {
        if (Mathf.Abs(directionX) > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Sign(directionX), 1, 1);
        }
    }

    private void FlipTowardsPlayer()
    {
        if (playerTransform == null) return;
        float directionToPlayer = playerTransform.position.x - transform.position.x;
        Flip(directionToPlayer);
    }

    #endregion Utilities

    protected override void Hurt(AttackDetails attackDetails, Vector2 attackPosition)
    {
        StopAILoop(); // ��� �񵿱� �۾� ��� �ߴ�
        isActing = false;
        IsWalking = false;
       
        rb.linearVelocity = Vector2.zero; // �˹� ���� �ӵ� �ʱ�ȭ

        float direction = (transform.position.x > attackPosition.x) ? 1 : -1;

        if (IsGrounded) // ���� ���� ��
        {
            
            if (attackDetails.launchForce > 0)
            {
                // ���� �˹�
                rb.AddForce(new Vector2(direction * attackDetails.knockbackForce, 0), ForceMode2D.Impulse);

                // ���߿� �ߴ� �� ����
                verticalVelocity = attackDetails.launchForce;

                IsGrounded = false;
                anim.SetTrigger("airborne");
            }
            else
            {   
                // ���� �˹�
                transform.position += new Vector3(direction * attackDetails.knockbackForce * 0.1f, 0);

                anim.SetTrigger("hurt");
            }
        }
        else // ���߿� ���� ��
        {   
            // ���� �˹�
            rb.AddForce(new Vector2(direction * attackDetails.knockbackForce, 0), ForceMode2D.Impulse);

            if (attackDetails.launchForce > 0) airHitCounter++;
            verticalVelocity = 2f + (attackDetails.launchForce * Mathf.Max(0, 0.5f - (airHitCounter * 0.125f)));
            gravity += 0.05f;
        }
    }

    protected override void Die()
    {
        StopAILoop(); // ��� �񵿱� �۾� �ߴ�
        isActing = false;

        // ... ���� Die ���� ...
        Debug.Log($"{monsterData.MonsterName}��(��) �׾����ϴ�.");

        // ������ �����Ӱ� �浹�� ����
        rb.linearVelocity = Vector2.zero;
        GetComponentInChildren<Collider2D>().enabled = false; // �ٸ� ������Ʈ�� �浹���� �ʵ���

        // ���� �ִϸ��̼� ���
        anim.SetTrigger("die");

        // ����: 2�� �Ŀ� ������Ʈ �ı�
        Destroy(gameObject, 2f);
    }

    protected override void Attack()
    {
        anim.SetTrigger("attack");
        Debug.Log("����� ����!");
    }
    public void HandleGravity()
    {
        // 1. ���߿� �� �ִٸ�
        if (!IsGrounded)
        {
            // 2. �߷��� ��� ����
            verticalVelocity += (-gravity) * Time.deltaTime;

            // 3. ���� �ӵ��� Visuals�� local Y��ǥ�� ����
            visualsTransform.localPosition += new Vector3(0, verticalVelocity * Time.deltaTime, 0);

            // 4. �����ߴ��� Ȯ��
            CheckForLanding();
        }
    }

    // ���� �Ǻ� ����
    private void CheckForLanding()
    {
        // Visuals�� Y ��ǥ�� ���� Y��ǥ���� �Ʒ��� �������ٸ� ������ ����
        if (visualsTransform.localPosition.y <= startPos.y)
        {
            if (verticalVelocity < -3f)
            {
                verticalVelocity *= -0.5f;
                return;
            }
            // ���� �ʱ�ȭ
            IsGrounded = true;
            airHitCounter = 0;


            // ��ġ�� �ӵ�, �߷� �ʱ�ȭ
            rb.linearVelocity = Vector2.zero;
            visualsTransform.localPosition = startPos;
            verticalVelocity = 0f;
            gravity = ORIGINAL_GRAVITY;
        }
    }
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // �⺻ �ν�/���� ���� ����� �׸���

        Gizmos.color = Color.green;
        Vector3 center = initialPosition + (Vector3)patrolAreaCenter - (Vector3)initialPosition; // ���� ��ǥ ����
        Gizmos.DrawWireCube(center, patrolAreaSize);
    }
}