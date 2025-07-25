using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CharacterCreate : MonoBehaviour
{
    public TMP_InputField characterNameInputField;
    public Button createButton;
    public SpriteRenderer backgroundImageSR; // Background 이미지의 스프라이트 렌더러
    public Sprite[] characterBackgrounds;

    [Header("일러스트 애니메이션")]
    public CanvasGroup[] illustrationContainers = new CanvasGroup[2]; // 부모 오브젝트의 CanvasGroup
    public Animator[] characterAnimators = new Animator[2]; // 일러스트 이미지의 애니메이터

    private int currentContainerIndex = 0;
    private bool isAnimating = false;

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
        if (createButton != null) createButton.onClick.AddListener(CreateCharacter);
        else Debug.LogError("생성 버튼이 할당되지 않음");

        if (characterNameInputField != null) characterNameInputField.onValueChanged.AddListener(ValidateInput);
        else Debug.LogError("캐릭터 이름 입력 필드가 할당되지 않음");
        
        ValidateInput(""); // 시작할 때 버튼 비활성화

        // --- 일러스트 컨테이너 초기화 ---
        if (illustrationContainers[0] != null && illustrationContainers[1] != null)
        {
            // 씬 뷰에서 설정한 위치를 애니메이션의 최종 위치로 사용
            centerPosition = illustrationContainers[0].GetComponent<RectTransform>().anchoredPosition;

            // 화면 너비를 기준으로 화면 밖 위치를 계산
            float screenWidth = illustrationContainers[0].GetComponent<RectTransform>().rect.width * 1.5f;
            offscreenRight = new Vector3(centerPosition.x + screenWidth, centerPosition.y, centerPosition.z);
            offscreenLeft = new Vector3(centerPosition.x - screenWidth, centerPosition.y, centerPosition.z);

            // 초기 상태 설정
            illustrationContainers[0].gameObject.SetActive(false);
            illustrationContainers[1].gameObject.SetActive(false);
            illustrationContainers[0].alpha = 0;
            illustrationContainers[1].alpha = 0;
            illustrationContainers[1].GetComponent<RectTransform>().anchoredPosition = offscreenRight;
        }
    }

    private void ValidateInput(string input)
    {
        if (createButton != null) createButton.interactable = !string.IsNullOrEmpty(input);
    }

    public void CreateCharacter()
    {
        string characterName = characterNameInputField.text;
        if (string.IsNullOrEmpty(characterName)) {
            Debug.LogWarning("캐릭터 이름이 비어있음");
            return;
        }
        Debug.Log($"캐릭터 '{characterName}' 생성");
        LoadCharacterSelectScene().Forget();
    }

    private async UniTaskVoid LoadCharacterSelectScene()
    {
        await SceneManager.LoadSceneAsync("CharacterSelect_Scene");
    }

    public void OnCharacterButtonClick(int characterIndex)
    {
        // 배경 이미지 바꾸기
        if (backgroundImageSR != null && characterBackgrounds != null && characterIndex >= 0 && characterIndex < characterBackgrounds.Length)
        {
            backgroundImageSR.sprite = characterBackgrounds[characterIndex];
        } else Debug.LogWarning("백그라운드 이미지 or 스프라이트가 알맞게 셋업되지 않음");

        // 일러스트가 나타나는 애니메이션 트리거
        if (!isAnimating)
        {
            SwitchCharacterIllustration(characterIndex).Forget();
        }
    }

    private async UniTaskVoid SwitchCharacterIllustration(int characterIndex)
    {
        isAnimating = true;

        int incomingIndex = (currentContainerIndex + 1) % 2;
        int outgoingIndex = currentContainerIndex;

        // 나타날 일러스트 설정
        CanvasGroup incomingGroup = illustrationContainers[incomingIndex];
        RectTransform incomingRect = incomingGroup.GetComponent<RectTransform>();
        Animator incomingAnimator = characterAnimators[incomingIndex];

        incomingGroup.gameObject.SetActive(true);
        incomingGroup.alpha = 0;
        incomingRect.anchoredPosition = offscreenRight;
        incomingAnimator.SetInteger(CHAR_INDEX_PARAM, characterIndex); // 어떤 일러스트를 재생해야할지 애니메이터에게 전달
        incomingAnimator.SetTrigger(PLAY_TRIGGER_PARAM); // 재생하라는 신호 보냄

        // 사라질 일러스트 설정
        CanvasGroup outgoingGroup = illustrationContainers[outgoingIndex];
        RectTransform outgoingRect = outgoingGroup.GetComponent<RectTransform>();
        Vector3 outgoingStartPos = outgoingRect.anchoredPosition;

        float duration = 0.4f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
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

            await UniTask.Yield();
        }

        // 좌표와 알파값 최종 초기화
        incomingRect.anchoredPosition = centerPosition;
        incomingGroup.alpha = 1;
        if (outgoingGroup.gameObject.activeSelf) {
            outgoingGroup.gameObject.SetActive(false);
        }

        currentContainerIndex = incomingIndex;
        isAnimating = false;
    }
}