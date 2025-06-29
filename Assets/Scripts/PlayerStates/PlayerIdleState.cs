using UnityEngine;

public class PlayerIdleState : PlayerStateBehaviourBase
{
    // 이 상태로 진입할 때 호출
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
