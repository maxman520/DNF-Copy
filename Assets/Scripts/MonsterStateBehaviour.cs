using UnityEngine;

public class MonsterStateBehaviour : StateMachineBehaviour
{
    [Header("상태 설정")]
    [SerializeField] private bool isAttackState = false;
    [SerializeField] private bool isHurtState = false;
    [SerializeField] private bool isGroundedState = false;

    private Monster monster;

    // 이 상태로 진입할 때 호출
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (monster == null) monster = animator.GetComponentInParent<Monster>();
        if (monster == null) return;
        if (isAttackState)
        {
            animator.SetBool("isAttacking", true);
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (isGroundedState)
        {
            animator.SetBool("isGrounded", true);
        } else
        {
            animator.SetBool("isGrounded", false);
        }
    }


    // 이 상태에서 빠져나갈 때 호출
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (monster == null) return;

        // --- 상태 종료 처리 ---
        if (isAttackState)
        {
            animator.SetBool("isAttacking", false);
        }

    }
}