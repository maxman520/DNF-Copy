using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{

    [Header("HP Gauge")]
    [SerializeField] private Image hpGauge;

    [Header("MP Gauge")]
    [SerializeField] private Image mpGauge;

    [Header("몬스터 UI")]
    [Tooltip("일반 몬스터용 HP바")]
    [SerializeField] private MonsterHPBar monsterHPBar; // 일반 몬스터 HP 바
    [Tooltip("보스 몬스터용 HP바")]
    [SerializeField] private MonsterHPBar bossHPBar; // 보스 몬스터 HP 바

    [Header("스킬창")]
    [SerializeField] private SkillShopUI skillShopUI; // 스킬샵 창

    private Monster currentTarget; // 현재 추적 중인 타겟 몬스터
    private CancellationTokenSource monsterHPBarCts; // 몬스터 HP바 자동 숨김 작업을 위한 토큰
    private void Start()
    {
        // 시작할 때 모든 몬스터 HP바는 숨겨둠
        monsterHPBar?.gameObject.SetActive(false);
        bossHPBar?.gameObject.SetActive(false);
    }

    private void Update()
    {
        // 'K' 키를 누르면 스킬샵 창을 토글
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (skillShopUI != null)
            {
                skillShopUI.ToggleShop();
            }
        }
    }

    // 몬스터가 데미지를 입었을 때 호출될 함수
    public void OnMonsterDamaged(Monster monster)
    {
        // 보스인지 일반 몬스터인지 확인하여 분기 처리
        if (monster.GetMonsterData().isBoss)
        {
            HandleBossDamaged(monster);
        }
        else
        {
            HandleMonsterDamaged(monster);
        }
    }
    #region Boss
    private void HandleBossDamaged(Monster boss)
    {
        // 1. 일반 몬스터 HP바가 켜져 있다면 즉시 숨김
        HideMonsterHPBar();

        // 2. 현재 타겟이 아니거나, 보스 HP바가 꺼져있다면 새로 표시
        if (currentTarget != boss || !bossHPBar.gameObject.activeSelf)
        {
            ShowBossHPBar(boss);
        }

        // 3. HP 업데이트
        UpdateBossHP();
    }
    private void ShowBossHPBar(Monster boss)
    {
        currentTarget = boss;
        bossHPBar.SetFace(boss.GetMonsterData().FaceSprite);
        bossHPBar.gameObject.SetActive(true);
        bossHPBar.Show(currentTarget);
    }

    private void UpdateBossHP()
    {
        if (currentTarget == null) return;
        bossHPBar.UpdateInfo(
            currentTarget.GetName(),
            currentTarget.GetMaxHP(),
            currentTarget.GetPreviousHP(),
            currentTarget.GetCurrentHP(),
            currentTarget.GetHpPerLine()
        );
    }

    // 보스 몬스터 HP바를 숨김
    public void HideBossHPBar()
    {
        if (bossHPBar == null || !bossHPBar.gameObject.activeSelf) return;

        bossHPBar.gameObject.SetActive(false);

        currentTarget = null;
    }
    #endregion Boss 

    #region Regular Monster
    // --- 일반 몬스터 HP 바 처리 ---
    private void HandleMonsterDamaged(Monster monster)
    {
        // 보스 HP바가 활성화되어 있다면, 일반 몬스터 HP바를 표시하지 않음
        if (bossHPBar.gameObject.activeSelf) return;

        // 타겟이 바뀌었거나 HP바가 꺼져있다면 새로 표시
        if (currentTarget != monster || !monsterHPBar.gameObject.activeSelf)
        {
            ShowMonsterHPBar(monster);
        }

        // HP 업데이트 및 자동 숨김 타이머 리셋
        UpdateMonsterHP();
        RestartHideTimer();
    }
    private void ShowMonsterHPBar(Monster monster)
    {
        if (monsterHPBar == null || monster == null) return;

        currentTarget = monster;

        // 몬스터 데이터에서 초상화 가져와서 설정
        monsterHPBar.SetFace(monster.GetMonsterData().FaceSprite);

        monsterHPBar.gameObject.SetActive(true);
        monsterHPBar.Show(currentTarget);
    }
    // 현재 타겟 몬스터의 HP를 업데이트
    public void UpdateMonsterHP()
    {
        if (monsterHPBar == null || currentTarget == null || !monsterHPBar.gameObject.activeSelf) return;

        monsterHPBar.UpdateInfo(
            currentTarget.GetName(),
            currentTarget.GetMaxHP(),
            currentTarget.GetPreviousHP(),
            currentTarget.GetCurrentHP(),
            currentTarget.GetHpPerLine()
        );
    }

    // 몬스터 HP바를 숨김
    public void HideMonsterHPBar()
    {
        if (monsterHPBar == null || !monsterHPBar.gameObject.activeSelf) return;

        monsterHPBar.gameObject.SetActive(false);

        currentTarget = null;
    }

    // 자동 숨김 타이머를 리셋하고 새로 시작
    private void RestartHideTimer()
    {
        monsterHPBarCts?.Cancel(); // 이전 타이머가 있다면 취소
        monsterHPBarCts?.Dispose();
        var destroyToken = this.GetCancellationTokenOnDestroy();
        monsterHPBarCts = CancellationTokenSource.CreateLinkedTokenSource(destroyToken);
        AutoHideAsync(monsterHPBarCts.Token).Forget();
    }

    // 일정 시간 후 HP바를 자동으로 숨기는 비동기 함수
    private async UniTask AutoHideAsync(CancellationToken token)
    {
        try
        {
            // 몬스터 HP바가 화면에 표시될 시간
            await UniTask.Delay((int)(5f * 1000), cancellationToken: token);
            HideMonsterHPBar();
        }
        catch (OperationCanceledException)
        {
            // 타이머가 리셋되면 여기로 들어옴. 정상적인 동작이므로 아무것도 안 함.
        }
    }
    #endregion Regular Monster



    #region Player
    // HP 게이지 업데이트
    public void UpdateHP(float maxHP, float currentHP)
    {
        if (hpGauge != null)
        {
            hpGauge.fillAmount = currentHP / maxHP;
        }
    }

    // MP 게이지 업데이트
    public void UpdateMP(float maxMP, float currentMP)
    {
        if (mpGauge != null)
        {
            mpGauge.fillAmount = currentMP / maxMP;
        }
    }
    #endregion Player
}