using UnityEngine;

public class MonsterHitbox : MonoBehaviour
{
    // 이 Hitbox의 주인
    private Monster ownerMonster;

    private void Awake()
    {
        ownerMonster = GetComponentInParent<Monster>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (ownerMonster == null) return;

            Player player = Player.Instance;

            if (player != null)
            {
                player.TakeDamage(ownerMonster.GetAtk());
            }
        }
    }
}