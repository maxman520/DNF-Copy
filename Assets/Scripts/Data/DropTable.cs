using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Drop Table", menuName = "Data/Drop Table")]
public class DropTable : ScriptableObject
{
    [Serializable]
    public class ItemEntry
    {
        public ItemData Item;
        [Tooltip("가중치. 높을수록 잘 나옴")]
        public int Weight = 1;
        public Vector2Int QuantityRange = new Vector2Int(1, 1);
    }

    [Serializable]
    public class GoldEntry
    {
        [Tooltip("가중치. 높을수록 잘 나옴")]
        public int Weight = 1;
        public Vector2Int AmountRange = new Vector2Int(10, 30);
    }

    [Header("아이템 드랍 테이블")]
    public List<ItemEntry> ItemEntries = new List<ItemEntry>();

    [Header("골드 드랍 설정")]
    public GoldEntry Gold = new GoldEntry();

    [Header("아무것도 나오지 않을 확률(가중치)")]
    [Tooltip("아이템/골드와 동일한 방식의 상대 가중치. 값이 클수록 '드랍 없음'이 더 자주 발생합니다.")]
    public int EmptyWeight = 0;

    [Header("드랍 개수/연출")]
    [Tooltip("최소~최대 드랍 개수 (아이템+골드 합)")]
    public Vector2Int DropCountRange = new Vector2Int(0, 2);
    [Tooltip("드랍 시 퍼지는 반경")]
    public float DropSpreadRadius = 0.5f;
}
