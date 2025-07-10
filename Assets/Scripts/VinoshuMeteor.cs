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
    private MonsterHitbox meteorHitbox; // Visuals�� ��Ʈ�ڽ� ��ũ��Ʈ ����

    public void Awake()
    {
        this.visualsTransform = transform.Find("Visuals");
        meteorHitbox = visualsTransform.GetComponent<MonsterHitbox>();
    }

    // Vinoshu�� �� �Լ��� ȣ���Ͽ� ���׿��� ���۽�Ŵ
    public void Initialize(AttackDetails details, Vector3 origin)
    {
        this.visualsTransform.localPosition = new Vector3 (8f, 8f ,0);
        this.attackDetails = details;
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
            visualsTransform.localPosition = Vector3.MoveTowards(visualsTransform.localPosition, targetPosition, fallSpeed * Time.deltaTime);

            // Ÿ�ٿ� ���� �����ߴٸ� ����
            if (Vector3.Distance(visualsTransform.localPosition, targetPosition) < 0.1f)
            {
                Explode();
                isFalling = false; // �ߺ� ���� ����
            }
        }
    }

    private void Explode()
    {
        // GameObject MeteorExplosion = EffectManager.Instance.PlayEffect("MeteorExplosion", transform.position, Quaternion.identity);
        // ...
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