using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    [Header("데이터 참조")]
    [SerializeField] protected MonsterData monsterData; // 몬스터의 원본 데이터를 담는 ScriptableObject

    [Header("실시간 스탯")]
    protected float maxHP;
    protected float previousHP;
    protected float currentHP;
    protected float hpPerLine;
    protected float moveSpeed;
    protected float atk;
    protected float def;
    protected float recognitionRange;
    protected float attackRange;

    public MonsterData GetMonsterData() => monsterData;
    public float GetMaxHP() => maxHP;
    public float GetPreviousHP() => previousHP;
    public float GetCurrentHP() => currentHP;
    public float GetHpPerLine() => hpPerLine;
    public float GetAtk() => atk;

    [Header("컴포넌트 참조")]
    protected Rigidbody2D rb;
    protected Animator anim;
    protected Transform visualsTransform;
    protected Transform hurtboxTransform;
    protected Transform hitboxTransform;
    protected Transform playerTransform;

    protected Vector3 startPos; // 내부 visuals 위치 제어용

    [Header("공격 정보")]
    public AttackDetails currentAttackDetails;


    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        visualsTransform = transform.Find("Visuals");
        hurtboxTransform = visualsTransform.Find("Hurtbox");
        hitboxTransform = visualsTransform.Find("Hitbox");
        startPos = visualsTransform.localPosition;

        // 데이터 초기화
        maxHP = monsterData.MaxHP;
        currentHP = monsterData.MaxHP;
        previousHP = monsterData.MaxHP;
        hpPerLine = monsterData.HpPerLine;
        moveSpeed = monsterData.MoveSpeed;
        atk = monsterData.Atk;
        def = monsterData.Def;
        recognitionRange = monsterData.RecognitionRange;
        attackRange = monsterData.AttackRange;

    }

    // 데미지를 입었을 때
    public abstract void OnDamaged(AttackDetails attackDetails, Vector2 attackPosition);

    // 데미지 계산 로직
    protected virtual float CalculateDamage(AttackDetails attackDetails)
    {
        // !! 데미지 배율에 플레이어의 공격력이 이미 곱해져있음 !!
        float finalDamage = (attackDetails.damageRate) - (def * 0.5f);
        finalDamage = Mathf.Max(1, Mathf.RoundToInt(finalDamage * Random.Range(0.8f, 1.2f)));

        return finalDamage;
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