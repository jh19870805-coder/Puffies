using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 用途：统一管理本地 JSON 文件（单文件多键）的增删改查。返回：按方法说明。
/// </summary>
public static class JsonLocalStore
{
    private const string TempFileSuffix = ".tmp";
    private static readonly object sLock = new object();
    private static bool sIsLoaded;
    private static string sFilePath;
    private static JsonLocalStoreFile sCache;

    /// <summary>
    /// 用途：启动时预加载 JSON 存储。返回：是否初始化成功。
    /// </summary>
    public static bool Initialize()
    {
        lock (sLock)
        {
            try
            {
                EnsureLoaded();
                if (!File.Exists(sFilePath))
                {
                    TryPersist();
                }

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
    /// 用途：新增一条记录；键已存在时返回 false。返回：是否新增成功。
    /// </summary>
    public static bool Create<T>(string key, T value)
    {
        if (!TryValidateKey(key))
        {
            return false;
        }

        lock (sLock)
        {
            EnsureLoaded();
            if (TryFindEntryIndex(key) >= 0)
            {
                Debug.LogWarning($"JsonLocalStore.Create skipped, key already exists: {key}");
                return false;
            }

            sCache.entries.Add(new JsonLocalStoreEntry
            {
                key = key,
                json = JsonUtility.ToJson(value)
            });
            return TryPersist();
        }
    }

    /// <summary>
    /// 用途：按键读取并反序列化为指定类型。返回：是否读取成功。
    /// </summary>
    public static bool TryRead<T>(string key, out T value)
    {
        value = default;
        if (!TryValidateKey(key))
        {
            return false;
        }

        lock (sLock)
        {
            EnsureLoaded();
            var entryIndex = TryFindEntryIndex(key);
            if (entryIndex < 0)
            {
                return false;
            }

            try
            {
                value = JsonUtility.FromJson<T>(sCache.entries[entryIndex].json);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"JsonLocalStore.TryRead failed for key={key}\n{exception}");
                return false;
            }
        }
    }

    /// <summary>
    /// 用途：更新已存在记录；键不存在时返回 false。返回：是否更新成功。
    /// </summary>
    public static bool Update<T>(string key, T value)
    {
        if (!TryValidateKey(key))
        {
            return false;
        }

        lock (sLock)
        {
            EnsureLoaded();
            var entryIndex = TryFindEntryIndex(key);
            if (entryIndex < 0)
            {
                Debug.LogWarning($"JsonLocalStore.Update skipped, key not found: {key}");
                return false;
            }

            sCache.entries[entryIndex].json = JsonUtility.ToJson(value);
            return TryPersist();
        }
    }

    /// <summary>
    /// 用途：写入或覆盖记录（不存在则创建，存在则更新）。返回：是否保存成功。
    /// </summary>
    public static bool Upsert<T>(string key, T value)
    {
        if (!TryValidateKey(key))
        {
            return false;
        }

        lock (sLock)
        {
            EnsureLoaded();
            var entryIndex = TryFindEntryIndex(key);
            if (entryIndex < 0)
            {
                sCache.entries.Add(new JsonLocalStoreEntry
                {
                    key = key,
                    json = JsonUtility.ToJson(value)
                });
            }
            else
            {
                sCache.entries[entryIndex].json = JsonUtility.ToJson(value);
            }

            return TryPersist();
        }
    }

    /// <summary>
    /// 用途：删除指定键。返回：是否删除成功。
    /// </summary>
    public static bool Delete(string key)
    {
        if (!TryValidateKey(key))
        {
            return false;
        }

        lock (sLock)
        {
            EnsureLoaded();
            var entryIndex = TryFindEntryIndex(key);
            if (entryIndex < 0)
            {
                return false;
            }

            sCache.entries.RemoveAt(entryIndex);
            return TryPersist();
        }
    }

    /// <summary>
    /// 用途：判断键是否存在。返回：存在为 true。
    /// </summary>
    public static bool Exists(string key)
    {
        if (!TryValidateKey(key))
        {
            return false;
        }

        lock (sLock)
        {
            EnsureLoaded();
            return TryFindEntryIndex(key) >= 0;
        }
    }

    /// <summary>
    /// 用途：返回当前所有键名快照。返回：键名列表。
    /// </summary>
    public static List<string> ListKeys()
    {
        lock (sLock)
        {
            EnsureLoaded();
            var keys = new List<string>(sCache.entries.Count);
            for (var i = 0; i < sCache.entries.Count; i++)
            {
                var entry = sCache.entries[i];
                if (entry != null && !string.IsNullOrWhiteSpace(entry.key))
                {
                    keys.Add(entry.key);
                }
            }

            return keys;
        }
    }

    /// <summary>
    /// 用途：清空内存缓存并删除 JSON 文件。返回：是否删除成功。
    /// </summary>
    public static bool ClearAll()
    {
        lock (sLock)
        {
            sCache = CreateEmptyFileModel();
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
        if (!File.Exists(sFilePath))
        {
            sCache = CreateEmptyFileModel();
            sIsLoaded = true;
            return;
        }

        try
        {
            var json = File.ReadAllText(sFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                sCache = CreateEmptyFileModel();
            }
            else
            {
                sCache = JsonUtility.FromJson<JsonLocalStoreFile>(json) ?? CreateEmptyFileModel();
                if (sCache.entries == null)
                {
                    sCache.entries = new List<JsonLocalStoreEntry>();
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"JsonLocalStore load failed, fallback to empty store: {sFilePath}\n{exception}");
            sCache = CreateEmptyFileModel();
        }

        sIsLoaded = true;
    }

    private static bool TryPersist()
    {
        try
        {
            var directory = Path.GetDirectoryName(sFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            sCache.version = GameDefine.LocalStoreSchemaVersion;
            var json = JsonUtility.ToJson(sCache, prettyPrint: true);
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

    private static int TryFindEntryIndex(string key)
    {
        for (var i = 0; i < sCache.entries.Count; i++)
        {
            var entry = sCache.entries[i];
            if (entry != null && entry.key == key)
            {
                return i;
            }
        }

        return -1;
    }

    private static bool TryValidateKey(string key)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            return true;
        }

        Debug.LogWarning("JsonLocalStore: key is null or empty.");
        return false;
    }

    private static JsonLocalStoreFile CreateEmptyFileModel()
    {
        return new JsonLocalStoreFile
        {
            version = GameDefine.LocalStoreSchemaVersion,
            entries = new List<JsonLocalStoreEntry>()
        };
    }

    [Serializable]
    private sealed class JsonLocalStoreFile
    {
        public int version;
        public List<JsonLocalStoreEntry> entries;
    }

    [Serializable]
    private sealed class JsonLocalStoreEntry
    {
        public string key;
        public string json;
    }
}
