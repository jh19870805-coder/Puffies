#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 用途：统一 CanvasScaler 设计分辨率为 GameDefine（2560×1440）；新建 Canvas 时自动套用。返回：按方法说明。
/// </summary>
[InitializeOnLoad]
public static class CanvasDesignResolutionEditor
{
    private const string ScenesRoot = "Assets/Scenes";
    private const string PrefabsRoot = "Assets";

    static CanvasDesignResolutionEditor()
    {
        ObjectFactory.componentWasAdded += OnComponentWasAdded;
    }

    [MenuItem("Puffies/Apply Design Resolution (Current Scene)", false, 40)]
    public static void ApplyToCurrentSceneMenu()
    {
        var changed = ApplyToScene(SceneManager.GetActiveScene());
        Debug.Log($"Canvas design resolution: current scene updated {changed} CanvasScaler(s).");
    }

    [MenuItem("Puffies/Apply Design Resolution (All Scenes & Prefabs)", false, 41)]
    public static void ApplyToAllAssetsMenu()
    {
        if (!EditorUtility.DisplayDialog(
                "Apply Canvas Design Resolution",
                $"将所有场景与预制体中的 CanvasScaler 设为 {GameDefine.DesignWidth}×{GameDefine.DesignHeight}？\n" +
                "（不含 TextMesh Pro 示例资源）",
                "执行",
                "取消"))
        {
            return;
        }

        var totalChanged = 0;
        totalChanged += ApplyToAllScenes();
        totalChanged += ApplyToAllPrefabs();
        AssetDatabase.SaveAssets();
        Debug.Log($"Canvas design resolution: batch complete, updated {totalChanged} CanvasScaler(s).");
    }

    private static void OnComponentWasAdded(Component component)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (component is CanvasScaler scaler)
        {
            ApplyDesignResolution(scaler, recordUndo: true);
        }
    }

    public static bool ApplyDesignResolution(CanvasScaler scaler, bool recordUndo)
    {
        if (scaler == null)
        {
            return false;
        }

        var targetResolution = new Vector2(GameDefine.DesignWidth, GameDefine.DesignHeight);
        var alreadyMatched = scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize
            && scaler.referenceResolution == targetResolution
            && Mathf.Approximately(scaler.referencePixelsPerUnit, GameDefine.PixelsPerUnit)
            && scaler.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight
            && Mathf.Approximately(scaler.matchWidthOrHeight, 0.5f);
        if (alreadyMatched)
        {
            return false;
        }

        if (recordUndo)
        {
            Undo.RecordObject(scaler, "Apply Canvas Design Resolution");
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = targetResolution;
        scaler.referencePixelsPerUnit = GameDefine.PixelsPerUnit;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        EditorUtility.SetDirty(scaler);
        return true;
    }

    private static int ApplyToAllScenes()
    {
        if (!Directory.Exists(ScenesRoot))
        {
            return 0;
        }

        var scenePaths = Directory.GetFiles(ScenesRoot, "*.unity", SearchOption.AllDirectories);
        var activeScenePath = SceneManager.GetActiveScene().path;
        var changed = 0;

        for (var i = 0; i < scenePaths.Length; i++)
        {
            var scenePath = scenePaths[i].Replace('\\', '/');
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            changed += ApplyToScene(scene);
            EditorSceneManager.SaveScene(scene);
        }

        if (!string.IsNullOrEmpty(activeScenePath))
        {
            EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Single);
        }

        return changed;
    }

    private static int ApplyToAllPrefabs()
    {
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabsRoot });
        var changed = 0;

        for (var i = 0; i < prefabGuids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]).Replace('\\', '/');
            if (ShouldSkipAssetPath(path))
            {
                continue;
            }

            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var scalers = root.GetComponentsInChildren<CanvasScaler>(true);
                var prefabChanged = false;
                for (var j = 0; j < scalers.Length; j++)
                {
                    if (ApplyDesignResolution(scalers[j], recordUndo: false))
                    {
                        prefabChanged = true;
                        changed++;
                    }
                }

                if (prefabChanged)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        return changed;
    }

    private static int ApplyToScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return 0;
        }

        var scalers = CollectSceneCanvasScalers(scene);
        var changed = 0;
        for (var i = 0; i < scalers.Count; i++)
        {
            if (ApplyDesignResolution(scalers[i], recordUndo: false))
            {
                changed++;
            }
        }

        if (changed > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }

        return changed;
    }

    private static List<CanvasScaler> CollectSceneCanvasScalers(Scene scene)
    {
        var results = new List<CanvasScaler>();
        var roots = scene.GetRootGameObjects();
        for (var i = 0; i < roots.Length; i++)
        {
            results.AddRange(roots[i].GetComponentsInChildren<CanvasScaler>(true));
        }

        return results;
    }

    private static bool ShouldSkipAssetPath(string assetPath)
    {
        return assetPath.Contains("/TextMesh Pro/")
            || assetPath.Contains("/Plugins/")
            || assetPath.Contains("/Packages/");
    }
}
#endif
