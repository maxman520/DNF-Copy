using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class BehaviourController
{
    private readonly Player player;
    private readonly InputHandler inputHandler;
    private readonly AnimController animController;
    private Coroutine jumpCoroutine;

    private const float JUMP_MOVEMENT_PENALTY = 0.3f;
    private const float JUMP_DURATION = 1.0f;
    private const float JUMP_HEIGHT = 3.0f;

    public BehaviourController(Player player, InputHandler inputHandler, AnimController animController)
    {
        this.player = player;
        this.inputHandler = inputHandler;
        this.animController = animController;
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
        if (!player.CanMove) return;

        if (Mathf.Abs(inputHandler.MoveInput.x) > 0.1f)
        {
            float direction = Mathf.Sign(inputHandler.MoveInput.x);
            player.transform.localScale = new Vector3(direction, 1f, 1f);
        }
    }

    // ĳ���� �̵�
    public void ApplyMovement()
    {
        if (!player.CanMove) return;

        float currentSpeed = player.IsRunning ? player.RunSpeed : player.WalkSpeed;
        player.IsMoving = inputHandler.MoveInput.magnitude > 0.1f;
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
        if (!player.IsJumping && player.CanMove) player.IsRunning = true;
    }

    // �޸��� ����
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        player.IsRunning = false;
    }

    // ����
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (player.CanMove) StartJump();
    }

    // ����
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (!player.CanAttack) return;

        // < ���� ���� >
        if (player.IsJumping)
        {
            PerformJumpAttack();
            return;
        }

        if (!player.IsGrounded) return;


        // < ���� ���� >
        // ������ �̾ �� �ִ� Ÿ�̹��̶��
        if (player.CanContinueAttack)
        {
            player.AttackCounter++;
            PerformAttack();
        }
        // ù ������ ���
        else if (!player.IsAttacking)
        {
            player.Rb.linearVelocity = Vector2.zero;
            // �޸��� �� ����
            if (player.IsRunning && Mathf.Abs(inputHandler.MoveInput.x) > 0.1f)
            {
                PerformRunAttack();
            }
            // �̿� �ȴ� ��, Idle ���� ��
            else
            {
                player.IsRunning = false;
                player.AttackCounter = 1;
                PerformAttack();
            }
        }
    }

    private void PerformAttack()
    {
        float direction = player.transform.localScale.x;
        player.Rb.AddForceX(direction * 0.05f, ForceMode2D.Impulse);

        player.CanContinueAttack = false; // ���� ������ ���� �ϴ� �ݾƵ�
        animController.PlayAttack(player.AttackCounter);
    }

    private void PerformRunAttack()
    {
        Debug.Log("�޸��� ����");
        return; // �ӽ÷� �ۼ�
        player.Anim.SetTrigger("RunAttack");
    }

    private void PerformJumpAttack()
    {
        // �̹� �̹� �������� ������ �ߴٸ� ����
        if (player.IsJumpAttacking) return;

        // ���� ���� ����
        animController.PlayJumpAttack();
    }

    // �޺��� �̾ �� �ִ� "Ÿ�̹�" �� �� �ִϸ��̼ǿ��� ȣ��
    public void OnComboWindowOpen()
    {
        // 3Ÿ�� �ƴϸ� ���� �޺� �Է��� ���� �غ� ��
        if (player.AttackCounter < 3)
        {
            player.CanContinueAttack = true;
        }
    }

    // �޺��� �̾ �� �ִ� "Ÿ�̹�" �� ������ �� �ִϸ��̼ǿ��� ȣ��
    public void OnComboWindowClose()
    {
        // �޺��� �̾����� �ʰ� ������ ��쿡 ���� ���� ó��
        player.AttackCounter = 0;
        player.CanContinueAttack = false;
    }
    public void ResetAttackState()
    {
        player.AttackCounter = 0;
        player.CanContinueAttack = false;
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
        }
    }

    private IEnumerator JumpRoutine()
    {
        // ���� ���� ����
        player.IsGrounded = false;
        player.IsJumping = true;
        animController.PlayJump();

        // ���� ��
        float elapsedTime = 0f;
        Vector3 startPos = player.VisualsTransform.localPosition;
        float previousHeight = 0f;

        while (elapsedTime < JUMP_DURATION)
        {
            float progress = elapsedTime / JUMP_DURATION;
            float currentHeight = Mathf.Sin(progress * Mathf.PI) * JUMP_HEIGHT + startPos.y;

            // ���־� ��ġ ������Ʈ
            player.VisualsTransform.localPosition = new Vector3(startPos.x, currentHeight, startPos.z);

            // �ִϸ��̼� ������Ʈ
            float yVelocity = (currentHeight - previousHeight) / Time.deltaTime;
            animController.UpdateJumpAnimation(yVelocity);

            previousHeight = currentHeight;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ���� �Ϸ� ó��
        player.VisualsTransform.localPosition = startPos;
        player.IsGrounded = true;
        player.IsJumping = false;
        player.IsRunning = false;

        animController.ResetJumpAttackTrigger();
        animController.UpdateJumpAnimation(0f);

        jumpCoroutine = null;
    }
    #endregion
}