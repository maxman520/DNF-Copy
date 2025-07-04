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

    [Header("이펙트 프리팹 목록")]
    [SerializeField] private List<Effect> effectPrefabs = new List<Effect>();

    // 오브젝트 풀링을 위한 딕셔너리
    private Dictionary<string, Queue<GameObject>> effectPool = new Dictionary<string, Queue<GameObject>>();

    // 이펙트 이름과 프리팹을 매칭하는 딕셔너리 (빠른 검색용)
    private Dictionary<string, GameObject> effectPrefabDict = new Dictionary<string, GameObject>();

    protected override void Awake()
    {
        // 싱글턴 패턴
        base.Awake();
        InitializePool();
    }

    // 오브젝트 풀과 프리팹 딕셔너리 초기화
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

    // 이펙트 재생 요청
    public void PlayEffect(string name, Vector3 position, Quaternion rotation)
    {
        if (!effectPrefabDict.ContainsKey(name))
        {
            Debug.LogWarning($"EffectManager: '{name}' 이라는 이름의 이펙트를 찾을 수 없습니다.");
            return;
        }

        GameObject effectObject;

        // 풀에 사용 가능한 오브젝트가 있는지 확인
        if (effectPool.ContainsKey(name) && effectPool[name].Count > 0)
        {
            effectObject = effectPool[name].Dequeue(); // 풀에서 꺼내옴

            effectObject.transform.position = position;
            effectObject.transform.rotation = rotation;
            effectObject.SetActive(true); // 다시 활성화

        }
        else
        {
            // 풀에 없으면 새로 생성
            effectObject = Instantiate(effectPrefabDict[name], position, rotation);
        }
    }

    // 데미지 폰트 재생 요청
    public void PlayEffect(string name, Vector3 position, Quaternion rotation, float damage)
    {
        position = new Vector3(position.x, position.y + 0.5f, position.z);
        if (!effectPrefabDict.ContainsKey(name))
        {
            Debug.LogWarning($"EffectManager: '{name}' 이라는 이름의 이펙트를 찾을 수 없습니다.");
            return;
        }

        GameObject damageTextObj;

        // 풀에 사용 가능한 오브젝트가 있는지 확인
        if (effectPool.ContainsKey(name) && effectPool[name].Count > 0)
        {
            damageTextObj = effectPool[name].Dequeue(); // 풀에서 꺼내옴

            damageTextObj.transform.position = position;
            damageTextObj.transform.rotation = rotation;
            damageTextObj.SetActive(true); // 다시 활성화

        }
        else
        {
            // 풀에 없으면 새로 생성
            damageTextObj = Instantiate(effectPrefabDict[name], position, rotation);
        }

        damageTextObj.GetComponent<DamageText>().SetDamageAndPlay((int)damage);
    }

    // 사용이 끝난 이펙트를 풀에 반납
    public void ReturnEffectToPool(string name, GameObject effectObject)
    {
        if (!effectPool.ContainsKey(name))
        {
            Debug.LogWarning($"EffectManager: '{name}' 이라는 이름의 풀이 없습니다. 오브젝트를 파괴합니다.");
            Destroy(effectObject);
            return;
        }

        effectObject.SetActive(false); // 비활성화
        effectPool[name].Enqueue(effectObject); // 풀에 다시 넣음
    }
}