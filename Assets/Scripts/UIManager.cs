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

    [Header("���� HP ��")]
    [SerializeField] private MonsterHPBar monsterHPBar;
    
    private Monster currentTargetMonster; // ���� ���� HP�ٰ� �����ϴ� ����

    private CancellationTokenSource hpBarCts; // ���� HP�� �ڵ� ���� �۾��� ���� ��ū
    private void Start()
    {
        // ������ �� ���� HP�ٴ� ���ܵ�
        if (monsterHPBar != null)
        {
            monsterHPBar.gameObject.SetActive(false);
        }
    }

    // ���Ͱ� �������� �Ծ��� �� ȣ��� �Լ�
    public void OnMonsterDamaged(Monster monster)
    {
        // Ÿ���� �ٲ���ų�, HP�ٰ� ��Ȱ��ȭ ���¶�� ���� ǥ��
        if (currentTargetMonster != monster || !monsterHPBar.gameObject.activeSelf)
        {
            ShowMonsterHPBar(monster);
            UpdateMonsterHP();
        }
        else // ���� Ÿ���� ��� ������ �ִٸ�
        {
            // HP�� ������Ʈ�ϰ�, �ڵ� ���� Ÿ�̸Ӹ� ����
            UpdateMonsterHP();
            ResetHideTimer();
        }
    }
    // ���ο� ���͸� Ÿ������ HP�ٸ� ������
    public void ShowMonsterHPBar(Monster target)
    {
        if (monsterHPBar == null || target == null) return;

        currentTargetMonster = target;

        // ���� �����Ϳ��� �ʻ�ȭ �����ͼ� ����
        monsterHPBar.SetFace(target.GetMonsterData().FaceSprite);

        monsterHPBar.gameObject.SetActive(true);
        monsterHPBar.Show(currentTargetMonster);

        ResetHideTimer(); // �ڵ� ���� Ÿ�̸� ����
    }

    // ���� Ÿ�� ������ HP�� ������Ʈ
    public void UpdateMonsterHP()
    {
        if (monsterHPBar == null || currentTargetMonster == null || !monsterHPBar.gameObject.activeSelf) return;

        monsterHPBar.UpdateHP(
            currentTargetMonster.GetMaxHP(),
            currentTargetMonster.GetPreviousHP(),
            currentTargetMonster.GetCurrentHP(),
            currentTargetMonster.GetHpPerLine()
        );
    }

    // ���� HP�ٸ� ����
    public void HideMonsterHPBar()
    {
        if (monsterHPBar == null || !monsterHPBar.gameObject.activeSelf) return;

        monsterHPBar.Hide();
        // Hide �ִϸ��̼��� ���� �� ��Ȱ��ȭ�ϴ� ������ �߰��� ���� ����
        // await monsterHPBar.Hide();
        // monsterHPBar.gameObject.SetActive(false);

        currentTargetMonster = null;
    }

    // �ڵ� ���� Ÿ�̸Ӹ� �����ϰ� ���� ����
    private void ResetHideTimer()
    {
        hpBarCts?.Cancel(); // ���� Ÿ�̸Ӱ� �ִٸ� ���
        var destroyToken = this.GetCancellationTokenOnDestroy();
        hpBarCts = CancellationTokenSource.CreateLinkedTokenSource(destroyToken);
        AutoHideAsync(hpBarCts.Token).Forget();
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
}