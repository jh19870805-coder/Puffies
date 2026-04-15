using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScene : MonoBehaviour
{
    private const float ReferenceHeight = 1080f;
    private const float PixelsPerUnit = 100f;
    private const float GamePageCameraPadding = 0.3f;
    private const float DraggableLeftPadding = 0.6f;
    private const float DraggableVerticalSpacing = 0.2f;
    private const float SnapDistance = 0.35f;
    private const int GameBoardSortingOrder = 0;
    private const int GrooveSortingOrder = 5;
    private const int PieceSortingOrder = 20;
    private const string BootstrapObjectName = "GameSceneBootstrap";
    private const string GameBoardObjectName = "GameBoard";
    private const string AllPiecesRootObjectName = "AllPieces";
    private const string DraggableGroupRootObjectName = "DraggableGroupPieces";
    private const string PlacedPiecesRootObjectName = "PlacedPieces";
    private static bool sHookedSceneLoaded;
    private string _activeBagFolderPath;
    private string _activeGameBoardPath;
    private List<List<string>> _activePieceGroups;
    private PackageConfigData _activePackageConfig;
    private SpriteRenderer _gameBoardRenderer;
    private List<List<SpriteRenderer>> _grooveRenderersByGroup = new List<List<SpriteRenderer>>();
    private readonly List<DraggablePieceState> _currentGroupDraggables = new List<DraggablePieceState>();
    private int _currentGroupIndex = -1;
    private DraggablePieceState _draggingPiece;
    private Vector3 _dragOffset;

    private sealed class DraggablePieceState
    {
        public SpriteRenderer PieceRenderer;
        public SpriteRenderer GrooveRenderer;
        public Vector3 StartPosition;
        public bool IsPlaced;
    }

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
    /// 用途：每帧处理拖拽输入，支持鼠标与触屏拖拽拼图碎片。返回：无。
    /// </summary>
    private void Update()
    {
        HandleMouseDragInput();
        HandleTouchDragInput();
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
        _gameBoardRenderer = CreateGameBoard(_activePackageConfig.Board);
        if (_gameBoardRenderer == null)
        {
            Debug.LogWarning($"GameBoard create failed: {_activePackageConfig.Board}");
            return;
        }

        _grooveRenderersByGroup = CreateAllPieces(_activePackageConfig, _gameBoardRenderer);
        CreateDraggableGroup(0);
        FitGamePageToCamera(_gameBoardRenderer, CollectCurrentVisibleRenderers());
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
    /// 用途：读取配置中的全部碎片并按相对棋盘坐标创建。返回：已创建碎片渲染器列表。
    /// </summary>
    /// <param name="config">参数：当前卡包配置数据。</param>
    /// <param name="boardRenderer">参数：已创建的棋盘精灵渲染器。</param>
    /// <returns>返回：所有成功创建的碎片渲染器。</returns>
    private List<List<SpriteRenderer>> CreateAllPieces(PackageConfigData config, SpriteRenderer boardRenderer)
    {
        var renderersByGroup = new List<List<SpriteRenderer>>();
        if (boardRenderer == null || config.Pieces == null || config.Pieces.Length == 0)
        {
            return renderersByGroup;
        }

        var existingRoot = GameObject.Find(AllPiecesRootObjectName);
        if (existingRoot != null)
        {
            Destroy(existingRoot);
        }

        var root = new GameObject(AllPiecesRootObjectName);
        var boardTextureSize = boardRenderer.sprite.rect.size;
        for (var groupIndex = 0; groupIndex < config.Pieces.Length; groupIndex++)
        {
            var groupRenderers = new List<SpriteRenderer>();
            var items = config.Pieces[groupIndex].Items;
            if (items == null || items.Length == 0)
            {
                renderersByGroup.Add(groupRenderers);
                continue;
            }

            for (var itemIndex = 0; itemIndex < items.Length; itemIndex++)
            {
                var piece = items[itemIndex];
                var pieceRenderer = CreateSpriteObject(
                    $"Piece_{groupIndex}_{itemIndex}",
                    $"{GameDefine.TexturesRoot}/{piece.Sprite}",
                    GrooveSortingOrder + piece.z,
                    root.transform,
                    forceCreate: true);
                if (pieceRenderer == null)
                {
                    continue;
                }

                pieceRenderer.transform.position = ConvertBoardRelativeToWorldPosition(
                    boardRenderer.transform.position,
                    boardTextureSize,
                    new Vector2(piece.x, piece.y));
                SetRendererAlpha(pieceRenderer, 0f);
                groupRenderers.Add(pieceRenderer);
            }

            renderersByGroup.Add(groupRenderers);
        }

        return renderersByGroup;
    }

    /// <summary>
    /// 用途：创建指定组的可拖拽碎片（左侧竖排），并初始化吸附目标。返回：无。
    /// </summary>
    /// <param name="groupIndex">参数：要创建的组索引。</param>
    private void CreateDraggableGroup(int groupIndex)
    {
        ClearCurrentDraggableGroup();
        _currentGroupIndex = groupIndex;

        if (_activePackageConfig.Pieces == null
            || groupIndex < 0
            || groupIndex >= _activePackageConfig.Pieces.Length
            || _grooveRenderersByGroup == null
            || groupIndex >= _grooveRenderersByGroup.Count)
        {
            return;
        }

        var groupItems = _activePackageConfig.Pieces[groupIndex].Items;
        var grooveGroup = _grooveRenderersByGroup[groupIndex];
        if (groupItems == null || groupItems.Length == 0 || grooveGroup == null || grooveGroup.Count == 0)
        {
            return;
        }

        var root = new GameObject(DraggableGroupRootObjectName);
        var firstPieceRenderer = CreateSpriteObject(
            $"DraggablePiece_{groupIndex}_0",
            $"{GameDefine.TexturesRoot}/{groupItems[0].Sprite}",
            PieceSortingOrder,
            root.transform,
            forceCreate: true);
        if (firstPieceRenderer == null)
        {
            return;
        }

        var boardBounds = _gameBoardRenderer.bounds;
        var firstHalfWidth = firstPieceRenderer.bounds.extents.x;
        var firstHeight = firstPieceRenderer.bounds.size.y;
        var startX = boardBounds.min.x - DraggableLeftPadding - firstHalfWidth;
        var totalHeight = (groupItems.Length - 1) * (firstHeight + DraggableVerticalSpacing);
        var startY = boardBounds.center.y + totalHeight * 0.5f;

        firstPieceRenderer.transform.position = new Vector3(startX, startY, 0f);
        _currentGroupDraggables.Add(new DraggablePieceState
        {
            PieceRenderer = firstPieceRenderer,
            GrooveRenderer = grooveGroup.Count > 0 ? grooveGroup[0] : null,
            StartPosition = firstPieceRenderer.transform.position,
            IsPlaced = false
        });

        for (var i = 1; i < groupItems.Length; i++)
        {
            var pieceRenderer = CreateSpriteObject(
                $"DraggablePiece_{groupIndex}_{i}",
                $"{GameDefine.TexturesRoot}/{groupItems[i].Sprite}",
                PieceSortingOrder,
                root.transform,
                forceCreate: true);
            if (pieceRenderer == null)
            {
                continue;
            }

            pieceRenderer.transform.position = new Vector3(startX, startY - i * (firstHeight + DraggableVerticalSpacing), 0f);
            _currentGroupDraggables.Add(new DraggablePieceState
            {
                PieceRenderer = pieceRenderer,
                GrooveRenderer = i < grooveGroup.Count ? grooveGroup[i] : null,
                StartPosition = pieceRenderer.transform.position,
                IsPlaced = false
            });
        }
    }

    /// <summary>
    /// 用途：清理当前可拖拽碎片组对象与状态。返回：无。
    /// </summary>
    private void ClearCurrentDraggableGroup()
    {
        _draggingPiece = null;
        _currentGroupDraggables.Clear();

        var root = GameObject.Find(DraggableGroupRootObjectName);
        if (root != null)
        {
            Destroy(root);
        }
    }

    /// <summary>
    /// 用途：收集当前页面可见内容用于相机框选。返回：渲染器列表。
    /// </summary>
    /// <returns>返回：页面可见渲染器集合。</returns>
    private List<SpriteRenderer> CollectCurrentVisibleRenderers()
    {
        var renderers = new List<SpriteRenderer>();
        if (_gameBoardRenderer != null)
        {
            renderers.Add(_gameBoardRenderer);
        }

        for (var i = 0; i < _currentGroupDraggables.Count; i++)
        {
            var pieceRenderer = _currentGroupDraggables[i].PieceRenderer;
            if (pieceRenderer != null)
            {
                renderers.Add(pieceRenderer);
            }
        }

        return renderers;
    }

    /// <summary>
    /// 用途：处理鼠标拖拽输入。返回：无。
    /// </summary>
    private void HandleMouseDragInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryBeginDrag(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            UpdateDragging(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndDragging();
        }
    }

    /// <summary>
    /// 用途：处理触屏拖拽输入（首个触点）。返回：无。
    /// </summary>
    private void HandleTouchDragInput()
    {
        if (Input.touchCount <= 0)
        {
            return;
        }

        var touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            TryBeginDrag(touch.position);
            return;
        }

        if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
        {
            UpdateDragging(touch.position);
            return;
        }

        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            EndDragging();
        }
    }

    /// <summary>
    /// 用途：尝试开始拖拽当前组中的碎片。返回：无。
    /// </summary>
    /// <param name="screenPosition">参数：输入屏幕坐标。</param>
    private void TryBeginDrag(Vector2 screenPosition)
    {
        if (_draggingPiece != null)
        {
            return;
        }

        var world = ScreenToWorld(screenPosition);
        for (var i = _currentGroupDraggables.Count - 1; i >= 0; i--)
        {
            var state = _currentGroupDraggables[i];
            if (state == null || state.IsPlaced || state.PieceRenderer == null)
            {
                continue;
            }

            if (!state.PieceRenderer.bounds.Contains(world))
            {
                continue;
            }

            _draggingPiece = state;
            _dragOffset = state.PieceRenderer.transform.position - world;
            state.PieceRenderer.sortingOrder = PieceSortingOrder + 100;
            break;
        }
    }

    /// <summary>
    /// 用途：更新当前拖拽碎片的位置。返回：无。
    /// </summary>
    /// <param name="screenPosition">参数：输入屏幕坐标。</param>
    private void UpdateDragging(Vector2 screenPosition)
    {
        if (_draggingPiece == null || _draggingPiece.PieceRenderer == null)
        {
            return;
        }

        var world = ScreenToWorld(screenPosition);
        _draggingPiece.PieceRenderer.transform.position = new Vector3(
            world.x + _dragOffset.x,
            world.y + _dragOffset.y,
            0f);
    }

    /// <summary>
    /// 用途：结束当前拖拽并执行吸附或回弹。返回：无。
    /// </summary>
    private void EndDragging()
    {
        if (_draggingPiece == null || _draggingPiece.PieceRenderer == null)
        {
            return;
        }

        var state = _draggingPiece;
        _draggingPiece = null;
        state.PieceRenderer.sortingOrder = PieceSortingOrder;

        if (state.GrooveRenderer != null
            && Vector3.Distance(state.PieceRenderer.transform.position, state.GrooveRenderer.transform.position) <= SnapDistance)
        {
            state.PieceRenderer.transform.position = state.GrooveRenderer.transform.position;
            var placedRoot = GetOrCreatePlacedPiecesRoot();
            state.PieceRenderer.transform.SetParent(placedRoot.transform, worldPositionStays: true);
            state.IsPlaced = true;
            TryAdvanceGroup();
            return;
        }

        state.PieceRenderer.transform.position = state.StartPosition;
    }

    /// <summary>
    /// 用途：检查当前组是否全部放置完成，若完成则切到下一组或结束游戏。返回：无。
    /// </summary>
    private void TryAdvanceGroup()
    {
        for (var i = 0; i < _currentGroupDraggables.Count; i++)
        {
            if (!_currentGroupDraggables[i].IsPlaced)
            {
                return;
            }
        }

        var nextGroupIndex = _currentGroupIndex + 1;
        if (_activePackageConfig.Pieces != null && nextGroupIndex < _activePackageConfig.Pieces.Length)
        {
            CreateDraggableGroup(nextGroupIndex);
            FitGamePageToCamera(_gameBoardRenderer, CollectCurrentVisibleRenderers());
            return;
        }

        Debug.Log("游戏结束");
    }

    /// <summary>
    /// 用途：获取已吸附碎片根节点，不存在时自动创建。返回：根节点对象。
    /// </summary>
    /// <returns>返回：用于承载已吸附碎片的根节点。</returns>
    private static GameObject GetOrCreatePlacedPiecesRoot()
    {
        var root = GameObject.Find(PlacedPiecesRootObjectName);
        if (root != null)
        {
            return root;
        }

        return new GameObject(PlacedPiecesRootObjectName);
    }

    /// <summary>
    /// 用途：将棋盘相对坐标（像素，左下为原点且坐标点为碎片中心）转换为世界坐标。返回：世界坐标。
    /// </summary>
    /// <param name="boardWorldCenter">参数：棋盘中心的世界坐标。</param>
    /// <param name="boardTextureSize">参数：棋盘纹理尺寸（像素）。</param>
    /// <param name="relativePixelPosition">参数：配置中的相对坐标（x/y）。</param>
    /// <returns>返回：转换后的世界坐标。</returns>
    private static Vector3 ConvertBoardRelativeToWorldPosition(
        Vector3 boardWorldCenter,
        Vector2 boardTextureSize,
        Vector2 relativePixelPosition)
    {
        var localX = (relativePixelPosition.x - boardTextureSize.x * 0.5f) / PixelsPerUnit;
        var localY = (relativePixelPosition.y - boardTextureSize.y * 0.5f) / PixelsPerUnit;
        return new Vector3(
            boardWorldCenter.x + localX,
            boardWorldCenter.y + localY,
            0f);
    }

    /// <summary>
    /// 用途：设置精灵渲染器透明度。返回：无。
    /// </summary>
    /// <param name="renderer">参数：要设置透明度的渲染器。</param>
    /// <param name="alpha">参数：目标透明度（0~1）。</param>
    private static void SetRendererAlpha(SpriteRenderer renderer, float alpha)
    {
        if (renderer == null)
        {
            return;
        }

        var color = renderer.color;
        color.a = Mathf.Clamp01(alpha);
        renderer.color = color;
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
    /// 用途：将屏幕坐标转换为世界坐标。返回：世界坐标。
    /// </summary>
    /// <param name="screenPosition">参数：屏幕坐标。</param>
    /// <returns>返回：世界坐标，若相机为空则返回零向量。</returns>
    private static Vector3 ScreenToWorld(Vector2 screenPosition)
    {
        var camera = Camera.main;
        if (camera == null)
        {
            return Vector3.zero;
        }

        var world = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -camera.transform.position.z));
        world.z = 0f;
        return world;
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
    private SpriteRenderer CreateSpriteObject(
        string objectName,
        string spritePath,
        int sortingOrder,
        Transform parent,
        bool forceCreate = false)
    {
        if (!forceCreate)
        {
            var existing = GameObject.Find(objectName);
            if (existing != null)
            {
                return existing.GetComponent<SpriteRenderer>();
            }
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
