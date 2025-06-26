using UnityEngine;

public class PlayerStateBehaviour : StateMachineBehaviour
{
    [Header("���� ����")]
    [SerializeField] private bool isIdleState = false; // Idle ����
    [SerializeField] private bool isAttackingState = false; // ����
    [SerializeField] private bool isJumpAttackingState = false; // ���� ����
    [SerializeField] private bool isHurtState = false;      // �ǰ�

    private Player player;

    // �� ���·� ������ �� ȣ��
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

    // �� ���¿��� �������� �� ȣ��
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;
        if (isAttackingState) player.IsAttacking = false;
        if (isJumpAttackingState) player.IsJumpAttacking = false;
        if (isHurtState) player.IsHurt = false;
    }
    // ���� ���� ���� �ʱ�ȭ
    public void ResetAttackState()
    {
        player.AttackCounter = 0;
        player.CanContinueAttack = false;
    }

}
