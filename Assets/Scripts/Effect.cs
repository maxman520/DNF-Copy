using UnityEngine;
using Cysharp.Threading.Tasks;

public class Effect : MonoBehaviour
{
    [Header("����Ʈ �̸� (EffectManager�� ��ϵ� �̸��� �����ؾ� ��)")]
    [SerializeField] private string effectName;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        // ������Ʈ�� Ȱ��ȭ�� ������ �ִϸ��̼��� ó������ ����ϰ�,
        // ���� �� Ǯ�� �ݳ��ϴ� ���� ����
        if (animator == null)
        {
            Debug.Log("����Ʈ�� animator������ �ʱ�ȭ ���� ����!");
            return;
        }
            
        // �ִϸ��̼��� 0�� ���̾��� ó��(0f)���� ���
        animator.Play(0, -1, 0f);
        ReturnToPoolAfterAnimation().Forget();
    }

    private async UniTask ReturnToPoolAfterAnimation()
    {
        // ���� ��� ���� �ִϸ��̼� Ŭ���� ���̸� ������
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
        {
            // Ŭ�� ���̸�ŭ ���
            float animationLength = clipInfo[0].clip.length;
            await UniTask.Delay((int)(animationLength * 1000), cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.ReturnEffectToPool(effectName, this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}