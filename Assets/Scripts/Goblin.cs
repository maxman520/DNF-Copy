using UnityEngine;
using Cysharp.Threading.Tasks; // UniTask
using System.Threading;       // CancellationToken

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

    [Header("AI 상태")]
    protected bool isActing = false; // 현재 어떤 행동(Idle, Move 등)을 하고 있는지 여부
    protected bool isAware = false; // 플레이어를 인식했는가

    [Header("AI Configuration")]
    [SerializeField] private Vector2 patrolAreaCenter; // 순찰 구역 중심 (로컬 좌표)
    [SerializeField] private Vector2 patrolAreaSize;   // 순찰 구역 크기
    private Vector3 initialPosition; // 몬스터의 초기 위치


    private CancellationTokenSource aiLoopCts; // 비동기 작업 관리. 외부에서는 CancellationToken만 사용
    private CancellationTokenSource moveCts; // 이동 작업 전용 토큰 - 이동 중단을 위해

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake(); // 부모의 Awake를 먼저 호출
    }

    protected void Start()
    {
        if (Player.Instance != null)
        {
            playerTransform = Player.Instance.transform;
        }
        initialPosition = transform.position; // 초기 위치 저장
        patrolAreaCenter += (Vector2)initialPosition; // 순찰 중심점을 월드 좌표로 변환

        // AI 루프 시작
        StartAILoop();
    }
    private void Update()
    {
        HandleGravity();

        // 애니메이션 업데이트
        anim.SetBool("isGrounded", IsGrounded);
        anim.SetBool("isWalking", IsWalking);


        // 사망 처리
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
        // 이동 작업 중단
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
            Debug.Log($"순찰: {idleTime:F1}초 동안 대기");
            await UniTask.Delay(System.TimeSpan.FromSeconds(idleTime), cancellationToken: token);
        }
        else
        {
            // 순찰 영역 내 랜덤 목적지 설정
            Vector2 randomOffset = new Vector2(
                Random.Range(-patrolAreaSize.x / 2, patrolAreaSize.x / 2),
                Random.Range(-patrolAreaSize.y / 2, patrolAreaSize.y / 2)
            );
            Vector2 destination = patrolAreaCenter + randomOffset;

            // 순찰 범위 내의 랜덤한 목표 지점으로 이동
            Debug.Log($"순찰: {destination.x}, {destination.y} 위치로 이동");

            await MoveTo(destination, token);
        }

        isActing = false;
    }


    // --- 감지 후: 전투 패턴 ---
    private async UniTask Pattern_Aware(CancellationToken token)
    {
        if (isActing) return; // 이미 다른 행동중이면 실행하지 않음

        // 플레이어가 공격 범위 안에 있으면 70% 확률로 공격
        if (IsPlayerInAttackRange()&& Random.value > 0.3f)
        {
            isActing = true;
            IsWalking = false;
            rb.linearVelocity = Vector2.zero;
            FlipTowardsPlayer();

            Attack(); // 공격 실행

            // 공격 후 딜레이
            await UniTask.Delay(3000, cancellationToken: token); // 공격 쿨타임
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
                    Debug.Log($"경계: {idleTime:F1}초 동안 대기");
                    await UniTask.Delay(System.TimeSpan.FromSeconds(idleTime), cancellationToken: token);
                    break;
                case 1: // 플레이어에게 접근
                    destination = transform.position + (playerTransform.position - transform.position).normalized * Random.Range(1f, 3f);
                    Debug.Log($"경계: 플레이어에게 접근");
                    await MoveTo(destination, token);
                    break;
                case 2: // 플레이어에게서 후퇴
                    destination = transform.position - (playerTransform.position - transform.position).normalized * Random.Range(1f, 3f);
                    Debug.Log($"경계: 플레이어에게서 후진");
                    await MoveTo(destination, token);
                    break;
            }
        }
        isActing = false;
    }

    // 목표 지점까지 이동하는 UniTask 함수
    private async UniTask MoveTo(Vector3 destination, CancellationToken parentToken)
    {
        // 이전 이동 작업 중단
        StopMovement();

        // 새로운 이동 토큰 생성 (부모 토큰과 연결)
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

            await UniTask.Yield(PlayerLoopTiming.Update, moveToken);  // 다음 프레임까지 이동
        }

        rb.linearVelocity = Vector2.zero;
        IsWalking = false;
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
        StopAILoop(); // 모든 비동기 작업 즉시 중단
        isActing = false;
        IsWalking = false;
       
        rb.linearVelocity = Vector2.zero; // 넉백 전에 속도 초기화

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

        // ... 기존 Die 로직 ...
        Debug.Log($"{monsterData.MonsterName}이(가) 죽었습니다.");

        // 물리적 움직임과 충돌을 중지
        rb.linearVelocity = Vector2.zero;
        GetComponentInChildren<Collider2D>().enabled = false; // 다른 오브젝트와 충돌하지 않도록

        // 죽음 애니메이션 재생
        anim.SetTrigger("die");

        // 예시: 2초 후에 오브젝트 파괴
        Destroy(gameObject, 2f);
    }

    protected override void Attack()
    {
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
            if (verticalVelocity < -3f)
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
        base.OnDrawGizmosSelected(); // 기본 인식/공격 범위 기즈모 그리기

        Gizmos.color = Color.green;
        Vector3 center = initialPosition + (Vector3)patrolAreaCenter - (Vector3)initialPosition; // 월드 좌표 보정
        Gizmos.DrawWireCube(center, patrolAreaSize);
    }
}