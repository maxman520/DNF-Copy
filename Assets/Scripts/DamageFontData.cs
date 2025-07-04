using UnityEngine;

[CreateAssetMenu(fileName = "DamageFontData", menuName = "Data/Damage Font Data")]
public class DamageFontData : ScriptableObject
{
    // �ν����Ϳ��� 0���� 9���� ������� ��������Ʈ�� �Ҵ�
    public Sprite[] numberSprites = new Sprite[10];
}