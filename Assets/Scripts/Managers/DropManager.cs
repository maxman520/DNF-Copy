using UnityEngine;
using Cysharp.Threading.Tasks;

public class DropManager : Singleton<DropManager>
{
    [Header("프리팹 참조")]
    public GameObject DropItemPrefab;
    public GameObject DropGoldPrefab;

    [Header("물리 설정")]
    [SerializeField] private float launchForce = 3.0f;
    [SerializeField] private float horizontalJitter = 1.5f;

    public void SpawnDrops(DropTable table, Vector3 origin)
    {
        if (table == null)
        {
            return;
        }

        int dropCount = Random.Range(table.DropCountRange.x, table.DropCountRange.y + 1);
        for (int i = 0; i < dropCount; i++)
        {
            // 아이템과 골드 중 무엇을 뽑을지 결정 (간단 가중치: 아이템 테이블 합 vs 골드 가중치)
            int itemWeightSum = 0;
            foreach (var e in table.ItemEntries) itemWeightSum += Mathf.Max(0, e.Weight);
            int goldWeight = Mathf.Max(0, table.Gold.Weight);
            int emptyWeight = Mathf.Max(0, table.EmptyWeight);
            int total = itemWeightSum + goldWeight + emptyWeight;
            if (total <= 0) continue;

            int pick = Random.Range(1, total + 1);
            if (pick <= itemWeightSum)
            {
                // 아이템 한 개 선택
                var entry = PickItemEntry(table);
                if (entry != null && entry.Item != null)
                {
                    int qty = Random.Range(entry.QuantityRange.x, entry.QuantityRange.y + 1);
                    SpawnDropItem(entry.Item, qty, origin, table.DropSpreadRadius);
                }
            }
            else if (pick <= itemWeightSum + goldWeight)
            {
                // 골드
                int amount = Random.Range(table.Gold.AmountRange.x, table.Gold.AmountRange.y + 1);
                SpawnDropGold(amount, origin, table.DropSpreadRadius);
            }
            else
            {
                // '아무것도 나오지 않음' 구간: 스킵
            }
        }
    }

    private DropTable.ItemEntry PickItemEntry(DropTable table)
    {
        int sum = 0;
        foreach (var e in table.ItemEntries) sum += Mathf.Max(0, e.Weight);
        if (sum <= 0) return null;
        int roll = Random.Range(1, sum + 1);
        int acc = 0;
        foreach (var e in table.ItemEntries)
        {
            acc += Mathf.Max(0, e.Weight);
            if (roll <= acc) return e;
        }
        return null;
    }

    private void ApplyLaunch(Rigidbody2D rb, Transform visualsTransform, out float initialVerticalVelocity, out float visualsAngularVelocity)
    {
        initialVerticalVelocity = 0f;
        visualsAngularVelocity = 0f;
        if (rb == null) return;
        float dir = Random.value < 0.5f ? -1f : 1f;
        float h = Random.Range(0.5f, horizontalJitter) * dir;
        // 수평 이동은 Rigidbody가 담당
        rb.AddForce(new Vector2(h, 0f), ForceMode2D.Impulse);
        // 시각적 회전은 Visuals가 담당하므로 각속도만 전달
        visualsAngularVelocity = 360f * dir;
        // 공중으로 솟구치는 힘은 Visuals의 verticalVelocity가 담당하므로 초기값만 반환
        initialVerticalVelocity = launchForce;
    }

    private void SpawnDropItem(ItemData item, int quantity, Vector3 origin, float spread)
    {
        if (DropItemPrefab == null) return;
        Vector3 pos = origin + (Vector3)(Random.insideUnitCircle * spread);
        var go = Instantiate(DropItemPrefab, pos, Quaternion.identity);
        var wi = go.GetComponent<DropItem>();
        if (wi != null)
        {
            wi.Initialize(item, quantity);
        }
        var rb = go.GetComponent<Rigidbody2D>();
        float v0; float av;
        var visuals = go.transform.Find("Visuals");
        ApplyLaunch(rb, visuals, out v0, out av);
        // DropItem에 초기 verticalVelocity와 visuals 각속도 전달
        if (wi != null)
        {
            wi.SetInitialVerticalVelocity(v0);
            wi.SetVisualsAngularVelocity(av);
        }

        // 스프라이트 적용: DropSprite 우선, 없으면 ItemIcon
        var sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = item.DropSprite != null ? item.DropSprite : item.ItemIcon;
        }
    }

    private void SpawnDropGold(int amount, Vector3 origin, float spread)
    {
        if (DropGoldPrefab == null) return;
        Vector3 pos = origin + (Vector3)(Random.insideUnitCircle * spread);
        var go = Instantiate(DropGoldPrefab, pos, Quaternion.identity);
        var wg = go.GetComponent<DropGold>();
        if (wg != null)
        {
            wg.Initialize(amount);
        }
        var rb = go.GetComponent<Rigidbody2D>();
        float v0; float av;
        var visuals = go.transform.Find("Visuals");
        ApplyLaunch(rb, visuals, out v0, out av);
        if (wg != null)
        {
            wg.SetInitialVerticalVelocity(v0);
            wg.SetVisualsAngularVelocity(av);
        }
    }
}
