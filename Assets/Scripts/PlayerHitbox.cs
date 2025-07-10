using UnityEngine;
using System.Collections.Generic; // ���� ����� ����ϱ� ���� �ʿ�

public class PlayerHitbox : MonoBehaviour
{
    public AttackDetails attackDetails;
    private Player player;

    // �� ���� ���� ��ǿ��� ������ ���� ���� �� ������ �ʵ��� ����ϴ� ����Ʈ
    private List<Collider2D> alreadyHit = new List<Collider2D>();

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }
    private void OnEnable()
    {
        // ���ο� ������ ���۵� ������, ������ ���ȴ� �� ����� �ʱ�ȭ
        alreadyHit.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("MonsterHurtbox"))
        {
            Monster monster = other.GetComponentInParent<Monster>();
            if (monster == null) return;

            // �÷��̾� ������ Y�� ���� üũ
            if (Mathf.Abs(this.transform.position.y - monster.transform.position.y) > attackDetails.yOffset)
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

                monster.OnDamaged(finalAttackDetails, transform.position); // ��Ʈ�ڽ��� ���� ��ġ�� ����
                Debug.Log($"{monster.name}���� �������� ����!");
            }
        }
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ���� �Լ�
    public void SetComboAttackDetails(int index)
    {
        attackDetails = player.AttackDetails[index];
    }
}