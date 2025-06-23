using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class MoveController
{
    private readonly Player player;
    private readonly InputHandler inputHander;
    private readonly AnimHashes animHashes;
    private Coroutine jumpCoroutine;

    private const float JUMP_MOVEMENT_PENALTY = 0.2f;
    private const float JUMP_DURATION = 1.0f;
    private const float JUMP_HEIGHT = 3.0f;

    public MoveController(Player player, InputHandler inputHandler)
    {
        this.player = player;
        this.inputHander = inputHandler;
        this.animHashes = new AnimHashes();
    }
    public void SubscribeToEvents()
    {
        inputHander.OnRunPerformed += OnRunPerformed;
        inputHander.OnMoveCanceled += OnMoveCanceled;
        inputHander.OnJumpPerformed += OnJumpPerformed;
    }

    public void UnsubscribeFromEvents()
    {
        inputHander.OnRunPerformed -= OnRunPerformed;
        inputHander.OnMoveCanceled -= OnMoveCanceled;
        inputHander.OnJumpPerformed -= OnJumpPerformed;
    }

    // ĳ���� ����, ���� ����
    public void Tick()
    {

        if (!player.IsJumping && Mathf.Abs(inputHander.MoveInput.x) > 0.1f)
        {
            float direction = Mathf.Sign(inputHander.MoveInput.x);
            player.transform.localScale = new Vector3(direction, 1f, 1f);
        }
    }

    // ĳ���� �̵�
    public void ApplyMovement()
    {
        float currentSpeed = player.IsRunning ? player.RunSpeed : player.WalkSpeed;
        Vector2 velocity = inputHander.MoveInput.normalized * currentSpeed;

        if (player.IsJumping)
        {
            velocity.y *= JUMP_MOVEMENT_PENALTY;
        }

        player.Rb.linearVelocity = velocity;
    }

    // �޸��� ����
    private void OnRunPerformed(InputAction.CallbackContext context)
    {
        if (!player.IsJumping) player.IsRunning = true;
    }

    // �޸��� ����
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {        
        player.IsRunning = false;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        StartJump();
    }

    #region ����
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