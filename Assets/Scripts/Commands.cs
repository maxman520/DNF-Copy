using UnityEngine.InputSystem;

public interface ICommand {
    bool Execute(BehaviourController behaviourController, SkillManager skillManager);
}

public class JumpCommand : ICommand
{
    InputAction.CallbackContext context;
    public JumpCommand (InputAction.CallbackContext context) {
        this.context = context;
    }
    public bool Execute(BehaviourController behaviourController, SkillManager skillManager)
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
    public bool Execute(BehaviourController behaviourController, SkillManager skillManager)
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
    public bool Execute(BehaviourController behaviourController, SkillManager skillManager)
    {
        // skillManager�� ��ų ���� ������ ȣ��
        return skillManager.TryExecuteSkill(skillIndex);
    }
}