using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 用途：将 PlaneGroup Prefab 与材质同步到 Resources，供 effect 场景与打包后加载。返回：无。
/// </summary>
public class PlaneGroupResourcesSync : IPreprocessBuildWithReport
{
    private const string SourcePrefabPath = "Assets/ArtRes/PlaneGroup/Prefab/mesh_PlaneGroup_001.prefab";
    private const string SourceMaterialPath = "Assets/ArtRes/PlaneGroup/Materials/002.mat";
    private const string ResourcesFolder = "Assets/Resources/PlaneGroup";

    public int callbackOrder => 2;

    [InitializeOnLoadMethod]
    private static void SyncOnEditorLoad()
    {
        EditorApplication.delayCall += () => EnsurePlaneGroupResourcesSynced(false);
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        EnsurePlaneGroupResourcesSynced(true);
    }

    private static void EnsurePlaneGroupResourcesSynced(bool logOnComplete)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (!AssetDatabase.IsValidFolder(ResourcesFolder))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "PlaneGroup");
        }

        CopyOrReplaceAsset(SourcePrefabPath, $"{ResourcesFolder}/mesh_PlaneGroup_001.prefab");
        CopyOrReplaceAsset(SourceMaterialPath, $"{ResourcesFolder}/002.mat");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (logOnComplete)
        {
            Debug.Log("PlaneGroup Resources sync completed.");
        }
    }

    private static void CopyOrReplaceAsset(string sourcePath, string destinationPath)
    {
        if (!File.Exists(sourcePath))
        {
            Debug.LogWarning($"PlaneGroup sync skipped, missing source: {sourcePath}");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<Object>(destinationPath) != null)
        {
            AssetDatabase.DeleteAsset(destinationPath);
        }

        if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
        {
            Debug.LogWarning($"PlaneGroup sync failed: {sourcePath} -> {destinationPath}");
        }
    }
}
