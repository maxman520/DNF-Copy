using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MinimapIconData", menuName = "UI/Minimap Icon Data")]
public class MinimapIconData : ScriptableObject
{
    public Sprite PlayerIcon;
    public Sprite BossIcon;

    // < 길 모양 아이콘들 > (모든 조합을 여기에 할당)
    // 인덱스 값 = 방 연결 정보
    // Top / Bottom / Left / Right
    // ex) index == 11
    // 11은 이진수로 1011.
    // 1011 이면 Top과 Left, Right에 방이 연결되어 있다는 뜻.
    // 그에 맞는 스프라이트 설정
    public List<Sprite> Inactive; // 한번 방문했으나 Exit하면서 비활성화된 방
    public List<Sprite> Active; // 지금 플레이어가 위치한 방

    // 연결 정보를 바탕으로 올바른 길 스프라이트를 반환하는 함수
    public Sprite GetPathSprite(bool isActive, Room.HasExit hasExit)
    {
        return isActive ? Active[(int)hasExit] : Inactive[(int)hasExit];
    }
}