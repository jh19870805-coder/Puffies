using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScene : MonoBehaviour
{
    private const float ReferenceHeight = 1080f;
    private const float PixelsPerUnit = 100f;
    private const float PieceLeftPadding = 0.5f;
    private const float PieceVerticalSpacing = 0.2f;
    private const float GamePageCameraPadding = 0.3f;
    private const int GameBoardSortingOrder = 0;
    private const int PieceSortingOrder = 10;
    private const string BootstrapObjectName = "GameSceneBootstrap";
    private const string GameBoardObjectName = "GameBoard";
    private const string FirstGroupRootObjectName = "FirstGroupPieces";
    private static bool sHookedSceneLoaded;
    private string _activeBagFolderPath;
    private string _activeGameBoardPath;
    private List<List<string>> _activePieceGroups;
    private PackageConfigData _activePackageConfig;

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

        var selectedBagId = gameManager.GetBagId();
        PrepareBagResources(gameManager, selectedBagId);
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
    /// <param name="bagId">参数：本次进入游戏场景时要加载的卡包编号。</param>
    private void PrepareBagResources(GameManager gameManager, int bagId)
    {
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager is null, cannot prepare bag resources.");
            return;
        }

        gameManager.SetBagId(bagId);
        _activeBagFolderPath = gameManager.GetBagFolderPath();
        var configPath = gameManager.GetBagConfigPath();
        if (!gameManager.TryLoadPackageConfig(configPath, out _activePackageConfig))
        {
            Debug.LogWarning($"Failed to load package config: {configPath}");
            return;
        }

        _activeGameBoardPath = _activePackageConfig.Board;
        var boardRenderer = CreateGameBoard(_activePackageConfig.Board);
        if (boardRenderer == null)
        {
            Debug.LogWarning($"GameBoard create failed: {_activePackageConfig.Board}");
            return;
        }

        var firstGroupRenderers = CreateFirstGroupPieces(_activePackageConfig, boardRenderer);
        FitGamePageToCamera(boardRenderer, firstGroupRenderers);
        _activePieceGroups = ConvertConfigToPieceGroups(_activePackageConfig);
        var pieceCount = CountPieces(_activePieceGroups);

        Debug.Log(
            $"GameScene bag resources ready. Folder={_activeBagFolderPath}, " +
            $"Board={_activeGameBoardPath}, Groups={_activePieceGroups?.Count ?? 0}, Pieces={pieceCount}");
    }

    /// <summary>
    /// 用途：根据配置中的棋盘路径创建并居中显示游戏棋盘。返回：棋盘精灵渲染器。
    /// </summary>
    /// <param name="boardRelativePath">参数：棋盘相对资源路径（如 Game001/GameBoard.png）。</param>
    /// <returns>返回：创建或已存在的棋盘 SpriteRenderer，失败返回 null。</returns>
    private SpriteRenderer CreateGameBoard(string boardRelativePath)
    {
        var boardPath = $"{GameDefine.TexturesRoot}/{boardRelativePath}";
        return CreateCenteredSpriteObject(GameBoardObjectName, boardPath, GameBoardSortingOrder);
    }

    /// <summary>
    /// 用途：读取配置第一组碎片并在棋盘左侧按单列等间距创建。返回：无。
    /// </summary>
    /// <param name="config">参数：当前卡包配置数据。</param>
    /// <param name="boardRenderer">参数：已创建的棋盘精灵渲染器。</param>
    private List<SpriteRenderer> CreateFirstGroupPieces(PackageConfigData config, SpriteRenderer boardRenderer)
    {
        var createdRenderers = new List<SpriteRenderer>();
        if (boardRenderer == null || config.Pieces == null || config.Pieces.Length == 0)
        {
            return createdRenderers;
        }

        var firstGroup = config.Pieces[0].Items;
        if (firstGroup == null || firstGroup.Length == 0)
        {
            return createdRenderers;
        }

        var existingRoot = GameObject.Find(FirstGroupRootObjectName);
        if (existingRoot != null)
        {
            Destroy(existingRoot);
        }

        var root = new GameObject(FirstGroupRootObjectName);
        var boardBounds = boardRenderer.bounds;
        var firstPieceRenderer = CreateSpriteObject($"FirstPiece_0", $"{GameDefine.TexturesRoot}/{firstGroup[0].Sprite}", PieceSortingOrder, root.transform);
        if (firstPieceRenderer == null)
        {
            return createdRenderers;
        }

        var firstPieceHeight = firstPieceRenderer.bounds.size.y;
        var firstPieceHalfWidth = firstPieceRenderer.bounds.extents.x;
        var startX = boardBounds.min.x - PieceLeftPadding - firstPieceHalfWidth;
        var totalHeight = (firstGroup.Length - 1) * (firstPieceHeight + PieceVerticalSpacing);
        var startY = boardBounds.center.y + totalHeight * 0.5f;

        firstPieceRenderer.transform.position = new Vector3(startX, startY, 0f);
        createdRenderers.Add(firstPieceRenderer);

        for (var i = 1; i < firstGroup.Length; i++)
        {
            var pieceRenderer = CreateSpriteObject(
                $"FirstPiece_{i}",
                $"{GameDefine.TexturesRoot}/{firstGroup[i].Sprite}",
                PieceSortingOrder,
                root.transform);
            if (pieceRenderer == null)
            {
                continue;
            }

            var y = startY - i * (firstPieceHeight + PieceVerticalSpacing);
            pieceRenderer.transform.position = new Vector3(startX, y, 0f);
            createdRenderers.Add(pieceRenderer);
        }

        return createdRenderers;
    }

    /// <summary>
    /// 用途：根据棋盘和碎片的实际范围自动调整相机，确保页面完整可见。返回：无。
    /// </summary>
    /// <param name="boardRenderer">参数：棋盘渲染器。</param>
    /// <param name="pieceRenderers">参数：第一组碎片渲染器列表。</param>
    private void FitGamePageToCamera(SpriteRenderer boardRenderer, List<SpriteRenderer> pieceRenderers)
    {
        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        var renderers = new List<Renderer> { boardRenderer };
        if (pieceRenderers != null)
        {
            for (var i = 0; i < pieceRenderers.Count; i++)
            {
                renderers.Add(pieceRenderers[i]);
            }
        }

        GameCommonUtility.FitOrthographicCameraToRenderers(camera, GamePageCameraPadding, renderers.ToArray());
    }

    /// <summary>
    /// 用途：把配置中的碎片分组转换为路径二维列表，便于复用现有统计逻辑。返回：路径二维列表。
    /// </summary>
    /// <param name="config">参数：当前卡包配置数据。</param>
    /// <returns>返回：外层为组、内层为碎片路径的列表。</returns>
    private static List<List<string>> ConvertConfigToPieceGroups(PackageConfigData config)
    {
        var groups = new List<List<string>>();
        if (config.Pieces == null)
        {
            return groups;
        }

        for (var i = 0; i < config.Pieces.Length; i++)
        {
            var items = config.Pieces[i].Items;
            var group = new List<string>();
            if (items != null)
            {
                for (var j = 0; j < items.Length; j++)
                {
                    group.Add($"{GameDefine.TexturesRoot}/{items[j].Sprite}");
                }
            }

            groups.Add(group);
        }

        return groups;
    }

    /// <summary>
    /// 用途：创建居中的精灵对象（若已存在则直接返回）。返回：精灵渲染器。
    /// </summary>
    /// <param name="objectName">参数：场景对象名。</param>
    /// <param name="spritePath">参数：精灵资源路径。</param>
    /// <param name="sortingOrder">参数：渲染顺序。</param>
    /// <returns>返回：创建或已存在的 SpriteRenderer，失败返回 null。</returns>
    private SpriteRenderer CreateCenteredSpriteObject(string objectName, string spritePath, int sortingOrder)
    {
        var renderer = CreateSpriteObject(objectName, spritePath, sortingOrder, parent: null);
        if (renderer == null)
        {
            return null;
        }

        renderer.transform.position = Vector3.zero;
        return renderer;
    }

    /// <summary>
    /// 用途：按资源路径创建精灵对象并返回渲染器。返回：精灵渲染器。
    /// </summary>
    /// <param name="objectName">参数：场景对象名。</param>
    /// <param name="spritePath">参数：精灵资源路径。</param>
    /// <param name="sortingOrder">参数：渲染顺序。</param>
    /// <param name="parent">参数：父节点，传 null 表示无父节点。</param>
    /// <returns>返回：创建或已存在的 SpriteRenderer，失败返回 null。</returns>
    private SpriteRenderer CreateSpriteObject(string objectName, string spritePath, int sortingOrder, Transform parent)
    {
        var existing = GameObject.Find(objectName);
        if (existing != null)
        {
            return existing.GetComponent<SpriteRenderer>();
        }

        var sprite = CreateSpriteByPath(spritePath);
        if (sprite == null)
        {
            Debug.LogWarning($"Failed to create sprite from {spritePath}");
            return null;
        }

        var go = new GameObject(objectName);
        if (parent != null)
        {
            go.transform.SetParent(parent, worldPositionStays: true);
        }

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    /// <summary>
    /// 用途：根据资源路径读取图片并构建 Sprite。返回：Sprite 对象。
    /// </summary>
    /// <param name="imageResourcePath">参数：图片资源路径，支持绝对路径或相对 Assets 路径。</param>
    /// <returns>返回：成功时为 Sprite，失败返回 null。</returns>
    private static Sprite CreateSpriteByPath(string imageResourcePath)
    {
        if (string.IsNullOrWhiteSpace(imageResourcePath))
        {
            return null;
        }

        var imagePathOnDisk = GameCommonUtility.ToDiskPath(imageResourcePath);
        if (!File.Exists(imagePathOnDisk))
        {
            Debug.LogWarning($"CreateSpriteByPath failed: file not found: {imagePathOnDisk}");
            return null;
        }

        var imageBytes = File.ReadAllBytes(imagePathOnDisk);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(imageBytes))
        {
            Debug.LogWarning($"CreateSpriteByPath failed: invalid image file: {imagePathOnDisk}");
            return null;
        }

        var imageSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            PixelsPerUnit);
        imageSprite.name = Path.GetFileNameWithoutExtension(imagePathOnDisk);
        return imageSprite;
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
