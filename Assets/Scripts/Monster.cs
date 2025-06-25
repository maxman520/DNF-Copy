using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    [Header("[Reference] Monster Data")]
    [SerializeField]
    private MonsterData monsterData; // 몬스터의 원본 데이터를 담는 ScriptableObject

    protected float currentHP;
    protected float maxHP;
    protected float moveSpeed;
    protected float atk;
    protected float recognitionRange;
    protected float attackRange;

    protected Rigidbody2D rb;
    protected Animator anim;
    protected readonly AnimHashes animHashes = new();
    protected Transform playerTransform; // 인스펙터에 노출시켜 직접 할당받음

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();

        // 데이터 초기화
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

    // 이 몬스터가 데미지를 입었을 때 호출될 공통 함수
    public virtual void TakeDamage(float damage)
    {
        currentHP -= damage;
        Debug.Log($"{monsterData.MonsterName}이(가) {damage}의 데미지를 입음. 현재 체력: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            // 피격 애니메이션 재생, 넉백 등 공통 피격 반응 로직
            anim.SetTrigger("Hurt");
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{monsterData.MonsterName}이(가) 죽었습니다.");

        // 여기에 죽음 애니메이션, 아이템 드랍, 경험치 제공 등의 로직을 추가

        // 예시: 2초 후에 오브젝트 파괴
        Destroy(gameObject, 2f);
    }

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