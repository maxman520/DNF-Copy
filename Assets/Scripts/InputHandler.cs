using UnityEngine;
using UnityEngine.InputSystem;
using System; // Action �̺�Ʈ�� ���� �߰�

public class InputHandler : System.IDisposable
{
    private readonly PlayerInputActions inputActions;
    private bool isDisposed = false;

    // �Է� ���� ������Ƽ
    public Vector2 MoveInput { get; private set; }

    // �̺�Ʈ��
    public event Action<InputAction.CallbackContext> OnRunPerformed;
    public event Action<InputAction.CallbackContext> OnMoveCanceled;
    public event Action<InputAction.CallbackContext> OnJumpPerformed;


    public InputHandler()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();

        // �Է� �׼� �̺�Ʈ�� �߻��ϸ�, �� Ŭ������ public �̺�Ʈ�� ȣ��
        inputActions.Player.Run.performed += ctx => OnRunPerformed?.Invoke(ctx);
        inputActions.Player.Move.canceled += ctx => OnMoveCanceled?.Invoke(ctx);
        inputActions.Player.Jump.performed += ctx => OnJumpPerformed?.Invoke(ctx);
    }

    // Update���� ȣ��� �޼���
    public void ReadInput()
    {
        if (isDisposed) return;

        MoveInput = inputActions.Player.Move.ReadValue<Vector2>();
    }

    // Player�� �ı��� �� ȣ��� ���� �޼���
    public void Dispose()
    {
        if (isDisposed) return;

        inputActions.Player.Disable();
        inputActions.Dispose();
        isDisposed = true;
    }
}