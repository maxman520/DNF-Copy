using System.Threading;
using UnityEngine;

public class MonsterHitbox : MonoBehaviour
{
    // �� Hitbox�� ����
    private Monster ownerMonster;
    private Player player;

    private void Awake()
    {
        ownerMonster = GetComponentInParent<Monster>();
        player = Player.Instance;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (ownerMonster == null) return;

            AttackDetails attackDetails = ownerMonster.currentAttackDetails;

            // ���� ������ Y�� ���� üũ
            if (Mathf.Abs(this.transform.position.y - player.transform.position.y) >= attackDetails.yOffset)
                return;

            if (player != null)
            {
                // ���� ������ ������ ������ ������ �⺻ ���ݷ��� ���ؼ� ����
                attackDetails.damageRate *= ownerMonster.GetAtk();

                player.OnDamaged(attackDetails, transform.position);
            }
        }

    }
}