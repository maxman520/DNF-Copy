using UnityEngine;

public class PlayerHurtState : PlayerStateBehaviourBase
{
    // �� ���·� ������ �� ȣ��
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);


        player.CanMove = false;
        player.Rb.linearVelocity = Vector2.zero;
        ResetAttackState();
    }

    // �� ���¿��� �������� �� ȣ��
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);


    }

}
