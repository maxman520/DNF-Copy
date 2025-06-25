using System.Collections;
using UnityEngine;

public class Goblin : Monster
{
    private enum AIPhase { Unaware, Aware } // �÷��̾� ���ν�, �ν� ����
    private AIPhase currentPhase = AIPhase.Unaware; // �⺻�� ���ν� ����

    private Coroutine currentActionCoroutine;

    [Header("AI �ൿ ���� ����")]
    [SerializeField] private float minActionInterval = 1.0f; // �ּ� �ൿ ���� �ð�
    [SerializeField] private float maxActionInterval = 1.5f; // �ִ� �ൿ ���� �ð�

    [Header("AI ���� ����")]
    [SerializeField] private Vector2 patrolAreaCenter; // ���� ���� �߽� (���� ��ǥ)
    [SerializeField] private Vector2 patrolAreaSize;   // ���� ���� ũ��
    private Vector3 initialPosition; // ������ �ʱ� ��ġ

    protected override void Start()
    {
        base.Start();
        initialPosition = transform.position; // �ʱ� ��ġ ����
        patrolAreaCenter += (Vector2)initialPosition; // ���� �߽����� ���� ��ǥ�� ��ȯ


        // AI �ൿ ���� ����
        StartCoroutine(AILoop());
    }

    // AI�� ��ü���� �帧�� �����ϴ� ���� �ڷ�ƾ
    private IEnumerator AILoop()
    {
        while (currentHP > 0)
        {
            // �ڡڡ� AILoop�� �� �� ����ִ��� Ȯ���ϴ� �α� �ڡڡ�
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
            // ���� ���� ��: 70% Ȯ���� ����, 30% Ȯ���� �ٸ� �ൿ(����/���)
            if (Random.value > 0.3f)
            {
                yield return StartCoroutine(AwareAttack());
            }
            else
            {
                Debug.Log("<color=yellow>�÷��̾ ���� ���� �ۿ� ����. �߰�/����/��� ����.</color>"); // 3. ���� ���� ��
                // �ٸ� �ൿ ���� (���� �Ǵ� ���)
                if (Random.value > 0.5f)
                    yield return StartCoroutine(AwareRetreat());
                else
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
        rb.linearVelocity = Vector2.zero;
        anim.SetBool(animHashes.IsWalking, false);
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

        anim.SetBool(animHashes.IsWalking, true);
        while (Vector2.Distance(transform.position, destination) > 0.1f)
        {
            // ���� ���� �� �÷��̾ �߰��ϸ� ��� �ߴ�
            if (IsPlayerInRecognitionRange())
            {
                currentPhase = AIPhase.Aware;
                yield break;
            }

            Vector2 direction = (destination - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
            Flip(rb.linearVelocity.x);
            yield return null; // ���� �����ӱ��� �̵�
        }
        rb.linearVelocity = Vector2.zero;
        anim.SetBool(animHashes.IsWalking, false);
    }

    // --- Aware Actions ---
    private IEnumerator AwareIdle()
    {
        rb.linearVelocity = Vector2.zero;
        anim.SetBool(animHashes.IsWalking, false);
        FlipTowardsPlayer();
        yield return null; // �ൿ ���� �ֱ���� �� ���¸� ����
    }

    private IEnumerator AwareWalk()
    {
        Debug.Log(">> �ൿ ����: AwareWalk (�߰�)"); // 5. ���� �ൿ ���� �α�
        anim.SetBool(animHashes.IsWalking, true);
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed; // MonsterData�� moveSpeed ���
        Flip(rb.linearVelocity.x);
        yield return null;
    }

    private IEnumerator AwareRetreat()
    {
        anim.SetBool(animHashes.IsWalking, true);
        Vector2 direction = (transform.position - playerTransform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
        FlipTowardsPlayer();
        yield return null;
    }

    private IEnumerator AwareAttack()
    {
        rb.linearVelocity = Vector2.zero;
        anim.SetBool(animHashes.IsWalking, false);
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

    public override void Attack()
    {
        anim.SetTrigger(animHashes.Attack);
        Debug.Log("����� ����");
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
}