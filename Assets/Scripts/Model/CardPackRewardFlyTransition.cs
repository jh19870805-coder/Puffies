using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class CardPackRewardFlyTransition : MonoBehaviour
{
    private const string TransitionObjectName = "CardPackRewardFlyTransition";
    private const float SceneCrossFadeDuration = 0.3f;
    private const float TargetMoveDuration = 0.72f;
    private const float TargetMoveStagger = 0.12f;
    private const float RewardRevealEffectPlaceholderDuration = 0.2f;
    private const float TargetLookupTimeout = 5f;
    private const float TargetArcHeight = 72f;
    private const int TransitionSortingOrder = 32000;
    private static CardPackRewardFlyTransition sInstance;

    private sealed class FlyIcon
    {
        public int PackId;
        public RectTransform RectTransform;
        public Image Image;
        public Image SourceImage;
        public bool HasLanded;
        public bool HasRevealed;
        public float LandedAt;
    }

    private readonly List<int> mPackIds = new List<int>();
    private readonly List<FlyIcon> mIcons = new List<FlyIcon>();
    private Canvas mCanvas;
    private RectTransform mCanvasRect;
    private RawImage mSceneSnapshotImage;
    private RenderTexture mSceneSnapshotTexture;
    private MainScene mPreparedMainScene;

    public static bool IsActive => sInstance != null;

    public static bool IsPackPending(int packId)
    {
        return sInstance != null && sInstance.mPackIds.Contains(packId);
    }

    public static bool TryStart(
        IReadOnlyList<RectTransform> sources,
        IReadOnlyList<int> packIds)
    {
        if (sInstance != null || packIds == null)
        {
            return false;
        }

        var uniquePackIds = new List<int>(packIds.Count);
        var uniqueSources = new List<RectTransform>(packIds.Count);
        for (var i = 0; i < packIds.Count; i++)
        {
            var packId = packIds[i];
            if (packId > 0 && !uniquePackIds.Contains(packId))
            {
                uniquePackIds.Add(packId);
                uniqueSources.Add(sources != null && i < sources.Count ? sources[i] : null);
            }
        }

        var transitionObject = new GameObject(
            TransitionObjectName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CardPackRewardFlyTransition));
        DontDestroyOnLoad(transitionObject);

        var transition = transitionObject.GetComponent<CardPackRewardFlyTransition>();
        sInstance = transition;
        if (transition.Initialize(uniqueSources, uniquePackIds))
        {
            return true;
        }

        sInstance = null;
        Destroy(transitionObject);
        return false;
    }

    private bool Initialize(
        IReadOnlyList<RectTransform> sources,
        IReadOnlyList<int> packIds)
    {
        mPackIds.AddRange(packIds);
        mCanvas = GetComponent<Canvas>();
        mCanvasRect = GetComponent<RectTransform>();
        mCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mCanvas.overrideSorting = true;
        mCanvas.sortingOrder = TransitionSortingOrder;

        var scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(GameDefine.DesignWidth, GameDefine.DesignHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        CreateInputBlocker();
        CreateSceneSnapshotLayer();
        Canvas.ForceUpdateCanvases();
        for (var i = 0; i < mPackIds.Count; i++)
        {
            var packId = mPackIds[i];
            var source = sources != null && i < sources.Count ? sources[i] : null;
            var sourceImage = source != null ? source.GetComponent<Image>() : null;
            if (sourceImage == null
                || sourceImage.sprite == null
                || !TryGetOverlayGeometry(source, out var sourcePosition, out var sourceSize))
            {
                Debug.LogWarning(
                    $"CardPackRewardFlyTransition: settlement reward source missing. packId={packId}");
                return false;
            }

            var iconObject = new GameObject(
                $"RewardPack_{packId:D3}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.SetParent(mCanvasRect, false);
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = sourcePosition;
            iconRect.sizeDelta = sourceSize;

            var iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = sourceImage.sprite;
            iconImage.color = sourceImage.color;
            iconImage.preserveAspect = sourceImage.preserveAspect;
            iconImage.raycastTarget = false;
            mIcons.Add(new FlyIcon
            {
                PackId = packId,
                RectTransform = iconRect,
                Image = iconImage,
                SourceImage = sourceImage
            });
        }

        StartCoroutine(PlayTransition());
        return true;
    }

    private void CreateInputBlocker()
    {
        var blockerObject = new GameObject(
            "InputBlocker",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        var blockerRect = blockerObject.GetComponent<RectTransform>();
        blockerRect.SetParent(mCanvasRect, false);
        blockerRect.anchorMin = Vector2.zero;
        blockerRect.anchorMax = Vector2.one;
        blockerRect.offsetMin = Vector2.zero;
        blockerRect.offsetMax = Vector2.zero;

        var blockerImage = blockerObject.GetComponent<Image>();
        blockerImage.color = new Color(0f, 0f, 0f, 0f);
        blockerImage.raycastTarget = true;
    }

    private void CreateSceneSnapshotLayer()
    {
        var snapshotObject = new GameObject(
            "SceneSnapshot",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(RawImage));
        var snapshotRect = snapshotObject.GetComponent<RectTransform>();
        snapshotRect.SetParent(mCanvasRect, false);
        snapshotRect.anchorMin = Vector2.zero;
        snapshotRect.anchorMax = Vector2.one;
        snapshotRect.offsetMin = Vector2.zero;
        snapshotRect.offsetMax = Vector2.zero;

        mSceneSnapshotImage = snapshotObject.GetComponent<RawImage>();
        mSceneSnapshotImage.color = Color.white;
        mSceneSnapshotImage.raycastTarget = false;
        mSceneSnapshotImage.enabled = false;
    }

    private IEnumerator PlayTransition()
    {
        yield return CaptureSceneSnapshot();
        GameManager.EnterMainScene();
        yield return null;

        var mainScene = default(MainScene);
        var elapsed = 0f;
        while (elapsed < TargetLookupTimeout)
        {
            elapsed += Time.unscaledDeltaTime;
            mainScene = FindObjectOfType<MainScene>();
            if (mainScene != null && mainScene.TryPreparePackageRewardEntrance(mPackIds))
            {
                mPreparedMainScene = mainScene;
                break;
            }

            yield return null;
        }

        if (mPreparedMainScene == null)
        {
            Debug.LogWarning(
                "CardPackRewardFlyTransition: MainScene package list was not ready before timeout.");
            Destroy(gameObject);
            yield break;
        }

        var targetPositions = new Vector2[mIcons.Count];
        var targetSizes = new Vector2[mIcons.Count];
        for (var i = 0; i < mIcons.Count; i++)
        {
            if (!mPreparedMainScene.TryGetPackageRewardTargetScreenRect(
                    mIcons[i].PackId,
                    out var targetScreenRect)
                || !TryGetOverlayGeometry(
                    targetScreenRect,
                    out targetPositions[i],
                    out targetSizes[i]))
            {
                Debug.LogWarning(
                    $"CardPackRewardFlyTransition: target slot missing. packId={mIcons[i].PackId}");
                mPreparedMainScene.CancelPackageRewardEntrance();
                Destroy(gameObject);
                yield break;
            }
        }

        yield return AnimateSceneSnapshotFade();
        if (mIcons.Count > 0)
        {
            yield return AnimateRewardIconsIntoTargets(targetPositions, targetSizes);
        }

        yield return mPreparedMainScene.AnimateRemainingPackageRewardEntrance();
        mPreparedMainScene = null;
        Destroy(gameObject);
    }

    private IEnumerator CaptureSceneSnapshot()
    {
        if (mSceneSnapshotImage == null)
        {
            yield break;
        }

        var sourceImageStates = new Dictionary<Image, bool>();
        for (var i = 0; i < mIcons.Count; i++)
        {
            var icon = mIcons[i];
            if (icon.SourceImage != null && !sourceImageStates.ContainsKey(icon.SourceImage))
            {
                sourceImageStates.Add(icon.SourceImage, icon.SourceImage.enabled);
                icon.SourceImage.enabled = false;
            }

            if (icon.Image != null)
            {
                icon.Image.enabled = false;
            }
        }

        yield return new WaitForEndOfFrame();

        try
        {
            var width = Mathf.Max(1, Screen.width);
            var height = Mathf.Max(1, Screen.height);
            mSceneSnapshotTexture = new RenderTexture(
                width,
                height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Default)
            {
                name = "SettlementSceneTransitionSnapshot",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            mSceneSnapshotTexture.Create();
            ScreenCapture.CaptureScreenshotIntoRenderTexture(mSceneSnapshotTexture);
            mSceneSnapshotImage.texture = mSceneSnapshotTexture;
            mSceneSnapshotImage.color = Color.white;
            mSceneSnapshotImage.enabled = true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"CardPackRewardFlyTransition: settlement snapshot capture failed. error={exception.Message}");
            ReleaseSceneSnapshot();
        }
        finally
        {
            foreach (var pair in sourceImageStates)
            {
                if (pair.Key != null)
                {
                    pair.Key.enabled = pair.Value;
                }
            }

            for (var i = 0; i < mIcons.Count; i++)
            {
                if (mIcons[i].Image != null)
                {
                    mIcons[i].Image.enabled = true;
                }
            }
        }
    }

    private IEnumerator AnimateSceneSnapshotFade()
    {
        if (mSceneSnapshotImage == null || !mSceneSnapshotImage.enabled)
        {
            yield break;
        }

        var elapsed = 0f;
        while (elapsed < SceneCrossFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(elapsed / SceneCrossFadeDuration);
            var eased = Mathf.SmoothStep(0f, 1f, normalized);
            mSceneSnapshotImage.color = new Color(1f, 1f, 1f, 1f - eased);
            yield return null;
        }

        ReleaseSceneSnapshot();
    }

    private void ReleaseSceneSnapshot()
    {
        if (mSceneSnapshotImage != null)
        {
            mSceneSnapshotImage.enabled = false;
            mSceneSnapshotImage.texture = null;
        }

        if (mSceneSnapshotTexture == null)
        {
            return;
        }

        if (mSceneSnapshotTexture.IsCreated())
        {
            mSceneSnapshotTexture.Release();
        }

        Destroy(mSceneSnapshotTexture);
        mSceneSnapshotTexture = null;
    }

    private IEnumerator AnimateRewardIconsIntoTargets(
        Vector2[] targetPositions,
        Vector2[] targetSizes)
    {
        var startPositions = new Vector2[mIcons.Count];
        var startSizes = new Vector2[mIcons.Count];
        for (var i = 0; i < mIcons.Count; i++)
        {
            startPositions[i] = mIcons[i].RectTransform.anchoredPosition;
            startSizes[i] = mIcons[i].RectTransform.sizeDelta;
        }

        var totalDuration = TargetMoveDuration
                            + TargetMoveStagger * Mathf.Max(0, mIcons.Count - 1)
                            + RewardRevealEffectPlaceholderDuration;
        var elapsed = 0f;
        while (elapsed < totalDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            for (var i = 0; i < mIcons.Count; i++)
            {
                var icon = mIcons[i];
                var iconElapsed = elapsed - TargetMoveStagger * i;
                var normalized = Mathf.Clamp01(iconElapsed / TargetMoveDuration);
                var eased = Mathf.SmoothStep(0f, 1f, normalized);
                var arc = Mathf.Sin(normalized * Mathf.PI) * TargetArcHeight;
                var position = Vector2.LerpUnclamped(startPositions[i], targetPositions[i], eased);
                position.y += arc;
                icon.RectTransform.anchoredPosition = position;
                icon.RectTransform.sizeDelta = Vector2.LerpUnclamped(
                    startSizes[i],
                    targetSizes[i],
                    eased);

                if (!icon.HasLanded && normalized >= 1f)
                {
                    icon.HasLanded = true;
                    icon.LandedAt = elapsed;
                }

                // The imported landing flash will play during this reserved interval.
                if (icon.HasLanded
                    && !icon.HasRevealed
                    && elapsed - icon.LandedAt >= RewardRevealEffectPlaceholderDuration)
                {
                    icon.HasRevealed = true;
                    mPreparedMainScene.RevealPackageRewardTarget(icon.PackId);
                    icon.Image.enabled = false;
                }
            }

            yield return null;
        }

        for (var i = 0; i < mIcons.Count; i++)
        {
            var icon = mIcons[i];
            icon.RectTransform.anchoredPosition = targetPositions[i];
            icon.RectTransform.sizeDelta = targetSizes[i];
            if (!icon.HasRevealed)
            {
                mPreparedMainScene.RevealPackageRewardTarget(icon.PackId);
                icon.Image.enabled = false;
            }
        }
    }

    private bool TryGetOverlayGeometry(
        RectTransform source,
        out Vector2 localPosition,
        out Vector2 localSize)
    {
        localPosition = Vector2.zero;
        localSize = Vector2.zero;
        if (source == null || mCanvasRect == null)
        {
            return false;
        }

        var sourceCanvas = source.GetComponentInParent<Canvas>();
        var rootCanvas = sourceCanvas != null ? sourceCanvas.rootCanvas : null;
        var eventCamera = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? rootCanvas.worldCamera ?? Camera.main
            : null;
        var corners = new Vector3[4];
        source.GetWorldCorners(corners);
        var bottomLeftScreen = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[0]);
        var topRightScreen = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[2]);
        var centerScreen = (bottomLeftScreen + topRightScreen) * 0.5f;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mCanvasRect,
                centerScreen,
                null,
                out localPosition)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mCanvasRect,
                bottomLeftScreen,
                null,
                out var bottomLeft)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mCanvasRect,
                topRightScreen,
                null,
                out var topRight))
        {
            return false;
        }

        localSize = new Vector2(
            Mathf.Abs(topRight.x - bottomLeft.x),
            Mathf.Abs(topRight.y - bottomLeft.y));
        return localSize.x > 0.01f && localSize.y > 0.01f;
    }

    private bool TryGetOverlayGeometry(
        Rect screenRect,
        out Vector2 localPosition,
        out Vector2 localSize)
    {
        localPosition = Vector2.zero;
        localSize = Vector2.zero;
        if (mCanvasRect == null
            || screenRect.width <= 0.01f
            || screenRect.height <= 0.01f
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mCanvasRect,
                screenRect.center,
                null,
                out localPosition)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mCanvasRect,
                screenRect.min,
                null,
                out var minimum)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mCanvasRect,
                screenRect.max,
                null,
                out var maximum))
        {
            return false;
        }

        localSize = new Vector2(
            Mathf.Abs(maximum.x - minimum.x),
            Mathf.Abs(maximum.y - minimum.y));
        return localSize.x > 0.01f && localSize.y > 0.01f;
    }

    private void OnDestroy()
    {
        ReleaseSceneSnapshot();
        if (mPreparedMainScene != null)
        {
            mPreparedMainScene.CancelPackageRewardEntrance();
            mPreparedMainScene = null;
        }

        if (sInstance == this)
        {
            sInstance = null;
        }
    }
}

public sealed class CardPackGameEntranceTransition : MonoBehaviour
{
    private const int GameSceneSettleFrameCount = 2;
    private const float PlaybackGraceSeconds = 1f;
    private const float SlowDropDurationRatio = 0.42f;
    private const float TornPackPieceLaunchPackHeightRatio = 0.72f;
    private const float ProgressPieceRetractionParentHeightRatio = 0.28f;

    private static CardPackGameEntranceTransition sInstance;

    private readonly List<RectTransform> mPieceRects = new List<RectTransform>();
    private readonly List<Vector2> mPieceStartPositions = new List<Vector2>();
    private readonly List<Vector2> mPieceRetractionTargets = new List<Vector2>();
    private Canvas mCanvas;
    private RectTransform mPackRect;
    private Vector2 mPackStart;
    private Vector2 mPackLaunchPoint;
    private Vector2 mPackTarget;
    private float mDuration;
    private float mSlowDropDuration;
    private float mFastDropDuration;
    private bool mRetractProgressPiecesOnDrop;
    private bool mUseContinuousLinearDrop;
    private bool mReachedPieceLaunch;
    private Material mOwnedCoverMaterial;
    private Texture mOwnedMaskTexture;

    public static bool IsPending => sInstance != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        sInstance = null;
    }

    public static bool TryBegin(
        Canvas canvas,
        RectTransform packRect,
        Image coverImage,
        IReadOnlyList<RectTransform> pieceRects,
        float dropDistance,
        float horizontalSpread,
        float pieceVerticalCompensation,
        float duration,
        bool retractProgressPiecesOnDrop,
        bool useContinuousLinearDrop)
    {
        if (sInstance != null
            || canvas == null
            || packRect == null
            || duration <= 0f)
        {
            return false;
        }

        var transition = canvas.gameObject.AddComponent<CardPackGameEntranceTransition>();
        if (!transition.Initialize(
                canvas,
                packRect,
                coverImage,
                pieceRects,
                dropDistance,
                horizontalSpread,
                pieceVerticalCompensation,
                duration,
                retractProgressPiecesOnDrop,
                useContinuousLinearDrop))
        {
            Destroy(transition);
            return false;
        }

        sInstance = transition;
        DontDestroyOnLoad(canvas.gameObject);
        Debug.Log(
            $"CardPackGameEntranceTransition: prepared. "
            + $"pieces={transition.mPieceRects.Count}, duration={duration:F2}s, "
            + $"canvasActive={canvas.gameObject.activeInHierarchy}");
        return true;
    }

    public static void NotifyGameSceneReady(Camera gameCamera)
    {
        if (sInstance == null)
        {
            return;
        }

        if (sInstance.mCanvas != null
            && sInstance.mCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            sInstance.mCanvas.worldCamera = gameCamera;
        }

        Debug.Log(
            $"CardPackGameEntranceTransition: GameScene camera bound. "
            + $"canvasActive={sInstance.gameObject.activeInHierarchy}");
    }

    public static IEnumerator WaitForCompletion()
    {
        yield return WaitForPieceLaunch();
        yield return FinishAfterPieceLaunch();
    }

    public static IEnumerator WaitForPieceLaunch()
    {
        var transition = sInstance;
        if (transition == null)
        {
            yield break;
        }

        if (!transition.gameObject.activeSelf)
        {
            transition.gameObject.SetActive(true);
        }

        Debug.Log("CardPackGameEntranceTransition: slow drop started.");
        var playback = transition.PlayToPieceLaunch();
        var deadline = Time.realtimeSinceStartup
                       + Mathf.Max(0f, transition.mSlowDropDuration)
                       + PlaybackGraceSeconds;
        while (sInstance == transition && Time.realtimeSinceStartup < deadline)
        {
            var hasNext = false;
            try
            {
                hasNext = playback.MoveNext();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogWarning(
                    "CardPackGameEntranceTransition: slow drop failed; "
                    + "forcing the piece launch point.");
                break;
            }

            if (!hasNext)
            {
                break;
            }

            yield return playback.Current;
        }

        if (sInstance == transition)
        {
            transition.ReachPieceLaunchImmediately();
        }

        Debug.Log("CardPackGameEntranceTransition: piece launch point reached.");
    }

    public static IEnumerator FinishAfterPieceLaunch()
    {
        var transition = sInstance;
        if (transition == null)
        {
            yield break;
        }

        transition.ReachPieceLaunchImmediately();
        Debug.Log("CardPackGameEntranceTransition: accelerated exit started.");
        var playback = transition.PlayAcceleratedExit();
        var deadline = Time.realtimeSinceStartup
                       + Mathf.Max(0f, transition.mFastDropDuration)
                       + PlaybackGraceSeconds;
        while (sInstance == transition && Time.realtimeSinceStartup < deadline)
        {
            var hasNext = false;
            try
            {
                hasNext = playback.MoveNext();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogWarning(
                    "CardPackGameEntranceTransition: accelerated exit failed; "
                    + "forcing final state so GameScene can continue.");
                break;
            }

            if (!hasNext)
            {
                break;
            }

            yield return playback.Current;
        }

        if (sInstance == transition)
        {
            transition.CompleteImmediately(storeExitPosition: false);
        }

        Debug.Log("CardPackGameEntranceTransition: GameScene playback completed.");
    }

    public static void CancelPending()
    {
        if (sInstance != null)
        {
            sInstance.CompleteImmediately(storeExitPosition: false);
        }
    }

    private bool Initialize(
        Canvas canvas,
        RectTransform packRect,
        Image coverImage,
        IReadOnlyList<RectTransform> pieceRects,
        float dropDistance,
        float horizontalSpread,
        float pieceVerticalCompensation,
        float duration,
        bool retractProgressPiecesOnDrop,
        bool useContinuousLinearDrop)
    {
        mCanvas = canvas;
        mPackRect = packRect;
        mDuration = duration;
        mRetractProgressPiecesOnDrop = retractProgressPiecesOnDrop;
        mUseContinuousLinearDrop = useContinuousLinearDrop;
        mPackStart = packRect.anchoredPosition;
        var displayedPackHeight = GetDisplayedHeightInParent(packRect);
        if (mUseContinuousLinearDrop)
        {
            dropDistance = Mathf.Max(
                dropDistance,
                GetDropDistanceToClearParentBottom(packRect));
        }

        var launchPackHeight = mUseContinuousLinearDrop
            ? displayedPackHeight * TornPackPieceLaunchPackHeightRatio
            : displayedPackHeight;
        var launchDropDistance = displayedPackHeight > 0.01f
            ? Mathf.Min(dropDistance, launchPackHeight)
            : dropDistance * SlowDropDurationRatio;
        mPackLaunchPoint = mPackStart
                           + Vector2.down * launchDropDistance;
        mPackTarget = mPackStart + Vector2.down * dropDistance;
        var slowDurationRatio = dropDistance > 0.01f
            ? Mathf.Clamp(launchDropDistance / dropDistance, 0.2f, 0.8f)
            : SlowDropDurationRatio;
        mSlowDropDuration = duration * slowDurationRatio;
        mFastDropDuration = Mathf.Max(0.01f, duration - mSlowDropDuration);

        if (pieceRects != null)
        {
            for (var i = 0; i < pieceRects.Count; i++)
            {
                if (pieceRects[i] != null)
                {
                    var pieceRect = pieceRects[i];
                    mPieceRects.Add(pieceRect);
                    mPieceStartPositions.Add(pieceRect.anchoredPosition);
                    var pieceParent = pieceRect.parent as RectTransform;
                    var parentHeight = pieceParent != null
                        ? pieceParent.rect.height
                        : packRect.rect.height;
                    mPieceRetractionTargets.Add(
                        pieceRect.anchoredPosition
                        + Vector2.down
                        * parentHeight
                        * ProgressPieceRetractionParentHeightRatio);
                }
            }
        }

        CloneTransientCoverMaterial(coverImage);
        StorePieceOriginPosition();
        return true;
    }

    private static float GetDisplayedHeightInParent(RectTransform rectTransform)
    {
        if (rectTransform == null || rectTransform.parent == null)
        {
            return 0f;
        }

        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        var bottom = rectTransform.parent.InverseTransformPoint(corners[0]);
        var top = rectTransform.parent.InverseTransformPoint(corners[1]);
        return Mathf.Abs(top.y - bottom.y);
    }

    private static float GetDropDistanceToClearParentBottom(RectTransform rectTransform)
    {
        var parentRect = rectTransform != null
            ? rectTransform.parent as RectTransform
            : null;
        if (parentRect == null)
        {
            return 0f;
        }

        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        var top = float.NegativeInfinity;
        for (var i = 0; i < corners.Length; i++)
        {
            top = Mathf.Max(top, parentRect.InverseTransformPoint(corners[i]).y);
        }

        return Mathf.Max(0f, top - parentRect.rect.yMin + 1f);
    }

    private void CloneTransientCoverMaterial(Image coverImage)
    {
        if (coverImage == null || coverImage.material == null)
        {
            return;
        }

        var sourceMaterial = coverImage.material;
        var tornMaskTextureId = Shader.PropertyToID("_TornMaskTex");
        var useTornMaskId = Shader.PropertyToID("_UseTornMask");
        if (!sourceMaterial.HasProperty(tornMaskTextureId)
            || !sourceMaterial.HasProperty(useTornMaskId)
            || sourceMaterial.GetFloat(useTornMaskId) <= 0f)
        {
            return;
        }

        mOwnedCoverMaterial = new Material(sourceMaterial)
        {
            name = sourceMaterial.name + " (Game Entrance)"
        };
        var sourceMask = sourceMaterial.GetTexture(tornMaskTextureId);
        if (sourceMask != null)
        {
            mOwnedMaskTexture = Instantiate(sourceMask);
            mOwnedCoverMaterial.SetTexture(tornMaskTextureId, mOwnedMaskTexture);
        }

        coverImage.material = mOwnedCoverMaterial;
    }

    private IEnumerator PlayToPieceLaunch()
    {
        if (mReachedPieceLaunch)
        {
            yield break;
        }

        for (var frame = 0; frame < GameSceneSettleFrameCount; frame++)
        {
            yield return null;
        }

        var elapsed = 0f;
        while (elapsed < mSlowDropDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(elapsed / mSlowDropDuration);
            var packT = mUseContinuousLinearDrop
                ? normalized
                : Mathf.SmoothStep(0f, 1f, normalized);
            if (mPackRect != null)
            {
                mPackRect.anchoredPosition = Vector2.LerpUnclamped(
                    mPackStart,
                    mPackLaunchPoint,
                    packT);
            }

            if (mRetractProgressPiecesOnDrop)
            {
                RetractProgressPieces(normalized);
            }

            yield return null;
        }

        ReachPieceLaunchImmediately();
    }

    private void RetractProgressPieces(float normalized)
    {
        for (var i = 0; i < mPieceRects.Count; i++)
        {
            var pieceRect = mPieceRects[i];
            if (pieceRect == null)
            {
                continue;
            }

            pieceRect.anchoredPosition = Vector2.LerpUnclamped(
                mPieceStartPositions[i],
                mPieceRetractionTargets[i],
                Mathf.SmoothStep(0f, 1f, normalized));
            if (normalized >= 1f)
            {
                pieceRect.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator PlayAcceleratedExit()
    {
        var elapsed = 0f;
        while (elapsed < mFastDropDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(elapsed / mFastDropDuration);
            var packT = mUseContinuousLinearDrop
                ? normalized
                : normalized * normalized * normalized;
            if (mPackRect != null)
            {
                mPackRect.anchoredPosition = Vector2.LerpUnclamped(
                    mPackLaunchPoint,
                    mPackTarget,
                    packT);
            }

            yield return null;
        }

        if (mPackRect != null)
        {
            mPackRect.anchoredPosition = mPackTarget;
        }

        CompleteImmediately(storeExitPosition: false);
    }

    private void ReachPieceLaunchImmediately()
    {
        if (mReachedPieceLaunch)
        {
            return;
        }

        mReachedPieceLaunch = true;
        if (mPackRect != null)
        {
            mPackRect.anchoredPosition = mPackLaunchPoint;
        }

        for (var i = 0; i < mPieceRects.Count; i++)
        {
            if (mPieceRects[i] != null)
            {
                mPieceRects[i].gameObject.SetActive(false);
            }
        }
    }

    private void CompleteImmediately(bool storeExitPosition)
    {
        if (mPackRect != null)
        {
            mPackRect.anchoredPosition = mPackTarget;
        }

        if (storeExitPosition)
        {
            StorePieceOriginPosition();
        }

        if (mCanvas != null)
        {
            mCanvas.gameObject.SetActive(false);
        }

        if (sInstance == this)
        {
            sInstance = null;
        }

        Debug.Log(
            $"CardPackGameEntranceTransition: released. "
            + $"storedExitPosition={storeExitPosition}");
        Destroy(gameObject);
    }

    private void StorePieceOriginPosition()
    {
        if (mPackRect == null || Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        var camera = mCanvas != null && mCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? mCanvas.worldCamera
            : null;
        var corners = new Vector3[4];
        mPackRect.GetWorldCorners(corners);
        var bottomLeft = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        var topRight = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
        var screenPosition = (bottomLeft + topRight) * 0.5f;
        GameManager.SetOpeningPackExitPosition(new Vector2(
            screenPosition.x / Screen.width,
            screenPosition.y / Screen.height));
    }

    private void OnDestroy()
    {
        if (mOwnedCoverMaterial != null)
        {
            Destroy(mOwnedCoverMaterial);
        }

        if (mOwnedMaskTexture != null)
        {
            Destroy(mOwnedMaskTexture);
        }

        if (sInstance == this)
        {
            sInstance = null;
        }
    }
}
