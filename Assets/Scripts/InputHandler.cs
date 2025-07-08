using UnityEngine;
using UnityEngine.InputSystem;
using System; // Action 이벤트를 위해 추가
using System.Collections.Generic;

public class InputHandler : System.IDisposable
{
    private readonly PlayerInputActions inputActions;
    private readonly InputBuffer inputBuffer;
    private bool isDisposed = false;

    // 입력 상태 프로퍼티
    public Vector2 MoveInput;

    // 이벤트
    public event Action<InputAction.CallbackContext> OnRunPerformed;

    public InputHandler()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        inputBuffer = new InputBuffer(0.3f); // 0.3초 동안 입력을 기억하도록 버퍼 생성


        // 입력 액션 이벤트가 발생하면, 이 클래스의 이벤트를 호출
        inputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => { 
            MoveInput = Vector2.zero;
            Player.Instance.IsRunning = false;
        };
        inputActions.Player.Run.performed += ctx => OnRunPerformed?.Invoke(ctx);
        inputActions.Player.Jump.performed += ctx => inputBuffer.AddCommand(new JumpCommand(ctx));
        inputActions.Player.Attack.performed += ctx => inputBuffer.AddCommand(new AttackCommand(ctx));

    }
    // 버퍼의 맨 위 커맨드 '확인'
    public ICommand PeekCommand()
    {
        return inputBuffer.PeekCommand();
    }

    // 버퍼의 맨 위 커맨드 제거
    public void RemoveCommand()
    {
        inputBuffer.RemoveCommand();
    }
    // 버퍼 내용을 요청
    public List<string> GetBufferedCommandNames()
    {
        return inputBuffer.GetBufferedCommandNames();
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