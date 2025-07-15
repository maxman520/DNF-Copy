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
        // behaviourController의 점프 로직을 호출
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
        // behaviourController의 공격 로직을 호출
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
        // skillManager의 스킬 시전 로직을 호출
        return skillManager.TryExecuteSkill(skillIndex);
    }
}