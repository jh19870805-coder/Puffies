using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScene : MonoBehaviour
{
    private const float ReferenceHeight = 1080f;
    private const float PixelsPerUnit = 100f;
    private const string BootstrapObjectName = "GameSceneBootstrap";
    private static bool sHookedSceneLoaded;
    private string _activeBagFolderPath;
    private string _activeGameBoardPath;
    private List<List<string>> _activePieceGroups;

    /// <summary>
    /// 用途：在场景加载后自动挂接游戏场景引导逻辑，并对当前活动场景尝试初始化。返回：无。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameCommonUtility.BootstrapSceneComponent<GameScene>(
            ref sHookedSceneLoaded,
            GameDefine.SceneGame,
            BootstrapObjectName);
    }

    /// <summary>
    /// 用途：游戏场景启动入口，完成主相机设置与当前关卡资源准备。返回：无。
    /// </summary>
    private void Start()
    {
        if (!GameCommonUtility.IsSceneMatch(SceneManager.GetActiveScene(), GameDefine.SceneGame))
        {
            Destroy(gameObject);
            return;
        }

        var gameManager = GameManager.CreateInstance();
        if (Camera.main != null)
        {
            SetupMainCamera(Camera.main);
        }

        PrepareBagResources(gameManager);
        Debug.Log("GameScene bootstrap completed with bag resources prepared.");
    }

    /// <summary>
    /// 用途：将指定相机设置为正交相机并按参考高度计算正交尺寸。返回：无。
    /// </summary>
    /// <param name="camera">参数：需要配置的相机对象。</param>
    private static void SetupMainCamera(Camera camera)
    {
        GameCommonUtility.SetupOrthographicCamera(camera, ReferenceHeight, PixelsPerUnit);
    }

    /// <summary>
    /// 用途：根据当前包配置准备棋盘和拼图碎片资源路径，并输出资源统计信息。返回：无。
    /// </summary>
    /// <param name="gameManager">参数：用于读取包配置与资源路径的 GameManager 实例。</param>
    private void PrepareBagResources(GameManager gameManager)
    {
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager is null, cannot prepare bag resources.");
            return;
        }

        _activeBagFolderPath = gameManager.GetBagFolderPath();
        _activeGameBoardPath = gameManager.GetGameBoard();
        _activePieceGroups = gameManager.LoadBagPieces(_activeBagFolderPath);

        var pieceCount = CountPieces(_activePieceGroups);
        if (pieceCount == 0)
        {
            Debug.LogWarning($"No puzzle pieces found under bag folder: {_activeBagFolderPath}");
        }

        if (!File.Exists(_activeGameBoardPath))
        {
            Debug.LogWarning($"GameBoard image not found: {_activeGameBoardPath}");
        }

        Debug.Log(
            $"GameScene bag resources ready. Folder={_activeBagFolderPath}, " +
            $"Board={_activeGameBoardPath}, Groups={_activePieceGroups?.Count ?? 0}, Pieces={pieceCount}");
    }

    /// <summary>
    /// 用途：统计拼图分组中的碎片总数量。返回：碎片总数。
    /// </summary>
    /// <param name="pieceGroups">参数：拼图分组列表，每组包含若干碎片路径。</param>
    /// <returns>返回：所有分组中的碎片数量总和。</returns>
    private static int CountPieces(List<List<string>> pieceGroups)
    {
        if (pieceGroups == null)
        {
            return 0;
        }

        var total = 0;
        for (var i = 0; i < pieceGroups.Count; i++)
        {
            total += pieceGroups[i]?.Count ?? 0;
        }

        return total;
    }
}
