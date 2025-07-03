using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class MonsterFragment : MonoBehaviour
{
    [Header("���� ����")]
    [SerializeField] private float lifeTime = 5.0f; // �� ���� �ð�
    [SerializeField] private float fadeOutDuration = 1.0f; // ������� �� �ɸ��� �ð�
    private const float GRAVITY = 21f; // ���� ����� ���� �߷�
    private float verticalVelocity; // ���� �ӵ�

    [Header("����")]
    private Rigidbody2D rb;
    public Transform VisualTransform;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        VisualTransform = transform.Find("Visual");
        spriteRenderer = VisualTransform.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // ������ ���ÿ� �ڵ� �ı� ������ ����
        FadeAndDestroySequence(this.GetCancellationTokenOnDestroy()).Forget();
    }

    void Update()
    {
        // ���� �߷� ����
        HandleGravity();
    }

    // �� ���� �ʱ� ���� ���ϴ� public �Լ�
    public void Initialize(Vector2 horizontalForce, float verticalForce)
    {
        // ����/���� �̵��� ��Ʈ�� Rigidbody�� ���
        rb.AddForce(horizontalForce, ForceMode2D.Impulse);

        // �������� �ڱ�ġ�� ���� Visuals�� verticalVelocity�� ���
        this.verticalVelocity = verticalForce;
    }

    private void HandleGravity()
    {
        // ���������� �� �̻� ������� ����
        if (VisualTransform.localPosition.y <= 0f && verticalVelocity < 0)
        {
            rb.linearVelocity = Vector2.zero;
            VisualTransform.localPosition = Vector3.zero;
            return;
        }

        verticalVelocity -= GRAVITY * Time.deltaTime;
        VisualTransform.localPosition += new Vector3(0, verticalVelocity * Time.deltaTime, 0);
    }
    private async UniTask FadeAndDestroySequence(CancellationToken token)
    {
        float aliveDuration = lifeTime - fadeOutDuration;
        if (aliveDuration > 0)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(aliveDuration), cancellationToken: token);
        }

        // ������ ������� ȿ��
        float elapsedTime = 0f;
        Color startColor = spriteRenderer.color;

        while (elapsedTime < fadeOutDuration)
        {
            float alpha = 1.0f - (elapsedTime / fadeOutDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            elapsedTime += Time.deltaTime;
            await UniTask.Yield(token);
        }
        Destroy(gameObject);
    }
}