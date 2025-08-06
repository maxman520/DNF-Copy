using UnityEngine;

public class DropItem: MonoBehaviour
{
    public ItemData Item;
    public int Quantity = 1;

    [Header("중력/점프")]
    [SerializeField] private float gravity = 21f;
    [SerializeField] private float verticalVelocity;
    private Transform visuals;
    private Rigidbody2D rb;
    private float visualsAngularVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        visuals = transform.Find("Visuals");
        if (visuals != null)
        {
            visuals.localPosition = Vector3.zero;
        }
    }

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = false;
    }

    public void Initialize(ItemData item, int quantity)
    {
        this.Item = item;
        this.Quantity = Mathf.Max(1, quantity);
    }

    public void SetInitialVerticalVelocity(float v0)
    {
        verticalVelocity = Mathf.Max(0f, v0);
    }
    public void SetVisualsAngularVelocity(float av)
    {
        visualsAngularVelocity = av;
    }

    private void Update()
    {
        HandleVertical();
    }

    private void HandleVertical()
    {
        if (visuals == null) return;
        // 이미 바닥에 닿아 있고 아래로 가는 중이면 정지
        if (visuals.localPosition.y <= 0f && verticalVelocity <= 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            visuals.localPosition = new Vector3(visuals.localPosition.x, 0f, visuals.localPosition.z);
            verticalVelocity = 0f;
            return;
        }
        // 중력 적용 및 이동
        verticalVelocity -= gravity * Time.deltaTime;
        visuals.localPosition += new Vector3(0f, verticalVelocity * Time.deltaTime, 0f);
        // 공중에서는 visuals 회전 연출
        if (verticalVelocity != 0f)
        {
            visuals.localRotation *= Quaternion.Euler(0f, 0f, visualsAngularVelocity * Time.deltaTime);
        }
        // 착지 체크
        if (visuals.localPosition.y < 0f && verticalVelocity < 0f)
        {
            visuals.localPosition = new Vector3(visuals.localPosition.x, 0f, visuals.localPosition.z);
            verticalVelocity = 0f;
            // 착지 시 회전 초기화
            transform.rotation = Quaternion.identity;
            visuals.localRotation = Quaternion.identity;
        }
    }

    private bool IsLanded()
    {
        return visuals != null && visuals.localPosition.y <= 0f && Mathf.Approximately(verticalVelocity, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerGround") && IsLanded())
        {
            var inv = Player.Instance.GetComponent<Inventory>();
            if (inv != null)
            {
                for (int i = 0; i < Quantity; i++)
                {
                    inv.AddItem(Item);
                }
            }
            Destroy(gameObject);
        }
    }
}
