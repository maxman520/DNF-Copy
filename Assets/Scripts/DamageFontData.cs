using UnityEngine;

[CreateAssetMenu(fileName = "DamageFontData", menuName = "Data/Damage Font Data")]
public class DamageFontData : ScriptableObject
{
    // 인스펙터에서 0부터 9까지 순서대로 스프라이트를 할당
    public Sprite[] numberSprites = new Sprite[10];
}