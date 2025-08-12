using UnityEngine;

public class DropGold : MonoBehaviour
{
    public int Amount;

    [Header("중력/점프")]
    [SerializeField] private float gravity;
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
        visualsAngularVelocity = 360 * (Random.value < 0.5f ? -1f : 1f);
    }

    public void Initialize(int amount)
    {
        Amount = Mathf.Max(1, amount);

        var nameLabel = GetComponentInChildren<DropNameLabel>();
        if (nameLabel != null)
        {
            nameLabel.SetText($"{Amount:N0} G");
        }
    }


    private void Update()
    {
        HandleVertical();
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
            var inv = Player.Instance.GetComponent<Inventory>();
            if (inv != null)
            {
                inv.AddGold(Amount);
                Vector3 position = Player.Instance.HurtboxTransform.position;
                position = new Vector3(position.x, position.y + 0.8f, 0);
                
                EffectManager.Instance.PlayEffect("GoldGainText", position, Quaternion.identity, Amount);
            }
            Destroy(gameObject);
        }
    }
}
