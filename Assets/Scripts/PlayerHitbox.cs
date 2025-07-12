using UnityEngine;
using System.Collections.Generic; // 여러 대상을 기억하기 위해 필요

public class PlayerHitbox : MonoBehaviour
{
    public AttackDetails attackDetails;

    // 이 공격의 y축 판정 기준이 될 좌표. 초기값을 float.MinValue로 설정하여, 초기화 여부를 명확하게 판단
    private float originY = float.MinValue;
    private Player player;

    // 한 번의 공격 모션에서 동일한 적을 여러 번 때리지 않도록 기억하는 리스트
    private List<Collider2D> alreadyHit = new List<Collider2D>();

    // 히트박스를 초기화
    public void Initialize(AttackDetails details, float originY)
    {
        this.attackDetails = details;
        this.originY = originY;
        this.alreadyHit.Clear();// 새로운 공격이 시작될 때마다, 이전에 때렸던 적 목록을 초기화
    }

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }
    private void OnEnable()
    {
        // 재사용될 경우를 대비해 초기화
        alreadyHit.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("MonsterHurtbox"))
        {
            Monster monster = other.GetComponentInParent<Monster>();
            if (monster == null) return;

            // originY가 초기값(MinValue) 그대로라면, 히트박스 자신의 Y 위치를 기준으로 사용.
            this.originY = (originY == float.MinValue) ? this.transform.position.y : this.originY;

            // 플레이어 공격의 Y축 범위 체크
            if (Mathf.Abs(this.originY - monster.transform.position.y) > attackDetails.yOffset)
                return;

            // 이번 공격에서 이미 때린 적이면 무시
            if (alreadyHit.Contains(other))
                return;

            // 때린 적으로 기록
            alreadyHit.Add(other);


            if (monster != null)
            {
                // 공격 정보의 데미지 배율에 플레이어의 기본 공격력을 곱해서 전달
                AttackDetails finalAttackDetails = attackDetails;
                finalAttackDetails.damageRate *= player.Atk;


                monster.OnDamaged(finalAttackDetails, transform.position); // 히트박스의 위치 값도 전달
                Debug.Log($"{monster.name}에게 데미지를 가함!");
            }
        }
    }
}