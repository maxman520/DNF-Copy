using UnityEngine;

public class PlayerRunState : PlayerStateBehaviourBase
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
        player.CurrentAnimState = PlayerAnimState.Run | PlayerAnimState.Move;

        player.CanMove = true;
        player.CanAttack = true;
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;

        player.CurrentAnimState &= ~(PlayerAnimState.Run | PlayerAnimState.Move);
    }
}
