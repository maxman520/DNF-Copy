using UnityEngine;
using System.Collections.Generic; // ���� ����� ����ϱ� ���� �ʿ�

public class PlayerHitbox : MonoBehaviour
{
    private Player player;

    // �� ���� ���� ��ǿ��� ������ ���� ���� �� ������ �ʵ��� ����ϴ� ����Ʈ
    private List<Collider2D> alreadyHit = new List<Collider2D>();

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }
    private void OnEnable()
    {
        // ���ο� ������ ���۵� ������, ������ ���ȴ� �� ����� �ʱ�ȭ
        alreadyHit.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            // �̹� ���ݿ��� �̹� ���� ������ Ȯ��
            if (alreadyHit.Contains(other))
            {
                return; // �̹� �������� ����
            }

            // ���� ������ ���
            alreadyHit.Add(other);

            // Hurtbox�� �θ𿡰Լ� Monster ������Ʈ�� ã��
            Monster monster = other.GetComponentInParent<Monster>();
            if (monster != null)
            {
                // �÷��̾��� ���ݷ����� ���Ϳ��� �������� ����
                monster.TakeDamage(player.Atk);
            }
        }
    }
}