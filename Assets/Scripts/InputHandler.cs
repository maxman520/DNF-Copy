using UnityEngine;
using UnityEngine.InputSystem;
using System; // Action 이벤트를 위해 추가

public class InputHandler : System.IDisposable
{
    private readonly PlayerInputActions inputActions;
    private bool isDisposed = false;

    // 입력 상태 프로퍼티
    public Vector2 MoveInput { get; private set; }

    // 이벤트들
    public event Action<InputAction.CallbackContext> OnMoveCanceled;
    public event Action<InputAction.CallbackContext> OnRunPerformed;
    public event Action<InputAction.CallbackContext> OnJumpPerformed;
    public event Action<InputAction.CallbackContext> OnAttackPerformed;

    public InputHandler()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();

        // 입력 액션 이벤트가 발생하면, 이 클래스의 이벤트를 호출
        inputActions.Player.Move.canceled += ctx => OnMoveCanceled?.Invoke(ctx);
        inputActions.Player.Run.performed += ctx => OnRunPerformed?.Invoke(ctx);
        inputActions.Player.Jump.performed += ctx => OnJumpPerformed?.Invoke(ctx);
        inputActions.Player.Attack.performed += ctx => OnAttackPerformed?.Invoke(ctx);
    }

    // Update에서 호출될 메서드
    public void ReadInput()
    {
        if (isDisposed) return;

        MoveInput = inputActions.Player.Move.ReadValue<Vector2>();

        return;
    }


    // Player가 파괴될 때 호출될 정리 메서드
    public void Dispose()
    {
        if (isDisposed) return;

        inputActions.Player.Disable();
        inputActions.Dispose();
        isDisposed = true;
    }
}