using UnityEngine;

public class MonsterHitbox : MonoBehaviour
{
    private Vector3 originPosition; // 공격 판정 y축 계산을 위한 기준 벡터
    private AttackDetails attackDetails;

    // 한 번의 공격 모션에서 플레이어를 여러 번 때리지 않도록 기억하는 bool변수
    private bool alreadyHit = false;

    // 외부에서 이 히트박스의 공격 정보를 설정해주는 함수
    // origin을 생략할 시 히트박스의 Position을 기준으로 y축 범위를 계산
    public void Initialize(AttackDetails details, Vector3? origin = null)
    {
        this.attackDetails = details;
        this.originPosition = origin.HasValue ? origin.Value : this.transform.position;
        Debug.Log("Monster Hitbox origin position " + originPosition.x + " " + originPosition.y);

        // 히트박스가 활성화될 때마다 초기화
        this.alreadyHit = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerHurtbox"))
        {
            // 공격 정보가 설정되지 않았다면 아무것도 하지 않음
            if (attackDetails.attackName == null)
            {
                Debug.Log("공격 정보가 설정되지 않음");
                return;
            }
            if (alreadyHit) {
                Debug.Log("이미 때렸으므로 무시됨");
                return;
            } // 이미 때렸다면 무시

            Player player = Player.Instance;

            // 몬스터 공격의 Y축 범위 체크
            if (Mathf.Abs(originPosition.y - player.transform.position.y) >= attackDetails.yOffset)
                return;

            // 때린 것으로 기록
            alreadyHit = true;

            if (player != null)
            {
                player.OnDamaged(attackDetails, originPosition);
            }
        }

    }
}