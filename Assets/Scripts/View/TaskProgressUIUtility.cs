using TMPro;
using UnityEngine;

/// <summary>
/// 用途：统一刷新 MainScene 与 GameScene 中共享 TaskItem 的任务内容和进度。返回：按方法说明。
/// </summary>
public static class TaskProgressUIUtility
{
    private const string TaskContentPath = "TaskContent";
    private const string TextProgressPath = "ProgressBg/TextProgress";
    private const string ProgressMaskPath = "ProgressBg/ProgressMask";
    private const string ProgressFillPath = "ProgressBg/ProgressMask/Progress";
    private const string RewardCountPath = "BagBg/TextAddNum";

    /// <summary>
    /// 用途：刷新任务静态信息、奖励信息和当前进度。返回：必要进度节点是否完整。
    /// </summary>
    public static bool RefreshTask(
        Transform taskItem,
        TaskConfigData taskConfig,
        int displayValue,
        bool showCompletedMessage = false)
    {
        if (taskItem == null)
        {
            Debug.LogWarning("TaskProgressUIUtility: TaskItem root is missing.");
            return false;
        }

        taskItem.gameObject.SetActive(true);
        RefreshTaskContent(taskItem, taskConfig, showCompletedMessage);
        RefreshReward(taskItem, taskConfig);
        return SetProgressInternal(taskItem, taskConfig, displayValue, true);
    }

    /// <summary>
    /// 用途：使用同一显示值刷新任务数字和进度条宽度。返回：必要进度节点是否完整。
    /// </summary>
    public static bool SetProgress(
        Transform taskItem,
        TaskConfigData taskConfig,
        int displayValue)
    {
        return SetProgressInternal(taskItem, taskConfig, displayValue, false);
    }

    private static bool SetProgressInternal(
        Transform taskItem,
        TaskConfigData taskConfig,
        int displayValue,
        bool initialize)
    {
        if (taskItem == null)
        {
            return false;
        }

        var progressText = taskItem.Find(TextProgressPath)?.GetComponent<TMP_Text>();
        var progressMask = taskItem.Find(ProgressMaskPath) as RectTransform;
        var progressFill = taskItem.Find(ProgressFillPath) as RectTransform;
        if (progressText == null || progressMask == null || progressFill == null)
        {
            if (initialize)
            {
                Debug.LogWarning(
                    "TaskProgressUIUtility: shared TaskItem is missing TextProgress, ProgressMask, or Progress.");
            }

            return false;
        }

        var safeDisplayValue = Mathf.Max(0, displayValue);
        var targetValue = Mathf.Max(0, taskConfig.CompleteValue);
        if (initialize)
        {
            GameFontUtility.ApplyDefaultFont(progressText);
        }

        progressText.text = $"{safeDisplayValue}/{targetValue}";

        var ratio = targetValue > 0
            ? Mathf.Clamp01((float)safeDisplayValue / targetValue)
            : 0f;
        var fullWidth = progressFill.sizeDelta.x;
        if (fullWidth <= 0f)
        {
            fullWidth = progressFill.rect.width;
        }

        progressMask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fullWidth * ratio);
        return true;
    }

    private static void RefreshTaskContent(
        Transform taskItem,
        TaskConfigData taskConfig,
        bool showCompletedMessage)
    {
        var taskContent = taskItem.Find(TaskContentPath)?.GetComponent<TMP_Text>();
        if (taskContent == null)
        {
            Debug.LogWarning("TaskProgressUIUtility: shared TaskItem is missing TaskContent.");
            return;
        }

        GameFontUtility.ApplyDefaultFont(taskContent);
        taskContent.text = showCompletedMessage
            ? $"累计获得 {taskConfig.CompleteValue} 分，获得卡包奖励！"
            : $"累计获得 {taskConfig.CompleteValue} 分";
    }

    private static void RefreshReward(Transform taskItem, TaskConfigData taskConfig)
    {
        var rewardValue = taskConfig.RewardValue > 0 ? taskConfig.RewardValue : 1;

        var rewardCountText = taskItem.Find(RewardCountPath)?.GetComponent<TMP_Text>();
        if (rewardCountText != null)
        {
            GameFontUtility.ApplyDefaultFont(rewardCountText);
            rewardCountText.text = $"+{rewardValue}";
        }
    }
}
