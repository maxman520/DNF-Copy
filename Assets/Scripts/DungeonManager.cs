using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonManager : Singleton<DungeonManager>
{
    private Dungeon currentDungeon; // ���� �÷��� ���� ������ ����
    private Room currentRoom;

    protected override void Awake()
    {
        base.Awake();
    }

    // ��ǥ ��Ż �������� �̵��ϴ� �Լ�
    public void EnterRoom(int targetRoomIndex, Portal targetPortal)
    {

        if (targetPortal == null)
        {
            Debug.LogError("targetPortal�� �Ҵ���� �ʾ���");
            return;
        }

        if (targetRoomIndex < 0 || targetRoomIndex >= currentDungeon.Rooms.Count)
        {
            Debug.LogError("targetRoomIndex�� �߸��Ǿ���");
            return;
        }

        // ���� ���� �־��ٸ� ���� ó��
        currentRoom?.OnExitRoom();

        // currentRoom�� ���ο� ������ ����
        currentRoom = currentDungeon.Rooms[targetRoomIndex];

        // �÷��̾� �̴ϸ� ��ġ ������Ʈ
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateMinimapPlayerPosition(targetRoomIndex);

        // ���ο� ���� ���� ����
        currentRoom.OnEnterRoom();

        // �÷��̾� ��ġ �̵�
        Player.Instance.transform.position = targetPortal.transform.position;
    }

    // ���ο� ������ �����ϴ� �Լ�
    public void StartDungeon(Dungeon dungeonToStart)
    {
        Debug.Log($"���ο� ���� '{dungeonToStart.DungeonName}'�� ����");
        this.currentDungeon = dungeonToStart;


        if (UIManager.Instance != null) 
            UIManager.Instance.SetMapName(dungeonToStart.DungeonName);

        if (currentDungeon.Rooms == null)
            Debug.LogError("Dungeon_Data�� Room���� �Ҵ���� �ʾ���");

        // ��� ���� �ϴ� ����
        foreach (var room in currentDungeon.Rooms)
        {
            room.gameObject.SetActive(false);
        }

        // ù ��° ����� ����
        currentRoom = currentDungeon.Rooms[0];
        currentRoom.OnEnterRoom();

        // �̴ϸ� ���� ��û & �÷��̾� �̴ϸ� ��ġ ������Ʈ
        if (UIManager.Instance != null)
        {
            UIManager.Instance.GenerateMinimap(dungeonToStart);
            UIManager.Instance.UpdateMinimapPlayerPosition(0);
        }

        Player.Instance.transform.position = currentDungeon.StartPosition;
        
    }

    // ���� ���Ͱ� ȣ���� �Լ�
    public void ShowResultPanel()
    {
        // ���� Ŭ���� ���� ���
        DungeonResultData resultData = CalculateDungeonResult();

        // ���â ǥ��
        UIManager.Instance.ShowResultPanel(resultData);
    }

    // ���� ��� ����
    private DungeonResultData CalculateDungeonResult()
    {
        // �����δ� ���� �����ͳ� óġ�� ���� ����Ʈ ���� ������� ����ؾ� ��
        // �ӽ� ��. ���߿� ������ ��
        return new DungeonResultData
        {
            // clearTime�� �߰��ؾ� ��
            HuntEXP = 12345,
            ClearEXP = 67890
        };
    }

    // "������ ���ư���" ��ư�� ȣ���� �Լ�
    public void ReturnToTown()
    {
        Debug.Log("����� Ȯ���߽��ϴ�. ������ ���ư��ϴ�.");

        string townToReturn = currentDungeon.TownToReturn;

        // ���� ���� ������ �ʱ�ȭ
        currentDungeon = null;

        // ���� �� �ε�
        SceneManager.LoadScene(townToReturn);
    }

    // "���� ���� ����" ��ư�� ȣ���� �Լ�
    public void GoToNextDungeon()
    {
        Debug.Log("����� Ȯ���߽��ϴ�. ���� �������� �̵��մϴ�.");

        string nextDungeonSceneName = currentDungeon.NextDungeonName; // �ӽ� ��. ���� ���� �����Ϳ� ���� ���� �̸��� �߰��� ��

        SceneManager.LoadScene(nextDungeonSceneName);
    }
}