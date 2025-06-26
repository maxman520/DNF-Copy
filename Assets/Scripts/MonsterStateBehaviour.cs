using UnityEngine;

public class MonsterStateBehaviour : StateMachineBehaviour
{
    [Header("���� ����")]
    [SerializeField] private bool isHurtState = false;

    private Monster monster;

    // �� ���·� ������ �� ȣ��
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (monster == null)
        {
            monster = animator.GetComponentInParent<Monster>();
        }
        if (monster == null) return;

        // ���� �� ���°� '�ǰ�' ���¶��
        if (isHurtState)
        {
            monster.SetHurtState(true);
        }
    }

    // �� ���¿��� �������� �� ȣ��
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (monster == null) return;

        // ���� '�ǰ�' ���¿��� ���������ٸ�
        if (isHurtState)
        {
            monster.SetHurtState(false); // �ǰ� ���� ����
        }
    }
}