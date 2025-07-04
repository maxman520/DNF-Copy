using UnityEngine;
using System.Collections.Generic;

public class EffectManager : Singleton<EffectManager>
{

    [System.Serializable]
    public class Effect
    {
        public string name;
        public GameObject prefab;
    }

    [Header("����Ʈ ������ ���")]
    [SerializeField] private List<Effect> effectPrefabs = new List<Effect>();

    // ������Ʈ Ǯ���� ���� ��ųʸ�
    private Dictionary<string, Queue<GameObject>> effectPool = new Dictionary<string, Queue<GameObject>>();

    // ����Ʈ �̸��� �������� ��Ī�ϴ� ��ųʸ� (���� �˻���)
    private Dictionary<string, GameObject> effectPrefabDict = new Dictionary<string, GameObject>();

    protected override void Awake()
    {
        // �̱��� ����
        base.Awake();
        InitializePool();
    }

    // ������Ʈ Ǯ�� ������ ��ųʸ� �ʱ�ȭ
    private void InitializePool()
    {
        foreach (var effect in effectPrefabs)
        {
            if (!string.IsNullOrEmpty(effect.name) && effect.prefab != null)
            {
                effectPool[effect.name] = new Queue<GameObject>();
                effectPrefabDict[effect.name] = effect.prefab;
            }
        }
    }

    // ����Ʈ ��� ��û
    public void PlayEffect(string name, Vector3 position, Quaternion rotation)
    {
        if (!effectPrefabDict.ContainsKey(name))
        {
            Debug.LogWarning($"EffectManager: '{name}' �̶�� �̸��� ����Ʈ�� ã�� �� �����ϴ�.");
            return;
        }

        GameObject effectObject;

        // Ǯ�� ��� ������ ������Ʈ�� �ִ��� Ȯ��
        if (effectPool.ContainsKey(name) && effectPool[name].Count > 0)
        {
            effectObject = effectPool[name].Dequeue(); // Ǯ���� ������

            effectObject.transform.position = position;
            effectObject.transform.rotation = rotation;
            effectObject.SetActive(true); // �ٽ� Ȱ��ȭ

        }
        else
        {
            // Ǯ�� ������ ���� ����
            effectObject = Instantiate(effectPrefabDict[name], position, rotation);
        }
    }

    // ������ ��Ʈ ��� ��û
    public void PlayEffect(string name, Vector3 position, Quaternion rotation, float damage)
    {
        position = new Vector3(position.x, position.y + 0.5f, position.z);
        if (!effectPrefabDict.ContainsKey(name))
        {
            Debug.LogWarning($"EffectManager: '{name}' �̶�� �̸��� ����Ʈ�� ã�� �� �����ϴ�.");
            return;
        }

        GameObject damageTextObj;

        // Ǯ�� ��� ������ ������Ʈ�� �ִ��� Ȯ��
        if (effectPool.ContainsKey(name) && effectPool[name].Count > 0)
        {
            damageTextObj = effectPool[name].Dequeue(); // Ǯ���� ������

            damageTextObj.transform.position = position;
            damageTextObj.transform.rotation = rotation;
            damageTextObj.SetActive(true); // �ٽ� Ȱ��ȭ

        }
        else
        {
            // Ǯ�� ������ ���� ����
            damageTextObj = Instantiate(effectPrefabDict[name], position, rotation);
        }

        damageTextObj.GetComponent<DamageText>().SetDamageAndPlay((int)damage);
    }

    // ����� ���� ����Ʈ�� Ǯ�� �ݳ�
    public void ReturnEffectToPool(string name, GameObject effectObject)
    {
        if (!effectPool.ContainsKey(name))
        {
            Debug.LogWarning($"EffectManager: '{name}' �̶�� �̸��� Ǯ�� �����ϴ�. ������Ʈ�� �ı��մϴ�.");
            Destroy(effectObject);
            return;
        }

        effectObject.SetActive(false); // ��Ȱ��ȭ
        effectPool[name].Enqueue(effectObject); // Ǯ�� �ٽ� ����
    }
}