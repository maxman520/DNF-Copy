using UnityEngine;

public class PlayerStateBehaviour : StateMachineBehaviour
{
    [Header("상태 설정")]
    [SerializeField] private bool isIdleState = false; // Idle 상태
    [SerializeField] private bool isAttackingState = false; // 공격
    [SerializeField] private bool isJumpAttackingState = false; // 점프 공격
    [SerializeField] private bool isHurtState = false;      // 피격

    private Player player;

    // 이 상태로 진입할 때 호출
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null)
        {
            player = animator.GetComponentInParent<Player>();
        }
        if (player == null) return;

        if (isIdleState) ResetAttackState();
        if (isAttackingState) player.IsAttacking = true;
        if (isJumpAttackingState) player.IsJumpAttacking = true;
        if (isHurtState)
        {
            player.IsHurt = true;
            ResetAttackState();
        }
    }

    // 이 상태에서 빠져나갈 때 호출
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;
        if (isAttackingState) player.IsAttacking = false;
        if (isJumpAttackingState) player.IsJumpAttacking = false;
        if (isHurtState) player.IsHurt = false;
    }
    // 공격 관련 변수 초기화
    public void ResetAttackState()
    {
        player.AttackCounter = 0;
        player.CanContinueAttack = false;
    }

}
