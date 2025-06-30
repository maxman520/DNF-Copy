using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Goblin : Monster
{
    public bool IsGrounded = true;
    public bool IsWalking = false;


    // 가상 물리 변수
    private const float ORIGINAL_GRAVITY = 10f;
    private float verticalVelocity; // 수직 '힘'의 결과로 나타나는 현재 속도
    private float gravity = ORIGINAL_GRAVITY; // 가상 중력값
    private int airHitCounter = 0;


    void OnGUI()
    {
        GUI.Label(new Rect(10, 80, 200, 20), "goblin IsGrounded: " + IsGrounded);
        GUI.Label(new Rect(10, 90, 200, 20), "goblin IsWalking: " + IsWalking);
    }

    protected void Start()
    {
        if (Player.Instance != null)
        {
            playerTransform = Player.Instance.transform;
        }
    }
    private void Update()
    {
        HandleGravity();
        anim.SetBool("isGrounded", IsGrounded);
        anim.SetBool("isWalking", IsWalking);
    }

    #region Utilities
    private bool IsPlayerInRecognitionRange()
    {
        return playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= recognitionRange;
    }

    private bool IsPlayerInAttackRange()
    {
        return playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= attackRange;
    }

    private void Flip(float directionX)
    {
        if (Mathf.Abs(directionX) > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Sign(directionX), 1, 1);
        }
    }

    private void FlipTowardsPlayer()
    {
        if (playerTransform == null) return;
        float directionToPlayer = playerTransform.position.x - transform.position.x;
        Flip(directionToPlayer);
    }

    #endregion

    public override void Attack()
    {
        anim.SetTrigger("attack");
        Debug.Log("고블린의 공격!");
    }

    protected override void Hurt(AttackDetails attackDetails, Vector2 attackPosition)
    {
        // 수평 넉백
        float direction = (transform.position.x > attackPosition.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * attackDetails.knockbackForce, 0);

        if (IsGrounded) // 땅에 있을 때
        {
            if (attackDetails.launchForce > 0)
            {
                verticalVelocity = attackDetails.launchForce;

                IsGrounded = false;
                anim.SetTrigger("airborne");
            }
            else
            {
                anim.SetTrigger("hurt");
            }
        }
        else // 공중에 있을 때
        {
            if (attackDetails.launchForce > 0) airHitCounter++;
            verticalVelocity = 2f + (attackDetails.launchForce * Mathf.Max(0, 0.5f - (airHitCounter * 0.125f)));
            gravity += 0.05f;
        }
    }
    public void HandleGravity()
    {
        // 1. 공중에 떠 있다면
        if (!IsGrounded)
        {
            // 2. 중력을 계속 적용
            verticalVelocity += (-gravity) * Time.deltaTime;

            // 3. 계산된 속도로 Visuals의 local Y좌표를 변경
            visualsTransform.localPosition += new Vector3(0, verticalVelocity * Time.deltaTime, 0);

            // 4. 착지했는지 확인
            CheckForLanding();
        }
    }

    // 착지 판별 로직
    private void CheckForLanding()
    {
        // Visuals의 Y 좌표가 시작 Y좌표보다 아래로 내려갔다면 착지로 간주
        if (visualsTransform.localPosition.y <= startPos.y)
        {
            // 상태 초기화
            IsGrounded = true;
            airHitCounter = 0;


            // 위치와 속도, 중력 초기화
            visualsTransform.localPosition = startPos;
            verticalVelocity = 0f;
            gravity = ORIGINAL_GRAVITY;
        }
    }
    protected override void Die()
    {
        Debug.Log($"{monsterData.MonsterName}이(가) 죽었습니다.");

        // 물리적 움직임과 충돌을 중지
        rb.linearVelocity = Vector2.zero;
        GetComponentInChildren<Collider2D>().enabled = false; // 다른 오브젝트와 충돌하지 않도록

        // 죽음 애니메이션 재생
        anim.SetTrigger("die");

        // 예시: 2초 후에 오브젝트 파괴
        Destroy(gameObject, 2f);
    }
}