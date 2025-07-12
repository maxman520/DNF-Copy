using UnityEngine;
using System.Collections.Generic; // ���� ����� ����ϱ� ���� �ʿ�

public class PlayerHitbox : MonoBehaviour
{
    public AttackDetails attackDetails;

    // �� ������ y�� ���� ������ �� ��ǥ. �ʱⰪ�� float.MinValue�� �����Ͽ�, �ʱ�ȭ ���θ� ��Ȯ�ϰ� �Ǵ�
    private float originY = float.MinValue;
    private Player player;

    // �� ���� ���� ��ǿ��� ������ ���� ���� �� ������ �ʵ��� ����ϴ� ����Ʈ
    private List<Collider2D> alreadyHit = new List<Collider2D>();

    // ��Ʈ�ڽ��� �ʱ�ȭ
    public void Initialize(AttackDetails details, float originY)
    {
        this.attackDetails = details;
        this.originY = originY;
        this.alreadyHit.Clear();// ���ο� ������ ���۵� ������, ������ ���ȴ� �� ����� �ʱ�ȭ
    }

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }
    private void OnEnable()
    {
        // ����� ��츦 ����� �ʱ�ȭ
        alreadyHit.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("MonsterHurtbox"))
        {
            Monster monster = other.GetComponentInParent<Monster>();
            if (monster == null) return;

            // originY�� �ʱⰪ(MinValue) �״�ζ��, ��Ʈ�ڽ� �ڽ��� Y ��ġ�� �������� ���.
            this.originY = (originY == float.MinValue) ? this.transform.position.y : this.originY;

            // �÷��̾� ������ Y�� ���� üũ
            if (Mathf.Abs(this.originY - monster.transform.position.y) > attackDetails.yOffset)
                return;

            // �̹� ���ݿ��� �̹� ���� ���̸� ����
            if (alreadyHit.Contains(other))
                return;

            // ���� ������ ���
            alreadyHit.Add(other);


            if (monster != null)
            {
                // ���� ������ ������ ������ �÷��̾��� �⺻ ���ݷ��� ���ؼ� ����
                AttackDetails finalAttackDetails = attackDetails;
                finalAttackDetails.damageRate *= player.Atk;


                monster.OnDamaged(finalAttackDetails, transform.position); // ��Ʈ�ڽ��� ��ġ ���� ����
                Debug.Log($"{monster.name}���� �������� ����!");
            }
        }
    }
}