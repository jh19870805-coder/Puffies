using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 构建前与编辑器菜单共用的 StreamingAssets 同步入口。
/// </summary>
public class BuildSync : IPreprocessBuildWithReport
{
    private const string UiSourceRoot = "Assets/UI";
    private const string StreamingRoot = "Assets/StreamingAssets";

    private static readonly string[] UiStreamingFolders =
    {
        "PackImages",
        "BasicUI",
        "MainScene",
        "GameScene",
        "AchieveScene",
        "RankScene"
    };

    public int callbackOrder => 0;

    [InitializeOnLoadMethod]
    private static void SyncOnEditorLoad()
    {
        EditorApplication.delayCall += () => RunAll(false);
    }

    [MenuItem("Puffies/Sync Build Resources", false, 10)]
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
