using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class DamageText : MonoBehaviour
{
    [Header("����")]
    [SerializeField] private DamageFontData fontData; // ����� ��Ʈ ������
    [SerializeField] private List<SpriteRenderer> numberRenderers; // ���� �ڸ��� ��������

    [Header("�ִϸ��̼� ����")]
    [SerializeField] private float moveDistance = 0.5f; // ���� ������ �Ÿ�
    [SerializeField] private float duration = 0.8f;     // ��ü ���� �ð�
    [SerializeField] private float fadeOutDelay = 0.5f; // ������� �����ϴ� �ð�

    [Header("ũ�� �ִϸ��̼� ����")]
    [SerializeField] private Vector3 startScale = new Vector3(10f, 10f, 1f); // ���� �� ũ�� ����
    [SerializeField] private float scaleInDuration = 0.15f; // ���� ũ��� ���ƿ��� �� �ɸ��� �ð�

    [Header("������ �ؽ�Ʈ �̸� (EffectManager�� ��ϵ� �̸��� �����ؾ� ��)")]
    [SerializeField] private string effectName;

    // ������ ���� �޾ƿͼ� ��������Ʈ�� ��ȯ�Ͽ� ǥ��
    public void SetDamageAndPlay(int damage)
    {
        string damageString = damage.ToString();

        // ��� ���� �������� �ϴ� ��Ȱ��ȭ�ϰ� ���İ� �ʱ�ȭ
        foreach (var renderer in numberRenderers)
        {
            renderer.gameObject.SetActive(false);
            Color color = renderer.color;
            color.a = 1f; // ���İ� ����
            renderer.color = color;
        }

        // ������ ���ڿ��� �� ���ڿ� �ش��ϴ� ��������Ʈ�� �����ϰ� Ȱ��ȭ
        for (int i = 0; i < damageString.Length; i++)
        {
            if (i >= numberRenderers.Count) break; // �غ�� �ڸ����� ������ �ߴ�

            int number = int.Parse(damageString[i].ToString());
            numberRenderers[i].sprite = fontData.numberSprites[number];
            numberRenderers[i].gameObject.SetActive(true);
        }

        // �ִϸ��̼� ����
        Animate().Forget();
    }

    // ���� �ִϸ��̼� �Լ�: �� ���� �������� ���������� ȣ��
    private async UniTask Animate()
    {

        // ������ 1: ũ�� ���� �ִϸ��̼� ���� �� �Ϸ� ���
        await ScaleInAnimation();

        // ������ 2: �̵� �� �Ҹ� �ִϸ��̼� ���� �� �Ϸ� ���
        await MoveAndFadeAnimation();

        // �ִϸ��̼��� ������ Ǯ�� �ݳ�
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.ReturnEffectToPool(effectName, this.gameObject);
        }
        else
        {
            // �Ŵ����� ������ �׳� �ı�
            Destroy(gameObject);
        }
    }

    // ������ 1: ū ũ�⿡�� ���� ũ��(1,1,1)�� �پ��� �ִϸ��̼�
    private async UniTask ScaleInAnimation()
    {
        float elapsedTime = 0f;
        Vector3 originalScale = Vector3.one; // ��ǥ ũ�� (1, 1, 1)

        while (elapsedTime < scaleInDuration)
        {
            // ����� (0 -> 1)
            float progress = elapsedTime / scaleInDuration;

            // Ease-Out ȿ���� ���� ������� ���� (ó���� ������ ���߿� ������)
            // 1 - (1-x)^n ���� ���
            float easedProgress = 1 - Mathf.Pow(1 - progress, 3); // 3�� ���� ����

            // ũ�⸦ ���� ũ�⿡�� ��ǥ ũ��� ����
            transform.localScale = Vector3.Lerp(startScale, originalScale, easedProgress);

            elapsedTime += Time.deltaTime;
            await UniTask.Yield(this.GetCancellationTokenOnDestroy());
        }

        // �ִϸ��̼��� ������ ũ�⸦ ��Ȯ�� ������� ����
        transform.localScale = originalScale;
    }

    // ������ 2: ���� �������� ������� �ִϸ��̼� (���� ����)
    private async UniTask MoveAndFadeAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * moveDistance;
        float elapsedTime = 0f;

        // ���� �������� ������
        while (elapsedTime < duration)
        {
            float easedProgress = Mathf.Pow(elapsedTime / duration, 2.5f); // ó���� ������ �������� ���� ���� ����������

            transform.position = Vector3.Lerp(startPos, endPos, easedProgress);

            if (elapsedTime > fadeOutDelay)
            {
                float fadeProgress = (elapsedTime - fadeOutDelay) / (duration - fadeOutDelay);
                SetAlpha(1f - fadeProgress);
            }

            elapsedTime += Time.deltaTime;
            await UniTask.Yield(this.GetCancellationTokenOnDestroy());
        }
    }

    // ��� ���� ��������Ʈ�� ����(����) ���� ����
    private void SetAlpha(float alpha)
    {
        foreach (var renderer in numberRenderers)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }
}