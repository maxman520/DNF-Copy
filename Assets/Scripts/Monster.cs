using UnityEngine;
using System.Collections;

public abstract class Monster : MonoBehaviour
{
    [Header("������ ����")]
    [SerializeField] protected MonsterData monsterData; // ������ ���� �����͸� ��� ScriptableObject

    [Header("�ǽð� ����")]
    protected float currentHP;
    protected float maxHP;
    protected float moveSpeed;
    protected float atk;
    protected float def;
    protected float recognitionRange;
    protected float attackRange;

    [Header("������Ʈ ����")]
    protected Rigidbody2D rb;
    protected Animator anim;
    protected Transform visualsTransform;
    protected Transform playerTransform;

    protected Vector3 startPos; // ���� visuals ��ġ �����

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        visualsTransform = transform.Find("Visuals");
        startPos = visualsTransform.localPosition;

        // ������ �ʱ�ȭ
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
            Debug.LogError($"{gameObject.name}: MonsterData�� �Ҵ���� �ʾ���");
        }

    }
    public float GetAtk()
    {
        return this.atk;
    }

    // �� ���Ͱ� �������� �Ծ��� ��
    public virtual void TakeDamage(AttackDetails attackDetails, Vector2 attackPosition)
    {
        // �̹� �׾��ų� ���� ������ ���� ����� ����
        if (currentHP <= 0) return;

        // 1. ������ ��� �� ü�� ����
        ApplyDamage(attackDetails);

        // 2. �ǰ� ������ �ڽĿ��� ������ ����
        Hurt(attackDetails, attackPosition);

        // 3. ��� ó��
        if (currentHP <= 0)
        {
            Die();
        }
    }

    // ������ ��� ����
    protected virtual void ApplyDamage(AttackDetails attackDetails)
    {
        float finalDamage = (attackDetails.damageRate * Player.Instance.Atk) - (monsterData.Def * 0.5f);
        finalDamage = Mathf.Max(1, Mathf.RoundToInt(finalDamage * Random.Range(0.9f, 1.1f)));
        currentHP -= finalDamage;
        Debug.Log($"{monsterData.MonsterName}��(��) {finalDamage}�� �������� ����. ���� ü��: {currentHP}");
    }

    protected abstract void Hurt(AttackDetails attackDetails, Vector2 attackPosition);
    protected abstract void Die();
    public abstract void Attack();

    // �����Ϳ����� ���̴� �����(Gizmo)�� �׷��� AI ������ �ð������� Ȯ��
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; // �ν� ������ �����
        Gizmos.DrawWireSphere(transform.position, recognitionRange);

        Gizmos.color = Color.red; // ���� ������ ������
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}