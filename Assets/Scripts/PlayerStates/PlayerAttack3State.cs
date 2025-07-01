using UnityEngine;

public class PlayerAttack3State : PlayerStateBehaviourBase
{
    // �� ���·� ������ �� ȣ��
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        InitializePlayer(animator);
        if (player == null)
        {
            Debug.Log("player is NULL");
            return;
        }
        player.CurrentAnimState = PlayerAnimState.Attack;

        player.CanMove = false;
    }

    // �� ���¿��� �������� �� ȣ��
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;
        player.CurrentAnimState &= ~PlayerAnimState.Attack;

    }

}
