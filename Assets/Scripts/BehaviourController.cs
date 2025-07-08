using UnityEngine;
using UnityEngine.InputSystem;

public class BehaviourController
{
    private readonly Player player;
    private readonly InputHandler inputHandler;
    private readonly AnimController animController;
    Vector3 startPos;

    // 가상 물리 변수
    private const float ORIGINAL_GRAVITY = 21f;
    private const float JUMP_FORCE = 11f;     // 점프 시 위로 가하는 초기 '힘'(속도)
    private const float JUMP_MOVEMENT_PENALTY = 0.3f;
    private float verticalVelocity; // 수직 '힘'의 결과로 나타나는 현재 속도
    private float gravity = ORIGINAL_GRAVITY; // 가상 중력값

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


    // 캐릭터 방향 조절
    public void Flip()
    {
        if (!player.CanMove || player.HasState(PlayerAnimState.Attack)) return;

        if (Mathf.Abs(inputHandler.MoveInput.x) > 0.1f)
        {
            float direction = Mathf.Sign(inputHandler.MoveInput.x);
            player.transform.localScale = new Vector3(direction, 1f, 1f);
        }
    }

    // 캐릭터 이동
    public void ApplyMovement()
    {
        if (!player.CanMove)
            return;

        float currentSpeed = player.IsRunning ? player.RunSpeed : player.WalkSpeed;
        player.IsMoving = inputHandler.MoveInput.magnitude > 0.1f;

        Vector2 velocity = inputHandler.MoveInput.normalized * currentSpeed;

        // 점프중이라면 y축 속도에 패널티 부과
        if (player.HasState(PlayerAnimState.Jump))
        {
            velocity.y *= JUMP_MOVEMENT_PENALTY;
        }

        player.Rb.linearVelocity = velocity;
    }


    // 달리기 시작
    public void OnRunPerformed(InputAction.CallbackContext context)
    {
        if (!player.HasState(PlayerAnimState.Jump) && player.CanMove)
            player.IsRunning = true;
    }
    // 점프
    public bool PerformJump(InputAction.CallbackContext context)
    {
        // 땅에 있지않으면 점프 X
        if (!player.CanMove || !player.IsGrounded)
            return false;

        // 수직 속도에 점프 '힘'을 즉시 적용
        verticalVelocity = JUMP_FORCE;

        // 상태를 '공중에 뜬' 상태로 변경
        player.IsGrounded = false;
        animController.PlayJump();

        return true;
    }

    // 공격
    public bool PerformAttack(InputAction.CallbackContext context)
    {
        if (!player.CanAttack) return false;

        // < 점프 공격 >
        if (player.HasState(PlayerAnimState.Jump) && !player.HasState(PlayerAnimState.Attack))
        {
            PerformJumpAttack();
            return true;
        }


        // < 지상 공격 >
        // 공격을 이어갈 수 있는 타이밍이라면
        if (player.CanContinueAttack)
        {
            player.AttackCounter++;
            PerformComboAttack();
            return true;
        }
        // 첫 공격인 경우
        else if (!player.HasState(PlayerAnimState.Attack))
        {
            player.Rb.linearVelocity = Vector2.zero;
            // 달리는 중 공격
            if (player.IsRunning && Mathf.Abs(inputHandler.MoveInput.x) > 0.1f)
            {
                PerformRunAttack();
            }
            // 이외 걷는 중, Idle 상태 등
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

    // 플레이어의 1, 2, 3 콤보 공격
    public void PerformComboAttack()
    {
        float direction = player.transform.localScale.x;
        player.Rb.AddForceX(direction * 0.05f, ForceMode2D.Impulse);

        player.CanContinueAttack = false; // 다음 공격을 위해 일단 닫아둠
        animController.PlayAttack(player.AttackCounter);
    }

    public void PerformRunAttack()
    {
        Debug.Log("달리기 공격");
        return; // 임시로 작성
        animController.PlayAttack(player.AttackCounter);
    }

    public void PerformJumpAttack()
    {
        // 이미 이번 점프에서 공격을 했다면 무시
        if (player.HasState(PlayerAnimState.Jump)
            && player.HasState(PlayerAnimState.Attack)) return;

        // 점프 공격 실행
        animController.PlayAttack(player.AttackCounter);
    }

    // 콤보를 이어갈 수 있는 "타이밍" 일 때 애니메이션에서 호출
    public void OnComboWindowOpen()
    {
        // 3타가 아니면 다음 콤보 입력을 받을 준비를 함
        if (player.AttackCounter < 3)
        {
            player.CanContinueAttack = true;
        }
    }

    // 콤보를 이어갈 수 있는 "타이밍" 이 끝났을 때 애니메이션에서 호출
    public void OnComboWindowClose()
    {
        // 콤보가 이어지지 않고 끝났을 경우에 대한 최종 처리
        ResetAttackState();
    }
    public void ResetAttackState()
    {
        player.AttackCounter = 0;
        player.CanContinueAttack = false;
    }

    #region Jump

    // Update에서 매번 체크하며 중력 적용
    public void HandleGravity()
    {
        // 1. 공중에 떠 있다면
        if (!player.IsGrounded)
        {
            // 2. 중력을 계속 적용
            verticalVelocity += (- gravity) * Time.deltaTime;

            // 3. 계산된 속도로 Visuals의 local Y좌표를 변경
            player.VisualsTransform.localPosition += new Vector3(0, verticalVelocity * Time.deltaTime, 0);

            // 4. 착지했는지 확인
            CheckForLanding();
        }

        // 5. 현재 수직 속도를 애니메이터에 전달
        animController.UpdateJumpAnimation(verticalVelocity);
    }

    // 착지 판별 로직
    private void CheckForLanding()
    {
        // Visuals의 Y 좌표가 바닥(0)보다 아래로 내려갔다면 착지로 간주
        if (player.VisualsTransform.localPosition.y <= startPos.y)
        {
            // 위치와 속도, 중력 초기화
            player.VisualsTransform.localPosition = startPos;
            verticalVelocity = 0f;
            gravity = ORIGINAL_GRAVITY;

            // 상태 초기화
            player.IsGrounded = true;
            player.IsRunning = false;
            animController.ResetJumpAttackTrigger();
        }
    }
    #endregion Jump
}