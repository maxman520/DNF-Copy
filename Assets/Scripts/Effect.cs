using UnityEngine;
using Cysharp.Threading.Tasks;

public class Effect : MonoBehaviour
{
    [Header("이펙트 이름 (EffectManager에 등록된 이름과 동일해야 함)")]
    [SerializeField] private string effectName;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        // 오브젝트가 활성화될 때마다 애니메이션을 처음부터 재생하고,
        // 끝날 때 풀에 반납하는 로직 실행
        if (animator == null)
        {
            Debug.Log(this.effectName + " 이펙트의 animator변수가 초기화 되지 않음!");
            return;
        }
            
        // 애니메이션을 0번 레이어의 처음(0f)부터 재생
        animator.Play(0, -1, 0f);
        ReturnToPoolAfterAnimation().Forget();
    }

    private async UniTask ReturnToPoolAfterAnimation()
    {
        // 현재 재생 중인 애니메이션 클립의 길이를 가져옴
        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
        {
            // 클립 길이만큼 대기
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