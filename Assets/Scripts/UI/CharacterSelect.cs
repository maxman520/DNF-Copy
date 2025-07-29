using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
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
        if (StartGameButton != null)
        {
            StartGameButton.onClick.AddListener(StartGame);
        }
        else
        {
            Debug.LogError("게임 시작 버튼이 인스펙터에서 할당되지 않음");
        }
        
        if (CreateCharacterButton != null)
        {
            CreateCharacterButton.onClick.AddListener(CreateCharacter);
        }
        else
        {
            Debug.LogError("캐릭터 생성 버튼이 인스펙터에서 할당되지 않음");
        }

        if (DeleteCharacterButton != null)
        {
            DeleteCharacterButton.onClick.AddListener(DeleteCharacter);
        }
        else
        {
            Debug.LogError("캐릭터 삭제 버튼이 인스펙터에서 할당되지 않음");
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
        if (characters.Count > 0)
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
        SceneManager.LoadScene("Town_Scene");
    }

    public void CreateCharacter()
    {
        SceneManager.LoadScene("CharacterCreate_Scene");
    }

    public void DeleteCharacter()
    {
        if (selectedSlot == null)
        {
            Debug.LogWarning("삭제할 캐릭터가 선택되지 않았습니다.");
            return;
        }

        // DataManager에 캐릭터 삭제 요청 (영구 데이터 삭제)
        DataManager.Instance.DeleteCharacter(selectedSlot.GetCharacterData());

        // 데이터와 UI를 새로고침
        LoadCharacterData();
        PopulateCharacterSlots();
    }
}
