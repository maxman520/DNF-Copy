using UnityEngine;
using UnityEngine.InputSystem;
using System; // Action �̺�Ʈ�� ���� �߰�
using System.Collections.Generic;

public class InputHandler : System.IDisposable
{
    private readonly PlayerInputActions inputActions;
    private readonly InputBuffer inputBuffer;
    private bool isDisposed = false;

    // �Է� ���� ������Ƽ
    public Vector2 MoveInput;

    // �̺�Ʈ
    public event Action<InputAction.CallbackContext> OnRunPerformed;

    public InputHandler()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        inputBuffer = new InputBuffer(0.3f); // 0.3�� ���� �Է��� ����ϵ��� ���� ����


        // �Է� �׼� �̺�Ʈ�� �߻��ϸ�, �� Ŭ������ �̺�Ʈ�� ȣ��
        inputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => { 
            MoveInput = Vector2.zero;
            Player.Instance.IsRunning = false;
        };
        inputActions.Player.Run.performed += ctx => OnRunPerformed?.Invoke(ctx);
        inputActions.Player.Jump.performed += ctx => inputBuffer.AddCommand(new JumpCommand(ctx));
        inputActions.Player.Attack.performed += ctx => inputBuffer.AddCommand(new AttackCommand(ctx));

    }
    // ������ �� �� Ŀ�ǵ� 'Ȯ��'
    public ICommand PeekCommand()
    {
        return inputBuffer.PeekCommand();
    }

    // ������ �� �� Ŀ�ǵ� ����
    public void RemoveCommand()
    {
        inputBuffer.RemoveCommand();
    }
    // ���� ������ ��û
    public List<string> GetBufferedCommandNames()
    {
        return inputBuffer.GetBufferedCommandNames();
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