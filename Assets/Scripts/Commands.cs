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
        // behaviourController의 점프 로직을 호출
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
    public bool Execute(BehaviourController behaviourController)
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
    public bool Execute(BehaviourController behaviourController)
    {
        // behaviourController의 스킬 시전 로직을 호출
        return behaviourController.PerformSkill(context, skillIndex);
    }
}

public class QuickSlotCommand : ICommand
{
    InputAction.CallbackContext context;
    private int quickSlotIndex;
    public QuickSlotCommand(InputAction.CallbackContext context, int quickSlotIndex)
    {
        this.context = context;
        this.quickSlotIndex = quickSlotIndex;
    }
    public bool Execute(BehaviourController behaviourController)
    {
        // behaviourController의 스킬 시전 로직을 호출
        return behaviourController.PerformQuickSlot(context, quickSlotIndex);
    }
}