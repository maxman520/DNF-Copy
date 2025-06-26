using UnityEngine;

public class MonsterStateBehaviour : StateMachineBehaviour
{
    [Header("상태 설정")]
    [SerializeField] private bool isHurtState = false;

    private Monster monster;

    // 이 상태로 진입할 때 호출
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (monster == null)
        {
            monster = animator.GetComponentInParent<Monster>();
        }
        if (monster == null) return;

        // 만약 이 상태가 '피격' 상태라면
        if (isHurtState)
        {
            monster.SetHurtState(true);
        }
    }

    // 이 상태에서 빠져나갈 때 호출
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (monster == null) return;

        // 만약 '피격' 상태에서 빠져나간다면
        if (isHurtState)
        {
            monster.SetHurtState(false); // 피격 상태 해제
        }
    }
}