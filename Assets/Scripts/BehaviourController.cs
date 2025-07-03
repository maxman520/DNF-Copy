using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEditorInternal;

public class BehaviourController
{
    private readonly Player player;
    private readonly InputHandler inputHandler;
    private readonly AnimController animController;
    Vector3 startPos;

    // ���� ���� ����
    private const float ORIGINAL_GRAVITY = 21f;
    private const float JUMP_FORCE = 11f;     // ���� �� ���� ���ϴ� �ʱ� '��'(�ӵ�)
    private const float JUMP_MOVEMENT_PENALTY = 0.3f;
    private float verticalVelocity; // ���� '��'�� ����� ��Ÿ���� ���� �ӵ�
    private float gravity = ORIGINAL_GRAVITY; // ���� �߷°�

    public BehaviourController(Player player, InputHandler inputHandler, AnimController animController)
    {
        this.player = player;
        this.inputHandler = inputHandler;
        this.animController = animController;
        startPos = player.VisualsTransform.localPosition;
    }
    public void SubscribeToEvents()
    {
        inputHandler.OnMoveCanceled += OnMoveCanceled;
        inputHandler.OnRunPerformed += OnRunPerformed;
        inputHandler.OnJumpPerformed += OnJumpPerformed;
        inputHandler.OnAttackPerformed += OnAttackPerformed;
    }

    public void UnsubscribeFromEvents()
    {
        inputHandler.OnMoveCanceled -= OnMoveCanceled;
        inputHandler.OnRunPerformed -= OnRunPerformed;
        inputHandler.OnJumpPerformed -= OnJumpPerformed;
        inputHandler.OnAttackPerformed -= OnAttackPerformed;
    }


    // ĳ���� ���� ����
    public void Flip()
    {
        if (!player.CanMove || player.HasState(PlayerAnimState.Jump)) return;

        if (Mathf.Abs(inputHandler.MoveInput.x) > 0.1f)
        {
            float direction = Mathf.Sign(inputHandler.MoveInput.x);
            player.transform.localScale = new Vector3(direction, 1f, 1f);
        }
    }

    // ĳ���� �̵�
    public void ApplyMovement()
    {
        if (!player.CanMove)
            return;

        float currentSpeed = player.IsRunning ? player.RunSpeed : player.WalkSpeed;
        player.IsMoving = inputHandler.MoveInput.magnitude > 0.1f;
        Vector2 velocity = inputHandler.MoveInput.normalized * currentSpeed;

        // �������̶�� y�� �ӵ��� �г�Ƽ �ΰ�
        if (player.HasState(PlayerAnimState.Jump))
        {
            velocity.y *= JUMP_MOVEMENT_PENALTY;
        }

        if (player.CanMove)
            player.Rb.linearVelocity = velocity;
    }

    // �޸��� ����
    private void OnRunPerformed(InputAction.CallbackContext context)
    {
        if (!player.HasState(PlayerAnimState.Jump) && player.CanMove)
            player.IsRunning = true;
    }
    // �޸��� ����
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        player.IsRunning = false;
    }
    // ����
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        // ���� ���� ���� ���� '��'�� ���� �� �ִ�.
        if (player.CanMove && player.IsGrounded)
        {
            ApplyJumpForce();
        }
    }

    // ����
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (!player.CanAttack) return;

        // < ���� ���� >
        if (player.HasState(PlayerAnimState.Jump))
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
        else if (!player.HasState(PlayerAnimState.Attack))
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
        animController.PlayAttack(player.AttackCounter);
    }

    private void PerformJumpAttack()
    {
        // �̹� �̹� �������� ������ �ߴٸ� ����
        if (player.HasState(PlayerAnimState.Jump)
            && player.HasState(PlayerAnimState.Attack)) return;

        // ���� ���� ����
        animController.PlayAttack(player.AttackCounter);
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

    // Update���� �Ź� üũ�ϸ� �߷� ����
    public void HandleGravity()
    {
        // 1. ���߿� �� �ִٸ�
        if (!player.IsGrounded)
        {
            // 2. �߷��� ��� ����
            verticalVelocity += (- gravity) * Time.deltaTime;

            // 3. ���� �ӵ��� Visuals�� local Y��ǥ�� ����
            player.VisualsTransform.localPosition += new Vector3(0, verticalVelocity * Time.deltaTime, 0);

            // 4. �����ߴ��� Ȯ��
            CheckForLanding();
        }

        // 5. ���� ���� �ӵ��� �ִϸ����Ϳ� ����
        animController.UpdateJumpAnimation(verticalVelocity);
    }

    // "������ Visuals�� local y��ǥ���� �̵��� �ϰ� ����� ���� ���ϴ� ���"
    private void ApplyJumpForce()
    {
        // ���� �ӵ��� ���� '��'�� ��� ����
        verticalVelocity = JUMP_FORCE;

        // ���¸� '���߿� ��' ���·� ����
        player.IsGrounded = false;
        animController.PlayJump();
    }

    // ���� �Ǻ� ����
    private void CheckForLanding()
    {
        // Visuals�� Y ��ǥ�� �ٴ�(0)���� �Ʒ��� �������ٸ� ������ ����
        if (player.VisualsTransform.localPosition.y <= startPos.y)
        {
            // ��ġ�� �ӵ�, �߷� �ʱ�ȭ
            player.VisualsTransform.localPosition = startPos;
            verticalVelocity = 0f;
            gravity = ORIGINAL_GRAVITY;

            // ���� �ʱ�ȭ
            player.IsGrounded = true;
            player.IsRunning = false;
            animController.ResetJumpAttackTrigger();
        }
    }
    #endregion
}