using System;
using System.Collections.Generic;
using System.IO;
using SQLite;
using UnityEngine;

// JSON, SQLite, and persisted game settings share this local persistence layer.

public readonly struct LocalSaveSlotSummary
{
    public LocalSaveSlotSummary(int slotId, bool hasData, int unlockedPackCount, DateTime lastUpdatedTime)
    {
        SlotId = slotId;
        HasData = hasData;
        UnlockedPackCount = unlockedPackCount;
        LastUpdatedTime = lastUpdatedTime;
    }

    public int SlotId { get; }
    public bool HasData { get; }
    public int UnlockedPackCount { get; }
    public DateTime LastUpdatedTime { get; }
}

/// <summary>
/// 用途：管理三份互相隔离的本地存档及当前活动档位。
/// </summary>
public static class LocalSaveSlotUtility
{
    [Serializable]
    private sealed class SaveSlotSelectionData
    {
        public int ActiveSlotId = 1;
    }

    public const int SlotCount = 3;

    private const int DefaultSlotId = 1;
    private const string SaveSlotDirectoryPrefix = "SaveSlot";
    private const string SelectionFileName = "SaveSlots.json";
    private const string TempFileSuffix = ".tmp";

    private static readonly object sLock = new object();
    private static bool sHasLoadedSelection;
    private static int sActiveSlotId = DefaultSlotId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSessionState()
    {
        sHasLoadedSelection = false;
        sActiveSlotId = DefaultSlotId;
    }

    public static int ActiveSlotId
    {
        get
        {
            lock (sLock)
            {
                EnsureSelectionLoaded();
                return sActiveSlotId;
            }
        }
    }

    public static string ActiveSaveDirectoryPath => GetSaveDirectoryPath(ActiveSlotId);

    public static bool TryActivateSlot(int slotId)
    {
        if (!IsValidSlotId(slotId))
        {
            Debug.LogWarning($"LocalSaveSlotUtility: invalid slot id {slotId}.");
            return false;
        }

        lock (sLock)
        {
            EnsureSelectionLoaded();
            if (!TrySaveSelection(slotId))
            {
                return false;
            }

            sActiveSlotId = slotId;
        }

        ResetRuntimeState();
        return true;
    }

    public static LocalSaveSlotSummary GetSlotSummary(int slotId)
    {
        if (!IsValidSlotId(slotId))
        {
            return new LocalSaveSlotSummary(slotId, false, 0, default);
        }

        var directoryPath = GetSaveDirectoryPath(slotId);
        var databasePath = Path.Combine(directoryPath, GameDefine.LocalSqliteFileName);
        var jsonPath = Path.Combine(directoryPath, GameDefine.LocalJsonFileName);
        var hasData = File.Exists(databasePath) || File.Exists(jsonPath);
        if (!hasData)
        {
            return new LocalSaveSlotSummary(slotId, false, 0, default);
        }

        return new LocalSaveSlotSummary(
            slotId,
            true,
            ReadUnlockedPackCount(databasePath),
            ReadLastUpdatedTime(directoryPath));
    }

    public static bool DeleteSlot(int slotId)
    {
        if (!IsValidSlotId(slotId))
        {
            Debug.LogWarning($"LocalSaveSlotUtility: invalid slot id {slotId}.");
            return false;
        }

        try
        {
            if (slotId == ActiveSlotId)
            {
                ResetRuntimeState();
            }

            var directoryPath = GetSaveDirectoryPath(slotId);
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"LocalSaveSlotUtility.DeleteSlot failed for slot {slotId}.\n{exception}");
            return false;
        }
    }

    internal static string GetSaveDirectoryPath(int slotId)
    {
        return Path.Combine(
            Application.persistentDataPath,
            SaveSlotDirectoryPrefix + Mathf.Clamp(slotId, 1, SlotCount));
    }

    private static bool IsValidSlotId(int slotId)
    {
        return slotId >= 1 && slotId <= SlotCount;
    }

    private static void EnsureSelectionLoaded()
    {
        if (sHasLoadedSelection)
        {
            return;
        }

        sActiveSlotId = DefaultSlotId;
        var path = Path.Combine(Application.persistentDataPath, SelectionFileName);
        if (File.Exists(path))
        {
            try
            {
                var json = File.ReadAllText(path);
                var selection = JsonUtility.FromJson<SaveSlotSelectionData>(json);
                if (selection != null && IsValidSlotId(selection.ActiveSlotId))
                {
                    sActiveSlotId = selection.ActiveSlotId;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"LocalSaveSlotUtility: could not read {path}; using slot 1.\n{exception}");
            }
        }

        sHasLoadedSelection = true;
    }

    private static bool TrySaveSelection(int slotId)
    {
        var path = Path.Combine(Application.persistentDataPath, SelectionFileName);
        var tempPath = path + TempFileSuffix;
        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(
                tempPath,
                JsonUtility.ToJson(new SaveSlotSelectionData { ActiveSlotId = slotId }, true));
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.Move(tempPath, path);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"LocalSaveSlotUtility: could not save active slot {slotId}.\n{exception}");
            return false;
        }
    }

    private static int ReadUnlockedPackCount(string databasePath)
    {
        if (!File.Exists(databasePath))
        {
            return 0;
        }

        try
        {
            using (var connection = new SQLiteConnection(databasePath, SQLiteOpenFlags.ReadOnly))
            {
                var tableExists = connection.ExecuteScalar<int>(
                    "SELECT COUNT(1) FROM sqlite_master WHERE type = 'table' AND name = ?",
                    GameDefine.LocalSqliteCardPackTable);
                if (tableExists <= 0)
                {
                    return 0;
                }

                return connection.ExecuteScalar<int>(
                    $"SELECT COUNT(1) FROM {GameDefine.LocalSqliteCardPackTable} WHERE LifecycleState <> ?",
                    (int)CardPackLifecycleState.Locked);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"LocalSaveSlotUtility: could not read unlocked pack count from {databasePath}.\n{exception}");
            return 0;
        }
    }

    private static DateTime ReadLastUpdatedTime(string directoryPath)
    {
        var latest = DateTime.MinValue;
        if (!Directory.Exists(directoryPath))
        {
            return latest;
        }

        try
        {
            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; i++)
            {
                var updatedTime = File.GetLastWriteTime(files[i]);
                if (updatedTime > latest)
                {
                    latest = updatedTime;
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"LocalSaveSlotUtility: could not read last update time from {directoryPath}.\n{exception}");
        }

        return latest;
    }

    private static void ResetRuntimeState()
    {
        GameSettingsUtility.ResetForSaveSlotChange();
        GameTaskUtility.ResetForSaveSlotChange();
        CardPackDataUtility.ResetForSaveSlotChange();
        JsonLocalStore.ResetForSaveSlotChange();
        SqliteLocalStore.ResetForSaveSlotChange();
    }
}

/// <summary>
/// 用途：统一管理本地 SQLite 数据库的增删改查。返回：按方法说明。
/// </summary>
public static class SqliteLocalStore
{
    private static readonly object sLock = new object();
    private static bool sIsInitialized;
    private static string sDatabasePath;
    private static SQLiteConnection sConnection;

    /// <summary>
    /// 用途：启动时打开数据库并建表。返回：是否初始化成功。
    /// </summary>
    public static bool Initialize()
    {
        lock (sLock)
        {
            if (sIsInitialized)
            {
                return true;
            }

            try
            {
                EnsureInitialized();
                return sIsInitialized;
            }
            catch (Exception exception)
            {
                Debug.LogError($"SqliteLocalStore.Initialize failed.\n{exception}");
                sIsInitialized = false;
                if (sConnection != null)
                {
                    sConnection.Dispose();
                    sConnection = null;
                }

                return false;
            }
        }
    }

    /// <summary>
    /// 用途：是否已完成初始化。返回：已初始化为 true。
    /// </summary>
    public static bool IsInitialized => sIsInitialized;

    internal static void ResetForSaveSlotChange()
    {
        lock (sLock)
        {
            sConnection?.Dispose();
            sConnection = null;
            sDatabasePath = null;
            sIsInitialized = false;
        }
    }

    /// <summary>
    /// 用途：在指定集合中新增记录；同键已存在时返回 false。返回：是否新增成功。
    /// </summary>
    public static bool Create(string collection, string key, string value)
    {
        if (!TryValidateCollectionAndKey(collection, key))
        {
            return false;
        }

        lock (sLock)
        {
            EnsureInitialized();
            if (Exists(collection, key))
            {
                Debug.LogWarning($"SqliteLocalStore.Create skipped, key already exists: {collection}/{key}");
                return false;
            }

            var utcNow = DateTime.UtcNow.ToString("o");
            var affected = sConnection.Execute(
                $"INSERT INTO {GameDefine.LocalSqliteCollectionTable} (Collection, RecordKey, JsonValue, CreatedUtc, UpdatedUtc) VALUES (?, ?, ?, ?, ?)",
                collection,
                key,
                value ?? string.Empty,
                utcNow,
                utcNow);
            return affected > 0;
        }
    }

    /// <summary>
    /// 用途：在指定集合中新增对象记录（序列化为 JSON）。返回：是否新增成功。
    /// </summary>
    public static bool Create<T>(string collection, string key, T value)
    {
        return Create(collection, key, JsonUtility.ToJson(value));
    }

    /// <summary>
    /// 用途：读取指定集合中的字符串值。返回：值；不存在时返回 null。
    /// </summary>
    public static string Read(string collection, string key)
    {
        if (!TryValidateCollectionAndKey(collection, key))
        {
            return null;
        }

        lock (sLock)
        {
            EnsureInitialized();
            return sConnection.ExecuteScalar<string>(
                $"SELECT JsonValue FROM {GameDefine.LocalSqliteCollectionTable} WHERE Collection = ? AND RecordKey = ? LIMIT 1",
                collection,
                key);
        }
    }

    /// <summary>
    /// 用途：读取并反序列化指定集合中的对象。返回：是否读取成功。
    /// </summary>
    public static bool TryRead<T>(string collection, string key, out T value)
    {
        value = default;
        var json = Read(collection, key);
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonUtility.FromJson<T>(json);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"SqliteLocalStore.TryRead failed for {collection}/{key}\n{exception}");
            return false;
        }
    }

    /// <summary>
    /// 用途：更新指定集合中的记录；键不存在时返回 false。返回：是否更新成功。
    /// </summary>
    public static bool Update(string collection, string key, string value)
    {
        if (!TryValidateCollectionAndKey(collection, key))
        {
            return false;
        }

        lock (sLock)
        {
            EnsureInitialized();
            if (!Exists(collection, key))
            {
                Debug.LogWarning($"SqliteLocalStore.Update skipped, key not found: {collection}/{key}");
                return false;
            }

            var affected = sConnection.Execute(
                $"UPDATE {GameDefine.LocalSqliteCollectionTable} SET JsonValue = ?, UpdatedUtc = ? WHERE Collection = ? AND RecordKey = ?",
                value ?? string.Empty,
                DateTime.UtcNow.ToString("o"),
                collection,
                key);
            return affected > 0;
        }
    }

    /// <summary>
    /// 用途：更新指定集合中的对象记录（序列化为 JSON）。返回：是否更新成功。
    /// </summary>
    public static bool Update<T>(string collection, string key, T value)
    {
        return Update(collection, key, JsonUtility.ToJson(value));
    }

    /// <summary>
    /// 用途：写入或覆盖记录（不存在则创建，存在则更新）。返回：是否保存成功。
    /// </summary>
    public static bool Upsert(string collection, string key, string value)
    {
        if (!TryValidateCollectionAndKey(collection, key))
        {
            return false;
        }

        return Exists(collection, key)
            ? Update(collection, key, value)
            : Create(collection, key, value);
    }

    /// <summary>
    /// 用途：写入或覆盖对象记录（序列化为 JSON）。返回：是否保存成功。
    /// </summary>
    public static bool Upsert<T>(string collection, string key, T value)
    {
        return Upsert(collection, key, JsonUtility.ToJson(value));
    }

    /// <summary>
    /// 用途：判断指定集合中是否存在某键。返回：存在为 true。
    /// </summary>
    public static bool Exists(string collection, string key)
    {
        if (!TryValidateCollectionAndKey(collection, key))
        {
            return false;
        }

        lock (sLock)
        {
            EnsureInitialized();
            var count = sConnection.ExecuteScalar<int>(
                $"SELECT COUNT(1) FROM {GameDefine.LocalSqliteCollectionTable} WHERE Collection = ? AND RecordKey = ?",
                collection,
                key);
            return count > 0;
        }
    }

    /// <summary>
    /// 用途：执行自定义查询并返回映射结果列表。返回：结果列表。
    /// </summary>
    public static List<T> Query<T>(string sql, params object[] args) where T : new()
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            Debug.LogWarning("SqliteLocalStore.Query: sql is null or empty.");
            return new List<T>();
        }

        lock (sLock)
        {
            EnsureInitialized();
            return sConnection.Query<T>(sql, args);
        }
    }

    /// <summary>
    /// 用途：执行自定义非查询 SQL（INSERT/UPDATE/DELETE）。返回：受影响行数。
    /// </summary>
    public static int ExecuteNonQuery(string sql, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            Debug.LogWarning("SqliteLocalStore.ExecuteNonQuery: sql is null or empty.");
            return 0;
        }

        lock (sLock)
        {
            EnsureInitialized();
            return sConnection.Execute(sql, args);
        }
    }

    /// <summary>
    /// 用途：执行自定义查询并返回标量结果。返回：标量值。
    /// </summary>
    public static T ExecuteScalar<T>(string sql, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            Debug.LogWarning("SqliteLocalStore.ExecuteScalar: sql is null or empty.");
            return default;
        }

        lock (sLock)
        {
            EnsureInitialized();
            return sConnection.ExecuteScalar<T>(sql, args);
        }
    }

    private static void EnsureInitialized()
    {
        if (sIsInitialized)
        {
            return;
        }

        sDatabasePath = Path.Combine(
            LocalSaveSlotUtility.ActiveSaveDirectoryPath,
            GameDefine.LocalSqliteFileName);
        var directory = Path.GetDirectoryName(sDatabasePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        sConnection = new SQLiteConnection(sDatabasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        sConnection.Execute("PRAGMA foreign_keys = ON;");
        sConnection.Execute(
            $@"CREATE TABLE IF NOT EXISTS {GameDefine.LocalSqliteCollectionTable} (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Collection TEXT NOT NULL,
                RecordKey TEXT NOT NULL,
                JsonValue TEXT NOT NULL DEFAULT '',
                CreatedUtc TEXT NOT NULL,
                UpdatedUtc TEXT NOT NULL,
                UNIQUE(Collection, RecordKey)
            );");
        sConnection.Execute(
            $@"CREATE INDEX IF NOT EXISTS {GameDefine.LocalSqliteAppRecordsCollectionIndex}
               ON {GameDefine.LocalSqliteCollectionTable}(Collection);");
        sConnection.Execute(
            $@"CREATE TABLE IF NOT EXISTS {GameDefine.LocalSqliteCardPackTable} (
                PackId INTEGER PRIMARY KEY,
                PackSize INTEGER NOT NULL DEFAULT 0,
                LifecycleState INTEGER NOT NULL DEFAULT 0,
                UnlockTime TEXT NOT NULL DEFAULT '',
                CompletionTime TEXT NOT NULL DEFAULT ''
            );");
        sConnection.Execute(
            $@"CREATE TABLE IF NOT EXISTS {GameDefine.LocalSqliteCardPackPuzzleProgressTable} (
                PackId INTEGER PRIMARY KEY,
                PlacedPieceNumbersJson TEXT NOT NULL DEFAULT '',
                UpdatedTime TEXT NOT NULL DEFAULT ''
            );");

        sIsInitialized = true;
        Debug.Log($"SqliteLocalStore initialized: {sDatabasePath}");
    }

    private static bool TryValidateCollectionAndKey(string collection, string key)
    {
        if (string.IsNullOrWhiteSpace(collection))
        {
            Debug.LogWarning("SqliteLocalStore: collection is null or empty.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(key))
        {
            return true;
        }

        Debug.LogWarning("SqliteLocalStore: key is null or empty.");
        return false;
    }

}

public static class JsonLocalStore
{
    private const string TempFileSuffix = ".tmp";

    private static readonly object sLock = new object();
    private static bool sIsLoaded;
    private static string sFilePath;

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

    public static bool IsInitialized => sIsLoaded;

    internal static void ResetForSaveSlotChange()
    {
        lock (sLock)
        {
            sFilePath = null;
            sIsLoaded = false;
        }
    }

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

    public static bool SaveRoot<T>(T value)
    {
        lock (sLock)
        {
            EnsureLoaded();
            return TryPersist(JsonUtility.ToJson(value, prettyPrint: true));
        }
    }

    private static void EnsureLoaded()
    {
        if (sIsLoaded)
        {
            return;
        }

        sFilePath = Path.Combine(
            LocalSaveSlotUtility.ActiveSaveDirectoryPath,
            GameDefine.LocalJsonFileName);
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

[Serializable]
public sealed class GameSettingsData
{
    public float MusicVolume = 1f;
    public float EffectVolume = 1f;
    public bool IsWindowed;
    public bool UsableOption1;
    public bool UsableOption2;
    public bool UsableOption3;

    public bool IsLevelOutlineEnabled => UsableOption1;
    public bool IsStickerOutlineEnabled => UsableOption2;
    public bool IsHighContrastEnabled => UsableOption3;
}

public static class GameSettingsUtility
{
    private enum UsableOption
    {
        LevelOutline,
        StickerOutline,
        HighContrast
    }

    private const string SettingsCollection = "GameSettings";
    private const string SettingsKey = "Runtime";

    private static GameSettingsData sSettings = CreateDefaultSettings();
    private static bool sHasLoaded;
    private static bool sHasAppliedDisplayMode;
    private static bool sAppliedWindowedState;

    public static bool Initialize()
    {
        if (sHasLoaded)
        {
            ApplyRuntimeSettings();
            return true;
        }

        sSettings = CreateDefaultSettings();
        if (!SqliteLocalStore.Initialize())
        {
            ApplyRuntimeSettings();
            return false;
        }

        if (SqliteLocalStore.TryRead(SettingsCollection, SettingsKey, out GameSettingsData loadedSettings)
            && loadedSettings != null)
        {
            sSettings = loadedSettings;
        }
        else
        {
            Save();
        }

        Sanitize(sSettings);
        sHasLoaded = true;
        ApplyRuntimeSettings();
        return true;
    }

    internal static void ResetForSaveSlotChange()
    {
        sSettings = CreateDefaultSettings();
        sHasLoaded = false;
        sHasAppliedDisplayMode = false;
    }

    public static GameSettingsData GetSettings()
    {
        if (!sHasLoaded)
        {
            Initialize();
        }

        return new GameSettingsData
        {
            MusicVolume = sSettings.MusicVolume,
            EffectVolume = sSettings.EffectVolume,
            IsWindowed = sSettings.IsWindowed,
            UsableOption1 = sSettings.UsableOption1,
            UsableOption2 = sSettings.UsableOption2,
            UsableOption3 = sSettings.UsableOption3
        };
    }

    public static void SetMusicVolume(float value)
    {
        EnsureSettingsLoaded();
        sSettings.MusicVolume = Mathf.Clamp01(value);
        ApplyAudioSettings();
        Save();
    }

    public static void SetEffectVolume(float value)
    {
        EnsureSettingsLoaded();
        sSettings.EffectVolume = Mathf.Clamp01(value);
        ApplyAudioSettings();
        Save();
    }

    public static void SetWindowed(bool isWindowed)
    {
        EnsureSettingsLoaded();
        sSettings.IsWindowed = isWindowed;
        ApplyRuntimeSettings();
        Save();
    }

    public static void SetUsableOption1(bool enabled)
    {
        SetUsableOption(UsableOption.LevelOutline, enabled);
    }

    public static void SetUsableOption2(bool enabled)
    {
        SetUsableOption(UsableOption.StickerOutline, enabled);
    }

    public static void SetUsableOption3(bool enabled)
    {
        SetUsableOption(UsableOption.HighContrast, enabled);
    }

    private static void SetUsableOption(UsableOption option, bool enabled)
    {
        EnsureSettingsLoaded();
        switch (option)
        {
            case UsableOption.LevelOutline:
                sSettings.UsableOption1 = enabled;
                break;
            case UsableOption.StickerOutline:
                sSettings.UsableOption2 = enabled;
                break;
            case UsableOption.HighContrast:
                sSettings.UsableOption3 = enabled;
                break;
        }

        Save();
    }

    public static void ApplyRuntimeSettings()
    {
        Sanitize(sSettings);
        ApplyAudioSettings();
        ApplyDisplayMode();
    }

    private static void ApplyAudioSettings()
    {
        AudioListener.volume = 1f;
        AudioManager.ApplySettingsVolumes(sSettings.MusicVolume, sSettings.EffectVolume);
        ApplyAudioSourceVolumes();
    }

    private static void ApplyDisplayMode()
    {
        var isWindowed = sSettings.IsWindowed;
        var targetMode = isWindowed
            ? FullScreenMode.Windowed
            : FullScreenMode.FullScreenWindow;
        if (sHasAppliedDisplayMode
            && sAppliedWindowedState == isWindowed
            && Screen.fullScreenMode == targetMode)
        {
            return;
        }

        sHasAppliedDisplayMode = true;
        sAppliedWindowedState = isWindowed;
        if (!isWindowed && Application.platform == RuntimePlatform.WindowsPlayer)
        {
            var nativeResolution = Screen.currentResolution;
            Screen.SetResolution(
                nativeResolution.width,
                nativeResolution.height,
                FullScreenMode.FullScreenWindow,
                nativeResolution.refreshRateRatio);
            return;
        }

        Screen.fullScreenMode = targetMode;
    }

    private static void EnsureSettingsLoaded()
    {
        if (!sHasLoaded)
        {
            Initialize();
        }
    }

    private static bool Save()
    {
        Sanitize(sSettings);
        if (!SqliteLocalStore.Initialize())
        {
            return false;
        }

        return SqliteLocalStore.Upsert(SettingsCollection, SettingsKey, sSettings);
    }

    private static void ApplyAudioSourceVolumes()
    {
        var audioSources = UnityEngine.Object.FindObjectsOfType<AudioSource>(true);
        for (var i = 0; i < audioSources.Length; i++)
        {
            var audioSource = audioSources[i];
            if (audioSource == null)
            {
                continue;
            }

            audioSource.volume = IsMusicSource(audioSource)
                ? sSettings.MusicVolume
                : sSettings.EffectVolume;
        }
    }

    private static bool IsMusicSource(AudioSource audioSource)
    {
        var objectName = audioSource.gameObject.name.ToLowerInvariant();
        return objectName.Contains("music") || objectName.Contains("bgm");
    }

    private static GameSettingsData CreateDefaultSettings()
    {
        return new GameSettingsData
        {
            MusicVolume = 1f,
            EffectVolume = 1f,
            IsWindowed = false,
            UsableOption1 = false,
            UsableOption2 = false,
            UsableOption3 = false
        };
    }

    private static void Sanitize(GameSettingsData settings)
    {
        if (settings == null)
        {
            sSettings = CreateDefaultSettings();
            return;
        }

        settings.MusicVolume = Mathf.Clamp01(settings.MusicVolume);
        settings.EffectVolume = Mathf.Clamp01(settings.EffectVolume);
    }
}
