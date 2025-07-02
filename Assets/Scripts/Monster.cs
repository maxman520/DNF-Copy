using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    [Header("데이터 참조")]
    [SerializeField] protected MonsterData monsterData; // 몬스터의 원본 데이터를 담는 ScriptableObject

    [Header("실시간 스탯")]
    protected float currentHP;
    protected float maxHP;
    protected float moveSpeed;
    protected float atk;
    protected float def;
    protected float recognitionRange;
    protected float attackRange;

    [Header("컴포넌트 참조")]
    protected Rigidbody2D rb;
    protected Animator anim;
    protected Transform visualsTransform;
    protected Transform playerTransform;

    protected Vector3 startPos; // 내부 visuals 위치 제어용


    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        visualsTransform = transform.Find("Visuals");
        startPos = visualsTransform.localPosition;

        // 데이터 초기화
        currentHP = monsterData.MaxHP;
        moveSpeed = monsterData.MoveSpeed;
        atk = monsterData.Atk;
        def = monsterData.Def;
        recognitionRange = monsterData.RecognitionRange;
        attackRange = monsterData.AttackRange;

    }
    public float GetAtk()
    {
        return this.atk;
    }

    // 데미지를 입었을 때
    public virtual void OnDamaged(AttackDetails attackDetails, Vector2 attackPosition)
    {
        // 피격 반응은 자식에게위임
        Hurt(attackDetails, attackPosition);

        // 이미 죽었거나 무적 상태일 때를 대비한 가드
        if (currentHP <= 0) return;

        // 입을 데미지 계산
        CalculateDamage(attackDetails);

    }

    // 데미지 계산 로직
    protected virtual void CalculateDamage(AttackDetails attackDetails)
    {
        // !! 데미지 배율에 플레이어의 공격력이 이미 곱해져있음 !!
        float finalDamage = (attackDetails.damageRate) - (def * 0.5f);
        finalDamage = Mathf.Max(1, Mathf.RoundToInt(finalDamage * Random.Range(0.8f, 1.2f)));
        currentHP -= finalDamage;
        Debug.Log($"{monsterData.MonsterName}이(가) {finalDamage}의 데미지를 입음. 현재 체력: {currentHP}");
    }

    protected abstract void Hurt(AttackDetails attackDetails, Vector2 attackPosition);
    protected abstract void Die();
    protected abstract void Attack();

    // 대기 애니메이션으로 진입 시 호출
    public abstract void OnIdleStateEnter();
    // 걷기 애니메이션이 끝났을 때 호출
    public abstract void OnWalkStateExit();
    // 공격 애니메이션이 끝났을 때 호출
    public abstract void OnAttackStateExit();
    // 피격 애니메이션이 끝났을 때 호출
    public abstract void OnHurtStateExit();
    // 기상 애니메이션이 끝났을 때 호출
    public abstract void OnGetUpStateExit();


    // 에디터에서만 보이는 기즈모(Gizmo)를 그려서 AI 범위를 시각적으로 확인
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; // 인식 범위는 노란색
        Gizmos.DrawWireSphere(transform.position, recognitionRange);

        Gizmos.color = Color.red; // 공격 범위는 빨간색
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}