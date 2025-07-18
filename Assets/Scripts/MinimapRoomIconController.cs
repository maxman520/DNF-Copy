using UnityEngine;
using UnityEngine.UI;

public enum MinimapRoomState { Hidden, Empty, Discovered, Current }

public class MinimapRoomIconController : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private MinimapIconData iconData; // MinimapIconData ����

    public MinimapRoomState CurrentState { get; private set; }
    private Room assignedRoom; // �� �������� � ���� ��Ÿ������

    // �� �����ܿ� �ش��ϴ� �� ������ ����
    public void AssignRoom(Room room)
    {
        this.assignedRoom = room;
    }

    public void SetState(MinimapRoomState newState)
    {
        CurrentState = newState;
        UpdateIcon();
    }

    // ���� ���¿� ���� ������ �̹����� ������Ʈ
    private void UpdateIcon()
    {
        if (assignedRoom == null || iconData == null)
        {
            iconImage.enabled = false;
            return;
        }

        iconImage.enabled = true;

        switch (CurrentState)
        {
            case MinimapRoomState.Hidden:
                iconImage.enabled = false;
                break;
            case MinimapRoomState.Empty:
                // �������� ���� ������, �������� �̹߰� ������
                iconImage.sprite = (assignedRoom.roomType == Room.RoomType.Boss) ? iconData.BossIcon : iconData.GetPathSprite(true, 0);
                break;
            case MinimapRoomState.Current:
                // ���� ���� Active �� ��� ���������� ����
                // GetPathSprite�� true�� ���� hasExit ������ ����
                iconImage.sprite = iconData.GetPathSprite(true, assignedRoom.hasExit);
                break;
            case MinimapRoomState.Discovered:
                // ������ ���� Inactive �� ��� ���������� ����
                // GetPathSprite�� false�� ���� hasExit ������ ����
                iconImage.sprite = iconData.GetPathSprite(false, assignedRoom.hasExit);
                break;
        }
    }
}