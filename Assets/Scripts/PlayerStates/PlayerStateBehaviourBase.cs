using UnityEngine;

public abstract class PlayerStateBehaviourBase : StateMachineBehaviour
{
    protected Player player;

    protected virtual void InitializePlayer(Animator animator)
    {
        if (player == null)
            player = animator.GetComponentInParent<Player>();

        if (player == null)
            Debug.LogError("Player component not found on " + animator.name);
    }

    protected void ResetAttackState()
    {
        if (player == null) return;
        player.AttackCounter = 0;
        player.CanContinueAttack = false;
    }
}