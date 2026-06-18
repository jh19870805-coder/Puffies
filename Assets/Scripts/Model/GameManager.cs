using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameManager
{
    private static int sBagId = GameDefine.DefaultBagId;
    private static bool sIsInitialized;

    /// <summary>
    /// 用途：在首个场景加载前初始化运行时状态。返回：无。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        Initialize();
    }

    /// <summary>
    /// 用途：初始化运行时默认状态，只在首次调用时生效。返回：无。
    /// </summary>
    public static void Initialize()
    {
        if (sIsInitialized)
        {
            return;
        }

        sBagId = GameDefine.DefaultBagId;
        sIsInitialized = true;
        Debug.Log("GameManager initialized.");
    }

    /// <summary>
    /// 用途：获取当前生效的包编号。返回：包编号整数值。
    /// </summary>
    /// <returns>返回：当前包编号。</returns>
    public static int GetBagId()
    {
        return sBagId;
    }

    /// <summary>
    /// 用途：设置当前使用的包编号。返回：无。
    /// </summary>
    /// <param name="bagId">参数：目标包编号。</param>
    public static void SetBagId(int bagId)
    {
        sBagId = bagId;
    }

    /// <summary>
    /// 用途：设置目标卡包编号并切换到游戏场景。返回：无。
    /// </summary>
    /// <param name="bagId">参数：进入游戏场景时要使用的卡包编号。</param>
    public static void EnterGameScene(int bagId)
    {
        SetBagId(bagId);
        SceneManager.LoadScene(GameDefine.SceneGame);
    }

    /// <summary>
    /// 用途：切换到排行榜场景。返回：无。
    /// </summary>
    public static void EnterRankScene()
    {
        SceneManager.LoadScene(GameDefine.SceneRank);
    }

    /// <summary>
    /// 用途：切换到成就场景。返回：无。
    /// </summary>
    public static void EnterAchieveScene()
    {
        SceneManager.LoadScene(GameDefine.SceneAchieve);
    }

    /// <summary>
    /// 用途：返回主场景。返回：无。
    /// </summary>
    public static void EnterMainScene()
    {
        SceneManager.LoadScene(GameDefine.SceneMain);
    }
}
