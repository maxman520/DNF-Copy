using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : Singleton<RoomManager>
{
    private Room currentRoom;

    // 플레이어 위치를 기반으로 CurrentRoom을 업데이트
    public void UpdateCurrentRoomByPlayerPosition()
    {
        Room[] rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);
        bool roomFound = false;
        foreach (Room room in rooms)
        {
            // 플레이어의 위치와 겹치는 CameraBound 찾기
            if (room.CameraBound != null && room.CameraBound.OverlapPoint(Player.Instance.transform.position))
            {
                currentRoom = room;
                ChangeCameraConfiner(currentRoom.CameraBound);
                currentRoom.OnEnterRoom();
                roomFound = true;
                break;
            }
        }

        if (!roomFound)
        {
            Debug.LogWarning("플레이어의 현재 위치에 해당하는 방을 찾지 못했습니다. 카메라 경계가 설정되지 않았습니다.");
            ChangeCameraConfiner(null); // 경계를 찾지 못하면 Confiner를 비활성화
        }
    }

    // 목표 방으로 이동하는 함수
    public void EnterRoom(Room targetRoom, Portal targetPortal)
    {
        if (targetRoom == null || targetPortal == null)
        {
            Debug.LogError("TargetRoom 또는 TargetPortal이 할당되지 않았습니다.");
            return;
        }

        // 이전 방 퇴장 처리
        currentRoom?.OnExitRoom();

        // 새로운 방으로 설정
        currentRoom = targetRoom;

        // 새로운 방 입장 처리
        currentRoom.OnEnterRoom();

        // 카메라 경계 변경
        ChangeCameraConfiner(currentRoom.CameraBound);

        // 플레이어 위치 이동
        Player.Instance.transform.position = targetPortal.transform.position;
    }

    private void ChangeCameraConfiner(Collider2D newBound)
    {
        if (VirtualCamera.Instance != null)
        {
            VirtualCamera.Instance.ChangeConfiner(newBound);
        }
    }
}
