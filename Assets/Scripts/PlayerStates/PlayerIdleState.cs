using UnityEngine;

public class PlayerIdleState : PlayerStateBehaviourBase
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
        ResetAttackState();
        player.CanMove = true;
        player.CanAttack = true;
    }
}
