using UnityEngine;

public static class HitSfxRouter
{
    // 플레이어가 몬스터를 때렸을 때 임팩트 SFX 선택
    public static void PlayImpact_PlayerToMonster(Player player, Monster monster, AttackDetails details)
    {
        string key = null;

        // AttackDetails.kind가 Weapon이면 장착 무기 종류를 따라가고,
        // 그 외에는 AttackDetails.kind를 그대로 사용
        AttackKind effectiveKind = details.kind;
        if (details.kind == AttackKind.Weapon)
        {
            var inventory = player?.PlayerInventory;
            WeaponKind weaponKind = WeaponKind.None;
            if (inventory != null && inventory.EquippedItems != null && inventory.EquippedItems.TryGetValue(EquipmentType.Weapon, out var equip) && equip is WeaponData w)
            {
                weaponKind = w.WeaponKind;
            }

            switch (weaponKind)
            {
                case WeaponKind.Blunt:  effectiveKind = AttackKind.Blunt; break;
                case WeaponKind.Katana: effectiveKind = AttackKind.Katana; break;
                case WeaponKind.Sword:  effectiveKind = AttackKind.Slash; break;
                case WeaponKind.BeamSword:  effectiveKind = AttackKind.BeamSword; break;
                default:                effectiveKind = AttackKind.Blunt; break; // 무기를 장착하지 않은 상태에선 기본 Blunt
            }
        }

        switch (effectiveKind)
        {
            case AttackKind.Blunt:
                {
                    // 둔기 히트: 지정된 5개 키 중 랜덤 (Sticka_Hit_01 2회로 가중치)
                    var pool = new string[] { "Sticka_Hit_01", "Sticka_Hit_01", "Stickb_Hit_01", "Stickc_Hit_01", "Stickc_Hit_02" };
                    int idx = Mathf.FloorToInt(Random.value * pool.Length);
                    if (idx < 0 || idx >= pool.Length) idx = 0;
                    key = pool[idx];
                }
                break;
            case AttackKind.Katana:
                {
                    // 카타나 히트: 지정된 4가지 키 중 랜덤
                    var pool = new string[] { "Kata_Hit_01", "Kata_Hit_02", "Katb_Hit", "Katc_Hit_01" };
                    int idx = Mathf.FloorToInt(Random.value * pool.Length);
                    if (idx < 0 || idx >= pool.Length) idx = 0;
                    key = pool[idx];
                }
                break;
            case AttackKind.Slash:
                break;
            case AttackKind.Pierce:
                break;
            case AttackKind.Magic:
                break;
            case AttackKind.Explosion:
                break;
            default:
                break;
        }

        if (!string.IsNullOrEmpty(key))
        {
            AudioManager.Instance.PlaySFX(key);
        }
    }

    // 몬스터가 플레이어를 때렸을 때 임팩트 SFX 선택
    public static void PlayImpact_MonsterToPlayer(Monster monster, Player player, AttackDetails details)
    {
        string key = null;
        switch (details.kind)
        {
            case AttackKind.Blunt:
                {
                    // 둔기 히트: 지정된 5개 키 중 랜덤 (Sticka_Hit_01 2회로 가중치)
                    var pool = new string[] { "Sticka_Hit_01", "Sticka_Hit_01", "Stickb_Hit_01", "Stickc_Hit_01", "Stickc_Hit_02" };
                    int idx = Mathf.FloorToInt(Random.value * pool.Length);
                    if (idx < 0 || idx >= pool.Length) idx = 0;
                    key = pool[idx];
                }
                break;
            case AttackKind.Katana: // 카타나는 베기 계열로 매핑
            case AttackKind.Slash:
                break;
            case AttackKind.Pierce:
                break;
            case AttackKind.Magic:
                break;
            case AttackKind.Explosion:
                break;
            default:
                key = "Pub_Hit_01";
                break;
        }

        if (!string.IsNullOrEmpty(key))
        {
            AudioManager.Instance.PlaySFX(key);
        }
    }
}
