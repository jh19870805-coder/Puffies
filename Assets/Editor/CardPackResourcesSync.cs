using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 用途：将卡包 Prefab 与材质同步到 Resources，供打包后运行时加载。返回：无。
/// </summary>
public class CardPackResourcesSync : IPreprocessBuildWithReport
{
    private const string SourcePrefabFolder = "Assets/ArtRes/Effect/Prefab/CardPack";
    private const string SourceMaterialPath = "Assets/ArtRes/Effect/Texture/Materials/001.mat";
    private const string ResourcesFolder = "Assets/Resources/CardPack";

    public int callbackOrder => 1;

    [InitializeOnLoadMethod]
    private static void SyncOnEditorLoad()
    {
        EditorApplication.delayCall += () => EnsureCardPackResourcesSynced(false);
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        EnsureCardPackResourcesSynced(true);
    }

    /// <summary>
    /// 用途：把卡包 Prefab 与 URP 材质复制到 Resources/CardPack。返回：无。
    /// </summary>
    private static void EnsureCardPackResourcesSynced(bool logOnComplete)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (!AssetDatabase.IsValidFolder(ResourcesFolder))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "CardPack");
        }

        SyncPrefabs();
        SyncMaterial();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (logOnComplete)
        {
            Debug.Log("CardPack Resources sync completed.");
        }
    }

    private static void SyncPrefabs()
    {
        if (!Directory.Exists(SourcePrefabFolder))
        {
            Debug.LogWarning($"CardPack sync skipped, source folder missing: {SourcePrefabFolder}");
            return;
        }

        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { SourcePrefabFolder });
        for (var i = 0; i < guids.Length; i++)
        {
            var sourcePath = AssetDatabase.GUIDToAssetPath(guids[i]);
            var fileName = Path.GetFileName(sourcePath);
            var destinationPath = $"{ResourcesFolder}/{fileName}";
            CopyOrReplaceAsset(sourcePath, destinationPath);
        }
    }

    private static void SyncMaterial()
    {
        if (!File.Exists(SourceMaterialPath))
        {
            return;
        }

        CopyOrReplaceAsset(SourceMaterialPath, $"{ResourcesFolder}/001.mat");
    }

    private static void CopyOrReplaceAsset(string sourcePath, string destinationPath)
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(destinationPath) != null)
        {
            AssetDatabase.DeleteAsset(destinationPath);
        }

        if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
        {
            Debug.LogWarning($"CardPack sync failed: {sourcePath} -> {destinationPath}");
        }
    }
}
