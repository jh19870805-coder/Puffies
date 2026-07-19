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
            $@"SELECT PackId, PackSize, LifecycleState, IsUnlocked, UnlockTime, IsPlayed
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
            $@"SELECT PackId, PackSize, LifecycleState, IsUnlocked, UnlockTime, IsPlayed
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
                UnlockTime = string.Empty
            };
        }

        if (record.LifecycleState == CardPackLifecycleState.Locked)
        {
            record.LifecycleState = CardPackLifecycleState.Unlocked;
            record.UnlockTime = FormatUnlockTime(DateTime.Now);
        }

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
        record.LifecycleState = CardPackLifecycleState.Unlocked;
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
                UnlockTime = FormatUnlockTime(DateTime.Now)
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
                UnlockTime = string.Empty
            };
        }
        else
        {
            record.PackSize = packSize;
        }

        record.LifecycleState = CardPackLifecycleState.Completed;
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

        EnsureValidLifecycleState(ref record);
        EnsureUnlockTime(ref record);
        var unlockTime = record.UnlockTime ?? string.Empty;
        var isUnlocked = record.LifecycleState != CardPackLifecycleState.Locked;
        var isCompleted = record.LifecycleState == CardPackLifecycleState.Completed;
        var affected = SqliteLocalStore.ExecuteNonQuery(
            $@"INSERT INTO {GameDefine.LocalSqliteCardPackTable}
               (PackId, PackSize, LifecycleState, IsUnlocked, UnlockTime, IsPlayed)
               VALUES (?, ?, ?, ?, ?, ?)
               ON CONFLICT(PackId) DO UPDATE SET
                PackSize = excluded.PackSize,
                LifecycleState = excluded.LifecycleState,
                IsUnlocked = excluded.IsUnlocked,
                UnlockTime = excluded.UnlockTime,
                IsPlayed = excluded.IsPlayed",
            record.PackId,
            (int)record.PackSize,
            (int)record.LifecycleState,
            isUnlocked ? 1 : 0,
            unlockTime,
            isCompleted ? 1 : 0);
        return affected > 0;
    }

    private static CardPackRecord ToRecord(CardPackTableRow row)
    {
        return new CardPackRecord
        {
            PackId = row.PackId,
            PackSize = (CardPackSize)row.PackSize,
            LifecycleState = ResolveLifecycleState(row),
            UnlockTime = row.UnlockTime ?? string.Empty
        };
    }

    private static bool HasUnlockTime(string unlockTime)
    {
        return !string.IsNullOrWhiteSpace(unlockTime)
            && TryParseUnlockTime(unlockTime, out _);
    }

    private static CardPackLifecycleState ResolveLifecycleState(CardPackTableRow row)
    {
        if (Enum.IsDefined(typeof(CardPackLifecycleState), row.LifecycleState))
        {
            var state = (CardPackLifecycleState)row.LifecycleState;
            if (state != CardPackLifecycleState.Locked
                || (row.IsUnlocked == 0 && row.IsPlayed == 0))
            {
                return state;
            }
        }

        if (row.IsPlayed != 0)
        {
            return CardPackLifecycleState.Completed;
        }

        return row.IsUnlocked != 0
            ? CardPackLifecycleState.Unlocked
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
        if (record.LifecycleState == CardPackLifecycleState.Locked
            || HasUnlockTime(record.UnlockTime))
        {
            return;
        }

        record.UnlockTime = FormatUnlockTime(DateTime.Now);
    }

    private static void TryNormalizeAndPersistUnlockTime(ref CardPackRecord record)
    {
        var unlockTimeBefore = record.UnlockTime;
        var lifecycleStateBefore = record.LifecycleState;
        EnsureValidLifecycleState(ref record);
        EnsureUnlockTime(ref record);
        if (record.UnlockTime != unlockTimeBefore || record.LifecycleState != lifecycleStateBefore)
        {
            UpsertPackInternal(record);
        }
    }

    private sealed class CardPackTableRow
    {
        public int PackId { get; set; }
        public int PackSize { get; set; }
        public int LifecycleState { get; set; }
        public int IsUnlocked { get; set; }
        public string UnlockTime { get; set; }
        public int IsPlayed { get; set; }
    }

    private sealed class CardPackIdRow
    {
        public int PackId { get; set; }
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
        int targetMin,
        int targetMax,
        float probability,
        float roll)
    {
        Stage = stage;
        RemainingLockedCount = remainingLockedCount;
        HeldPlayableCount = heldPlayableCount;
        TargetMin = targetMin;
        TargetMax = targetMax;
        Probability = probability;
        Roll = roll;
    }

    public CardPackChapterStage Stage { get; }
    public int RemainingLockedCount { get; }
    public int HeldPlayableCount { get; }
    public int TargetMin { get; }
    public int TargetMax { get; }
    public float Probability { get; }
    public float Roll { get; }
    public bool ShouldGrant => Probability >= 1f || (Probability > 0f && Roll < Probability);
}

public static class CardPackDistributionUtility
{
    public const float WithinTargetRewardProbability = 0.5f;

    public static bool TryGrantTaskReward(int preferredPackId, out int grantedPackId)
    {
        grantedPackId = 0;
        if (!TryBuildState(out var configs, out var states))
        {
            return false;
        }

        var chapterId = ResolveTaskRewardChapter(configs, states);
        if (chapterId <= 0)
        {
            return false;
        }

        var candidate = FindLockedCandidate(configs, states, chapterId, preferredPackId);
        if (candidate.PackId <= 0 || !CardPackDataUtility.TryUnlockPack(candidate.PackId))
        {
            return false;
        }

        grantedPackId = candidate.PackId;
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
        var roll = UnityEngine.Random.value;
        decision = EvaluateFirstCompletionReward(remainingLockedCount, heldPlayableCount, roll);
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

    public static CardPackGrantDecision EvaluateFirstCompletionReward(
        int remainingLockedCount,
        int heldPlayableCount,
        float roll)
    {
        var stage = ResolveStage(remainingLockedCount, out var targetMin, out var targetMax);
        var probability = 0f;
        if (stage != CardPackChapterStage.None)
        {
            if (heldPlayableCount < targetMin)
            {
                probability = 1f;
            }
            else if (heldPlayableCount < targetMax)
            {
                probability = WithinTargetRewardProbability;
            }
        }

        return new CardPackGrantDecision(
            stage,
            Mathf.Max(0, remainingLockedCount),
            Mathf.Max(0, heldPlayableCount),
            targetMin,
            targetMax,
            probability,
            Mathf.Clamp01(roll));
    }

    private static CardPackChapterStage ResolveStage(
        int remainingLockedCount,
        out int targetMin,
        out int targetMax)
    {
        if (remainingLockedCount >= 9)
        {
            targetMin = 5;
            targetMax = 6;
            return CardPackChapterStage.Initial;
        }

        if (remainingLockedCount >= 3)
        {
            targetMin = 2;
            targetMax = 3;
            return CardPackChapterStage.MidToLate;
        }

        if (remainingLockedCount >= 1)
        {
            targetMin = 1;
            targetMax = 1;
            return CardPackChapterStage.Final;
        }

        targetMin = 0;
        targetMax = 0;
        return CardPackChapterStage.None;
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
