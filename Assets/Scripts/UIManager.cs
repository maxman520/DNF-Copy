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

    [Header("���� UI")]
    [Tooltip("�Ϲ� ���Ϳ� HP��")]
    [SerializeField] private MonsterHPBar monsterHPBar; // �Ϲ� ���� HP ��
    [Tooltip("���� ���Ϳ� HP��")]
    [SerializeField] private MonsterHPBar bossHPBar; // ���� ���� HP ��

    [Header("��ųâ")]
    [SerializeField] private SkillShopUI skillShopUI; // ��ų�� â

    private Monster currentTarget; // ���� ���� ���� Ÿ�� ����
    private CancellationTokenSource monsterHPBarCts; // ���� HP�� �ڵ� ���� �۾��� ���� ��ū
    private void Start()
    {
        // ������ �� ��� ���� HP�ٴ� ���ܵ�
        monsterHPBar?.gameObject.SetActive(false);
        bossHPBar?.gameObject.SetActive(false);
    }

    private void Update()
    {
        // 'K' Ű�� ������ ��ų�� â�� ���
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (skillShopUI != null)
            {
                skillShopUI.ToggleShop();
            }
        }
    }

    // ���Ͱ� �������� �Ծ��� �� ȣ��� �Լ�
    public void OnMonsterDamaged(Monster monster)
    {
        // �������� �Ϲ� �������� Ȯ���Ͽ� �б� ó��
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
        // 1. �Ϲ� ���� HP�ٰ� ���� �ִٸ� ��� ����
        HideMonsterHPBar();

        // 2. ���� Ÿ���� �ƴϰų�, ���� HP�ٰ� �����ִٸ� ���� ǥ��
        if (currentTarget != boss || !bossHPBar.gameObject.activeSelf)
        {
            ShowBossHPBar(boss);
        }

        // 3. HP ������Ʈ
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

    // ���� ���� HP�ٸ� ����
    public void HideBossHPBar()
    {
        if (bossHPBar == null || !bossHPBar.gameObject.activeSelf) return;

        bossHPBar.gameObject.SetActive(false);

        currentTarget = null;
    }
    #endregion Boss 

    #region Regular Monster
    // --- �Ϲ� ���� HP �� ó�� ---
    private void HandleMonsterDamaged(Monster monster)
    {
        // ���� HP�ٰ� Ȱ��ȭ�Ǿ� �ִٸ�, �Ϲ� ���� HP�ٸ� ǥ������ ����
        if (bossHPBar.gameObject.activeSelf) return;

        // Ÿ���� �ٲ���ų� HP�ٰ� �����ִٸ� ���� ǥ��
        if (currentTarget != monster || !monsterHPBar.gameObject.activeSelf)
        {
            ShowMonsterHPBar(monster);
        }

        // HP ������Ʈ �� �ڵ� ���� Ÿ�̸� ����
        UpdateMonsterHP();
        RestartHideTimer();
    }
    private void ShowMonsterHPBar(Monster monster)
    {
        if (monsterHPBar == null || monster == null) return;

        currentTarget = monster;

        // ���� �����Ϳ��� �ʻ�ȭ �����ͼ� ����
        monsterHPBar.SetFace(monster.GetMonsterData().FaceSprite);

        monsterHPBar.gameObject.SetActive(true);
        monsterHPBar.Show(currentTarget);
    }
    // ���� Ÿ�� ������ HP�� ������Ʈ
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

    // ���� HP�ٸ� ����
    public void HideMonsterHPBar()
    {
        if (monsterHPBar == null || !monsterHPBar.gameObject.activeSelf) return;

        monsterHPBar.gameObject.SetActive(false);

        currentTarget = null;
    }

    // �ڵ� ���� Ÿ�̸Ӹ� �����ϰ� ���� ����
    private void RestartHideTimer()
    {
        monsterHPBarCts?.Cancel(); // ���� Ÿ�̸Ӱ� �ִٸ� ���
        monsterHPBarCts?.Dispose();
        var destroyToken = this.GetCancellationTokenOnDestroy();
        monsterHPBarCts = CancellationTokenSource.CreateLinkedTokenSource(destroyToken);
        AutoHideAsync(monsterHPBarCts.Token).Forget();
    }

    // ���� �ð� �� HP�ٸ� �ڵ����� ����� �񵿱� �Լ�
    private async UniTask AutoHideAsync(CancellationToken token)
    {
        try
        {
            // ���� HP�ٰ� ȭ�鿡 ǥ�õ� �ð�
            await UniTask.Delay((int)(5f * 1000), cancellationToken: token);
            HideMonsterHPBar();
        }
        catch (OperationCanceledException)
        {
            // Ÿ�̸Ӱ� ���µǸ� ����� ����. �������� �����̹Ƿ� �ƹ��͵� �� ��.
        }
    }
    #endregion Regular Monster



    #region Player
    // HP ������ ������Ʈ
    public void UpdateHP(float maxHP, float currentHP)
    {
        if (hpGauge != null)
        {
            hpGauge.fillAmount = currentHP / maxHP;
        }
    }

    // MP ������ ������Ʈ
    public void UpdateMP(float maxMP, float currentMP)
    {
        if (mpGauge != null)
        {
            mpGauge.fillAmount = currentMP / maxMP;
        }
    }
    #endregion Player
}