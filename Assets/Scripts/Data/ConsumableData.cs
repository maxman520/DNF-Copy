using UnityEngine;

// 소비 아이템 효과 종류
public enum ConsumableEffectType
{
    HealHP,
    HealMP
    // ... 버프, 디버프 제거 등 추가 가능
}

[CreateAssetMenu(fileName = "New Consumable Data", menuName = "Data/Consumable Data")]
public class ConsumableData : ItemData
{
    [Header("소비 아이템 정보")]
    public ConsumableEffectType EffectType;
    public int EffectValue; // 효과량 (예: 회복량)

    public ConsumableData()
    {
        ItemKind = Kind.Consume;
    }

    // 아이템 사용 로직
    public void Use()
    {
        // 플레이어를 찾아서 효과 적용
        Player player = FindFirstObjectByType<Player>();
        if (player == null) return;

        switch (EffectType)
        {
            case ConsumableEffectType.HealHP:
                player.HealHP(EffectValue);
                Debug.Log($"{ItemName}을(를) 사용하여 HP를 {EffectValue}만큼 회복했습니다.");
                break;
            case ConsumableEffectType.HealMP:
                player.HealMP(EffectValue);
                Debug.Log($"{ItemName}을(를) 사용하여 MP를 {EffectValue}만큼 회복했습니다.");
                break;
        }
    }
}
