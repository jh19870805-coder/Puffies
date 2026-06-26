using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 任务类型（与 TaskConfig.csv 的 TaskType 列数值对应）。
/// </summary>
public enum TaskType
{
    None = 0,
    CollectPuzzle = 1, // 收集拼图
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
}

/// <summary>
/// 用途：读取 TaskConfig.csv 并管理当前任务进度（本地 JSON）。返回：按方法说明。
/// </summary>
public static class GameTaskUtility
{
    private static readonly List<TaskConfigData> sTaskConfigs = new List<TaskConfigData>();
    private static int sCurrentTaskId = GameDefine.DefaultTaskId;
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

        if (!TryLoadTaskConfigCsv())
        {
            return false;
        }

        if (!TryLoadOrCreateTaskProgress())
        {
            return false;
        }

        sIsInitialized = true;
        Debug.Log($"GameTaskUtility initialized. tasks={sTaskConfigs.Count}, currentTaskId={sCurrentTaskId}");
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
    /// 用途：设置当前任务 Id 并写入本地 JSON。返回：是否保存成功。
    /// </summary>
    public static bool SetCurrentTaskId(int taskId)
    {
        EnsureInitialized();
        if (taskId <= 0)
        {
            Debug.LogWarning($"GameTaskUtility.SetCurrentTaskId skipped, invalid taskId={taskId}");
            return false;
        }

        sCurrentTaskId = taskId;
        return SaveTaskProgress();
    }

    /// <summary>
    /// 用途：按 TaskId 查找任务配置。返回：是否找到。
    /// </summary>
    public static bool TryGetTaskConfig(int taskId, out TaskConfigData taskConfig)
    {
        EnsureInitialized();
        for (var i = 0; i < sTaskConfigs.Count; i++)
        {
            if (sTaskConfigs[i].TaskId == taskId)
            {
                taskConfig = sTaskConfigs[i];
                return true;
            }
        }

        taskConfig = default;
        return false;
    }

    /// <summary>
    /// 用途：获取全部任务配置只读列表。返回：任务配置列表。
    /// </summary>
    public static IReadOnlyList<TaskConfigData> GetAllTaskConfigs()
    {
        EnsureInitialized();
        return sTaskConfigs;
    }

    private static void EnsureInitialized()
    {
        if (!sIsInitialized)
        {
            Initialize();
        }
    }

    private static bool TryLoadTaskConfigCsv()
    {
        sTaskConfigs.Clear();
        var csvText = LoadTaskConfigText();
        if (string.IsNullOrWhiteSpace(csvText))
        {
            Debug.LogError("GameTaskUtility failed to load TaskConfig.csv.");
            return false;
        }

        var lines = csvText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("GameTaskUtility: TaskConfig.csv has no data rows.");
            return true;
        }

        for (var i = 1; i < lines.Length; i++)
        {
            if (!TryParseTaskConfigLine(lines[i], out var taskConfig))
            {
                Debug.LogWarning($"GameTaskUtility skipped invalid row: {lines[i]}");
                continue;
            }

            sTaskConfigs.Add(taskConfig);
        }

        return true;
    }

    private static string LoadTaskConfigText()
    {
        var resourcesAsset = Resources.Load<TextAsset>(GameDefine.TaskConfigResourcesPath);
        if (resourcesAsset != null && !string.IsNullOrWhiteSpace(resourcesAsset.text))
        {
            return resourcesAsset.text;
        }

        var diskPath = GameCommonUtility.ToDiskPath(GameDefine.TaskConfigEditorPath);
        if (File.Exists(diskPath))
        {
            return File.ReadAllText(diskPath);
        }

        return null;
    }

    private static bool TryParseTaskConfigLine(string line, out TaskConfigData taskConfig)
    {
        taskConfig = default;
        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var columns = line.Split(',');
        if (columns.Length < 7)
        {
            return false;
        }

        if (!int.TryParse(columns[0].Trim(), out var index)
            || !int.TryParse(columns[1].Trim(), out var taskId))
        {
            return false;
        }

        taskConfig = new TaskConfigData
        {
            Index = index,
            TaskId = taskId,
            TaskType = (TaskType)TryParseInt(columns[2]),
            CompleteValue = TryParseInt(columns[3]),
            RewardType = (RewardType)TryParseInt(columns[4]),
            RewardId = TryParseInt(columns[5]),
            RewardValue = TryParseInt(columns[6])
        };
        return taskId > 0;
    }

    private static int TryParseInt(string text)
    {
        return int.TryParse((text ?? string.Empty).Trim(), out var value) ? value : 0;
    }

    private static bool TryLoadOrCreateTaskProgress()
    {
        if (JsonLocalStore.TryRead(GameDefine.TaskProgressJsonKey, out TaskProgressData progress)
            && progress.CurrentTaskId > 0)
        {
            sCurrentTaskId = progress.CurrentTaskId;
            return true;
        }

        sCurrentTaskId = GameDefine.DefaultTaskId;
        return SaveTaskProgress();
    }

    private static bool SaveTaskProgress()
    {
        var progress = new TaskProgressData
        {
            CurrentTaskId = sCurrentTaskId
        };
        return JsonLocalStore.Upsert(GameDefine.TaskProgressJsonKey, progress);
    }
}
