using UnityEngine;
using System.Collections;

public abstract class Monster : MonoBehaviour
{
    [Header("[Reference] Monster Data")]
    [SerializeField]
    protected MonsterData monsterData; // ������ ���� �����͸� ��� ScriptableObject

    protected float currentHP;
    protected float maxHP;
    protected float moveSpeed;
    protected float atk;
    protected float def;
    protected float recognitionRange;
    protected float attackRange;


    private Coroutine launchCoroutine; // ���� �ڷ�ƾ�� �����ϱ� ���� ����

    [Header("���� �޺� ����")]
    private int airHitCounter = 0;  // ���߿��� ���� Ƚ��
    [SerializeField] private float airHitDurationDecrease = 0.1f; // ���߿��� ���� ������ ������ ü�� �ð�

    protected bool isGrounded = true; // ���� �پ��ִ°�?
    public bool IsHurt { get; protected set; } = false; // �ǰ� ���� ����
    public bool IsAttacking { get; protected set; } = false;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected readonly AnimHashes animHashes = new();
    protected Transform playerTransform; // �ν����Ϳ� ������� ���� �Ҵ����
    protected Transform visualsTransform { get; private set; }
    private Vector3 startPos;

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

    // �� ���Ͱ� �������� �Ծ��� ��
    public virtual void TakeDamage(AttackDetails attackDetails, Vector2 attackPosition)
    {
        float damage = (attackDetails.damageRate - (this.def * 0.5f));
        damage = Mathf.RoundToInt(damage * Random.Range(0.9f, 1.1f));


        currentHP -= damage;
        Debug.Log($"{monsterData.MonsterName}��(��) {damage}�� �������� ����. ���� ü��: {currentHP}");


        // �ڡڡ� �˹� �� ���� ���� (�ڷ�ƾ ���) �ڡڡ�
        // 1. ���� �˹��� �״�� ����
        float direction = (transform.position.x > attackPosition.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * attackDetails.knockbackForce, 0);

        // 2. ���� �������� Ȯ��
        if (attackDetails.launchDuration > 0)
        {
            // �̹� �ٸ� ���� �ڷ�ƾ�� ���� ���̶�� ����
            if (launchCoroutine != null)
            {
                StopCoroutine(launchCoroutine);
            }
            // ���ο� ���� �ڷ�ƾ ����
            launchCoroutine = StartCoroutine(LaunchRoutine(attackDetails));
        }
        // 3. ���� �޺� ����
        if (!isGrounded) // �̹� ���߿� �� �ִ� ���¿��� �� �¾Ҵٸ�
        {
            airHitCounter++;
        }


        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            // �ǰ� �ִϸ��̼� ���, �˹� �� ���� �ǰ� ���� ����
            anim.SetTrigger("isHurt");
        }
    }

    // ���� �ڷ�ƾ
    protected virtual IEnumerator LaunchRoutine(AttackDetails attackDetails)
    {
        isGrounded = false;
        // ���� �ǰ� �ִϸ��̼� Ʈ���� (���� �ִٸ�)
        // anim.SetTrigger("isHurt_Air");

        // ���� �޺� ����: �������� ü�� �ð� ����
        float currentDuration = attackDetails.launchDuration - (airHitCounter * airHitDurationDecrease);
        if (currentDuration < 0.2f) currentDuration = 0.2f; // �ּ� ü�� �ð� ����

        // 3. �÷��̾�� ������ ������� ���� ����
        float elapsedTime = 0f;

        while (elapsedTime < currentDuration)
        {
            float progress = elapsedTime / currentDuration;
            float currentHeight = Mathf.Sin(progress * Mathf.PI) * attackDetails.launchHeight;

            visualsTransform.localPosition = new Vector3(startPos.x, currentHeight + startPos.y, startPos.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 4. ���� �Ϸ� ó��
        visualsTransform.localPosition = startPos; // ���� ���� ���󺹱�
        isGrounded = true;
        airHitCounter = 0; // ���� ������Ƿ� ���� �ǰ� Ƚ�� �ʱ�ȭ
        launchCoroutine = null;
    }
    public virtual void SetHurtState(bool isHurt)
    {
        this.IsHurt = isHurt;
        if (isHurt)
        {
            // �ʿ��ϴٸ� �ٸ� AI �ڷ�ƾ ���� ������ ���⿡ �߰�

            //rb.linearVelocity = Vector2.zero;
        }
        else
        {
            // �ǰ� ���°� ���� ���� ó��
        }
    }

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