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
    // ���� ������ ���� Ŀ�ǵ带 ���ۿ� �߰�
    private void AddCommandIfInDungeon(ICommand command)
    {
        // GameManager�� �����ϰ�, ���� ���°� Dungeon�� ���� ����
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Dungeon)
        {
            inputBuffer.AddCommand(command);
        }
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