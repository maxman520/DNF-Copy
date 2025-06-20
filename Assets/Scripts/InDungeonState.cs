using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class InDungeonState : PlayerStateInterface
{
    private readonly Player player;
    private MovementController movementController;
    private AnimationController animationController;

    public InDungeonState(Player player)
    {
        this.player = player ?? throw new System.ArgumentNullException(nameof(player));

        movementController = new MovementController(player);
        animationController = new AnimationController(player);
    }

    public void Enter()
    {
        Debug.Log("던전 상태에 진입");
        player.anim.Play("Idle_Dungeon");
        player.isRunning = false;
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
        player.isRunning = false;
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

    public bool IsJumping => player.isJumping;

    public MovementController(Player player)
    {
        this.player = player;
        this.animHashes = new AnimationHashes();
    }

    public void HandleInput()
    {
        if (player.inputActions.Player.Jump.WasPressedThisFrame())
        {
            StartJump();
        }
    }

    public void SubscribeToEvents()
    {
        player.inputActions.Player.Run.performed += OnRunPerformed;
        player.inputActions.Player.Move.canceled += OnMoveCanceled;
    }

    public void UnsubscribeFromEvents()
    {
        player.inputActions.Player.Run.performed -= OnRunPerformed;
        player.inputActions.Player.Move.canceled -= OnMoveCanceled;
    }

    public void UpdateMovement()
    {
        // 입력 처리
        Vector2 moveInput = player.inputActions.Player.Move.ReadValue<Vector2>();
        player.moveInput = moveInput;

        // 캐릭터 방향 설정
        if (!player.isJumping && Mathf.Abs(player.moveInput.x) > 0.1f)
        {
            float direction = Mathf.Sign(player.moveInput.x);
            player.transform.localScale = new Vector3(direction, 1f, 1f);
        }
    }

    public void ApplyMovement()
    {
        float currentSpeed = player.isRunning ? player.runSpeed : player.walkSpeed;
        Vector2 velocity = player.moveInput.normalized * currentSpeed;

        if (player.isJumping)
        {
            velocity.y *= JUMP_MOVEMENT_PENALTY;
        }

        player.rb.linearVelocity = velocity;
    }

    private void OnRunPerformed(InputAction.CallbackContext context)
    {
        // 달리기 시작
        if (!player.isJumping) player.isRunning = true;
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        // 달리기 중지
        player.isRunning = false;
    }

    public void StartJump()
    {
        if (!player.isGrounded || player.isJumping) return;

        jumpCoroutine = player.StartCoroutineFromState(JumpRoutine());
    }

    public void ForceStopJump()
    {
        if (jumpCoroutine != null)
        {
            player.StopCoroutine(jumpCoroutine);
            jumpCoroutine = null;
            player.isJumping = false;
            player.isGrounded = true;

            if (player.playerGround != null)
                player.playerGround.enabled = true;
        }
    }

    private IEnumerator JumpRoutine()
    {
        // 점프 시작 설정
        player.isGrounded = false;
        player.isJumping = true;

        if (player.playerGround != null)
            player.playerGround.enabled = false;

        player.anim.SetBool(animHashes.IsGrounded, false);
        player.anim.SetTrigger(animHashes.Jump);

        // 점프 중
        float elapsedTime = 0f;
        Vector3 startVisualPos = player.visualsTransform.localPosition;
        float previousHeight = 0f;

        while (elapsedTime < JUMP_DURATION)
        {
            float progress = elapsedTime / JUMP_DURATION;
            float currentHeight = Mathf.Sin(progress * Mathf.PI) * JUMP_HEIGHT;

            // 비주얼 위치 업데이트
            player.visualsTransform.localPosition = new Vector3(startVisualPos.x, currentHeight, startVisualPos.z);

            // 애니메이션 업데이트
            float yVelocity = (currentHeight - previousHeight) / Time.deltaTime;
            player.anim.SetFloat(animHashes.YVelocity, yVelocity);

            previousHeight = currentHeight;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 점프 완료 처리
        player.visualsTransform.localPosition = startVisualPos;

        if (player.playerGround != null)
            player.playerGround.enabled = true;

        player.isJumping = false;
        player.isGrounded = true;
        player.isRunning = false;

        player.anim.SetBool(animHashes.IsGrounded, true);
        player.anim.SetFloat(animHashes.YVelocity, 0);

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
        bool isMoving = player.moveInput.magnitude > 0;

        player.anim.SetBool(animHashes.IsWalking, isMoving && !player.isRunning);
        player.anim.SetBool(animHashes.IsRunning, isMoving && player.isRunning);
    }

    public void ResetAnimations()
    {
        player.anim.SetBool(animHashes.IsWalking, false);
        player.anim.SetBool(animHashes.IsRunning, false);
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