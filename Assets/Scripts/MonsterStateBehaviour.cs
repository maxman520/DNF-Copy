using UnityEngine;

public class MonsterStateBehaviour : StateMachineBehaviour
{
    [Header("상태 설정")]
    [SerializeField] private bool idle = false;
    [SerializeField] private bool walk = false;
    [SerializeField] private bool attack = false;
    [SerializeField] private bool hurt = false;
    [SerializeField] private bool getUp = false;


    private Monster monster;

    // 이 상태로 진입할 때 호출
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        monster = animator.GetComponentInParent<Monster>();
        if (monster == null)
            return;
        if (idle)
        {
            monster.OnIdleStateEnter();
        }
    }

    // 이 상태에서 빠져나갈 때 호출
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (monster == null) return;
        if (walk)
        {
            monster.OnWalkStateExit();
        }
        if (attack)
        {
            monster.OnAttackStateExit();
        }
        if (hurt)
        {
            monster.OnHurtStateExit();
        }
        if (getUp)
        {
            monster.OnGetUpStateExit();
        }
    }
}
