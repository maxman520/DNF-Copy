using UnityEngine;

public class PlayerHurtState : PlayerStateBehaviourBase
{
    // �� ���·� ������ �� ȣ��
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

    // �� ���¿��� �������� �� ȣ��
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player.IsHurt = false;
    }

}
