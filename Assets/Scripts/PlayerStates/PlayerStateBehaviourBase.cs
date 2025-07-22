using UnityEngine;

public abstract class PlayerStateBehaviourBase : StateMachineBehaviour
{
    [Header("상태 설정")]
    [SerializeField] protected bool Idle = false;
    [SerializeField] protected bool Move = false;
    [SerializeField] protected bool Run = false;
    [SerializeField] protected bool Jump = false;
    [SerializeField] protected bool Attack = false;
    [SerializeField] protected bool Hurt = false;
    [SerializeField] protected bool Airborne = false;

    protected Player player;

    // 이 상태로 진입할 때 호출
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        InitializePlayer(animator);
        if (player == null)
        {
            Debug.Log("player is NULL");
            return;
        }
        SetPlayerAnimState();

    }

    // 이 상태에서 빠져나갈 때 호출
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;

        ResetPlayerAnimState();
    }

    protected virtual void InitializePlayer(Animator animator)
    {
        if (player == null)
            player = animator.GetComponentInParent<Player>();

        if (player == null)
            Debug.LogError("Player component not found on " + animator.name);
    }

    protected void SetPlayerAnimState()
    {
        if (player == null) return;

        if (Idle) player.CurrentAnimState |= PlayerAnimState.Idle;
        if (Move) player.CurrentAnimState |= PlayerAnimState.Move;
        if (Run) player.CurrentAnimState |= PlayerAnimState.Run;
        if (Jump) player.CurrentAnimState |= PlayerAnimState.Jump;
        if (Attack) player.CurrentAnimState |= PlayerAnimState.Attack;
        if (Hurt) player.CurrentAnimState |= PlayerAnimState.Hurt;
        if (Airborne) player.CurrentAnimState |= PlayerAnimState.Airborne;
    }

    protected void ResetPlayerAnimState()
    {
        if (player == null) return;

        if (Idle) player.CurrentAnimState &= ~PlayerAnimState.Idle;
        if (Move) player.CurrentAnimState  &= ~PlayerAnimState.Move;
        if (Run) player.CurrentAnimState &= ~PlayerAnimState.Run;
        if (Jump) player.CurrentAnimState &= ~PlayerAnimState.Jump;
        if (Attack) player.CurrentAnimState &= ~PlayerAnimState.Attack;
        if (Hurt) player.CurrentAnimState &= ~PlayerAnimState.Hurt;
        if (Airborne) player.CurrentAnimState &= ~PlayerAnimState.Airborne;
    }

    protected void ResetAttackState()
    {
        if (player == null) return;
        player.AttackCounter = 0;
        player.CanContinueAttack = false;
    }
}