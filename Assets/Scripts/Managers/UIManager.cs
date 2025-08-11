using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{

    [Header("HP, MP, EXP 게이지")]
    [SerializeField] private Image hpGauge;
    [SerializeField] private Image mpGauge;
    [SerializeField] private Image expGauge;

    [Header("몬스터 UI")]
    [Tooltip("일반 몬스터용 HP바")]
    [SerializeField] private MonsterHPBar monsterHPBar; // 일반 몬스터 HP 바
    [Tooltip("보스 몬스터용 HP바")]
    [SerializeField] private MonsterHPBar bossHPBar; // 보스 몬스터 HP 바

    [Header("스킬 창")]
    [SerializeField] private SkillShopUI skillShopUI; // 스킬샵 창


    [Header("미니맵")]
    [SerializeField] private MinimapUI minimapUI; // 미니맵
    [SerializeField] private TextMeshProUGUI mapName; // 맵 이름

    [Header("던전 결과 창")]
    [SerializeField] private ResultPanel resultPanel; // 던전 결과 창

    [Header("메뉴 창")]
    [SerializeField] private MenuUI menuPanel; // 메뉴 창

    [Header("인벤토리 창")]
    [SerializeField] private InventoryUI inventoryPanel; // 인벤토리 창

    
    [Header("아이템 설명 창")]
    [SerializeField] private ItemDescriptionPanel itemDescriptionPanel; // 아이템 설명 창

    [Header("플레이어 사망 UI")]
    [SerializeField] private GhostStatePanel ghostStatePanel; // 플레이어 사망 시 띄워질 창

    private List<GameObject> openedUIList = new List<GameObject>(); // 열려있는 창의 리스트. Esc버튼을 누를 시 창을 닫는데에 사용

    private Monster currentTarget; // 현재 추적 중인 타겟 몬스터
    private CancellationTokenSource monsterHPBarCts; // 몬스터 HP바 자동 숨김 작업을 위한 토큰
    private void Start()
    {
        // 시작할 때 모든 몬스터 HP바는 숨겨둠
        monsterHPBar?.gameObject.SetActive(false);
        bossHPBar?.gameObject.SetActive(false);

        resultPanel?.gameObject.SetActive(false); // 던전 결과 창 비활성화
        skillShopUI?.gameObject.SetActive(false); // 스킬샵 창 비활성화
        minimapUI?.gameObject.SetActive(false); // 미니맵 비활성화
        menuPanel?.gameObject.SetActive(false); // 메뉴 창 비활성화
        inventoryPanel?.gameObject.SetActive(false); // 인벤토리 창 비활성화
        itemDescriptionPanel?.gameObject.SetActive(false); // 아이템 설명 창 비활성화
        ghostStatePanel?.gameObject.SetActive(false); // 플레이어 사망 UI 비활성화

        // RoomManager 이벤트 구독
        if (RoomManager.Instance != null) {
            RoomManager.Instance.OnRoomEntered += HandleRoomEntered;
        }
    }

    private void Update()
    {
        // 'K' 키를 누르면 스킬샵 창을 토글
        if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleSkillShopUI();
        }
        // 'I' 키를 누르면 스킬샵 창을 토글
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventoryUI();
        }
        // 'Escape' 키를 누르면 메뉴 창을 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }


    // UI 창이 열릴 때 리스트에 추가. 각각의 창이 OnEnable 에서 호출
    public void OpenUI(GameObject uiObject)
    {
        if (!openedUIList.Contains(uiObject))
        {
            openedUIList.Add(uiObject);
        }
    }
    // UI 창이 닫힐 때 리스트에서 제거. 각각의 창이 OnDisable 에서 호출
    public void CloseUI(GameObject uiObject)
    {
        if (openedUIList.Contains(uiObject))
        {
            openedUIList.Remove(uiObject);
        }
    }

    public void SetMapName(string name)
    {
        if (this.mapName != null)
            this.mapName.text = name;
    }
    
    public void HideDungeonUI()
    {
        HideMonsterHPBar();
        HideBossHPBar();
        HideMinimap();
        HideResultPanel();
        HideGhostStatePanel();        
    }
    
    #region Menu
    private void HandleEscapeKey()
    {
        // 열려있는 UI가 있다면 가장 마지막에 열린 UI를 닫음
        if (openedUIList.Count > 0)
        {
            // 리스트의 가장 마지막 요소 (가장 최근에 열린 창)를 가져옴
            GameObject lastOpenedUI = openedUIList[openedUIList.Count - 1];
            lastOpenedUI.SetActive(false); // 이 창을 닫으면 OnDisable에서 리스트에서 제거됨
        }
        // 열려있는 UI가 없다면 메뉴창을 토글
        else
        {
            ToggleMenuUI();
        }
    }

    public void ToggleMenuUI()
    {
        if (menuPanel != null)
        {
            menuPanel.gameObject.SetActive(!menuPanel.gameObject.activeSelf);
        }
    }
    #endregion Menu

    #region SkillShop Panel
    public void ToggleSkillShopUI() {
        if (skillShopUI != null)
        {
            skillShopUI.gameObject.SetActive(!skillShopUI.gameObject.activeSelf);
        }
    }
    #endregion SkillShop

    #region Inventory
    public void ToggleInventoryUI()
    {
        if (inventoryPanel != null && itemDescriptionPanel != null)
        {
            inventoryPanel.gameObject.SetActive(!inventoryPanel.gameObject.activeSelf);
            itemDescriptionPanel.gameObject.SetActive(false); // 아이템 설명 창도 인벤토리와 함께 비활성화
        }
    }
    #endregion Inventory

    #region Result
    public void SetResultPanelData(DungeonResultData resultData) {
        if (resultPanel != null)
            resultPanel.SetResultData(resultData);

    }
    public void ToggleResultPanel()
    {
        if (resultPanel != null)
            resultPanel.gameObject.SetActive(!resultPanel.gameObject.activeSelf);
    }

    public void HideResultPanel()
    {
        if (resultPanel != null && resultPanel.gameObject.activeSelf)
            resultPanel.gameObject.SetActive(false);
    }
    #endregion Result


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
    #region Boss HP Bar
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
    #endregion Boss  HP Bar

    #region Regular Monster HP Bar
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
    #endregion Regular Monster HP Bar



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

    public void UpdateEXP(float requiredEXP, float currentEXP)
    {   
        if (expGauge != null)
        {
            expGauge.fillAmount = currentEXP / requiredEXP;
        }
    }
    #endregion Player

    #region Minimap
    // RoomManager의 OnRoomEntered 이벤트가 발생했을 때 호출될 함수
    private void HandleRoomEntered(Room newRoom)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentDungeon == null) return;

        // 현재 던전의 방 목록에서 새로운 방의 인덱스를 찾음
        int roomIndex = GameManager.Instance.CurrentDungeon.Rooms.IndexOf(newRoom);

        if (roomIndex != -1)
        {
            // 찾은 인덱스로 미니맵 업데이트
            UpdateMinimapPlayerPosition(roomIndex);
        }
    }

    // 미니맵 생성 요청을 받는 함수''
    public void GenerateMinimap(Dungeon dungeonData)
    {
        if (minimapUI != null)
        {
            minimapUI.gameObject.SetActive(true); // 미니맵 활성화
            minimapUI.GenerateMap(dungeonData);
        }
    }

    // 플레이어 위치 업데이트 요청을 받는 함수
    public void UpdateMinimapPlayerPosition(int roomIndex)
    {
        if (minimapUI != null && minimapUI.gameObject.activeSelf)
            minimapUI.UpdatePlayerPosition(roomIndex);
    }

    // 던전 퇴장 시 미니맵을 숨기는 함수
    public void HideMinimap()
    {
        if (minimapUI != null)
            minimapUI.gameObject.SetActive(false);
    }
    #endregion Minimap

    #region Item Description
    public void ShowItemDescription(ItemData data, RectTransform slotRectTransform)
    {
        itemDescriptionPanel?.Show(data, slotRectTransform);
    }

    public void HideItemDescription()
    {
        itemDescriptionPanel?.Hide();
    }
    #endregion

    #region Ghost State
    public void ShowGhostStatePanel()
    {
        if (ghostStatePanel == null) return;
        ghostStatePanel.gameObject.SetActive(true);
        ghostStatePanel.Initialize();
    }

    public void ShowCountdown()
    {
        if (ghostStatePanel == null || !ghostStatePanel.gameObject.activeSelf) return;
        ghostStatePanel.StartCountdown();
    }

    public void HideGhostStatePanel()
    {
        if (ghostStatePanel == null) return;
        ghostStatePanel.gameObject.SetActive(false);
    }
    #endregion  Ghost State
}