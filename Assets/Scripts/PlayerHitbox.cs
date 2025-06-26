using UnityEngine;
using System.Collections.Generic; // 여러 대상을 기억하기 위해 필요

public class PlayerHitbox : MonoBehaviour
{
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
            // 이번 공격에서 이미 때린 적인지 확인
            if (alreadyHit.Contains(other))
            {
                return; // 이미 때렸으면 무시
            }

            // 때린 적으로 기록
            alreadyHit.Add(other);

            // Hurtbox의 부모에게서 Monster 컴포넌트를 찾음
            Monster monster = other.GetComponentInParent<Monster>();
            if (monster != null)
            {
                // 플레이어의 공격력으로 몬스터에게 데미지를 입힘
                monster.TakeDamage(player.Atk);
            }
        }
    }
}