using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DNF.Audio.Editor
{
    public class AudioLibraryBuilderWindow : EditorWindow
    {
        private const string DefaultSourceFolder = "Assets/Sounds";

        [SerializeField]
        private AudioLibrary _targetLibrary;

        [SerializeField]
        private string _sourceFolder = DefaultSourceFolder;

        private KeyMode _keyMode = KeyMode.FileName;
        private KeyCasing _keyCasing = KeyCasing.PascalWithUnderscore;
        private bool _resetCategories = true;
        private bool _overwriteExisting = true;

        public enum KeyMode
        {
            FileName,
            RelativePath
        }

        public enum KeyCasing
        {
            None,
            PascalWithUnderscore
        }

        [MenuItem("Tools/Audio/Audio Library Builder...")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<AudioLibraryBuilderWindow>(true, "Audio Library Builder");
            wnd.minSize = new Vector2(420, 260);
            wnd.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("오디오 라이브러리 자동 채우기", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _targetLibrary = (AudioLibrary)EditorGUILayout.ObjectField("타겟 라이브러리", _targetLibrary, typeof(AudioLibrary), false);
            _sourceFolder = EditorGUILayout.TextField("소스 폴더", _sourceFolder);
            _keyMode = (KeyMode)EditorGUILayout.EnumPopup("키 생성 방식", _keyMode);
            _keyCasing = (KeyCasing)EditorGUILayout.EnumPopup("키 규칙", _keyCasing);
            _resetCategories = EditorGUILayout.ToggleLeft("카테고리 리셋 후 채우기", _resetCategories);
            _overwriteExisting = EditorGUILayout.ToggleLeft("같은 키가 있으면 덮어쓰기", _overwriteExisting);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(_targetLibrary == null || !AssetDatabase.IsValidFolder(_sourceFolder)))
            {
                if (GUILayout.Button("스캔 및 라이브러리 채우기"))
                {
                    TryFill();
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "설정된 폴더 내의 모든 AudioClip을 검색하여 키→클립 매핑을 채웁니다.\n" +
                "키 생성 방식: 파일명(확장자 제외) 또는 폴더 기준 상대경로.",
                MessageType.Info);
        }

        private void TryFill()
        {
            if (_targetLibrary == null)
            {
                Debug.LogWarning("타겟 라이브러리가 지정되지 않았습니다.");
                return;
            }

            if (!AssetDatabase.IsValidFolder(_sourceFolder))
            {
                Debug.LogWarning($"유효하지 않은 폴더 경로입니다: {_sourceFolder}");
                return;
            }

            FillLibrary(_targetLibrary, _sourceFolder, _keyMode, _keyCasing, _resetCategories, _overwriteExisting, out int added, out int updated, out int skipped);
            Debug.Log($"오디오 라이브러리 채우기 완료 — 추가: {added}, 업데이트: {updated}, 스킵: {skipped}");
        }

        public static void FillLibrary(AudioLibrary library, string sourceFolder, KeyMode keyMode, KeyCasing keyCasing, bool resetCategories, bool overwriteExisting,
            out int added, out int updated, out int skipped)
        {
            added = updated = skipped = 0;

            // --- 1단계: 모든 오디오 클립 정보를 수집하여 메모리에 정리 ---
            var knownCategoryMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var sub in AssetDatabase.GetSubFolders(sourceFolder))
            {
                var name = Path.GetFileName(sub).Replace("\\", "/");
                if (!string.IsNullOrWhiteSpace(name) && !knownCategoryMap.ContainsKey(name))
                    knownCategoryMap[name] = name;
            }

            var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { sourceFolder });
            var clipsData = new Dictionary<string, Dictionary<string, AudioClip>>(StringComparer.OrdinalIgnoreCase);

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null) continue;

                var key = GenerateKey(path, sourceFolder, keyMode, keyCasing);
                if (string.IsNullOrWhiteSpace(key)) continue;

                var relPath = ToRelativePath(path, sourceFolder).Replace("\\", "/");
                string category = ResolveCategoryByKnownSegments(relPath, knownCategoryMap);

                if (!clipsData.ContainsKey(category))
                {
                    clipsData[category] = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);
                }

                clipsData[category][key] = clip; // 같은 키는 자동으로 덮어쓰기 (최신 파일 기준)
            }

            // --- 2단계: 수집된 정보를 바탕으로 SerializedObject(AudioLibrary)를 업데이트 ---
            var so = new SerializedObject(library);
            so.Update();

            var categoriesProp = so.FindProperty("_categories");
            if (categoriesProp == null)
            {
                Debug.LogError("라이브러리 내부 구조를 찾지 못했습니다. (_categories)");
                return;
            }

            if (resetCategories)
            {
                categoriesProp.ClearArray();
            }

            var existingCategoryIndices = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < categoriesProp.arraySize; i++)
            {
                var nameProp = categoriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("name");
                if (nameProp != null)
                {
                    existingCategoryIndices[nameProp.stringValue] = i;
                }
            }

            foreach (var categoryData in clipsData)
            {
                var categoryName = categoryData.Key;
                var clipsInBuffer = categoryData.Value;

                SerializedProperty categoryElemProp;
                if (existingCategoryIndices.TryGetValue(categoryName, out int catIdx))
                {
                    // 기존 카테고리 사용
                    categoryElemProp = categoriesProp.GetArrayElementAtIndex(catIdx);
                }
                else
                {
                    // 새 카테고리 추가
                    int newIndex = categoriesProp.arraySize;
                    categoriesProp.InsertArrayElementAtIndex(newIndex);
                    categoryElemProp = categoriesProp.GetArrayElementAtIndex(newIndex);
                    categoryElemProp.FindPropertyRelative("name").stringValue = categoryName;
                }

                var entriesProp = categoryElemProp.FindPropertyRelative("entries");
                
                // 중복 누적을 막기 위한 가장 확실한 방법: 항상 비우고 새로 채운다.
                entriesProp.ClearArray();

                var existingKeys = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                
                // 정렬된 순서로 추가하여 일관성 유지
                foreach (var clipInfo in clipsInBuffer.OrderBy(kvp => kvp.Key))
                {
                    var key = clipInfo.Key;
                    var clip = clipInfo.Value;

                    if (existingKeys.ContainsKey(key))
                    {
                        if (overwriteExisting)
                        {
                            var entryElem = entriesProp.GetArrayElementAtIndex(existingKeys[key]);
                            entryElem.FindPropertyRelative("clip").objectReferenceValue = clip;
                            updated++;
                        }
                        else
                        {
                            skipped++;
                        }
                    }
                    else
                    {
                        int newEntryIndex = entriesProp.arraySize;
                        entriesProp.InsertArrayElementAtIndex(newEntryIndex);
                        var entryElem = entriesProp.GetArrayElementAtIndex(newEntryIndex);
                        entryElem.FindPropertyRelative("key").stringValue = key;
                        entryElem.FindPropertyRelative("clip").objectReferenceValue = clip;
                        existingKeys.Add(key, newEntryIndex);
                        added++;
                    }
                }
            }

            so.ApplyModifiedProperties();
            library.BuildMap();
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        private static string GenerateKey(string assetPath, string sourceFolder, KeyMode keyMode, KeyCasing keyCasing)
        {
            switch (keyMode)
            {
                case KeyMode.FileName:
                {
                    var name = Path.GetFileNameWithoutExtension(assetPath);
                    return ApplyCasing(name, keyCasing);
                }
                case KeyMode.RelativePath:
                {
                    var rel = ToRelativePath(assetPath, sourceFolder);
                    rel = rel.Replace("\\", "/");
                    if (rel.EndsWith(Path.GetExtension(rel), StringComparison.OrdinalIgnoreCase))
                    {
                        rel = rel.Substring(0, rel.Length - Path.GetExtension(rel).Length);
                    }
                    var segments = rel.Split('/');
                    for (int i = 0; i < segments.Length; i++)
                    {
                        segments[i] = ApplyCasing(segments[i], keyCasing);
                    }
                    return string.Join("/", segments);
                }
                default:
                    return ApplyCasing(Path.GetFileNameWithoutExtension(assetPath), keyCasing);
            }
        }

        private static string ApplyCasing(string input, KeyCasing keyCasing)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            switch (keyCasing)
            {
                case KeyCasing.None:
                    return input;
                case KeyCasing.PascalWithUnderscore:
                    return ToPascalWithUnderscore(input);
                default:
                    return input;
            }
        }

        private static string ToPascalWithUnderscore(string input)
        {
            var parts = input.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return string.Empty;

            for (int i = 0; i < parts.Length; i++)
            {
                var p = parts[i];
                if (p.Length == 0) { continue; }
                parts[i] = char.ToUpperInvariant(p[0]) + (p.Length > 1 ? p.Substring(1).ToLowerInvariant() : "");
            }

            return string.Join("_", parts);
        }

        private static string ToRelativePath(string path, string folder)
        {
            if (path.StartsWith(folder, StringComparison.Ordinal))
            {
                return path.Substring(folder.Length).TrimStart('/', '\\');
            }
            return path;
        }

        private static string ResolveCategoryByKnownSegments(string relPath, Dictionary<string, string> knownCategoryMap)
        {
            if (string.IsNullOrEmpty(relPath)) return "Root";
            
            var segments = relPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length <= 1) return "Root"; // 파일만 있거나 경로가 없는 경우

            // 폴더 경로만 순회하며 알려진 카테고리 폴더를 찾음
            for (int i = 0; i < segments.Length - 1; i++)
            {
                if (knownCategoryMap.TryGetValue(segments[i], out var canonicalName))
                {
                    return canonicalName; // 가장 먼저 일치하는 최상위 카테고리 폴더를 반환
                }
            }

            // 알려진 카테고리 폴더에 속하지 않으면 첫 번째 폴더명을 카테고리로 사용
            return segments[0];
        }

        [MenuItem("Assets/Audio/라이브러리 채우기 (Assets/Sounds)", true)]
        private static bool ValidateFillFromContext()
        {
            return Selection.activeObject is AudioLibrary;
        }

        [MenuItem("Assets/Audio/라이브러리 채우기 (Assets/Sounds)")]
        private static void FillFromContext()
        {
            var lib = Selection.activeObject as AudioLibrary;
            if (lib == null) return;

            FillLibrary(lib, DefaultSourceFolder, KeyMode.FileName, KeyCasing.PascalWithUnderscore, true, true, out int added, out int updated, out int skipped);
            Debug.Log($"컨텍스트 실행 완료 — 추가: {added}, 업데이트: {updated}, 스킵: {skipped}");
        }
    }
}