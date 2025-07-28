using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CharacterSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI 요소")]
    public Transform PreviewAreaParent; // 각 슬롯의 프리팹 생성 위치
    public TextMeshProUGUI NameText;
    public GameObject SelectionHighlight; // 선택되었을 때 켤 하이라이트 오브젝트

    private CharacterData characterData;
    private CharacterSelect characterSelectManager; // 상위 매니저 참조
    private GameObject characterInstance; // 이 슬롯이 생성한 프리팹 인스턴스


    // 슬롯이 비활성화 되거나 파괴될 때 프리팹도 함께 정리
    private void OnDisable()
    {
        if (characterInstance != null)
            Destroy(characterInstance);   
    }

    public void Initialize(CharacterData data, CharacterSelect manager)
    {
        this.characterData = data;
        this.characterSelectManager = manager;
        NameText.text = data.CharacterName;

        // 이전에 있던 프리팹이 혹시 남아있다면 삭제
        if (characterInstance != null)
            Destroy(characterInstance);

        // CharacterData의 PreviewPrefabName으로 프리팹을 로드
        if (!string.IsNullOrEmpty(data.PreviewPrefabName))
        {
            string prefabPath = $"Prefabs/Player/{data.PreviewPrefabName}";
            GameObject previewPrefab = Resources.Load<GameObject>(prefabPath);

            if (previewPrefab != null)
            {
                // 1. 위치 기준점(UI)의 스크린 좌표 가져오기
                Vector3 screenPosition = PreviewAreaParent.position;

                // 2. 카메라와의 거리 설정 (카메라가 z=-10에 있다고 가정)
                screenPosition.z = 10.0f;

                // 3. 스크린 좌표를 월드 좌표로 변환
                // ※ 중요: 씬에 있는 메인 카메라에 "MainCamera" 태그가 설정되어 있어야 함
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

                // 4. 계산된 월드 좌표에 부모 없이(!), 원래 크기 그대로 생성
                characterInstance = Instantiate(previewPrefab, worldPosition, Quaternion.identity);

                // 5. (선택) 생성된 프리팹의 Sorting Layer를 UI보다 높게 설정
                //    예시: "CharacterPreview"라는 Sorting Layer를 만들었다고 가정
                // Renderer renderer = characterInstance.GetComponentInChildren<Renderer>();
                // if (renderer != null)
                // {
                //     renderer.sortingLayerName = "CharacterPreview";
                // }
            }
            else
            {
                Debug.LogError($"프리팹 로드 실패: {prefabPath}");
            }
        }
        else
        {
            Debug.LogError($"{data.CharacterName}의 PreviewPrefabName이 비어있습니다.");
        }

        Deselect();
    }

    public CharacterData GetCharacterData() => characterData;

    public void Select()
    {
        SelectionHighlight.SetActive(true);
    }

    public void Deselect()
    {
        SelectionHighlight.SetActive(false);
    }

    // 슬롯을 클릭했을 때의 이벤트 처리
    public void OnPointerClick(PointerEventData eventData)
    {
        characterSelectManager.SelectCharacter(this);
    }

    /// 드래그 앤 드롭 관련 함수들
    public void OnBeginDrag(PointerEventData eventData)
    {
        // TODO: 드래그 시작 로직 구현
        Debug.Log($"Begin Drag: {characterData.CharacterName}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        // TODO: 드래그 중 로직 구현
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // TODO: 드래그 종료 로직 구현
        Debug.Log($"End Drag: {characterData.CharacterName}");
    }

    public void OnDrop(PointerEventData eventData)
    {
        // TODO: 다른 슬롯이 여기에 드롭되었을 때 로직 구현
        Debug.Log($"Dropped on: {characterData.CharacterName}");
    }
}
