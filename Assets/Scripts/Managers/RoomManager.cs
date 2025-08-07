using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : Singleton<RoomManager>
{
    private Room currentRoom;

    // 플레이어 위치를 기반으로 CurrentRoom을 업데이트
    public void UpdateCurrentRoom(Room currentRoom)
    {
        this.currentRoom = currentRoom;
    }

    // 같은 씬 내 목표 방으로 이동하는 함수. Portal이 호출
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

    public void ChangeCameraConfiner(Collider2D newBound)
    {
        if (VirtualCamera.Instance != null)
        {
            VirtualCamera.Instance.ChangeConfiner(newBound);
            VirtualCamera.Instance.SetFollowTarget(Player.Instance.transform);
        }
    }
}
