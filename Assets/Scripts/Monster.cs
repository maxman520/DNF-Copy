using UnityEngine;
using System.Collections;

public abstract class Monster : MonoBehaviour
{
    [Header("[Reference] Monster Data")]
    [SerializeField]
    protected MonsterData monsterData; // 몬스터의 원본 데이터를 담는 ScriptableObject

    protected float currentHP;
    protected float maxHP;
    protected float moveSpeed;
    protected float atk;
    protected float def;
    protected float recognitionRange;
    protected float attackRange;


    private Coroutine launchCoroutine; // 띄우기 코루틴을 제어하기 위한 변수

    [Header("공중 콤보 관련")]
    private int airHitCounter = 0;  // 공중에서 맞은 횟수
    [SerializeField] private float airHitDurationDecrease = 0.1f; // 공중에서 맞을 때마다 감소할 체공 시간

    protected bool isGrounded = true; // 땅에 붙어있는가?
    public bool IsHurt { get; protected set; } = false; // 피격 상태 변수
    public bool IsAttacking { get; protected set; } = false;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected readonly AnimHashes animHashes = new();
    protected Transform playerTransform; // 인스펙터에 노출시켜 직접 할당받음
    protected Transform visualsTransform { get; private set; }
    private Vector3 startPos;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        visualsTransform = transform.Find("Visuals");
        startPos = visualsTransform.localPosition;

        // 데이터 초기화
        if (monsterData != null)
        {
            currentHP = monsterData.MaxHP;
            moveSpeed = monsterData.MoveSpeed;
            atk = monsterData.Atk;
            def = monsterData.Def;
            recognitionRange = monsterData.RecognitionRange;
            attackRange = monsterData.AttackRange;
        }
        else
        {
            Debug.LogError($"{gameObject.name}: MonsterData가 할당되지 않았음");
        }

    }
    protected virtual void Start()
    {
        // 추후 플레이어를 찾아서 playerTransform에 할당하는 로직을 여기에 추가할 수 있다.
        // GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        // if (playerObject != null)
        // {
        //     playerTransform = playerObject.transform;
        // }
        if (Player.Instance != null)
        {
            playerTransform = Player.Instance.transform;
        }
    }

    public float GetAtk()
    {
        return this.atk;
    }

    // 이 몬스터가 데미지를 입었을 때
    public virtual void TakeDamage(AttackDetails attackDetails, Vector2 attackPosition)
    {
        float damage = (attackDetails.damageRate - (this.def * 0.5f));
        damage = Mathf.RoundToInt(damage * Random.Range(0.9f, 1.1f));


        currentHP -= damage;
        Debug.Log($"{monsterData.MonsterName}이(가) {damage}의 데미지를 입음. 현재 체력: {currentHP}");


        // ★★★ 넉백 및 띄우기 로직 (코루틴 기반) ★★★
        // 1. 수평 넉백은 그대로 적용
        float direction = (transform.position.x > attackPosition.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * attackDetails.knockbackForce, 0);

        // 2. 띄우기 공격인지 확인
        if (attackDetails.launchDuration > 0)
        {
            // 이미 다른 띄우기 코루틴이 실행 중이라면 중지
            if (launchCoroutine != null)
            {
                StopCoroutine(launchCoroutine);
            }
            // 새로운 띄우기 코루틴 시작
            launchCoroutine = StartCoroutine(LaunchRoutine(attackDetails));
        }
        // 3. 무한 콤보 방지
        if (!isGrounded) // 이미 공중에 떠 있는 상태에서 또 맞았다면
        {
            airHitCounter++;
        }


        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            // 피격 애니메이션 재생, 넉백 등 공통 피격 반응 로직
            anim.SetTrigger("isHurt");
        }
    }

    // 띄우기 코루틴
    protected virtual IEnumerator LaunchRoutine(AttackDetails attackDetails)
    {
        isGrounded = false;
        // 공중 피격 애니메이션 트리거 (만약 있다면)
        // anim.SetTrigger("isHurt_Air");

        // 무한 콤보 방지: 맞을수록 체공 시간 감소
        float currentDuration = attackDetails.launchDuration - (airHitCounter * airHitDurationDecrease);
        if (currentDuration < 0.2f) currentDuration = 0.2f; // 최소 체공 시간 보장

        // 3. 플레이어와 동일한 방식으로 띄우기 실행
        float elapsedTime = 0f;

        while (elapsedTime < currentDuration)
        {
            float progress = elapsedTime / currentDuration;
            float currentHeight = Mathf.Sin(progress * Mathf.PI) * attackDetails.launchHeight;

            visualsTransform.localPosition = new Vector3(startPos.x, currentHeight + startPos.y, startPos.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 4. 띄우기 완료 처리
        visualsTransform.localPosition = startPos; // 원래 높이 원상복구
        isGrounded = true;
        airHitCounter = 0; // 땅에 닿았으므로 공중 피격 횟수 초기화
        launchCoroutine = null;
    }
    public virtual void SetHurtState(bool isHurt)
    {
        this.IsHurt = isHurt;
        if (isHurt)
        {
            // 필요하다면 다른 AI 코루틴 중지 로직을 여기에 추가

            //rb.linearVelocity = Vector2.zero;
        }
        else
        {
            // 피격 상태가 끝날 때의 처리
        }
    }

    protected abstract void Die();

    public abstract void Attack();

    // 에디터에서만 보이는 기즈모(Gizmo)를 그려서 AI 범위를 시각적으로 확인
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; // 인식 범위는 노란색
        Gizmos.DrawWireSphere(transform.position, recognitionRange);

        Gizmos.color = Color.red; // 공격 범위는 빨간색
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}