using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class BehaviourController
{
    private readonly Player player;
    private readonly InputHandler inputHandler;
    private readonly AnimHashes animHashes;
    private Coroutine jumpCoroutine;

    // --- ���� ���� ---
    private int attackCounter = 0;

    // --- �޺� �ý����� ���� ���� ---
    private bool canContinueAttack = false; // ������ �̾ �� �ִ� �������� üũ
    private Coroutine attackResetCoroutine; // ���� �޺� �ʱ�ȭ �ڷ�ƾ�� �����ϱ� ���� ����
    private const float ATTACK_TIMEOUT = 1.5f; // ���� �Է� ��� �ð�

    private const float JUMP_MOVEMENT_PENALTY = 0.2f;
    private const float JUMP_DURATION = 1.0f;
    private const float JUMP_HEIGHT = 3.0f;

    public BehaviourController(Player player, InputHandler inputHandler)
    {
        this.player = player;
        this.inputHandler = inputHandler;
        this.animHashes = new AnimHashes();
    }
    public void SubscribeToEvents()
    {
        inputHandler.OnRunPerformed += OnRunPerformed;
        inputHandler.OnMoveCanceled += OnMoveCanceled;
        inputHandler.OnJumpPerformed += OnJumpPerformed;
        inputHandler.OnAttackPerformed += OnAttackPerformed;
    }

    public void UnsubscribeFromEvents()
    {
        inputHandler.OnRunPerformed -= OnRunPerformed;
        inputHandler.OnMoveCanceled -= OnMoveCanceled;
        inputHandler.OnJumpPerformed -= OnJumpPerformed;
        inputHandler.OnAttackPerformed -= OnAttackPerformed;
    }


    // ĳ���� ���� ����
    public void Flip()
    {
        if (player.IsAttacking || !player.IsGrounded) return;

        if (Mathf.Abs(inputHandler.MoveInput.x) > 0.1f)
        {
            float direction = Mathf.Sign(inputHandler.MoveInput.x);
            player.transform.localScale = new Vector3(direction, 1f, 1f);
        }
    }

    // ĳ���� �̵�
    public void ApplyMovement()
    {
        if (player.IsAttacking)
        {
            player.Rb.linearVelocity = Vector2.zero;
            return;
        }
        float currentSpeed = player.IsRunning ? player.RunSpeed : player.WalkSpeed;
        Vector2 velocity = inputHandler.MoveInput.normalized * currentSpeed;

        if (player.IsJumping)
        {
            velocity.y *= JUMP_MOVEMENT_PENALTY;
        }

        player.Rb.linearVelocity = velocity;
    }

    // �޸��� ����
    private void OnRunPerformed(InputAction.CallbackContext context)
    {
        if (!player.IsJumping && !player.IsAttacking) player.IsRunning = true;
    }

    // �޸��� ����
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        player.IsRunning = false;
    }

    // ����
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!player.IsAttacking) StartJump();
    }

    // ����
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (!player.IsGrounded || player.IsJumping) return;


        // ������ �̾ �� �ִ� Ÿ�̹��̶��
        if (canContinueAttack)
        {
            // �������� �޺� ���� �ڷ�ƾ�� ����
            if (attackResetCoroutine != null)
            {
                player.StopCoroutine(attackResetCoroutine);
            }
            attackCounter++;
            PerformComboAttack();
        }
        // �޺��� �̾ �� ������, �ٸ� ���� �ߵ� �ƴ϶�� (ù ����)
        else if (!player.IsAttacking)
        {
            Debug.Log("My IsRunning is " + player.IsRunning);
            if (player.IsRunning && Mathf.Abs(inputHandler.MoveInput.x) > 0.1f) // �޸��� ��
            {
                PerformRunAttack();
            }
            else // �̿� �ȴ� ��, Idle ���� ��
            {
                player.IsRunning = false;
                attackCounter = 1;
                PerformComboAttack();
            }
        }
    }

    private void PerformComboAttack()
    {
        player.IsAttacking = true;
        canContinueAttack = false; // ���� ������ ���� �ϴ� �ݾƵ�
        player.Anim.SetTrigger("attack"+ attackCounter);
    }

    private void PerformRunAttack()
    {
        Debug.Log("�޸��� ����");
        return; // �ӽ÷� �ۼ�
        player.IsAttacking = true;
        player.Anim.SetTrigger("RunAttack");
    }

    // --- ���� ���� Ÿ�̸� �ڷ�ƾ ---
    private IEnumerator ResetAttackCoroutine()
    {
        yield return new WaitForSeconds(ATTACK_TIMEOUT);
        // �ð��� ������ �޺� ���� �������� ��� �ʱ�ȭ
        attackCounter = 0;
        canContinueAttack = false;
        player.IsAttacking = false;
    }

    // �ִϸ��̼� ���� �� ȣ��
    public void OnAttackStart()
    {
        player.IsAttacking = true;
    }

    // �޺��� �̾ �� �ִ� "Ÿ�̹�" �� �� �ִϸ��̼ǿ��� ȣ��
    public void OnComboWindowOpen()
    {
        // 3Ÿ�� �ƴϸ� ���� �޺� �Է��� ���� �غ� ��
        if (attackCounter < 3)
        {
            canContinueAttack = true;
            // �޺� ���� �ڷ�ƾ ����
            attackResetCoroutine = player.StartCoroutineFromController(ResetAttackCoroutine());
        }
    }

    // ���� �ִϸ��̼��� ������ ������ �� ȣ��
    public void OnAttackEnd()
    {
        // �޺��� �̾����� �ʰ� ������ ��쿡 ���� ���� ó��
        attackCounter = 0;
        player.IsAttacking = false;
        canContinueAttack = false;
    }



    #region Jump
    public void StartJump()
    {
        if (!player.IsGrounded || player.IsJumping) return;

        jumpCoroutine = player.StartCoroutineFromController(JumpRoutine());
    }

    public void ForceStopJump()
    {
        if (jumpCoroutine != null)
        {
            player.StopCoroutine(jumpCoroutine);
            jumpCoroutine = null;
            player.IsJumping = false;
            player.IsGrounded = true;

            if (player.PlayerGround != null)
                player.PlayerGround.enabled = true;
        }
    }

    private IEnumerator JumpRoutine()
    {
        // ���� ���� ����
        player.IsGrounded = false;
        player.IsJumping = true;

        player.Anim.SetBool(animHashes.IsGrounded, false);
        player.Anim.SetTrigger(animHashes.Jump);

        // ���� ��
        float elapsedTime = 0f;
        Vector3 startVisualPos = player.VisualsTransform.localPosition;
        float previousHeight = 0f;

        while (elapsedTime < JUMP_DURATION)
        {
            float progress = elapsedTime / JUMP_DURATION;
            float currentHeight = Mathf.Sin(progress * Mathf.PI) * JUMP_HEIGHT;

            // ���־� ��ġ ������Ʈ
            player.VisualsTransform.localPosition = new Vector3(startVisualPos.x, currentHeight, startVisualPos.z);

            // �ִϸ��̼� ������Ʈ
            float yVelocity = (currentHeight - previousHeight) / Time.deltaTime;
            player.Anim.SetFloat(animHashes.YVelocity, yVelocity);

            previousHeight = currentHeight;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ���� �Ϸ� ó��
        player.VisualsTransform.localPosition = startVisualPos;
        player.IsJumping = false;
        player.IsGrounded = true;
        player.IsRunning = false;

        player.Anim.SetBool(animHashes.IsGrounded, true);
        player.Anim.SetFloat(animHashes.YVelocity, 0);

        jumpCoroutine = null;
    }
    #endregion
}