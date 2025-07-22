using UnityEngine;
using UnityEngine.UI;

public enum MinimapRoomState { Hidden, Empty, Discovered, Current }

public class MinimapRoomIconController : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private MinimapIconData iconData; // MinimapIconData 참조

    public MinimapRoomState CurrentState { get; private set; }
    private Room assignedRoom; // 이 아이콘이 어떤 방을 나타내는지

    // 이 아이콘에 해당하는 방 정보를 설정
    public void AssignRoom(Room room)
    {
        this.assignedRoom = room;
    }

    public void SetState(MinimapRoomState newState)
    {
        CurrentState = newState;
        UpdateIcon();
    }

    // 현재 상태에 맞춰 아이콘 이미지를 업데이트
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
                // 보스방은 보스 아이콘, 나머지는 미발견 아이콘
                iconImage.sprite = (assignedRoom.roomType == Room.RoomType.Boss) ? iconData.BossIcon : iconData.GetPathSprite(true, 0);
                break;
            case MinimapRoomState.Current:
                // 현재 방은 Active 길 모양 아이콘으로 변경
                // GetPathSprite에 true와 방의 hasExit 정보를 전달
                iconImage.sprite = iconData.GetPathSprite(true, assignedRoom.hasExit);
                break;
            case MinimapRoomState.Discovered:
                // 나머지 방은 Inactive 길 모양 아이콘으로 변경
                // GetPathSprite에 false와 방의 hasExit 정보를 전달
                iconImage.sprite = iconData.GetPathSprite(false, assignedRoom.hasExit);
                break;
        }
    }
}