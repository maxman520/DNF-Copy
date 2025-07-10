using UnityEngine;
using Random = UnityEngine.Random;
using Cysharp.Threading.Tasks; // UniTask
using System.Threading;       // CancellationToken
using System;

public class Vinoshu : Monster
{
    [Header("�̵� ����")]
    public bool IsGrounded
    {
        get
        {
            return anim.GetBool("isGrounded");
        }

        set
        {
            anim.SetBool("isGrounded", value);
        }
    }
    public bool IsWalking
    {
        get
        {
            return anim.GetBool("isWalking");
        }

        set
        {
            anim.SetBool("isWalking", value);
        }
    }
    public bool IsBackward
    {
        get
        {
            return anim.GetBool("isBackward");
        }

        set
        {
            anim.SetBool("isBackward", value);
        }
    }

    [Header("���� ����")]
    private const float ORIGINAL_GRAVITY = 10f;
    public float verticalVelocity; // ���� '��'�� ����� ��Ÿ���� ���� �ӵ�
    private float gravity = ORIGINAL_GRAVITY; // ���� �߷°�
    private int airHitCounter = 0;

    [Header("���� ����")]
    [SerializeField] private GameObject attackHitboxObject;
    private MonsterHitbox attackHitbox;

    [Header("AI ����")]
    protected bool isActing = false; // ���� � �ൿ(Idle, Move ��)�� �ϰ� �ִ��� ����
    
    // ������ �ν� ���·� ���� ���̹Ƿ� isAware �Լ��� �ʿ����
    // protected bool isAware = false;

    [Header("AI Configuration")]
    [Tooltip("���� ��, �̵� ������ ���� ���� �Ʒ� ��踦 ��Ÿ���� Ʈ������")]
    [SerializeField] private Transform combatMinBoundary;
    [Tooltip("���� ��, �̵� ������ ���� ������ �� ��踦 ��Ÿ���� Ʈ������")]
    [SerializeField] private Transform combatMaxBoundary;

    [SerializeField] private GameObject meteorPrefab; // ��뽴�� ��ȯ�� ���׿�

    private bool isDead // HP�� 0���Ϸ� �������°� (��� ���� �ߺ� ���� ������)
    {
        get
        {
            return anim.GetBool("isDead");
        }

        set
        {
            anim.SetBool("isDead", value);
        }
    } 

    private CancellationTokenSource aiLoopCts; // �񵿱� �۾� ����. �ܺο����� CancellationToken�� ���

    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake();
        // ��Ʈ�ڽ� ��ũ��Ʈ ����
        if (attackHitboxObject != null)
        {
            attackHitbox = attackHitboxObject.GetComponent<MonsterHitbox>();
        }
    }
    protected override void Start()
    {
        base.Start();

        // AI ���� ����
        StartAILoop();
    }
    private void Update()
    {
        HandleGravity();
    }
    void OnGUI()
    {
        GUI.Label(new Rect(10, 120, 200, 20), "Vinoshu IsGrounded: " + IsGrounded);
        GUI.Label(new Rect(10, 130, 200, 20), "Vinoshu IsWalking: " + IsWalking);
        GUI.Label(new Rect(10, 140, 200, 20), "Vinoshu isBackward: " + IsBackward);
        GUI.Label(new Rect(10, 150, 200, 20), "Vinoshu isActing: " + isActing);
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

        // ������ �̵� ��� �ߴ�
        rb.linearVelocity = Vector2.zero;
        IsWalking = false;
    }

    private async UniTask AI_Loop(CancellationToken token)
    {
        while (token.IsCancellationRequested == false)
        {
            await Pattern_Boss(token);

            // �� ������ ������� �ʰ�, �ణ�� �����̸� �־� ���� ���ϸ� ����
            await UniTask.Delay(100, cancellationToken: token);
        }
    }

    // --- ���� ���� ---
    private async UniTask Pattern_Boss(CancellationToken token)
    {
        if (isActing) return; // �̹� �ٸ� �ൿ���̸� �������� ����


        // �÷��̾ ���� ���� ���� �ȿ� ���� ���
        if (IsPlayerInAttackRange(monsterData.attackDetails[0])) // ���� ���� �������� �Ǵ�
        {
            // �ൿ ������ ����
            float action = Random.value;

            // 60% Ȯ���� ���� ����
            if (action > 0.4f)
            {
                await Attack(token); // ���� ����
            } else if (action < 0.1f)
            {
                // ���׿�
                await Meteor(token); // ���׿� ���� ����

            } else
            {
                // ���� or ����
                Vector3 destination; // �̵� ��ǥ ����
                float directionFactor = -1f;
                IsBackward = true;
                if (action == 1)
                {
                    directionFactor = 1f;
                    IsBackward = false;
                }
                Vector3 direction = (playerTransform.position - transform.position).normalized * directionFactor;
                destination = transform.position + direction * Random.Range(1f, 3f);

                // ��ǥ ������ combatMinBoundary/combatMaxBoundary ���� ����
                if (combatMinBoundary != null && combatMaxBoundary != null)
                {
                    destination.x = Mathf.Clamp(destination.x, combatMinBoundary.position.x, combatMaxBoundary.position.x);
                    destination.y = Mathf.Clamp(destination.y, combatMinBoundary.position.y, combatMaxBoundary.position.y);
                }
                await MoveTo(destination, token);
            }
        }
        else // ���� ���� �ۿ� ������
        {
            // �ൿ ������ ����
            float action = Random.value;
            Vector3 destination;

            // 10% Ȯ���� ���׿�
            if (action < 0.1f)
            {
                // ���׿�
                await Meteor(token);
            }
            else if (action > 0.7f) // ������. ��� / ���� / ����
            {
                action = Random.Range(0, 3);
                switch (action)
                {
                    case 0: // ��� 1 ~ 2�� ���
                        float idleTime = Random.Range(1f, 2f);
                        await UniTask.Delay(System.TimeSpan.FromSeconds(idleTime), cancellationToken: token);
                        break;
                    case 1: // �÷��̾�� ����
                    case 2: // �÷��̾�Լ� ����
                            // �����̸� +, ����� - ����
                        float directionFactor = -1f;
                        IsBackward = true;
                        if (action == 1)
                        {
                            directionFactor = 1f;
                            IsBackward = false;
                        }
                        Vector3 direction = (playerTransform.position - transform.position).normalized * directionFactor;
                        destination = transform.position + direction * Random.Range(1f, 3f);

                        // ��ǥ ������ combatMinBoundary/combatMaxBoundary ���� ����
                        if (combatMinBoundary != null && combatMaxBoundary != null)
                        {
                            destination.x = Mathf.Clamp(destination.x, combatMinBoundary.position.x, combatMaxBoundary.position.x);
                            destination.y = Mathf.Clamp(destination.y, combatMinBoundary.position.y, combatMaxBoundary.position.y);
                        }
                        await MoveTo(destination, token);
                        break;
                }

            }
        }
    }

    protected async UniTask Attack(CancellationToken token)
    {
        isActing = true;
        try
        {
            // ù ��° ���� ������ ������
            currentAttackDetails = monsterData.attackDetails[0];

            // ���� �������� ����Ͽ� AttackDetails�� ä������
            currentAttackDetails.damageRate *= this.atk;

            // ��Ʈ�ڽ��� �ϼ��� ���� ������ �����Ͽ� �ʱ�ȭ
            if (attackHitbox != null)
            {
                attackHitbox.Initialize(currentAttackDetails);
            }
            FlipTowardsPlayer();
            anim.SetTrigger("attack");
            Debug.Log("��뽴�� ���� ����!");

            await UniTask.Delay(1100, cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            // ���� ó��
        }
        finally
        {
            isActing = false;
        }
    }
    protected async UniTask Meteor(CancellationToken token)
    {
        isActing = true;
        try
        {
            // �ν����Ϳ��� �����س��� �� ��° ���� ����(���׿� ���� ����)�� ������
            Debug.Log("��뽴�� ���׿� ����!");
            currentAttackDetails = monsterData.attackDetails[1];
            currentAttackDetails.damageRate *= this.atk; // ���ݷ��� ���� ���׿����� ������ ����
            FlipTowardsPlayer();
            anim.SetTrigger("cast");

            // ������ ����. MagicCircle ����Ʈ�� visuals�� 0.4f ��ŭ ������ �������־� �׸�ŭ offset�� �־������
            Vector3 targetPosition = new Vector3(playerTransform.position.x, playerTransform.position.y + 0.4f, playerTransform.position.z);
            EffectManager.Instance.PlayEffect("MagicCircle", targetPosition, Quaternion.identity);

            // �ٸ� �ൿ ������ ���� ���� cast�ִϸ��̼��� ���� 1.1�� ��ŭ ���
            await UniTask.Delay(1100, cancellationToken: token);

            targetPosition.y -= 0.4f;

            Debug.Log("���׿� ��ȯ!");
            if (meteorPrefab != null)
            {
                GameObject meteorInstance = Instantiate(meteorPrefab, targetPosition, Quaternion.identity);

                // ���׿��� Ÿ���� ���ϵ��� ���� ���� ������ �Բ� �ʱ�ȭ
                meteorInstance.GetComponent<VinoshuMeteor>().Initialize(currentAttackDetails, targetPosition);
            }

        }
        catch (OperationCanceledException)
        {
            // ���� ó��
        }
        finally
        {
            isActing = false;
        }        
    }
    // ��ǥ �������� �̵�
    private async UniTask MoveTo(Vector3 destination, CancellationToken token)
    {
        IsWalking = true;
        isActing = true;

        try
        {
            while (Vector2.Distance(transform.position, destination) > 0.1f
                   && !token.IsCancellationRequested)
            {
                Vector2 direction = (destination - transform.position).normalized;
                rb.linearVelocity = direction * moveSpeed;

                FlipTowardsPlayer();

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException)
        {
            // ���� ó��
        }
        finally
        {
            rb.linearVelocity = Vector2.zero;
            IsWalking = false;
            isActing = false;
        }
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
        // Do nothing
    }

    // ���� �ִϸ��̼��� ������ �� ȣ��
    public override void OnAttackStateExit()
    {
        isActing = false;
    }

    // �ǰ� �ִϸ��̼��� ������ �� ȣ��
    public override void OnHurtStateExit() {
        // �ǰ��� ������ ���� ���·� �����ϰ� AI ���� �����
        isActing = false;
    }

    // ��� �ִϸ��̼��� ������ �� ȣ��
    public override void OnGetUpStateExit()
    {
        // �ǰ��� ������ ���� ���·� �����ϰ� AI ���� �����
        isActing = false;
        visualsTransform.localPosition = startPos;
    }
    #endregion State Behaviour

    #region Utilities
    private bool IsPlayerInRecognitionRange()
    {
        return playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= recognitionRange;
    }

    private bool IsPlayerInAttackRange(AttackDetails currentAttackDetails)
    {
        if (playerTransform == null) return false;

        // X�� �Ÿ� ���
        float distanceX = Mathf.Abs(playerTransform.position.x - transform.position.x);

        // Y�� �Ÿ� ��� (Visuals�� Y ��ġ�� ��������)
        float distanceY = Mathf.Abs((playerTransform.position.y) - (transform.position.y));

        // X�� �Ÿ��� ���� ���� ���� �ְ�, Y�� �Ÿ��� ���� ����(yOffset) ���� �ִ��� Ȯ��
        return distanceX <= attackRange && distanceY <= currentAttackDetails.yOffset;
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

    public override void OnDamaged(AttackDetails attackDetails, Vector2 attackPosition)
    {
        // ���� ������ ���
        float damage = CalculateDamage(attackDetails);

        // ������ �ؽ�Ʈ ���
        EffectManager.Instance.PlayEffect("DefaultDamageText", hurtboxTransform.position, Quaternion.identity, damage);

        // �ǰ� ����
        Hurt(attackDetails, attackPosition);

        // �̹� �׾��ٸ� ������ ����X. return
        if (isDead) return;

        // ������ ����
        previousHP = currentHP;
        currentHP -= damage;
        Debug.Log($"{monsterData.MonsterName}��(��) {damage}�� �������� ����. ���� ü��: {currentHP}");

        if (currentHP <= 0)
        {
            isDead = true; // ���� ���� ���� �÷���
            WaitUntilGroundedAndDie(this.GetCancellationTokenOnDestroy()).Forget();
        }

        // UIManager�� �ڽ��� Ÿ������ �˸�
        UIManager.Instance.OnMonsterDamaged(this);
        //UIManager.Instance.UpdateMonsterHP();

    }

    // ������ ��ٷȴٰ� Die()�� ȣ���ϴ� �񵿱� �Լ�
    private async UniTask WaitUntilGroundedAndDie(CancellationToken token)
    {
        // IsGrounded�� true�� �� ������ �� ������ Ȯ���ϸ� ���
        await UniTask.WaitUntil(() => IsGrounded, cancellationToken: token);

        Die();
    }

    protected override void Hurt(AttackDetails attackDetails, Vector2 attackPosition)
    {
        StopAILoop(); // �񵿱� �۾� ��� �ߴ�
        isActing = false;
        IsWalking = false;
       
        rb.linearVelocity = Vector2.zero; // �˹� ���� �ӵ� �ʱ�ȭ

        // ����Ʈ ��� ��û

        // hurtbox ������ ����Ʈ�� ����
        Vector3 hurtPoint = hurtboxTransform.position;

        // ����Ʈ�� ������ �÷��̾ �ٶ󺸴� ������ �����ų�, �⺻ �������� ����
        Quaternion effectRotation = (transform.position.x > attackPosition.x) ? Quaternion.identity : Quaternion.Euler(0, 180, 0);

        // attackDetails�� ����Ʈ �̸��� �ִٸ� �װ� ���, ���ٸ� �⺻ ����Ʈ ���
        // string effectToPlay = string.IsNullOrEmpty(attackDetails.effectName) ? "NormalHit_Slash" : attackDetails.effectName;
        string effectToPlay = "SlashSmall" + Random.Range(1, 4);
        EffectManager.Instance.PlayEffect(effectToPlay, hurtPoint, Quaternion.identity);
        EffectManager.Instance.PlayEffect("BloodLarge", hurtPoint, effectRotation);


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

                anim.SetTrigger("hurt" + Random.Range(1, 3));
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
        UIManager.Instance.HideMonsterHPBar(); // HP�ٸ� ���⵵�� ��û

        Debug.Log($"{monsterData.MonsterName}��(��) �׾����ϴ�.");

        // ������ �����Ӱ� �浹�� ����
        rb.linearVelocity = Vector2.zero;
        GetComponentInChildren<Collider2D>().enabled = false;

        DeathSequenceAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTask DeathSequenceAsync(CancellationToken token)
    {
        // 1. �Ͼ�� ���ϰ� ���� �����ϰ�
        var mat = sr.material;
        mat.SetFloat("_Blend", 1f);
        float duration = 0.3f; // �����ϰ� ���ϴ� �� �ɸ��� �ð�
        float elapsedTime = 0f;

        // ��Ƽ������ ������Ƽ ���� �ִϸ��̼�
        while (elapsedTime < duration)
        {
            // ���� ��� ��� (0���� 1�� ����)
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);

            // �������� ����ϴ� ��Ƽ������ "_Alpha" ������Ƽ ���� ����
            mat.SetFloat("_Alpha", alpha);

            elapsedTime += Time.deltaTime;
            await UniTask.Yield(token);
        }

        // 2. �Ҹ� �� ���� ����
        // ���� ��ġ�� ����Ʈ�� ����
        Vector3 hurtPoint = hurtboxTransform.position;
        EffectManager.Instance.PlayEffect("MonsterDieFlash", hurtPoint, Quaternion.identity);
        
        sr.enabled = false;

        // 3. ���� ������Ʈ �ı�
        await UniTask.Delay(System.TimeSpan.FromSeconds(1.0), cancellationToken: token);
        Destroy(gameObject);
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
        if (visualsTransform.localPosition.y <= startPos.y - 0.25f) // ��뽴�� ������ ��¦ ���ִ� ä�� �����̴� ���ͱ� ������ 0.25��ŭ y���� ���־�� ���� ����� ������ ���� ����
        {
            if (verticalVelocity < -1f)
            {
                verticalVelocity *= -0.5f;
                return;
            }
            // ���� �ʱ�ȭ
            IsGrounded = true;
            airHitCounter = 0;


            // ��ġ�� �ӵ�, �߷� �ʱ�ȭ
            rb.linearVelocity = Vector2.zero;
            // OnGetUpStateExit()���� visualsTransform�� �� ��ġ�� �����ϵ��� ���� �ű�
            // visualsTransform.localPosition = startPos;
            verticalVelocity = 0f;
            gravity = ORIGINAL_GRAVITY;
        }
    }
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // �⺻ �ν�/���� ���� ����� �׸���

        // 2. ���� ���� �׸��� (�Ķ���)
        if (combatMinBoundary != null && combatMaxBoundary != null)
        {
            Gizmos.color = Color.blue;
            Vector3 p1 = combatMinBoundary.position;
            Vector3 p2 = new Vector3(combatMaxBoundary.position.x, combatMinBoundary.position.y);
            Vector3 p3 = combatMaxBoundary.position;
            Vector3 p4 = new Vector3(combatMinBoundary.position.x, combatMaxBoundary.position.y);

            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p4);
            Gizmos.DrawLine(p4, p1);
        }
    }
}