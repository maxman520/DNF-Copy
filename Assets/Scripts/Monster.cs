using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    [Header("������ ����")]
    [SerializeField] protected MonsterData monsterData; // ������ ���� �����͸� ��� ScriptableObject

    [Header("�ǽð� ����")]
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

    [Header("������Ʈ ����")]
    protected Rigidbody2D rb;
    protected Animator anim;
    protected Transform visualsTransform;
    protected Transform hurtboxTransform;
    protected Transform hitboxTransform;
    protected Transform playerTransform;

    protected Vector3 startPos; // ���� visuals ��ġ �����

    [Header("���� ����")]
    public AttackDetails currentAttackDetails;


    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        visualsTransform = transform.Find("Visuals");
        hurtboxTransform = visualsTransform.Find("Hurtbox");
        hitboxTransform = visualsTransform.Find("Hitbox");
        startPos = visualsTransform.localPosition;

        // ������ �ʱ�ȭ
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

    // �������� �Ծ��� ��
    public abstract void OnDamaged(AttackDetails attackDetails, Vector2 attackPosition);

    // ������ ��� ����
    protected virtual float CalculateDamage(AttackDetails attackDetails)
    {
        // !! ������ ������ �÷��̾��� ���ݷ��� �̹� ���������� !!
        float finalDamage = (attackDetails.damageRate) - (def * 0.5f);
        finalDamage = Mathf.Max(1, Mathf.RoundToInt(finalDamage * Random.Range(0.8f, 1.2f)));

        return finalDamage;
    }

    protected abstract void Hurt(AttackDetails attackDetails, Vector2 attackPosition);
    protected abstract void Die();
    protected abstract void Attack();

    // ��� �ִϸ��̼����� ���� �� ȣ��
    public abstract void OnIdleStateEnter();
    // �ȱ� �ִϸ��̼��� ������ �� ȣ��
    public abstract void OnWalkStateExit();
    // ���� �ִϸ��̼��� ������ �� ȣ��
    public abstract void OnAttackStateExit();
    // �ǰ� �ִϸ��̼��� ������ �� ȣ��
    public abstract void OnHurtStateExit();
    // ��� �ִϸ��̼��� ������ �� ȣ��
    public abstract void OnGetUpStateExit();


    // �����Ϳ����� ���̴� �����(Gizmo)�� �׷��� AI ������ �ð������� Ȯ��
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; // �ν� ������ �����
        Gizmos.DrawWireSphere(transform.position, recognitionRange);

        Gizmos.color = Color.red; // ���� ������ ������
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}