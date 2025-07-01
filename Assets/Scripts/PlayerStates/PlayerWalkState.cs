using UnityEngine;

public class PlayerWalkState : PlayerStateBehaviourBase
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
        player.CurrentAnimState = PlayerAnimState.Move;

        player.CanMove = true;
        player.CanAttack = true;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;

        player.CurrentAnimState &= ~PlayerAnimState.Move;
    }
}
