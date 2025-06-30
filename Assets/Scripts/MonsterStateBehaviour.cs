using UnityEngine;

public class MonsterStateBehaviour : StateMachineBehaviour
{
    [Header("���� ����")]
    [SerializeField] private bool isAttackState = false;
    [SerializeField] private bool isHurtState = false;
    [SerializeField] private bool isGroundedState = false;

    private Monster monster;

    // �� ���·� ������ �� ȣ��
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


    // �� ���¿��� �������� �� ȣ��
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (monster == null) return;

        // --- ���� ���� ó�� ---
        if (isAttackState)
        {
            animator.SetBool("isAttacking", false);
        }

    }
}