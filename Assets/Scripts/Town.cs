using System.Collections.Generic;
using UnityEngine;

public class Town : MonoBehaviour
{
    [Header("마을 정보")]
    [SerializeField] public string TownName;

    [Header("마을 구성")]
    [SerializeField] public List<Room> Rooms; // 이 마을을 구성하는 방 목록
    [SerializeField] public Vector3 StartPosition; // 마을 시작 시 플레이어 시작 위치

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartTown(this);
        }
        else
        {
            Debug.LogError("GameManager가 씬에 존재하지 않음");
        }
    }
}
