using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MinimapIconData", menuName = "UI/Minimap Icon Data")]
public class MinimapIconData : ScriptableObject
{
    public Sprite PlayerIcon;
    public Sprite BossIcon;

    // < �� ��� �����ܵ� > (��� ������ ���⿡ �Ҵ�)
    // �ε��� �� = �� ���� ����
    // Top / Bottom / Left / Right
    // ex) index == 11
    // 11�� �������� 1011.
    // 1011 �̸� Top�� Left, Right�� ���� ����Ǿ� �ִٴ� ��.
    // �׿� �´� ��������Ʈ ����
    public List<Sprite> Inactive; // �ѹ� �湮������ Exit�ϸ鼭 ��Ȱ��ȭ�� ��
    public List<Sprite> Active; // ���� �÷��̾ ��ġ�� ��

    // ���� ������ �������� �ùٸ� �� ��������Ʈ�� ��ȯ�ϴ� �Լ�
    public Sprite GetPathSprite(bool isActive, Room.HasExit hasExit)
    {
        return isActive ? Active[(int)hasExit] : Inactive[(int)hasExit];
    }
}