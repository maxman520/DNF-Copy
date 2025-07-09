using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

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

    // 활성화된 모든 이펙트를 추적하는 리스트
    private Dictionary<string, List<GameObject>> activeEffects = new Dictionary<string, List<GameObject>>();

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
                activeEffects[effect.name] = new List<GameObject>();
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

        activeEffects[name].Add(effectObject);
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
        activeEffects[name].Add(damageTextObj);
    }

    // 몬스터 HP바 점멸 이펙트 생성 요청
    public GameObject PlayEffect(string name, Transform hpFlashParent)
    {

        GameObject flashEffectObj;

        // 풀에 사용 가능한 오브젝트가 있는지 확인
        if (effectPool.ContainsKey(name) && effectPool[name].Count > 0)
        {
            flashEffectObj = effectPool[name].Dequeue(); // 풀에서 꺼내옴
            flashEffectObj.SetActive(true); // 다시 활성화

        }
        else
        {
            // 풀에 없으면 새로 생성
            flashEffectObj = Instantiate(effectPrefabDict[name], hpFlashParent);
        }


        activeEffects[name].Add(flashEffectObj);


        return flashEffectObj;
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

        // 활성화된 오브젝트 리스트에서 제거
        if (activeEffects.ContainsKey(name))
        {
            activeEffects[name].Remove(effectObject);
        }
    }

    // 특정 이름의 이펙트가 현재 몇 개 활성화되어 있는지 반환하는 함수
    public int GetActiveEffectCount(string name)
    {
        if (activeEffects.ContainsKey(name))
        {
            // 리스트에 있는 요소의 개수가 바로 활성화된 이펙트의 개수
            return activeEffects[name].Count;
        }

        // 해당 이름의 풀이 없으면 0을 반환
        return 0;
    }

    // 특정 이름의 모든 활성 이펙트를 정리하는 함수
    public void ClearEffectsByName(string name)
    {
        if (!activeEffects.ContainsKey(name)) return;

        // 원본 리스트를 순회하며 삭제하면 에러가 나므로, 복사본을 만들어 사용
        List<GameObject> effectsToClear = new List<GameObject>(activeEffects[name]);

        foreach (var effect in effectsToClear)
        {
            // 풀에 반납하거나 그냥 파괴
            ReturnEffectToPool(name, effect);
            // 또는 Destroy(effect);
        }

        activeEffects[name].Clear(); // 리스트를 확실하게 비움
    }

}