using UnityEngine;
using Random = UnityEngine.Random;
using Cysharp.Threading.Tasks; // UniTask
using System.Threading;       // CancellationToken
using System;

public class Goblin : Monster
{
    [Header("이동 상태")]
    public bool IsGrounded = true;
    public bool IsWalking = false;

    [Header("물리 변수")]
    private const float ORIGINAL_GRAVITY = 10f;
    public float verticalVelocity; // 수직 '힘'의 결과로 나타나는 현재 속도
    private float gravity = ORIGINAL_GRAVITY; // 가상 중력값
    private int airHitCounter = 0;

    [Header("공격 판정")]
    [SerializeField] private GameObject attackHitboxObject;
    private MonsterHitbox attackHitbox;

    [Header("AI 상태")]
    protected bool isActing = false; // 현재 어떤 행동(Idle, Move 등)을 하고 있는지 여부
    protected bool isAware = false; // 플레이어를 인식했는가

    [Header("AI Configuration")]
    [Tooltip("순찰 시, 초기 위치를 중심으로 한 활동 반경")]
    [SerializeField] private Vector2 patrolAreaSize;
    [Tooltip("전투 시, 이동 가능한 가장 왼쪽 아래 경계")]
    [SerializeField] private Transform combatMinBoundary;
    [Tooltip("전투 시, 이동 가능한 가장 오른쪽 위 경계")]
    [SerializeField] private Transform combatMaxBoundary;
    private Vector3 initialPosition; // 몬스터의 초기 위치

    [Header("사망 연출")]
    [SerializeField] private GameObject[] fragPrefabs; // 시체 파편
    private bool isDead = false; // HP가 0이하로 떨어졌는가 (사망 로직 중복 실행 방지용)


    private CancellationTokenSource aiLoopCts; // 비동기 작업 관리. 외부에서는 CancellationToken만 사용
    private CancellationTokenSource moveCts; // 이동 작업 전용 토큰 - 이동 중단을 위해

    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake();
        // 히트박스 스크립트 참조
        if (attackHitboxObject != null)
        {
            attackHitbox = attackHitboxObject.GetComponent<MonsterHitbox>();
        }
    }
    protected override void Start()
    {
        base.Start();
        initialPosition = transform.position; // 초기 위치 저장

        // AI 루프 시작
        StartAILoop();
    }
    private void Update()
    {
        HandleGravity();

        // 애니메이션 업데이트

        anim.SetBool("isGrounded", IsGrounded);
        anim.SetBool("isWalking", IsWalking);
        anim.SetBool("isDead", isDead);
    }

    /*
    void OnGUI()
    {
        GUI.Label(new Rect(10, 80, 200, 20), "goblin IsGrounded: " + IsGrounded);
        GUI.Label(new Rect(10, 90, 200, 20), "goblin IsWalking: " + IsWalking);
        GUI.Label(new Rect(10, 100, 200, 20), "goblin isActing: " + isActing);
        GUI.Label(new Rect(10, 110, 200, 20), "goblin isAware: " + isAware);
    }
    */
    #endregion Unity Lifecycle


    #region AI System
    private void StartAILoop()
    {
        // 이전 CancellationTokenSource가 있다면 return
        if (aiLoopCts != null) return;

        // 오브젝트 파괴 시 취소되는 토큰과 연결된 새로운 CancellationTokenSource 생성
        var destroyToken = this.GetCancellationTokenOnDestroy();
        aiLoopCts = CancellationTokenSource.CreateLinkedTokenSource(destroyToken);

        // AI 루프 시작
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
        moveCts?.Cancel();
        moveCts?.Dispose();
        moveCts = null;

        // 물리적 이동 즉시 중단
        rb.linearVelocity = Vector2.zero;
        IsWalking = false;
    }

    private async UniTask AI_Loop(CancellationToken token)
    {
        while (token.IsCancellationRequested == false)
        {
            // 플레이어 인식 상태에 따라 다른 행동 패턴 실행
            if (isAware)
                await Pattern_Aware(token);
            else
                await Pattern_UnAware(token);

            // 매 프레임 실행되지 않고, 약간의 딜레이를 주어 성능 부하를 줄임
            await UniTask.Delay(100, cancellationToken: token);
        }
    }

    // --- 감지 전: 순찰 패턴 ---
    private async UniTask Pattern_UnAware(CancellationToken token)
    {
        if (isActing) return; // 이미 다른 행동 중이면 실행하지 않음


        // 플레이어 감지
        if (IsPlayerInRecognitionRange())
        {
            isAware = true; // 전투 상태로 전환
            Debug.Log("플레이어 감지! 전투 태세로 전환");
            return;
        }

        isActing = true;
        IsWalking = false;

        // 50% 확률로 대기. 아니면 이동
        if (Random.value < 0.5f)
        {
            // 랜덤 시간(1~2초) 동안 대기
            float idleTime = Random.Range(1f, 2f);
            await UniTask.Delay(System.TimeSpan.FromSeconds(idleTime), cancellationToken: token);
        }
        else
        {
            // 순찰 영역 내 랜덤 목적지 설정
            Vector2 patrolAreaCenter = initialPosition;
            Vector2 randomOffset = new Vector2(
                Random.Range(-patrolAreaSize.x / 2, patrolAreaSize.x / 2),
                Random.Range(-patrolAreaSize.y / 2, patrolAreaSize.y / 2)
            );
            Vector2 destination = patrolAreaCenter + randomOffset;

            // 생성된 순찰 목적지를 전투 경계 안으로 보정
            if (combatMinBoundary != null && combatMaxBoundary != null)
            {
                destination.x = Mathf.Clamp(destination.x, combatMinBoundary.position.x, combatMaxBoundary.position.x);
                destination.y = Mathf.Clamp(destination.y, combatMinBoundary.position.y, combatMaxBoundary.position.y);
            }

            // 순찰 범위 내의 랜덤한 목표 지점으로 이동
            await MoveTo(destination, token);
        }

        isActing = false;
    }


    // --- 감지 후: 전투 패턴 ---
    private async UniTask Pattern_Aware(CancellationToken token)
    {
        if (isActing) return; // 이미 다른 행동중이면 실행하지 않음
        // 플레이어가 공격 범위 안에 있으면 70% 확률로 공격
        if (IsPlayerInAttackRange(monsterData.attackDetails[0]) && Random.value > 0.3f)
        {
            isActing = true;
            IsWalking = false;
            rb.linearVelocity = Vector2.zero;
            FlipTowardsPlayer();

            Attack(); // 공격 실행

            // 공격 후 딜레이를 줘 다른 행동 방지 + 내부 쿨타임 역할
            await UniTask.Delay(3000, cancellationToken: token);
        }
        else // 공격 범위 밖에 있으면 경계 행동
        {
            isActing = true;
            IsWalking = false;

            // 3가지 행동 중 하나를 랜덤하게 선택
            Vector3 destination;
            int action = Random.Range(0, 3);
            switch (action)
            {
                case 0: // 잠시 대기
                    float idleTime = Random.Range(1f, 2f);
                    await UniTask.Delay(System.TimeSpan.FromSeconds(idleTime), cancellationToken: token);
                    break;
                case 1: // 플레이어에게 접근
                case 2: // 플레이어에게서 후퇴
                    // 접근이면 +, 후퇴면 - 방향
                    float directionFactor = (action == 1) ? 1f : -1f;
                    Vector3 direction = (playerTransform.position - transform.position).normalized * directionFactor;
                    destination = transform.position + direction * Random.Range(1f, 3f);

                    // 목표 지점을 combatMinBoundary/combatMaxBoundary 내로 보정
                    if (combatMinBoundary != null && combatMaxBoundary != null)
                    {
                        destination.x = Mathf.Clamp(destination.x, combatMinBoundary.position.x, combatMaxBoundary.position.x);
                        destination.y = Mathf.Clamp(destination.y, combatMinBoundary.position.y, combatMaxBoundary.position.y);
                    }
                    await MoveTo(destination, token);
                    break;
            }
        }
        isActing = false;
    }

    // 목표 지점까지 이동하는 UniTask 함수
    private async UniTask MoveTo(Vector3 destination, CancellationToken parentToken)
    {
        // 이전 이동 작업이 있다면 중단
        StopMovement();

        // 새로운 이동 토큰 생성 (부모 토큰과 연결)
        moveCts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
        var moveToken = moveCts.Token;

        IsWalking = true;

        try
        {
            while (Vector2.Distance(transform.position, destination) > 0.1f
                   && !moveToken.IsCancellationRequested)
            {
                Vector2 direction = (destination - transform.position).normalized;
                rb.linearVelocity = direction * moveSpeed;

                if (isAware) FlipTowardsPlayer();
                else Flip(direction.x);

                await UniTask.Yield(PlayerLoopTiming.Update, moveToken);
            }
        }
        catch (OperationCanceledException)
        {
            // 예외 처리
        }
        finally
        {
            rb.linearVelocity = Vector2.zero;
            IsWalking = false;
        }
    }
    #endregion AI System

    #region State Behaviour
    // 대기 애니메이션으로 진입 시 호출
    public override void OnIdleStateEnter()
    {
        // AI 루프 재시작 함수 호출
        StartAILoop();
    }

    public override void OnWalkStateExit()
    {
        StopMovement();
    }

    // 공격 애니메이션이 끝났을 때 호출
    public override void OnAttackStateExit()
    {
        isActing = false;
    }

    // 피격 애니메이션이 끝났을 때 호출
    public override void OnHurtStateExit() {
        // 피격이 끝나면 전투 상태로 복귀하고 AI 루프 재시작
        isAware = true; // 피격당했으니 플레이어는 감지된 상태
        isActing = false;
    }

    // 기상 애니메이션이 끝났을 때 호출
    public override void OnGetUpStateExit()
    {
        // 피격이 끝나면 전투 상태로 복귀하고 AI 루프 재시작
        isAware = true;
        isActing = false;
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

        // X축 거리 계산
        float distanceX = Mathf.Abs(playerTransform.position.x - transform.position.x);

        // Y축 거리 계산 (Visuals의 Y 위치를 기준으로)
        float distanceY = Mathf.Abs((playerTransform.position.y) - (transform.position.y));

        // X축 거리가 공격 범위 내에 있고, Y축 거리도 공격 범위(yOffset) 내에 있는지 확인
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
        // 입을 데미지 계산
        float damage = CalculateDamage(attackDetails);

        // 데미지 텍스트 출력
        EffectManager.Instance.PlayEffect("DefaultDamageText", hurtboxTransform.position, Quaternion.identity, damage);

        // 피격 반응
        Hurt(attackDetails, attackPosition);

        // 이미 죽었다면 데미지 적용X. return
        if (isDead) return;

        // 데미지 적용
        previousHP = currentHP;
        currentHP -= damage;
        Debug.Log($"{monsterData.MonsterName}이(가) {damage}의 데미지를 입음. 현재 체력: {currentHP}");

        if (currentHP <= 0)
        {
            isDead = true; // 죽음 절차 시작 플래그
            WaitUntilGroundedAndDie(this.GetCancellationTokenOnDestroy()).Forget();
        }

        // UIManager에 자신을 타겟으로 알림
        UIManager.Instance.OnMonsterDamaged(this);
        //UIManager.Instance.UpdateMonsterHP();

    }

    // 착지를 기다렸다가 Die()를 호출하는 비동기 함수
    private async UniTask WaitUntilGroundedAndDie(CancellationToken token)
    {
        // IsGrounded가 true가 될 때까지 매 프레임 확인하며 대기
        await UniTask.WaitUntil(() => IsGrounded, cancellationToken: token);

        Die();
    }

    protected override void Hurt(AttackDetails attackDetails, Vector2 attackPosition)
    {
        StopAILoop(); // 모든 비동기 작업 즉시 중단
        isActing = false;
        IsWalking = false;
       
        rb.linearVelocity = Vector2.zero; // 넉백 전에 속도 초기화

        // 이펙트 재생 요청

        // hurtbox 지점에 이펙트를 생성
        Vector3 hurtPoint = hurtboxTransform.position;

        // 출혈 이펙트의 방향을 조절하기 위한 변수. attackPosition(플레이어 히트박스의 좌표)와 몬스터의 좌표를 비교
        // 이펙트의 방향은 플레이어가 바라보는 방향을 따르거나, 기본 방향으로 설정
        Quaternion effectRotation = (transform.position.x > attackPosition.x) ? Quaternion.identity : Quaternion.Euler(0, 180, 0);

        // attackDetails에 이펙트 이름이 있다면 그걸 사용, 없다면 기본 이펙트 사용
        // string effectToPlay = string.IsNullOrEmpty(attackDetails.effectName) ? "NormalHit_Slash" : attackDetails.effectName;
        string effectToPlay = "SlashSmall" + Random.Range(1, 4);
        EffectManager.Instance.PlayEffect(effectToPlay, hurtPoint, Quaternion.identity);
        EffectManager.Instance.PlayEffect("BloodLarge", hurtPoint, effectRotation);


        // 넉백 적용 방향 결정. attackPosition은 플레이어 히트박스의 위치
        float direction = (transform.position.x > attackPosition.x) ? 1 : -1;

        if (IsGrounded) // 땅에 있을 때
        {
            
            if (attackDetails.launchForce > 0)
            {
                // 수평 넉백
                rb.AddForce(new Vector2(direction * attackDetails.knockbackForce, 0), ForceMode2D.Impulse);

                // 공중에 뜨는 힘 적용
                verticalVelocity = attackDetails.launchForce;

                IsGrounded = false;
                anim.SetTrigger("airborne");
            }
            else
            {   
                // 수평 넉백
                transform.position += new Vector3(direction * attackDetails.knockbackForce * 0.1f, 0);

                anim.SetTrigger("hurt");
            }
        }
        else // 공중에 있을 때
        {   
            // 수평 넉백
            rb.AddForce(new Vector2(direction * attackDetails.knockbackForce, 0), ForceMode2D.Impulse);

            if (attackDetails.launchForce > 0) airHitCounter++;
            verticalVelocity = 2f + (attackDetails.launchForce * Mathf.Max(0, 0.5f - (airHitCounter * 0.125f)));
            gravity += 0.05f;
        }
    }

    protected override void Die()
    {
        StopAILoop(); // 모든 비동기 작업 중단
        isActing = false;
        UIManager.Instance.HideMonsterHPBar(); // HP바를 숨기도록 요청

        Debug.Log($"{monsterData.MonsterName}이(가) 죽었습니다.");

        // 플레이어에게 경험치 지급
        GameManager.Instance.AddExp(monsterData.EXP);
        GameManager.Instance.AddHuntExp(monsterData.EXP);

        // 물리적 움직임과 충돌을 중지
        rb.linearVelocity = Vector2.zero;
        GetComponentInChildren<Collider2D>().enabled = false;

        DeathSequenceAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTask DeathSequenceAsync(CancellationToken token)
    {
        // 1. 하얗게 변하고 점점 투명하게
        var mat = sr.material;
        mat.SetFloat("_Blend", 1f);
        float duration = 0.3f; // 투명하게 변하는 데 걸리는 시간
        float elapsedTime = 0f;

        // 머티리얼의 프로퍼티 값을 애니메이션
        while (elapsedTime < duration)
        {
            // 보간 계수 계산 (0에서 1로 증가)
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);

            // 렌더러가 사용하는 머티리얼의 "_Alpha" 프로퍼티 값을 변경
            mat.SetFloat("_Alpha", alpha);

            elapsedTime += Time.deltaTime;
            await UniTask.Yield(token);
        }

        // 2. 소멸 및 파편 생성
        // 몬스터 위치에 이펙트를 생성
        Vector3 hurtPoint = hurtboxTransform.position;
        EffectManager.Instance.PlayEffect("MonsterDieYoung", hurtPoint, Quaternion.identity);
        
        sr.enabled = false;

        if (fragPrefabs != null && fragPrefabs.Length > 0)
        {
            // 파편 생성 위치: 몬스터의 월드 좌표
            Vector3 spawnPosition = transform.position;

            foreach (GameObject fragPrefab in fragPrefabs)
            {
                // 파편 생성
                GameObject fragment = Instantiate(fragPrefab, spawnPosition, Quaternion.identity);
                MonsterFragment fragmentObj = fragment.GetComponent<MonsterFragment>();

                if (fragmentObj != null)
                {
                    // 각 파편에 가할 힘 계산
                    Vector2 horizontalForce = new Vector2(Random.Range(-1f, 1f), Random.Range(-0.3f, 0.3f)).normalized * Random.Range(3f, 7f);
                    float verticalForce = Random.Range(5f, 7f);

                    // 파편에 힘 적용
                    fragmentObj.Initialize(horizontalForce, verticalForce);
                }
            }
        }
        // 3. 최종 오브젝트 파괴
        await UniTask.Delay(System.TimeSpan.FromSeconds(1.0), cancellationToken: token);
        Destroy(gameObject);
    }

    protected void Attack()
    {
        // 첫 번째 공격 정보를 가져옴
        currentAttackDetails = monsterData.attackDetails[0];

        // 최종 데미지를 계산하여 AttackDetails에 채워넣음
        currentAttackDetails.damageRate *= this.atk;

        // 히트박스에 완성된 공격 정보를 전달하여 초기화
        if (attackHitbox != null)
        {
            attackHitbox.Initialize(currentAttackDetails);
        }

        anim.SetTrigger("attack");
        Debug.Log("고블린의 공격!");
    }
    public void HandleGravity()
    {
        // 1. 공중에 떠 있다면
        if (!IsGrounded)
        {
            // 2. 중력을 계속 적용
            verticalVelocity += (-gravity) * Time.deltaTime;

            // 3. 계산된 속도로 Visuals의 local Y좌표를 변경
            visualsTransform.localPosition += new Vector3(0, verticalVelocity * Time.deltaTime, 0);

            // 4. 착지했는지 확인
            CheckForLanding();
        }
    }

    // 착지 판별 로직
    private void CheckForLanding()
    {
        // Visuals의 Y 좌표가 시작 Y좌표보다 아래로 내려갔다면 착지로 간주
        if (visualsTransform.localPosition.y <= startPos.y)
        {
            if (verticalVelocity < -1.5f)
            {
                verticalVelocity *= -0.5f;
                return;
            }
            // 상태 초기화
            IsGrounded = true;
            airHitCounter = 0;


            // 위치와 속도, 중력 초기화
            rb.linearVelocity = Vector2.zero;
            visualsTransform.localPosition = startPos;
            verticalVelocity = 0f;
            gravity = ORIGINAL_GRAVITY;
        }
    }
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // 기본 공격 범위 기즈모 그리기

        Gizmos.color = Color.yellow; // 인식 범위는 노란색
        Gizmos.DrawWireSphere(transform.position, recognitionRange);

        // 1. 순찰 영역 그리기 (녹색)
        Gizmos.color = Color.green;
        // 실행 중이 아닐 때만 초기 위치를 사용, 실행 중일 땐 실제 초기 위치를 사용
        Vector3 patrolCenter = initialPosition;
        Gizmos.DrawWireCube(patrolCenter, patrolAreaSize);

        // 2. 전투 영역 그리기 (파란색)
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