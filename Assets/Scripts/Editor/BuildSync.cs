using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 用途：构建前与菜单统一同步 StreamingAssets。3D 特效资源统一在 Assets/Resources/Effects。返回：无。
/// </summary>
public class BuildSync : IPreprocessBuildWithReport
{
    private const string UiSourceRoot = "Assets/UI";
    private const string StreamingRoot = "Assets/StreamingAssets";

    private static readonly string[] UiStreamingFolders =
    {
        "PackImages",
        "BasicUI",
        "AchieveScene",
        "RankScene"
    };

    private static readonly string[] LegacyAssetFolders =
    {
        "Assets/ArtRes",
        "Assets/Configs",
        "Assets/Core",
        "Assets/Tools",
        "Assets/Editor",
        "Assets/Models",
        "Assets/Materials",
        "Assets/Effects",
        "Assets/Resources/CardPack",
        "Assets/Resources/PlaneGroup",
        "Assets/Resources/Effect"
    };

    private static readonly string[] LegacyStreamingRoots =
    {
        "ArtRes",
        "Config",
        "Configs",
        "Textures"
    };

    public int callbackOrder => 0;

    [InitializeOnLoadMethod]
    private static void SyncOnEditorLoad()
    {
        EditorApplication.delayCall += () => RunAll(false);
    }

    [MenuItem("Puffies/Sync Build Resources")]
    public static void SyncFromMenu()
    {
        RunAll(true);
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        RunAll(true);
    }

    private static void RunAll(bool logOnComplete)
    {
        EnsureFolder(StreamingRoot);
        CleanupLegacyAssetFolders();
        RemoveLegacyStreamingRoots();
        SyncUiToStreaming();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (logOnComplete)
        {
            Debug.Log("BuildSync completed.");
        }
    }

    private static void SyncUiToStreaming()
    {
        if (!Directory.Exists(UiSourceRoot))
        {
            Debug.LogWarning($"BuildSync skipped, missing: {UiSourceRoot}");
            return;
        }

        var targetRoot = Path.Combine(StreamingRoot, GameDefine.UiRoot);
        if (Directory.Exists(targetRoot))
        {
            Directory.Delete(targetRoot, true);
        }

        Directory.CreateDirectory(targetRoot);

        for (var i = 0; i < UiStreamingFolders.Length; i++)
        {
            var folderName = UiStreamingFolders[i];
            var source = Path.Combine(UiSourceRoot, folderName);
            if (!Directory.Exists(source))
            {
                Debug.LogWarning($"BuildSync skipped UI folder: {source}");
                continue;
            }

            CopyDirectory(source, Path.Combine(targetRoot, folderName));
        }
    }

    private static void RemoveLegacyStreamingRoots()
    {
        for (var i = 0; i < LegacyStreamingRoots.Length; i++)
        {
            var legacyRoot = Path.Combine(StreamingRoot, LegacyStreamingRoots[i]).Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(legacyRoot) && AssetDatabase.DeleteAsset(legacyRoot))
            {
                continue;
            }

            if (Directory.Exists(legacyRoot))
            {
                Directory.Delete(legacyRoot, true);
            }

            var legacyMeta = legacyRoot + ".meta";
            if (File.Exists(legacyMeta))
            {
                File.Delete(legacyMeta);
            }
        }
    }

    private static void CleanupLegacyAssetFolders()
    {
        for (var i = 0; i < LegacyAssetFolders.Length; i++)
        {
            var legacyFolder = LegacyAssetFolders[i];
            if (!AssetDatabase.IsValidFolder(legacyFolder))
            {
                continue;
            }

            if (AssetDatabase.DeleteAsset(legacyFolder))
            {
                Debug.Log($"BuildSync removed legacy folder: {legacyFolder}");
            }
        }
    }

    private static void EnsureFolder(string assetFolder)
    {
        if (AssetDatabase.IsValidFolder(assetFolder))
        {
            return;
        }

        var parent = Path.GetDirectoryName(assetFolder)?.Replace("\\", "/");
        var folderName = Path.GetFileName(assetFolder);
        if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
        {
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        var files = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
        for (var i = 0; i < files.Length; i++)
        {
            var file = files[i];
            if (file.EndsWith(".meta"))
            {
                continue;
            }

            var relativePath = file.Substring(source.Length).TrimStart('\\', '/');
            var destination = Path.Combine(target, relativePath);
            var destinationDirectory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            File.Copy(file, destination, true);
        }
    }
}
