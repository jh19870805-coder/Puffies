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

/// <summary>
/// 用途：根据卡包尺寸计算游戏结算分数。返回：按方法说明。
/// </summary>
public static class GameScoreUtility
{
    public const float TimeThresholdASeconds = 15f;
    public const float TimeThresholdBSeconds = 30f;
    public const float TimeThresholdCSeconds = 60f;

    private const int NoHintBonusPercent = 5;
    private const int LevelOutlineDisabledBonusPercent = 2;
    private const int StickerOutlineDisabledBonusPercent = 5;

    /// <summary>
    /// 用途：按卡包 Id 获取当前基础结算分数。返回：是否找到有效的卡包尺寸配置。
    /// </summary>
    public static bool TryGetCardPackBaseScore(int packId, out int baseScore)
    {
        baseScore = 0;
        if (!CardPackDataUtility.TryGetPackConfig(packId, out var packSize))
        {
            return false;
        }

        baseScore = GetBaseScore(packSize);
        return baseScore > 0;
    }

    /// <summary>
    /// 用途：按卡包尺寸获取基础结算分数。返回：未配置尺寸返回 0。
    /// </summary>
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

    /// <summary>
    /// 用途：计算卡包本局的完整结算分数。返回：是否找到有效卡包基础分。
    /// </summary>
    public static bool TryCalculateCardPackScore(
        int packId,
        GameScoreContext context,
        out GameScoreResult result)
    {
        result = default;
        if (!TryGetCardPackBaseScore(packId, out var baseScore))
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

    /// <summary>
    /// 用途：按完成耗时返回时间加成百分比。返回：3、2、1 或 0。
    /// </summary>
    public static int GetCompletionTimeBonusPercent(float completionTimeSeconds)
    {
        var safeTime = SanitizeCompletionTime(completionTimeSeconds);
        if (safeTime <= TimeThresholdASeconds)
        {
            return 3;
        }

        if (safeTime <= TimeThresholdBSeconds)
        {
            return 2;
        }

        return safeTime <= TimeThresholdCSeconds ? 1 : 0;
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
