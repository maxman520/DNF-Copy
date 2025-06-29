using System.Collections;
using UnityEngine;

public class Goblin : Monster
{
    private enum AIPhase { Unaware, Aware } // �÷��̾� ���ν�, �ν� ����
    private AIPhase currentPhase = AIPhase.Unaware; // �⺻�� ���ν� ����
    private Coroutine aiLoopCoroutine; // AILoop �ڷ�ƾ�� �����ϱ� ���� ����

    [Header("AI �ൿ ���� ����")]
    [SerializeField] private float minActionInterval = 1.0f; // �ּ� �ൿ ���� �ð�
    [SerializeField] private float maxActionInterval = 1.5f; // �ִ� �ൿ ���� �ð�

    [Header("AI ���� ����")]
    [SerializeField] private Vector2 patrolAreaCenter; // ���� ���� �߽� (���� ��ǥ)
    [SerializeField] private Vector2 patrolAreaSize;   // ���� ���� ũ��
    private Vector3 initialPosition; // ������ �ʱ� ��ġ


    public bool IsGrounded = true; // ���� �پ��ִ°�?
    public bool IsWalking = false;
    public bool IsHurt { get; protected set; } = false; // �ǰ� ���� ����
    public bool IsAttacking { get; protected set; } = false;


    // --- ���� ���� ���� ������ ---
    private int airHitCounter = 0;
    private float visualYVelocity = 0f;
    private float virtualGravity = 0f;
    [SerializeField] private float initialVirtualGravity = 4f;
    [SerializeField] private float gravityIncreaseFactor = 1f;
    private Coroutine launchCoroutine;

    private Vector2 desiredVelocity;

    protected void Start()
    {
        if (Player.Instance != null)
        {
            playerTransform = Player.Instance.transform;
        }

        initialPosition = transform.position; // �ʱ� ��ġ ����
        patrolAreaCenter += (Vector2)initialPosition; // ���� �߽����� ���� ��ǥ�� ��ȯ


        // AI �ൿ ���� ����
        aiLoopCoroutine = StartCoroutine(AILoop());
    }
    private void Update()
    {
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("isWalking", IsWalking);
        // anim.SetBool("isGrounded", IsGrounded);
        // anim.SetBool("isHurt", player.IsMoving && !player.IsRunning);
        // anim.SetBool("isRunning", player.IsMoving && player.IsRunning);
    }
    private void FixedUpdate()
    {

        // �ǰ� �Ǵ� ���� ���� �ƴ϶��, AI�� �ǵ��� �ݿ��Ѵ�.
        if (!IsHurt && !IsAttacking)
        {
            rb.linearVelocity = desiredVelocity;
        }
        else // �ǰ� �Ǵ� ���� �߿��� ��� ���� �������� �����.
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }


    // AI�� ��ü���� �帧�� �����ϴ� ���� �ڷ�ƾ
    private IEnumerator AILoop()
    {
        while (currentHP > 0)
        {
            if (IsHurt)
            {
                yield return null; // ���� �����ӿ� �ٽ� IsHurt���� �˻�
                continue;          // �Ʒ� ������ �������� �ʰ� ������ ó������
            }

            Debug.Log($"<color=orange>--- AILoop �� ���� (���� ����: {currentPhase}) ---</color>");
            switch (currentPhase)
            {
                case AIPhase.Unaware:
                    yield return StartCoroutine(UnawarePhase());
                    break;
                case AIPhase.Aware:
                    yield return StartCoroutine(AwarePhase());
                    break;
            }
            // �ڡڡ� ���� ���� �� ����� �����̸� �༭ �α� Ȯ���� ���� �� �ڡڡ�
            yield return new WaitForSeconds(0.1f);
        }
        // ���� ������ �����ٸ� �� �������� �α׸� ����
        Debug.LogError($"AILoop �����! (currentHP: {currentHP})");
    }

    // ���ν� ������ �ൿ ����
    private IEnumerator UnawarePhase()
    {
        // �ν� ������ �÷��̾ ������ Aware ���·� ��ȯ
        if (IsPlayerInRecognitionRange())
        {
            currentPhase = AIPhase.Aware;
            yield break; // UnawarePhase �ڷ�ƾ ��� ����
        }

        // ���� �ൿ ���� (���� �Ǵ� ���)
        if (Random.value > 0.5f)
        {
            yield return StartCoroutine(UnawareWalk());
        }
        else
        {
            yield return StartCoroutine(UnawareIdle());
        }
    }

    // �ν� ������ �ൿ ����
    private IEnumerator AwarePhase()
    {
        // �ൿ ���� �� ��� ���
        float interval = Random.Range(minActionInterval, maxActionInterval);
        Debug.Log($"<color=cyan>AI ���� ��... {interval}�� �� �ൿ ����.</color>"); // 1. ���� ����
        yield return new WaitForSeconds(interval);

        // �÷��̾ ���� ���� �ȿ� �ִ°�?
        if (IsPlayerInAttackRange())
        {
            Debug.Log("<color=red>�÷��̾ ���� ���� �ȿ� ����!</color>"); // 2. ���� ���� ����
            // ���� ���� ��: 90% Ȯ���� ����, 10% Ȯ���� �ٸ� �ൿ(����/���)
            if (Random.value > 0.1f)
            {
                yield return StartCoroutine(AwareAttack());
            }
            else
            {
                Debug.Log("<color=yellow>����/��� ����.</color>");
                if (Random.value > 0.5f)
                    // ����
                    yield return StartCoroutine(AwareRetreat());
                else
                    // ���
                    yield return StartCoroutine(AwareIdle());
            }
        }
        else // �÷��̾ ���� ���� �ۿ� ���� ��
        {
            // ����, ����, ��� �� �ϳ��� �������� ����
            int randomAction = Random.Range(0, 3);
            switch (randomAction)
            {
                case 0:
                    yield return StartCoroutine(AwareWalk());
                    break;
                case 1:
                    yield return StartCoroutine(AwareRetreat());
                    break;
                case 2:
                    yield return StartCoroutine(AwareIdle());
                    break;
            }
        }
    }

    #region Action Coroutines
    // --- Unaware Actions ---
    private IEnumerator UnawareIdle()
    {
        desiredVelocity = Vector2.zero;
        IsWalking = false;
        yield return new WaitForSeconds(Random.Range(1f, 3f)); // 1~3�ʰ� ���
    }

    private IEnumerator UnawareWalk()
    {
        // ���� ���� �� ���� ������ ����
        Vector2 randomOffset = new Vector2(
            Random.Range(-patrolAreaSize.x / 2, patrolAreaSize.x / 2),
            Random.Range(-patrolAreaSize.y / 2, patrolAreaSize.y / 2)
        );
        Vector2 destination = patrolAreaCenter + randomOffset;

        IsWalking = true;
        while (Vector2.Distance(transform.position, destination) > 0.1f)
        {
            // ���� ���� �� �÷��̾ �߰��ϸ� ��� �ߴ�
            if (IsPlayerInRecognitionRange())
            {
                currentPhase = AIPhase.Aware;
                yield break;
            }

            Vector2 direction = (destination - (Vector2)transform.position).normalized;
            desiredVelocity = direction * moveSpeed;
            Flip(desiredVelocity.x);
            yield return null; // ���� �����ӱ��� �̵�
        }
        desiredVelocity = Vector2.zero;
        IsWalking = false;
    }

    // --- Aware Actions ---
    private IEnumerator AwareIdle()
    {
        desiredVelocity = Vector2.zero;
        IsWalking = false;
        FlipTowardsPlayer();
        yield return null; // �ൿ ���� �ֱ���� �� ���¸� ����
    }

    private IEnumerator AwareWalk()
    {
        Debug.Log(">> �ൿ ����: AwareWalk (�߰�)"); // 5. ���� �ൿ ���� �α�
        IsWalking = true;
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        desiredVelocity = direction * moveSpeed; // MonsterData�� moveSpeed ���
        Flip(desiredVelocity.x);
        yield return null;
    }

    private IEnumerator AwareRetreat()
    {
        IsWalking = true;
        Vector2 direction = (transform.position - playerTransform.position).normalized;
        desiredVelocity = direction * moveSpeed;
        FlipTowardsPlayer();
        yield return null;
    }

    private IEnumerator AwareAttack()
    {
        desiredVelocity = Vector2.zero;
        IsWalking = false;
        FlipTowardsPlayer();
        Attack(); // Monster Ŭ������ Attack() ȣ��
        // ���� �ִϸ��̼� �ð���ŭ ��� (AILoop�� ���ð��� ��Ÿ�� ������ ��)
        yield return null;
    }

    #endregion

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

    // ���� ���Ǹ� ���� ���� ������ �� �信 ǥ��
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // �⺻ �ν�/���� ���� ����� �׸���

        Gizmos.color = Color.green;
        Vector3 center = initialPosition + (Vector3)patrolAreaCenter - (Vector3)initialPosition; // ���� ��ǥ ����
        Gizmos.DrawWireCube(center, patrolAreaSize);
    }

    #endregion

    public override void Attack()
    {
        anim.SetTrigger("attack");
        Debug.Log("����� ����");
    }

    protected override void Hurt(AttackDetails attackDetails, Vector2 attackPosition)
    {
        // AI�� �ǵ�(������)�� ��� �����.
        desiredVelocity = Vector2.zero;
        // AI ���� ��ü�� ��� ���߰� �ʹٸ� ���⿡ ���� �߰� ����

        // ���� �˹��� desiredVelocity�� �ƴ�, �������� ������ ��� ����
        float direction = (transform.position.x > attackPosition.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * attackDetails.knockbackForce, 0);

        if (IsGrounded)
        {
            if (attackDetails.launchYVelocity > 0)
            {
                visualYVelocity = attackDetails.launchYVelocity;
                anim.SetTrigger("isAirborne");
            }
        }
        else // ���߿� ���� ��
        {
            airHitCounter++;
            visualYVelocity += attackDetails.airComboYVelocity;
            virtualGravity = initialVirtualGravity + (airHitCounter * gravityIncreaseFactor);
            anim.SetTrigger("isHurt");
        }
    }
    protected override void Die()
    {
        Debug.Log($"{monsterData.MonsterName}��(��) �׾����ϴ�.");

        // �׾��� �� AI �ڷ�ƾ�� Ȯ���� ����
        if (aiLoopCoroutine != null)
        {
            StopCoroutine(aiLoopCoroutine);
            aiLoopCoroutine = null;
        }

        // ������ �����Ӱ� �浹�� ����
        rb.linearVelocity = Vector2.zero;
        GetComponentInChildren<Collider2D>().enabled = false; // �ٸ� ������Ʈ�� �浹���� �ʵ���

        // ���� �ִϸ��̼� ���
        anim.SetTrigger("Die");

        // ����: 2�� �Ŀ� ������Ʈ �ı�
        Destroy(gameObject, 2f);
    }
}