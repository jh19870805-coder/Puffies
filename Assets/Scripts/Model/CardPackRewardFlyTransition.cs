using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class CardPackRewardFlyTransition : MonoBehaviour
{
    private const string TransitionObjectName = "CardPackRewardFlyTransition";
    private const float CenterMoveDuration = 0.4f;
    private const float CenterHoldDuration = 0.55f;
    private const float TargetMoveDuration = 0.6f;
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

        yield return AnimateIcons(centerPositions, centerSizes, CenterMoveDuration, 0f);
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

        yield return AnimateIcons(targetPositions, targetSizes, TargetMoveDuration, TargetArcHeight);
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
        float arcHeight)
    {
        var startPositions = new Vector2[mIcons.Count];
        var startSizes = new Vector2[mIcons.Count];
        for (var i = 0; i < mIcons.Count; i++)
        {
            startPositions[i] = mIcons[i].RectTransform.anchoredPosition;
            startSizes[i] = mIcons[i].RectTransform.sizeDelta;
        }

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(elapsed / duration);
            var eased = Mathf.SmoothStep(0f, 1f, normalized);
            var arc = Mathf.Sin(normalized * Mathf.PI) * arcHeight;
            for (var i = 0; i < mIcons.Count; i++)
            {
                var position = Vector2.LerpUnclamped(startPositions[i], targetPositions[i], eased);
                position.y += arc;
                mIcons[i].RectTransform.anchoredPosition = position;
                mIcons[i].RectTransform.sizeDelta = Vector2.LerpUnclamped(
                    startSizes[i],
                    targetSizes[i],
                    eased);
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
