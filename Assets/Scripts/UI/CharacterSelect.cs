using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    [Header("BGM")]
    [SerializeField] private string bgmKey = "Characterselectstage";
    [SerializeField] private bool bgmLoop = true;
    [SerializeField] private float bgmFade = 1.0f;
    [Header("UI 요소")]
    public List<CharacterSlot> CharacterSlots; // 씬에 미리 배치된 캐릭터 슬롯들
    public Button StartGameButton;
    public Button CreateCharacterButton;
    public Button DeleteCharacterButton;

    private List<CharacterData> characters;
    private CharacterSlot selectedSlot; // 현재 선택된 캐릭터 슬롯

    void Start()
    {
        // 1. 데이터 로드
        LoadCharacterData();

        // 2. UI에 캐릭터 표시
        PopulateCharacterSlots();

        // 3. 버튼들의 리스너 설정
        if (StartGameButton != null) StartGameButton.onClick.AddListener(StartGame);
        else Debug.LogError("게임 시작 버튼이 인스펙터에서 할당되지 않음");
        
        if (CreateCharacterButton != null) CreateCharacterButton.onClick.AddListener(CreateCharacter);
        else Debug.LogError("캐릭터 생성 버튼이 인스펙터에서 할당되지 않음");

        if (DeleteCharacterButton != null) DeleteCharacterButton.onClick.AddListener(DeleteCharacter);
        else Debug.LogError("캐릭터 삭제 버튼이 인스펙터에서 할당되지 않음");

        // 4. BGM 재생
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGMIfChanged(bgmKey, bgmLoop, bgmFade);
        }

    }

    void LoadCharacterData()
    {
        // DataManager를 통해 저장된 캐릭터 목록을 가져온다.
        characters = DataManager.Instance.GetCharacters();
        Debug.Log($"{characters.Count}명의 캐릭터 정보를 로드");
    }

    void PopulateCharacterSlots()
    {
        // 미리 할당된 슬롯의 수와 불러온 캐릭터 데이터의 수를 비교
        for (int i = 0; i < CharacterSlots.Count; i++)
        {
            // 할당할 캐릭터 데이터가 있는 경우
            if (i < characters.Count)
            {
                CharacterData data = characters[i];

                CharacterSlots[i].gameObject.SetActive(true); // 슬롯 활성화
                CharacterSlots[i].Initialize(data, this);
            }
            // 할당할 캐릭터 데이터가 없는 경우 (빈 슬롯)
            else
            {
                CharacterSlots[i].gameObject.SetActive(false); // 슬롯 비활성화
            }
        }

        // 첫 번째 캐릭터가 있다면 기본으로 선택하고, 없다면 선택 해제 상태로 둠
        if (characters.Count > 0 && CharacterSlots[0] != null)
        {
            SelectCharacter(CharacterSlots[0]);
        }
        else
        {
            SelectCharacter(null);
        }
    }

    // 슬롯의 OnPointerClick에서 호출되거나, 캐릭터가 없을 때 null로 호출됨
    public void SelectCharacter(CharacterSlot slot)
    {
        // 이전에 선택된 슬롯이 있다면 선택 해제
        if (selectedSlot != null)
        {
            selectedSlot.Deselect();
        }
        
        selectedSlot = slot;

        // 새로 선택된 슬롯이 있다면 선택 효과를 켬
        if (selectedSlot != null)
        {
            selectedSlot.Select();
        }
        AudioManager.Instance.PlaySFX("Selectcharacter");

        // 버튼 활성화 상태는 항상 현재 선택된 슬롯을 기준으로 결정
        bool isCharacterSelected = selectedSlot != null;
        if (StartGameButton != null)
        {
            StartGameButton.interactable = isCharacterSelected;
        }
        if (DeleteCharacterButton != null)
        {
            DeleteCharacterButton.interactable = isCharacterSelected;
        }
    }

    public void StartGame()
    {
        if (selectedSlot == null) return;
        DataManager.Instance.SelectCharacter(selectedSlot.GetCharacterData());
        AudioManager.Instance.PlaySFX("Click2");
        LoadScene("Town_Scene");
    }

    public void CreateCharacter()
    {
        AudioManager.Instance.PlaySFX("Click2");
        LoadScene("CharacterCreate_Scene");
    }

    public void DeleteCharacter()
    {
        if (selectedSlot == null)
        {
            Debug.LogWarning("삭제할 캐릭터가 선택되지 않았습니다.");
            return;
        }
        AudioManager.Instance.PlaySFX("Click2");

        // DataManager에 캐릭터 삭제 요청 (영구 데이터 삭제)
        DataManager.Instance.DeleteCharacter(selectedSlot.GetCharacterData());

        AudioManager.Instance.PlaySFX("Char_Delete");

        // 데이터와 UI를 새로고침
        LoadCharacterData();
        PopulateCharacterSlots();
    }

    private async void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;
        
        // 로딩 씬 Additive 비동기 로드
        var loadLoadingScene = SceneManager.LoadSceneAsync("Loading_Scene", LoadSceneMode.Additive);
        await UniTask.WaitUntil(() => loadLoadingScene.isDone);
        Debug.Log("Loading_Scene 로드 완료");

        // 메인 씬을 비동기적으로 로드하고 완료될 때까지 기다립니다.
        var loadSceneOperation = SceneManager.LoadSceneAsync(sceneName);
        loadSceneOperation.allowSceneActivation = false; // 씬 활성화를 수동으로 제어
        Debug.Log("메인 Scene 로드 후 비활성화");

        // !! Unity에서 씬 비동기 로드의 progress는 최대 0.9까지만 오르고, 실제 씬 활성화는 allowSceneActivation = true가 되었을 때 이뤄짐 !!
        // 로딩 진행 상황 업데이트 (필요시)
        while (loadSceneOperation.progress < 0.9f)
        {
            // 구현 예: 로딩 UI에 진행 상황 표시
            Debug.Log($"로딩 진행 중: {loadSceneOperation.progress * 100}%");
            await UniTask.Yield();
        }

        // 씬 활성화
        loadSceneOperation.allowSceneActivation = true;
        Debug.Log("메인 Scene 활성화");
    }
}
