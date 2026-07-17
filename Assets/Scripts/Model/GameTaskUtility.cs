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
public struct TaskConfigData
{
    public int Index;
    public int TaskId;
    public TaskType TaskType;
    public int CompleteValue;
    public RewardType RewardType;
    public int RewardId;
    public int RewardValue;
}

[Serializable]
public struct TaskProgressData
{
    public int CurrentTaskId;
    public int CurrentCompleteValue; // 默认 0，对应当前任务完成进度
}

/// <summary>
/// 用途：读取 TaskConfig.csv 并管理当前任务进度（本地 JSON）。返回：按方法说明。
/// </summary>
public static class GameTaskUtility
{
    private static int sCurrentTaskId = GameDefine.DefaultTaskId;
    private static int sCurrentCompleteValue = 0;
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

        if (!TryLoadOrCreateTaskProgress())
        {
            return false;
        }

        if (!GameConfigRepository.TryGetTaskConfigs(out var taskConfigs))
        {
            Debug.LogError("GameTaskUtility.Initialize failed: task config is not ready.");
            return false;
        }

        sIsInitialized = true;
        Debug.Log(
            $"GameTaskUtility initialized. tasks={taskConfigs.Count}, " +
            $"currentTaskId={sCurrentTaskId}, currentCompleteValue={sCurrentCompleteValue}");
        return true;
    }

    public static bool IsInitialized => sIsInitialized;

    /// <summary>
    /// 用途：获取当前任务 Id。返回：当前任务 Id。
    /// </summary>
    public static int GetCurrentTaskId()
    {
        EnsureInitialized();
        return sCurrentTaskId;
    }

    /// <summary>
    /// 用途：设置当前任务 Id 并写入本地 JSON；切换任务 Id 时将完成进度重置为 0。返回：是否保存成功。
    /// </summary>
    public static bool SetCurrentTaskId(int taskId)
    {
        EnsureInitialized();
        if (taskId <= 0)
        {
            Debug.LogWarning($"GameTaskUtility.SetCurrentTaskId skipped, invalid taskId={taskId}");
            return false;
        }

        if (taskId == sCurrentTaskId)
        {
            return true;
        }

        sCurrentTaskId = taskId;
        ResetCurrentCompleteValue();
        return SaveTaskProgress();
    }

    /// <summary>
    /// 用途：当前任务完成后切换到下一任务 Id；连续累计得分任务结转超额分数。返回：是否成功。
    /// </summary>
    public static bool TryCompleteAndSetNextTaskId(int nextTaskId)
    {
        EnsureInitialized();
        if (!TryGetCurrentTaskConfig(out var currentTaskConfig)
            || currentTaskConfig.CompleteValue <= 0
            || sCurrentCompleteValue < currentTaskConfig.CompleteValue)
        {
            return false;
        }

        if (nextTaskId <= 0)
        {
            Debug.LogWarning($"GameTaskUtility.TryCompleteAndSetNextTaskId skipped, invalid nextTaskId={nextTaskId}");
            return false;
        }

        var carryOverValue = 0;
        if (currentTaskConfig.TaskType == TaskType.AccumulateScore
            && TryGetTaskConfig(nextTaskId, out var nextTaskConfig)
            && nextTaskConfig.TaskType == TaskType.AccumulateScore)
        {
            carryOverValue = Math.Max(0, sCurrentCompleteValue - currentTaskConfig.CompleteValue);
        }

        sCurrentTaskId = nextTaskId;
        sCurrentCompleteValue = carryOverValue;
        Debug.Log(
            $"GameTaskUtility: task advanced. nextTaskId={nextTaskId}, " +
            $"carryOverValue={carryOverValue}");
        return SaveTaskProgress();
    }

    /// <summary>
    /// 用途：获取当前任务已完成进度值。返回：本地存储的完成值。
    /// </summary>
    public static int GetCurrentCompleteValue()
    {
        EnsureInitialized();
        return sCurrentCompleteValue;
    }

    /// <summary>
    /// 用途：增加当前任务累计获得的分数并写入本地 JSON。返回：是否保存成功。
    /// </summary>
    public static bool AddCurrentScore(int score)
    {
        EnsureInitialized();
        if (score <= 0)
        {
            Debug.LogWarning($"GameTaskUtility.AddCurrentScore skipped, invalid score={score}");
            return false;
        }

        return SetCurrentCompleteValue(sCurrentCompleteValue + score);
    }

    /// <summary>
    /// 用途：当前任务完成后将任务 Id 自动 +1，并按任务类型结转超额进度。返回：是否成功。
    /// </summary>
    public static bool TryCompleteAndAdvanceTask()
    {
        EnsureInitialized();
        return TryCompleteAndSetNextTaskId(sCurrentTaskId + 1);
    }

    /// <summary>
    /// 用途：判断当前任务是否为累计获得分数类型。返回：是否为 AccumulateScore。
    /// </summary>
    public static bool IsCurrentTaskAccumulateScore()
    {
        EnsureInitialized();
        return TryGetCurrentTaskConfig(out var taskConfig) && taskConfig.TaskType == TaskType.AccumulateScore;
    }

    /// <summary>
    /// 用途：设置当前任务已完成进度值并写入本地 JSON。返回：是否保存成功。
    /// </summary>
    public static bool SetCurrentCompleteValue(int completeValue)
    {
        EnsureInitialized();
        if (completeValue < 0)
        {
            Debug.LogWarning($"GameTaskUtility.SetCurrentCompleteValue skipped, invalid value={completeValue}");
            return false;
        }

        sCurrentCompleteValue = completeValue;
        return SaveTaskProgress();
    }

    /// <summary>
    /// 用途：判断当前任务是否已完成（CurrentCompleteValue >= CompleteValue）。返回：是否完成。
    /// </summary>
    public static bool IsCurrentTaskCompleted()
    {
        EnsureInitialized();
        if (!TryGetTaskConfig(sCurrentTaskId, out var taskConfig))
        {
            return false;
        }

        if (taskConfig.CompleteValue <= 0)
        {
            return false;
        }

        return sCurrentCompleteValue >= taskConfig.CompleteValue;
    }

    /// <summary>
    /// 用途：获取当前任务配置。返回：是否找到当前任务。
    /// </summary>
    public static bool TryGetCurrentTaskConfig(out TaskConfigData taskConfig)
    {
        EnsureInitialized();
        return TryGetTaskConfig(sCurrentTaskId, out taskConfig);
    }

    /// <summary>
    /// 用途：按 TaskId 查找任务配置。返回：是否找到。
    /// </summary>
    public static bool TryGetTaskConfig(int taskId, out TaskConfigData taskConfig)
    {
        EnsureInitialized();
        return GameConfigRepository.TryGetTaskConfig(taskId, out taskConfig);
    }

    /// <summary>
    /// 用途：获取全部任务配置只读列表。返回：任务配置列表。
    /// </summary>
    public static IReadOnlyList<TaskConfigData> GetAllTaskConfigs()
    {
        EnsureInitialized();
        return GameConfigRepository.GetTaskConfigs();
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
            && progress.CurrentTaskId > 0)
        {
            sCurrentTaskId = progress.CurrentTaskId;
            sCurrentCompleteValue = progress.CurrentCompleteValue < 0 ? 0 : progress.CurrentCompleteValue;
            return true;
        }

        sCurrentTaskId = GameDefine.DefaultTaskId;
        ResetCurrentCompleteValue();
        return SaveTaskProgress();
    }

    private static void ResetCurrentCompleteValue()
    {
        sCurrentCompleteValue = 0;
    }

    private static bool SaveTaskProgress()
    {
        var progress = new TaskProgressData
        {
            CurrentTaskId = sCurrentTaskId,
            CurrentCompleteValue = sCurrentCompleteValue
        };
        return JsonLocalStore.SaveRoot(progress);
    }
}
