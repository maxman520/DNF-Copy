using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class MenuPanel : MonoBehaviour
{
    [Header("세부 메뉴 버튼 목록")]
    [FormerlySerializedAs("myInfoButton")]
    [SerializeField] private Button profileButton;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button skillButton;
    [Header("하단 버튼 목록")]
    [SerializeField] private Button gameSettingsButton;
    [SerializeField] private Button characterSelectButton;
    [SerializeField] private Button goToTownButton;
    [SerializeField] private Button exitGameButton;

    private void Start()
    {
        // 각 버튼에 리스너 연결
        profileButton?.onClick.AddListener(OnProfileButtonClicked);
        inventoryButton?.onClick.AddListener(OnInventoryButtonClicked);
        skillButton?.onClick.AddListener(OnSkillButtonClicked);
        gameSettingsButton?.onClick.AddListener(OnGameSettingsButtonClicked);
        characterSelectButton?.onClick.AddListener(OnCharacterSelectButtonClicked);
        goToTownButton?.onClick.AddListener(OnGoToTownButtonClicked);
        exitGameButton?.onClick.AddListener(OnExitGameButtonClicked);
    }

    private void OnEnable()
    {
        // UIManager에 현재 UI가 열렸음을 알림
        UIManager.Instance?.OpenUI(this.gameObject);

        // 메뉴창이 활성화될 때, 현재 씬이 던전인지 확인하여 '마을로 가기' 버튼 활성화
        if (goToTownButton != null)
        {
            // 현재 활성화된 씬의 이름에 "Dungeon"이 포함되어 있는지 확인
            bool isInDungeon = SceneManager.GetActiveScene().name.Contains("Dungeon");
            goToTownButton.interactable = isInDungeon;
        }
        // 메뉴창이 활성화될 때, 현재 씬이 던전인지 확인하여 '캐릭터 선택' 버튼 활성화
        if (characterSelectButton != null)
        {
            // 현재 활성화된 씬의 이름에 "Dungeon"이 포함되어 있는지 확인
            bool isInDungeon = SceneManager.GetActiveScene().name.Contains("Dungeon");
            characterSelectButton.interactable = !isInDungeon;
        }
    }

    private void OnDisable()
    {
        // UIManager에 현재 UI가 닫혔음을 알림
        UIManager.Instance?.CloseUI(this.gameObject);
    }

    // 메뉴창 비활성화
    private void DeactivateMenu()
    {
        gameObject.SetActive(false);
    }

    // 프로필 버튼
    public void OnProfileButtonClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ToggleProfilePanel();
        }
        DeactivateMenu();
    }

    // 인벤토리 버튼
    public void OnInventoryButtonClicked()
    {
        // UIManager를 통해 인벤토리 UI를 토글
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ToggleInventoryPanel();
        }
        DeactivateMenu();
    }

    // 스킬 버튼
    public void OnSkillButtonClicked()
    {
        // UIManager를 통해 스킬샵 UI를 토글
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ToggleSkillShopPanel();
        }
        DeactivateMenu();
    }

    // 게임 설정 버튼 (구현 예정)
    public void OnGameSettingsButtonClicked()
    {
        Debug.Log("게임 설정창 열기 (구현 예정)");
        DeactivateMenu();
    }

    // 캐릭터 선택 버튼
    public void OnCharacterSelectButtonClicked()
    {
        DeactivateMenu();
        GameManager.Instance.ResetGameAndGoToCharacterSelect();
    }

    // 마을로 가기 버튼
    public void OnGoToTownButtonClicked()
    {
        // GameManager를 통해 마을로 이동하는 로직 호출
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToTown();
        }
        DeactivateMenu();
    }

    // 7. 게임 종료 버튼
    public void OnExitGameButtonClicked()
    {
        DeactivateMenu();
        GameManager.Instance.SavePlayerData(); // 데이터 저장
        Debug.Log("게임을 종료합니다.");

#if UNITY_EDITOR
        // 유니티 에디터에서 실행 중일 경우, 플레이 모드를 중지
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임일 경우, 어플리케이션 종료
        Application.Quit();
#endif
    }
}
