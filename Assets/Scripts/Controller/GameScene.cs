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
    private const float DefaultBoardScale = 1f;
    private const float WorldGameplayDepth = -0.5f;
    private const float GamePageCameraPadding = 0.3f;
    private const float DraggableLeftPadding = 0.2f;
    private const float DraggableHorizontalSpacingPixels = 20f;
    private const float PieceTrayMaxHeightRatio = 0.9f;
    private const float SnapDistanceMin = 0.2f;
    private const float SnapDistanceMax = 0.8f;
    private const float SnapDistanceSizeRatio = 0.22f;
    private const float PieceBgSlideDuration = 0.25f;
    private const float TaskProgressRollDuration = 0.8f;
    private const float PieceBgSlideOutPadding = 0.15f;
    private const int PieceBgFillSortingOrder = 499;
    private const int PieceBgSortingOrder = 500;
    private const float PieceBgAlpha = 1f;
    private const float PieceBgFillAlpha = 0.3f;
    private const float GameEntranceBoardDuration = 0.46f;
    private const float GameEntranceTrayDelay = 0.12f;
    private const float GameEntranceTrayDuration = 0.38f;
    private const float GameEntrancePieceDelay = 0.28f;
    private const float GameEntrancePieceDuration = 0.34f;
    private const float GameEntrancePieceStagger = 0.035f;
    private const float GameEntranceControlDelay = 0.18f;
    private const float GameEntranceControlDuration = 0.24f;
    private const int GameEntranceWarmupFrameCount = 2;
    private const float GameEntranceMaxFrameDelta = 1f / 30f;
    private const int PieceSortingOrder = 520;
    private const float HintShakeAngle = 6f;
    private const float HintShakeCyclesPerSecond = 4.5f;
    private const float HintShakeDuration = 0.8f;
    private const float HintDashLength = 20f;
    private const float HintDashGap = 15f;
    private const float HintOutlineWidth = 3f;
    private const float HintOutlineScrollSpeed = 60f;
    private const string BootstrapObjectName = "GameSceneBootstrap";
    private const string PieceBgFillObjectName = "PieceBgFill";
    private const string PieceBgObjectName = "PieceBg";
    private const string PieceBgPath = GameDefine.UiRoot + "/BasicUI/ImgMaskBlack.png";
    private const string DraggableGroupRootObjectName = "DraggableGroupPieces";
    private const string ActiveGroupOutlineRootObjectName = "ActiveGroupOutline";
    private const string PlacedPiecesRootObjectName = "PlacedPieces";
    private const string TaskItemObjectName = "TaskItem";
    private const string TaskScorePath = "TaskBg2/TaskScore";
    private const string TaskBagCountPath = "TaskBg2/TaskBagNum";
    private const string TaskRewardImgBagPath = "ImgBagBg/ImgBag";
    private const string HintButtonObjectName = "BtnTips";
    private const string PieceHintOutlineObjectName = "PieceHintOutline";
    private static readonly Color PieceHintOutlineColor = new Color32(112, 151, 75, 255);
    private static bool sHookedSceneLoaded;
    private readonly BoardState _board = new BoardState();
    private readonly DragState _drag = new DragState();
    private readonly List<int> _settlementPackRewardIds = new List<int>();
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
    private bool _isAccumulateScoreTaskActive;
    private bool _wasHintUsed;
    private bool _isLevelOutlineEnabled;
    private bool _isStickerOutlineEnabled;
    private bool _hasGameplayTimerStarted;
    private float _gameplayStartRealtime;
    private float _completionTimeSeconds;
    private bool _wasSelectedPackCompletedOnEntry;
    private bool _didAdvanceTaskDuringSettlement;
    private bool _didFailTaskAdvanceDuringSettlement;
    private bool _didSavePackCompletion;
    private GameObject _rewardPanelRoot;
    private Transform _rewardTaskItem;
    private TMP_Text _settlementScoreText;
    private TMP_Text _settlementBagCountText;
    private Image _taskRewardImage;
    private Button _finishButton;
    private bool _isSettlementReadyForFinish;
    private bool _isFinishTransitionStarted;
    private bool _isEntranceAnimating;
    private GameObject _loadedCardBagRoot;
    private RectTransform _loadedCardBagRect;
    private float _configuredBoardScale = DefaultBoardScale;
    private Vector3 _originalCardBagLocalScale = Vector3.one;
    private bool _hasOriginalCardBagLocalScale;
    private Vector2 _originalCardBagAnchoredPosition;
    private bool _hasOriginalCardBagAnchoredPosition;
    private DraggablePieceState _hintedPiece;
    private Quaternion _hintedPieceBaseRotation = Quaternion.identity;
    private float _hintShakeStartTime;
    private bool _isHintPieceShaking;
    private GameObject _pieceHintOutlineRoot;

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
        InitializeScoringSession();
        var selectedBagId = GameManager.GetBagId();
        var playEntranceAnimation = GameManager.ConsumeGameEntranceAnimation();
        CardPackDataUtility.Initialize();
        _wasSelectedPackCompletedOnEntry = CardPackDataUtility.IsPackCompleted(selectedBagId);
        _didAdvanceTaskDuringSettlement = false;
        _didFailTaskAdvanceDuringSettlement = false;
        _didSavePackCompletion = false;
        _isSettlementReadyForFinish = false;
        _isFinishTransitionStarted = false;
        _settlementPackRewardIds.Clear();
        InitializeGameplay(selectedBagId);
        InitializeTaskTracking();
        ConfigureReturnButton();
        ConfigureHintButton();
        ConfigureRewardPanel();
        if (playEntranceAnimation)
        {
            StartCoroutine(PlayGameEntranceAnimation());
        }

        Debug.Log("GameScene bootstrap completed.");
    }

    private void OnDestroy()
    {
        GameCursorUtility.SetDefault();
        ClearPieceHint();
        HintDashedOutlineGraphic.ClearPathCache();
    }

    private void Update()
    {
        UpdatePieceHintAnimation();
        if (_isEntranceAnimating)
        {
            GameCursorUtility.SetDefault();
            return;
        }

        GameCommonUtility.ProcessPointerInput(
            TryBeginDrag,
            UpdateDragging,
            OnPointerEnd);
        RefreshCursorForPointer(Input.mousePosition);
    }

    private void InitializeGameplay(int bagId)
    {
        GameManager.SetBagId(bagId);
        LoadConfiguredBoardScale(bagId);
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
            $"Pieces={CountGrooveImages(_board.GrooveImagesByGroup)}, BoardScale={_configuredBoardScale:0.###}");
    }

    private void LoadConfiguredBoardScale(int bagId)
    {
        _configuredBoardScale = DefaultBoardScale;
        if (!GameConfigRepository.TryGetCardPackConfig(bagId, out var config))
        {
            Debug.LogWarning(
                $"GameScene: card pack config not found; BoardScale defaults to 1. bagId={bagId}");
            return;
        }

        _configuredBoardScale = config.BoardScale;
    }

    private IEnumerator PlayGameEntranceAnimation()
    {
        _isEntranceAnimating = true;
        Canvas.ForceUpdateCanvases();

        var camera = Camera.main;
        var boardCenter = _board.GameBoardImage != null && camera != null
            ? GameCommonUtility.RectTransformToCameraWorld(
                _board.GameBoardImage.rectTransform,
                camera,
                WorldGameplayDepth)
            : Vector3.zero;

        var boardRect = _loadedCardBagRect;
        var boardTarget = boardRect != null
            ? boardRect.anchoredPosition
            : _hasOriginalCardBagAnchoredPosition
                ? _originalCardBagAnchoredPosition
                : Vector2.zero;
        var boardStart = boardTarget + Vector2.up * ReferenceHeight;
        if (boardRect != null)
        {
            boardRect.anchoredPosition = boardStart;
        }

        var trayRect = _board.PieceBoardRect;
        var trayTarget = _hasPieceBoardOriginalAnchoredPosition
            ? _pieceBoardOriginalAnchoredPosition
            : trayRect != null ? trayRect.anchoredPosition : Vector2.zero;
        var trayOffset = trayRect != null
            ? trayRect.rect.height + 120f
            : 420f;
        var trayStart = trayTarget - Vector2.up * trayOffset;
        if (trayRect != null)
        {
            trayRect.anchoredPosition = trayStart;
        }

        var returnCanvasGroup = GetOrAddCanvasGroup(
            GameCommonUtility.FindSceneObject(GameDefine.ReturnButtonObjectName));
        var hintCanvasGroup = GetOrAddCanvasGroup(
            GameCommonUtility.FindSceneObject(HintButtonObjectName));
        SetCanvasGroupAlpha(returnCanvasGroup, 0f);
        SetCanvasGroupAlpha(hintCanvasGroup, 0f);

        var pieceCount = _drag.CurrentGroupDraggables.Count;
        var pieceTargets = new Vector3[pieceCount];
        var pieceStarts = new Vector3[pieceCount];
        var pieceTargetScales = new Vector3[pieceCount];
        var pieceTargetRotations = new Quaternion[pieceCount];
        var pieceTargetColors = new Color[pieceCount];
        for (var i = 0; i < pieceCount; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            var renderer = state?.PieceRenderer;
            if (renderer == null)
            {
                continue;
            }

            pieceTargets[i] = renderer.transform.position;
            pieceTargetScales[i] = renderer.transform.localScale;
            pieceTargetRotations[i] = renderer.transform.rotation;
            pieceTargetColors[i] = renderer.color;
            var angle = i * 137.5f * Mathf.Deg2Rad;
            var radius = 0.12f + (i % 4) * 0.07f;
            pieceStarts[i] = boardCenter + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f);
            pieceStarts[i].z = pieceTargets[i].z;
            renderer.transform.position = pieceStarts[i];
            renderer.transform.localScale = pieceTargetScales[i] * 1.12f;
            renderer.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(angle) * 18f);
            var color = pieceTargetColors[i];
            color.a = 0f;
            renderer.color = color;
        }

        // Let the entrance start pose reach the screen before advancing its clock.
        for (var frame = 0; frame < GameEntranceWarmupFrameCount; frame++)
        {
            yield return null;
        }

        var totalDuration = Mathf.Max(
            GameEntranceBoardDuration,
            GameEntranceTrayDelay + GameEntranceTrayDuration,
            GameEntranceControlDelay + GameEntranceControlDuration,
            GameEntrancePieceDelay
                + Mathf.Max(0, pieceCount - 1) * GameEntrancePieceStagger
                + GameEntrancePieceDuration);
        var elapsed = 0f;
        while (elapsed < totalDuration)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, GameEntranceMaxFrameDelta);
            if (boardRect != null)
            {
                var boardT = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(elapsed / GameEntranceBoardDuration));
                boardRect.anchoredPosition = Vector2.LerpUnclamped(boardStart, boardTarget, boardT);
            }

            if (trayRect != null)
            {
                var trayT = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(
                        (elapsed - GameEntranceTrayDelay) / GameEntranceTrayDuration));
                trayRect.anchoredPosition = Vector2.LerpUnclamped(trayStart, trayTarget, trayT);
            }

            var controlT = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(
                    (elapsed - GameEntranceControlDelay) / GameEntranceControlDuration));
            SetCanvasGroupAlpha(returnCanvasGroup, controlT);
            SetCanvasGroupAlpha(hintCanvasGroup, controlT);

            for (var i = 0; i < pieceCount; i++)
            {
                var state = _drag.CurrentGroupDraggables[i];
                var renderer = state?.PieceRenderer;
                if (renderer == null)
                {
                    continue;
                }

                var pieceT = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(
                        (elapsed - GameEntrancePieceDelay - i * GameEntrancePieceStagger)
                        / GameEntrancePieceDuration));
                renderer.transform.position = Vector3.LerpUnclamped(
                    pieceStarts[i],
                    pieceTargets[i],
                    pieceT);
                renderer.transform.localScale = Vector3.LerpUnclamped(
                    pieceTargetScales[i] * 1.12f,
                    pieceTargetScales[i],
                    pieceT);
                renderer.transform.rotation = Quaternion.SlerpUnclamped(
                    Quaternion.Euler(0f, 0f, Mathf.Sin(i * 137.5f * Mathf.Deg2Rad) * 18f),
                    pieceTargetRotations[i],
                    pieceT);
                var color = pieceTargetColors[i];
                color.a *= pieceT;
                renderer.color = color;
            }

            yield return null;
        }

        if (boardRect != null)
        {
            boardRect.anchoredPosition = boardTarget;
        }

        if (trayRect != null)
        {
            trayRect.anchoredPosition = trayTarget;
        }

        SetCanvasGroupAlpha(returnCanvasGroup, 1f);
        SetCanvasGroupAlpha(hintCanvasGroup, 1f);
        for (var i = 0; i < pieceCount; i++)
        {
            var renderer = _drag.CurrentGroupDraggables[i]?.PieceRenderer;
            if (renderer == null)
            {
                continue;
            }

            renderer.transform.position = pieceTargets[i];
            renderer.transform.localScale = pieceTargetScales[i];
            renderer.transform.rotation = pieceTargetRotations[i];
            renderer.color = pieceTargetColors[i];
        }

        _isEntranceAnimating = false;
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        if (target == null)
        {
            return null;
        }

        var canvasGroup = target.GetComponent<CanvasGroup>();
        return canvasGroup != null ? canvasGroup : target.AddComponent<CanvasGroup>();
    }

    private static void SetCanvasGroupAlpha(CanvasGroup canvasGroup, float alpha)
    {
        if (canvasGroup == null)
        {
            return;
        }

        var clampedAlpha = Mathf.Clamp01(alpha);
        canvasGroup.alpha = clampedAlpha;
        canvasGroup.interactable = clampedAlpha >= 0.999f;
        canvasGroup.blocksRaycasts = clampedAlpha >= 0.999f;
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
            _originalCardBagLocalScale = rectTransform.localScale;
            _hasOriginalCardBagLocalScale = true;
            ApplyConfiguredBoardScale();
            PlaceCardBagAfterBackground(rectTransform);
            _originalCardBagAnchoredPosition = rectTransform.anchoredPosition;
            _hasOriginalCardBagAnchoredPosition = true;
        }

        _board.IsBoardAndGroovesInitialized = false;
        Debug.Log($"GameScene: loaded card bag prefab Resources/{resourcePath}.");
    }

    private void ApplyConfiguredBoardScale()
    {
        if (_loadedCardBagRect == null || !_hasOriginalCardBagLocalScale)
        {
            return;
        }

        _loadedCardBagRect.localScale = _originalCardBagLocalScale * _configuredBoardScale;
        Canvas.ForceUpdateCanvases();
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

    private Vector3 CalculatePieceScaleOnBoard(Image grooveImage)
    {
        if (grooveImage == null || grooveImage.sprite == null || Camera.main == null)
        {
            return Vector3.one * GetBoardToSpriteScaleFactor();
        }

        var sprite = grooveImage.sprite;
        var pixelsPerUnit = Mathf.Max(0.001f, sprite.pixelsPerUnit);
        var spriteWorldSize = sprite.rect.size / pixelsPerUnit;
        var grooveWorldSize = GameCommonUtility.GetRectTransformCameraWorldBounds(
            grooveImage.rectTransform,
            Camera.main,
            WorldGameplayDepth).size;
        if (spriteWorldSize.x <= 0.001f
            || spriteWorldSize.y <= 0.001f
            || grooveWorldSize.x <= 0.001f
            || grooveWorldSize.y <= 0.001f)
        {
            return Vector3.one * GetBoardToSpriteScaleFactor();
        }

        return new Vector3(
            grooveWorldSize.x / spriteWorldSize.x,
            grooveWorldSize.y / spriteWorldSize.y,
            1f);
    }

    private Vector3 CalculateTrayScaleForPiece(
        SpriteRenderer pieceRenderer,
        Bounds hostBounds,
        Vector3 dragScale)
    {
        var boardScale = dragScale / _configuredBoardScale;
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

            var dragScale = CalculatePieceScaleOnBoard(grooveImage);
            var trayScale = Vector3.Min(
                CalculateTrayScaleForPiece(pieceRenderer, hostBounds, dragScale),
                dragScale);
            pieceRenderer.transform.localScale = trayScale;
            _drag.CurrentGroupDraggables.Add(new DraggablePieceState
            {
                PieceRenderer = pieceRenderer,
                GrooveImage = grooveImage,
                GrooveRect = grooveImage.rectTransform,
                StartPosition = pieceRenderer.transform.position,
                TrayScale = trayScale,
                DragScale = dragScale,
                IsOnTray = true,
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
        ClearPieceHint();
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
        if (!_isLevelOutlineEnabled)
        {
            ClearActiveGroupOutline();
            return;
        }

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

        if (!TryGetNumberedGroup(grooveGroup[0], out var groupNumber))
        {
            return;
        }

        var resourcePath = GameDefine.FormatPuzzleOutlineResourcesPath(
            GameManager.GetBagId(),
            groupNumber);
        var outlineSprite = Resources.Load<Sprite>(resourcePath);
        if (outlineSprite == null)
        {
            Debug.LogWarning(
                $"GameScene: baked puzzle outline is missing at Resources/{resourcePath}. " +
                "Run Puffies/Puzzles/Bake Outline Masks in the Unity Editor.");
            return;
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
    }

    private void ClearActiveGroupOutline()
    {
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

        var state = FindDraggablePieceAt(screenPosition);
        if (state == null)
        {
            return;
        }

        var world = ToGameplayWorld(screenPosition);
        _drag.DraggingPiece = state;
        _drag.DragOffset = state.PieceRenderer.transform.position - world;
        if (!state.IsOnTray)
        {
            ResetPieceTrayPosition(instant: true);
        }
        if (state == _hintedPiece)
        {
            state.PieceRenderer.transform.rotation = _hintedPieceBaseRotation;
        }
        state.PieceRenderer.transform.localScale = state.DragScale;
        state.PieceRenderer.sortingOrder = PieceSortingOrder + 100;
        if (state.IsOnTray && CountUnplacedTrayPieces() == 1)
        {
            SlidePieceTrayOutOfScreen();
        }
    }

    private void RefreshCursorForPointer(Vector2 screenPosition)
    {
        if (_drag.DraggingPiece != null)
        {
            GameCursorUtility.SetPieceDrag();
            return;
        }

        if (!_isGameFinished && FindDraggablePieceAt(screenPosition) != null)
        {
            GameCursorUtility.SetPieceHover();
            return;
        }

        GameCursorUtility.SetDefault();
    }

    private DraggablePieceState FindDraggablePieceAt(Vector2 screenPosition)
    {
        var world = ToGameplayWorld(screenPosition);
        for (var i = _drag.CurrentGroupDraggables.Count - 1; i >= 0; i--)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state != null
                && !state.IsPlaced
                && state.PieceRenderer != null
                && ContainsWorldXY(state.PieceRenderer.bounds, world))
            {
                return state;
            }
        }

        return null;
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
        var wasOnTray = state.IsOnTray;

        var groovePosition = GetGrooveSnapPosition(state.GrooveRect, Camera.main);
        if (state.GrooveRect != null
            && Vector3.Distance(state.PieceRenderer.transform.position, groovePosition) <= CalculateSnapDistance(state))
        {
            state.PieceRenderer.transform.position = groovePosition;
            state.PieceRenderer.transform.localScale = state.DragScale;
            state.IsOnTray = false;
            state.IsPlaced = true;
            if (state == _hintedPiece)
            {
                ClearPieceHint();
            }
            StartGameplayTimerIfNeeded();
            if (wasOnTray)
            {
                CompactFollowingTrayPieces(state);
            }
            CommitPlacedPieceToBoardImage(state);
            TryAdvanceGroup();
            return;
        }

        ResetPieceTrayPosition(instant: true);
        if (ShouldReturnPieceToTray(state.PieceRenderer))
        {
            state.IsOnTray = true;
            state.PieceRenderer.transform.localScale = state.TrayScale;
            LayoutTrayPieces();
            return;
        }

        state.PieceRenderer.transform.localScale = state.DragScale;
        state.PieceRenderer.transform.position = ClampPieceToTableBounds(state.PieceRenderer);
        state.IsOnTray = false;
        if (wasOnTray)
        {
            CompactFollowingTrayPieces(state);
        }
    }

    private bool ShouldReturnPieceToTray(SpriteRenderer renderer)
    {
        if (renderer == null)
        {
            return false;
        }

        var pieceBounds = renderer.bounds;
        var trayBounds = GetPieceTrayBounds();
        if (pieceBounds.size.y <= 0f || trayBounds.size.sqrMagnitude <= 0f)
        {
            return false;
        }

        var horizontalOverlap = Mathf.Min(pieceBounds.max.x, trayBounds.max.x)
                                - Mathf.Max(pieceBounds.min.x, trayBounds.min.x);
        var verticalOverlap = Mathf.Min(pieceBounds.max.y, trayBounds.max.y)
                              - Mathf.Max(pieceBounds.min.y, trayBounds.min.y);
        return horizontalOverlap > 0f && verticalOverlap >= pieceBounds.size.y * 0.5f;
    }

    private Vector3 ClampPieceToTableBounds(SpriteRenderer renderer)
    {
        if (renderer == null)
        {
            return Vector3.zero;
        }

        var camera = Camera.main;
        Bounds tableBounds;
        if (_board.BackgroundRect != null && camera != null)
        {
            tableBounds = GameCommonUtility.GetRectTransformCameraWorldBounds(
                _board.BackgroundRect,
                camera,
                WorldGameplayDepth);
        }
        else
        {
            var bottomLeft = ToGameplayWorld(Vector2.zero);
            var topRight = ToGameplayWorld(new Vector2(Screen.width, Screen.height));
            tableBounds = new Bounds(
                (bottomLeft + topRight) * 0.5f,
                new Vector3(
                    Mathf.Abs(topRight.x - bottomLeft.x),
                    Mathf.Abs(topRight.y - bottomLeft.y),
                    0f));
        }

        var pieceBounds = renderer.bounds;
        var offsetX = CalculateBoundsClampOffset(
            pieceBounds.min.x,
            pieceBounds.max.x,
            tableBounds.min.x,
            tableBounds.max.x,
            pieceBounds.center.x,
            tableBounds.center.x);
        var offsetY = CalculateBoundsClampOffset(
            pieceBounds.min.y,
            pieceBounds.max.y,
            tableBounds.min.y,
            tableBounds.max.y,
            pieceBounds.center.y,
            tableBounds.center.y);
        var position = renderer.transform.position;
        position.x += offsetX;
        position.y += offsetY;
        position.z = WorldGameplayDepth;
        return position;
    }

    private static float CalculateBoundsClampOffset(
        float itemMin,
        float itemMax,
        float containerMin,
        float containerMax,
        float itemCenter,
        float containerCenter)
    {
        if (itemMax - itemMin >= containerMax - containerMin)
        {
            return containerCenter - itemCenter;
        }

        if (itemMin < containerMin)
        {
            return containerMin - itemMin;
        }

        return itemMax > containerMax ? containerMax - itemMax : 0f;
    }

    private void CommitPlacedPieceToBoardImage(DraggablePieceState state)
    {
        if (state?.GrooveImage == null || state.PieceRenderer == null)
        {
            return;
        }

        state.GrooveImage.gameObject.SetActive(true);
        SetImageAlpha(state.GrooveImage, 1f);
        state.PieceRenderer.gameObject.SetActive(false);
        Destroy(state.PieceRenderer.gameObject);
        state.PieceRenderer = null;
    }

    private int CountUnplacedTrayPieces()
    {
        var count = 0;
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state != null && !state.IsPlaced && state.IsOnTray)
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
            if (state == null
                || state.IsPlaced
                || !state.IsOnTray
                || state.PieceRenderer == null
                || state == _drag.DraggingPiece)
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
            var position = PlaceTrayPieceAt(
                state.PieceRenderer,
                state.TrayScale,
                pieceCenterX,
                trayCenterY);
            state.StartPosition = position;
            nextCenterX = pieceCenterX + pieceHalfWidth + horizontalSpacing;
        }
    }

    private void CompactFollowingTrayPieces(DraggablePieceState placedState)
    {
        if (placedState?.PieceRenderer == null)
        {
            return;
        }

        var placedPieceNumber = GetPieceNumberFromState(placedState);
        if (placedPieceNumber == int.MaxValue)
        {
            return;
        }

        var shiftX = GameCommonUtility.GetPieceWidth(
            placedState.PieceRenderer,
            placedState.TrayScale) + DraggableHorizontalSpacingPixels / PixelsPerUnit;
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null
                || state.IsPlaced
                || !state.IsOnTray
                || state.PieceRenderer == null
                || GetPieceNumberFromState(state) <= placedPieceNumber)
            {
                continue;
            }

            var position = state.PieceRenderer.transform.position;
            position.x -= shiftX;
            state.PieceRenderer.transform.position = position;
            state.StartPosition = position;
        }
    }

    private static Vector3 PlaceTrayPieceAt(
        SpriteRenderer renderer,
        Vector3 trayScale,
        float centerX,
        float trayCenterY)
    {
        renderer.transform.localScale = trayScale;
        var spriteCenter = renderer.sprite != null ? renderer.sprite.bounds.center : Vector3.zero;
        renderer.transform.position = new Vector3(
            centerX - spriteCenter.x * trayScale.x,
            trayCenterY - spriteCenter.y * trayScale.y,
            WorldGameplayDepth);

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
            MarkCurrentPackInProgress();
            CreateDraggableGroup(nextGroupIndex);
            return;
        }

        ShowRewardPanel();
    }

    private static void MarkCurrentPackInProgress()
    {
        var packId = GameManager.GetBagId();
        if (packId <= 0 || CardPackDataUtility.TryMarkPackInProgress(packId))
        {
            return;
        }

        Debug.LogWarning($"GameScene: failed to mark card pack in progress. packId={packId}");
    }

    private void ConfigureRewardPanel()
    {
        _rewardPanelRoot = GameCommonUtility.FindSceneObject(GameDefine.RewardPanelObjectName);
        if (_rewardPanelRoot == null)
        {
            Debug.LogWarning($"GameScene: reward panel not found. Expected object named {GameDefine.RewardPanelObjectName}.");
            return;
        }

        CacheRewardPanelReferences();
        _rewardPanelRoot.SetActive(false);
        _isGameFinished = false;

        var finishButtonObject = GameCommonUtility.FindSceneObject(GameDefine.FinishButtonObjectName);
        if (finishButtonObject == null)
        {
            Debug.LogWarning($"GameScene: finish button not found. Expected object named {GameDefine.FinishButtonObjectName}.");
            return;
        }

        _finishButton = finishButtonObject.GetComponent<Button>();
        if (_finishButton == null)
        {
            Debug.LogWarning($"GameScene: {GameDefine.FinishButtonObjectName} is missing Button component.");
            return;
        }

        _finishButton.interactable = false;
        _finishButton.onClick.RemoveListener(OnFinishButtonClicked);
        _finishButton.onClick.AddListener(OnFinishButtonClicked);
    }

    private void CacheRewardPanelReferences()
    {
        _rewardTaskItem = null;
        _settlementScoreText = null;
        _settlementBagCountText = null;
        _taskRewardImage = null;
        if (_rewardPanelRoot == null)
        {
            return;
        }

        _rewardTaskItem = _rewardPanelRoot.transform.Find(TaskItemObjectName);
        _settlementScoreText = _rewardPanelRoot.transform.Find(TaskScorePath)?.GetComponent<TMP_Text>();
        _settlementBagCountText = _rewardPanelRoot.transform.Find(TaskBagCountPath)?.GetComponent<TMP_Text>();
        _taskRewardImage = _rewardPanelRoot.transform.Find(TaskRewardImgBagPath)?.GetComponent<Image>();

        if (_rewardTaskItem == null)
        {
            Debug.LogWarning($"GameScene: task reward UI not found. Expected {TaskItemObjectName} under RewardPanel.");
        }

        if (_settlementScoreText == null)
        {
            Debug.LogWarning($"GameScene: settlement score text not found. Expected {TaskScorePath}.");
        }
        else
        {
            GameFontUtility.ApplyDefaultFont(_settlementScoreText);
        }

        if (_settlementBagCountText == null)
        {
            Debug.LogWarning($"GameScene: card pack count text not found. Expected {TaskBagCountPath}.");
        }
        else
        {
            GameFontUtility.ApplyDefaultFont(_settlementBagCountText);
        }
    }

    private void ShowRewardPanel()
    {
        if (_isGameFinished)
        {
            return;
        }

        _isGameFinished = true;
        StopGameplayTimer();
        EndDragging();

        if (_rewardPanelRoot == null)
        {
            _rewardPanelRoot = GameCommonUtility.FindSceneObject(GameDefine.RewardPanelObjectName);
            CacheRewardPanelReferences();
        }

        if (_rewardPanelRoot == null)
        {
            Debug.LogWarning($"GameScene: cannot show reward panel. Expected object named {GameDefine.RewardPanelObjectName}.");
            return;
        }

        PrepareBoardForRewardPanel();
        _didSavePackCompletion = SaveCardPackAfterPuzzleComplete();
        _isSettlementReadyForFinish = false;
        if (_finishButton != null)
        {
            _finishButton.interactable = false;
        }

        _rewardPanelRoot.SetActive(true);
        _rewardPanelRoot.transform.SetAsLastSibling();
        StartCoroutine(ProcessTaskSettlement());
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

            state.PieceRenderer.gameObject.SetActive(false);
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
                child.gameObject.SetActive(false);
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
        ClearPieceHint();
        _drag.DraggingPiece = null;
        _drag.CurrentGroupDraggables.Clear();
        ClearActiveGroupOutline();

        var draggableRoot = GameObject.Find(DraggableGroupRootObjectName);
        if (draggableRoot != null)
        {
            draggableRoot.SetActive(false);
            Destroy(draggableRoot);
        }

        var placedRoot = GameObject.Find(PlacedPiecesRootObjectName);
        if (placedRoot != null)
        {
            placedRoot.SetActive(false);
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
        if (!_isSettlementReadyForFinish || _isFinishTransitionStarted)
        {
            return;
        }

        _isFinishTransitionStarted = true;
        if (_finishButton != null)
        {
            _finishButton.interactable = false;
        }

        if (_settlementPackRewardIds.Count == 0
            || _taskRewardImage == null
            || !CardPackRewardFlyTransition.TryStart(
                _taskRewardImage.rectTransform,
                _settlementPackRewardIds))
        {
            GameManager.EnterMainScene();
        }
    }

    private void InitializeScoringSession()
    {
        var settings = GameSettingsUtility.GetSettings();
        _wasHintUsed = false;
        _isLevelOutlineEnabled = settings.IsLevelOutlineEnabled;
        _isStickerOutlineEnabled = settings.IsStickerOutlineEnabled;
        _hasGameplayTimerStarted = false;
        _gameplayStartRealtime = 0f;
        _completionTimeSeconds = 0f;

        Debug.Log(
            $"GameScene: scoring session initialized. levelOutline={_isLevelOutlineEnabled}, " +
            $"stickerOutline={_isStickerOutlineEnabled}");
    }

    private void ConfigureHintButton()
    {
        var hintButtonObject = GameCommonUtility.FindSceneObject(HintButtonObjectName);
        if (hintButtonObject == null)
        {
            Debug.LogWarning($"GameScene: hint button not found. Expected object named {HintButtonObjectName}.");
            return;
        }

        var hintButton = hintButtonObject.GetComponent<Button>();
        if (hintButton == null)
        {
            Debug.LogWarning($"GameScene: {HintButtonObjectName} is missing Button component.");
            return;
        }

        hintButton.onClick.RemoveListener(OnHintButtonClicked);
        hintButton.onClick.AddListener(OnHintButtonClicked);
    }

    private void OnHintButtonClicked()
    {
        if (_isGameFinished || _isEntranceAnimating || _drag.DraggingPiece != null)
        {
            return;
        }

        if (_hintedPiece != null)
        {
            ClearPieceHint();
            return;
        }

        var target = FindHintTarget();
        if (target == null)
        {
            return;
        }

        _wasHintUsed = true;
        ShowPieceHint(target);
        Debug.Log("GameScene: hint used; no-hint score bonus disabled for this game.");
    }

    private DraggablePieceState FindHintTarget()
    {
        if (_hintedPiece != null
            && !_hintedPiece.IsPlaced
            && _hintedPiece.PieceRenderer != null)
        {
            return _hintedPiece;
        }

        DraggablePieceState target = null;
        var lowestPieceNumber = int.MaxValue;
        for (var i = 0; i < _drag.CurrentGroupDraggables.Count; i++)
        {
            var state = _drag.CurrentGroupDraggables[i];
            if (state == null || state.IsPlaced || state.PieceRenderer == null || state.GrooveRect == null)
            {
                continue;
            }

            var pieceNumber = GetPieceNumberFromState(state);
            if (pieceNumber >= lowestPieceNumber)
            {
                continue;
            }

            target = state;
            lowestPieceNumber = pieceNumber;
        }

        return target;
    }

    private void ShowPieceHint(DraggablePieceState state)
    {
        ClearPieceHint();
        if (state == null || state.PieceRenderer == null || state.GrooveImage == null || state.GrooveRect == null)
        {
            return;
        }

        _hintedPiece = state;
        _hintedPieceBaseRotation = state.PieceRenderer.transform.rotation;
        _hintShakeStartTime = Time.unscaledTime;
        _isHintPieceShaking = true;
        CreatePieceHintOutline(state);
    }

    private void UpdatePieceHintAnimation()
    {
        if (_hintedPiece == null)
        {
            return;
        }

        var renderer = _hintedPiece.PieceRenderer;
        if (_hintedPiece.IsPlaced || renderer == null)
        {
            ClearPieceHint();
            return;
        }

        if (_drag.DraggingPiece == _hintedPiece)
        {
            renderer.transform.rotation = _hintedPieceBaseRotation;
            return;
        }

        if (!_isHintPieceShaking)
        {
            return;
        }

        var elapsed = Time.unscaledTime - _hintShakeStartTime;
        if (elapsed >= HintShakeDuration)
        {
            renderer.transform.rotation = _hintedPieceBaseRotation;
            _isHintPieceShaking = false;
            return;
        }

        var angle = Mathf.Sin(
            elapsed * HintShakeCyclesPerSecond * Mathf.PI * 2f) * HintShakeAngle;
        renderer.transform.rotation = _hintedPieceBaseRotation * Quaternion.Euler(0f, 0f, angle);
    }

    private void CreatePieceHintOutline(DraggablePieceState state)
    {
        var grooveRect = state.GrooveRect;
        var outlineObject = new GameObject(
            PieceHintOutlineObjectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(HintDashedOutlineGraphic));
        var outlineRect = outlineObject.GetComponent<RectTransform>();
        outlineRect.SetParent(grooveRect.parent, false);
        outlineRect.anchorMin = grooveRect.anchorMin;
        outlineRect.anchorMax = grooveRect.anchorMax;
        outlineRect.pivot = grooveRect.pivot;
        outlineRect.anchoredPosition = grooveRect.anchoredPosition;
        outlineRect.sizeDelta = grooveRect.sizeDelta;
        outlineRect.localRotation = grooveRect.localRotation;
        outlineRect.localScale = grooveRect.localScale;
        outlineRect.SetAsLastSibling();

        var outlineGraphic = outlineObject.GetComponent<HintDashedOutlineGraphic>();
        outlineGraphic.maskable = false;
        outlineGraphic.Configure(
            state.GrooveImage.sprite,
            PieceHintOutlineColor,
            HintOutlineWidth,
            HintDashLength,
            HintDashGap,
            HintOutlineScrollSpeed,
            state.GrooveImage.preserveAspect);
        _pieceHintOutlineRoot = outlineObject;
    }

    private void ClearPieceHint()
    {
        if (_hintedPiece?.PieceRenderer != null)
        {
            _hintedPiece.PieceRenderer.transform.rotation = _hintedPieceBaseRotation;
        }

        _hintedPiece = null;
        _hintedPieceBaseRotation = Quaternion.identity;
        _hintShakeStartTime = 0f;
        _isHintPieceShaking = false;
        if (_pieceHintOutlineRoot != null)
        {
            _pieceHintOutlineRoot.SetActive(false);
            Destroy(_pieceHintOutlineRoot);
            _pieceHintOutlineRoot = null;
        }
    }

    private void StartGameplayTimerIfNeeded()
    {
        if (_hasGameplayTimerStarted || _isGameFinished)
        {
            return;
        }

        _hasGameplayTimerStarted = true;
        _gameplayStartRealtime = Time.realtimeSinceStartup;
        Debug.Log("GameScene: gameplay score timer started after first Piece placement.");
    }

    private void StopGameplayTimer()
    {
        if (!_hasGameplayTimerStarted)
        {
            _completionTimeSeconds = 0f;
            return;
        }

        _completionTimeSeconds = Mathf.Max(0f, Time.realtimeSinceStartup - _gameplayStartRealtime);
        Debug.Log($"GameScene: gameplay score timer stopped at {_completionTimeSeconds:F2}s.");
    }

    private void InitializeTaskTracking()
    {
        GameTaskUtility.Initialize();
        _isAccumulateScoreTaskActive = GameTaskUtility.IsCurrentTaskAccumulateScore();
        if (_isAccumulateScoreTaskActive)
        {
            Debug.Log(
                $"GameScene: AccumulateScore task active. taskId={GameTaskUtility.GetCurrentTaskId()}, " +
                $"progress={GameTaskUtility.GetCurrentCompleteValue()}");
        }
    }

    private bool SaveCardPackAfterPuzzleComplete()
    {
        var packId = GameManager.GetBagId();
        if (packId <= 0)
        {
            return false;
        }

        if (!CardPackDataUtility.TrySavePackAfterPuzzleComplete(packId))
        {
            Debug.LogWarning($"GameScene: failed to save card pack data after puzzle complete. packId={packId}");
            return false;
        }

        Debug.Log($"GameScene: card pack data saved after puzzle complete. packId={packId}");
        return true;
    }

    private void TryGrantFirstCompletionPackReward()
    {
        var completedPackId = GameManager.GetBagId();
        if (_wasSelectedPackCompletedOnEntry)
        {
            Debug.Log($"GameScene: completion pack reward skipped for replay. packId={completedPackId}");
            return;
        }

        var granted = CardPackDistributionUtility.TryGrantFirstCompletionReward(
            completedPackId,
            out var grantedPackId,
            out var chapterId,
            out var decision);
        Debug.Log(
            $"GameScene: first-completion pack reward evaluated. completedPackId={completedPackId}, " +
            $"chapter={chapterId}, stage={decision.Stage}, R={decision.RemainingLockedCount}, " +
            $"held={decision.HeldPlayableCount}, maxHeldBeforeGrant={decision.MaximumHeldBeforeGrant}, " +
            $"expectedHeldAfterGrant={decision.ExpectedHeldAfterGrant}, granted={granted}, " +
            $"grantedPackId={grantedPackId}");
        if (granted)
        {
            QueuePackReward(grantedPackId);
        }
    }

    private IEnumerator ProcessTaskSettlement()
    {
        yield return ProcessTaskSettlementCore();

        if (_didSavePackCompletion
            && !_didFailTaskAdvanceDuringSettlement
            && (!_wasSelectedPackCompletedOnEntry || _didAdvanceTaskDuringSettlement))
        {
            TryGrantPendingTaskPackReward(
                _didAdvanceTaskDuringSettlement ? "task completion" : "first-completion retry");
        }

        if (_didSavePackCompletion)
        {
            TryGrantFirstCompletionPackReward();
        }
        RefreshSettlementBagCount();
        _isSettlementReadyForFinish = true;
        if (_finishButton != null)
        {
            _finishButton.interactable = true;
        }
    }

    private IEnumerator ProcessTaskSettlementCore()
    {
        if (_rewardPanelRoot == null)
        {
            yield break;
        }

        SetSettlementScore(0);
        RefreshSettlementBagCount();

        var packId = GameManager.GetBagId();
        var scoreContext = new GameScoreContext
        {
            WasHintUsed = _wasHintUsed,
            IsLevelOutlineEnabled = _isLevelOutlineEnabled,
            IsStickerOutlineEnabled = _isStickerOutlineEnabled,
            CompletionTimeSeconds = _completionTimeSeconds
        };
        if (!GameScoreUtility.TryCalculateCardPackScore(packId, scoreContext, out var scoreResult))
        {
            Debug.LogWarning($"GameScene: cannot calculate settlement score. Invalid card pack config. packId={packId}");
            SetTaskRewardSectionVisible(false);
            yield break;
        }

        var settlementScore = scoreResult.FinalScore;
        Debug.Log(
            $"GameScene: score calculated. base={scoreResult.BaseScore}, " +
            $"noHint=+{scoreResult.NoHintBonusPercent}%, " +
            $"levelOutlineOff=+{scoreResult.LevelOutlineDisabledBonusPercent}%, " +
            $"stickerOutlineOff=+{scoreResult.StickerOutlineDisabledBonusPercent}%, " +
            $"time=+{scoreResult.CompletionTimeBonusPercent}% ({scoreResult.CompletionTimeSeconds:F2}s), " +
            $"total=+{scoreResult.TotalBonusPercent}%, final={scoreResult.FinalScore}");

        if (!_isAccumulateScoreTaskActive
            || !GameTaskUtility.TryGetCurrentTaskConfig(out var taskConfig))
        {
            SetTaskRewardSectionVisible(false);
            SetSettlementScore(settlementScore);
            yield break;
        }

        var progressBeforeSettlement = GameTaskUtility.GetCurrentCompleteValue();
        if (!GameTaskUtility.AddCurrentScore(settlementScore))
        {
            Debug.LogWarning(
                $"GameScene: failed to add settlement score to current task. " +
                $"packId={packId}, score={settlementScore}");
            SetTaskRewardSectionVisible(false);
            SetSettlementScore(settlementScore);
            yield break;
        }

        var progressAfterSettlement = GameTaskUtility.GetCurrentCompleteValue();
        var isTaskCompleted = GameTaskUtility.IsCurrentTaskCompleted();
        Debug.Log(
            $"GameScene: settlement score added to task. packId={packId}, score={settlementScore}, " +
            $"progress={progressAfterSettlement}");

        SetTaskRewardSectionVisible(true);
        var taskItem = _rewardTaskItem;
        TaskProgressUIUtility.RefreshTask(
            taskItem,
            taskConfig,
            progressBeforeSettlement,
            isTaskCompleted);

        if (isTaskCompleted)
        {
            if (QueueTaskReward(taskConfig))
            {
                if (GameTaskUtility.TryCompleteAndAdvanceTask())
                {
                    _didAdvanceTaskDuringSettlement = true;
                    _isAccumulateScoreTaskActive = GameTaskUtility.IsCurrentTaskAccumulateScore();
                    Debug.Log($"GameScene: task advanced. nextTaskId={GameTaskUtility.GetCurrentTaskId()}");
                }
                else
                {
                    _didFailTaskAdvanceDuringSettlement = true;
                    Debug.LogError(
                        $"GameScene: task reward queued but task advance failed. taskId={taskConfig.TaskId}");
                }
            }
        }

        RefreshSettlementBagCount();
        yield return AnimateTaskSettlementProgress(
            taskItem,
            taskConfig,
            progressBeforeSettlement,
            progressAfterSettlement,
            settlementScore);
    }

    private bool QueueTaskReward(TaskConfigData taskConfig)
    {
        var preferredPackId = taskConfig.RewardType == RewardType.CardPack
            ? taskConfig.RewardId
            : 0;
        if (!CardPackDistributionUtility.EnqueueTaskReward(taskConfig.TaskId, preferredPackId))
        {
            Debug.LogError(
                $"GameScene: failed to persist guaranteed task reward. " +
                $"taskId={taskConfig.TaskId}, preferredPackId={preferredPackId}");
            return false;
        }

        Debug.Log(
            $"GameScene: guaranteed task reward queued. taskId={taskConfig.TaskId}, " +
            $"preferredPackId={preferredPackId}, " +
            $"pending={CardPackDistributionUtility.GetPendingTaskRewardCount()}");
        return true;
    }

    private void TryGrantPendingTaskPackReward(string source)
    {
        var granted = CardPackDistributionUtility.TryGrantPendingTaskReward(
            out var rewardPackId,
            out var chapterId,
            out var decision);
        Debug.Log(
            $"GameScene: pending task reward evaluated. source={source}, chapter={chapterId}, " +
            $"stage={decision.Stage}, R={decision.RemainingLockedCount}, " +
            $"held={decision.HeldPlayableCount}, maxHeldBeforeGrant={decision.MaximumHeldBeforeGrant}, " +
            $"granted={granted}, grantedPackId={rewardPackId}, " +
            $"pending={CardPackDistributionUtility.GetPendingTaskRewardCount()}");
        if (!granted)
        {
            return;
        }

        QueuePackReward(rewardPackId);
    }

    private void SetTaskRewardSectionVisible(bool visible)
    {
        if (_rewardTaskItem != null)
        {
            _rewardTaskItem.gameObject.SetActive(visible);
        }
    }

    private IEnumerator AnimateTaskSettlementProgress(
        Transform taskItem,
        TaskConfigData taskConfig,
        int progressBeforeSettlement,
        int progressAfterSettlement,
        int settlementScore)
    {
        TaskProgressUIUtility.SetProgress(taskItem, taskConfig, progressBeforeSettlement);
        SetSettlementScore(0);

        var elapsed = 0f;
        while (elapsed < TaskProgressRollDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalizedTime = Mathf.Clamp01(elapsed / TaskProgressRollDuration);
            var easedTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
            var animatedScore = Mathf.RoundToInt(Mathf.Lerp(0f, settlementScore, easedTime));
            var animatedTaskProgress = progressBeforeSettlement + animatedScore;

            SetSettlementScore(animatedScore);
            TaskProgressUIUtility.SetProgress(taskItem, taskConfig, animatedTaskProgress);
            yield return null;
        }

        SetSettlementScore(settlementScore);
        TaskProgressUIUtility.SetProgress(taskItem, taskConfig, progressAfterSettlement);
    }

    private void SetSettlementScore(int score)
    {
        if (_settlementScoreText == null)
        {
            return;
        }

        _settlementScoreText.text = Mathf.Max(0, score).ToString();
    }

    private void RefreshSettlementBagCount()
    {
        if (_settlementBagCountText == null)
        {
            return;
        }

        _settlementBagCountText.text = CardPackDataUtility.GetCompletedPackCount().ToString();
    }

    private void QueuePackReward(int rewardPackId)
    {
        if (rewardPackId > 0 && !_settlementPackRewardIds.Contains(rewardPackId))
        {
            _settlementPackRewardIds.Add(rewardPackId);
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

internal sealed class HintDashedOutlineGraphic : MaskableGraphic
{
    private const byte AlphaThreshold = 26;
    private const float OutlineSimplifyTolerancePixels = 0.75f;
    private static readonly Dictionary<Sprite, List<List<Vector2>>> sNormalizedPathCache =
        new Dictionary<Sprite, List<List<Vector2>>>();
    private readonly List<List<Vector2>> _spritePaths = new List<List<Vector2>>();
    private readonly List<Vector2> _mappedPath = new List<Vector2>();
    private Sprite _sourceSprite;
    private float _lineWidth = 3f;
    private float _dashLength = 5.6f;
    private float _dashGap = 4f;
    private float _scrollSpeed = 8f;
    private bool _preserveAspect;

    public static void ClearPathCache()
    {
        sNormalizedPathCache.Clear();
    }

    public void Configure(
        Sprite sourceSprite,
        Color lineColor,
        float lineWidth,
        float dashLength,
        float dashGap,
        float scrollSpeed,
        bool preserveAspect)
    {
        _sourceSprite = sourceSprite;
        color = lineColor;
        _lineWidth = Mathf.Max(0.5f, lineWidth);
        _dashLength = Mathf.Max(1f, dashLength);
        _dashGap = Mathf.Max(1f, dashGap);
        _scrollSpeed = scrollSpeed;
        _preserveAspect = preserveAspect;
        raycastTarget = false;
        RebuildSpritePaths();
        SetAllDirty();
    }

    private void Update()
    {
        if (_sourceSprite != null && _spritePaths.Count > 0 && Mathf.Abs(_scrollSpeed) > 0.001f)
        {
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        if (_sourceSprite == null || _spritePaths.Count == 0)
        {
            return;
        }

        var spriteBounds = _sourceSprite.bounds;
        if (spriteBounds.size.x <= 0.0001f || spriteBounds.size.y <= 0.0001f)
        {
            return;
        }

        var targetRect = GetDrawingRect(spriteBounds, rectTransform.rect);
        for (var pathIndex = 0; pathIndex < _spritePaths.Count; pathIndex++)
        {
            AddDashedPath(vertexHelper, _spritePaths[pathIndex], targetRect);
        }
    }

    private void AddDashedPath(
        VertexHelper vertexHelper,
        List<Vector2> spritePath,
        Rect targetRect)
    {
        if (spritePath == null || spritePath.Count < 2)
        {
            return;
        }

        _mappedPath.Clear();
        var pathLength = 0f;
        for (var i = 0; i < spritePath.Count; i++)
        {
            _mappedPath.Add(MapNormalizedPoint(spritePath[i], targetRect));
        }

        for (var i = 0; i < _mappedPath.Count; i++)
        {
            pathLength += Vector2.Distance(_mappedPath[i], _mappedPath[(i + 1) % _mappedPath.Count]);
        }

        if (pathLength <= 0.001f)
        {
            return;
        }

        var dashLength = Mathf.Min(_dashLength, pathLength * 0.6f);
        var dashCount = Mathf.Max(1, Mathf.FloorToInt(pathLength / (_dashLength + _dashGap)));
        var period = pathLength / dashCount;
        var offset = Mathf.Repeat(Time.unscaledTime * _scrollSpeed, period);
        var pathDistance = 0f;
        for (var i = 0; i < _mappedPath.Count; i++)
        {
            var from = _mappedPath[i];
            var to = _mappedPath[(i + 1) % _mappedPath.Count];
            var edge = to - from;
            var edgeLength = edge.magnitude;
            if (edgeLength <= 0.001f)
            {
                continue;
            }

            var vertexPhase = Mathf.Repeat(pathDistance + offset, period);
            if (vertexPhase > 0.001f && vertexPhase < dashLength - 0.001f)
            {
                AddRoundJoin(vertexHelper, from);
            }

            var edgeDirection = edge / edgeLength;
            var edgeOffset = 0f;
            while (edgeOffset < edgeLength - 0.001f)
            {
                var phase = Mathf.Repeat(pathDistance + edgeOffset + offset, period);
                var inDash = phase < dashLength;
                var remainingPatternLength = inDash
                    ? dashLength - phase
                    : period - phase;
                var step = Mathf.Min(
                    Mathf.Max(remainingPatternLength, 0.001f),
                    edgeLength - edgeOffset);
                if (inDash)
                {
                    AddLineSegment(
                        vertexHelper,
                        from + edgeDirection * edgeOffset,
                        from + edgeDirection * (edgeOffset + step));
                }

                edgeOffset += step;
            }

            pathDistance += edgeLength;
        }
    }

    private void AddRoundJoin(VertexHelper vertexHelper, Vector2 center)
    {
        const int segmentCount = 8;
        var centerIndex = vertexHelper.currentVertCount;
        AddVertex(vertexHelper, center);
        for (var i = 0; i <= segmentCount; i++)
        {
            var angle = i * Mathf.PI * 2f / segmentCount;
            var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (_lineWidth * 0.5f);
            AddVertex(vertexHelper, center + offset);
            if (i > 0)
            {
                vertexHelper.AddTriangle(centerIndex, centerIndex + i, centerIndex + i + 1);
            }
        }
    }

    private void AddLineSegment(VertexHelper vertexHelper, Vector2 from, Vector2 to)
    {
        var direction = to - from;
        if (direction.sqrMagnitude <= 0.000001f)
        {
            return;
        }

        direction.Normalize();
        var normal = new Vector2(-direction.y, direction.x) * (_lineWidth * 0.5f);
        var startIndex = vertexHelper.currentVertCount;
        AddVertex(vertexHelper, from - normal);
        AddVertex(vertexHelper, from + normal);
        AddVertex(vertexHelper, to + normal);
        AddVertex(vertexHelper, to - normal);
        vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vertexHelper.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
    }

    private void AddVertex(VertexHelper vertexHelper, Vector2 position)
    {
        var vertex = UIVertex.simpleVert;
        vertex.position = position;
        vertex.color = color;
        vertexHelper.AddVert(vertex);
    }

    private static Vector2 MapNormalizedPoint(Vector2 point, Rect targetRect)
    {
        return new Vector2(
            Mathf.Lerp(targetRect.xMin, targetRect.xMax, point.x),
            Mathf.Lerp(targetRect.yMin, targetRect.yMax, point.y));
    }

    private Rect GetDrawingRect(Bounds spriteBounds, Rect targetRect)
    {
        if (!_preserveAspect)
        {
            return targetRect;
        }

        var spriteAspect = spriteBounds.size.x / spriteBounds.size.y;
        var rectAspect = targetRect.width / targetRect.height;
        if (spriteAspect > rectAspect)
        {
            var originalHeight = targetRect.height;
            targetRect.height = targetRect.width / spriteAspect;
            targetRect.y += (originalHeight - targetRect.height) * rectTransform.pivot.y;
        }
        else
        {
            var originalWidth = targetRect.width;
            targetRect.width = targetRect.height * spriteAspect;
            targetRect.x += (originalWidth - targetRect.width) * rectTransform.pivot.x;
        }

        return targetRect;
    }

    private static bool TryBuildAlphaPaths(Sprite sprite, List<List<Vector2>> outputPaths)
    {
        if (!TryReadSpritePixels(sprite, out var pixels, out var width, out var height))
        {
            return false;
        }

        var outgoingEdges = new Dictionary<Vector2Int, List<Vector2Int>>();
        var unusedEdges = new HashSet<PixelEdge>();
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (!IsOpaque(pixels, width, height, x, y))
                {
                    continue;
                }

                if (!IsOpaque(pixels, width, height, x - 1, y))
                {
                    AddBoundaryEdge(outgoingEdges, unusedEdges, new Vector2Int(x, y), new Vector2Int(x, y + 1));
                }

                if (!IsOpaque(pixels, width, height, x, y + 1))
                {
                    AddBoundaryEdge(outgoingEdges, unusedEdges, new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1));
                }

                if (!IsOpaque(pixels, width, height, x + 1, y))
                {
                    AddBoundaryEdge(outgoingEdges, unusedEdges, new Vector2Int(x + 1, y + 1), new Vector2Int(x + 1, y));
                }

                if (!IsOpaque(pixels, width, height, x, y - 1))
                {
                    AddBoundaryEdge(outgoingEdges, unusedEdges, new Vector2Int(x + 1, y), new Vector2Int(x, y));
                }
            }
        }

        while (unusedEdges.Count > 0)
        {
            var startEdge = default(PixelEdge);
            foreach (var edge in unusedEdges)
            {
                startEdge = edge;
                break;
            }

            var pixelPath = TraceBoundary(startEdge, outgoingEdges, unusedEdges);
            if (pixelPath == null || pixelPath.Count < 3 || CalculateSignedArea(pixelPath) < 9f)
            {
                continue;
            }

            var simplifiedPath = SimplifyClosedPath(pixelPath, OutlineSimplifyTolerancePixels);
            if (simplifiedPath.Count < 3)
            {
                continue;
            }

            var normalizedPath = new List<Vector2>(simplifiedPath.Count);
            for (var pointIndex = 0; pointIndex < simplifiedPath.Count; pointIndex++)
            {
                var point = simplifiedPath[pointIndex];
                normalizedPath.Add(new Vector2(point.x / width, point.y / height));
            }

            outputPaths.Add(normalizedPath);
        }

        return outputPaths.Count > 0;
    }

    private static bool TryReadSpritePixels(
        Sprite sprite,
        out Color32[] pixels,
        out int width,
        out int height)
    {
        pixels = null;
        width = 0;
        height = 0;
        if (sprite == null || sprite.texture == null || sprite.packed)
        {
            return false;
        }

        var texture = sprite.texture;
        var spriteRect = sprite.textureRect;
        var sourceX = Mathf.RoundToInt(spriteRect.x);
        var sourceY = Mathf.RoundToInt(spriteRect.y);
        width = Mathf.RoundToInt(spriteRect.width);
        height = Mathf.RoundToInt(spriteRect.height);
        if (width <= 0 || height <= 0)
        {
            return false;
        }

        var previousRenderTexture = RenderTexture.active;
        var renderTexture = RenderTexture.GetTemporary(
            texture.width,
            texture.height,
            0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Linear);
        Texture2D readableTexture = null;
        try
        {
            renderTexture.filterMode = FilterMode.Point;
            Graphics.Blit(texture, renderTexture);
            RenderTexture.active = renderTexture;
            readableTexture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            readableTexture.ReadPixels(new Rect(sourceX, sourceY, width, height), 0, 0, false);
            pixels = readableTexture.GetPixels32();
            return pixels.Length == width * height;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"GameScene: failed to read hint Sprite alpha. {exception.Message}");
            pixels = null;
            return false;
        }
        finally
        {
            RenderTexture.active = previousRenderTexture;
            RenderTexture.ReleaseTemporary(renderTexture);
            if (readableTexture != null)
            {
                UnityEngine.Object.Destroy(readableTexture);
            }
        }
    }

    private static bool IsOpaque(Color32[] pixels, int width, int height, int x, int y)
    {
        return x >= 0
            && x < width
            && y >= 0
            && y < height
            && pixels[y * width + x].a >= AlphaThreshold;
    }

    private static void AddBoundaryEdge(
        Dictionary<Vector2Int, List<Vector2Int>> outgoingEdges,
        HashSet<PixelEdge> unusedEdges,
        Vector2Int from,
        Vector2Int to)
    {
        var edge = new PixelEdge(from, to);
        if (!unusedEdges.Add(edge))
        {
            return;
        }

        if (!outgoingEdges.TryGetValue(from, out var destinations))
        {
            destinations = new List<Vector2Int>(2);
            outgoingEdges[from] = destinations;
        }

        destinations.Add(to);
    }

    private static List<Vector2> TraceBoundary(
        PixelEdge startEdge,
        Dictionary<Vector2Int, List<Vector2Int>> outgoingEdges,
        HashSet<PixelEdge> unusedEdges)
    {
        var path = new List<Vector2>();
        var currentEdge = startEdge;
        var maxSteps = unusedEdges.Count + 1;
        path.Add(currentEdge.From);
        for (var step = 0; step < maxSteps; step++)
        {
            if (!unusedEdges.Remove(currentEdge))
            {
                return null;
            }

            path.Add(currentEdge.To);
            if (currentEdge.To == startEdge.From)
            {
                path.RemoveAt(path.Count - 1);
                return path;
            }

            if (!TryFindNextEdge(currentEdge, outgoingEdges, unusedEdges, out currentEdge))
            {
                return null;
            }
        }

        return null;
    }

    private static bool TryFindNextEdge(
        PixelEdge incomingEdge,
        Dictionary<Vector2Int, List<Vector2Int>> outgoingEdges,
        HashSet<PixelEdge> unusedEdges,
        out PixelEdge nextEdge)
    {
        nextEdge = default;
        if (!outgoingEdges.TryGetValue(incomingEdge.To, out var destinations))
        {
            return false;
        }

        var incomingDirection = incomingEdge.To - incomingEdge.From;
        var found = false;
        var bestTurnAngle = float.PositiveInfinity;
        for (var i = 0; i < destinations.Count; i++)
        {
            var candidate = new PixelEdge(incomingEdge.To, destinations[i]);
            if (!unusedEdges.Contains(candidate))
            {
                continue;
            }

            var outgoingDirection = candidate.To - candidate.From;
            var cross = incomingDirection.x * outgoingDirection.y - incomingDirection.y * outgoingDirection.x;
            var dot = incomingDirection.x * outgoingDirection.x
                + incomingDirection.y * outgoingDirection.y;
            var turnAngle = Mathf.Atan2(cross, dot);
            if (!found || turnAngle < bestTurnAngle)
            {
                found = true;
                bestTurnAngle = turnAngle;
                nextEdge = candidate;
            }
        }

        return found;
    }

    private static float CalculateSignedArea(List<Vector2> path)
    {
        var twiceArea = 0f;
        for (var i = 0; i < path.Count; i++)
        {
            var current = path[i];
            var next = path[(i + 1) % path.Count];
            twiceArea += current.x * next.y - next.x * current.y;
        }

        return Mathf.Abs(twiceArea) * 0.5f;
    }

    private static List<Vector2> SimplifyClosedPath(List<Vector2> path, float tolerance)
    {
        if (path.Count < 5)
        {
            return path;
        }

        var firstIndex = 0;
        var secondIndex = FindFarthestPointIndex(path, firstIndex);
        firstIndex = FindFarthestPointIndex(path, secondIndex);
        secondIndex = FindFarthestPointIndex(path, firstIndex);
        var firstArc = BuildPathArc(path, firstIndex, secondIndex);
        var secondArc = BuildPathArc(path, secondIndex, firstIndex);
        var simplifiedFirstArc = SimplifyOpenPath(firstArc, tolerance);
        var simplifiedSecondArc = SimplifyOpenPath(secondArc, tolerance);
        var result = new List<Vector2>(simplifiedFirstArc.Count + simplifiedSecondArc.Count - 2);
        result.AddRange(simplifiedFirstArc);
        for (var i = 1; i < simplifiedSecondArc.Count - 1; i++)
        {
            result.Add(simplifiedSecondArc[i]);
        }

        return result.Count >= 3 ? result : path;
    }

    private static int FindFarthestPointIndex(List<Vector2> path, int sourceIndex)
    {
        var farthestIndex = sourceIndex;
        var farthestDistance = -1f;
        for (var i = 0; i < path.Count; i++)
        {
            var distance = (path[i] - path[sourceIndex]).sqrMagnitude;
            if (distance > farthestDistance)
            {
                farthestDistance = distance;
                farthestIndex = i;
            }
        }

        return farthestIndex;
    }

    private static List<Vector2> BuildPathArc(List<Vector2> path, int startIndex, int endIndex)
    {
        var arc = new List<Vector2>();
        var index = startIndex;
        arc.Add(path[index]);
        while (index != endIndex)
        {
            index = (index + 1) % path.Count;
            arc.Add(path[index]);
        }

        return arc;
    }

    private static List<Vector2> SimplifyOpenPath(List<Vector2> path, float tolerance)
    {
        if (path.Count <= 2)
        {
            return path;
        }

        var keep = new bool[path.Count];
        keep[0] = true;
        keep[path.Count - 1] = true;
        var ranges = new Stack<Vector2Int>();
        ranges.Push(new Vector2Int(0, path.Count - 1));
        var toleranceSquared = tolerance * tolerance;
        while (ranges.Count > 0)
        {
            var range = ranges.Pop();
            var farthestIndex = -1;
            var farthestDistance = toleranceSquared;
            for (var i = range.x + 1; i < range.y; i++)
            {
                var distance = DistanceToSegmentSquared(path[i], path[range.x], path[range.y]);
                if (distance > farthestDistance)
                {
                    farthestDistance = distance;
                    farthestIndex = i;
                }
            }

            if (farthestIndex < 0)
            {
                continue;
            }

            keep[farthestIndex] = true;
            ranges.Push(new Vector2Int(range.x, farthestIndex));
            ranges.Push(new Vector2Int(farthestIndex, range.y));
        }

        var result = new List<Vector2>();
        for (var i = 0; i < path.Count; i++)
        {
            if (keep[i])
            {
                result.Add(path[i]);
            }
        }

        return result;
    }

    private static float DistanceToSegmentSquared(Vector2 point, Vector2 from, Vector2 to)
    {
        var segment = to - from;
        if (segment.sqrMagnitude <= 0.000001f)
        {
            return (point - from).sqrMagnitude;
        }

        var t = Mathf.Clamp01(Vector2.Dot(point - from, segment) / segment.sqrMagnitude);
        return (point - (from + segment * t)).sqrMagnitude;
    }

    private void RebuildSpritePaths()
    {
        _spritePaths.Clear();
        if (_sourceSprite == null)
        {
            return;
        }

        if (sNormalizedPathCache.TryGetValue(_sourceSprite, out var cachedPaths))
        {
            _spritePaths.AddRange(cachedPaths);
            return;
        }

        if (TryBuildAlphaPaths(_sourceSprite, _spritePaths))
        {
            sNormalizedPathCache[_sourceSprite] = new List<List<Vector2>>(_spritePaths);
            return;
        }

        Debug.LogWarning(
            $"GameScene: hint outline could not read Sprite alpha; using simplified Physics Shape. "
            + $"sprite={_sourceSprite.name}");
        var bounds = _sourceSprite.bounds;
        var shapeCount = _sourceSprite.GetPhysicsShapeCount();
        for (var shapeIndex = 0; shapeIndex < shapeCount; shapeIndex++)
        {
            var path = new List<Vector2>();
            _sourceSprite.GetPhysicsShape(shapeIndex, path);
            if (path.Count >= 2)
            {
                for (var pointIndex = 0; pointIndex < path.Count; pointIndex++)
                {
                    var point = path[pointIndex];
                    path[pointIndex] = new Vector2(
                        (point.x - bounds.min.x) / bounds.size.x,
                        (point.y - bounds.min.y) / bounds.size.y);
                }

                _spritePaths.Add(path);
            }
        }

        if (_spritePaths.Count == 0)
        {
            _spritePaths.Add(new List<Vector2>
            {
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 0f)
            });
        }

        sNormalizedPathCache[_sourceSprite] = new List<List<Vector2>>(_spritePaths);
    }

    private readonly struct PixelEdge : IEquatable<PixelEdge>
    {
        public PixelEdge(Vector2Int from, Vector2Int to)
        {
            From = from;
            To = to;
        }

        public Vector2Int From { get; }
        public Vector2Int To { get; }

        public bool Equals(PixelEdge other)
        {
            return From == other.From && To == other.To;
        }

        public override bool Equals(object obj)
        {
            return obj is PixelEdge other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (From.GetHashCode() * 397) ^ To.GetHashCode();
            }
        }
    }
}
