using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public static class GameManager
{
    private static int sBagId = GameDefine.DefaultBagId;
    private static bool sIsInitialized;

    /// <summary>
    /// 用途：在首个场景加载前初始化运行时状态。返回：无。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        Initialize();
    }

    /// <summary>
    /// 用途：初始化运行时默认状态，只在首次调用时生效。返回：无。
    /// </summary>
    public static void Initialize()
    {
        if (sIsInitialized)
        {
            return;
        }

        sBagId = GameDefine.DefaultBagId;
        sIsInitialized = true;
        Debug.Log("GameManager initialized.");
    }

    /// <summary>
    /// 用途：获取当前生效的包编号。返回：包编号整数值。
    /// </summary>
    /// <returns>返回：当前包编号。</returns>
    public static int GetBagId()
    {
        return sBagId;
    }

    /// <summary>
    /// 用途：设置当前使用的包编号。返回：无。
    /// </summary>
    /// <param name="bagId">参数：目标包编号。</param>
    public static void SetBagId(int bagId)
    {
        sBagId = bagId;
    }

    /// <summary>
    /// 用途：设置目标卡包编号并切换到游戏场景。返回：无。
    /// </summary>
    /// <param name="bagId">参数：进入游戏场景时要使用的卡包编号。</param>
    public static void EnterGameScene(int bagId)
    {
        SetBagId(bagId);
        SceneManager.LoadScene(GameDefine.SceneGame);
    }

    /// <summary>
    /// 用途：切换到排行榜场景。返回：无。
    /// </summary>
    public static void EnterRankScene()
    {
        SceneManager.LoadScene(GameDefine.SceneRank);
    }

    /// <summary>
    /// 用途：切换到成就场景。返回：无。
    /// </summary>
    public static void EnterAchieveScene()
    {
        SceneManager.LoadScene(GameDefine.SceneAchieve);
    }

    /// <summary>
    /// 用途：返回主场景。返回：无。
    /// </summary>
    public static void EnterMainScene()
    {
        SceneManager.LoadScene(GameDefine.SceneMain);
    }

    /// <summary>
    /// 用途：获取当前包对应的资源文件夹相对路径。返回：包资源目录路径字符串。
    /// </summary>
    /// <returns>返回：形如 UI/Game001 的相对路径。</returns>
    public static string GetBagFolderPath()
    {
        return $"{GameDefine.UiRoot}/{GetBagFolderName()}";
    }

    /// <summary>
    /// 用途：获取当前包封面图片相对路径。返回：封面图片路径字符串。
    /// </summary>
    /// <returns>返回：形如 UI/PackImages/Package001.png 的相对路径。</returns>
    public static string GetBagPackagePath()
    {
        return $"{GameDefine.UiRoot}/{GameDefine.PackImagesFolder}/{GameDefine.PackageFilePrefix}{GetBagIdText()}{GameDefine.ImageExtPng}";
    }

    /// <summary>
    /// 用途：获取当前包配置 Json 的相对路径。返回：配置文件路径字符串。
    /// </summary>
    /// <returns>返回：形如 Config/Package001.json 的相对路径。</returns>
    public static string GetBagConfigPath()
    {
        return $"{GameDefine.ConfigRoot}/{GameDefine.PackageFilePrefix}{GetBagIdText()}{GameDefine.ConfigExtJson}";
    }

    /// <summary>
    /// 用途：读取并解析包配置 Json 文件为结构体数据。返回：是否解析成功。
    /// </summary>
    /// <param name="configPath">参数：配置文件路径，支持绝对路径或相对 Assets 路径。</param>
    /// <param name="packageConfig">参数：输出的包配置结构体，失败时为默认值。</param>
    /// <returns>返回：true 表示解析成功，false 表示失败。</returns>
    public static bool TryLoadPackageConfig(string configPath, out PackageConfigData packageConfig)
    {
        packageConfig = default;
        if (string.IsNullOrWhiteSpace(configPath))
        {
            Debug.LogWarning("Config path is empty.");
            return false;
        }

        var configOnDisk = GameCommonUtility.ToDiskPath(configPath);
        if (!File.Exists(configOnDisk))
        {
            Debug.LogWarning($"Config file does not exist: {configOnDisk}");
            return false;
        }

        var json = File.ReadAllText(configOnDisk);
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning($"Config file is empty: {configOnDisk}");
            return false;
        }

        var normalizedJson = NormalizePieceGroupsJson(json);
        try
        {
            packageConfig = JsonUtility.FromJson<PackageConfigData>(normalizedJson);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to parse package config: {configOnDisk}\n{exception}");
            return false;
        }

        return !string.IsNullOrWhiteSpace(packageConfig.PackageId);
    }

    /// <summary>
    /// 用途：获取当前包编号的三位文本表示。返回：三位编号字符串。
    /// </summary>
    /// <returns>返回：形如 001 的编号文本。</returns>
    private static string GetBagIdText()
    {
        return sBagId.ToString("D3");
    }

    /// <summary>
    /// 用途：获取当前包资源目录名。返回：目录名字符串。
    /// </summary>
    /// <returns>返回：形如 Game001 的目录名。</returns>
    private static string GetBagFolderName()
    {
        return $"{GameDefine.GameFolderPrefix}{GetBagIdText()}";
    }

    /// <summary>
    /// 用途：将 Pieces 的二维数组 Json 结构包装为可被 JsonUtility 映射的对象数组结构。返回：标准化后的 Json 文本。
    /// </summary>
    /// <param name="json">参数：原始包配置 Json 字符串。</param>
    /// <returns>返回：处理后的 Json 字符串。</returns>
    private static string NormalizePieceGroupsJson(string json)
    {
        const string piecesKey = "\"Pieces\"";
        var piecesKeyIndex = json.IndexOf(piecesKey, StringComparison.Ordinal);
        if (piecesKeyIndex < 0)
        {
            return json;
        }

        var colonIndex = json.IndexOf(':', piecesKeyIndex);
        if (colonIndex < 0)
        {
            return json;
        }

        var piecesArrayStart = json.IndexOf('[', colonIndex);
        if (piecesArrayStart < 0)
        {
            return json;
        }

        var piecesArrayEnd = FindMatchingBracket(json, piecesArrayStart);
        if (piecesArrayEnd < 0)
        {
            return json;
        }

        var piecesInnerContent = json.Substring(piecesArrayStart + 1, piecesArrayEnd - piecesArrayStart - 1);
        var wrappedGroups = WrapTopLevelPieceGroups(piecesInnerContent);

        return string.Concat(
            json.Substring(0, piecesArrayStart + 1),
            wrappedGroups,
            json.Substring(piecesArrayEnd));
    }

    /// <summary>
    /// 用途：从指定左中括号位置开始查找与之匹配的右中括号索引。返回：匹配位置索引。
    /// </summary>
    /// <param name="text">参数：待扫描的文本内容。</param>
    /// <param name="openBracketIndex">参数：左中括号的起始索引。</param>
    /// <returns>返回：匹配的右中括号索引，未找到时返回 -1。</returns>
    private static int FindMatchingBracket(string text, int openBracketIndex)
    {
        var depth = 0;
        for (var i = openBracketIndex; i < text.Length; i++)
        {
            if (text[i] == '[')
            {
                depth++;
                continue;
            }

            if (text[i] != ']')
            {
                continue;
            }

            depth--;
            if (depth == 0)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 用途：将顶层碎片组数组片段包装成带 Items 字段的对象集合字符串。返回：包装后的内容。
    /// </summary>
    /// <param name="piecesContent">参数：Pieces 字段内部的原始数组内容。</param>
    /// <returns>返回：可映射到 PackagePieceGroupData[] 的字符串内容。</returns>
    private static string WrapTopLevelPieceGroups(string piecesContent)
    {
        var output = new System.Text.StringBuilder(piecesContent.Length + 32);
        var index = 0;

        while (index < piecesContent.Length)
        {
            var current = piecesContent[index];
            if (current == '[')
            {
                var groupEnd = FindMatchingBracket(piecesContent, index);
                if (groupEnd < 0)
                {
                    output.Append(piecesContent.Substring(index));
                    break;
                }

                output.Append("{\"Items\":");
                output.Append(piecesContent.Substring(index, groupEnd - index + 1));
                output.Append('}');
                index = groupEnd + 1;
                continue;
            }

            output.Append(current);
            index++;
        }

        return output.ToString();
    }
}
