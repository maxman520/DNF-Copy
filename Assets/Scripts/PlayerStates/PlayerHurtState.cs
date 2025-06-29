using UnityEngine;

public class PlayerHurtState : PlayerStateBehaviourBase
{
    // 이 상태로 진입할 때 호출
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        InitializePlayer(animator);
        if (player == null)
        {
            Debug.Log("player is NULL");
        }
        player.IsHurt = true;
        player.CanMove = false;
        player.Rb.linearVelocity = Vector2.zero;
        ResetAttackState();
    }

    // 이 상태에서 빠져나갈 때 호출
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player.IsHurt = false;
    }

}
