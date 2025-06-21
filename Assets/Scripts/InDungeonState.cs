using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class InDungeonState : IPlayerState
{
    private readonly Player player;
    private readonly MovementController movementController;
    private readonly AnimationController animationController;

    public InDungeonState(Player player)
    {
        this.player = player;
        if (player == null) {
            Debug.Log("Player를 찾을 수 없음");
            return;
        }

        movementController = new MovementController(player);
        animationController = new AnimationController(player);
    }

    public void Enter()
    {
        Debug.Log("던전 상태에 진입");
        player.Anim.Play("Idle_Dungeon");
        player.IsRunning = false;
        movementController.SubscribeToEvents();
    }

    public void Update()
    {
        movementController.HandleInput();
        movementController.UpdateMovement();
        animationController.UpdateAnimations();
    }

    public void FixedUpdate()
    {
        movementController.ApplyMovement();
    }

    public void Exit()
    {
        Debug.Log("던전 상태를 벗어남");
        player.IsRunning = false;
        movementController.UnsubscribeFromEvents();
        movementController.ForceStopJump();
        animationController.ResetAnimations();
    }
}

public class MovementController
{
    private readonly Player player;
    private readonly AnimationHashes animHashes;
    private Coroutine jumpCoroutine;

    private const float JUMP_MOVEMENT_PENALTY = 0.3f;
    private const float JUMP_DURATION = 1.0f;
    private const float JUMP_HEIGHT = 3.0f;

    public MovementController(Player player)
    {
        this.player = player;
        this.animHashes = new AnimationHashes();
    }

    public void HandleInput()
    {
        if (player.InputActions.Player.Jump.WasPressedThisFrame())
        {
            StartJump();
        }
    }

    public void SubscribeToEvents()
    {
        player.InputActions.Player.Run.performed += OnRunPerformed;
        player.InputActions.Player.Move.canceled += OnMoveCanceled;
    }

    public void UnsubscribeFromEvents()
    {
        player.InputActions.Player.Run.performed -= OnRunPerformed;
        player.InputActions.Player.Move.canceled -= OnMoveCanceled;
    }

    public void UpdateMovement()
    {
        // 입력 처리
        Vector2 moveInput = player.InputActions.Player.Move.ReadValue<Vector2>();
        player.MoveInput = moveInput;

        // 캐릭터 방향 설정
        if (!player.IsJumping && Mathf.Abs(player.MoveInput.x) > 0.1f)
        {
            float direction = Mathf.Sign(player.MoveInput.x);
            player.transform.localScale = new Vector3(direction, 1f, 1f);
        }
    }

    public void ApplyMovement()
    {
        float currentSpeed = player.IsRunning ? player.RunSpeed : player.WalkSpeed;
        Vector2 velocity = player.MoveInput.normalized * currentSpeed;

        if (player.IsJumping)
        {
            velocity.y *= JUMP_MOVEMENT_PENALTY;
        }

        player.Rb.linearVelocity = velocity;
    }

    private void OnRunPerformed(InputAction.CallbackContext context)
    {
        // 달리기 시작
        if (!player.IsJumping) player.IsRunning = true;
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        // 달리기 중지
        player.IsRunning = false;
    }

    public void StartJump()
    {
        if (!player.IsGrounded || player.IsJumping) return;

        jumpCoroutine = player.StartCoroutineFromState(JumpRoutine());
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

        /*
        if (player.PlayerGround != null)
            player.PlayerGround.enabled = false;
        */

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

        /*
        if (player.PlayerGround != null)
            player.PlayerGround.enabled = true;
        */

        player.IsJumping = false;
        player.IsGrounded = true;
        player.IsRunning = false;

        player.Anim.SetBool(animHashes.IsGrounded, true);
        player.Anim.SetFloat(animHashes.YVelocity, 0);

        jumpCoroutine = null;
    }
}

public class AnimationController
{
    private readonly Player player;
    private readonly AnimationHashes animHashes;

    public AnimationController(Player player)
    {
        this.player = player;
        this.animHashes = new AnimationHashes();
    }

    public void UpdateAnimations()
    {
        bool isMoving = player.MoveInput.magnitude > 0;

        player.Anim.SetBool(animHashes.IsWalking, isMoving && !player.IsRunning);
        player.Anim.SetBool(animHashes.IsRunning, isMoving && player.IsRunning);
    }

    public void ResetAnimations()
    {
        player.Anim.SetBool(animHashes.IsWalking, false);
        player.Anim.SetBool(animHashes.IsRunning, false);
    }
}

public class AnimationHashes
{
    public readonly int IsWalking = Animator.StringToHash("isWalking");
    public readonly int IsRunning = Animator.StringToHash("isRunning");
    public readonly int IsGrounded = Animator.StringToHash("isGrounded");
    public readonly int Jump = Animator.StringToHash("jump");
    public readonly int YVelocity = Animator.StringToHash("yVelocity");
}