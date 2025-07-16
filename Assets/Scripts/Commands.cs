using UnityEngine.InputSystem;

public interface ICommand {
    bool Execute(BehaviourController behaviourController);
}

public class RunCommand : ICommand
{
    InputAction.CallbackContext context;
    public RunCommand(InputAction.CallbackContext context)
    {
        this.context = context;
    }
    public bool Execute(BehaviourController behaviourController)
    {
        // behaviourController�� ���� ������ ȣ��
        return behaviourController.PerformRun(context);
    }
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

public class SkillCommand : ICommand
{
    InputAction.CallbackContext context;
    private int skillIndex;
    public SkillCommand(InputAction.CallbackContext context, int skillIndex)
    {
        this.context = context;
        this.skillIndex = skillIndex;
    }
    public bool Execute(BehaviourController behaviourController)
    {
        // behaviourController�� ��ų ���� ������ ȣ��
        return behaviourController.PerformSkill(context, skillIndex);
    }
}