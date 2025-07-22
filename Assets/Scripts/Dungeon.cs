using UnityEngine;
using System.Collections.Generic;

public class Dungeon : MonoBehaviour
{
    [Header("던전 정보")]
    [SerializeField] public string DungeonName;
    [SerializeField] public int ClearEXP;
    [Header("랭크 시스템")]
    [Tooltip("랭크 판별 시간 기준 (초). 가장 빠른 시간부터 순서대로 정렬")]
    [SerializeField] public List<float> RankTimeThresholds;
    [Tooltip("시간 기준에 해당하는 랭크 스프라이트. 위 리스트와 순서/개수 일치 필요")]
    [SerializeField] public List<Sprite> RankSprites;
    [Header("던전 구성")]
    [SerializeField] public List<Room> Rooms; // 이 던전을 구성하는 방 목록
    [SerializeField] public Vector3 StartPosition; // 던전 시작 시 플레이어 시작 위치
    [SerializeField] public Vector2Int MapSize; // 맵의 크기 (예: X=3, Y=3)
    [Tooltip("다음 던전의 씬 이름")]
    [SerializeField] public string NextDungeonName; // 다음 던전의 씬 이름
    [Tooltip("이 던전이 끝나고 돌아갈 마을의 씬 이름")]
    [SerializeField] public string TownToReturn; // 던전이 끝나고 돌아가야할 마을의 씬 이름

    private void Start()
    {
        // 이 던전 씬이 시작될 때, DungeonManager에게 자신을 등록하고 시작을 요청
        if (DungeonManager.Instance != null)
        {
            DungeonManager.Instance.StartDungeon(this);
        }
        else
        {
            Debug.LogError("DungeonManager가 씬에 존재하지 않음");
        }
    }
}