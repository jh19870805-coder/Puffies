using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 用途：构建前与菜单统一同步 StreamingAssets 与 Resources。返回：无。
/// </summary>
public class BuildSync : IPreprocessBuildWithReport
{
    private const string ConfigsSourceFolder = "Assets/Configs";
    private const string ArtResSourceRoot = "Assets/ArtRes";
    private const string StreamingRoot = "Assets/StreamingAssets";
    private const string LegacyTexturesStreamingFolder = "Textures";

    private const string PlaneGroupPrefabSource = "Assets/ArtRes/PlaneGroup/Prefab/mesh_PlaneGroup_001.prefab";
    private const string PlaneGroupMaterialSource = "Assets/ArtRes/PlaneGroup/Materials/002.mat";
    private const string PlaneGroupResourcesRoot = "Assets/Resources/PlaneGroup";
    private const string PlaneGroupPrefabsFolder = "Assets/Resources/PlaneGroup/Prefabs";
    private const string PlaneGroupMaterialsFolder = "Assets/Resources/PlaneGroup/Materials";
    private const string PlaneGroupLitMaterialName = "PlaneGroupLit.mat";

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
        SyncConfigsToStreaming();
        SyncArtResToStreaming();
        RemoveLegacyTexturesStreaming();
        SyncPlaneGroupToResources();

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

    private static void RemoveLegacyTexturesStreaming()
    {
        var legacyFolder = Path.Combine(StreamingRoot, LegacyTexturesStreamingFolder);
        if (Directory.Exists(legacyFolder))
        {
            Directory.Delete(legacyFolder, true);
        }
    }

    private static void SyncPlaneGroupToResources()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder(PlaneGroupResourcesRoot);
        EnsureFolder(PlaneGroupPrefabsFolder);
        EnsureFolder(PlaneGroupMaterialsFolder);
        ClearFolderAssets(PlaneGroupPrefabsFolder);

        CopyOrReplaceAsset(PlaneGroupPrefabSource, $"{PlaneGroupPrefabsFolder}/mesh_PlaneGroup_001.prefab");
        CopyOrReplaceAsset(PlaneGroupMaterialSource, $"{PlaneGroupMaterialsFolder}/{PlaneGroupLitMaterialName}");
        RemoveLegacyRootAssets(
            PlaneGroupResourcesRoot,
            new[] { PlaneGroupPrefabsFolder, PlaneGroupMaterialsFolder });
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

    private static void ClearFolderAssets(string assetFolder)
    {
        if (!AssetDatabase.IsValidFolder(assetFolder))
        {
            return;
        }

        var assetPaths = CollectAssetPaths(assetFolder, recursive: false);
        for (var i = 0; i < assetPaths.Count; i++)
        {
            AssetDatabase.DeleteAsset(assetPaths[i]);
        }
    }

    private static void RemoveLegacyRootAssets(string resourceRoot, string[] preservedSubfolders)
    {
        if (!AssetDatabase.IsValidFolder(resourceRoot))
        {
            return;
        }

        var preserved = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < preservedSubfolders.Length; i++)
        {
            preserved.Add(preservedSubfolders[i].Replace("\\", "/"));
        }

        var assetPaths = CollectAssetPaths(resourceRoot, recursive: false);
        for (var i = 0; i < assetPaths.Count; i++)
        {
            var assetPath = assetPaths[i];
            if (preserved.Contains(assetPath))
            {
                continue;
            }

            AssetDatabase.DeleteAsset(assetPath);
        }
    }

    private static List<string> CollectAssetPaths(string assetFolder, bool recursive)
    {
        var results = new List<string>();
        if (!AssetDatabase.IsValidFolder(assetFolder))
        {
            return results;
        }

        var guids = AssetDatabase.FindAssets(string.Empty, new[] { assetFolder });
        for (var i = 0; i < guids.Length; i++)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                continue;
            }

            var parent = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
            if (recursive)
            {
                if (parent == assetFolder || (parent != null && parent.StartsWith(assetFolder + "/", System.StringComparison.Ordinal)))
                {
                    results.Add(assetPath);
                }
            }
            else if (parent == assetFolder)
            {
                results.Add(assetPath);
            }
        }

        return results;
    }

    private static void CopyOrReplaceAsset(string sourcePath, string destinationPath)
    {
        if (!File.Exists(sourcePath))
        {
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<Object>(destinationPath) != null)
        {
            AssetDatabase.DeleteAsset(destinationPath);
        }

        if (!AssetDatabase.CopyAsset(sourcePath, destinationPath))
        {
            Debug.LogWarning($"BuildSync copy failed: {sourcePath} -> {destinationPath}");
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
