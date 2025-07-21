using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

// ���� ��� �����͸� ���� ������ ����ü
public struct DungeonResultData
{
    public int HuntEXP;
    public int ClearEXP;
}

[RequireComponent(typeof(CanvasGroup))]
public class ResultPanel : MonoBehaviour
{
    [Header("UI ����")]
    [SerializeField] private TextMeshProUGUI totalExpText;
    [SerializeField] private TextMeshProUGUI huntExpText;
    [SerializeField] private TextMeshProUGUI clearExpText;
    [SerializeField] private Button returnTownButton;
    [SerializeField] private Button nextDungeonButton;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        
        // ó������ ��ȣ�ۿ� �Ұ�
        gameObject.SetActive(false);
        canvasGroup.interactable = false;
    }
    private void Start()
    {   
        // ��ư Ŭ�� �̺�Ʈ�� DungeonManager�� �Լ����� ����
        if (DungeonManager.Instance != null)
        {
            returnTownButton.onClick.AddListener(DungeonManager.Instance.ReturnToTown);
            nextDungeonButton.onClick.AddListener(DungeonManager.Instance.GoToNextDungeon);
        }
    }

    // ��� �����͸� �޾� UI�� ä���, �г��� �����ִ� ���� �Լ�
    public void Show(DungeonResultData resultData)
    {
        // 1. �����ͷ� UI �ؽ�Ʈ ������Ʈ
        huntExpText.text = $"{resultData.HuntEXP:N0}"; // N0�� õ ���� �޸�
        clearExpText.text = $"{resultData.ClearEXP:N0}";
        totalExpText.text = $"{resultData.HuntEXP+resultData.ClearEXP:N0}";

        // 2. Ȱ��ȭ
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
    }
}