using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class BehaviourController
{
    private readonly Player player;
    private readonly InputHandler inputHandler;
    private readonly AnimHashes animHashes;
    private Coroutine jumpCoroutine;

    // --- 상태 변수 ---
    private int attackCounter = 0;

    // --- 콤보 시스템을 위한 변수 ---
    private bool canContinueAttack = false; // 공격을 이어갈 수 있는 상태인지 체크

    private const float JUMP_MOVEMENT_PENALTY = 0.3f;
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


    // 캐릭터 방향 조절
    public void Flip()
    {
        if (player.IsAttacking || !player.IsGrounded || player.IsHurt) return;

        if (Mathf.Abs(inputHandler.MoveInput.x) > 0.1f)
        {
            float direction = Mathf.Sign(inputHandler.MoveInput.x);
            player.transform.localScale = new Vector3(direction, 1f, 1f);
        }
    }

    // 캐릭터 이동
    public void ApplyMovement()
    {
        if (player.IsHurt)
        {
            player.Rb.linearVelocity = Vector2.zero;
            return;
        }

        if (player.IsAttacking)
        {
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

    // 달리기 시작
    private void OnRunPerformed(InputAction.CallbackContext context)
    {
        if (!player.IsJumping && !player.IsAttacking || !player.IsHurt) player.IsRunning = true;
    }

    // 달리기 중지
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        player.IsRunning = false;
    }

    // 점프
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (player.IsHurt) return; // 피격 시 점프 불가
        if (!player.IsAttacking) StartJump();
    }

    // 공격
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (player.IsHurt) return; // 피격 시 공격 불가

        // < 점프 공격 >
        if (player.IsJumping)
        {
            PerformJumpAttack();
            return;
        }

        if (!player.IsGrounded) return;


        // < 지상 공격 >
        // 공격을 이어갈 수 있는 타이밍이라면
        if (canContinueAttack)
        {
            attackCounter++;
            PerformComboAttack();
        }
        // 첫 공격인 경우
        else if (!player.IsAttacking)
        {
            player.Rb.linearVelocity = Vector2.zero;
            Debug.Log("My IsRunning is " + player.IsRunning);
            // 달리는 중 공격
            if (player.IsRunning && Mathf.Abs(inputHandler.MoveInput.x) > 0.1f)
            {
                PerformRunAttack();
            }
            // 이외 걷는 중, Idle 상태 등
            else
            {
                player.IsRunning = false;
                attackCounter = 1;
                PerformComboAttack();
            }
        }
    }

    private void PerformComboAttack()
    {
        float direction = player.transform.localScale.x;
        player.Rb.AddForceX(direction * 0.05f, ForceMode2D.Impulse);

        canContinueAttack = false; // 다음 공격을 위해 일단 닫아둠
        player.Anim.SetTrigger("attack"+ attackCounter);
    }

    private void PerformRunAttack()
    {
        Debug.Log("달리기 공격");
        return; // 임시로 작성
        player.Anim.SetTrigger("RunAttack");
    }

    private void PerformJumpAttack()
    {
        // 이미 이번 점프에서 공격을 했다면 무시
        if (player.IsJumpAttacking) return;

        // 점프 공격 실행
        player.IsJumpAttacking = true; // 공격했다고 표시
        player.Anim.SetTrigger("jumpAttack"); // 애니메이션 트리거 발동
    }
    // 애니메이션 시작 시 호출
    public void OnAttackStart()
    {
        player.IsAttacking = true;
    }

    // 콤보를 이어갈 수 있는 "타이밍" 일 때 애니메이션에서 호출
    public void OnComboWindowOpen()
    {
        // 3타가 아니면 다음 콤보 입력을 받을 준비를 함
        if (attackCounter < 3)
        {
            canContinueAttack = true;
        }
    }
    
    // 콤보를 이어갈 수 있는 "타이밍" 이 끝났을 때 애니메이션에서 호출
    public void OnComboWindowClose()
    {
        // 콤보가 이어지지 않고 끝났을 경우에 대한 최종 처리
        attackCounter = 0;
        canContinueAttack = false;
    }

    // 공격 애니메이션이 완전히 끝났을 때 호출
    public void OnAttackEnd()
    {
        player.IsAttacking = false;
        attackCounter = 0;
        canContinueAttack = false;
    }
    // 피격 애니메이션이 끝났을 때 호출
    public void OnHurtEnd()
    {
        player.IsHurt = false;
        Debug.Log("피격 상태 해제");
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
        // 점프 시작 설정
        player.IsGrounded = false;
        player.IsJumping = true;
        player.IsJumpAttacking = false;

        player.Anim.SetBool(animHashes.IsGrounded, false);
        player.Anim.SetTrigger(animHashes.Jump);

        // 점프 중
        float elapsedTime = 0f;
        Vector3 startVisualPos = player.VisualsTransform.localPosition;
        float previousHeight = 0f;

        while (elapsedTime < JUMP_DURATION)
        {
            float progress = elapsedTime / JUMP_DURATION;
            float currentHeight = Mathf.Sin(progress * Mathf.PI) * JUMP_HEIGHT;

            // 비주얼 위치 업데이트
            player.VisualsTransform.localPosition = new Vector3(startVisualPos.x, currentHeight, startVisualPos.z);

            // 애니메이션 업데이트
            float yVelocity = (currentHeight - previousHeight) / Time.deltaTime;
            player.Anim.SetFloat(animHashes.YVelocity, yVelocity);

            previousHeight = currentHeight;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 점프 완료 처리
        player.VisualsTransform.localPosition = startVisualPos;
        player.IsGrounded = true;
        player.IsJumping = false;
        player.IsRunning = false;
        player.IsJumpAttacking = false;

        player.Anim.ResetTrigger("jumpAttack");
        player.Anim.SetBool(animHashes.IsGrounded, true);
        player.Anim.SetFloat(animHashes.YVelocity, 0);

        jumpCoroutine = null;
    }
    #endregion
}