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

    [Header("몬스터 HP 바")]
    [SerializeField] private MonsterHPBar monsterHPBar;
    
    private Monster currentTargetMonster; // 현재 몬스터 HP바가 추적하는 몬스터

    private CancellationTokenSource hpBarCts; // 몬스터 HP바 자동 숨김 작업을 위한 토큰
    private void Start()
    {
        // 시작할 때 몬스터 HP바는 숨겨둠
        if (monsterHPBar != null)
        {
            monsterHPBar.gameObject.SetActive(false);
        }
    }

    // 몬스터가 데미지를 입었을 때 호출될 함수
    public void OnMonsterDamaged(Monster monster)
    {
        // 타겟이 바뀌었거나, HP바가 비활성화 상태라면 새로 표시
        if (currentTargetMonster != monster || !monsterHPBar.gameObject.activeSelf)
        {
            ShowMonsterHPBar(monster);
            UpdateMonsterHP();
        }
        else // 같은 타겟을 계속 때리고 있다면
        {
            // HP만 업데이트하고, 자동 숨김 타이머를 리셋
            UpdateMonsterHP();
            ResetHideTimer();
        }
    }
    // 새로운 몬스터를 타겟으로 HP바를 보여줌
    public void ShowMonsterHPBar(Monster target)
    {
        if (monsterHPBar == null || target == null) return;

        currentTargetMonster = target;

        // 몬스터 데이터에서 초상화 가져와서 설정
        monsterHPBar.SetFace(target.GetMonsterData().FaceSprite);

        monsterHPBar.gameObject.SetActive(true);
        monsterHPBar.Show(currentTargetMonster);

        ResetHideTimer(); // 자동 숨김 타이머 시작
    }

    // 현재 타겟 몬스터의 HP를 업데이트
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

    // 몬스터 HP바를 숨김
    public void HideMonsterHPBar()
    {
        if (monsterHPBar == null || !monsterHPBar.gameObject.activeSelf) return;

        monsterHPBar.Hide();
        // Hide 애니메이션이 끝난 후 비활성화하는 로직을 추가할 수도 있음
        // await monsterHPBar.Hide();
        // monsterHPBar.gameObject.SetActive(false);

        currentTargetMonster = null;
    }

    // 자동 숨김 타이머를 리셋하고 새로 시작
    private void ResetHideTimer()
    {
        hpBarCts?.Cancel(); // 이전 타이머가 있다면 취소
        var destroyToken = this.GetCancellationTokenOnDestroy();
        hpBarCts = CancellationTokenSource.CreateLinkedTokenSource(destroyToken);
        AutoHideAsync(hpBarCts.Token).Forget();
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
}