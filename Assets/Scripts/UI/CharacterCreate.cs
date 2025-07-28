using Cysharp.Threading.Tasks;
using System.Threading; // UniTask의 CancellationToken을 사용하기 위해 추가
using UnityEngine;
using UnityEngine.EventSystems; // EventSystem 사용을 위해 추가
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// 캐릭터 직업 정보를 담을클래스
[System.Serializable]
public class CharacterInfo
{
    public string Name; // 직업 이름
    [TextArea(3, 10)] // Inspector에서 여러 줄로 편하게 입력하도록 설정
    public string Description; // 직업 설명
    public Sprite EmblemSprite; // 문양 이미지
    public GameObject PreviewPrefab; // 미리보기용 애니메이션 프리팹
}

public class CharacterCreate : MonoBehaviour
{
    [Header("기본 UI")]
    public GameObject NicknamePanel; // 닉네임 입력 패널
    public TMP_InputField CharacterNameInputField;
    public Button CreateButton; // 캐릭터 생성창을 여는 버튼
    public Button ConfirmButton; // 최종적으로 캐릭터를 생성하는 버튼
    public Button CancelButton; // 닉네임 입력창을 닫는 버튼
    public SpriteRenderer BackgroundImageSR;  // Background 이미지의 스프라이트 렌더러
    public Sprite[] CharacterBackgrounds;

    [Header("일러스트 애니메이션")]
    public CanvasGroup[] IllustrationContainers = new CanvasGroup[2]; // 부모 오브젝트의 CanvasGroup
    public Animator[] CharacterAnimators = new Animator[2]; // 일러스트 이미지의 애니메이터

    [Header("직업 정보 UI")]
    public Image EmblemImage;
    public TextMeshProUGUI JobNameText;
    public TextMeshProUGUI JobDescriptionText;

    [Header("직업 데이터")]
    public CharacterInfo[] CharacterInfos;

    public Button[] CharacterButtons;
    private int currentSelectedIndex = -1; // 현재 선택된 버튼의 인덱스
    private int currentContainerIndex = 0;
    private CancellationTokenSource cts = new CancellationTokenSource();

    // 애니메이터 파라미터 이름들
    private const string CHAR_INDEX_PARAM = "characterIndex";
    private const string PLAY_TRIGGER_PARAM = "playTrigger";

    // 애니메이션을 위한 위치 값들
    private Vector3 centerPosition;
    private Vector3 offscreenRight;
    private Vector3 offscreenLeft;

    void Start()
    {
        // --- 기본 버튼 및 입력 필드 설정 ---
        if (NicknamePanel != null) NicknamePanel.SetActive(false);
        else Debug.LogError("Nickname Panel이 할당되지 않았습니다.");

        if (CreateButton != null) CreateButton.onClick.AddListener(ShowNicknamePanel);
        else Debug.LogError("생성 버튼이 할당되지 않음");

        if (ConfirmButton != null) ConfirmButton.onClick.AddListener(CreateCharacter);
        else Debug.LogError("확인 버튼이 할당되지 않음");

        if (CancelButton != null) CancelButton.onClick.AddListener(HideNicknamePanel);
        else Debug.LogError("취소 버튼이 할당되지 않음");

        if (CharacterNameInputField != null) CharacterNameInputField.onValueChanged.AddListener(ValidateInput);
        else Debug.LogError("캐릭터 이름 입력 필드가 할당되지 않음");
        
        ValidateInput(""); // 시작할 때 확인 버튼 비활성화

        // --- 일러스트 컨테이너 초기화 ---
        if (IllustrationContainers[0] != null && IllustrationContainers[1] != null)
        {
            // 씬 뷰에서 설정한 위치를 애니메이션의 최종 위치로 사용
            centerPosition = IllustrationContainers[0].GetComponent<RectTransform>().anchoredPosition;
            // 화면 너비를 기준으로 화면 밖 위치를 계산
            float screenWidth = IllustrationContainers[0].GetComponent<RectTransform>().rect.width * 1.5f;
            offscreenRight = new Vector3(centerPosition.x + screenWidth, centerPosition.y, centerPosition.z);
            offscreenLeft = new Vector3(centerPosition.x - screenWidth, centerPosition.y, centerPosition.z);

            // 초기 상태 설정
            IllustrationContainers[0].gameObject.SetActive(false);
            IllustrationContainers[1].gameObject.SetActive(false);
            IllustrationContainers[0].alpha = 0;
            IllustrationContainers[1].alpha = 0;
            IllustrationContainers[1].GetComponent<RectTransform>().anchoredPosition = offscreenRight;
        }

        // --- 초기 캐릭터 정보 설정 ---
        // 게임이 시작될 때 0번 캐릭터의 정보가 기본으로 표시되도록 함
        OnCharacterButtonClick(0);
    }

    void Update()
    {
        // 현재 선택된 UI가 없고, 마지막으로 선택한 버튼이 있을 경우
        if (EventSystem.current.currentSelectedGameObject == null && currentSelectedIndex != -1)
        {
            // 마지막으로 선택했던 버튼을 다시 선택 상태로
            EventSystem.current.SetSelectedGameObject(CharacterButtons[currentSelectedIndex].gameObject);
        }
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때 CancellationTokenSource를 정리
        cts.Cancel();
        cts.Dispose();
    }

    private void ValidateInput(string input)
    {
        if (ConfirmButton != null)
            ConfirmButton.interactable = IsValidNickname(input, false);
    }

    public void CreateCharacter()
    {
        string characterName = CharacterNameInputField.text;
        if (!IsValidNickname(characterName, true))
        {
            return;
        }

        // 현재 선택된 직업 정보 가져오기
        if (currentSelectedIndex < 0 || currentSelectedIndex >= CharacterInfos.Length)
        {
            Debug.LogError("유효한 직업이 선택되지 않았음");
            return;
        }
        CharacterInfo selectedInfo = CharacterInfos[currentSelectedIndex];

        // 새 캐릭터 데이터 생성 및 모든 정보 초기화
        CharacterData newCharacter = new CharacterData
        {
            CharacterName = characterName,
            JobName = selectedInfo.Name,
            PreviewPrefabName = selectedInfo.PreviewPrefab.name, // 프리팹 이름 저장

            // 기본 스탯 설정
            Level = 1,
            CurrentEXP = 0,
            RequiredEXP = 100, // 예시 값
            Atk = 10f,
            Def = 10f,
            MoveSpeed = 3f,
            MaxHP = 100f,
            MaxMP = 100f,
        };

        // 데이터 매니저를 통해 캐릭터 추가
        DataManager.Instance.AddCharacter(newCharacter);

        Debug.Log($"캐릭터 '{characterName}' ({selectedInfo.Name}) 생성 완료");
        LoadCharacterSelectScene().Forget();
    }
    // 닉네임 유효성 검사
    private bool IsValidNickname(string name, bool showWarning = false)
    {
        if (string.IsNullOrEmpty(name))
        {
            if (showWarning) Debug.LogWarning("캐릭터 이름이 비어있음");
            return false;
        }

        if (name.Contains(" "))
        {
            if (showWarning) Debug.LogWarning("캐릭터 이름에 공백을 포함할 수 없음");
            return false;
        }

        // 글자 길이 계산 (한글 2, 영어/기타 1)
        if (GetNicknameLength(name) > 12)
        {
            if (showWarning) Debug.LogWarning("캐릭터 이름은 한글 6자, 또는 영문 12자를 초과할 수 없음");
            return false;
        }

        return true;
    }
    // 캐릭터 닉네임 길이 계산
    private int GetNicknameLength(string name)
    {
        int length = 0;
        foreach (char c in name)
        {
            // 한글 범위 (가-힣)
            if (c >= '\uAC00' && c <= '\uD7A3')
            {
                length += 2;
            }
            else
            {
                length += 1;
            }
        }
        return length;
    }

    public void ShowNicknamePanel()
    {
        if (NicknamePanel != null) NicknamePanel.SetActive(true);
        else Debug.LogError("Nickname Panel이 할당되지 않았습니다.");
    }

    public void HideNicknamePanel()
    {
        if (NicknamePanel != null) NicknamePanel.SetActive(false);
        else Debug.LogError("Nickname Panel이 할당되지 않았습니다.");
    }

    private async UniTaskVoid LoadCharacterSelectScene()
    {
        await SceneManager.LoadSceneAsync("CharacterSelect_Scene");
    }

    public void OnCharacterButtonClick(int characterIndex)
    {
        // 이미 선택된 버튼이면 아무것도 하지 않음
        if (currentSelectedIndex == characterIndex) return;

        // 배경 이미지 바꾸기
        if (BackgroundImageSR != null && CharacterBackgrounds != null && characterIndex >= 0 && characterIndex < CharacterBackgrounds.Length)
        {
            BackgroundImageSR.sprite = CharacterBackgrounds[characterIndex];
        } else Debug.LogWarning("백그라운드 이미지 or 스프라이트가 알맞게 셋업되지 않음");

        // 기존 애니메이션을 취소하고 새로운 애니메이션을 시작
        cts.Cancel();
        cts.Dispose();
        cts = new CancellationTokenSource();

        // 다음 컨테이너 인덱스를 미리 계산하고 상태를 즉시 업데이트
        int incomingIndex = (currentContainerIndex + 1) % 2;
        int outgoingIndex = currentContainerIndex;
        currentContainerIndex = incomingIndex; // 상태 즉시 업데이트

        SwitchCharacterIllustration(characterIndex, incomingIndex, outgoingIndex, cts.Token).Forget();

        // 캐릭터 정보(문양, 이름, 설명) 업데이트
        UpdateCharacterInfo(characterIndex);

        // 현재 선택된 버튼을 시각적으로 표시 (초기에 0번 캐릭터의 정보가 기본으로 표시되도록 한 것에 맞춰 Selected 상태로 바꾸기 위함)
        // & 현재 선택 인덱스 업데이트
        if (CharacterButtons != null && characterIndex >= 0 && characterIndex < CharacterButtons.Length)
        {
            CharacterButtons[characterIndex].Select();
            EventSystem.current.SetSelectedGameObject(CharacterButtons[characterIndex].gameObject);
            currentSelectedIndex = characterIndex;
        }
    }

    // 캐릭터 정보를 업데이트하는 새로운 메서드
    private void UpdateCharacterInfo(int index)
    {
        if (CharacterInfos == null || index < 0 || index >= CharacterInfos.Length)
        {
            Debug.LogWarning($"잘못된 캐릭터 인덱스({index}) 또는 characterInfos 배열이 설정되지 않았습니다.");
            return;
        }

        CharacterInfo info = CharacterInfos[index];

        if (EmblemImage != null) EmblemImage.sprite = info.EmblemSprite;
        else Debug.LogError("Emblem Image가 할당되지 않았습니다.");

        if (JobNameText != null) JobNameText.text = info.Name;
        else Debug.LogError("Job Name Text가 할당되지 않았습니다.");

        if (JobDescriptionText != null) JobDescriptionText.text = info.Description;
        else Debug.LogError("Job Description Text가 할당되지 않았습니다.");
    }
    private async UniTaskVoid SwitchCharacterIllustration(int characterIndex, int incomingIndex, int outgoingIndex, CancellationToken token)
    {
        // 나타날 일러스트 설정
        CanvasGroup incomingGroup = IllustrationContainers[incomingIndex];
        RectTransform incomingRect = incomingGroup.GetComponent<RectTransform>();
        Animator incomingAnimator = CharacterAnimators[incomingIndex];

        incomingGroup.gameObject.SetActive(true);
        incomingGroup.alpha = 0;
        incomingRect.anchoredPosition = offscreenRight;
        incomingAnimator.SetInteger(CHAR_INDEX_PARAM, characterIndex); // 어떤 일러스트를 재생해야할지 애니메이터에게 전달
        incomingAnimator.SetTrigger(PLAY_TRIGGER_PARAM); // 재생하라는 신호 보냄

        // 사라질 일러스트 설정
        CanvasGroup outgoingGroup = IllustrationContainers[outgoingIndex];
        RectTransform outgoingRect = outgoingGroup.GetComponent<RectTransform>();
        Vector3 outgoingStartPos = outgoingRect.anchoredPosition;

        float duration = 0.4f;
        float elapsedTime = 0f;

        try
        {
            while (elapsedTime < duration)
            {
                // CancellationToken을 통해 작업 취소 요청을 확인
                token.ThrowIfCancellationRequested();

                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;

                // 나타날 일러스트 Container
                incomingRect.anchoredPosition = Vector3.Lerp(offscreenRight, centerPosition, progress);
                incomingGroup.alpha = Mathf.Lerp(0, 1, progress);

                // 사라질 일러스트 Container (존재한다면)
                if (outgoingGroup.gameObject.activeSelf)
                {
                    outgoingRect.anchoredPosition = Vector3.Lerp(outgoingStartPos, offscreenLeft, progress);
                    outgoingGroup.alpha = Mathf.Lerp(1, 0, progress);
                }

                await UniTask.Yield(cancellationToken: token);
            }

            // 애니메이션이 정상적으로 완료되었을 때의 최종 상태 설정
            incomingRect.anchoredPosition = centerPosition;
            incomingGroup.alpha = 1;
            if (outgoingGroup.gameObject.activeSelf)
            {
                outgoingGroup.gameObject.SetActive(false);
            }

            currentContainerIndex = incomingIndex;
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("애니메이션이 취소");
        }
    }
}