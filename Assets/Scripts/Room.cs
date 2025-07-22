using System;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public enum RoomType { Normal, Start, Boss } // 방의 종류

    [Flags]
    public enum HasExit // 방 연결 정보
    {
        None = 0,
        Right = 1 << 0,
        Left = 1 << 1,
        Bottom = 1 << 2,
        Top = 1 << 3,
    }

    [Header("방 설정")]
    [SerializeField] public RoomType roomType;
    [SerializeField] public HasExit hasExit;
    //[SerializeField] public CinemachineCamera virtualCamera; // 이 방에서 사용할 가상 카메라
    [SerializeField] public BoxCollider2D CameraBound; // 이 방에서 사용할 카메라 경계
    [SerializeField] private List<Monster> monsters; // 이 방에 있는 모든 몬스터 리스트
    [SerializeField] private List<Portal> portals; // 이 방의 모든 포탈 리스트

    [Header("미니맵 좌표")]
    public Vector2Int coordinates; // 이 방의 미니맵 상 좌표

    private bool isCleared = false;


    // 방이 활성화될 때 호출
    public void OnEnterRoom()
    {
        // 이 방의 모든 것을 활성화
        this.gameObject.SetActive(true);
    }

    // 방을 나갈 때 호출
    public void OnExitRoom()
    {
        // 이 방의 모든 것을 비활성화하여 연산을 멈춤
        this.gameObject.SetActive(false);
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