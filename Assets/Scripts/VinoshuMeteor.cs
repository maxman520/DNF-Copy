using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Unity.Cinemachine;

public class VinoshuMeteor : MonoBehaviour
{
    private AttackDetails attackDetails; // �� ���׿��� ȣ���� ������ ���� ����
    Vector3 origin; // ��Ʈ�ڽ� ���� ó�� ���� ��ġ

    private float fallSpeed = 8f;
    private bool isFalling = false;

    [SerializeField] private CinemachineImpulseSource impulseSource; // ī�޶� ��鸲
    private Transform visualsTransform;
    private MonsterHitbox meteorHitbox; // Visuals�� ��Ʈ�ڽ� ��ũ��Ʈ ����

    public void Awake()
    {
        this.visualsTransform = transform.Find("Visuals");
        meteorHitbox = visualsTransform.GetComponent<MonsterHitbox>();
        if (impulseSource == null)
            impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    // Vinoshu�� �� �Լ��� ȣ���Ͽ� ���׿��� ���۽�Ŵ
    public void Initialize(AttackDetails details, Vector3 origin)
    {
        this.visualsTransform.localPosition = new Vector3 (8f, 8f ,0);
        this.attackDetails = details;
        this.origin = origin;
        isFalling = true;

        var token = this.GetCancellationTokenOnDestroy();
        Glow(token).Forget();

        // Visuals�� ��Ʈ�ڽ��� ���� ������ �����Ͽ� �ʱ�ȭ
        if (meteorHitbox != null)
        {
            meteorHitbox.Initialize(this.attackDetails, origin);
            Debug.Log("origin position " + origin.x + " " + origin.y);
        }
        else
        {
            Debug.LogError("���׿��� Visuals�� MonsterHitbox ��ũ��Ʈ�� �����ϴ�!");
        }
    }

    void Update()
    {
        if (isFalling)
        {
            // �ܼ��ϰ� Ÿ���� ���� ��� �̵�
            visualsTransform.localPosition = Vector3.MoveTowards(visualsTransform.localPosition, Vector3.zero, fallSpeed * Time.deltaTime);

            // Ÿ�ٿ� ���� �����ߴٸ� ����
            if (Vector3.Distance(visualsTransform.localPosition, Vector3.zero) < 0.1f)
            {
                Explode();
                isFalling = false; // �ߺ� ���� ����
            }
        }
    }

    private void Explode()
    {
        GameObject meteorExplosion = EffectManager.Instance.PlayEffect("FireExplosion", transform.position, Quaternion.identity);
        attackDetails.yOffset += 0.3f; // ���� ����Ʈ�� y�� ������ ���׿� ��ü�� y�� �������� �а�
        meteorExplosion?.GetComponentInChildren<MonsterHitbox>().Initialize(attackDetails, origin);
        
        if (impulseSource != null) // ī�޶� ��鸲. ��鸲�� x������ ��������
            impulseSource.GenerateImpulse(new Vector3((Random.value < 0.5f ? -1 : 1), 1, 0));

        Destroy(gameObject);
    }

    // �������� ���� ���� �ܻ� ����Ʈ
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