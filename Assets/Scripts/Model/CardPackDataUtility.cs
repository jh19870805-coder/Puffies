using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// 卡包尺寸（与 CardPacks.csv 的 PackSize 列数值对应）。
/// </summary>
public enum CardPackSize
{
    None = 0,
    XS = 1,
    S = 2,
    M = 3,
    L = 4,
    XL = 5,
    XXL = 6,
    XXXL = 7,
}

/// <summary>
/// 本地卡包数据记录。
/// </summary>
[Serializable]
public struct CardPackRecord
{
    public int PackId;
    public CardPackSize PackSize;
    public bool IsUnlocked;
    public string UnlockTime;
    public bool IsPlayed;
}

/// <summary>
/// 用途：管理本地 SQLite 卡包表（尺寸、解锁状态与时间）。返回：按方法说明。
/// </summary>
public static class CardPackDataUtility
{
    private const string UnlockTimeFormat = "yyyy-MM-dd HH:mm:ss";

    private static bool sIsInitialized;

    /// <summary>
    /// 用途：准备卡包表访问（仅建表，不写入记录）。返回：是否初始化成功。
    /// </summary>
    public static bool Initialize()
    {
        if (sIsInitialized)
        {
            return true;
        }

        if (!SqliteLocalStore.IsInitialized && !SqliteLocalStore.Initialize())
        {
            Debug.LogError("CardPackDataUtility.Initialize failed: SqliteLocalStore is not ready.");
            return false;
        }

        sIsInitialized = true;
        EnsureDefaultPackUnlocked();
        Debug.Log("CardPackDataUtility initialized.");
        return true;
    }

    public static bool IsInitialized => sIsInitialized;

    /// <summary>
    /// 用途：确保默认卡包已解锁，保证新玩家可进入游戏。返回：是否成功。
    /// </summary>
    public static bool EnsureDefaultPackUnlocked()
    {
        EnsureInitialized();
        var defaultPackId = GameDefine.DefaultBagId;
        if (IsPackUnlocked(defaultPackId))
        {
            return true;
        }

        return TryUnlockPack(defaultPackId);
    }

    /// <summary>
    /// 用途：从 CardPacks.csv 读取卡包配置（不写数据库）。返回：是否找到。
    /// </summary>
    public static bool TryGetPackConfig(int packId, out CardPackSize packSize)
    {
        packSize = CardPackSize.None;
        if (packId <= 0)
        {
            return false;
        }

        if (GameConfigRepository.TryGetCardPackConfig(packId, out var config))
        {
            packSize = config.PackSize;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 用途：按卡包 Id 读取记录。返回：是否找到。
    /// </summary>
    public static bool TryGetPack(int packId, out CardPackRecord record)
    {
        EnsureInitialized();
        record = default;
        if (packId <= 0)
        {
            return false;
        }

        var rows = SqliteLocalStore.Query<CardPackTableRow>(
            $@"SELECT PackId, PackSize, IsUnlocked, UnlockTime, IsPlayed
               FROM {GameDefine.LocalSqliteCardPackTable}
               WHERE PackId = ?
               LIMIT 1",
            packId);
        if (rows == null || rows.Count == 0)
        {
            return false;
        }

        record = ToRecord(rows[0]);
        TryNormalizeAndPersistUnlockTime(ref record);
        return true;
    }

    /// <summary>
    /// 用途：获取全部卡包记录。返回：按 PackId 升序的列表。
    /// </summary>
    public static List<CardPackRecord> GetAllPacks()
    {
        EnsureInitialized();
        var rows = SqliteLocalStore.Query<CardPackTableRow>(
            $@"SELECT PackId, PackSize, IsUnlocked, UnlockTime, IsPlayed
               FROM {GameDefine.LocalSqliteCardPackTable}
               ORDER BY PackId");
        var records = new List<CardPackRecord>(rows.Count);
        for (var i = 0; i < rows.Count; i++)
        {
            var record = ToRecord(rows[i]);
            TryNormalizeAndPersistUnlockTime(ref record);
            records.Add(record);
        }

        return records;
    }

    /// <summary>
    /// 用途：获取已解锁卡包 Id 列表（含默认卡包）。返回：按 PackId 升序。
    /// </summary>
    public static List<int> GetUnlockedPackIds()
    {
        EnsureInitialized();
        EnsureDefaultPackUnlocked();

        var rows = SqliteLocalStore.Query<CardPackIdRow>(
            $@"SELECT PackId
               FROM {GameDefine.LocalSqliteCardPackTable}
               WHERE IsUnlocked = 1
               ORDER BY PackId");
        var packIds = new List<int>(rows.Count);
        for (var i = 0; i < rows.Count; i++)
        {
            packIds.Add(rows[i].PackId);
        }

        return packIds;
    }

    /// <summary>
    /// 用途：解锁卡包；无记录时按配置创建后解锁。返回：是否成功。
    /// </summary>
    public static bool TryUnlockPack(int packId)
    {
        EnsureInitialized();
        if (packId <= 0)
        {
            return false;
        }

        if (!TryGetPack(packId, out var record))
        {
            if (!TryGetPackConfig(packId, out var packSize))
            {
                Debug.LogWarning($"CardPackDataUtility.TryUnlockPack skipped, config not found: {packId}");
                return false;
            }

            record = new CardPackRecord
            {
                PackId = packId,
                PackSize = packSize,
                IsUnlocked = false,
                UnlockTime = string.Empty,
                IsPlayed = false
            };
        }

        record.IsUnlocked = true;
        record.UnlockTime = FormatUnlockTime(DateTime.Now);
        return UpsertPack(record);
    }

    /// <summary>
    /// 用途：任务奖励解锁卡包（已解锁、未玩过）。返回：是否成功。
    /// </summary>
    public static bool TryUnlockPackFromTaskReward(int packId)
    {
        EnsureInitialized();
        if (packId <= 0)
        {
            return false;
        }

        if (!TryGetPackConfig(packId, out var packSize))
        {
            Debug.LogWarning($"CardPackDataUtility.TryUnlockPackFromTaskReward skipped, config not found: {packId}");
            return false;
        }

        if (!TryGetPack(packId, out var record))
        {
            record = new CardPackRecord
            {
                PackId = packId
            };
        }

        record.PackSize = packSize;
        record.IsUnlocked = true;
        record.IsPlayed = false;
        record.UnlockTime = FormatUnlockTime(DateTime.Now);
        return UpsertPack(record);
    }

    /// <summary>
    /// 用途：写入或覆盖卡包记录。返回：是否保存成功。
    /// </summary>
    public static bool UpsertPack(CardPackRecord record)
    {
        EnsureInitialized();
        return UpsertPackInternal(record);
    }

    /// <summary>
    /// 用途：判断卡包是否已解锁。返回：已解锁为 true；记录不存在为 false。
    /// </summary>
    public static bool IsPackUnlocked(int packId)
    {
        return TryGetPack(packId, out var record) && record.IsUnlocked;
    }

    /// <summary>
    /// 用途：拼图完成后保存卡包数据（同 PackId 仅保留一条，覆盖更新）。返回：是否成功。
    /// </summary>
    public static bool TrySavePackAfterPuzzleComplete(int packId)
    {
        EnsureInitialized();
        if (packId <= 0)
        {
            return false;
        }

        if (!TryGetPackConfig(packId, out var packSize))
        {
            Debug.LogWarning($"CardPackDataUtility.TrySavePackAfterPuzzleComplete skipped, config not found: {packId}");
            return false;
        }

        if (!TryGetPack(packId, out var record))
        {
            record = new CardPackRecord
            {
                PackId = packId,
                PackSize = packSize,
                IsUnlocked = false,
                UnlockTime = string.Empty,
                IsPlayed = false
            };
        }
        else
        {
            record.PackSize = packSize;
        }

        record.IsPlayed = true;
        return UpsertPack(record);
    }

    /// <summary>
    /// 用途：标记卡包已玩过；无记录时按配置创建后标记。返回：是否成功。
    /// </summary>
    public static bool TryMarkPackPlayed(int packId)
    {
        EnsureInitialized();
        if (packId <= 0)
        {
            return false;
        }

        if (!TryGetPack(packId, out var record))
        {
            if (!TryGetPackConfig(packId, out var packSize))
            {
                Debug.LogWarning($"CardPackDataUtility.TryMarkPackPlayed skipped, config not found: {packId}");
                return false;
            }

            record = new CardPackRecord
            {
                PackId = packId,
                PackSize = packSize,
                IsUnlocked = false,
                UnlockTime = string.Empty,
                IsPlayed = false
            };
        }

        if (record.IsPlayed)
        {
            return true;
        }

        record.IsPlayed = true;
        return UpsertPack(record);
    }

    /// <summary>
    /// 用途：判断卡包是否已玩过。返回：已玩过为 true；记录不存在为 false。
    /// </summary>
    public static bool IsPackPlayed(int packId)
    {
        return TryGetPack(packId, out var record) && record.IsPlayed;
    }

    /// <summary>
    /// 用途：将 DateTime 格式化为解锁时间字符串（YYYY-MM-DD HH:MM:SS）。返回：格式化结果。
    /// </summary>
    public static string FormatUnlockTime(DateTime dateTime)
    {
        return dateTime.ToString(UnlockTimeFormat, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 用途：解析解锁时间字符串。返回：是否解析成功。
    /// </summary>
    public static bool TryParseUnlockTime(string unlockTime, out DateTime dateTime)
    {
        return DateTime.TryParseExact(
            unlockTime,
            UnlockTimeFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out dateTime);
    }

    private static void EnsureInitialized()
    {
        if (!sIsInitialized)
        {
            Initialize();
        }
    }

    private static bool UpsertPackInternal(CardPackRecord record)
    {
        if (record.PackId <= 0)
        {
            Debug.LogWarning($"CardPackDataUtility.UpsertPack skipped, invalid packId={record.PackId}");
            return false;
        }

        EnsurePlayedPackState(ref record);
        EnsureUnlockTime(ref record);
        var unlockTime = record.UnlockTime ?? string.Empty;
        var affected = SqliteLocalStore.ExecuteNonQuery(
            $@"INSERT INTO {GameDefine.LocalSqliteCardPackTable}
               (PackId, PackSize, IsUnlocked, UnlockTime, IsPlayed)
               VALUES (?, ?, ?, ?, ?)
               ON CONFLICT(PackId) DO UPDATE SET
                PackSize = excluded.PackSize,
                IsUnlocked = excluded.IsUnlocked,
                UnlockTime = excluded.UnlockTime,
                IsPlayed = excluded.IsPlayed",
            record.PackId,
            (int)record.PackSize,
            record.IsUnlocked ? 1 : 0,
            unlockTime,
            record.IsPlayed ? 1 : 0);
        return affected > 0;
    }

    private static CardPackRecord ToRecord(CardPackTableRow row)
    {
        return new CardPackRecord
        {
            PackId = row.PackId,
            PackSize = (CardPackSize)row.PackSize,
            IsUnlocked = row.IsUnlocked != 0,
            UnlockTime = row.UnlockTime ?? string.Empty,
            IsPlayed = row.IsPlayed != 0
        };
    }

    private static bool HasUnlockTime(string unlockTime)
    {
        return !string.IsNullOrWhiteSpace(unlockTime)
            && TryParseUnlockTime(unlockTime, out _);
    }

    private static void EnsurePlayedPackState(ref CardPackRecord record)
    {
        if (!record.IsPlayed)
        {
            return;
        }

        record.IsUnlocked = true;
        EnsureUnlockTime(ref record);
    }

    private static void EnsureUnlockTime(ref CardPackRecord record)
    {
        if (!record.IsUnlocked || HasUnlockTime(record.UnlockTime))
        {
            return;
        }

        record.UnlockTime = FormatUnlockTime(DateTime.Now);
    }

    private static void TryNormalizeAndPersistUnlockTime(ref CardPackRecord record)
    {
        var unlockTimeBefore = record.UnlockTime;
        var isUnlockedBefore = record.IsUnlocked;
        EnsurePlayedPackState(ref record);
        EnsureUnlockTime(ref record);
        if (record.UnlockTime != unlockTimeBefore || record.IsUnlocked != isUnlockedBefore)
        {
            UpsertPackInternal(record);
        }
    }

    private sealed class CardPackTableRow
    {
        public int PackId { get; set; }
        public int PackSize { get; set; }
        public int IsUnlocked { get; set; }
        public string UnlockTime { get; set; }
        public int IsPlayed { get; set; }
    }

    private sealed class CardPackIdRow
    {
        public int PackId { get; set; }
    }
}
