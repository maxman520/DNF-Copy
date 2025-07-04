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
    [SerializeField] private float duration = 0.2f;     // ��ü ���� �ð�
    [SerializeField] private float fadeOutDelay = 0.15f; // ������� �����ϴ� �ð�

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

    // ���� �������� ������� �ִϸ��̼�
    private async UniTask Animate()
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

        // �ִϸ��̼��� ������ Ǯ�� �ݳ�
        EffectManager.Instance.ReturnEffectToPool("DamageText", this.gameObject);
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