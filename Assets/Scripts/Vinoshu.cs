using UnityEngine;
using Random = UnityEngine.Random;
using Cysharp.Threading.Tasks; // UniTask
using System.Threading;       // CancellationToken
using System;

public class Vinoshu : Monster
{
    [Header("상태 변수")]
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
    private bool isDead // HP가 0이하로 떨어졌는가 (사망 로직 중복 실행 방지용)
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

    [Header("물리 변수")]
    private const float ORIGINAL_GRAVITY = 10f;
    public float verticalVelocity; // 현재 수직 속도. 에어본 시 y축 계산에 이용
    private float gravity = ORIGINAL_GRAVITY; // 가상 중력값
    private int airHitCounter = 0;

    [Header("공격 판정")]
    [SerializeField] private GameObject attackHitboxObject;
    private MonsterHitbox attackHitbox;

    [Header("AI 관련 변수")]
    protected bool isActing = false; // 현재 어떤 행동(Idle, Move 등)을 하고 있는지 여부
    [Tooltip("전투 시, 이동 가능한 가장 왼쪽 아래 경계를 나타내는 트랜스폼")]
    [SerializeField] private Transform combatMinBoundary;
    [Tooltip("전투 시, 이동 가능한 가장 오른쪽 위 경계를 나타내는 트랜스폼")]
    [SerializeField] private Transform combatMaxBoundary;

    [SerializeField] private GameObject meteorPrefab; // 비노슈가 소환할 메테오


    private CancellationTokenSource aiLoopCts; // 비동기 작업 관리

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

        // AI 루프 시작
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
        // 이전 CancellationTokenSource가 있다면. 실행중이던 AI Loop가 있다면 return
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

        // 물리적 이동 즉시 중단
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        IsWalking = false;
    }

    private async UniTask AI_Loop(CancellationToken token)
    {
        while (token.IsCancellationRequested == false)
        {
            await Pattern_Boss(token);

            // 매 프레임 실행되지 않고, 약간의 딜레이를 주어 성능 부하를 줄임
            await UniTask.Delay(100, cancellationToken: token);
        }
    }

    // --- 전투 패턴 ---
    private async UniTask Pattern_Boss(CancellationToken token)
    {
        if (isActing) return; // 이미 다른 행동중이면 실행하지 않음


        // 플레이어가 근접 공격 범위 안에 있을 경우
        if (IsPlayerInAttackRange(monsterData.attackDetails[0])) // 근접 공격 기준으로 판단
        {
            // 행동 결정용 변수
            float action = Random.value;

            // 근접 공격 (60% 확률)
            if (action > 0.4f)
            {
                await Attack(token);
            }
            else if (action < 0.1f) // 메테오 공격 (10% 확률)
            {
                await Meteor(token);

            }
            else
            {
                Vector3 destination; // 이동 목표 지점

                if (action > 0.25f) // 전진 (15% 확률)
                {
                    IsBackward = false;
                    // 플레이어 방향으로 이동
                    Vector3 direction = (playerTransform.position - transform.position).normalized;
                    destination = transform.position + direction * Random.Range(1f, 3f);
                }
                else // 후퇴 (15% 확률)
                {
                    IsBackward = true;
                    // 플레이어 반대 방향으로 이동
                    Vector3 direction = (playerTransform.position - transform.position).normalized * -1f;
                    destination = transform.position + direction * Random.Range(1f, 2f);
                }

                // 목표 지점을 정해진 전투 구역 (combatMinBoundary/combatMaxBoundary. ) 내로 보정
                if (combatMinBoundary != null && combatMaxBoundary != null)
                {
                    destination.x = Mathf.Clamp(destination.x, combatMinBoundary.position.x, combatMaxBoundary.position.x);
                    destination.y = Mathf.Clamp(destination.y, combatMinBoundary.position.y, combatMaxBoundary.position.y);
                }
                await MoveTo(destination, token);
            }
        }
        else // 자신의 근접 공격 범위 밖에 있으면
        {
            // 행동 결정용 변수
            float action = Random.value;
            Vector3 destination;

            // 메테오 공격 (10% 확률)
            if (action < 0.1f)
            {
                await Meteor(token);
            }
            else // 대기 / 전진 / 후퇴 (90% 확률)
            {
                action = Random.Range(0, 3);
                switch (action)
                {
                    case 0: // 잠시 1 ~ 2초 대기
                        float idleTime = Random.Range(1f, 2f);
                        await UniTask.Delay(System.TimeSpan.FromSeconds(idleTime), cancellationToken: token);
                        break;
                    case 1: // 플레이어에게 접근
                    case 2: // 플레이어에게서 후퇴
                        float directionFactor = -1f; // 접근이면 +, 후퇴면 - 방향
                        IsBackward = true;

                        if (action == 1) // 접근할 경우라면 상태 변수와 방향 수정
                        {
                            directionFactor = 1f;
                            IsBackward = false;
                        }

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
        }
    }

    protected async UniTask Attack(CancellationToken token)
    {
        isActing = true;
        try
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
            FlipTowardsPlayer();
            anim.SetTrigger("attack");
            Debug.Log("비노슈의 근접 공격!");

            // 다른 행동 진행을 막기 위해 attack 애니메이션 길이만큼 대기
            await UniTask.Delay(1100, cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            // 예외 처리
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
            // 인스펙터에서 설정해놓은 두 번째 공격 정보(메테오 공격 정보)를 가져옴
            Debug.Log("비노슈의 메테오 시전!");
            currentAttackDetails = monsterData.attackDetails[1];
            currentAttackDetails.damageRate *= this.atk; // 공격력이 곱해진 정보를 메테오에게 전달할 것임
            FlipTowardsPlayer();
            anim.SetTrigger("cast");

            // 마법진 생성. MagicCircle 이펙트의 visuals가 0.4f 만큼 밑으로 내려가있어 그만큼 offset을 넣어줘야함
            Vector3 targetPosition = new Vector3(playerTransform.position.x, playerTransform.position.y + 0.4f, playerTransform.position.z);
            EffectManager.Instance.PlayEffect("MagicCircle", targetPosition, Quaternion.identity);

            // 다른 행동 진행을 막기 위해 cast 애니메이션의 길이만큼 대기
            await UniTask.Delay(1100, cancellationToken: token);

            targetPosition.y -= 0.4f; // 정확한 타겟 위치를 구하기 위해 위에서 더해줬던 0.4를 다시 빼줌

            Debug.Log("메테오 소환!");
            if (meteorPrefab != null)
            {
                GameObject meteorInstance = Instantiate(meteorPrefab, targetPosition, Quaternion.identity);

                // 메테오가 타겟을 향하도록 현재 공격 정보와 함께 초기화
                meteorInstance.GetComponent<VinoshuMeteor>().Initialize(currentAttackDetails, targetPosition);
            }

        }
        catch (OperationCanceledException)
        {
            // 예외 처리
        }
        finally
        {
            isActing = false;
        }        
    }
    // 목표 지점까지 이동
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
                if (rb != null)
                    rb.linearVelocity = direction * moveSpeed;

                FlipTowardsPlayer();

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException)
        {
            // 예외 처리
        }
        finally
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            IsWalking = false;
            isActing = false;
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
        // Do nothing
    }

    // 공격 애니메이션이 끝났을 때 호출
    public override void OnAttackStateExit()
    {
        isActing = false;
    }

    // 피격 애니메이션이 끝났을 때 호출
    public override void OnHurtStateExit() {
        // 피격이 끝나면 전투 상태로 복귀하고 AI 루프 재시작
        isActing = false;
    }

    // 기상 애니메이션이 끝났을 때 호출
    public override void OnGetUpStateExit()
    {
        // 피격이 끝나면 전투 상태로 복귀하고 AI 루프 재시작
        isActing = false;
        visualsTransform.localPosition = startPos;
    }
    #endregion State Behaviour

    #region Utilities

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
            GameManager.Instance.DoSlowMotion(3f, 0.2f); // 1.5초 동안, 게임 속도를 20%로
        }

        // UIManager에 자신을 타겟으로 알림
        UIManager.Instance.OnMonsterDamaged(this);

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
        StopAILoop(); // 비동기 작업 즉시 중단
        isActing = false;
        IsWalking = false;
       
        
        if (rb != null)
            rb.linearVelocity = Vector2.zero; // 넉백 전에 속도 초기화

        // hurtbox 지점에 이펙트를 생성
        Vector3 hurtPoint = hurtboxTransform.position;

        // 이펙트의 방향은 플레이어가 바라보는 방향을 따르거나, 기본 방향으로 설정
        Quaternion effectRotation = (transform.position.x > attackPosition.x) ? Quaternion.identity : Quaternion.Euler(0, 180, 0);

        // attackDetails에 이펙트 이름이 있다면 그걸 사용, 없다면 기본 이펙트 사용
        // string effectToPlay = string.IsNullOrEmpty(attackDetails.effectName) ? "NormalHit_Slash" : attackDetails.effectName;
        string effectToPlay = "SlashSmall" + Random.Range(1, 4);
        EffectManager.Instance.PlayEffect(effectToPlay, hurtPoint, Quaternion.identity);
        EffectManager.Instance.PlayEffect("BloodLarge", hurtPoint, effectRotation);


        float direction = (transform.position.x > attackPosition.x) ? 1 : -1;

        if (IsGrounded) // 땅에 있을 때
        {
            
            if (attackDetails.launchForce > 0)
            {
                // 수평 넉백
                if (rb != null)
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

                anim.SetTrigger("hurt" + Random.Range(1, 3));
            }
        }
        else // 공중에 있을 때
        {   
            // 수평 넉백
            if (rb != null)
                rb.AddForce(new Vector2(direction * attackDetails.knockbackForce, 0), ForceMode2D.Impulse);

            if (attackDetails.launchForce > 0) airHitCounter++;
            verticalVelocity = 2f + (attackDetails.launchForce * Mathf.Max(0, 0.5f - (airHitCounter * 0.125f)));
            gravity += 0.05f;
        }
    }

    protected override async void Die()
    {
        StopAILoop(); // 모든 비동기 작업 중단
        isActing = true;
        UIManager.Instance.HideBossHPBar(); // HP바를 숨기도록 요청

        Debug.Log($"{monsterData.MonsterName}이(가) 죽었습니다.");
        
        // 플레이어에게 경험치 지급
        GameManager.Instance.AddExp(monsterData.EXP);
        GameManager.Instance.AddHuntExp(monsterData.EXP);

        // 동일 방 내 다른 몬스터도 즉시 처치 (경험치 포함)
        ForceKillOtherMonstersInRoom();

        // 물리적 움직임과 충돌을 중지
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        GetComponentInChildren<Collider2D>().enabled = false;

        var token = this.GetCancellationTokenOnDestroy();
        await DeathSequenceAsync(token);
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
        EffectManager.Instance.PlayEffect("MonsterDieFlash", hurtPoint, Quaternion.identity);
        
        sr.enabled = false;

        // 3. 던전 결과 창 표시 요청
        Debug.Log("던전 결과 창 표시를 요청");
        GameManager.Instance.ShowResultPanel();

        // 4. 최종 오브젝트 파괴
        await UniTask.Delay(System.TimeSpan.FromSeconds(1.0), cancellationToken: token);
        Destroy(gameObject);
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
        if (visualsTransform.localPosition.y <= startPos.y - 0.25f) // 비노슈는 땅에서 살짝 떠있는 채로 움직이는 몬스터기 때문에 0.25만큼 y값을 빼주어야 땅에 제대로 착지한 듯이 보임
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
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            // OnGetUpStateExit()에서 visualsTransform을 원 위치로 복구하도록 로직 옮김
            // visualsTransform.localPosition = startPos;
            verticalVelocity = 0f;
            gravity = ORIGINAL_GRAVITY;
        }
    }
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // 기본 인식/공격 범위 기즈모 그리기

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
