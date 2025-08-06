using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class MonsterFragment : MonoBehaviour
{
    [Header("물리 변수")]
    [SerializeField] private float lifeTime = 5.0f; // 총 생존 시간
    [SerializeField] private float fadeOutDuration = 1.0f; // 사라지는 데 걸리는 시간
    private const float GRAVITY = 21f; // 파편에 적용될 가상 중력
    private float verticalVelocity; // 수직 속도

    [Header("참조")]
    private Rigidbody2D rb;
    public Transform VisualTransform;
    private SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        VisualTransform = transform.Find("Visual");
        sr = VisualTransform.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // 생성과 동시에 자동 파괴 시퀀스 시작
        FadeAndDestroySequence(this.GetCancellationTokenOnDestroy()).Forget();
    }

    void Update()
    {
        // 가상 중력 로직
        HandleGravity();
    }

    // 이 파편에 초기 힘을 가하는 public 함수
    public void Initialize(Vector2 horizontalForce, float verticalForce)
    {
        // 수평 이동은 루트의 Rigidbody가 담당
        rb.AddForce(horizontalForce, ForceMode2D.Impulse);

        // 공중으로 솟구치는 힘은 Visuals의 verticalVelocity가 담당
        this.verticalVelocity = verticalForce;
    }

    private void HandleGravity()
    {
        // 착지했으면 더 이상 계산하지 않음
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

        // 서서히 사라지는 효과
        float elapsedTime = 0f;
        Color startColor = sr.color;

        while (elapsedTime < fadeOutDuration)
        {
            float alpha = 1.0f - (elapsedTime / fadeOutDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            elapsedTime += Time.deltaTime;
            await UniTask.Yield(token);
        }
        Destroy(gameObject);
    }
}