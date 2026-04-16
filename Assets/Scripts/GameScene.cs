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
    private const float DraggableLeftPadding = 0.2f;
    private const float DraggableHorizontalSpacingPixels = 20f;
    private const float PieceTrayMaxHeightRatio = 0.9f;
    private const float SnapDistance = 0.35f;
    private const int GameBoardSortingOrder = 0;
    private const int PieceBgFillSortingOrder = 499;
    private const int PieceBgSortingOrder = 500;
    private const float PieceBgAlpha = 1f;
    private const float PieceBgFillAlpha = 0.3f;
    private const int GrooveSortingOrder = 5;
    private const int PieceSortingOrder = 520;
    private const string BootstrapObjectName = "GameSceneBootstrap";
    private const string GameBoardObjectName = "GameBoard";
    private const string PieceBgFillObjectName = "PieceBgFill";
    private const string PieceBgObjectName = "PieceBg";
    private const string PieceBgPath = "Textures/BasicUI/ImgMaskBlack.png";
    private const string AllPiecesRootObjectName = "AllPieces";
    private const string DraggableGroupRootObjectName = "DraggableGroupPieces";
    private const string PlacedPiecesRootObjectName = "PlacedPieces";
    private static bool sHookedSceneLoaded;
    private string _activeBagFolderPath;
    private string _activeGameBoardPath;
    private List<List<string>> _activePieceGroups;
    private PackageConfigData _activePackageConfig;
    private SpriteRenderer _gameBoardRenderer;
    private SpriteRenderer _pieceBgRenderer;
    private List<List<SpriteRenderer>> _grooveRenderersByGroup = new List<List<SpriteRenderer>>();
    private bool _isBoardAndGroovesInitialized;
    private readonly List<DraggablePieceState> _currentGroupDraggables = new List<DraggablePieceState>();
    private int _currentGroupIndex = -1;
    private DraggablePieceState _draggingPiece;
    private Vector3 _dragOffset;

    private sealed class DraggablePieceState
    {
        public SpriteRenderer PieceRenderer;
        public SpriteRenderer GrooveRenderer;
        public Vector3 StartPosition;
        public Vector3 TrayScale;
        public Vector3 DragScale;
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

        if (Camera.main != null)
        {
            GameCommonUtility.SetupOrthographicCamera(Camera.main, ReferenceHeight, PixelsPerUnit);
        }

        var selectedBagId = GameManager.GetBagId();
        PrepareBagResources(selectedBagId);
        Debug.Log("GameScene bootstrap completed with bag resources prepared.");
    }

    /// <summary>
    /// 用途：每帧处理拖拽输入，支持鼠标与触屏拖拽拼图碎片。返回：无。
    /// </summary>
    private void Update()
    {
        GameCommonUtility.ProcessPointerInput(
            TryBeginDrag,
            UpdateDragging,
            OnPointerEnd);
    }

    /// <summary>
    /// 用途：根据当前包配置准备棋盘和拼图碎片资源路径，并输出资源统计信息。返回：无。
    /// </summary>
    /// <param name="bagId">参数：本次进入游戏场景时要加载的卡包编号。</param>
    private void PrepareBagResources(int bagId)
    {
        GameManager.SetBagId(bagId);
        _activeBagFolderPath = GameManager.GetBagFolderPath();
        var configPath = GameManager.GetBagConfigPath();
        if (!GameManager.TryLoadPackageConfig(configPath, out _activePackageConfig))
        {
            Debug.LogWarning($"Failed to load package config: {configPath}");
            return;
        }

        _activeGameBoardPath = _activePackageConfig.Board;
        EnsureBoardAndGroovesInitialized();
        if (_gameBoardRenderer == null)
        {
            Debug.LogWarning($"GameBoard create failed: {_activePackageConfig.Board}");
            return;
        }

        CreateDraggableGroup(0);
        FitGamePageToCamera(_gameBoardRenderer, CollectCurrentVisibleRenderers());
        _activePieceGroups = ConvertConfigToPieceGroups(_activePackageConfig);
        var pieceCount = CountPieces(_activePieceGroups);

        Debug.Log(
            $"GameScene bag resources ready. Folder={_activeBagFolderPath}, " +
            $"Board={_activeGameBoardPath}, Groups={_activePieceGroups?.Count ?? 0}, Pieces={pieceCount}");
    }

    /// <summary>
    /// 用途：确保棋盘和凹槽只在单次游戏进入时初始化一次。返回：无。
    /// </summary>
    private void EnsureBoardAndGroovesInitialized()
    {
        if (_isBoardAndGroovesInitialized)
        {
            return;
        }

        _gameBoardRenderer = CreateGameBoard(_activePackageConfig.Board);
        if (_gameBoardRenderer == null)
        {
            return;
        }

        _pieceBgRenderer = CreatePieceBackground(_gameBoardRenderer);
        _grooveRenderersByGroup = CreateAllPieces(_activePackageConfig, _gameBoardRenderer);
        _isBoardAndGroovesInitialized = true;
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
    /// 用途：创建拼图托盘背景（九宫拉伸），宽度贴合页面，高度为棋盘高度的 1/4。返回：背景渲染器。
    /// </summary>
    private SpriteRenderer CreatePieceBackground(SpriteRenderer boardRenderer)
    {
        if (boardRenderer == null)
        {
            return null;
        }

        var boardWidth = boardRenderer.bounds.size.x;
        var bgHeight = boardRenderer.bounds.size.y * 0.25f;
        var boardBottom = boardRenderer.bounds.min.y;
        var bgCenterY = boardBottom + bgHeight * 0.5f;
        var bgPosition = new Vector3(boardRenderer.transform.position.x, bgCenterY, -1f);

        var renderer = CreateSpriteObject(
            PieceBgObjectName,
            PieceBgPath,
            PieceBgSortingOrder,
            parent: null,
            forceCreate: true);
        if (renderer == null)
        {
            var fallbackObject = new GameObject(PieceBgObjectName);
            renderer = fallbackObject.AddComponent<SpriteRenderer>();
        }

        if (renderer.sprite == null)
        {
            renderer.sprite = CreateSolidSprite();
        }

        var slicedSprite = BuildSlicedSprite(renderer.sprite, PieceBgPath);
        if (slicedSprite != null)
        {
            renderer.sprite = slicedSprite;
        }

        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(boardWidth, bgHeight);
        renderer.transform.position = bgPosition;
        renderer.color = new Color(0f, 0f, 0f, PieceBgAlpha);
        CreateOrUpdatePieceBgFill(renderer);
        return renderer;
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

        var hostBounds = _pieceBgRenderer != null ? _pieceBgRenderer.bounds : _gameBoardRenderer.bounds;
        var trayScale = CalculateTrayScale(firstPieceRenderer, hostBounds);
        var firstPieceWidth = GetPieceWidth(firstPieceRenderer, trayScale);
        var firstHalfWidth = firstPieceWidth * 0.5f;
        var startX = hostBounds.min.x + DraggableLeftPadding + firstHalfWidth;
        var startY = hostBounds.center.y;
        var horizontalSpacing = DraggableHorizontalSpacingPixels / PixelsPerUnit;

        firstPieceRenderer.transform.localScale = trayScale;
        firstPieceRenderer.transform.position = new Vector3(startX, startY, 0f);
        _currentGroupDraggables.Add(new DraggablePieceState
        {
            PieceRenderer = firstPieceRenderer,
            GrooveRenderer = grooveGroup.Count > 0 ? grooveGroup[0] : null,
            StartPosition = firstPieceRenderer.transform.position,
            TrayScale = trayScale,
            DragScale = Vector3.one,
            IsPlaced = false
        });

        var nextCenterX = startX + firstHalfWidth + horizontalSpacing;
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

            var currentTrayScale = CalculateTrayScale(pieceRenderer, hostBounds);
            var pieceWidth = GetPieceWidth(pieceRenderer, currentTrayScale);
            var pieceHalfWidth = pieceWidth * 0.5f;
            var pieceCenterX = nextCenterX + pieceHalfWidth;
            pieceRenderer.transform.localScale = currentTrayScale;
            pieceRenderer.transform.position = new Vector3(pieceCenterX, startY, 0f);
            _currentGroupDraggables.Add(new DraggablePieceState
            {
                PieceRenderer = pieceRenderer,
                GrooveRenderer = i < grooveGroup.Count ? grooveGroup[i] : null,
                StartPosition = pieceRenderer.transform.position,
                TrayScale = currentTrayScale,
                DragScale = Vector3.one,
                IsPlaced = false
            });

            nextCenterX = pieceCenterX + pieceHalfWidth + horizontalSpacing;
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
        if (_pieceBgRenderer != null)
        {
            renderers.Add(_pieceBgRenderer);
        }

        if (_grooveRenderersByGroup != null)
        {
            for (var groupIndex = 0; groupIndex < _grooveRenderersByGroup.Count; groupIndex++)
            {
                var group = _grooveRenderersByGroup[groupIndex];
                if (group == null)
                {
                    continue;
                }

                for (var i = 0; i < group.Count; i++)
                {
                    if (group[i] != null)
                    {
                        renderers.Add(group[i]);
                    }
                }
            }
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
    /// 用途：统一输入结束阶段回调，转发到拖拽结束逻辑。返回：无。
    /// </summary>
    /// <param name="screenPosition">参数：输入结束时的屏幕坐标。</param>
    private void OnPointerEnd(Vector2 screenPosition)
    {
        EndDragging();
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

        var world = GameCommonUtility.ScreenToWorld(screenPosition);
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
            state.PieceRenderer.transform.localScale = state.DragScale;
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

        var world = GameCommonUtility.ScreenToWorld(screenPosition);
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
            state.PieceRenderer.transform.localScale = state.DragScale;
            var placedRoot = GetOrCreatePlacedPiecesRoot();
            state.PieceRenderer.transform.SetParent(placedRoot.transform, worldPositionStays: true);
            state.IsPlaced = true;
            TryAdvanceGroup();
            return;
        }

        state.PieceRenderer.transform.position = state.StartPosition;
        state.PieceRenderer.transform.localScale = state.TrayScale;
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
        return GameCommonUtility.CreateSpriteRendererObject(
            objectName,
            spritePath,
            sortingOrder,
            PixelsPerUnit,
            parent,
            forceCreate);
    }

    /// <summary>
    /// 用途：基于原图构建带边框的九宫格 Sprite。返回：可用于 Sliced 的 Sprite。
    /// </summary>
    private static Sprite BuildSlicedSprite(Sprite sourceSprite, string spritePath)
    {
        if (sourceSprite == null)
        {
            return null;
        }

        var imagePathOnDisk = GameCommonUtility.ToDiskPath(spritePath);
        if (!File.Exists(imagePathOnDisk))
        {
            return sourceSprite;
        }

        var imageBytes = File.ReadAllBytes(imagePathOnDisk);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(imageBytes))
        {
            return sourceSprite;
        }

        var borderSize = Mathf.Clamp(Mathf.RoundToInt(Mathf.Min(texture.width, texture.height) * 0.12f), 8, 64);
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            PixelsPerUnit,
            0,
            SpriteMeshType.FullRect,
            new Vector4(borderSize, borderSize, borderSize, borderSize));
    }

    /// <summary>
    /// 用途：创建纯色占位精灵，作为背景图加载失败时的兜底显示。返回：纯色精灵。
    /// </summary>
    private static Sprite CreateSolidSprite()
    {
        var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var colors = new Color[16];
        for (var i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white;
        }

        texture.SetPixels(colors);
        texture.Apply();
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            PixelsPerUnit,
            0,
            SpriteMeshType.FullRect,
            new Vector4(1f, 1f, 1f, 1f));
    }

    /// <summary>
    /// 用途：创建或更新 PieceBg 的实心填充层，确保背景区域可见。返回：无。
    /// </summary>
    private static void CreateOrUpdatePieceBgFill(SpriteRenderer pieceBgRenderer)
    {
        if (pieceBgRenderer == null)
        {
            return;
        }

        var fillObject = GameObject.Find(PieceBgFillObjectName);
        SpriteRenderer fillRenderer;
        if (fillObject == null)
        {
            fillObject = new GameObject(PieceBgFillObjectName);
            fillRenderer = fillObject.AddComponent<SpriteRenderer>();
        }
        else
        {
            fillRenderer = fillObject.GetComponent<SpriteRenderer>();
            if (fillRenderer == null)
            {
                fillRenderer = fillObject.AddComponent<SpriteRenderer>();
            }
        }

        fillRenderer.sprite = CreateSolidSprite();
        fillRenderer.drawMode = SpriteDrawMode.Sliced;
        fillRenderer.size = pieceBgRenderer.size;
        fillRenderer.sortingOrder = PieceBgFillSortingOrder;
        fillRenderer.color = new Color(0f, 0f, 0f, PieceBgFillAlpha);
        fillRenderer.transform.position = new Vector3(
            pieceBgRenderer.transform.position.x,
            pieceBgRenderer.transform.position.y,
            pieceBgRenderer.transform.position.z + 0.01f);
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

    /// <summary>
    /// 用途：根据 PieceBg 高度计算贴片在托盘中的自适应缩放（最大高度 90%）。返回：缩放向量。
    /// </summary>
    private static Vector3 CalculateTrayScale(SpriteRenderer pieceRenderer, Bounds trayBounds)
    {
        if (pieceRenderer == null || pieceRenderer.sprite == null)
        {
            return Vector3.one;
        }

        var spriteHeight = Mathf.Max(0.0001f, pieceRenderer.sprite.bounds.size.y);
        var maxHeight = Mathf.Max(0.0001f, trayBounds.size.y * PieceTrayMaxHeightRatio);
        var scale = Mathf.Min(1f, maxHeight / spriteHeight);
        return new Vector3(scale, scale, 1f);
    }

    /// <summary>
    /// 用途：根据给定缩放计算贴片世界宽度。返回：宽度值。
    /// </summary>
    private static float GetPieceWidth(SpriteRenderer pieceRenderer, Vector3 scale)
    {
        if (pieceRenderer == null || pieceRenderer.sprite == null)
        {
            return 0.01f;
        }

        return Mathf.Max(0.01f, pieceRenderer.sprite.bounds.size.x * scale.x);
    }
}
