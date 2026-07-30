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

public enum CardPackLifecycleState
{
    Locked = 0,
    Unlocked = 1,
    InProgress = 2,
    Completed = 3,
}

/// <summary>
/// 本地卡包数据记录。
/// </summary>
[Serializable]
public struct CardPackRecord
{
    public int PackId;
    public CardPackSize PackSize;
    public CardPackLifecycleState LifecycleState;
    public string UnlockTime;
    public string CompletionTime;

    public bool IsUnlocked => LifecycleState != CardPackLifecycleState.Locked;
    public bool IsPlayed => LifecycleState == CardPackLifecycleState.InProgress
        || LifecycleState == CardPackLifecycleState.Completed;
    public bool IsCompleted => LifecycleState == CardPackLifecycleState.Completed;
}

/// <summary>
/// 用途：管理本地 SQLite 卡包表（尺寸、解锁状态与时间）。返回：按方法说明。
/// </summary>
public static class CardPackDataUtility
{
    private const string TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff";

    private static bool sIsInitialized;
    private static readonly HashSet<int> sNewlyUnlockedPackIds = new HashSet<int>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSessionOrderingState()
    {
        sNewlyUnlockedPackIds.Clear();
    }

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
            $@"SELECT PackId, PackSize, LifecycleState, UnlockTime, CompletionTime
               FROM {GameDefine.LocalSqliteCardPackTable}
               WHERE PackId = ?
               LIMIT 1",
            packId);
        if (rows == null || rows.Count == 0)
        {
            return false;
        }

        record = ToRecord(rows[0]);
        TryNormalizeAndPersistTimes(ref record);
        return true;
    }

    /// <summary>
    /// 用途：获取全部卡包记录。返回：按 PackId 升序的列表。
    /// </summary>
    public static List<CardPackRecord> GetAllPacks()
    {
        EnsureInitialized();
        var rows = SqliteLocalStore.Query<CardPackTableRow>(
            $@"SELECT PackId, PackSize, LifecycleState, UnlockTime, CompletionTime
               FROM {GameDefine.LocalSqliteCardPackTable}
               ORDER BY PackId");
        var records = new List<CardPackRecord>(rows.Count);
        for (var i = 0; i < rows.Count; i++)
        {
            var record = ToRecord(rows[i]);
            TryNormalizeAndPersistTimes(ref record);
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
               WHERE LifecycleState <> ?
               ORDER BY PackId",
            (int)CardPackLifecycleState.Locked);
        var packIds = new List<int>(rows.Count);
        for (var i = 0; i < rows.Count; i++)
        {
            packIds.Add(rows[i].PackId);
        }

        return packIds;
    }

    /// <summary>
    /// 用途：获取已完成卡包数量。返回：生命周期为 Completed 的卡包记录数。
    /// </summary>
    public static int GetCompletedPackCount()
    {
        EnsureInitialized();
        return SqliteLocalStore.ExecuteScalar<int>(
            $@"SELECT COUNT(1)
               FROM {GameDefine.LocalSqliteCardPackTable}
               WHERE LifecycleState = ?",
            (int)CardPackLifecycleState.Completed);
    }

    public static List<int> TakeMainSceneOrderedPackIds()
    {
        EnsureInitialized();
        EnsureDefaultPackUnlocked();

        var records = GetAllPacks();
        records.RemoveAll(record => record.LifecycleState == CardPackLifecycleState.Locked);
        var newlyUnlockedPackIds = new HashSet<int>(sNewlyUnlockedPackIds);
        records.Sort((left, right) => CompareMainSceneOrder(left, right, newlyUnlockedPackIds));
        sNewlyUnlockedPackIds.Clear();

        var packIds = new List<int>(records.Count);
        for (var i = 0; i < records.Count; i++)
        {
            packIds.Add(records[i].PackId);
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
                LifecycleState = CardPackLifecycleState.Locked,
                UnlockTime = string.Empty,
                CompletionTime = string.Empty
            };
        }

        var newlyUnlocked = record.LifecycleState == CardPackLifecycleState.Locked;
        if (record.LifecycleState == CardPackLifecycleState.Locked)
        {
            record.LifecycleState = CardPackLifecycleState.Unlocked;
            record.UnlockTime = FormatUnlockTime(DateTime.Now);
            record.CompletionTime = string.Empty;
        }

        var saved = UpsertPack(record);
        if (saved && newlyUnlocked)
        {
            sNewlyUnlockedPackIds.Add(packId);
        }

        return saved;
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

        var recordExists = TryGetPack(packId, out var record);
        if (!recordExists)
        {
            record = new CardPackRecord
            {
                PackId = packId
            };
        }
        else if (record.LifecycleState != CardPackLifecycleState.Locked)
        {
            return false;
        }

        record.PackSize = packSize;
        var newlyUnlocked = !recordExists || record.LifecycleState == CardPackLifecycleState.Locked;
        record.LifecycleState = CardPackLifecycleState.Unlocked;
        record.UnlockTime = FormatUnlockTime(DateTime.Now);
        record.CompletionTime = string.Empty;
        var saved = UpsertPack(record);
        if (saved && newlyUnlocked)
        {
            sNewlyUnlockedPackIds.Add(packId);
        }

        return saved;
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

    public static bool TryGetPackLifecycleState(int packId, out CardPackLifecycleState lifecycleState)
    {
        lifecycleState = CardPackLifecycleState.Locked;
        if (!TryGetPack(packId, out var record))
        {
            return false;
        }

        lifecycleState = record.LifecycleState;
        return true;
    }

    public static bool TryMarkPackInProgress(int packId)
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
                Debug.LogWarning($"CardPackDataUtility.TryMarkPackInProgress skipped, config not found: {packId}");
                return false;
            }

            record = new CardPackRecord
            {
                PackId = packId,
                PackSize = packSize,
                LifecycleState = CardPackLifecycleState.Unlocked,
                UnlockTime = FormatUnlockTime(DateTime.Now),
                CompletionTime = string.Empty
            };
        }

        if (record.LifecycleState == CardPackLifecycleState.Completed
            || record.LifecycleState == CardPackLifecycleState.InProgress)
        {
            return true;
        }

        record.LifecycleState = CardPackLifecycleState.InProgress;
        return UpsertPack(record);
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
                LifecycleState = CardPackLifecycleState.Locked,
                UnlockTime = string.Empty,
                CompletionTime = string.Empty
            };
        }
        else
        {
            record.PackSize = packSize;
        }

        var wasCompleted = record.LifecycleState == CardPackLifecycleState.Completed;
        record.LifecycleState = CardPackLifecycleState.Completed;
        if (!wasCompleted)
        {
            record.CompletionTime = FormatUnlockTime(DateTime.Now);
        }

        return UpsertPack(record);
    }

    /// <summary>
    /// 用途：标记卡包已玩过；无记录时按配置创建后标记。返回：是否成功。
    /// </summary>
    public static bool TryMarkPackPlayed(int packId)
    {
        return TryMarkPackInProgress(packId);
    }

    /// <summary>
    /// 用途：判断卡包是否已玩过。返回：已玩过为 true；记录不存在为 false。
    /// </summary>
    public static bool IsPackPlayed(int packId)
    {
        return TryGetPack(packId, out var record) && record.IsPlayed;
    }

    public static bool IsPackCompleted(int packId)
    {
        return TryGetPack(packId, out var record) && record.IsCompleted;
    }

    public static bool TryEnsurePuzzleSession(int packId)
    {
        EnsureInitialized();
        if (packId <= 0)
        {
            return false;
        }

        var payload = new PuzzleProgressPayload();
        SqliteLocalStore.ExecuteNonQuery(
            $@"INSERT OR IGNORE INTO {GameDefine.LocalSqliteCardPackPuzzleProgressTable}
               (PackId, PlacedPieceNumbersJson, UpdatedTime)
               VALUES (?, ?, ?)",
            packId,
            JsonUtility.ToJson(payload),
            FormatUnlockTime(DateTime.Now));
        return HasActivePuzzleSession(packId);
    }

    public static bool HasActivePuzzleSession(int packId)
    {
        EnsureInitialized();
        if (packId <= 0)
        {
            return false;
        }

        var count = SqliteLocalStore.ExecuteScalar<int>(
            $@"SELECT COUNT(1)
               FROM {GameDefine.LocalSqliteCardPackPuzzleProgressTable}
               WHERE PackId = ?",
            packId);
        return count > 0;
    }

    public static bool TryGetPlacedPieceNumbers(int packId, out HashSet<int> pieceNumbers)
    {
        EnsureInitialized();
        pieceNumbers = new HashSet<int>();
        if (packId <= 0)
        {
            return false;
        }

        var rows = SqliteLocalStore.Query<PuzzleProgressTableRow>(
            $@"SELECT PackId, PlacedPieceNumbersJson, UpdatedTime
               FROM {GameDefine.LocalSqliteCardPackPuzzleProgressTable}
               WHERE PackId = ?
               LIMIT 1",
            packId);
        if (rows == null || rows.Count == 0)
        {
            return false;
        }

        try
        {
            var json = rows[0].PlacedPieceNumbersJson;
            var payload = string.IsNullOrWhiteSpace(json)
                ? new PuzzleProgressPayload()
                : JsonUtility.FromJson<PuzzleProgressPayload>(json);
            if (payload?.PieceNumbers == null)
            {
                return true;
            }

            for (var i = 0; i < payload.PieceNumbers.Count; i++)
            {
                if (payload.PieceNumbers[i] > 0)
                {
                    pieceNumbers.Add(payload.PieceNumbers[i]);
                }
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"CardPackDataUtility: invalid puzzle progress ignored. packId={packId}, error={exception.Message}");
            pieceNumbers.Clear();
            return false;
        }
    }

    public static bool TryRecordPlacedPiece(int packId, int pieceNumber)
    {
        return TryRecordPlacedPieces(packId, new[] { pieceNumber });
    }

    public static bool TryRecordPlacedPieces(int packId, IEnumerable<int> placedPieceNumbers)
    {
        if (packId <= 0 || placedPieceNumbers == null || !TryEnsurePuzzleSession(packId))
        {
            return false;
        }

        TryGetPlacedPieceNumbers(packId, out var pieceNumbers);
        foreach (var pieceNumber in placedPieceNumbers)
        {
            if (pieceNumber > 0)
            {
                pieceNumbers.Add(pieceNumber);
            }
        }

        if (pieceNumbers.Count == 0)
        {
            return false;
        }

        var sortedPieceNumbers = new List<int>(pieceNumbers);
        sortedPieceNumbers.Sort();
        var payload = new PuzzleProgressPayload
        {
            PieceNumbers = sortedPieceNumbers
        };
        var affected = SqliteLocalStore.ExecuteNonQuery(
            $@"UPDATE {GameDefine.LocalSqliteCardPackPuzzleProgressTable}
               SET PlacedPieceNumbersJson = ?, UpdatedTime = ?
               WHERE PackId = ?",
            JsonUtility.ToJson(payload),
            FormatUnlockTime(DateTime.Now),
            packId);
        return affected > 0;
    }

    public static bool TryClearPuzzleSession(int packId)
    {
        EnsureInitialized();
        if (packId <= 0)
        {
            return false;
        }

        SqliteLocalStore.ExecuteNonQuery(
            $@"DELETE FROM {GameDefine.LocalSqliteCardPackPuzzleProgressTable}
               WHERE PackId = ?",
            packId);
        return !HasActivePuzzleSession(packId);
    }

    /// <summary>
    /// 用途：将 DateTime 格式化为解锁时间字符串（YYYY-MM-DD HH:MM:SS）。返回：格式化结果。
    /// </summary>
    public static string FormatUnlockTime(DateTime dateTime)
    {
        return dateTime.ToString(TimestampFormat, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 用途：解析解锁时间字符串。返回：是否解析成功。
    /// </summary>
    public static bool TryParseUnlockTime(string unlockTime, out DateTime dateTime)
    {
        return DateTime.TryParseExact(
            unlockTime,
            TimestampFormat,
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

        EnsureValidLifecycleState(ref record);
        EnsureUnlockTime(ref record);
        EnsureCompletionTime(ref record);
        var unlockTime = record.UnlockTime ?? string.Empty;
        var completionTime = record.CompletionTime ?? string.Empty;
        var affected = SqliteLocalStore.ExecuteNonQuery(
            $@"INSERT INTO {GameDefine.LocalSqliteCardPackTable}
               (PackId, PackSize, LifecycleState, UnlockTime, CompletionTime)
               VALUES (?, ?, ?, ?, ?)
               ON CONFLICT(PackId) DO UPDATE SET
                PackSize = excluded.PackSize,
                LifecycleState = excluded.LifecycleState,
                UnlockTime = excluded.UnlockTime,
                CompletionTime = excluded.CompletionTime",
            record.PackId,
            (int)record.PackSize,
            (int)record.LifecycleState,
            unlockTime,
            completionTime);
        return affected > 0;
    }

    private static CardPackRecord ToRecord(CardPackTableRow row)
    {
        return new CardPackRecord
        {
            PackId = row.PackId,
            PackSize = (CardPackSize)row.PackSize,
            LifecycleState = ResolveLifecycleState(row),
            UnlockTime = row.UnlockTime ?? string.Empty,
            CompletionTime = row.CompletionTime ?? string.Empty
        };
    }

    private static bool HasUnlockTime(string unlockTime)
    {
        return !string.IsNullOrWhiteSpace(unlockTime)
            && TryParseUnlockTime(unlockTime, out _);
    }

    private static CardPackLifecycleState ResolveLifecycleState(CardPackTableRow row)
    {
        return Enum.IsDefined(typeof(CardPackLifecycleState), row.LifecycleState)
            ? (CardPackLifecycleState)row.LifecycleState
            : CardPackLifecycleState.Locked;
    }

    private static void EnsureValidLifecycleState(ref CardPackRecord record)
    {
        if (!Enum.IsDefined(typeof(CardPackLifecycleState), record.LifecycleState))
        {
            record.LifecycleState = CardPackLifecycleState.Locked;
        }
    }

    private static void EnsureUnlockTime(ref CardPackRecord record)
    {
        if (record.LifecycleState == CardPackLifecycleState.Locked)
        {
            record.UnlockTime = string.Empty;
            return;
        }

        if (HasUnlockTime(record.UnlockTime))
        {
            return;
        }

        record.UnlockTime = FormatUnlockTime(DateTime.Now);
    }

    private static void EnsureCompletionTime(ref CardPackRecord record)
    {
        if (record.LifecycleState != CardPackLifecycleState.Completed)
        {
            record.CompletionTime = string.Empty;
            return;
        }

        if (HasUnlockTime(record.CompletionTime))
        {
            return;
        }

        record.CompletionTime = FormatUnlockTime(DateTime.Now);
    }

    private static void TryNormalizeAndPersistTimes(ref CardPackRecord record)
    {
        var unlockTimeBefore = record.UnlockTime;
        var completionTimeBefore = record.CompletionTime;
        var lifecycleStateBefore = record.LifecycleState;
        EnsureValidLifecycleState(ref record);
        EnsureUnlockTime(ref record);
        EnsureCompletionTime(ref record);
        if (record.UnlockTime != unlockTimeBefore
            || record.CompletionTime != completionTimeBefore
            || record.LifecycleState != lifecycleStateBefore)
        {
            UpsertPackInternal(record);
        }
    }

    private static int CompareMainSceneOrder(
        CardPackRecord left,
        CardPackRecord right,
        HashSet<int> newlyUnlockedPackIds)
    {
        var leftIsNew = newlyUnlockedPackIds.Contains(left.PackId);
        var rightIsNew = newlyUnlockedPackIds.Contains(right.PackId);
        if (leftIsNew != rightIsNew)
        {
            return leftIsNew ? -1 : 1;
        }

        if (leftIsNew)
        {
            var newTimeComparison = CompareTimestamp(left.UnlockTime, right.UnlockTime, descending: true);
            if (newTimeComparison != 0)
            {
                return newTimeComparison;
            }
        }

        var priorityComparison = GetMainSceneLifecyclePriority(left.LifecycleState)
            .CompareTo(GetMainSceneLifecyclePriority(right.LifecycleState));
        if (priorityComparison != 0)
        {
            return priorityComparison;
        }

        var leftTime = left.LifecycleState == CardPackLifecycleState.Completed
            ? left.CompletionTime
            : left.UnlockTime;
        var rightTime = right.LifecycleState == CardPackLifecycleState.Completed
            ? right.CompletionTime
            : right.UnlockTime;
        var timeComparison = CompareTimestamp(leftTime, rightTime, descending: false);
        return timeComparison != 0 ? timeComparison : left.PackId.CompareTo(right.PackId);
    }

    private static int GetMainSceneLifecyclePriority(CardPackLifecycleState lifecycleState)
    {
        switch (lifecycleState)
        {
            case CardPackLifecycleState.InProgress:
                return 0;
            case CardPackLifecycleState.Unlocked:
                return 1;
            case CardPackLifecycleState.Completed:
                return 2;
            default:
                return 3;
        }
    }

    private static int CompareTimestamp(string left, string right, bool descending)
    {
        var leftIsValid = TryParseUnlockTime(left, out var leftTime);
        var rightIsValid = TryParseUnlockTime(right, out var rightTime);
        if (leftIsValid != rightIsValid)
        {
            return leftIsValid ? -1 : 1;
        }

        if (!leftIsValid)
        {
            return 0;
        }

        return descending ? rightTime.CompareTo(leftTime) : leftTime.CompareTo(rightTime);
    }

    private sealed class CardPackTableRow
    {
        public int PackId { get; set; }
        public int PackSize { get; set; }
        public int LifecycleState { get; set; }
        public string UnlockTime { get; set; }
        public string CompletionTime { get; set; }
    }

    private sealed class CardPackIdRow
    {
        public int PackId { get; set; }
    }

    [Serializable]
    private sealed class PuzzleProgressPayload
    {
        public List<int> PieceNumbers = new List<int>();
    }

    private sealed class PuzzleProgressTableRow
    {
        public int PackId { get; set; }
        public string PlacedPieceNumbersJson { get; set; }
        public string UpdatedTime { get; set; }
    }
}

public enum CardPackChapterStage
{
    None = 0,
    Initial = 1,
    MidToLate = 2,
    Final = 3,
}

public readonly struct CardPackGrantDecision
{
    public CardPackGrantDecision(
        CardPackChapterStage stage,
        int remainingLockedCount,
        int heldPlayableCount,
        int maximumHeldBeforeGrant,
        int expectedHeldAfterGrant,
        bool shouldGrant)
    {
        Stage = stage;
        RemainingLockedCount = remainingLockedCount;
        HeldPlayableCount = heldPlayableCount;
        MaximumHeldBeforeGrant = maximumHeldBeforeGrant;
        ExpectedHeldAfterGrant = expectedHeldAfterGrant;
        ShouldGrant = shouldGrant;
    }

    public CardPackChapterStage Stage { get; }
    public int RemainingLockedCount { get; }
    public int HeldPlayableCount { get; }
    public int MaximumHeldBeforeGrant { get; }
    public int ExpectedHeldAfterGrant { get; }
    public bool ShouldGrant { get; }
}

[Serializable]
public sealed class PendingCardPackTaskReward
{
    public int TaskId;
    public int PreferredPackId;
}

[Serializable]
public sealed class CardPackDistributionProgressData
{
    public List<PendingCardPackTaskReward> PendingTaskRewards = new List<PendingCardPackTaskReward>();
}

public static class CardPackDistributionUtility
{
    private const string ProgressCollection = "CardPackDistribution";
    private const string ProgressKey = "Progress";

    public static bool EnqueueTaskReward(int taskId, int preferredPackId)
    {
        if (taskId <= 0)
        {
            return false;
        }

        var progress = LoadProgress();
        for (var i = 0; i < progress.PendingTaskRewards.Count; i++)
        {
            if (progress.PendingTaskRewards[i] != null
                && progress.PendingTaskRewards[i].TaskId == taskId)
            {
                return true;
            }
        }

        progress.PendingTaskRewards.Add(new PendingCardPackTaskReward
        {
            TaskId = taskId,
            PreferredPackId = Mathf.Max(0, preferredPackId)
        });
        return SaveProgress(progress);
    }

    public static int GetPendingTaskRewardCount()
    {
        return LoadProgress().PendingTaskRewards.Count;
    }

    public static bool TryGrantPendingTaskReward(
        out int grantedPackId,
        out int chapterId,
        out CardPackGrantDecision decision)
    {
        grantedPackId = 0;
        chapterId = 0;
        decision = default;
        var progress = LoadProgress();
        if (progress.PendingTaskRewards.Count == 0
            || !TryBuildState(out var configs, out var states))
        {
            return false;
        }

        chapterId = ResolveTaskRewardChapter(configs, states);
        if (chapterId <= 0)
        {
            return false;
        }

        var remainingLockedCount = CountState(configs, states, chapterId, CardPackLifecycleState.Locked);
        var heldPlayableCount = CountPlayable(configs, states, chapterId);
        decision = EvaluateGrant(remainingLockedCount, heldPlayableCount);
        if (!decision.ShouldGrant)
        {
            return false;
        }

        var preferredPackId = progress.PendingTaskRewards[0].PreferredPackId;
        var candidate = FindLockedCandidate(configs, states, chapterId, preferredPackId);
        if (candidate.PackId <= 0 || !CardPackDataUtility.TryUnlockPack(candidate.PackId))
        {
            return false;
        }

        grantedPackId = candidate.PackId;
        progress.PendingTaskRewards.RemoveAt(0);
        if (!SaveProgress(progress))
        {
            Debug.LogError(
                $"CardPackDistributionUtility: granted pack {grantedPackId} but failed to persist pending task reward removal.");
        }

        return true;
    }

    public static bool TryGrantFirstCompletionReward(
        int completedPackId,
        out int grantedPackId,
        out int chapterId,
        out CardPackGrantDecision decision)
    {
        grantedPackId = 0;
        chapterId = 0;
        decision = default;
        if (!TryBuildState(out var configs, out var states)
            || !TryFindConfig(configs, completedPackId, out var completedConfig))
        {
            return false;
        }

        chapterId = ResolveCompletionRewardChapter(configs, states, completedConfig.ChapterId);
        if (chapterId <= 0)
        {
            return false;
        }

        var remainingLockedCount = CountState(configs, states, chapterId, CardPackLifecycleState.Locked);
        var heldPlayableCount = CountPlayable(configs, states, chapterId);
        decision = EvaluateGrant(remainingLockedCount, heldPlayableCount);
        if (!decision.ShouldGrant)
        {
            return false;
        }

        var candidate = FindLockedCandidate(configs, states, chapterId, 0);
        if (candidate.PackId <= 0 || !CardPackDataUtility.TryUnlockPack(candidate.PackId))
        {
            return false;
        }

        grantedPackId = candidate.PackId;
        return true;
    }

    public static CardPackGrantDecision EvaluateGrant(int remainingLockedCount, int heldPlayableCount)
    {
        var stage = ResolveStage(remainingLockedCount);
        var maximumHeldBeforeGrant = 0;
        var expectedHeldAfterGrant = 0;
        var shouldGrant = false;
        switch (stage)
        {
            case CardPackChapterStage.Initial:
                maximumHeldBeforeGrant = 5;
                expectedHeldAfterGrant = 6;
                shouldGrant = heldPlayableCount <= maximumHeldBeforeGrant;
                break;
            case CardPackChapterStage.MidToLate:
                maximumHeldBeforeGrant = remainingLockedCount == 8 ? 3 : 2;
                expectedHeldAfterGrant = maximumHeldBeforeGrant + 1;
                shouldGrant = heldPlayableCount <= maximumHeldBeforeGrant;
                break;
            case CardPackChapterStage.Final:
                maximumHeldBeforeGrant = 1;
                expectedHeldAfterGrant = 2;
                shouldGrant = heldPlayableCount <= maximumHeldBeforeGrant;
                break;
        }

        return new CardPackGrantDecision(
            stage,
            Mathf.Max(0, remainingLockedCount),
            Mathf.Max(0, heldPlayableCount),
            maximumHeldBeforeGrant,
            expectedHeldAfterGrant,
            shouldGrant);
    }

    private static CardPackChapterStage ResolveStage(int remainingLockedCount)
    {
        if (remainingLockedCount >= 9)
        {
            return CardPackChapterStage.Initial;
        }

        if (remainingLockedCount >= 3)
        {
            return CardPackChapterStage.MidToLate;
        }

        if (remainingLockedCount >= 1)
        {
            return CardPackChapterStage.Final;
        }

        return CardPackChapterStage.None;
    }

    private static CardPackDistributionProgressData LoadProgress()
    {
        if (SqliteLocalStore.Initialize()
            && SqliteLocalStore.TryRead(
                ProgressCollection,
                ProgressKey,
                out CardPackDistributionProgressData progress)
            && progress != null)
        {
            if (progress.PendingTaskRewards == null)
            {
                progress.PendingTaskRewards = new List<PendingCardPackTaskReward>();
            }

            progress.PendingTaskRewards.RemoveAll(item => item == null || item.TaskId <= 0);

            return progress;
        }

        return new CardPackDistributionProgressData();
    }

    private static bool SaveProgress(CardPackDistributionProgressData progress)
    {
        if (progress == null || !SqliteLocalStore.Initialize())
        {
            return false;
        }

        if (progress.PendingTaskRewards == null)
        {
            progress.PendingTaskRewards = new List<PendingCardPackTaskReward>();
        }

        return SqliteLocalStore.Upsert(ProgressCollection, ProgressKey, progress);
    }

    private static bool TryBuildState(
        out IReadOnlyList<CardPackConfigData> configs,
        out Dictionary<int, CardPackLifecycleState> states)
    {
        states = new Dictionary<int, CardPackLifecycleState>();
        if (!GameConfigRepository.TryGetCardPackConfigs(out configs))
        {
            return false;
        }

        var records = CardPackDataUtility.GetAllPacks();
        for (var i = 0; i < records.Count; i++)
        {
            states[records[i].PackId] = records[i].LifecycleState;
        }

        return true;
    }

    private static int ResolveTaskRewardChapter(
        IReadOnlyList<CardPackConfigData> configs,
        Dictionary<int, CardPackLifecycleState> states)
    {
        var playableChapter = int.MaxValue;
        for (var i = 0; i < configs.Count; i++)
        {
            var state = GetState(states, configs[i].PackId);
            if (IsPlayable(state) && configs[i].ChapterId < playableChapter)
            {
                playableChapter = configs[i].ChapterId;
            }
        }

        if (playableChapter != int.MaxValue
            && CountState(configs, states, playableChapter, CardPackLifecycleState.Locked) > 0)
        {
            return playableChapter;
        }

        var minimumChapter = playableChapter == int.MaxValue ? int.MinValue : playableChapter + 1;
        return FindFirstChapterWithLockedPack(configs, states, minimumChapter);
    }

    private static int ResolveCompletionRewardChapter(
        IReadOnlyList<CardPackConfigData> configs,
        Dictionary<int, CardPackLifecycleState> states,
        int completedChapterId)
    {
        if (CountState(configs, states, completedChapterId, CardPackLifecycleState.Locked) > 0)
        {
            return completedChapterId;
        }

        return FindFirstChapterWithLockedPack(configs, states, completedChapterId + 1);
    }

    private static int FindFirstChapterWithLockedPack(
        IReadOnlyList<CardPackConfigData> configs,
        Dictionary<int, CardPackLifecycleState> states,
        int minimumChapter)
    {
        var chapterId = int.MaxValue;
        for (var i = 0; i < configs.Count; i++)
        {
            var config = configs[i];
            if (config.ChapterId < minimumChapter
                || config.ChapterId >= chapterId
                || GetState(states, config.PackId) != CardPackLifecycleState.Locked)
            {
                continue;
            }

            chapterId = config.ChapterId;
        }

        return chapterId == int.MaxValue ? 0 : chapterId;
    }

    private static CardPackConfigData FindLockedCandidate(
        IReadOnlyList<CardPackConfigData> configs,
        Dictionary<int, CardPackLifecycleState> states,
        int chapterId,
        int preferredPackId)
    {
        var candidate = default(CardPackConfigData);
        for (var i = 0; i < configs.Count; i++)
        {
            var config = configs[i];
            if (config.ChapterId != chapterId
                || GetState(states, config.PackId) != CardPackLifecycleState.Locked)
            {
                continue;
            }

            if (config.PackId == preferredPackId)
            {
                return config;
            }

            if (candidate.PackId <= 0 || config.Index < candidate.Index)
            {
                candidate = config;
            }
        }

        return candidate;
    }

    private static int CountState(
        IReadOnlyList<CardPackConfigData> configs,
        Dictionary<int, CardPackLifecycleState> states,
        int chapterId,
        CardPackLifecycleState targetState)
    {
        var count = 0;
        for (var i = 0; i < configs.Count; i++)
        {
            if (configs[i].ChapterId == chapterId
                && GetState(states, configs[i].PackId) == targetState)
            {
                count++;
            }
        }

        return count;
    }

    private static int CountPlayable(
        IReadOnlyList<CardPackConfigData> configs,
        Dictionary<int, CardPackLifecycleState> states,
        int chapterId)
    {
        var count = 0;
        for (var i = 0; i < configs.Count; i++)
        {
            if (configs[i].ChapterId == chapterId && IsPlayable(GetState(states, configs[i].PackId)))
            {
                count++;
            }
        }

        return count;
    }

    private static bool TryFindConfig(
        IReadOnlyList<CardPackConfigData> configs,
        int packId,
        out CardPackConfigData config)
    {
        for (var i = 0; i < configs.Count; i++)
        {
            if (configs[i].PackId == packId)
            {
                config = configs[i];
                return true;
            }
        }

        config = default;
        return false;
    }

    private static CardPackLifecycleState GetState(
        Dictionary<int, CardPackLifecycleState> states,
        int packId)
    {
        return states.TryGetValue(packId, out var state)
            ? state
            : CardPackLifecycleState.Locked;
    }

    private static bool IsPlayable(CardPackLifecycleState state)
    {
        return state == CardPackLifecycleState.Unlocked
            || state == CardPackLifecycleState.InProgress;
    }
}
