#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

/// <summary>
/// 用途：生成 Noto Sans SC 的 TMP 字体资源，并设为项目默认中文字体。返回：按方法说明。
/// </summary>
public static class DefaultChineseFontEditor
{
    private const int SamplingPointSize = 36;
    private const int AtlasPadding = 5;
    private const int AtlasSize = 4096;

    [MenuItem("Puffies/Setup Default Chinese Font", false, 50)]
    public static void SetupDefaultChineseFont()
    {
        var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(GameDefine.DefaultChineseFontEditorPath);
        if (sourceFont == null)
        {
            EditorUtility.DisplayDialog(
                "Default Chinese Font",
                $"未找到字体文件：\n{GameDefine.DefaultChineseFontEditorPath}",
                "确定");
            return;
        }

        var fontAsset = EnsureTmpFontAsset(sourceFont);
        if (fontAsset == null)
        {
            EditorUtility.DisplayDialog("Default Chinese Font", "生成 TMP 字体资源失败。", "确定");
            return;
        }

        UpdateTmpSettings(fontAsset);
        var changedTexts = ApplyToAllScenes(fontAsset, sourceFont);
        changedTexts += ApplyToAllPrefabs(fontAsset, sourceFont);
        AssetDatabase.SaveAssets();

        Debug.Log(
            $"Default Chinese Font ready. TMP={fontAsset.name}, updated {changedTexts} text component(s).");
        EditorUtility.DisplayDialog(
            "Default Chinese Font",
            $"已设置默认中文字体：Noto Sans SC\n已更新 {changedTexts} 个文本组件。",
            "确定");
    }

    private static TMP_FontAsset EnsureTmpFontAsset(Font sourceFont)
    {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(GameDefine.DefaultChineseTmpFontEditorPath);
        if (existing != null)
        {
            return existing;
        }

        var fontAsset = TMP_FontAsset.CreateFontAsset(
            sourceFont,
            SamplingPointSize,
            AtlasPadding,
            GlyphRenderMode.SDFAA,
            AtlasSize,
            AtlasSize,
            AtlasPopulationMode.Dynamic);

        if (fontAsset == null)
        {
            return null;
        }

        fontAsset.name = "NotoSansSC-Regular SDF";
        AssetDatabase.CreateAsset(fontAsset, GameDefine.DefaultChineseTmpFontEditorPath);

        if (fontAsset.material != null)
        {
            fontAsset.material.name = "NotoSansSC-Regular SDF Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        if (fontAsset.atlasTexture != null)
        {
            fontAsset.atlasTexture.name = "NotoSansSC-Regular SDF Atlas";
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(GameDefine.DefaultChineseTmpFontEditorPath);
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(GameDefine.DefaultChineseTmpFontEditorPath);
    }

    private static void UpdateTmpSettings(TMP_FontAsset fontAsset)
    {
        var settingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        var settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(settingsPath);
        if (settings == null)
        {
            Debug.LogWarning($"DefaultChineseFontEditor: TMP Settings not found at {settingsPath}");
            return;
        }

        var serializedSettings = new SerializedObject(settings);
        serializedSettings.Update();
        serializedSettings.FindProperty("m_defaultFontAsset").objectReferenceValue = fontAsset;
        serializedSettings.FindProperty("m_fallbackFontAssets").arraySize = 0;
        serializedSettings.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(settings);
    }

    private static int ApplyToAllScenes(TMP_FontAsset fontAsset, Font uiFont)
    {
        const string scenesRoot = "Assets/Scenes";
        if (!Directory.Exists(scenesRoot))
        {
            return 0;
        }

        var scenePaths = Directory.GetFiles(scenesRoot, "*.unity", SearchOption.AllDirectories);
        var activeScenePath = SceneManager.GetActiveScene().path;
        var changed = 0;

        for (var i = 0; i < scenePaths.Length; i++)
        {
            var scenePath = scenePaths[i].Replace('\\', '/');
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            changed += ApplyToScene(scene, fontAsset, uiFont);
            EditorSceneManager.SaveScene(scene);
        }

        if (!string.IsNullOrEmpty(activeScenePath))
        {
            EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Single);
        }

        return changed;
    }

    private static int ApplyToAllPrefabs(TMP_FontAsset fontAsset, Font uiFont)
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        var changed = 0;

        for (var i = 0; i < prefabGuids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]).Replace('\\', '/');
            if (path.Contains("/TextMesh Pro/"))
            {
                continue;
            }

            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var prefabChanged = ApplyToGameObject(root, fontAsset, uiFont);
                if (prefabChanged > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    changed += prefabChanged;
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        return changed;
    }

    private static int ApplyToScene(Scene scene, TMP_FontAsset fontAsset, Font uiFont)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return 0;
        }

        var changed = 0;
        var roots = scene.GetRootGameObjects();
        for (var i = 0; i < roots.Length; i++)
        {
            changed += ApplyToGameObject(roots[i], fontAsset, uiFont);
        }

        if (changed > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }

        return changed;
    }

    private static int ApplyToGameObject(GameObject root, TMP_FontAsset fontAsset, Font uiFont)
    {
        var changed = 0;
        var tmpTexts = root.GetComponentsInChildren<TMP_Text>(true);
        for (var i = 0; i < tmpTexts.Length; i++)
        {
            if (ApplyTmpFont(tmpTexts[i], fontAsset))
            {
                changed++;
            }
        }

        var uiTexts = root.GetComponentsInChildren<Text>(true);
        for (var i = 0; i < uiTexts.Length; i++)
        {
            if (ApplyUiFont(uiTexts[i], uiFont))
            {
                changed++;
            }
        }

        return changed;
    }

    private static bool ApplyTmpFont(TMP_Text text, TMP_FontAsset fontAsset)
    {
        if (text == null || fontAsset == null || text.font == fontAsset)
        {
            return false;
        }

        Undo.RecordObject(text, "Apply Default Chinese Font");
        text.font = fontAsset;
        EditorUtility.SetDirty(text);
        return true;
    }

    private static bool ApplyUiFont(Text text, Font uiFont)
    {
        if (text == null || uiFont == null || text.font == uiFont)
        {
            return false;
        }

        Undo.RecordObject(text, "Apply Default Chinese Font");
        text.font = uiFont;
        EditorUtility.SetDirty(text);
        return true;
    }
}
#endif
