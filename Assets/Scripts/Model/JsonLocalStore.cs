using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 用途：读写本地 JSON 文件（根对象与文件一一对应）。返回：按方法说明。
/// </summary>
public static class JsonLocalStore
{
    private const string TempFileSuffix = ".tmp";

    private static readonly object sLock = new object();
    private static bool sIsLoaded;
    private static string sFilePath;

    /// <summary>
    /// 用途：启动时准备 JSON 存储路径。返回：是否初始化成功。
    /// </summary>
    public static bool Initialize()
    {
        lock (sLock)
        {
            try
            {
                EnsureLoaded();
                Debug.Log($"JsonLocalStore initialized: {sFilePath}");
                return sIsLoaded;
            }
            catch (Exception exception)
            {
                Debug.LogError($"JsonLocalStore.Initialize failed.\n{exception}");
                return false;
            }
        }
    }

    /// <summary>
    /// 用途：是否已完成初始化。返回：已初始化为 true。
    /// </summary>
    public static bool IsInitialized => sIsLoaded;

    /// <summary>
    /// 用途：获取当前 JSON 存储文件的完整路径。返回：绝对路径字符串。
    /// </summary>
    public static string GetFilePath()
    {
        EnsureLoaded();
        return sFilePath;
    }

    /// <summary>
    /// 用途：读取 JSON 根对象。返回：是否读取成功。
    /// </summary>
    public static bool TryReadRoot<T>(out T value)
    {
        value = default;
        lock (sLock)
        {
            EnsureLoaded();
            if (!File.Exists(sFilePath))
            {
                return false;
            }

            try
            {
                var json = File.ReadAllText(sFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                value = JsonUtility.FromJson<T>(json);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"JsonLocalStore.TryReadRoot failed: {sFilePath}\n{exception}");
                return false;
            }
        }
    }

    /// <summary>
    /// 用途：保存 JSON 根对象。返回：是否保存成功。
    /// </summary>
    public static bool SaveRoot<T>(T value)
    {
        lock (sLock)
        {
            EnsureLoaded();
            return TryPersist(JsonUtility.ToJson(value, prettyPrint: true));
        }
    }

    /// <summary>
    /// 用途：清空并删除 JSON 文件。返回：是否删除成功。
    /// </summary>
    public static bool ClearAll()
    {
        lock (sLock)
        {
            sIsLoaded = true;
            sFilePath = Path.Combine(Application.persistentDataPath, GameDefine.LocalJsonFileName);

            if (!File.Exists(sFilePath))
            {
                return true;
            }

            try
            {
                File.Delete(sFilePath);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"JsonLocalStore.ClearAll failed: {sFilePath}\n{exception}");
                return false;
            }
        }
    }

    private static void EnsureLoaded()
    {
        if (sIsLoaded)
        {
            return;
        }

        sFilePath = Path.Combine(Application.persistentDataPath, GameDefine.LocalJsonFileName);
        sIsLoaded = true;
    }

    private static bool TryPersist(string json)
    {
        try
        {
            var directory = Path.GetDirectoryName(sFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempPath = sFilePath + TempFileSuffix;
            File.WriteAllText(tempPath, json);
            if (File.Exists(sFilePath))
            {
                File.Delete(sFilePath);
            }

            File.Move(tempPath, sFilePath);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"JsonLocalStore persist failed: {sFilePath}\n{exception}");
            return false;
        }
    }
}
