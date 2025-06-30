using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Goblin : Monster
{
    public bool IsGrounded = true;
    public bool IsWalking = false;


    // ���� ���� ����
    private const float ORIGINAL_GRAVITY = 10f;
    private float verticalVelocity; // ���� '��'�� ����� ��Ÿ���� ���� �ӵ�
    private float gravity = ORIGINAL_GRAVITY; // ���� �߷°�
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
        Debug.Log("����� ����!");
    }

    protected override void Hurt(AttackDetails attackDetails, Vector2 attackPosition)
    {
        // ���� �˹�
        float direction = (transform.position.x > attackPosition.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * attackDetails.knockbackForce, 0);

        if (IsGrounded) // ���� ���� ��
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
        else // ���߿� ���� ��
        {
            if (attackDetails.launchForce > 0) airHitCounter++;
            verticalVelocity = 2f + (attackDetails.launchForce * Mathf.Max(0, 0.5f - (airHitCounter * 0.125f)));
            gravity += 0.05f;
        }
    }
    public void HandleGravity()
    {
        // 1. ���߿� �� �ִٸ�
        if (!IsGrounded)
        {
            // 2. �߷��� ��� ����
            verticalVelocity += (-gravity) * Time.deltaTime;

            // 3. ���� �ӵ��� Visuals�� local Y��ǥ�� ����
            visualsTransform.localPosition += new Vector3(0, verticalVelocity * Time.deltaTime, 0);

            // 4. �����ߴ��� Ȯ��
            CheckForLanding();
        }
    }

    // ���� �Ǻ� ����
    private void CheckForLanding()
    {
        // Visuals�� Y ��ǥ�� ���� Y��ǥ���� �Ʒ��� �������ٸ� ������ ����
        if (visualsTransform.localPosition.y <= startPos.y)
        {
            // ���� �ʱ�ȭ
            IsGrounded = true;
            airHitCounter = 0;


            // ��ġ�� �ӵ�, �߷� �ʱ�ȭ
            visualsTransform.localPosition = startPos;
            verticalVelocity = 0f;
            gravity = ORIGINAL_GRAVITY;
        }
    }
    protected override void Die()
    {
        Debug.Log($"{monsterData.MonsterName}��(��) �׾����ϴ�.");

        // ������ �����Ӱ� �浹�� ����
        rb.linearVelocity = Vector2.zero;
        GetComponentInChildren<Collider2D>().enabled = false; // �ٸ� ������Ʈ�� �浹���� �ʵ���

        // ���� �ִϸ��̼� ���
        anim.SetTrigger("die");

        // ����: 2�� �Ŀ� ������Ʈ �ı�
        Destroy(gameObject, 2f);
    }
}