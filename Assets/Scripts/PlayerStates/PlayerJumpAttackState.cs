using UnityEngine;

public class PlayerJumpAttackState : PlayerStateBehaviourBase
{
    // �� ���·� ������ �� ȣ��
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        player.CanAttack = false;
    }

    // �� ���¿��� �������� �� ȣ��
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
    }

}
