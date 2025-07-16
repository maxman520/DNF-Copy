using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;


public enum RoomType { Normal, Start, Boss } // 방의 종류

public class Room : MonoBehaviour
{
    [Header("방 설정")]
    [SerializeField] public RoomType roomType;
    [SerializeField] public CinemachineCamera virtualCamera; // 이 방에서 사용할 가상 카메라
    [SerializeField] private List<Monster> monsters; // 이 방에 있는 모든 몬스터 리스트
    [SerializeField] private List<Portal> portals; // 이 방의 모든 포탈 리스트

    private bool isCleared = false;

    public void OnEnable()
    {
        // 방이 활성화 되는 순간 Virtual_Camera의 Follow 타겟을 플레이어로 설정
        virtualCamera.Follow = Player.Instance.transform;
    }

    // 방이 활성화될 때 호출
    public void OnEnterRoom()
    {
        Debug.Log($"{this.name}에 입장");

        // 이 방의 모든 것을 활성화
        this.gameObject.SetActive(true);

        // 입장하면서 카메라 우선순위를 높여 카메라 변경
        if (virtualCamera != null)
            virtualCamera.Priority = 10;
    }

    // 방을 나갈 때 호출
    public void OnExitRoom()
    {
        Debug.Log($"{this.name}에서 퇴장");

        // 이 방의 모든 것을 비활성화하여 연산을 멈춤
        this.gameObject.SetActive(false);

        // 다른 카메라로 변경하기 위해 카메라 우선순위 낮춤
        if (virtualCamera != null)
            virtualCamera.Priority = 9;
    }

    void Update()
    {
        // 방이 클리어되지 않았을 때만 몬스터 생존 여부 체크
        if (!isCleared)
        {
            CheckClearCondition();
        }
    }

    void CheckClearCondition()
    {
        // 모든 몬스터가 죽었는지 확인
        foreach (var monster in monsters)
        {
            // 아직 살아있는 몬스터가 한 마리라도 있으면 함수 종료
            if (monster != null && monster.gameObject.activeSelf) // 죽으면 비활성화되거나 파괴된다는 가정
            {
                return;
            }
        }

        // 모든 몬스터가 죽었음
        isCleared = true;
        OnRoomCleared();
    }

    void OnRoomCleared()
    {
        Debug.Log($"{this.name} 클리어!");

        // 모든 포탈을 활성화
        foreach (var portal in portals)
        {
            if (portal != null)
                portal.Activate();
        }
    }
}