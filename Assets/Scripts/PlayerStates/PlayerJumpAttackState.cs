using UnityEngine;

public class PlayerJumpAttackState : PlayerStateBehaviourBase
{
    [SerializeField] private int AttackDetailsIndex;

    // 이 상태로 진입할 때 호출
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        player.CanAttack = false;
        player.SetAttackDetails(AttackDetailsIndex);
    }

    // 이 상태에서 빠져나갈 때 호출
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
    }

}
