using UnityEngine;

public class DropItem: MonoBehaviour
{
    public ItemData Item;
    public int Quantity = 1;

    [Header("중력/점프")]
    [SerializeField] private float gravity;
    [SerializeField] private float verticalVelocity;
    private Transform visuals;
    private Rigidbody2D rb;
    private float visualsAngularVelocity;

    // 플레이어 근접 및 획득 관련
    private DropNameLabel nameLabel;
    private SpriteRenderer nameBg;
    [SerializeField] private Sprite highlightBgSprite;
    private Sprite originalBgSprite;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        visuals = transform.Find("Visuals");
        if (visuals != null)
        {
            visuals.localPosition = Vector3.zero;
        }
        visualsAngularVelocity = 360 * (Random.value < 0.5f ? -1f : 1f);

        nameLabel = GetComponentInChildren<DropNameLabel>();
        if (nameLabel != null)
        {
            nameBg = nameLabel.GetComponentInChildren<SpriteRenderer>();
            if (nameBg != null)
            {
                originalBgSprite = nameBg.sprite;
            }
        }
    }

    public void Initialize(ItemData item, int quantity)
    {
        this.Item = item;
        this.Quantity = Mathf.Max(1, quantity);

        if (nameLabel != null)
        {
            string txt = (this.Quantity > 1) ? $"{this.Item.ItemName} ({this.Quantity}EA)" : this.Item.ItemName;
            nameLabel.SetText(txt);
        }
    }

    private void Update()
    {
        HandleVertical();
    }

    public void Pickup()
    {
        var inv = Player.Instance.PlayerInventory;
        if (inv != null)
        {
            for (int i = 0; i < Quantity; i++)
            {
                inv.AddItem(Item);
            }
        }
        Destroy(gameObject);
    }

    private void HandleVertical()
    {
        if (visuals == null) return;
        // 이미 바닥에 닿아 있고 아래로 가는 중이면 정지
        if (IsLanded())
        {
            rb.linearVelocity = Vector2.zero;
            visuals.localPosition = Vector3.zero;
            verticalVelocity = 0f;
            // 착지 시 회전 초기화
            transform.rotation = Quaternion.identity;
            visuals.localRotation = Quaternion.identity;
            return;
        }
        // 중력 적용 및 이동
        verticalVelocity -= gravity * Time.deltaTime;
        visuals.localPosition += new Vector3(0f, verticalVelocity * Time.deltaTime, 0f);
        // 공중에서는 visuals 회전 연출 (높이가 0보다 클 때 회전 유지)
        visuals.localRotation *= Quaternion.Euler(0f, 0f, visualsAngularVelocity * Time.deltaTime);
    }

    private bool IsLanded()
    {
        return visuals != null && visuals.localPosition.y <= 0f && verticalVelocity <= 0;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("PlayerGround") && IsLanded())
        {
            Player.Instance.ItemToPickUp = this;
            if (nameBg != null && highlightBgSprite != null)
            {
                nameBg.sprite = highlightBgSprite;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("PlayerGround"))
        {
            if (Player.Instance.ItemToPickUp == this)
            {
                Player.Instance.ItemToPickUp = null;
            }
            if (nameBg != null)
            {
                nameBg.sprite = originalBgSprite;
            }
        }
    }
}
