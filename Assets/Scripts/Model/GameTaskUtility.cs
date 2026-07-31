using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 任务类型（与 TaskConfig.csv 的 TaskType 列数值对应）。
/// </summary>
public enum TaskType
{
    None = 0,
    AccumulateScore = 1, // 累计获得分数
    CollectStickers = 2, // 累计收集贴纸
    CompleteCardPacks = 3, // 累计完成卡包
}

/// <summary>
/// 任务模板的尺寸筛选方式。
/// </summary>
public enum TaskSizeMode
{
    Any = 0,
    Specific = 1,
}

/// <summary>
/// 奖励类型（与 TaskConfig.csv 的 RewardType 列数值对应）。
/// </summary>
public enum RewardType
{
    None = 0,
    CardPack = 1, // 卡包
}

[Serializable]
public struct TaskTemplateConfigData
{
    public int Index;
    public int TemplateId;
    public bool Enabled;
    public TaskType TaskType;
    public TaskSizeMode SizeMode;
    public int[] SizePool;
    public int[] TargetPool;
    public int Weight;
    public int MinChapter;
    public int MaxChapter;
    public bool CountReplay;
    public RewardType RewardType;
    public int RewardId;
    public int RewardValue;
}

[Serializable]
public struct TaskInstanceData
{
    public int TaskInstanceId;
    public int TemplateId;
    public TaskType TaskType;
    public CardPackSize RequiredPackSize;
    public int CompleteValue;
    public bool CountReplay;
    public RewardType RewardType;
    public int RewardId;
    public int RewardValue;
}

[Serializable]
public struct TaskProgressData
{
    public TaskInstanceData CurrentTask;
    public int CurrentCompleteValue;
    public int NextTaskInstanceId;
    public int LastTemplateId;
    public int ScoreTargetCycleIndex;
}

/// <summary>
/// 用途：读取 TaskConfig.csv 并管理当前任务进度（本地 JSON）。返回：按方法说明。
/// </summary>
public static class GameTaskUtility
{
    private static TaskProgressData sProgress;
    private static bool sIsInitialized;

    /// <summary>
    /// 用途：加载任务配置并读取/初始化本地当前任务 Id。返回：是否初始化成功。
    /// </summary>
    public static bool Initialize()
    {
        if (sIsInitialized)
        {
            return true;
        }

        if (!JsonLocalStore.IsInitialized && !JsonLocalStore.Initialize())
        {
            Debug.LogError("GameTaskUtility.Initialize failed: JsonLocalStore is not ready.");
            return false;
        }

        if (!GameConfigRepository.TryGetTaskTemplates(out var taskTemplates)
            || taskTemplates.Count == 0)
        {
            Debug.LogError("GameTaskUtility.Initialize failed: task config is not ready.");
            return false;
        }

        if (!TryLoadOrCreateTaskProgress())
        {
            return false;
        }

        sIsInitialized = true;
        Debug.Log(
            $"GameTaskUtility initialized. templates={taskTemplates.Count}, " +
            $"taskInstanceId={sProgress.CurrentTask.TaskInstanceId}, " +
            $"templateId={sProgress.CurrentTask.TemplateId}, " +
            $"taskType={sProgress.CurrentTask.TaskType}, " +
            $"requiredPackSize={sProgress.CurrentTask.RequiredPackSize}, " +
            $"target={sProgress.CurrentTask.CompleteValue}, " +
            $"progress={sProgress.CurrentCompleteValue}");
        return true;
    }

    public static bool IsInitialized => sIsInitialized;

    public static int GetCurrentTaskInstanceId()
    {
        EnsureInitialized();
        return sProgress.CurrentTask.TaskInstanceId;
    }

    /// <summary>
    /// 用途：当前任务完成后随机生成下一任务；连续且匹配同一卡包的积分任务结转超额分数。返回：是否成功。
    /// </summary>
    public static bool TryCompleteAndAdvanceTask(int completedPackId, bool isReplay)
    {
        EnsureInitialized();
        var currentTask = sProgress.CurrentTask;
        if (currentTask.CompleteValue <= 0
            || sProgress.CurrentCompleteValue < currentTask.CompleteValue
            || !GameConfigRepository.TryGetCardPackConfig(completedPackId, out var completedPackConfig))
        {
            return false;
        }

        var previousProgress = sProgress;
        var overflow = Math.Max(0, sProgress.CurrentCompleteValue - currentTask.CompleteValue);
        if (!TryCreateTaskInstance(out var nextTask))
        {
            return false;
        }

        var carryOverValue = 0;
        if (currentTask.TaskType == TaskType.AccumulateScore
            && nextTask.TaskType == TaskType.AccumulateScore
            && IsPackEligible(nextTask, completedPackConfig.PackSize, isReplay))
        {
            carryOverValue = overflow;
        }

        sProgress.CurrentTask = nextTask;
        sProgress.CurrentCompleteValue = carryOverValue;
        Debug.Log(
            $"GameTaskUtility: task advanced. taskInstanceId={nextTask.TaskInstanceId}, " +
            $"templateId={nextTask.TemplateId}, taskType={nextTask.TaskType}, " +
            $"requiredPackSize={nextTask.RequiredPackSize}, target={nextTask.CompleteValue}, " +
            $"carryOverValue={carryOverValue}");
        if (SaveTaskProgress())
        {
            return true;
        }

        sProgress = previousProgress;
        return false;
    }

    /// <summary>
    /// 用途：获取当前任务已完成进度值。返回：本地存储的完成值。
    /// </summary>
    public static int GetCurrentCompleteValue()
    {
        EnsureInitialized();
        return sProgress.CurrentCompleteValue;
    }

    /// <summary>
    /// 用途：按已完成卡包计算并写入当前任务进度。返回：任务进度是否处理成功。
    /// </summary>
    public static bool ApplyCompletedPack(
        int packId,
        int stickerCount,
        int score,
        bool isReplay,
        out int contribution)
    {
        EnsureInitialized();
        contribution = 0;
        if (!GameConfigRepository.TryGetCardPackConfig(packId, out var packConfig))
        {
            return false;
        }

        var task = sProgress.CurrentTask;
        if (!IsPackEligible(task, packConfig.PackSize, isReplay))
        {
            return true;
        }

        switch (task.TaskType)
        {
            case TaskType.AccumulateScore:
                contribution = Math.Max(0, score);
                break;
            case TaskType.CollectStickers:
                contribution = Math.Max(0, stickerCount);
                break;
            case TaskType.CompleteCardPacks:
                contribution = 1;
                break;
        }

        if (contribution <= 0)
        {
            return true;
        }

        var previousValue = sProgress.CurrentCompleteValue;
        sProgress.CurrentCompleteValue = (int)Math.Min(
            int.MaxValue,
            (long)sProgress.CurrentCompleteValue + contribution);
        if (SaveTaskProgress())
        {
            return true;
        }

        sProgress.CurrentCompleteValue = previousValue;
        return false;
    }

    /// <summary>
    /// 用途：判断当前任务是否已完成（CurrentCompleteValue >= CompleteValue）。返回：是否完成。
    /// </summary>
    public static bool IsCurrentTaskCompleted()
    {
        EnsureInitialized();
        return sProgress.CurrentTask.CompleteValue > 0
            && sProgress.CurrentCompleteValue >= sProgress.CurrentTask.CompleteValue;
    }

    /// <summary>
    /// 用途：获取当前随机任务实例。返回：任务实例是否有效。
    /// </summary>
    public static bool TryGetCurrentTask(out TaskInstanceData task)
    {
        EnsureInitialized();
        task = sProgress.CurrentTask;
        return IsTaskInstanceValid(task);
    }

    /// <summary>
    /// 用途：获取全部任务模板只读列表。返回：任务模板列表。
    /// </summary>
    public static IReadOnlyList<TaskTemplateConfigData> GetAllTaskTemplates()
    {
        EnsureInitialized();
        return GameConfigRepository.GetTaskTemplates();
    }

    private static void EnsureInitialized()
    {
        if (!sIsInitialized)
        {
            Initialize();
        }
    }

    private static bool TryLoadOrCreateTaskProgress()
    {
        if (JsonLocalStore.TryReadRoot(out TaskProgressData progress)
            && IsTaskInstanceValid(progress.CurrentTask))
        {
            sProgress = progress;
            sProgress.CurrentCompleteValue = Math.Max(0, sProgress.CurrentCompleteValue);
            sProgress.NextTaskInstanceId = Math.Max(
                sProgress.CurrentTask.TaskInstanceId + 1,
                sProgress.NextTaskInstanceId);
            sProgress.ScoreTargetCycleIndex = Math.Max(0, sProgress.ScoreTargetCycleIndex);
            return true;
        }

        sProgress = new TaskProgressData
        {
            NextTaskInstanceId = 1
        };
        if (!TryCreateTaskInstance(out var firstTask))
        {
            Debug.LogError("GameTaskUtility: cannot create the initial task from TaskConfig.csv.");
            return false;
        }

        sProgress.CurrentTask = firstTask;
        sProgress.CurrentCompleteValue = 0;
        return SaveTaskProgress();
    }

    private static bool TryCreateTaskInstance(out TaskInstanceData task)
    {
        task = default;
        if (!GameConfigRepository.TryGetTaskTemplates(out var templates))
        {
            return false;
        }

        var chapterId = ResolveCurrentChapter();
        var candidates = new List<TaskTemplateConfigData>();
        for (var i = 0; i < templates.Count; i++)
        {
            if (IsTemplateEligible(templates[i], chapterId))
            {
                candidates.Add(templates[i]);
            }
        }

        if (candidates.Count > 1)
        {
            candidates.RemoveAll(item => item.TemplateId == sProgress.LastTemplateId);
        }

        if (!TryChooseWeightedTemplate(candidates, out var template))
        {
            return false;
        }

        var requiredPackSize = CardPackSize.None;
        if (template.SizeMode == TaskSizeMode.Specific
            && !TryChooseEligiblePackSize(template, out requiredPackSize))
        {
            return false;
        }

        var completeValue = ChooseTargetValue(template);
        if (completeValue <= 0)
        {
            return false;
        }

        var instanceId = Math.Max(1, sProgress.NextTaskInstanceId);
        sProgress.NextTaskInstanceId = instanceId == int.MaxValue ? 1 : instanceId + 1;
        sProgress.LastTemplateId = template.TemplateId;
        task = new TaskInstanceData
        {
            TaskInstanceId = instanceId,
            TemplateId = template.TemplateId,
            TaskType = template.TaskType,
            RequiredPackSize = requiredPackSize,
            CompleteValue = completeValue,
            CountReplay = template.CountReplay,
            RewardType = template.RewardType,
            RewardId = template.RewardId,
            RewardValue = template.RewardValue
        };
        return true;
    }

    private static bool IsTemplateEligible(TaskTemplateConfigData template, int chapterId)
    {
        if (!template.Enabled
            || template.TemplateId <= 0
            || template.TaskType < TaskType.AccumulateScore
            || template.TaskType > TaskType.CompleteCardPacks
            || template.Weight <= 0
            || template.TargetPool == null
            || template.TargetPool.Length == 0
            || chapterId < template.MinChapter
            || chapterId > template.MaxChapter)
        {
            return false;
        }

        return template.SizeMode == TaskSizeMode.Any
            || TryGetEligiblePackSizes(template, out var sizes) && sizes.Count > 0;
    }

    private static bool TryChooseWeightedTemplate(
        List<TaskTemplateConfigData> candidates,
        out TaskTemplateConfigData template)
    {
        template = default;
        var totalWeight = 0;
        for (var i = 0; i < candidates.Count; i++)
        {
            totalWeight += Math.Max(1, candidates[i].Weight);
        }

        if (totalWeight <= 0)
        {
            return false;
        }

        var roll = UnityEngine.Random.Range(0, totalWeight);
        for (var i = 0; i < candidates.Count; i++)
        {
            roll -= Math.Max(1, candidates[i].Weight);
            if (roll < 0)
            {
                template = candidates[i];
                return true;
            }
        }

        return false;
    }

    private static int ChooseTargetValue(TaskTemplateConfigData template)
    {
        if (template.TargetPool == null || template.TargetPool.Length == 0)
        {
            return 0;
        }

        if (template.TaskType == TaskType.AccumulateScore)
        {
            var index = sProgress.ScoreTargetCycleIndex % template.TargetPool.Length;
            sProgress.ScoreTargetCycleIndex++;
            return template.TargetPool[index];
        }

        return template.TargetPool[UnityEngine.Random.Range(0, template.TargetPool.Length)];
    }

    private static bool TryChooseEligiblePackSize(
        TaskTemplateConfigData template,
        out CardPackSize packSize)
    {
        packSize = CardPackSize.None;
        if (!TryGetEligiblePackSizes(template, out var eligibleSizes) || eligibleSizes.Count == 0)
        {
            return false;
        }

        packSize = eligibleSizes[UnityEngine.Random.Range(0, eligibleSizes.Count)];
        return true;
    }

    private static bool TryGetEligiblePackSizes(
        TaskTemplateConfigData template,
        out List<CardPackSize> eligibleSizes)
    {
        eligibleSizes = new List<CardPackSize>();
        if (template.SizePool == null || template.SizePool.Length == 0
            || !CardPackDataUtility.Initialize())
        {
            return false;
        }

        var availableSizes = new HashSet<CardPackSize>();
        var records = CardPackDataUtility.GetAllPacks();
        for (var i = 0; i < records.Count; i++)
        {
            var record = records[i];
            if (record.LifecycleState == CardPackLifecycleState.Locked
                || !template.CountReplay && record.LifecycleState == CardPackLifecycleState.Completed)
            {
                continue;
            }

            availableSizes.Add(record.PackSize);
        }

        for (var i = 0; i < template.SizePool.Length; i++)
        {
            var size = (CardPackSize)template.SizePool[i];
            if (size >= CardPackSize.XS
                && size <= CardPackSize.XXXL
                && availableSizes.Contains(size)
                && !eligibleSizes.Contains(size))
            {
                eligibleSizes.Add(size);
            }
        }

        return eligibleSizes.Count > 0;
    }

    private static int ResolveCurrentChapter()
    {
        if (!CardPackDataUtility.Initialize())
        {
            return 1;
        }

        var chapterId = 1;
        var records = CardPackDataUtility.GetAllPacks();
        for (var i = 0; i < records.Count; i++)
        {
            if (records[i].LifecycleState != CardPackLifecycleState.Locked
                && GameConfigRepository.TryGetCardPackConfig(records[i].PackId, out var config))
            {
                chapterId = Math.Max(chapterId, config.ChapterId);
            }
        }

        return chapterId;
    }

    private static bool IsPackEligible(
        TaskInstanceData task,
        CardPackSize packSize,
        bool isReplay)
    {
        return (!isReplay || task.CountReplay)
            && (task.RequiredPackSize == CardPackSize.None || task.RequiredPackSize == packSize);
    }

    private static bool IsTaskInstanceValid(TaskInstanceData task)
    {
        return task.TaskInstanceId > 0
            && task.TemplateId > 0
            && task.TaskType >= TaskType.AccumulateScore
            && task.TaskType <= TaskType.CompleteCardPacks
            && task.CompleteValue > 0;
    }

    private static bool SaveTaskProgress()
    {
        return JsonLocalStore.SaveRoot(sProgress);
    }
}

public struct GameScoreContext
{
    public bool WasHintUsed;
    public bool IsLevelOutlineEnabled;
    public bool IsStickerOutlineEnabled;
    public float CompletionTimeSeconds;
}

public struct GameScoreResult
{
    public int BaseScore;
    public int NoHintBonusPercent;
    public int LevelOutlineDisabledBonusPercent;
    public int StickerOutlineDisabledBonusPercent;
    public int CompletionTimeBonusPercent;
    public int TotalBonusPercent;
    public float CompletionTimeSeconds;
    public int FinalScore;
}

public static class GameScoreUtility
{
    public const float TimeThresholdASeconds = 15f;
    public const float TimeThresholdBSeconds = 30f;
    public const float TimeThresholdCSeconds = 60f;

    private const int NoHintBonusPercent = 5;
    private const int LevelOutlineDisabledBonusPercent = 2;
    private const int StickerOutlineDisabledBonusPercent = 5;

    public static int GetBaseScore(CardPackSize packSize)
    {
        switch (packSize)
        {
            case CardPackSize.XS:
                return 60;
            case CardPackSize.S:
                return 80;
            case CardPackSize.M:
                return 100;
            case CardPackSize.L:
                return 120;
            case CardPackSize.XL:
                return 140;
            case CardPackSize.XXL:
                return 160;
            case CardPackSize.XXXL:
                return 200;
            default:
                return 0;
        }
    }

    public static bool TryCalculateCardPackScore(
        int packId,
        GameScoreContext context,
        out GameScoreResult result)
    {
        result = default;
        if (!CardPackDataUtility.TryGetPackConfig(packId, out var packSize))
        {
            return false;
        }

        var baseScore = GetBaseScore(packSize);
        if (baseScore <= 0)
        {
            return false;
        }

        var completionTimeSeconds = SanitizeCompletionTime(context.CompletionTimeSeconds);
        var noHintBonus = context.WasHintUsed ? 0 : NoHintBonusPercent;
        var levelOutlineBonus = context.IsLevelOutlineEnabled
            ? 0
            : LevelOutlineDisabledBonusPercent;
        var stickerOutlineBonus = context.IsStickerOutlineEnabled
            ? 0
            : StickerOutlineDisabledBonusPercent;
        var completionTimeBonus = GetCompletionTimeBonusPercent(completionTimeSeconds);
        var totalBonus = noHintBonus
            + levelOutlineBonus
            + stickerOutlineBonus
            + completionTimeBonus;
        var scaledScore = baseScore * (100 + totalBonus);

        result = new GameScoreResult
        {
            BaseScore = baseScore,
            NoHintBonusPercent = noHintBonus,
            LevelOutlineDisabledBonusPercent = levelOutlineBonus,
            StickerOutlineDisabledBonusPercent = stickerOutlineBonus,
            CompletionTimeBonusPercent = completionTimeBonus,
            TotalBonusPercent = totalBonus,
            CompletionTimeSeconds = completionTimeSeconds,
            FinalScore = (scaledScore + 99) / 100
        };
        return true;
    }

    private static int GetCompletionTimeBonusPercent(float completionTimeSeconds)
    {
        if (completionTimeSeconds <= TimeThresholdASeconds)
        {
            return 3;
        }

        if (completionTimeSeconds <= TimeThresholdBSeconds)
        {
            return 2;
        }

        return completionTimeSeconds <= TimeThresholdCSeconds ? 1 : 0;
    }

    private static float SanitizeCompletionTime(float completionTimeSeconds)
    {
        if (float.IsNaN(completionTimeSeconds)
            || float.IsInfinity(completionTimeSeconds)
            || completionTimeSeconds < 0f)
        {
            return 0f;
        }

        return completionTimeSeconds;
    }
}
