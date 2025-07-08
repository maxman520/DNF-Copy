using UnityEngine.InputSystem;

public interface ICommand
{
    // ���࿡ �����ϸ� true, ������ �� �¾� �����ϸ� false�� ��ȯ
    bool Execute(BehaviourController behaviourController);
}

public class JumpCommand : ICommand
{
    InputAction.CallbackContext context;
    public JumpCommand (InputAction.CallbackContext context) {
        this.context = context;
    }
    public bool Execute(BehaviourController behaviourController)
    {
        // behaviourController�� ���� ������ ȣ��
        return behaviourController.PerformJump(context);
    }
}

public class AttackCommand : ICommand
{
    InputAction.CallbackContext context;
    public AttackCommand(InputAction.CallbackContext context)
    {
        this.context = context;
    }
    public bool Execute(BehaviourController behaviourController)
    {
        // behaviourController�� ���� ������ ȣ��
        return behaviourController.PerformAttack(context);
    }
}
