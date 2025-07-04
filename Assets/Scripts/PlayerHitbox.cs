using UnityEngine;
using System.Collections.Generic; // 여러 대상을 기억하기 위해 필요

public class PlayerHitbox : MonoBehaviour
{
    public AttackDetails attackDetails;
    private Player player;

    // 한 번의 공격 모션에서 동일한 적을 여러 번 때리지 않도록 기억하는 리스트
    private List<Collider2D> alreadyHit = new List<Collider2D>();

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }
    private void OnEnable()
    {
        // 새로운 공격이 시작될 때마다, 이전에 때렸던 적 목록을 초기화
        alreadyHit.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            // 이번 공격에서 이미 때린 적이면 무시
            if (alreadyHit.Contains(other)) return;

            // 때린 적으로 기록
            alreadyHit.Add(other);

            Monster monster = other.GetComponentInParent<Monster>();
            if (monster != null)
            {

                // 공격 정보의 데미지 배율에 플레이어의 기본 공격력을 곱해서 전달
                AttackDetails finalAttackDetails = attackDetails;
                finalAttackDetails.damageRate *= player.Atk;

                monster.OnDamaged(finalAttackDetails, transform.position); // 히트박스의 공격 위치도 전달
                Debug.Log($"{monster.name}에게 데미지를 가함!");
            }
        }
    }

    // 애니메이션 이벤트에서 호출할 함수
    public void SetComboAttackDetails(int index)
    {
        attackDetails = player.AttackDetails[index];
    }
}