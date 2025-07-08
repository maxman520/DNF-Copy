using UnityEngine.InputSystem;

public interface ICommand
{
    // 실행에 성공하면 true, 조건이 안 맞아 실패하면 false를 반환
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
