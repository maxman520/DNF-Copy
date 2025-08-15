using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Audio Library", fileName = "AudioLibrary")]
public class AudioLibrary : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string key;
        public AudioClip clip;
    }

    [System.Serializable]
    public class Category
    {
        public string name;
        public List<Entry> entries = new List<Entry>();
    }

    [SerializeField]
    private List<Category> _categories = new List<Category>();

    private Dictionary<string, AudioClip> _map; // 키 전역 조회용
    private Dictionary<string, Dictionary<string, AudioClip>> _categoryMap; // 카테고리별 조회용

    private void OnEnable()
    {
        BuildMap();
    }

    public void BuildMap()
    {
        _map = new Dictionary<string, AudioClip>();
        _categoryMap = new Dictionary<string, Dictionary<string, AudioClip>>();

        foreach (var cat in _categories)
        {
            if (cat == null || string.IsNullOrWhiteSpace(cat.name))
                continue;

            if (!_categoryMap.ContainsKey(cat.name))
                _categoryMap[cat.name] = new Dictionary<string, AudioClip>();

            foreach (var e in cat.entries)
            {
                if (e == null || string.IsNullOrWhiteSpace(e.key) || e.clip == null)
                    continue;

                _map[e.key] = e.clip; // 전역 키로도 매핑
                _categoryMap[cat.name][e.key] = e.clip;
            }
        }
    }

    public bool TryGet(string key, out AudioClip clip)
    {
        if (_map == null)
            BuildMap();
        return _map.TryGetValue(key, out clip);
    }

    public bool TryGet(string category, string key, out AudioClip clip)
    {
        clip = null;
        if (_categoryMap == null)
            BuildMap();
        if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(key))
            return false;
        return _categoryMap.TryGetValue(category, out var dict) && dict.TryGetValue(key, out clip);
    }

    public AudioClip Get(string key)
    {
        return TryGet(key, out var clip) ? clip : null;
    }

    public IReadOnlyList<Category> Categories => _categories;
}
