using UnityEngine;
using System.Collections;

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
    public float GetAtk()
    {
        return this.atk;
    }

    // 이 몬스터가 데미지를 입었을 때
    public virtual void TakeDamage(AttackDetails attackDetails, Vector2 attackPosition)
    {
        // 이미 죽었거나 무적 상태일 때를 대비한 가드
        if (currentHP <= 0) return;

        // 1. 데미지 계산 및 체력 적용
        ApplyDamage(attackDetails);

        // 2. 피격 반응은 자식에게 완전히 위임
        Hurt(attackDetails, attackPosition);

        // 3. 사망 처리
        if (currentHP <= 0)
        {
            Die();
        }
    }

    // 데미지 계산 로직
    protected virtual void ApplyDamage(AttackDetails attackDetails)
    {
        float finalDamage = (attackDetails.damageRate * Player.Instance.Atk) - (monsterData.Def * 0.5f);
        finalDamage = Mathf.Max(1, Mathf.RoundToInt(finalDamage * Random.Range(0.9f, 1.1f)));
        currentHP -= finalDamage;
        Debug.Log($"{monsterData.MonsterName}이(가) {finalDamage}의 데미지를 입음. 현재 체력: {currentHP}");
    }

    protected abstract void Hurt(AttackDetails attackDetails, Vector2 attackPosition);
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