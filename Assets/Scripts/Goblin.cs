using System.Collections;
using UnityEngine;

public class Goblin : Monster
{
    private enum AIPhase { Unaware, Aware } // 플레이어 비인식, 인식 상태
    private AIPhase currentPhase = AIPhase.Unaware; // 기본은 비인식 상태

    private Coroutine currentActionCoroutine;

    [Header("AI 행동 패턴 설정")]
    [SerializeField] private float minActionInterval = 1.0f; // 최소 행동 결정 시간
    [SerializeField] private float maxActionInterval = 1.5f; // 최대 행동 결정 시간

    [Header("AI 순찰 설정")]
    [SerializeField] private Vector2 patrolAreaCenter; // 순찰 구역 중심 (로컬 좌표)
    [SerializeField] private Vector2 patrolAreaSize;   // 순찰 구역 크기
    private Vector3 initialPosition; // 몬스터의 초기 위치

    protected override void Start()
    {
        base.Start();
        initialPosition = transform.position; // 초기 위치 저장
        patrolAreaCenter += (Vector2)initialPosition; // 순찰 중심점을 월드 좌표로 변환


        // AI 행동 루프 시작
        StartCoroutine(AILoop());
    }

    // AI의 전체적인 흐름을 제어하는 메인 코루틴
    private IEnumerator AILoop()
    {
        while (currentHP > 0)
        {
            // ★★★ AILoop이 매 턴 살아있는지 확인하는 로그 ★★★
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
            // 공격 가능 시: 70% 확률로 공격, 30% 확률로 다른 행동(후퇴/대기)
            if (Random.value > 0.3f)
            {
                yield return StartCoroutine(AwareAttack());
            }
            else
            {
                Debug.Log("<color=yellow>플레이어가 공격 범위 밖에 있음. 추격/후퇴/대기 결정.</color>"); // 3. 공격 범위 밖
                // 다른 행동 선택 (후퇴 또는 대기)
                if (Random.value > 0.5f)
                    yield return StartCoroutine(AwareRetreat());
                else
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
        rb.linearVelocity = Vector2.zero;
        anim.SetBool(animHashes.IsWalking, false);
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

        anim.SetBool(animHashes.IsWalking, true);
        while (Vector2.Distance(transform.position, destination) > 0.1f)
        {
            // 만약 순찰 중 플레이어를 발견하면 즉시 중단
            if (IsPlayerInRecognitionRange())
            {
                currentPhase = AIPhase.Aware;
                yield break;
            }

            Vector2 direction = (destination - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
            Flip(rb.linearVelocity.x);
            yield return null; // 다음 프레임까지 이동
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
        yield return null; // 행동 결정 주기까지 이 상태를 유지
    }

    private IEnumerator AwareWalk()
    {
        Debug.Log(">> 행동 실행: AwareWalk (추격)"); // 5. 실제 행동 실행 로그
        anim.SetBool(animHashes.IsWalking, true);
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed; // MonsterData의 moveSpeed 사용
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

    public override void Attack()
    {
        anim.SetTrigger(animHashes.Attack);
        Debug.Log("고블린의 공격");
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
}