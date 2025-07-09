using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class MonsterHPBar : MonoBehaviour
{
    [SerializeField] private Image faceImage;
    [SerializeField] private Image hpOverallImage; // ��ü HP ��
    [SerializeField] private Image hpFillFrontImage; // ���� HP ��
    [SerializeField] private Image hpFillBackImage;  // ���� HP ��
    [SerializeField] private Image hpFlashImage;   // ��ü HP ���� �̹���

    [Header("HP �� ���� (��������Ʈ)")]
    [SerializeField] private List<Sprite> hpBarSprites;

    [Header("�ִϸ��̼� ����")]
    [SerializeField] private GameObject hpFlashPrefab;
    [SerializeField] private Transform hpFlashParent;
    [SerializeField] private float flashDuration = 0.4f;       // ��ü HP �ִϸ��̼� ������ ������� �ð�

    [Header("�ܻ� ȿ�� ����")]
    [SerializeField] private float animDuration = 0.5f; // �ܻ��� ������� �� �ɸ��� �ð�
    
    private CanvasGroup canvasGroup;
    private CancellationTokenSource fillCts; // ���빰 �ִϸ��̼� ��ҿ� ��ū
    
    private Monster currentTarget; // ���� HP���� ������ ��������

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // �ʻ�ȭ ����
    public void SetFace(Sprite portraitSprite)
    {
        if (faceImage != null && portraitSprite != null)
        {
            faceImage.sprite = portraitSprite;
        }
    }

    
    // HP ������Ʈ
    public void UpdateHP(float maxHP,  float previousHP, float currentHP,  float hpPerLine)
    {
        if (hpOverallImage == null) return;
        if (hpFillFrontImage == null || hpFillBackImage == null || hpBarSprites.Count == 0) return;

        // --- 1. ��ü HP �� ũ�� �� ���� ó�� ---
        float currentTotalHPRatio = currentHP / maxHP;
        float previousTotalHPRatio = previousHP / maxHP;

        // ��ü HP �� fillAmount ��� ����
        hpOverallImage.fillAmount = currentTotalHPRatio;

        // ��ü HP ���� �ִϸ��̼� ����
        OverallFlashAsync(previousTotalHPRatio).Forget();

        // currentHP�� 0�� �� FloorToInt�� -1�� ��ȯ�ϴ� ���� ����
        currentHP = Mathf.Max(0, currentHP - 0.001f);

        // 1. ���� �� ��° ������ ��� (0���� ����. 0 = ����, 1 = ��Ȳ, ...)
        int currentLineIndex = Mathf.FloorToInt(currentHP / hpPerLine);

        // 2. ���� ���� ���� HP ���� ���
        float hpInCurrentLine = currentHP % hpPerLine;
        if (hpInCurrentLine == 0)
        {
            // ������ ������ 0�̶�� ���� �ش� ���� ü���� �� á�ٴ� �ǹ�
            hpInCurrentLine = hpPerLine;
        }
        float newFillAmount = hpInCurrentLine / hpPerLine;

        // 3. ���� ������ ���
        int colorCount = hpBarSprites.Count;
        int frontColorIndex = currentLineIndex % colorCount;
        int backColorIndex = (currentLineIndex - 1) >= 0 ? (currentLineIndex - 1) % colorCount : 0;

        // ���� Front ���� fillAmount�� Flash �ٿ� ���� ����
        hpFillFrontImage.sprite = hpBarSprites[frontColorIndex];

        // ���� �� ���� (������ ���� ���� �޹���� ������ ��)
        if (currentLineIndex > 0) // ���� ���� �ε����� 0���� ũ�ٸ� (��, ������ ���� �ƴ϶��)
        {
            hpFillBackImage.enabled = true;
            // ���� ��(�ε��� - 1)�� ������ ������� ���
            hpFillBackImage.sprite = hpBarSprites[backColorIndex];
        }
        else
        {
            // ������ ��(�ε��� 0)�� ���� �� �ٸ� ��Ȱ��ȭ
            hpFillBackImage.enabled = false;
        }

        // �ִϸ��̼� ����
        FillAnimationAsync(newFillAmount).Forget();

        // ü���� 0�� �Ǹ� ���� �ٵ� ��Ȱ��ȭ
        if (currentHP <= 0)
        {
            hpFillFrontImage.enabled = false;
        }
        else
        {
            hpFillFrontImage.enabled = true;
        }
    }

    // �÷��̾�� �������� �޾� �ش� ���� ���� HP�ٰ� �پ��� �ִϸ��̼�
    private async UniTask FillAnimationAsync(float newFillAmount)
    {
        fillCts?.Cancel();
        fillCts?.Dispose();

        var destroyToken = this.GetCancellationTokenOnDestroy();
        fillCts = CancellationTokenSource.CreateLinkedTokenSource(destroyToken);
        var token = fillCts.Token;

        float oldFillAmount = hpFillFrontImage.fillAmount;
        
        try
        {
            // ���� �پ��� ��
            float elapsedTime = 0f;
            while (elapsedTime < animDuration && !token.IsCancellationRequested)
            {
                // ���� �ٿ��� ���� �ٷ� �Ѿ ��, startFillAmount�� targetFillAmount���� ���� �� ����
                // �� ��� �ִϸ��̼� ���� ��� �ݿ��ؾ� �ڿ�������
                if (oldFillAmount < newFillAmount)
                    oldFillAmount = newFillAmount;

                float tmp = Mathf.Lerp(oldFillAmount, newFillAmount, elapsedTime / animDuration);

                // FrontImage�� Color ������Ƽ�� ����
                hpFillFrontImage.fillAmount = tmp;

                elapsedTime += Time.deltaTime;
                await UniTask.Yield(token);
            }
        }
        catch (OperationCanceledException)
        {
            // ���� ó��
        }
        finally
        {
            hpFillFrontImage.fillAmount = newFillAmount;
        }
    }
    // �÷��̾�� �������� �޾� ��ü ���� HP�ٰ� �پ��� �ִϸ��̼�
    private async UniTaskVoid OverallFlashAsync(float previousFillRatio)
    {
        // 1. ����� �̹��� ����
        var flash = EffectManager.Instance.PlayEffect("MonsterHPBarFlash", hpFlashParent);

        Image flashImage = flash.GetComponent<Image>();
        flashImage.fillAmount = previousFillRatio;
        flashImage.enabled = true;
        flashImage.color = Color.white;

        // 2. ���� �ִϸ��̼�
        float elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / flashDuration);
            flashImage.color = new Color(1, 1, 1, alpha);
            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }

        // 3. ����
        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.ReturnEffectToPool("MonsterHPBarFlash", flash);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ��� Ȱ�� �÷��� ������Ʈ�� �ı��ϴ� ���� �Լ�

    // UI ���̱�
    public void Show(Monster newTarget)
    {
        // �� ���� ������ ��� ������ ���� flash ����Ʈ ����
        bool targetChanged = (currentTarget != null && currentTarget != newTarget);
        bool hasActiveFlashes = EffectManager.Instance.GetActiveEffectCount("MonsterHPBarFlash") > 0;
        
        if (targetChanged && hasActiveFlashes)
            EffectManager.Instance.ClearEffectsByName("MonsterHPBarFlash");

        // ���ο� Ÿ�� ����
        this.currentTarget = newTarget;

        canvasGroup.alpha = 1f;
    }
    

    // UI �����
    public void Hide()
    {
        canvasGroup.alpha = 0f;
    }
}