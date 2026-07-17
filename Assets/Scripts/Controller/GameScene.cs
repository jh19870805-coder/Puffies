using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameScene : MonoBehaviour
{
    private const float ReferenceHeight = GameDefine.DesignHeight;
    private const float PixelsPerUnit = GameDefine.PixelsPerUnit;
    private const float WorldGameplayDepth = -0.5f;
    private const float GamePageCameraPadding = 0.3f;
    private const float DraggableLeftPadding = 0.2f;
    private const float DraggableHorizontalSpacingPixels = 20f;
    private const float PieceTrayMaxHeightRatio = 0.9f;
    private const float SnapDistanceMin = 0.2f;
    private const float SnapDistanceMax = 0.8f;
    private const float SnapDistanceSizeRatio = 0.22f;
    private const float PieceBgSlideDuration = 0.25f;
    private const float PieceBgSlideOutPadding = 0.15f;
    private const int PieceBgFillSortingOrder = 499;
    private const int PieceBgSortingOrder = 500;
    private const float PieceBgAlpha = 1f;
    private const float PieceBgFillAlpha = 0.3f;
    private const int PieceSortingOrder = 520;
    private const int ActiveGroupOutlineSortingOrder = PieceSortingOrder - 1;
    private static readonly Vector2 ActiveGroupOutlineScreenOffset = new Vector2(6f, 6f);
    private const string BootstrapObjectName = "GameSceneBootstrap";
    private const string PieceBgFillObjectName = "PieceBgFill";
    private const string PieceBgObjectName = "PieceBg";
    private const string PieceBgPath = GameDefine.UiRoot + "/BasicUI/ImgMaskBlack.png";
    private const string DraggableGroupRootObjectName = "DraggableGroupPieces";
    private const string ActiveGroupOutlineRootObjectName = "ActiveGroupOutline";
    private const string PlacedPiecesRootObjectName = "PlacedPieces";
    private const string TaskBg1ObjectName = "TaskBg1";
    private const string TaskContent1ObjectName = "TaskContent1";
    private const string TaskBagIconObjectName = "BagIcon";
    private const string TaskBagRewardCountPath = "TaskBg1/BagBg/Text (TMP)";
    private const string TaskRewardImgBagPath = "ImgBagBg/ImgBag";
    private static readonly Color ActiveGroupOutlineColor = new Color32(0x3f, 0x42, 0x3e, 0xff);
    private static bool sHookedSceneLoaded;
    private readonly BoardState _board = new BoardState();
    private readonly DragState _drag = new DragState();
    private readonly Dictionary<SpriteRenderer, RectTransform> _outlineProxyTargets =
        new Dictionary<SpriteRenderer, RectTransform>();
    private Vector3 _pieceBgOriginalPosition;
    private bool _hasPieceBgOriginalPosition;
    private bool _isPieceBgHidden;
    private Vector2 _pieceBoardOriginalAnchoredPosition;
    private bool _hasPieceBoardOriginalAnchoredPosition;
    private bool _isPieceBoardHidden;
    private Coroutine _pieceTraySlideCoroutine;
    private Vector2 _originalGameBoardAnchoredPosition;
    private bool _hasOriginalGameBoardAnchoredPosition;
    private bool _isGameFinished;
    private bool _isCollectPuzzleTaskActive;
    private GameObject _rewardPanelRoot;
    private GameObject _loadedCardBagRoot;
    private RectTransform _loadedCardBagRect;
    private Vector2 _originalCardBagAnchoredPosition;
    private bool _hasOriginalCardBagAnchoredPosition;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameCommonUtility.BootstrapSceneComponent<GameScene>(
            ref sHookedSceneLoaded,
            GameDefine.SceneGame,
            BootstrapObjectName);
    }

    private void Start()
    {
        if (!GameCommonUtility.IsSceneMatch(SceneManager.GetActiveScene(), GameDefine.SceneGame))
        {
            Destroy(gameObject);
            return;
        }

        var camera = Camera.main;
        if (camera != null)
        {
            GameCommonUtility.SetupOrthographicCamera(camera, ReferenceHeight, PixelsPerUnit);
        }

        ConfigureGameplayCanvas(camera);
        var selectedBagId = GameManager.GetBagId();
        InitializeGameplay(selectedBagId);
        InitializeTaskTracking();
        ConfigureReturnButton();
        ConfigureRewardPanel();
        Debug.Log("GameScene bootstrap completed.");
    }

    private void Update()
    {
        GameCommonUtility.ProcessPointerInput(
            TryBeginDrag,
            UpdateDragging,
            OnPointerEnd);
    }

    private void LateUpdate()
    {
        if (_outlineProxyTargets.Count == 0)
        {
            return;
        }

        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        var boardScale = CalculatePieceScaleOnBoard();
        foreach (var proxyTarget in _outlineProxyTargets)
        {
            var proxyRenderer = proxyTarget.Key;
            var grooveRect = proxyTarget.Value;
            if (proxyRenderer == null || grooveRect == null)
            {
                continue;
            }

            proxyRenderer.transform.localScale = boardScale;
            proxyRenderer.transform.position = GetActiveGroupOutlinePosition(grooveRect, camera);
        }
    }

    private void InitializeGameplay(int bagId)
    {
        GameManager.SetBagId(bagId);
        EnsureCardBagLoaded(bagId);
        EnsureBoardAndGroovesInitialized();
        if (_board.GameBoardImage == null)
        {
            Debug.LogWarning(
                $"GameBoard not found. Expected {GameDefine.FormatCardBagPrefabResourcesPath(bagId)} " +
                $"to contain an object named {GameDefine.GameBoardObjectName}.");
            return;
        }

        if (_board.GrooveImagesByGroup == null || _board.GrooveImagesByGroup.Count == 0)
        {
            Debug.LogWarning("GameScene: no editor groove images found. Expected objects named Piece01, Piece02, ...");
            return;
        }

        EnsureBackgroundCentered();
        EnsurePieceBoardInitialized();
        if (_board.PieceBoardRect == null)
        {
            _board.PieceBgRenderer = CreatePieceBackground();
        }

        CreateDraggableGroup(0);
        Debug.Log(
            $"GameScene ready. BagId={bagId}, Groups={_board.GrooveImagesByGroup.Count}, " +
            $"Pieces={CountGrooveImages(_board.GrooveImagesByGroup)}");
    }

    private void EnsureCardBagLoaded(int bagId)
    {
        if (_loadedCardBagRoot != null)
        {
            return;
        }

        var resourcePath = GameDefine.FormatCardBagPrefabResourcesPath(bagId);
        var prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            Debug.LogWarning($"GameScene: card bag prefab not found at Resources/{resourcePath}.");
            return;
        }

        var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
        var parent = canvas != null ? canvas.transform : null;
        _loadedCardBagRoot = Instantiate(prefab, parent, false);
        _loadedCardBagRoot.name = prefab.name;

        var rectTransform = _loadedCardBagRoot.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            _loadedCardBagRect = rectTransform;
            PlaceCardBagAfterBackground(rectTransform);
            _originalCardBagAnchoredPosition = rectTransform.anchoredPosition;
            _hasOriginalCardBagAnchoredPosition = true;
        }

        _board.IsBoardAndGroovesInitialized = false;
        Debug.Log($"GameScene: loaded card bag prefab Resources/{resourcePath}.");
    }

    private static void PlaceCardBagAfterBackground(RectTransform cardBagRect)
    {
        var background = GameObject.Find(GameDefine.BackgroundObjectName);
        if (background == null || background.transform.parent != cardBagRect.parent)
        {
            cardBagRect.SetAsFirstSibling();
            return;
        }

        cardBagRect.SetSiblingIndex(background.transform.GetSiblingIndex() + 1);
    }

    private void ConfigureGameplayCanvas(Camera camera)
    {
        var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        if (camera != null && canvas.transform.parent == camera.transform)
        {
            canvas.transform.SetParent(null, worldPositionStays: true);
        }

        var canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            canvasRect.localScale = Vector3.one;
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;
            canvasRect.anchoredPosition = Vector2.zero;
            canvasRect.pivot = new Vector2(0.5f, 0.5f);
        }

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceHeight * (16f / 9f), ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = PixelsPerUnit;
        }

        if (camera != null)
        {
            GameCommonUtility.ConfigureCanvasForGameplay(
                canvas,
                camera,
                ReferenceHeight * (16f / 9f),
                ReferenceHeight,
                PixelsPerUnit);
        }

        Canvas.ForceUpdateCanvases();
    }

    private void EnsureBoardAndGroovesInitialized()
    {
        if (_board.IsBoardAndGroovesInitialized)
        {
            return;
        }

        _board.BackgroundRect = FindBackgroundRect();
        _board.PieceBoardRect = FindPieceBoardRect();
        _board.GameBoardImage = FindSceneImage(GameDefine.GameBoardObjectName);
        if (_board.GameBoardImage == null)
        {
            return;
        }

        if (_board.GameBoardImage != null && !_hasOriginalGameBoardAnchoredPosition)
        {
            _originalGameBoardAnchoredPosition = _board.GameBoardImage.rectTransform.anchoredPosition;
            _hasOriginalGameBoardAnchoredPosition = true;
        }

        _board.GrooveImagesByGroup = CollectEditorGrooveGroups();
        SyncEditorLayoutToSprites();
        _board.IsBoardAndGroovesInitialized = true;
    }

    private static RectTransform FindBackgroundRect()
    {
        var background = GameObject.Find(GameDefine.BackgroundObjectName);
        return background != null ? background.GetComponent<RectTransform>() : null;
    }

    private static RectTransform FindPieceBoardRect()
    {
        var pieceBoard = GameObject.Find(GameDefine.PieceBoardObjectName);
        return pieceBoard != null ? pieceBoard.GetComponent<RectTransform>() : null;
    }

    private void EnsurePieceBoardInitialized()
    {
        if (_board.PieceBoardRect == null)
        {
            _board.PieceBoardRect = FindPieceBoardRect();
        }

        if (_board.PieceBoardRect == null || _hasPieceBoardOriginalAnchoredPosition)
        {
            return;
        }

        _pieceBoardOriginalAnchoredPosition = _board.PieceBoardRect.anchoredPosition;
        _hasPieceBoardOriginalAnchoredPosition = true;
        _isPieceBoardHidden = false;
    }

    private static Image FindSceneImage(string objectName)
    {
        var sceneObject = GameObject.Find(objectName);
        return sceneObject != null ? sceneObject.GetComponent<Image>() : null;
    }

    private static List<List<Image>> CollectEditorGrooveGroups()
    {
        var sortedGrooves = CollectSortedEditorPieceGrooves();
        for (var i = 0; i < sortedGrooves.Count; i++)
        {
            SetImageAlpha(sortedGrooves[i], 0f);
        }

        if (sortedGrooves.Count == 0)
        {
            return new List<List<Image>>();
        }

        return SplitGroovesIntoNumberedGroups(sortedGrooves);
    }

    private static List<List<Image>> SplitGroovesIntoNumberedGroups(List<Image> sortedGrooves)
    {
        var groupsByNumber = new SortedDictionary<int, List<Image>>();
        for (var i = 0; i < sortedGrooves.Count; i++)
        {
            var grooveImage = sortedGrooves[i];
            if (grooveImage == null || !TryGetNumberedGroup(grooveImage, out var groupNumber))
            {
                continue;
            }

            if (!groupsByNumber.TryGetValue(groupNumber, out var group))
            {
                group = new List<Image>();
                groupsByNumber[groupNumber] = group;
            }

            group.Add(grooveImage);
        }

        var groups = new List<List<Image>>();
        foreach (var group in groupsByNumber.Values)
        {
            group.Sort((a, b) => GetPieceNumberFromImage(a).CompareTo(GetPieceNumberFromImage(b)));
            groups.Add(group);
        }

        return groups;
    }

    private static int GetPieceNumberFromImage(Image image)
    {
        if (image == null || !TryParsePieceObjectName(image.gameObject.name, out var pieceNumber))
        {
            return int.MaxValue;
        }

        return pieceNumber;
    }

    private static int GetPieceNumberFromState(DraggablePieceState state)
    {
        if (state?.GrooveImage == null)
        {
            return int.MaxValue;
        }

        return GetPieceNumberFromImage(state.GrooveImage);
    }

    private static bool TryGetNumberedGroup(Image image, out int groupNumber)
    {
        groupNumber = 0;
        if (image == null || !TryParsePieceObjectName(image.gameObject.name, out var pieceNumber))
        {
            return false;
        }

        groupNumber = pieceNumber / 10;
        return groupNumber > 0;
    }

    private static List<Image> CollectSortedEditorPieceGrooves()
    {
        var groovesByNumber = new Dictionary<int, Image>();
        var images = UnityEngine.Object.FindObjectsOfType<Image>(true);
        for (var i = 0; i < images.Length; i++)
        {
            var image = images[i];
            if (image == null || !TryParsePieceObjectName(image.gameObject.name, out var pieceNumber))
            {
                continue;
            }

            groovesByNumber[pieceNumber] = image;
        }

        var numbers = new List<int>(groovesByNumber.Keys);
        numbers.Sort();
        var sortedGrooves = new List<Image>(numbers.Count);
        for (var i = 0; i < numbers.Count; i++)
        {
            sortedGrooves.Add(groovesByNumber[numbers[i]]);
        }

        return sortedGrooves;
    }

    private static bool TryParsePieceObjectName(string objectName, out int pieceNumber)
    {
        pieceNumber = 0;
        if (string.IsNullOrWhiteSpace(objectName)
            || !objectName.StartsWith(GameDefine.PieceObjectPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var numberText = objectName.Substring(GameDefine.PieceObjectPrefix.Length);
        return int.TryParse(numberText, out pieceNumber) && pieceNumber > 0;
    }

    private void SyncEditorLayoutToSprites()
    {
        SyncImageSizeToSprite(_board.GameBoardImage);
        if (_board.GrooveImagesByGroup == null)
        {
            return;
        }

        for (var groupIndex = 0; groupIndex < _board.GrooveImagesByGroup.Count; groupIndex++)
        {
            var group = _board.GrooveImagesByGroup[groupIndex];
            if (group == null)
            {
                continue;
            }

            for (var i = 0; i < group.Count; i++)
            {
                SyncImageSizeToSprite(group[i]);
            }
        }
    }

    private static void SyncImageSizeToSprite(Image image)
    {
        if (image == null || image.sprite == null)
        {
            return;
        }

        var rectTransform = image.rectTransform;
        rectTransform.sizeDelta = image.sprite.rect.size;
    }

    private Vector2 GetBoardDisplayWorldSize()
    {
        if (_board.GameBoardImage == null)
        {
            return new Vector2(18f, 18.6f);
        }

        var camera = Camera.main;
        if (camera == null)
        {
            return GameCommonUtility.GetRectTransformWorldBounds(_board.GameBoardImage.rectTransform).size;
        }

        return GameCommonUtility.GetRectTransformCameraWorldBounds(
            _board.GameBoardImage.rectTransform,
            camera).size;
    }

    /// <summary>
    /// 用途：计算 GameBoard 在场景中的显示缩放与贴图 PPU 世界尺寸之间的比例。返回：统一缩放系数。
    /// </summary>
    private float GetBoardToSpriteScaleFactor()
    {
        if (_board.GameBoardImage == null || _board.GameBoardImage.sprite == null)
        {
            return 1f;
        }

        var boardDisplaySize = GetBoardDisplayWorldSize();
        var boardTextureWorldSize = _board.GameBoardImage.sprite.rect.size / PixelsPerUnit;
        if (boardTextureWorldSize.x <= 0.001f || boardTextureWorldSize.y <= 0.001f)
        {
            return 1f;
        }

        var factorX = boardDisplaySize.x / boardTextureWorldSize.x;
        var factorY = boardDisplaySize.y / boardTextureWorldSize.y;
        return Mathf.Min(factorX, factorY);
    }

    private Vector3 CalculatePieceScaleOnBoard()
    {
        return Vector3.one * GetBoardToSpriteScaleFactor();
    }

    private Vector3 CalculateTrayScaleForPiece(SpriteRenderer pieceRenderer, Bounds hostBounds)
    {
        var boardScale = CalculatePieceScaleOnBoard();
        if (pieceRenderer == null || pieceRenderer.sprite == null)
        {
            return boardScale;
        }

        var scaledHeight = pieceRenderer.sprite.bounds.size.y * boardScale.y;
        var maxHeight = Mathf.Max(0.0001f, hostBounds.size.y * PieceTrayMaxHeightRatio);
        if (scaledHeight <= maxHeight)
        {
            return boardScale;
        }

        return boardScale * (maxHeight / scaledHeight);
    }

    private SpriteRenderer CreatePieceBackground()
    {
        if (_board.GameBoardImage == null)
        {
            return null;
        }

        var camera = Camera.main;
        var boardWorldSize = GetBoardDisplayWorldSize();
        var boardWidth = boardWorldSize.x;
        var bgHeight = boardWorldSize.y * 0.25f;
        var boardBounds = camera != null
            ? GameCommonUtility.GetRectTransformCameraWorldBounds(_board.GameBoardImage.rectTransform, camera)
            : GameCommonUtility.GetRectTransformWorldBounds(_board.GameBoardImage.rectTransform);
        var bottomEdge = camera != null ? camera.transform.position.y - camera.orthographicSize : boardBounds.min.y;
        var bgCenterY = bottomEdge + bgHeight * 0.5f;
        var bgCenterX = camera != null ? camera.transform.position.x : boardBounds.center.x;
        var bgPosition = new Vector3(bgCenterX, bgCenterY, WorldGameplayDepth);

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
            renderer.sprite = GameCommonUtility.CreateSolidSprite(Color.white, PixelsPerUnit);
        }

        var slicedSprite = GameCommonUtility.CreateSlicedSpriteByPath(PieceBgPath, PixelsPerUnit, renderer.sprite);
        if (slicedSprite != null)
        {
            renderer.sprite = slicedSprite;
        }

        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(boardWidth, bgHeight);
        renderer.transform.position = bgPosition;
        renderer.color = new Color(0f, 0f, 0f, PieceBgAlpha);
        CreateOrUpdatePieceBgFill(renderer);
        CachePieceBgOriginalPosition();
        return renderer;
    }

    private void CreateDraggableGroup(int groupIndex)
    {
        if (groupIndex > 0)
        {
            FinalizeCompletedGroup(groupIndex - 1);
        }

        PreparePieceTrayForGroupStart();
        ClearCurrentDraggableGroup();
        _drag.CurrentGroupIndex = groupIndex;

        if (_board.GrooveImagesByGroup == null
            || groupIndex < 0
            || groupIndex >= _board.GrooveImagesByGroup.Count)
        {
            return;
        }

        var grooveGroup = _board.GrooveImagesByGroup[groupIndex];
        if (grooveGroup == null || grooveGroup.Count == 0)
        {
            return;
        }

        UpdateGrooveGroupVisibility(groupIndex);
        ResetBoardPanState();
        FitCameraToActiveGroup(groupIndex);

        var root = new GameObject(DraggableGroupRootObjectName);
        var hostBounds = GetPieceTrayBounds();

        for (var i = 0; i < grooveGroup.Count; i++)
        {
            var grooveImage = grooveGroup[i];
            if (grooveImage == null || grooveImage.sprite == null)
            {
                Debug.LogWarning($"GameScene: groove image missing sprite at index {i}.");
                continue;
            }

            var pieceRenderer = CreateDraggablePieceFromGroove(
                grooveImage,
                $"DraggablePiece_{groupIndex}_{i}",
                root.transform);
            if (pieceRenderer == null)
            {
                continue;
            }

            var trayScale = CalculateTrayScaleForPiece(pieceRenderer, hostBounds);
            var dragScale = CalculatePieceScaleOnBoard();
            pieceRenderer.transform.localScale = trayScale;
            _drag.CurrentGroupDraggables.Add(new DraggablePieceState
            {
                PieceRenderer = pieceRenderer,
                GrooveImage = grooveImage,
                GrooveRect = grooveImage.rectTransform,
                StartPosition = pieceRenderer.transform.position,
                TrayScale = trayScale,
                DragScale = dragScale,
                IsPlaced = false
            });
        }

        LayoutTrayPieces();
        CachePieceTrayOriginalPosition();
        TryRefreshActiveGroupOutline(groupIndex);
    }

    private static SpriteRenderer CreateDraggablePieceFromGroove(Image grooveImage, string objectName, Transform parent)
    {
        return GameCommonUtility.CreateSpriteRendererFromSprite(
            objectName,
            grooveImage.sprite,
            PieceSortingOrder,
            parent,
            forceCreate: true);
    }

    private void UpdateGrooveGroupVisibility(int activeGroupIndex)
    {
        if (_board.GrooveImagesByGroup == null)
        {
            return;
        }

        for (var groupIndex = 0; groupIndex < _board.GrooveImagesByGroup.Count; groupIndex++)
        {
            var group = _board.GrooveImagesByGroup[groupIndex];
            if (group == null)
            {
                continue;
            }

            var isActiveGroup = groupIndex == activeGroupIndex;
            var isCompletedGroup = groupIndex < activeGroupIndex;
            for (var i = 0; i < group.Count; i++)
            {
                var grooveImage = group[i];
                if (grooveImage == null)
                {
                    continue;
                }

                if (isCompletedGroup)
                {
                    grooveImage.gameObject.SetActive(true);
                    SetImageAlpha(grooveImage, 1f);
                }
                else if (isActiveGroup)
                {
                    grooveImage.gameObject.SetActive(true);
                    SetImageAlpha(grooveImage, 0f);
                }
                else
                {
                    grooveImage.gameObject.SetActive(false);
                }
            }
        }
    }

    private void ClearCurrentDraggableGroup()
    {
        _drag.DraggingPiece = null;
        _drag.CurrentGroupDraggables.Clear();
        ClearActiveGroupOutline();

        var root = GameObject.Find(DraggableGroupRootObjectName);
        if (root != null)
        {
            Destroy(root);
        }
    }

    private void TryRefreshActiveGroupOutline(int groupIndex)
    {
        try
        {
            RefreshActiveGroupOutline(groupIndex);
        }
        catch (Exception exception)
        {
            ClearActiveGroupOutline();
            Debug.LogWarning($"GameScene: failed to create active group outline. {exception.Message}");
        }
    }

    private void RefreshActiveGroupOutline(int groupIndex)
    {
        ClearActiveGroupOutline();
        if (_board.GrooveImagesByGroup == null
            || groupIndex < 0
            || groupIndex >= _board.GrooveImagesByGroup.Count)
        {
            return;
        }

        var grooveGroup = _board.GrooveImagesByGroup[groupIndex];
        if (grooveGroup == null || grooveGroup.Count == 0)
        {
            return;
        }

        if (TryCreateBakedActiveGroupOutline(groupIndex))
        {
            return;
        }

        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        var root = new GameObject(ActiveGroupOutlineRootObjectName);
        var boardScale = CalculatePieceScaleOnBoard();
        var activeProxyCount = 0;
        for (var i = 0; i < grooveGroup.Count; i++)
        {
            if (CreateActiveGroupOutlineProxy(
                    grooveGroup[i],
                    $"ActiveGroupOutlineProxy_{groupIndex}_{i}",
                    root.transform,
                    camera,
                    boardScale))
            {
                activeProxyCount++;
            }
        }

        if (activeProxyCount == 0)
        {
            root.SetActive(false);
            Destroy(root);
            return;
        }

        for (var otherGroupIndex = 0;
             otherGroupIndex < _board.GrooveImagesByGroup.Count;
             otherGroupIndex++)
        {
            if (otherGroupIndex == groupIndex)
            {
                continue;
            }

            var blockerGroup = _board.GrooveImagesByGroup[otherGroupIndex];
            if (blockerGroup == null)
            {
                continue;
            }

            for (var i = 0; i < blockerGroup.Count; i++)
            {
                CreateActiveGroupOutlineBlocker(
                    blockerGroup[i],
                    $"ActiveGroupOutlineBlocker_{otherGroupIndex}_{i}",
                    root.transform,
                    camera,
                    boardScale);
            }
        }
    }

    private bool TryCreateBakedActiveGroupOutline(int groupIndex)
    {
        if (_board.GameBoardImage == null
            || _board.GrooveImagesByGroup == null
            || groupIndex < 0
            || groupIndex >= _board.GrooveImagesByGroup.Count)
        {
            return false;
        }

        var grooveGroup = _board.GrooveImagesByGroup[groupIndex];
        if (grooveGroup == null
            || grooveGroup.Count == 0
            || !TryGetNumberedGroup(grooveGroup[0], out var groupNumber))
        {
            return false;
        }

        var resourcePath = GameDefine.FormatPuzzleOutlineResourcesPath(
            GameManager.GetBagId(),
            groupNumber);
        var outlineSprite = Resources.Load<Sprite>(resourcePath);
        if (outlineSprite == null)
        {
            return false;
        }

        var outlineObject = new GameObject(
            ActiveGroupOutlineRootObjectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        var outlineRect = outlineObject.GetComponent<RectTransform>();
        outlineRect.SetParent(_board.GameBoardImage.rectTransform, false);
        outlineRect.anchorMin = Vector2.zero;
        outlineRect.anchorMax = Vector2.one;
        outlineRect.pivot = new Vector2(0.5f, 0.5f);
        outlineRect.anchoredPosition = Vector2.zero;
        outlineRect.offsetMin = Vector2.zero;
        outlineRect.offsetMax = Vector2.zero;
        outlineRect.localScale = Vector3.one;

        var outlineImage = outlineObject.GetComponent<Image>();
        outlineImage.sprite = outlineSprite;
        outlineImage.color = Color.white;
        outlineImage.raycastTarget = false;
        outlineImage.maskable = false;
        outlineImage.preserveAspect = false;
        return true;
    }

    private bool CreateActiveGroupOutlineProxy(
        Image grooveImage,
        string objectName,
        Transform parent,
        Camera camera,
        Vector3 boardScale)
    {
        var proxyRenderer = CreateActiveGroupOutlineRenderer(
            grooveImage,
            objectName,
            parent,
            camera,
            boardScale,
            ActiveGroupOutlineSortingOrder);
        if (proxyRenderer == null)
        {
            return false;
        }

        var outline = proxyRenderer.gameObject.AddComponent<global::OutlineFx.OutlineFx>();
        outline.Color = ActiveGroupOutlineColor;
        return true;
    }

    private void CreateActiveGroupOutlineBlocker(
        Image grooveImage,
        string objectName,
        Transform parent,
        Camera camera,
        Vector3 boardScale)
    {
        var proxyRenderer = CreateActiveGroupOutlineRenderer(
            grooveImage,
            objectName,
            parent,
            camera,
            boardScale,
            ActiveGroupOutlineSortingOrder + 1);
        if (proxyRenderer != null)
        {
            proxyRenderer.gameObject.AddComponent<global::OutlineFx.OutlineBlocker>();
        }
    }

    private SpriteRenderer CreateActiveGroupOutlineRenderer(
        Image grooveImage,
        string objectName,
        Transform parent,
        Camera camera,
        Vector3 boardScale,
        int sortingOrder)
    {
        if (grooveImage == null || grooveImage.sprite == null)
        {
            return null;
        }

        var proxyRenderer = CreateDraggablePieceFromGroove(grooveImage, objectName, parent);
        if (proxyRenderer == null)
        {
            return null;
        }

        proxyRenderer.sortingOrder = sortingOrder;
        proxyRenderer.color = new Color(1f, 1f, 1f, 0f);
        proxyRenderer.transform.localScale = boardScale;
        proxyRenderer.transform.position = GetActiveGroupOutlinePosition(grooveImage.rectTransform, camera);
        _outlineProxyTargets[proxyRenderer] = grooveImage.rectTransform;
        return proxyRenderer;
    }

    private void ClearActiveGroupOutline()
    {
        _outlineProxyTargets.Clear();
        var root = GameObject.Find(ActiveGroupOutlineRootObjectName);
        if (root != null)
        {
            root.SetActive(false);
            Destroy(root);
        }
    }

    private void OnPointerEnd(Vector2 screenPosition)
    {
        EndDragging();
    }

    private void TryBeginDrag(Vector2 screenPosition)
    {
        if (_isGameFinished)
        {
            return;
        }

        if (_drag.DraggingPiece != null)
        {
            return;
        }

        var world = ToGameplayWorld(screenPosition);
        for (var i = _drag.CurrentGroupDraggables.Count - 1; i >= 0; i--)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null || state.IsPlaced || state.PieceRenderer == null)
            {
                continue;
            }

            if (!ContainsWorldXY(state.PieceRenderer.bounds, world))
            {
                continue;
            }

            _drag.DraggingPiece = state;
            _drag.DragOffset = state.PieceRenderer.transform.position - world;
            state.DragScale = CalculatePieceScaleOnBoard();
            state.PieceRenderer.transform.localScale = state.DragScale;
            state.PieceRenderer.sortingOrder = PieceSortingOrder + 100;
            if (CountUnplacedTrayPieces() == 1)
            {
                SlidePieceTrayOutOfScreen();
            }

            break;
        }
    }

    private void UpdateDragging(Vector2 screenPosition)
    {
        if (_drag.DraggingPiece == null || _drag.DraggingPiece.PieceRenderer == null)
        {
            return;
        }

        var world = ToGameplayWorld(screenPosition);
        _drag.DraggingPiece.PieceRenderer.transform.position = new Vector3(
            world.x + _drag.DragOffset.x,
            world.y + _drag.DragOffset.y,
            WorldGameplayDepth);
    }

    private void EndDragging()
    {
        if (_drag.DraggingPiece == null || _drag.DraggingPiece.PieceRenderer == null)
        {
            return;
        }

        var state = _drag.DraggingPiece;
        _drag.DraggingPiece = null;
        state.PieceRenderer.sortingOrder = PieceSortingOrder;

        var groovePosition = GetGrooveSnapPosition(state.GrooveRect, Camera.main);
        if (state.GrooveRect != null
            && Vector3.Distance(state.PieceRenderer.transform.position, groovePosition) <= CalculateSnapDistance(state))
        {
            state.PieceRenderer.transform.position = groovePosition;
            state.PieceRenderer.transform.localScale = state.DragScale;
            var placedRoot = GetOrCreatePlacedPiecesRoot();
            state.PieceRenderer.transform.SetParent(placedRoot.transform, worldPositionStays: true);
            state.IsPlaced = true;
            TryIncrementCollectPuzzleTaskProgress();
            LayoutTrayPieces();
            TryAdvanceGroup();
            return;
        }

        state.PieceRenderer.transform.position = state.StartPosition;
        state.PieceRenderer.transform.localScale = state.TrayScale;
        SlidePieceTrayToOriginalPosition();
    }

    private int CountUnplacedTrayPieces()
    {
        var count = 0;
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state != null && !state.IsPlaced)
            {
                count++;
            }
        }

        return count;
    }

    private Bounds GetPieceTrayBounds()
    {
        var camera = Camera.main;
        if (_board.PieceBoardRect != null && camera != null)
        {
            return GameCommonUtility.GetRectTransformCameraWorldBounds(
                _board.PieceBoardRect,
                camera,
                WorldGameplayDepth);
        }

        if (_board.PieceBgRenderer != null)
        {
            return _board.PieceBgRenderer.bounds;
        }

        if (_board.GameBoardImage != null && camera != null)
        {
            return GameCommonUtility.GetRectTransformCameraWorldBounds(_board.GameBoardImage.rectTransform, camera);
        }

        return new Bounds(Vector3.zero, Vector3.one);
    }

    private void LayoutTrayPieces()
    {
        Canvas.ForceUpdateCanvases();
        var hostBounds = GetPieceTrayBounds();
        if (hostBounds.size.sqrMagnitude <= 0f)
        {
            return;
        }

        var horizontalSpacing = DraggableHorizontalSpacingPixels / PixelsPerUnit;
        var nextCenterX = hostBounds.min.x + DraggableLeftPadding;
        var unplaced = new List<DraggablePieceState>();
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null || state.IsPlaced || state.PieceRenderer == null || state == _drag.DraggingPiece)
            {
                continue;
            }

            unplaced.Add(state);
        }

        unplaced.Sort((a, b) => GetPieceNumberFromState(a).CompareTo(GetPieceNumberFromState(b)));
        var trayCenterY = hostBounds.center.y;
        for (var i = 0; i < unplaced.Count; i++)
        {
            var state = unplaced[i];
            var pieceWidth = GameCommonUtility.GetPieceWidth(state.PieceRenderer, state.TrayScale);
            var pieceHalfWidth = pieceWidth * 0.5f;
            var pieceCenterX = nextCenterX + pieceHalfWidth;
            var position = PlaceTrayPieceAt(state.PieceRenderer, state.TrayScale, pieceCenterX, trayCenterY);
            state.StartPosition = position;
            nextCenterX = pieceCenterX + pieceHalfWidth + horizontalSpacing;
        }
    }

    private static Vector3 PlaceTrayPieceAt(
        SpriteRenderer renderer,
        Vector3 trayScale,
        float centerX,
        float trayCenterY)
    {
        renderer.transform.localScale = trayScale;
        renderer.transform.position = new Vector3(centerX, trayCenterY, WorldGameplayDepth);
        var deltaY = trayCenterY - renderer.bounds.center.y;
        if (Mathf.Abs(deltaY) > 0.0001f)
        {
            var position = renderer.transform.position;
            position.y += deltaY;
            renderer.transform.position = position;
        }

        return renderer.transform.position;
    }

    private static Vector3 GetGrooveSnapPosition(RectTransform grooveRect, Camera camera)
    {
        if (grooveRect == null)
        {
            return Vector3.zero;
        }

        if (camera == null)
        {
            var position = grooveRect.position;
            position.z = WorldGameplayDepth;
            return position;
        }

        var worldPosition = GameCommonUtility.RectTransformToCameraWorld(
            grooveRect,
            camera,
            WorldGameplayDepth);
        worldPosition.z = WorldGameplayDepth;
        return worldPosition;
    }

    private static Vector3 GetActiveGroupOutlinePosition(RectTransform grooveRect, Camera camera)
    {
        var worldPosition = GetGrooveSnapPosition(grooveRect, camera);
        if (camera == null)
        {
            return worldPosition;
        }

        var distance = Mathf.Abs(camera.transform.position.z - WorldGameplayDepth);
        var screenOrigin = camera.ScreenToWorldPoint(new Vector3(0f, 0f, distance));
        var screenOffset = camera.ScreenToWorldPoint(new Vector3(
            ActiveGroupOutlineScreenOffset.x,
            ActiveGroupOutlineScreenOffset.y,
            distance));
        worldPosition += screenOffset - screenOrigin;
        worldPosition.z = WorldGameplayDepth;
        return worldPosition;
    }

    private static float CalculateSnapDistance(DraggablePieceState state)
    {
        if (state == null)
        {
            return SnapDistanceMin;
        }

        var referenceSize = 0f;
        if (state.GrooveRect != null)
        {
            var grooveBounds = GameCommonUtility.GetRectTransformCameraWorldBounds(
                state.GrooveRect,
                Camera.main,
                WorldGameplayDepth);
            referenceSize = Mathf.Max(grooveBounds.size.x, grooveBounds.size.y);
        }

        if (referenceSize <= 0f && state.PieceRenderer != null)
        {
            referenceSize = Mathf.Max(state.PieceRenderer.bounds.size.x, state.PieceRenderer.bounds.size.y);
        }

        if (referenceSize <= 0f)
        {
            return SnapDistanceMin;
        }

        var adaptiveDistance = referenceSize * SnapDistanceSizeRatio;
        return Mathf.Clamp(adaptiveDistance, SnapDistanceMin, SnapDistanceMax);
    }

    private void TryAdvanceGroup()
    {
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            if (!_drag.CurrentGroupDraggables[i].IsPlaced)
            {
                return;
            }
        }

        var nextGroupIndex = _drag.CurrentGroupIndex + 1;
        if (_board.GrooveImagesByGroup != null && nextGroupIndex < _board.GrooveImagesByGroup.Count)
        {
            CreateDraggableGroup(nextGroupIndex);
            return;
        }

        ShowRewardPanel();
    }

    private void ConfigureRewardPanel()
    {
        _rewardPanelRoot = GameCommonUtility.FindSceneObject(GameDefine.RewardPanelObjectName);
        if (_rewardPanelRoot == null)
        {
            Debug.LogWarning($"GameScene: reward panel not found. Expected object named {GameDefine.RewardPanelObjectName}.");
            return;
        }

        _rewardPanelRoot.SetActive(false);
        _isGameFinished = false;

        var finishButtonObject = GameCommonUtility.FindSceneObject(GameDefine.FinishButtonObjectName);
        if (finishButtonObject == null)
        {
            Debug.LogWarning($"GameScene: finish button not found. Expected object named {GameDefine.FinishButtonObjectName}.");
            return;
        }

        var button = finishButtonObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"GameScene: {GameDefine.FinishButtonObjectName} is missing Button component.");
            return;
        }

        button.onClick.RemoveListener(OnFinishButtonClicked);
        button.onClick.AddListener(OnFinishButtonClicked);
    }

    private void ShowRewardPanel()
    {
        if (_isGameFinished)
        {
            return;
        }

        _isGameFinished = true;
        EndDragging();

        if (_rewardPanelRoot == null)
        {
            _rewardPanelRoot = GameCommonUtility.FindSceneObject(GameDefine.RewardPanelObjectName);
        }

        if (_rewardPanelRoot == null)
        {
            Debug.LogWarning($"GameScene: cannot show reward panel. Expected object named {GameDefine.RewardPanelObjectName}.");
            return;
        }

        PrepareBoardForRewardPanel();
        SaveCardPackAfterPuzzleComplete();
        ProcessTaskSettlement();
        _rewardPanelRoot.SetActive(true);
        _rewardPanelRoot.transform.SetAsLastSibling();
        Debug.Log("GameScene: puzzle completed, RewardPanel shown.");
    }

    private void PrepareBoardForRewardPanel()
    {
        RemoveRuntimePuzzlePieces();
        RevealAllGroovesOnBoard();
    }

    private void FinalizeCompletedGroup(int completedGroupIndex)
    {
        RemovePlacedPiecesForGroup(completedGroupIndex);
    }

    private void RemovePlacedPiecesForGroup(int groupIndex)
    {
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null || !state.IsPlaced || state.PieceRenderer == null)
            {
                continue;
            }

            Destroy(state.PieceRenderer.gameObject);
            state.PieceRenderer = null;
        }

        var placedRoot = GameObject.Find(PlacedPiecesRootObjectName);
        if (placedRoot == null)
        {
            return;
        }

        var pieceNamePrefix = $"DraggablePiece_{groupIndex}_";
        for (var childIndex = placedRoot.transform.childCount - 1; childIndex >= 0; childIndex--)
        {
            var child = placedRoot.transform.GetChild(childIndex);
            if (child.name.StartsWith(pieceNamePrefix, StringComparison.Ordinal))
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void RevealGroovesForGroup(int groupIndex)
    {
        if (_board.GrooveImagesByGroup == null
            || groupIndex < 0
            || groupIndex >= _board.GrooveImagesByGroup.Count)
        {
            return;
        }

        var group = _board.GrooveImagesByGroup[groupIndex];
        if (group == null)
        {
            return;
        }

        for (var i = 0; i < group.Count; i++)
        {
            var grooveImage = group[i];
            if (grooveImage == null)
            {
                continue;
            }

            grooveImage.gameObject.SetActive(true);
            SetImageAlpha(grooveImage, 1f);
        }
    }

    private void RemoveRuntimePuzzlePieces()
    {
        _drag.DraggingPiece = null;
        _drag.CurrentGroupDraggables.Clear();
        ClearActiveGroupOutline();

        var draggableRoot = GameObject.Find(DraggableGroupRootObjectName);
        if (draggableRoot != null)
        {
            Destroy(draggableRoot);
        }

        var placedRoot = GameObject.Find(PlacedPiecesRootObjectName);
        if (placedRoot != null)
        {
            Destroy(placedRoot);
        }

        if (_board.PieceBgRenderer != null)
        {
            _board.PieceBgRenderer.gameObject.SetActive(false);
        }

        if (_board.PieceBoardRect != null)
        {
            _board.PieceBoardRect.gameObject.SetActive(false);
        }

        var fillTransform = GetPieceBgFillTransform();
        if (fillTransform != null)
        {
            fillTransform.gameObject.SetActive(false);
        }
    }

    private void RevealAllGroovesOnBoard()
    {
        if (_board.GrooveImagesByGroup == null)
        {
            return;
        }

        for (var groupIndex = 0; groupIndex < _board.GrooveImagesByGroup.Count; groupIndex++)
        {
            RevealGroovesForGroup(groupIndex);
        }
    }

    private void OnFinishButtonClicked()
    {
        GameManager.EnterMainScene();
    }

    private void InitializeTaskTracking()
    {
        GameTaskUtility.Initialize();
        _isCollectPuzzleTaskActive = GameTaskUtility.IsCurrentTaskCollectPuzzle();
        if (_isCollectPuzzleTaskActive)
        {
            Debug.Log(
                $"GameScene: CollectPuzzle task active. taskId={GameTaskUtility.GetCurrentTaskId()}, " +
                $"progress={GameTaskUtility.GetCurrentCompleteValue()}");
        }
    }

    private void TryIncrementCollectPuzzleTaskProgress()
    {
        if (!_isCollectPuzzleTaskActive)
        {
            return;
        }

        if (GameTaskUtility.AddCurrentCompleteValue(1))
        {
            Debug.Log(
                $"GameScene: puzzle piece collected. progress={GameTaskUtility.GetCurrentCompleteValue()}");
        }
    }

    private void SaveCardPackAfterPuzzleComplete()
    {
        var packId = GameManager.GetBagId();
        if (packId <= 0)
        {
            return;
        }

        if (!CardPackDataUtility.TrySavePackAfterPuzzleComplete(packId))
        {
            Debug.LogWarning($"GameScene: failed to save card pack data after puzzle complete. packId={packId}");
            return;
        }

        Debug.Log($"GameScene: card pack data saved after puzzle complete. packId={packId}");
    }

    private void ProcessTaskSettlement()
    {
        if (_rewardPanelRoot == null)
        {
            return;
        }

        if (!GameTaskUtility.IsCurrentTaskCompleted()
            || !GameTaskUtility.TryGetCurrentTaskConfig(out var taskConfig))
        {
            SetTaskRewardSectionVisible(false);
            return;
        }

        OutputTaskReward(taskConfig);
        if (GameTaskUtility.TryCompleteAndAdvanceTask())
        {
            _isCollectPuzzleTaskActive = GameTaskUtility.IsCurrentTaskCollectPuzzle();
            Debug.Log($"GameScene: task advanced. nextTaskId={GameTaskUtility.GetCurrentTaskId()}");
        }
    }

    private void OutputTaskReward(TaskConfigData taskConfig)
    {
        SetTaskRewardSectionVisible(true);
        UpdateTaskRewardPanel(taskConfig);

        var rewardPackId = taskConfig.RewardId > 0 ? taskConfig.RewardId : GameDefine.DefaultBagId;
        var rewardValue = taskConfig.RewardValue > 0 ? taskConfig.RewardValue : 1;
        Debug.Log(
            $"GameScene: task reward granted. type={taskConfig.RewardType}, " +
            $"rewardId={rewardPackId}, rewardValue={rewardValue}");

        if (taskConfig.RewardType == RewardType.CardPack)
        {
            if (!CardPackDataUtility.TryUnlockPackFromTaskReward(rewardPackId))
            {
                Debug.LogWarning($"GameScene: failed to unlock task reward card pack. packId={rewardPackId}");
            }

            PlayTaskCardPackReward(rewardPackId);
        }
    }

    private void SetTaskRewardSectionVisible(bool visible)
    {
        if (_rewardPanelRoot == null)
        {
            return;
        }

        var taskBg = _rewardPanelRoot.transform.Find(TaskBg1ObjectName);
        if (taskBg != null)
        {
            taskBg.gameObject.SetActive(visible);
        }
    }

    private void UpdateTaskRewardPanel(TaskConfigData taskConfig)
    {
        if (_rewardPanelRoot == null)
        {
            return;
        }

        var rewardPackId = taskConfig.RewardId > 0 ? taskConfig.RewardId : GameDefine.DefaultBagId;
        var rewardValue = taskConfig.RewardValue > 0 ? taskConfig.RewardValue : 1;
        var packImagePath = GameDefine.FormatPackImagePath(rewardPackId);
        var packSprite = GameCommonUtility.LoadSpriteByPath(packImagePath, PixelsPerUnit);

        var taskContentObject = GameCommonUtility.FindSceneObject(TaskContent1ObjectName);
        if (taskContentObject != null
            && taskContentObject.TryGetComponent(out TextMeshProUGUI taskContentText))
        {
            GameFontUtility.ApplyDefaultFont(taskContentText);
            taskContentText.text =
                $"完成收集拼图任务（{taskConfig.CompleteValue}/{taskConfig.CompleteValue}），获得卡包奖励！";
        }

        var bagIconObject = GameCommonUtility.FindSceneObject(TaskBagIconObjectName);
        if (bagIconObject != null && bagIconObject.TryGetComponent(out Image bagIconImage))
        {
            if (packSprite != null)
            {
                bagIconImage.sprite = packSprite;
            }
        }

        var rewardCountTransform = _rewardPanelRoot.transform.Find(TaskBagRewardCountPath);
        if (rewardCountTransform != null
            && rewardCountTransform.TryGetComponent(out TextMeshProUGUI rewardCountText))
        {
            GameFontUtility.ApplyDefaultFont(rewardCountText);
            rewardCountText.text = $"+{rewardValue}";
        }

        var imgBagTransform = _rewardPanelRoot.transform.Find(TaskRewardImgBagPath);
        if (imgBagTransform != null && imgBagTransform.TryGetComponent(out Image imgBagImage))
        {
            if (packSprite != null)
            {
                imgBagImage.sprite = packSprite;
            }
        }
    }

    private void PlayTaskCardPackReward(int rewardPackId)
    {
        Transform anchor = null;
        if (_rewardPanelRoot != null)
        {
            var imgBagTransform = _rewardPanelRoot.transform.Find(TaskRewardImgBagPath);
            if (imgBagTransform != null)
            {
                anchor = imgBagTransform;
            }
        }

        var canvas = _rewardPanelRoot != null ? _rewardPanelRoot.GetComponentInParent<Canvas>() : null;
        if (canvas != null && Camera.main != null)
        {
            GameCommonUtility.ConfigureCanvasForWorldCardPack(canvas, Camera.main);
        }

        var animationFileName = GameDefine.FormatCardPackAnimationFileName(rewardPackId);
        if (!GameAnimationUtility.PlayCardPackAnimation(animationFileName, anchor))
        {
            Debug.LogWarning($"GameScene: task reward card pack animation failed: {animationFileName}");
        }
    }

    private static GameObject GetOrCreatePlacedPiecesRoot()
    {
        var root = GameObject.Find(PlacedPiecesRootObjectName);
        if (root != null)
        {
            return root;
        }

        return new GameObject(PlacedPiecesRootObjectName);
    }

    private void FitCameraToActiveGroup(int activeGroupIndex)
    {
        var camera = Camera.main;
        if (camera == null || _board.GameBoardImage == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        GameCommonUtility.SetupOrthographicCamera(camera, ReferenceHeight, PixelsPerUnit);

        var activeGroupBounds = BuildActiveGroupBounds(activeGroupIndex);
        var pageBounds = BuildPageBoundsForActiveGroup(activeGroupBounds);
        if (pageBounds.size.sqrMagnitude <= 0f)
        {
            return;
        }

        GameCommonUtility.FitOrthographicCameraSizeOnly(camera, GamePageCameraPadding, pageBounds);
        Canvas.ForceUpdateCanvases();
        AlignPieceTrayToPageBottom();
        CenterCardBagOnActivePage(camera, activeGroupIndex);
    }

    private void CenterCardBagOnActivePage(Camera camera, int activeGroupIndex)
    {
        if (_loadedCardBagRect == null
            || !_hasOriginalCardBagAnchoredPosition
            || !TryBuildActiveGroupScreenRect(camera, activeGroupIndex, out var groupScreenRect)
            || !TryGetAvailableBoardScreenCenter(camera, out var targetScreenCenter))
        {
            return;
        }

        var parentRect = _loadedCardBagRect.parent as RectTransform;
        if (parentRect == null)
        {
            return;
        }

        var canvas = _loadedCardBagRect.GetComponentInParent<Canvas>();
        var eventCamera = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas != null ? canvas.worldCamera ?? camera : camera;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                groupScreenRect.center,
                eventCamera,
                out var groupLocalCenter)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                targetScreenCenter,
                eventCamera,
                out var targetLocalCenter))
        {
            return;
        }

        _loadedCardBagRect.anchoredPosition = _originalCardBagAnchoredPosition
            + targetLocalCenter
            - groupLocalCenter;
        Canvas.ForceUpdateCanvases();
    }

    private bool TryBuildActiveGroupScreenRect(
        Camera camera,
        int activeGroupIndex,
        out Rect screenRect)
    {
        screenRect = default;
        if (_board.GrooveImagesByGroup == null
            || activeGroupIndex < 0
            || activeGroupIndex >= _board.GrooveImagesByGroup.Count)
        {
            return false;
        }

        var group = _board.GrooveImagesByGroup[activeGroupIndex];
        var hasRect = false;
        if (group != null)
        {
            for (var i = 0; i < group.Count; i++)
            {
                var grooveImage = group[i];
                if (grooveImage == null || !grooveImage.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var grooveScreenRect = GetRectTransformScreenRect(grooveImage.rectTransform, camera);
                screenRect = hasRect ? UnionRects(screenRect, grooveScreenRect) : grooveScreenRect;
                hasRect = true;
            }
        }

        return hasRect;
    }

    private bool TryGetAvailableBoardScreenCenter(Camera camera, out Vector2 screenCenter)
    {
        var backgroundRect = _board.BackgroundRect != null
            ? GetRectTransformScreenRect(_board.BackgroundRect, camera)
            : Rect.MinMaxRect(0f, 0f, Screen.width, Screen.height);
        var availableBottom = backgroundRect.yMin;

        if (_board.PieceBoardRect != null
            && _board.PieceBoardRect.gameObject.activeInHierarchy
            && !_isPieceBoardHidden)
        {
            availableBottom = GetRectTransformScreenRect(_board.PieceBoardRect, camera).yMax;
        }
        else if (_board.PieceBgRenderer != null
                 && _board.PieceBgRenderer.gameObject.activeInHierarchy
                 && !_isPieceBgHidden)
        {
            availableBottom = camera.WorldToScreenPoint(
                new Vector3(
                    _board.PieceBgRenderer.bounds.center.x,
                    _board.PieceBgRenderer.bounds.max.y,
                    _board.PieceBgRenderer.bounds.center.z)).y;
        }

        availableBottom = Mathf.Clamp(availableBottom, backgroundRect.yMin, backgroundRect.yMax);
        screenCenter = new Vector2(
            backgroundRect.center.x,
            (availableBottom + backgroundRect.yMax) * 0.5f);
        return backgroundRect.width > 0f && backgroundRect.height > 0f;
    }

    private static Rect GetRectTransformScreenRect(RectTransform rectTransform, Camera fallbackCamera)
    {
        var canvas = rectTransform.GetComponentInParent<Canvas>();
        var eventCamera = canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas != null ? canvas.worldCamera ?? fallbackCamera : fallbackCamera;
        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        var first = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[0]);
        var xMin = first.x;
        var xMax = first.x;
        var yMin = first.y;
        var yMax = first.y;
        for (var i = 1; i < corners.Length; i++)
        {
            var point = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[i]);
            xMin = Mathf.Min(xMin, point.x);
            xMax = Mathf.Max(xMax, point.x);
            yMin = Mathf.Min(yMin, point.y);
            yMax = Mathf.Max(yMax, point.y);
        }

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private static Rect UnionRects(Rect left, Rect right)
    {
        return Rect.MinMaxRect(
            Mathf.Min(left.xMin, right.xMin),
            Mathf.Min(left.yMin, right.yMin),
            Mathf.Max(left.xMax, right.xMax),
            Mathf.Max(left.yMax, right.yMax));
    }

    private Bounds BuildActiveGroupBounds(int activeGroupIndex)
    {
        var camera = Camera.main;
        var hasBounds = false;
        var combinedBounds = new Bounds(Vector3.zero, Vector3.zero);
        if (_board.GrooveImagesByGroup != null && camera != null
            && activeGroupIndex >= 0
            && activeGroupIndex < _board.GrooveImagesByGroup.Count)
        {
            var group = _board.GrooveImagesByGroup[activeGroupIndex];
            if (group != null)
            {
                for (var i = 0; i < group.Count; i++)
                {
                    var grooveImage = group[i];
                    if (grooveImage == null || !grooveImage.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    var grooveBounds = GameCommonUtility.GetRectTransformCameraWorldBounds(
                        grooveImage.rectTransform,
                        camera,
                        WorldGameplayDepth);
                    if (!hasBounds)
                    {
                        combinedBounds = grooveBounds;
                        hasBounds = true;
                    }
                    else
                    {
                        combinedBounds.Encapsulate(grooveBounds);
                    }
                }
            }
        }

        return hasBounds ? combinedBounds : new Bounds(Vector3.zero, Vector3.zero);
    }

    private Bounds BuildPageBoundsForActiveGroup(Bounds activeGroupBounds)
    {
        var camera = Camera.main;
        var hasBounds = activeGroupBounds.size.sqrMagnitude > 0f;
        var combinedBounds = activeGroupBounds;
        if (!hasBounds && _board.GameBoardImage != null && camera != null)
        {
            combinedBounds = GameCommonUtility.GetRectTransformCameraWorldBounds(
                _board.GameBoardImage.rectTransform,
                camera,
                WorldGameplayDepth);
            hasBounds = true;
        }

        if (_board.PieceBoardRect != null && camera != null)
        {
            var pieceBoardBounds = GameCommonUtility.GetRectTransformCameraWorldBounds(
                _board.PieceBoardRect,
                camera,
                WorldGameplayDepth);
            if (!hasBounds)
            {
                combinedBounds = pieceBoardBounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(pieceBoardBounds);
            }
        }
        else if (_board.PieceBgRenderer != null)
        {
            if (!hasBounds)
            {
                combinedBounds = _board.PieceBgRenderer.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(_board.PieceBgRenderer.bounds);
            }
        }

        var placedRoot = GameObject.Find(PlacedPiecesRootObjectName);
        if (placedRoot != null)
        {
            var placedRenderers = placedRoot.GetComponentsInChildren<SpriteRenderer>(true);
            for (var i = 0; i < placedRenderers.Length; i++)
            {
                var renderer = placedRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    combinedBounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    combinedBounds.Encapsulate(renderer.bounds);
                }
            }
        }

        return combinedBounds;
    }

    private void ResetBoardPanState()
    {
        RestoreGameBoardLayout();
        ResetPlacedPiecesRootPan();
    }

    private void RestoreGameBoardLayout()
    {
        if (_loadedCardBagRect != null && _hasOriginalCardBagAnchoredPosition)
        {
            _loadedCardBagRect.anchoredPosition = _originalCardBagAnchoredPosition;
        }

        if (!_hasOriginalGameBoardAnchoredPosition || _board.GameBoardImage == null)
        {
            return;
        }

        _board.GameBoardImage.rectTransform.anchoredPosition = _originalGameBoardAnchoredPosition;
    }

    private static void ResetPlacedPiecesRootPan()
    {
        var placedRoot = GameObject.Find(PlacedPiecesRootObjectName);
        if (placedRoot == null)
        {
            return;
        }

        var panOffset = placedRoot.transform.position;
        if (panOffset.sqrMagnitude <= 0.000001f)
        {
            return;
        }

        var renderers = placedRoot.GetComponentsInChildren<SpriteRenderer>(true);
        var worldPositions = new Vector3[renderers.Length];
        for (var i = 0; i < renderers.Length; i++)
        {
            worldPositions[i] = renderers[i].transform.position;
        }

        placedRoot.transform.position = Vector3.zero;
        for (var i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].transform.position = worldPositions[i];
            }
        }
    }

    private static int CountGrooveImages(List<List<Image>> grooveGroups)
    {
        if (grooveGroups == null)
        {
            return 0;
        }

        var total = 0;
        for (var i = 0; i < grooveGroups.Count; i++)
        {
            total += grooveGroups[i]?.Count ?? 0;
        }

        return total;
    }

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

        fillRenderer.sprite = GameCommonUtility.CreateSolidSprite(Color.white, PixelsPerUnit);
        fillRenderer.drawMode = SpriteDrawMode.Sliced;
        fillRenderer.size = pieceBgRenderer.size;
        fillRenderer.sortingOrder = PieceBgFillSortingOrder;
        fillRenderer.color = new Color(0f, 0f, 0f, PieceBgFillAlpha);
        fillRenderer.transform.position = new Vector3(
            pieceBgRenderer.transform.position.x,
            pieceBgRenderer.transform.position.y,
            pieceBgRenderer.transform.position.z + 0.01f);
    }

    private void EnsureBackgroundCentered()
    {
        if (_board.BackgroundRect == null)
        {
            return;
        }

        var rect = _board.BackgroundRect;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
    }

    private static Vector3 ToGameplayWorld(Vector2 screenPosition)
    {
        var world = GameCommonUtility.ScreenToWorld(screenPosition);
        world.z = WorldGameplayDepth;
        return world;
    }

    private static bool ContainsWorldXY(Bounds bounds, Vector3 worldPosition)
    {
        return worldPosition.x >= bounds.min.x
            && worldPosition.x <= bounds.max.x
            && worldPosition.y >= bounds.min.y
            && worldPosition.y <= bounds.max.y;
    }

    private void CachePieceTrayOriginalPosition()
    {
        if (_board.PieceBoardRect != null)
        {
            _pieceBoardOriginalAnchoredPosition = _board.PieceBoardRect.anchoredPosition;
            _hasPieceBoardOriginalAnchoredPosition = true;
            _isPieceBoardHidden = false;
        }

        CachePieceBgOriginalPosition();
    }

    private void CachePieceBgOriginalPosition()
    {
        if (_board.PieceBgRenderer == null)
        {
            _hasPieceBgOriginalPosition = false;
            return;
        }

        _pieceBgOriginalPosition = _board.PieceBgRenderer.transform.position;
        _hasPieceBgOriginalPosition = true;
        _isPieceBgHidden = false;
    }

    private void AlignPieceTrayToPageBottom()
    {
        if (_board.PieceBoardRect != null)
        {
            if (_isPieceBoardHidden)
            {
                return;
            }

            CachePieceTrayOriginalPosition();
            return;
        }

        AlignPieceBgToPageBottom();
    }

    private void AlignPieceBgToPageBottom()
    {
        if (_board.PieceBgRenderer == null || _isPieceBgHidden)
        {
            return;
        }

        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        var halfHeight = _board.PieceBgRenderer.bounds.extents.y;
        var bottomEdge = camera.transform.position.y - camera.orthographicSize;
        var anchoredPosition = new Vector3(
            camera.transform.position.x,
            bottomEdge + halfHeight,
            _board.PieceBgRenderer.transform.position.z);

        ApplyPieceBgSlidePosition(anchoredPosition, GetPieceBgFillTransform());
        _pieceBgOriginalPosition = anchoredPosition;
        _hasPieceBgOriginalPosition = true;
    }

    private void SlidePieceTrayOutOfScreen()
    {
        if (_board.PieceBoardRect != null)
        {
            SlidePieceBoardOutOfScreen();
            return;
        }

        SlidePieceBgOutOfScreen();
    }

    private void PreparePieceTrayForGroupStart()
    {
        ResetPieceTrayPosition(instant: true);
    }

    private void ResetPieceTrayPosition(bool instant)
    {
        if (_board.PieceBoardRect != null)
        {
            if (_pieceTraySlideCoroutine != null)
            {
                StopCoroutine(_pieceTraySlideCoroutine);
                _pieceTraySlideCoroutine = null;
            }

            if (!_hasPieceBoardOriginalAnchoredPosition)
            {
                CachePieceTrayOriginalPosition();
            }

            if (instant || !_isPieceBoardHidden)
            {
                _board.PieceBoardRect.anchoredPosition = _pieceBoardOriginalAnchoredPosition;
                _isPieceBoardHidden = false;
            }
            else
            {
                SlidePieceBoardToOriginalPosition();
            }

            return;
        }

        if (instant)
        {
            if (_pieceTraySlideCoroutine != null)
            {
                StopCoroutine(_pieceTraySlideCoroutine);
                _pieceTraySlideCoroutine = null;
            }

            if (_board.PieceBgRenderer != null && _hasPieceBgOriginalPosition)
            {
                ApplyPieceBgSlidePosition(_pieceBgOriginalPosition, GetPieceBgFillTransform());
                _isPieceBgHidden = false;
            }
        }
        else
        {
            SlidePieceBgToOriginalPosition();
        }
    }

    private void SlidePieceTrayToOriginalPosition()
    {
        ResetPieceTrayPosition(instant: false);
    }

    private void SlidePieceBoardOutOfScreen()
    {
        if (_board.PieceBoardRect == null || _isPieceBoardHidden)
        {
            return;
        }

        if (!_hasPieceBoardOriginalAnchoredPosition)
        {
            CachePieceTrayOriginalPosition();
        }

        var slideDistance = _board.PieceBoardRect.rect.height + 80f;
        var from = _board.PieceBoardRect.anchoredPosition;
        var target = _pieceBoardOriginalAnchoredPosition - new Vector2(0f, slideDistance);
        StartPieceTraySlide(from, target, true, usePieceBoard: true);
    }

    private void SlidePieceBoardToOriginalPosition()
    {
        if (_board.PieceBoardRect == null || !_hasPieceBoardOriginalAnchoredPosition || !_isPieceBoardHidden)
        {
            return;
        }

        StartPieceTraySlide(
            _board.PieceBoardRect.anchoredPosition,
            _pieceBoardOriginalAnchoredPosition,
            false,
            usePieceBoard: true);
    }

    private void SlidePieceBgOutOfScreen()
    {
        if (_board.PieceBgRenderer == null || _isPieceBgHidden)
        {
            return;
        }

        if (!_hasPieceBgOriginalPosition)
        {
            CachePieceBgOriginalPosition();
        }

        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        var halfHeight = _board.PieceBgRenderer.bounds.extents.y;
        var bottomEdge = camera.transform.position.y - camera.orthographicSize;
        var targetY = bottomEdge - halfHeight - PieceBgSlideOutPadding;
        var from = _board.PieceBgRenderer.transform.position;
        var target = new Vector3(from.x, targetY, from.z);
        StartPieceTraySlide(from, target, true, usePieceBoard: false);
    }

    private void SlidePieceBgToOriginalPosition()
    {
        if (_board.PieceBgRenderer == null || !_hasPieceBgOriginalPosition || !_isPieceBgHidden)
        {
            return;
        }

        StartPieceTraySlide(_board.PieceBgRenderer.transform.position, _pieceBgOriginalPosition, false, usePieceBoard: false);
    }

    private void StartPieceTraySlide(Vector2 fromAnchored, Vector2 toAnchored, bool willHidden, bool usePieceBoard)
    {
        if (_pieceTraySlideCoroutine != null)
        {
            StopCoroutine(_pieceTraySlideCoroutine);
            _pieceTraySlideCoroutine = null;
        }

        var fillTransform = GetPieceBgFillTransform();
        _pieceTraySlideCoroutine = StartCoroutine(
            AnimatePieceTraySlideAnchored(fromAnchored, toAnchored, fillTransform, willHidden, usePieceBoard));
    }

    private void StartPieceTraySlide(Vector3 from, Vector3 to, bool willHidden, bool usePieceBoard)
    {
        if (_pieceTraySlideCoroutine != null)
        {
            StopCoroutine(_pieceTraySlideCoroutine);
            _pieceTraySlideCoroutine = null;
        }

        var fillTransform = GetPieceBgFillTransform();
        _pieceTraySlideCoroutine = StartCoroutine(
            AnimatePieceTraySlideWorld(from, to, fillTransform, willHidden, usePieceBoard));
    }

    private IEnumerator AnimatePieceTraySlideAnchored(
        Vector2 from,
        Vector2 to,
        Transform fillTransform,
        bool willHidden,
        bool usePieceBoard)
    {
        var duration = Mathf.Max(0f, PieceBgSlideDuration);
        if (duration <= 0f)
        {
            ApplyPieceBoardSlidePosition(to, fillTransform);
            _isPieceBoardHidden = willHidden;
            _pieceTraySlideCoroutine = null;
            yield break;
        }

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            var eased = Mathf.SmoothStep(0f, 1f, t);
            var position = Vector2.LerpUnclamped(from, to, eased);
            ApplyPieceBoardSlidePosition(position, fillTransform);
            yield return null;
        }

        ApplyPieceBoardSlidePosition(to, fillTransform);
        _isPieceBoardHidden = willHidden;
        _pieceTraySlideCoroutine = null;
    }

    private IEnumerator AnimatePieceTraySlideWorld(
        Vector3 from,
        Vector3 to,
        Transform fillTransform,
        bool willHidden,
        bool usePieceBoard)
    {
        var duration = Mathf.Max(0f, PieceBgSlideDuration);
        if (duration <= 0f)
        {
            ApplyPieceBgSlidePosition(to, fillTransform);
            _isPieceBgHidden = willHidden;
            _pieceTraySlideCoroutine = null;
            yield break;
        }

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            var eased = Mathf.SmoothStep(0f, 1f, t);
            var position = Vector3.LerpUnclamped(from, to, eased);
            ApplyPieceBgSlidePosition(position, fillTransform);
            yield return null;
        }

        ApplyPieceBgSlidePosition(to, fillTransform);
        _isPieceBgHidden = willHidden;
        _pieceTraySlideCoroutine = null;
    }

    private void ApplyPieceBoardSlidePosition(Vector2 anchoredPosition, Transform fillTransform)
    {
        if (_board.PieceBoardRect != null)
        {
            _board.PieceBoardRect.anchoredPosition = anchoredPosition;
        }
    }

    private void ApplyPieceBgSlidePosition(Vector3 pieceBgPosition, Transform fillTransform)
    {
        if (_board.PieceBgRenderer != null)
        {
            _board.PieceBgRenderer.transform.position = pieceBgPosition;
        }

        if (fillTransform != null)
        {
            fillTransform.position = new Vector3(
                pieceBgPosition.x,
                pieceBgPosition.y,
                pieceBgPosition.z + 0.01f);
        }
    }

    private static Transform GetPieceBgFillTransform()
    {
        var fillObject = GameObject.Find(PieceBgFillObjectName);
        return fillObject != null ? fillObject.transform : null;
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        var color = image.color;
        color.a = Mathf.Clamp01(alpha);
        image.color = color;
    }

    private void ConfigureReturnButton()
    {
        var returnButtonObject = GameObject.Find(GameDefine.ReturnButtonObjectName);
        if (returnButtonObject == null)
        {
            Debug.LogWarning($"GameScene: return button not found. Expected object named {GameDefine.ReturnButtonObjectName}.");
            return;
        }

        var button = returnButtonObject.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"GameScene: {GameDefine.ReturnButtonObjectName} is missing Button component.");
            return;
        }

        button.onClick.RemoveListener(OnReturnButtonClicked);
        button.onClick.AddListener(OnReturnButtonClicked);
    }

    private void OnReturnButtonClicked()
    {
        GameManager.EnterMainScene();
    }
}
