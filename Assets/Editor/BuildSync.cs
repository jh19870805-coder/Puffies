using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 用途：构建前与菜单统一同步 StreamingAssets。3D 资源在 Assets/ArtRes/Resources 维护。返回：无。
/// </summary>
public class BuildSync : IPreprocessBuildWithReport
{
    private const string ConfigsSourceFolder = "Assets/Configs";
    private const string ArtResSourceRoot = "Assets/ArtRes";
    private const string StreamingRoot = "Assets/StreamingAssets";
    private const string LegacyTexturesStreamingFolder = "Textures";
    private const string CanonicalRuntimeResourcesRoot = "Assets/ArtRes/Resources";

    private static readonly string[] LegacyAssetFolders =
    {
        "Assets/Resources",
        "Assets/ArtRes/Effect",
        "Assets/ArtRes/PlaneGroup",
        "Assets/ArtRes/Shader"
    };

    private static readonly string[] ArtResStreamingFolders =
    {
        "PackImages",
        "Game001",
        "BasicUI"
    };

    private static readonly string[] ArtResStreamingFiles =
    {
        "MainBg.png"
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
        SyncConfigsToStreaming();
        SyncArtResToStreaming();
        RemoveLegacyTexturesStreaming();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (logOnComplete)
        {
            Debug.Log("BuildSync completed.");
        }
    }

    private static void SyncConfigsToStreaming()
    {
        if (!Directory.Exists(ConfigsSourceFolder))
        {
            Debug.LogWarning($"BuildSync skipped, missing: {ConfigsSourceFolder}");
            return;
        }

        CopyDirectory(ConfigsSourceFolder, Path.Combine(StreamingRoot, Path.GetFileName(ConfigsSourceFolder)));
    }

    private static void SyncArtResToStreaming()
    {
        if (!Directory.Exists(ArtResSourceRoot))
        {
            Debug.LogWarning($"BuildSync skipped, missing: {ArtResSourceRoot}");
            return;
        }

        var targetRoot = Path.Combine(StreamingRoot, Path.GetFileName(ArtResSourceRoot));
        if (Directory.Exists(targetRoot))
        {
            Directory.Delete(targetRoot, true);
        }

        Directory.CreateDirectory(targetRoot);

        for (var i = 0; i < ArtResStreamingFolders.Length; i++)
        {
            var folderName = ArtResStreamingFolders[i];
            var source = Path.Combine(ArtResSourceRoot, folderName);
            if (!Directory.Exists(source))
            {
                Debug.LogWarning($"BuildSync skipped ArtRes folder: {source}");
                continue;
            }

            CopyDirectory(source, Path.Combine(targetRoot, folderName));
        }

        for (var i = 0; i < ArtResStreamingFiles.Length; i++)
        {
            var fileName = ArtResStreamingFiles[i];
            var source = Path.Combine(ArtResSourceRoot, fileName);
            if (File.Exists(source))
            {
                File.Copy(source, Path.Combine(targetRoot, fileName), true);
            }
        }
    }

    private static void CleanupLegacyAssetFolders()
    {
        if (!AssetDatabase.IsValidFolder(CanonicalRuntimeResourcesRoot))
        {
            return;
        }

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

    private static void RemoveLegacyTexturesStreaming()
    {
        var legacyFolder = Path.Combine(StreamingRoot, LegacyTexturesStreamingFolder);
        if (Directory.Exists(legacyFolder))
        {
            Directory.Delete(legacyFolder, true);
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
