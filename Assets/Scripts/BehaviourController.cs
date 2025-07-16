using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class BehaviourController
{
    private readonly Player player;
    private readonly InputHandler inputHandler;
    private readonly AnimController animController;
    private readonly SkillManager skillManager;
    Vector3 startPos;

    // ���� ���� ����
    private const float ORIGINAL_GRAVITY = 12f;
    private const float JUMP_FORCE = 8f;     // ���� �� ���� ���ϴ� �ʱ� '��'(�ӵ�)
    private const float JUMP_MOVEMENT_PENALTY = 0.5f;
    private float verticalVelocity; // ���� '��'�� ����� ��Ÿ���� ���� �ӵ�
    private float gravity = ORIGINAL_GRAVITY; // ���� �߷°�

    public BehaviourController(Player player, InputHandler inputHandler, AnimController animController, SkillManager skillManager)
    {
        this.player = player;
        this.inputHandler = inputHandler;
        this.animController = animController;
        this.skillManager = skillManager;
        startPos = player.VisualsTransform.localPosition;
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

        velocity.y *= 0.7f; // y���� x�ຸ�� ������ �����̵���

        // �������̶�� y�� �ӵ��� �г�Ƽ �ΰ�
        if (player.HasState(PlayerAnimState.Jump))
        {
            velocity.y *= JUMP_MOVEMENT_PENALTY;
        }

        player.Rb.linearVelocity = velocity;
    }


    // �޸��� ����
    public bool PerformRun(InputAction.CallbackContext context)
    {
        if (!player.HasState(PlayerAnimState.Jump) && player.CanMove)
            player.IsRunning = true;

        return player.IsRunning;
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
    #region Attack
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
        // player.Rb.AddForceX(direction * 0.05f, ForceMode2D.Impulse);
        player.Rb.linearVelocity = new Vector3(direction * 0.05f, 0 , 0);

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
    #endregion Attack
    // Player�κ��� �ǰ� ó���� ���ӹ޴� �Լ�
    public void HandleHurt(AttackDetails attackDetails, Vector3 attackPosition)
    {
        // ���� ����
        float direction = (player.transform.position.x > attackPosition.x) ? 1 : -1;

        if (player.IsGrounded) // ���� ���� ��
        {
            if (attackDetails.launchForce > 0) // ���� ����
            {
                // ���� �˹�
                player.Rb.linearVelocity = Vector2.zero; // ���� �ӵ� �ʱ�ȭ
                player.Rb.AddForce(new Vector2(direction * attackDetails.knockbackForce, 0), ForceMode2D.Impulse);

                // ���߿� �ߴ� �� ����
                verticalVelocity = attackDetails.launchForce;
                player.IsGrounded = false;
                animController.PlayAirborne(); // �Ǵ� AnimController�� ���� �ִϸ��̼� ���� ����
            }
            else // �Ϲ� ����
            {
                // ª�� ���� �˹�
                player.Rb.linearVelocity = Vector2.zero; // ���� �ӵ� �ʱ�ȭ
                player.transform.position += new Vector3(direction * attackDetails.knockbackForce * 0.1f, 0);
                // player.Rb.AddForce(new Vector2(direction * attackDetails.knockbackForce, 0), ForceMode2D.Impulse);

                animController.PlayHurt(Random.Range(1, 3)); // hurt �ִϸ��̼� ���
            }
        }
        else // ���߿� ���� ��
        {
            // ���� �˹�
            player.Rb.AddForce(new Vector2(direction * attackDetails.knockbackForce, 0), ForceMode2D.Impulse);
            animController.PlayAirborne(); // hurt �ִϸ��̼� ���

            // ���� �޺� ����. �÷��̾�� �ʿ����
            // if (attackDetails.launchForce > 0) airHitCounter++;

            // ���� �ǰ� ���� ���� ����. �÷��̾�� �ʿ����
            // float heightDecayFactor = Mathf.Max(0, 1f - (airHitCounter * 0.2f));
            // float finalLaunchForce = attackDetails.launchForce * heightDecayFactor;
            // float airHitReactionForce = 4f; // ���߿��� �¾��� �� ��¦ Ƣ������� �⺻ ��

            verticalVelocity = 4f;
        }
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
            player.Rb.linearVelocity = Vector3.zero;
            player.IsGrounded = true;
            player.IsRunning = false;
            animController.ResetJumpAttackTrigger();
        }
    }
    #endregion Jump


    #region Skill
    // ��ų�� �����ϴ� ���ο� �Լ�
    public bool PerformSkill(InputAction.CallbackContext context, int slotIndex)
    {
        // ���� ���� üũ
        if (!player.CanAttack || player.HasState(PlayerAnimState.Attack))
            return false;

        // ��ų ����� �������� üũ
        if (skillManager.IsSkillReady(slotIndex, out SkillData skillToExecute))
        {
            Debug.Log($"'{skillToExecute.skillName}' ��ų ����");

            player.Rb.linearVelocity = Vector3.zero;
            player.Anim.Play(skillToExecute.animName);

            // ���� �Ҹ�
            // player.ConsumeMana(skillToExecute.manaCost);

            // ��ų ��Ÿ���� ����
            skillManager.StartCooldown(slotIndex);

            return true;
        }

       // ��ų ����� �Ұ������� �ִϸ��̼����� �÷��̾�� �νĽ�Ŵ
        CooldownShake().Forget();

        // ��ų ����� �Ұ����ϹǷ� false
        return false;
    }

    // ��ų�� ��Ÿ���Ͻ� �ε�ε� ���� �ִϸ��̼�
    private async UniTask CooldownShake()
    {
        float shakeDuration = 0.05f; // ��ü ���� �ð�
        float shakeAmount = 0.04f;   // ������ ���� (�����̴� �Ÿ�)
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            // ������ �������� ��¦ �̵�
            float x = Random.Range(-1f, 1f) * shakeAmount;

            player.VisualsTransform.localPosition = startPos + new Vector3(x, 0, 0);

            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }

        // ������ ������ ���� ��ġ�� ����
        player.VisualsTransform.localPosition = startPos;
    }
    #endregion Skill
}