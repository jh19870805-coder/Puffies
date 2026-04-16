using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 用途：构建前自动将运行时直读资源同步到 StreamingAssets，保证打包后可被文件接口读取。返回：无。
/// </summary>
public class StreamingAssetsBuildSync : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    private static readonly string[] SourceFolders =
    {
        "Assets/Configs",
        "Assets/Textures"
    };

    private const string StreamingRoot = "Assets/StreamingAssets";

    /// <summary>
    /// 用途：构建前执行资源同步。返回：无。
    /// </summary>
    public void OnPreprocessBuild(BuildReport report)
    {
        EnsureStreamingAssetsSynced();
    }

    /// <summary>
    /// 用途：把配置和贴图目录拷贝到 StreamingAssets 同名目录。返回：无。
    /// </summary>
    private static void EnsureStreamingAssetsSynced()
    {
        if (!Directory.Exists(StreamingRoot))
        {
            Directory.CreateDirectory(StreamingRoot);
        }

        for (var i = 0; i < SourceFolders.Length; i++)
        {
            var source = SourceFolders[i];
            if (!Directory.Exists(source))
            {
                Debug.LogWarning($"StreamingAssets sync skipped, source folder missing: {source}");
                continue;
            }

            var folderName = Path.GetFileName(source);
            var target = Path.Combine(StreamingRoot, folderName);
            CopyDirectory(source, target);
        }

        AssetDatabase.Refresh();
        Debug.Log("StreamingAssets sync completed.");
    }

    /// <summary>
    /// 用途：递归复制目录内容（忽略 .meta）。返回：无。
    /// </summary>
    private static void CopyDirectory(string source, string target)
    {
        if (Directory.Exists(target))
        {
            Directory.Delete(target, true);
        }

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
