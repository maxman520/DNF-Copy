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
        inputActions.Player.Run.performed += ctx => AddCommandIfInDungeon(new RunCommand(ctx));
        inputActions.Player.Jump.performed += ctx => AddCommandIfInDungeon(new JumpCommand(ctx));
        inputActions.Player.Attack.performed += ctx => AddCommandIfInDungeon(new AttackCommand(ctx));
        inputActions.Player.SkillSlot_1.performed += ctx => AddCommandIfInDungeon(new SkillCommand(ctx, 0));
        inputActions.Player.SkillSlot_2.performed += ctx => AddCommandIfInDungeon(new SkillCommand(ctx, 1));
        inputActions.Player.SkillSlot_3.performed += ctx => AddCommandIfInDungeon(new SkillCommand(ctx, 2));
        inputActions.Player.SkillSlot_4.performed += ctx => AddCommandIfInDungeon(new SkillCommand(ctx, 3));
        inputActions.Player.SkillSlot_5.performed += ctx => AddCommandIfInDungeon(new SkillCommand(ctx, 4));
        inputActions.Player.SkillSlot_6.performed += ctx => AddCommandIfInDungeon(new SkillCommand(ctx, 5));
        inputActions.Player.SkillSlot_7.performed += ctx => AddCommandIfInDungeon(new SkillCommand(ctx, 6));
        inputActions.Player.SkillSlot_8.performed += ctx => AddCommandIfInDungeon(new SkillCommand(ctx, 7));
        inputActions.Player.SkillSlot_9.performed += ctx => AddCommandIfInDungeon(new SkillCommand(ctx, 8));
        inputActions.Player.SkillSlot_10.performed += ctx => AddCommandIfInDungeon(new SkillCommand(ctx, 9));
        inputActions.Player.SkillSlot_11.performed += ctx => AddCommandIfInDungeon(new SkillCommand(ctx, 10));
        inputActions.Player.SkillSlot_12.performed += ctx => AddCommandIfInDungeon(new SkillCommand(ctx, 11));
        inputActions.Player.SkillSlot_13.performed += ctx => AddCommandIfInDungeon(new SkillCommand(ctx, 12));
        inputActions.Player.SkillSlot_14.performed += ctx => AddCommandIfInDungeon(new SkillCommand(ctx, 13));

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
    // 던전 상태일 때만 커맨드를 버퍼에 추가
    private void AddCommandIfInDungeon(ICommand command)
    {
        // GameManager가 존재하고, 현재 상태가 Dungeon일 때만 실행
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Dungeon)
        {
            inputBuffer.AddCommand(command);
        }
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