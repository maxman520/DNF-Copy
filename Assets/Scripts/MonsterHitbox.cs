using System.Threading;
using UnityEngine;

public class MonsterHitbox : MonoBehaviour
{
    // 이 Hitbox의 주인
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

            // 몬스터 공격의 Y축 범위 체크
            if (Mathf.Abs(this.transform.position.y - player.transform.position.y) >= attackDetails.yOffset)
                return;

            if (player != null)
            {
                // 공격 정보의 데미지 배율에 몬스터의 기본 공격력을 곱해서 전달
                attackDetails.damageRate *= ownerMonster.GetAtk();

                player.OnDamaged(attackDetails, transform.position);
            }
        }

    }
}