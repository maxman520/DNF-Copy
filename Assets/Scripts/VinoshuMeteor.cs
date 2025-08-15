using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Unity.Cinemachine;

public class VinoshuMeteor : MonoBehaviour
{
    private AttackDetails attackDetails; // 이 메테오를 호출한 몬스터의 공격 정보
    Vector3 origin; // 히트박스 판정 처리 기준 위치

    private float fallSpeed = 8f;
    private bool isFalling = false;

    [SerializeField] private CinemachineImpulseSource impulseSource; // 카메라 흔들림
    private Transform visualsTransform;
    private MonsterHitbox meteorHitbox; // Visuals의 히트박스 스크립트 참조

    public void Awake()
    {
        this.visualsTransform = transform.Find("Visuals");
        meteorHitbox = visualsTransform.GetComponent<MonsterHitbox>();
        if (impulseSource == null)
            impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    // Vinoshu가 이 함수를 호출하여 메테오를 시작시킴
    public void Initialize(AttackDetails details, Vector3 origin)
    {
        this.visualsTransform.localPosition = new Vector3 (8f, 8f ,0);
        this.attackDetails = details;
        this.origin = origin;
        isFalling = true;

        var token = this.GetCancellationTokenOnDestroy();
        Glow(token).Forget();

        // Visuals의 히트박스에 공격 정보를 전달하여 초기화
        if (meteorHitbox != null)
        {
            meteorHitbox.Initialize(this.attackDetails, origin);
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
            visualsTransform.localPosition = Vector3.MoveTowards(visualsTransform.localPosition, Vector3.zero, fallSpeed * Time.deltaTime);

            // 타겟에 거의 도착했다면 폭발
            if (Vector3.Distance(visualsTransform.localPosition, Vector3.zero) < 0.1f)
            {
                isFalling = false; // 중복 폭발 방지
                Explode();
            }
        }
    }

    private void Explode()
    {
        GameObject meteorExplosion = EffectManager.Instance.PlayEffect("FireExplosion", transform.position, Quaternion.identity);
        AudioManager.Instance.PlaySFX("Sstar_Hit");
        MonsterHitbox explosionHitbox = null;
        attackDetails.yOffset += 0.3f; // 폭발 이펙트의 y축 범위는 메테오 자체의 y축 범위보다 넓게
        
        if (meteorExplosion != null)
            // 비활성화된 자식까지 포함하여 검색
            explosionHitbox = meteorExplosion.GetComponentInChildren<MonsterHitbox>(true);
        
        if (explosionHitbox != null)
            explosionHitbox.Initialize(attackDetails, origin);
        else Debug.Log("폭발의 히트박스가 Initialize되지 않음");
        
        if (impulseSource != null) // 카메라 흔들림. 흔들림의 x방향은 랜덤으로
            impulseSource.GenerateImpulse(new Vector3((Random.value < 0.5f ? -1 : 1), 1, 0));

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
