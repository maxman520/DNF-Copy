using UnityEngine;
using UnityEngine.InputSystem;

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
        inputHandler.OnRunPerformed += OnRunPerformed;
    }

    public void UnsubscribeFromEvents()
    {
        inputHandler.OnRunPerformed -= OnRunPerformed;
    }


    // ĳ���� ���� ����
    public void Flip()
    {
        if (!player.CanMove || player.HasState(PlayerAnimState.Attack)) return;

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

        player.Rb.linearVelocity = velocity;
    }


    // �޸��� ����
    public void OnRunPerformed(InputAction.CallbackContext context)
    {
        if (!player.HasState(PlayerAnimState.Jump) && player.CanMove)
            player.IsRunning = true;
    }
    // ����
    public bool PerformJump(InputAction.CallbackContext context)
    {
        // ���� ���������� ���� X
        if (!player.CanMove || !player.IsGrounded)
            return false;

        // ���� �ӵ��� ���� '��'�� ��� ����
        verticalVelocity = JUMP_FORCE;

        // ���¸� '���߿� ��' ���·� ����
        player.IsGrounded = false;
        animController.PlayJump();

        return true;
    }

    // ����
    public bool PerformAttack(InputAction.CallbackContext context)
    {
        if (!player.CanAttack) return false;

        // < ���� ���� >
        if (player.HasState(PlayerAnimState.Jump) && !player.HasState(PlayerAnimState.Attack))
        {
            PerformJumpAttack();
            return true;
        }


        // < ���� ���� >
        // ������ �̾ �� �ִ� Ÿ�̹��̶��
        if (player.CanContinueAttack)
        {
            player.AttackCounter++;
            PerformComboAttack();
            return true;
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
                PerformComboAttack();
            }
            return true;
        }

        return false;
    }

    // �÷��̾��� 1, 2, 3 �޺� ����
    public void PerformComboAttack()
    {
        float direction = player.transform.localScale.x;
        player.Rb.AddForceX(direction * 0.05f, ForceMode2D.Impulse);

        player.CanContinueAttack = false; // ���� ������ ���� �ϴ� �ݾƵ�
        animController.PlayAttack(player.AttackCounter);
    }

    public void PerformRunAttack()
    {
        Debug.Log("�޸��� ����");
        return; // �ӽ÷� �ۼ�
        animController.PlayAttack(player.AttackCounter);
    }

    public void PerformJumpAttack()
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
        ResetAttackState();
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
    #endregion Jump
}