using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class VinoshuMeteor : MonoBehaviour
{
    private AttackDetails attackDetails;
    private float fallSpeed = 8f;
    private Vector3 targetPosition = Vector3.zero;
    private bool isFalling = false;

    private Transform visualsTransform;
    private MonsterHitbox meteorHitbox; // Visuals의 히트박스 스크립트 참조

    public void Awake()
    {
        this.visualsTransform = transform.Find("Visuals");
        meteorHitbox = visualsTransform.GetComponent<MonsterHitbox>();
    }

    // Vinoshu가 이 함수를 호출하여 메테오를 시작시킴
    public void Initialize(AttackDetails details, Vector3 origin)
    {
        this.visualsTransform.localPosition = new Vector3 (8f, 8f ,0);
        this.attackDetails = details;
        isFalling = true;

        var token = this.GetCancellationTokenOnDestroy();
        Glow(token).Forget();

        // Visuals의 히트박스에 공격 정보를 전달하여 초기화
        if (meteorHitbox != null)
        {
            meteorHitbox.Initialize(this.attackDetails, origin);
            Debug.Log("origin position " + origin.x + " " + origin.y);
        }
        else
        {
            Debug.LogError("메테오의 Visuals에 MonsterHitbox 스크립트가 없습니다!");
        }
    }

    void Update()
    {
        if (isFalling)
        {
            // 단순하게 타겟을 향해 등속 이동
            visualsTransform.localPosition = Vector3.MoveTowards(visualsTransform.localPosition, targetPosition, fallSpeed * Time.deltaTime);

            // 타겟에 거의 도착했다면 폭발
            if (Vector3.Distance(visualsTransform.localPosition, targetPosition) < 0.1f)
            {
                Explode();
                isFalling = false; // 중복 폭발 방지
            }
        }
    }

    private void Explode()
    {
        // GameObject MeteorExplosion = EffectManager.Instance.PlayEffect("MeteorExplosion", transform.position, Quaternion.identity);
        // ...
        Destroy(gameObject);
    }

    // 떨어지고 있을 때의 잔상 이펙트
    private async UniTaskVoid Glow(CancellationToken token)
    {
        while (true)
        {
            await UniTask.Delay(50, cancellationToken: token);

            EffectManager.Instance.PlayEffect("ShootingStarGlow", transform.position + visualsTransform.localPosition, Quaternion.identity);

            await UniTask.Yield(PlayerLoopTiming.Update);
        }
    }
}