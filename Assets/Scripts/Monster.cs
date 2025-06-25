using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    [Header("[Reference] Monster Data")]
    [SerializeField]
    private MonsterData monsterData; // ������ ���� �����͸� ��� ScriptableObject

    protected float currentHP;
    protected float maxHP;
    protected float moveSpeed;
    protected float atk;
    protected float recognitionRange;
    protected float attackRange;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected readonly AnimHashes animHashes = new();
    protected Transform playerTransform; // �ν����Ϳ� ������� ���� �Ҵ����

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();

        // ������ �ʱ�ȭ
        if (monsterData != null)
        {
            currentHP = monsterData.MaxHP;
            moveSpeed = monsterData.MoveSpeed;
            atk = monsterData.Atk;
            recognitionRange = monsterData.RecognitionRange;
            attackRange = monsterData.AttackRange;
        }
        else
        {
            Debug.LogError($"{gameObject.name}: MonsterData�� �Ҵ���� �ʾ���");
        }

    }
    protected virtual void Start()
    {
        // ���� �÷��̾ ã�Ƽ� playerTransform�� �Ҵ��ϴ� ������ ���⿡ �߰��� �� �ִ�.
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

    // �� ���Ͱ� �������� �Ծ��� �� ȣ��� ���� �Լ�
    public virtual void TakeDamage(float damage)
    {
        currentHP -= damage;
        Debug.Log($"{monsterData.MonsterName}��(��) {damage}�� �������� ����. ���� ü��: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            // �ǰ� �ִϸ��̼� ���, �˹� �� ���� �ǰ� ���� ����
            anim.SetTrigger("Hurt");
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{monsterData.MonsterName}��(��) �׾����ϴ�.");

        // ���⿡ ���� �ִϸ��̼�, ������ ���, ����ġ ���� ���� ������ �߰�

        // ����: 2�� �Ŀ� ������Ʈ �ı�
        Destroy(gameObject, 2f);
    }

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