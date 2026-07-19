using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public struct CardPackConfigData
{
    public int Index;
    public int PackId;
    public CardPackSize PackSize;
    public int ChapterId;
}

public readonly struct GameConfigAssetDefinition
{
    public GameConfigAssetDefinition(string displayName, string resourcesPath, string editorPath)
    {
        DisplayName = displayName;
        ResourcesPath = resourcesPath;
        EditorPath = editorPath;
    }

    public string DisplayName { get; }
    public string ResourcesPath { get; }
    public string EditorPath { get; }
}

public interface IGameConfigTextSource
{
    bool TryLoadText(GameConfigAssetDefinition config, out string text);
}

public sealed class ResourcesGameConfigTextSource : IGameConfigTextSource
{
    public bool TryLoadText(GameConfigAssetDefinition config, out string text)
    {
        var resourcesAsset = Resources.Load<TextAsset>(config.ResourcesPath);
        if (resourcesAsset != null && !string.IsNullOrWhiteSpace(resourcesAsset.text))
        {
            text = resourcesAsset.text;
            return true;
        }

        var diskPath = GameCommonUtility.ToDiskPath(config.EditorPath);
        if (File.Exists(diskPath))
        {
            text = File.ReadAllText(diskPath);
            return !string.IsNullOrWhiteSpace(text);
        }

        text = null;
        return false;
    }
}

public static class GameConfigRepository
{
    private static readonly GameConfigAssetDefinition TaskConfigAsset = new GameConfigAssetDefinition(
        GameDefine.TaskConfigFileName,
        GameDefine.TaskConfigResourcesPath,
        GameDefine.TaskConfigEditorPath);

    private static readonly GameConfigAssetDefinition CardPackConfigAsset = new GameConfigAssetDefinition(
        GameDefine.CardPackConfigFileName,
        GameDefine.CardPackConfigResourcesPath,
        GameDefine.CardPackConfigEditorPath);

    private static IGameConfigTextSource sTextSource = new ResourcesGameConfigTextSource();
    private static bool sTaskConfigsLoaded;
    private static bool sTaskConfigsLoadSucceeded;
    private static bool sCardPackConfigsLoaded;
    private static bool sCardPackConfigsLoadSucceeded;
    private static readonly List<TaskConfigData> sTaskConfigs = new List<TaskConfigData>();
    private static readonly List<CardPackConfigData> sCardPackConfigs = new List<CardPackConfigData>();

    public static void SetTextSource(IGameConfigTextSource textSource)
    {
        sTextSource = textSource ?? new ResourcesGameConfigTextSource();
        ResetCache();
    }

    public static void ResetCache()
    {
        sTaskConfigsLoaded = false;
        sTaskConfigsLoadSucceeded = false;
        sCardPackConfigsLoaded = false;
        sCardPackConfigsLoadSucceeded = false;
        sTaskConfigs.Clear();
        sCardPackConfigs.Clear();
    }

    public static IReadOnlyList<TaskConfigData> GetTaskConfigs()
    {
        EnsureTaskConfigsLoaded();
        return sTaskConfigs;
    }

    public static bool TryGetTaskConfigs(out IReadOnlyList<TaskConfigData> configs)
    {
        EnsureTaskConfigsLoaded();
        configs = sTaskConfigs;
        return sTaskConfigsLoadSucceeded;
    }

    public static bool TryGetTaskConfig(int taskId, out TaskConfigData config)
    {
        EnsureTaskConfigsLoaded();
        for (var i = 0; i < sTaskConfigs.Count; i++)
        {
            if (sTaskConfigs[i].TaskId == taskId)
            {
                config = sTaskConfigs[i];
                return true;
            }
        }

        config = default;
        return false;
    }

    public static IReadOnlyList<CardPackConfigData> GetCardPackConfigs()
    {
        EnsureCardPackConfigsLoaded();
        return sCardPackConfigs;
    }

    public static bool TryGetCardPackConfigs(out IReadOnlyList<CardPackConfigData> configs)
    {
        EnsureCardPackConfigsLoaded();
        configs = sCardPackConfigs;
        return sCardPackConfigsLoadSucceeded;
    }

    public static bool TryGetCardPackConfig(int packId, out CardPackConfigData config)
    {
        EnsureCardPackConfigsLoaded();
        for (var i = 0; i < sCardPackConfigs.Count; i++)
        {
            if (sCardPackConfigs[i].PackId == packId)
            {
                config = sCardPackConfigs[i];
                return true;
            }
        }

        config = default;
        return false;
    }

    private static void EnsureTaskConfigsLoaded()
    {
        if (sTaskConfigsLoaded)
        {
            return;
        }

        sTaskConfigs.Clear();
        if (!TryLoadConfigTable(TaskConfigAsset, out var table))
        {
            sTaskConfigsLoadSucceeded = false;
            sTaskConfigsLoaded = true;
            return;
        }

        for (var i = 0; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];
            if (!TryParseTaskConfig(row, out var config))
            {
                Debug.LogWarning($"{TaskConfigAsset.DisplayName}: skipped invalid row at line {row.LineNumber}.");
                continue;
            }

            sTaskConfigs.Add(config);
        }

        sTaskConfigsLoadSucceeded = true;
        sTaskConfigsLoaded = true;
    }

    private static void EnsureCardPackConfigsLoaded()
    {
        if (sCardPackConfigsLoaded)
        {
            return;
        }

        sCardPackConfigs.Clear();
        if (!TryLoadConfigTable(CardPackConfigAsset, out var table))
        {
            sCardPackConfigsLoadSucceeded = false;
            sCardPackConfigsLoaded = true;
            return;
        }

        for (var i = 0; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];
            if (!TryParseCardPackConfig(row, out var config))
            {
                Debug.LogWarning($"{CardPackConfigAsset.DisplayName}: skipped invalid row at line {row.LineNumber}.");
                continue;
            }

            sCardPackConfigs.Add(config);
        }

        sCardPackConfigsLoadSucceeded = true;
        sCardPackConfigsLoaded = true;
    }

    private static bool TryLoadConfigTable(GameConfigAssetDefinition asset, out CsvTable table)
    {
        table = null;
        if (!sTextSource.TryLoadText(asset, out var text))
        {
            Debug.LogError($"GameConfigRepository failed to load config: {asset.DisplayName}");
            return false;
        }

        table = CsvTable.Parse(text);
        if (table.Headers.Count == 0)
        {
            Debug.LogError($"GameConfigRepository loaded empty config: {asset.DisplayName}");
            return false;
        }

        return true;
    }

    private static bool TryParseTaskConfig(CsvRow row, out TaskConfigData config)
    {
        config = default;
        if (!row.TryGetInt("Index", out var index)
            || !row.TryGetInt("TaskId", out var taskId)
            || taskId <= 0)
        {
            return false;
        }

        config = new TaskConfigData
        {
            Index = index,
            TaskId = taskId,
            TaskType = (TaskType)GetOptionalInt(row, "TaskType"),
            CompleteValue = GetOptionalInt(row, "CompleteValue"),
            RewardType = (RewardType)GetOptionalInt(row, "RewardType"),
            RewardId = GetOptionalInt(row, "RewardId"),
            RewardValue = GetOptionalInt(row, "RewardValue")
        };
        return true;
    }

    private static bool TryParseCardPackConfig(CsvRow row, out CardPackConfigData config)
    {
        config = default;
        if (!row.TryGetInt("Index", out var index))
        {
            return false;
        }

        var packId = GetOptionalInt(row, "PackId");
        if (packId <= 0)
        {
            packId = index;
        }

        if (packId <= 0)
        {
            return false;
        }

        var chapterId = GetOptionalInt(row, "ChapterId");
        if (chapterId <= 0)
        {
            chapterId = ((Math.Max(1, index) - 1) / 18) + 1;
        }

        config = new CardPackConfigData
        {
            Index = index,
            PackId = packId,
            PackSize = (CardPackSize)GetOptionalInt(row, "PackSize"),
            ChapterId = chapterId
        };
        return true;
    }

    private static int GetOptionalInt(CsvRow row, string header)
    {
        return row.TryGetInt(header, out var value) ? value : 0;
    }
}
