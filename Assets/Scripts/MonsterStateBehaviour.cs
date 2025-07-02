using UnityEngine;

public class MonsterStateBehaviour : StateMachineBehaviour
{
    [Header("���� ����")]
    [SerializeField] private bool idle = false;
    [SerializeField] private bool walk = false;
    [SerializeField] private bool attack = false;
    [SerializeField] private bool hurt = false;
    [SerializeField] private bool getUp = false;


    private Monster monster;

    // �� ���·� ������ �� ȣ��
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

    // �� ���¿��� �������� �� ȣ��
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
