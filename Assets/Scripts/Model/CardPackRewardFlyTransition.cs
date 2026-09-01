using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class CardPackRewardFlyTransition : MonoBehaviour
{
    private const string TransitionObjectName = "CardPackRewardFlyTransition";
    private const float CenterMoveDuration = 0.46f;
    private const float CenterHoldDuration = 0.42f;
    private const float TargetMoveDuration = 0.64f;
    private const float TargetMoveStagger = 0.08f;
    private const float CenterSizeOvershoot = 0.06f;
    private const float TargetLookupTimeout = 5f;
    private const float CenterSpacing = 32f;
    private const float CenterHorizontalPadding = 80f;
    private const float TargetArcHeight = 90f;
    private const int TransitionSortingOrder = 32000;
    private static readonly Vector2 DefaultCenterIconSize = new Vector2(240f, 272f);
    private static CardPackRewardFlyTransition sInstance;

    private sealed class FlyIcon
    {
        public int PackId;
        public RectTransform RectTransform;
    }

    private readonly List<int> mPackIds = new List<int>();
    private readonly List<FlyIcon> mIcons = new List<FlyIcon>();
    private Canvas mCanvas;
    private RectTransform mCanvasRect;

    public static bool IsPackPending(int packId)
    {
        return sInstance != null && sInstance.mPackIds.Contains(packId);
    }

    public static bool TryStart(RectTransform source, IReadOnlyList<int> packIds)
    {
        if (sInstance != null || source == null || packIds == null || packIds.Count == 0)
        {
            return false;
        }

        var uniquePackIds = new List<int>(packIds.Count);
        for (var i = 0; i < packIds.Count; i++)
        {
            var packId = packIds[i];
            if (packId > 0 && !uniquePackIds.Contains(packId))
            {
                uniquePackIds.Add(packId);
            }
        }

        if (uniquePackIds.Count == 0)
        {
            return false;
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
        if (transition.Initialize(source, uniquePackIds))
        {
            return true;
        }

        sInstance = null;
        Destroy(transitionObject);
        return false;
    }

    private bool Initialize(RectTransform source, List<int> packIds)
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
        Canvas.ForceUpdateCanvases();
        if (!TryGetOverlayGeometry(source, out var sourcePosition, out var sourceSize))
        {
            return false;
        }

        var fallbackSprite = source.GetComponent<Image>()?.sprite;
        for (var i = 0; i < mPackIds.Count; i++)
        {
            var packId = mPackIds[i];
            var sprite = GameCommonUtility.LoadSpriteByPath(
                GameDefine.FormatPackImagePath(packId),
                GameDefine.PixelsPerUnit) ?? fallbackSprite;
            if (sprite == null)
            {
                Debug.LogWarning($"CardPackRewardFlyTransition: pack sprite missing. packId={packId}");
                continue;
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
            iconImage.sprite = sprite;
            iconImage.color = Color.white;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            mIcons.Add(new FlyIcon
            {
                PackId = packId,
                RectTransform = iconRect
            });
        }

        if (mIcons.Count == 0)
        {
            return false;
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

    private IEnumerator PlayTransition()
    {
        var centerPositions = BuildCenterPositions();
        var centerSizes = new Vector2[mIcons.Count];
        for (var i = 0; i < centerSizes.Length; i++)
        {
            centerSizes[i] = GetCenterIconSize();
        }

        yield return AnimateIcons(
            centerPositions,
            centerSizes,
            CenterMoveDuration,
            0f,
            0f,
            CenterSizeOvershoot);
        yield return new WaitForSecondsRealtime(CenterHoldDuration);

        GameManager.EnterMainScene();
        yield return null;

        var targets = new RectTransform[mIcons.Count];
        var mainScene = default(MainScene);
        var elapsed = 0f;
        while (elapsed < TargetLookupTimeout)
        {
            elapsed += Time.unscaledDeltaTime;
            mainScene = FindObjectOfType<MainScene>();
            if (mainScene != null && TryResolveTargets(mainScene, targets))
            {
                break;
            }

            yield return null;
        }

        if (mainScene == null || !TryResolveTargets(mainScene, targets))
        {
            Debug.LogWarning("CardPackRewardFlyTransition: MainScene package targets were not ready before timeout.");
            RevealTargets(mainScene);
            Destroy(gameObject);
            yield break;
        }

        var targetPositions = new Vector2[mIcons.Count];
        var targetSizes = new Vector2[mIcons.Count];
        for (var i = 0; i < targets.Length; i++)
        {
            if (!TryGetOverlayGeometry(targets[i], out targetPositions[i], out targetSizes[i]))
            {
                targetPositions[i] = mIcons[i].RectTransform.anchoredPosition;
                targetSizes[i] = mIcons[i].RectTransform.sizeDelta;
            }
        }

        yield return AnimateIcons(
            targetPositions,
            targetSizes,
            TargetMoveDuration,
            TargetArcHeight,
            TargetMoveStagger,
            0f);
        RevealTargets(mainScene);
        Destroy(gameObject);
    }

    private Vector2[] BuildCenterPositions()
    {
        var iconSize = GetCenterIconSize();
        var step = iconSize.x + CenterSpacing;
        var firstX = -(mIcons.Count - 1) * step * 0.5f;
        var positions = new Vector2[mIcons.Count];
        for (var i = 0; i < positions.Length; i++)
        {
            positions[i] = new Vector2(firstX + i * step, 0f);
        }

        return positions;
    }

    private Vector2 GetCenterIconSize()
    {
        var canvasWidth = mCanvasRect != null && mCanvasRect.rect.width > 0f
            ? mCanvasRect.rect.width
            : GameDefine.DesignWidth;
        var availableWidth = Mathf.Max(1f, canvasWidth - CenterHorizontalPadding * 2f);
        var width = Mathf.Min(
            DefaultCenterIconSize.x,
            (availableWidth - CenterSpacing * Mathf.Max(0, mIcons.Count - 1)) / mIcons.Count);
        width = Mathf.Max(1f, width);
        return new Vector2(width, width * DefaultCenterIconSize.y / DefaultCenterIconSize.x);
    }

    private IEnumerator AnimateIcons(
        Vector2[] targetPositions,
        Vector2[] targetSizes,
        float duration,
        float arcHeight,
        float stagger,
        float sizeOvershoot)
    {
        var startPositions = new Vector2[mIcons.Count];
        var startSizes = new Vector2[mIcons.Count];
        for (var i = 0; i < mIcons.Count; i++)
        {
            startPositions[i] = mIcons[i].RectTransform.anchoredPosition;
            startSizes[i] = mIcons[i].RectTransform.sizeDelta;
        }

        var totalDuration = duration + stagger * Mathf.Max(0, mIcons.Count - 1);
        var elapsed = 0f;
        while (elapsed < totalDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            for (var i = 0; i < mIcons.Count; i++)
            {
                var iconElapsed = elapsed - stagger * i;
                var normalized = Mathf.Clamp01(iconElapsed / duration);
                var eased = Mathf.SmoothStep(0f, 1f, normalized);
                var arc = Mathf.Sin(normalized * Mathf.PI) * arcHeight;
                var position = Vector2.LerpUnclamped(startPositions[i], targetPositions[i], eased);
                position.y += arc;
                mIcons[i].RectTransform.anchoredPosition = position;
                var size = Vector2.LerpUnclamped(
                    startSizes[i],
                    targetSizes[i],
                    eased);
                if (sizeOvershoot > 0f)
                {
                    size *= 1f + Mathf.Sin(normalized * Mathf.PI) * sizeOvershoot;
                }

                mIcons[i].RectTransform.sizeDelta = size;
            }

            yield return null;
        }

        for (var i = 0; i < mIcons.Count; i++)
        {
            mIcons[i].RectTransform.anchoredPosition = targetPositions[i];
            mIcons[i].RectTransform.sizeDelta = targetSizes[i];
        }
    }

    private bool TryResolveTargets(MainScene mainScene, RectTransform[] targets)
    {
        for (var i = 0; i < mIcons.Count; i++)
        {
            if (!mainScene.TryGetPackageFlyTarget(mIcons[i].PackId, out targets[i]))
            {
                return false;
            }
        }

        return true;
    }

    private void RevealTargets(MainScene mainScene)
    {
        if (mainScene == null)
        {
            return;
        }

        for (var i = 0; i < mPackIds.Count; i++)
        {
            mainScene.RevealPackageFlyTarget(mPackIds[i]);
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

    private void OnDestroy()
    {
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
