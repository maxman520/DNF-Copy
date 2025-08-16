using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 캐릭터 정보창 UI에 부착할 스크립트
public class ProfilePanel : MonoBehaviour
{
    [Header("기본 정보 텍스트")]
    [SerializeField] private TextMeshProUGUI levelAndNicknameText;

    [Header("스탯/자원 텍스트")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI mpText;
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI defText;
    [SerializeField] private TextMeshProUGUI moveSpeedText;


    private Player player;
    private Inventory inventory;

    private void OnEnable()
    {
        UIManager.Instance?.OpenUI(this.gameObject);
        CacheRefs();
        Subscribe();
        RefreshAll();
    }

    private void Start()
    {
        // 씬 초기화 타이밍을 고려하여 안전하게 한 번 더 캐시 시도
        if (player == null || inventory == null)
        {
            CacheRefs();
            Subscribe();
            RefreshAll();
        }
    }

    private void OnDisable()
    {
        UIManager.Instance?.CloseUI(this.gameObject);
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void CacheRefs()
    {
        player = Player.Instance;
        if (player != null)
        {
            inventory = player.PlayerInventory;
        }
    }

    private void Subscribe()
    {
        if (player != null)
        {
            player.OnHPChanged += OnHPChanged;
            player.OnMPChanged += OnMPChanged;
            // 플레이어 레벨 변경 시 프로필 텍스트 갱신
            player.OnLevelChanged += OnLevelChanged;
        }
        if (inventory != null)
        {
            inventory.OnInventoryChanged += OnInventoryChanged;
        }
    }

    private void Unsubscribe()
    {
        if (player != null)
        {
            player.OnHPChanged -= OnHPChanged;
            player.OnMPChanged -= OnMPChanged;
            player.OnLevelChanged -= OnLevelChanged;
        }
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= OnInventoryChanged;
        }
    }

    // 전체 UI를 갱신
    private void RefreshAll()
    {
        UpdateIdentityTexts();
        if (player != null)
        {
            OnHPChanged(player.MaxHP, player.CurrentHP);
            OnMPChanged(player.MaxMP, player.CurrentMP);
        }
        UpdateStatsAndCurrency();
    }

    private void UpdateIdentityTexts()
    {
        // 선택 캐릭터의 닉네임은 DataManager에서, 레벨은 Player의 현재 레벨을 사용
        var data = DataManager.Instance != null ? DataManager.Instance.SelectedCharacter : null;
        int level = player != null ? player.Level : (data != null ? data.Level : 1);
        string name = data != null ? data.CharacterName : "";
        if (levelAndNicknameText != null)
            levelAndNicknameText.text = $"Lv. {level} {name}";
    }

    private void UpdateStatsAndCurrency()
    {
        if (player != null)
        {
            if (atkText != null) atkText.text = Mathf.RoundToInt(player.Atk).ToString();
            if (defText != null) defText.text = Mathf.RoundToInt(player.Def).ToString();
        }
    }

    // --- 이벤트 핸들러 ---
    private void OnHPChanged(float maxHP, float currentHP)
    {
        if (hpText == null) return;
        int c = Mathf.RoundToInt(currentHP);
        int m = Mathf.RoundToInt(maxHP);
        hpText.text = $"{c.ToString()} / {m.ToString()}";
    }

    private void OnMPChanged(float maxMP, float currentMP)
    {
        if (mpText == null) return;
        int c = Mathf.RoundToInt(currentMP);
        int m = Mathf.RoundToInt(maxMP);
        mpText.text = $"{c.ToString()} / {m.ToString()}";
    }

    private void OnInventoryChanged()
    {
        UpdateStatsAndCurrency();
    }

    // 레벨 변경 이벤트 핸들러: 프로필 상단 텍스트 즉시 반영
    private void OnLevelChanged(int newLevel)
    {
        UpdateIdentityTexts();
    }
}
