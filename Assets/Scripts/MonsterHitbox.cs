using UnityEngine;

public class MonsterHitbox : MonoBehaviour
{
    // �� Hitbox�� ����
    private Monster ownerMonster;
    private Player player;

    private void Awake()
    {
        ownerMonster = GetComponentInParent<Monster>();
        player = Player.Instance;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (ownerMonster == null) return;

            if (player != null)
            {
                player.OnDamaged(ownerMonster.GetAtk());
            }
        }
    }
}