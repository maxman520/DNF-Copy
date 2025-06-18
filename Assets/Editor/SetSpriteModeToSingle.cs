using UnityEditor;
using UnityEngine;

public class SetSpriteModeToSingleAndPPU : EditorWindow
{
    [MenuItem("Tools/Set Sprites to Single and PPU 60")]
    private static void SetToSingleAndPPU()
    {
        var textures = Selection.GetFiltered<Texture2D>(SelectionMode.DeepAssets);

        foreach (var tex in textures)
        {
            string path = AssetDatabase.GetAssetPath(tex);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null && importer.textureType == TextureImporterType.Sprite)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 60f;

                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();

                Debug.Log($"Set to Single + PPU 60: {path}");
            }
        }

        AssetDatabase.Refresh();
    }
}
