using UnityEngine;

public class MonsterHitbox : MonoBehaviour
{
    private Vector3 originPosition; // ���� ���� y�� ����� ���� ���� ����
    private AttackDetails attackDetails;

    // �� ���� ���� ��ǿ��� �÷��̾ ���� �� ������ �ʵ��� ����ϴ� bool����
    private bool alreadyHit = false;

    // �ܺο��� �� ��Ʈ�ڽ��� ���� ������ �������ִ� �Լ�
    // origin�� ������ �� ��Ʈ�ڽ��� Position�� �������� y�� ������ ���
    public void Initialize(AttackDetails details, Vector3? origin = null)
    {
        this.attackDetails = details;
        this.originPosition = origin.HasValue ? origin.Value : this.transform.position;
        Debug.Log("Monster Hitbox origin position " + originPosition.x + " " + originPosition.y);

        // ��Ʈ�ڽ��� Ȱ��ȭ�� ������ �ʱ�ȭ
        this.alreadyHit = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerHurtbox"))
        {
            // ���� ������ �������� �ʾҴٸ� �ƹ��͵� ���� ����
            if (attackDetails.attackName == null)
            {
                Debug.Log("���� ������ �������� ����");
                return;
            }
            if (alreadyHit) {
                Debug.Log("�̹� �������Ƿ� ���õ�");
                return;
            } // �̹� ���ȴٸ� ����

            Player player = Player.Instance;

            // ���� ������ Y�� ���� üũ
            if (Mathf.Abs(originPosition.y - player.transform.position.y) >= attackDetails.yOffset)
                return;

            // ���� ������ ���
            alreadyHit = true;

            if (player != null)
            {
                player.OnDamaged(attackDetails, originPosition);
            }
        }

    }
}