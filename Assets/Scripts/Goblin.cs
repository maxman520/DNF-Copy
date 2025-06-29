using System.Collections;
using UnityEngine;

public class Goblin : Monster
{
    private enum AIPhase { Unaware, Aware } // 플레이어 비인식, 인식 상태
    private AIPhase currentPhase = AIPhase.Unaware; // 기본은 비인식 상태
    private Coroutine aiLoopCoroutine; // AILoop 코루틴을 제어하기 위한 변수

    [Header("AI 행동 패턴 설정")]
    [SerializeField] private float minActionInterval = 1.0f; // 최소 행동 결정 시간
    [SerializeField] private float maxActionInterval = 1.5f; // 최대 행동 결정 시간

    [Header("AI 순찰 설정")]
    [SerializeField] private Vector2 patrolAreaCenter; // 순찰 구역 중심 (로컬 좌표)
    [SerializeField] private Vector2 patrolAreaSize;   // 순찰 구역 크기
    private Vector3 initialPosition; // 몬스터의 초기 위치


    public bool IsGrounded = true; // 땅에 붙어있는가?
    public bool IsWalking = false;
    public bool IsHurt { get; protected set; } = false; // 피격 상태 변수
    public bool IsAttacking { get; protected set; } = false;


    // --- 공중 상태 제어 변수들 ---
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

        initialPosition = transform.position; // 초기 위치 저장
        patrolAreaCenter += (Vector2)initialPosition; // 순찰 중심점을 월드 좌표로 변환


        // AI 행동 루프 시작
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

        // 피격 또는 공격 중이 아니라면, AI의 의도를 반영한다.
        if (!IsHurt && !IsAttacking)
        {
            rb.linearVelocity = desiredVelocity;
        }
        else // 피격 또는 공격 중에는 모든 수평 움직임을 멈춘다.
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }


    // AI의 전체적인 흐름을 제어하는 메인 코루틴
    private IEnumerator AILoop()
    {
        while (currentHP > 0)
        {
            if (IsHurt)
            {
                yield return null; // 다음 프레임에 다시 IsHurt인지 검사
                continue;          // 아래 로직을 실행하지 않고 루프의 처음으로
            }

            Debug.Log($"<color=orange>--- AILoop 턴 시작 (현재 상태: {currentPhase}) ---</color>");
            switch (currentPhase)
            {
                case AIPhase.Unaware:
                    yield return StartCoroutine(UnawarePhase());
                    break;
                case AIPhase.Aware:
                    yield return StartCoroutine(AwarePhase());
                    break;
            }
            // ★★★ 턴이 끝난 후 잠깐의 딜레이를 줘서 로그 확인을 쉽게 함 ★★★
            yield return new WaitForSeconds(0.1f);
        }
        // 만약 루프가 끝났다면 왜 끝났는지 로그를 남김
        Debug.LogError($"AILoop 종료됨! (currentHP: {currentHP})");
    }

    // 비인식 상태의 행동 패턴
    private IEnumerator UnawarePhase()
    {
        // 인식 범위에 플레이어가 들어오면 Aware 상태로 전환
        if (IsPlayerInRecognitionRange())
        {
            currentPhase = AIPhase.Aware;
            yield break; // UnawarePhase 코루틴 즉시 종료
        }

        // 다음 행동 결정 (순찰 또는 대기)
        if (Random.value > 0.5f)
        {
            yield return StartCoroutine(UnawareWalk());
        }
        else
        {
            yield return StartCoroutine(UnawareIdle());
        }
    }

    // 인식 상태의 행동 패턴
    private IEnumerator AwarePhase()
    {
        // 행동 결정 전 잠시 대기
        float interval = Random.Range(minActionInterval, maxActionInterval);
        Debug.Log($"<color=cyan>AI 생각 중... {interval}초 후 행동 결정.</color>"); // 1. 생각 시작
        yield return new WaitForSeconds(interval);

        // 플레이어가 공격 범위 안에 있는가?
        if (IsPlayerInAttackRange())
        {
            Debug.Log("<color=red>플레이어가 공격 범위 안에 있음!</color>"); // 2. 공격 범위 감지
            // 공격 가능 시: 90% 확률로 공격, 10% 확률로 다른 행동(후퇴/대기)
            if (Random.value > 0.1f)
            {
                yield return StartCoroutine(AwareAttack());
            }
            else
            {
                Debug.Log("<color=yellow>후퇴/대기 결정.</color>");
                if (Random.value > 0.5f)
                    // 후퇴
                    yield return StartCoroutine(AwareRetreat());
                else
                    // 대기
                    yield return StartCoroutine(AwareIdle());
            }
        }
        else // 플레이어가 공격 범위 밖에 있을 때
        {
            // 접근, 후퇴, 대기 중 하나를 랜덤으로 선택
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
        yield return new WaitForSeconds(Random.Range(1f, 3f)); // 1~3초간 대기
    }

    private IEnumerator UnawareWalk()
    {
        // 순찰 영역 내 랜덤 목적지 설정
        Vector2 randomOffset = new Vector2(
            Random.Range(-patrolAreaSize.x / 2, patrolAreaSize.x / 2),
            Random.Range(-patrolAreaSize.y / 2, patrolAreaSize.y / 2)
        );
        Vector2 destination = patrolAreaCenter + randomOffset;

        IsWalking = true;
        while (Vector2.Distance(transform.position, destination) > 0.1f)
        {
            // 만약 순찰 중 플레이어를 발견하면 즉시 중단
            if (IsPlayerInRecognitionRange())
            {
                currentPhase = AIPhase.Aware;
                yield break;
            }

            Vector2 direction = (destination - (Vector2)transform.position).normalized;
            desiredVelocity = direction * moveSpeed;
            Flip(desiredVelocity.x);
            yield return null; // 다음 프레임까지 이동
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
        yield return null; // 행동 결정 주기까지 이 상태를 유지
    }

    private IEnumerator AwareWalk()
    {
        Debug.Log(">> 행동 실행: AwareWalk (추격)"); // 5. 실제 행동 실행 로그
        IsWalking = true;
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        desiredVelocity = direction * moveSpeed; // MonsterData의 moveSpeed 사용
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
        Attack(); // Monster 클래스의 Attack() 호출
        // 공격 애니메이션 시간만큼 대기 (AILoop의 대기시간이 쿨타임 역할을 함)
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

    // 개발 편의를 위해 순찰 영역을 씬 뷰에 표시
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // 기본 인식/공격 범위 기즈모 그리기

        Gizmos.color = Color.green;
        Vector3 center = initialPosition + (Vector3)patrolAreaCenter - (Vector3)initialPosition; // 월드 좌표 보정
        Gizmos.DrawWireCube(center, patrolAreaSize);
    }

    #endregion

    public override void Attack()
    {
        anim.SetTrigger("attack");
        Debug.Log("고블린의 공격");
    }

    protected override void Hurt(AttackDetails attackDetails, Vector2 attackPosition)
    {
        // AI의 의도(움직임)를 즉시 멈춘다.
        desiredVelocity = Vector2.zero;
        // AI 루프 자체를 잠시 멈추고 싶다면 여기에 로직 추가 가능

        // 수평 넉백은 desiredVelocity가 아닌, 직접적인 힘으로 즉시 적용
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
        else // 공중에 있을 때
        {
            airHitCounter++;
            visualYVelocity += attackDetails.airComboYVelocity;
            virtualGravity = initialVirtualGravity + (airHitCounter * gravityIncreaseFactor);
            anim.SetTrigger("isHurt");
        }
    }
    protected override void Die()
    {
        Debug.Log($"{monsterData.MonsterName}이(가) 죽었습니다.");

        // 죽었을 때 AI 코루틴을 확실히 중지
        if (aiLoopCoroutine != null)
        {
            StopCoroutine(aiLoopCoroutine);
            aiLoopCoroutine = null;
        }

        // 물리적 움직임과 충돌을 중지
        rb.linearVelocity = Vector2.zero;
        GetComponentInChildren<Collider2D>().enabled = false; // 다른 오브젝트와 충돌하지 않도록

        // 죽음 애니메이션 재생
        anim.SetTrigger("Die");

        // 예시: 2초 후에 오브젝트 파괴
        Destroy(gameObject, 2f);
    }
}