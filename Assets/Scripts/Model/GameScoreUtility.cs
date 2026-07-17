/// <summary>
/// 用途：根据卡包尺寸计算游戏结算分数。返回：按方法说明。
/// </summary>
public static class GameScoreUtility
{
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
}
