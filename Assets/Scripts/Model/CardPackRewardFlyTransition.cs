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
    private const float RewardRevealSwitchDelay = 0.2f;
    private const float RewardVisualOverlapDuration = 0.05f;
    private const float TargetLookupTimeout = 5f;
    private const float TargetArcHeight = 72f;
    private const int SnapshotSortingOrder = 32000;
    private const int RewardContentSortingOrder = 1;
    private static CardPackRewardFlyTransition sInstance;

    private sealed class FlyIcon
    {
        public int PackId;
        public Transform RewardItemTransform;
        public RectTransform CoverRect;
        public Image Image;
        public Color BaseColor;
        public GameObject RevealEffect;
        public Vector3 InitialItemScale;
        public Vector2 CoverOffsetFromItem;
        public Vector2 DisplaySize;
        public bool HasLanded;
        public bool HasTargetRevealed;
        public bool HasRevealed;
        public float LandedAt;
        public float TargetRevealedAt;
    }

    private sealed class RewardSource
    {
        public int PackId;
        public Transform RewardItemTransform;
        public RectTransform RewardCanvasRect;
        public RectTransform CoverRect;
        public Image CoverImage;
        public GameObject RevealEffect;
        public Vector2 Position;
        public Vector2 Size;
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
        mCanvas = GetComponent<Canvas>();
        mCanvasRect = GetComponent<RectTransform>();
        ConfigureTransitionCanvas(Camera.main);
        mCanvas.overrideSorting = true;
        mCanvas.sortingOrder = SnapshotSortingOrder;

        var scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(GameDefine.DesignWidth, GameDefine.DesignHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        CreateInputBlocker();
        CreateSceneSnapshotLayer();
        Canvas.ForceUpdateCanvases();

        var rewardSources = new List<RewardSource>(packIds.Count);
        var rewardItemRoots = new HashSet<Transform>();
        for (var i = 0; i < packIds.Count; i++)
        {
            var packId = packIds[i];
            var source = sources != null && i < sources.Count ? sources[i] : null;
            var sourceImage = source != null ? source.GetComponent<Image>() : null;
            var rewardCanvasRect = source != null ? source.parent as RectTransform : null;
            var rewardCanvas = rewardCanvasRect != null
                ? rewardCanvasRect.GetComponent<Canvas>()
                : null;
            var rewardItemTransform = rewardCanvasRect != null
                ? rewardCanvasRect.parent
                : null;
            if (sourceImage == null
                || sourceImage.sprite == null
                || rewardCanvas == null
                || rewardItemTransform == null
                || !rewardItemRoots.Add(rewardItemTransform)
                || !TryGetCanvasGeometry(source, out var sourcePosition, out var sourceSize))
            {
                Debug.LogWarning(
                    $"CardPackRewardFlyTransition: complete BagRewardItem source missing. "
                    + $"packId={packId}");
                return false;
            }

            rewardSources.Add(new RewardSource
            {
                PackId = packId,
                RewardItemTransform = rewardItemTransform,
                RewardCanvasRect = rewardCanvasRect,
                CoverRect = source,
                CoverImage = sourceImage,
                RevealEffect = rewardCanvasRect.Find("FX_ui_jieSuo_w")?.gameObject,
                Position = sourcePosition,
                Size = sourceSize
            });
        }

        mPackIds.AddRange(packIds);
        for (var i = 0; i < rewardSources.Count; i++)
        {
            var source = rewardSources[i];
            source.RewardItemTransform.SetParent(mCanvasRect, false);
            source.RewardItemTransform.localRotation = Quaternion.identity;
            source.RewardItemTransform.localScale = Vector3.one;
            Canvas.ForceUpdateCanvases();
            if (!TryGetCanvasGeometry(
                    source.CoverRect,
                    out var currentPosition,
                    out var currentSize))
            {
                Debug.LogWarning(
                    $"CardPackRewardFlyTransition: BagRewardItem geometry was lost while reparenting. "
                    + $"packId={source.PackId}");
                return false;
            }

            source.RewardItemTransform.localScale *= CalculateUniformDisplayScale(
                currentSize,
                source.Size);
            Canvas.ForceUpdateCanvases();
            if (!TryGetCanvasGeometry(
                    source.CoverRect,
                    out currentPosition,
                    out currentSize))
            {
                return false;
            }

            var positionDelta = source.Position - currentPosition;
            source.RewardItemTransform.localPosition += new Vector3(
                positionDelta.x,
                positionDelta.y,
                0f);
            Canvas.ForceUpdateCanvases();
            TryGetCanvasGeometry(
                source.CoverRect,
                out var restoredPosition,
                out var restoredSize);
            source.CoverImage.raycastTarget = false;
            source.CoverImage.enabled = true;
            StopAndClearParticleSystems(source.RevealEffect);
            if (source.RevealEffect != null)
            {
                source.RevealEffect.SetActive(false);
            }

            mIcons.Add(new FlyIcon
            {
                PackId = source.PackId,
                RewardItemTransform = source.RewardItemTransform,
                CoverRect = source.CoverRect,
                Image = source.CoverImage,
                BaseColor = source.CoverImage.color,
                RevealEffect = source.RevealEffect,
                InitialItemScale = source.RewardItemTransform.localScale,
                CoverOffsetFromItem = source.Position
                                      - (Vector2)source.RewardItemTransform.localPosition,
                DisplaySize = source.Size
            });
            Debug.Log(
                $"CardPackRewardFlyTransition: complete BagRewardItem reparented. "
                + $"packId={source.PackId}, center={source.Position}->{restoredPosition}, "
                + $"size={source.Size}->{restoredSize}");
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
            var activeCamera = Camera.main;
            if (activeCamera != null && mCanvas.worldCamera != activeCamera)
            {
                ConfigureTransitionCanvas(activeCamera);
                Canvas.ForceUpdateCanvases();
            }

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
                || !TryGetCanvasGeometry(
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
        mCanvas.sortingOrder = RewardContentSortingOrder;
        Canvas.ForceUpdateCanvases();
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
            for (var i = 0; i < mIcons.Count; i++)
            {
                var icon = mIcons[i];
                if (icon.Image != null)
                {
                    icon.Image.color = mSceneSnapshotImage.enabled
                        ? WithAlpha(icon.BaseColor, 0f)
                        : icon.BaseColor;
                    icon.Image.enabled = true;
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
            for (var i = 0; i < mIcons.Count; i++)
            {
                var icon = mIcons[i];
                if (icon.Image != null)
                {
                    icon.Image.color = WithAlpha(icon.BaseColor, icon.BaseColor.a * eased);
                }
            }

            yield return null;
        }

        for (var i = 0; i < mIcons.Count; i++)
        {
            if (mIcons[i].Image != null)
            {
                mIcons[i].Image.color = mIcons[i].BaseColor;
            }
        }

        ReleaseSceneSnapshot();
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
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
        var targetScales = new float[mIcons.Count];
        var targetItemPositions = new Vector2[mIcons.Count];
        for (var i = 0; i < mIcons.Count; i++)
        {
            var icon = mIcons[i];
            var localPosition = icon.RewardItemTransform.localPosition;
            startPositions[i] = new Vector2(localPosition.x, localPosition.y);
            startSizes[i] = icon.DisplaySize;
            targetScales[i] = CalculateUniformDisplayScale(
                startSizes[i],
                targetSizes[i]);
            targetItemPositions[i] = targetPositions[i]
                                     - icon.CoverOffsetFromItem * targetScales[i];
        }

        var totalDuration = TargetMoveDuration
                            + TargetMoveStagger * Mathf.Max(0, mIcons.Count - 1)
                            + RewardRevealSwitchDelay
                            + RewardVisualOverlapDuration;
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
                var position = Vector2.LerpUnclamped(
                    startPositions[i],
                    targetItemPositions[i],
                    eased);
                position.y += arc;
                icon.RewardItemTransform.localPosition = new Vector3(
                    position.x,
                    position.y,
                    0f);
                icon.RewardItemTransform.localScale = Vector3.LerpUnclamped(
                    icon.InitialItemScale,
                    icon.InitialItemScale * targetScales[i],
                    eased);

                if (!icon.HasLanded && normalized >= 1f)
                {
                    icon.HasLanded = true;
                    icon.LandedAt = elapsed;
                    PlayRewardRevealEffect(icon);
                }

                if (icon.HasLanded
                    && !icon.HasTargetRevealed
                    && elapsed - icon.LandedAt >= RewardRevealSwitchDelay)
                {
                    mPreparedMainScene.RevealPackageRewardTarget(icon.PackId);
                    Canvas.ForceUpdateCanvases();
                    icon.HasTargetRevealed = true;
                    icon.TargetRevealedAt = elapsed;
                }

                if (icon.HasTargetRevealed
                    && !icon.HasRevealed
                    && elapsed - icon.TargetRevealedAt >= RewardVisualOverlapDuration)
                {
                    icon.Image.enabled = false;
                    icon.HasRevealed = true;
                }
            }

            yield return null;
        }

        for (var i = 0; i < mIcons.Count; i++)
        {
            var icon = mIcons[i];
            icon.RewardItemTransform.localPosition = new Vector3(
                targetItemPositions[i].x,
                targetItemPositions[i].y,
                0f);
            icon.RewardItemTransform.localScale = icon.InitialItemScale * targetScales[i];
            if (!icon.HasTargetRevealed)
            {
                mPreparedMainScene.RevealPackageRewardTarget(icon.PackId);
                Canvas.ForceUpdateCanvases();
                icon.HasTargetRevealed = true;
            }

            if (!icon.HasRevealed)
            {
                icon.Image.enabled = false;
                icon.HasRevealed = true;
            }
        }
    }

    private void PlayRewardRevealEffect(FlyIcon icon)
    {
        if (icon?.RevealEffect == null)
        {
            return;
        }

        Debug.Log(
            $"CardPackRewardFlyTransition: existing BagRewardItem effect played. "
            + $"packId={icon.PackId}, itemScale={icon.RewardItemTransform.localScale}, "
            + $"effectLocalScale={icon.RevealEffect.transform.localScale}, "
            + $"canvasMode={mCanvas.renderMode}, camera={mCanvas.worldCamera?.name}, "
            + $"sortingOrder={mCanvas.sortingOrder}");
        StopAndClearParticleSystems(icon.RevealEffect);
        icon.RevealEffect.SetActive(true);
        var particleSystems = icon.RevealEffect.GetComponentsInChildren<ParticleSystem>(true);
        for (var i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Play(false);
        }
    }

    private static float CalculateUniformDisplayScale(
        Vector2 sourceSize,
        Vector2 targetSize)
    {
        var widthScale = sourceSize.x > 0.01f
            ? targetSize.x / sourceSize.x
            : 1f;
        var heightScale = sourceSize.y > 0.01f
            ? targetSize.y / sourceSize.y
            : 1f;
        var uniformScale = Mathf.Min(widthScale, heightScale);
        return float.IsFinite(uniformScale) && uniformScale > 0.0001f
            ? uniformScale
            : 1f;
    }

    private static void StopAndClearParticleSystems(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        var particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);
        for (var i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Stop(
                false,
                ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void ConfigureTransitionCanvas(Camera camera)
    {
        if (mCanvas == null || camera == null)
        {
            return;
        }

        GameCommonUtility.ConfigureCanvasForGameplay(
            mCanvas,
            camera,
            GameDefine.DesignWidth,
            GameDefine.DesignHeight,
            GameDefine.PixelsPerUnit);
        mCanvas.overrideSorting = true;
        mCanvas.sortingOrder = SnapshotSortingOrder;
    }

    private bool TryGetCanvasGeometry(
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
                GetTransitionEventCamera(),
                out localPosition)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mCanvasRect,
                bottomLeftScreen,
                GetTransitionEventCamera(),
                out var bottomLeft)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mCanvasRect,
                topRightScreen,
                GetTransitionEventCamera(),
                out var topRight))
        {
            return false;
        }

        localSize = new Vector2(
            Mathf.Abs(topRight.x - bottomLeft.x),
            Mathf.Abs(topRight.y - bottomLeft.y));
        return localSize.x > 0.01f && localSize.y > 0.01f;
    }

    private bool TryGetCanvasGeometry(
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
                GetTransitionEventCamera(),
                out localPosition)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mCanvasRect,
                screenRect.min,
                GetTransitionEventCamera(),
                out var minimum)
            || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mCanvasRect,
                screenRect.max,
                GetTransitionEventCamera(),
                out var maximum))
        {
            return false;
        }

        localSize = new Vector2(
            Mathf.Abs(maximum.x - minimum.x),
            Mathf.Abs(maximum.y - minimum.y));
        return localSize.x > 0.01f && localSize.y > 0.01f;
    }

    private Camera GetTransitionEventCamera()
    {
        return mCanvas != null && mCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? mCanvas.worldCamera ?? Camera.main
            : null;
    }

    private void OnDestroy()
    {
        for (var i = 0; i < mIcons.Count; i++)
        {
            var revealEffect = mIcons[i].RevealEffect;
            if (revealEffect != null)
            {
                StopAndClearParticleSystems(revealEffect);
            }
        }

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
