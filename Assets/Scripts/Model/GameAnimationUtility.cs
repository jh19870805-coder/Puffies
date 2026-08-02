using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class GameAnimationUtility
{
    private const string DefaultCardPackStateName = "CardPackOpening";
    private const string CardPackPrefabEditorFolder = GameDefine.CardPackPrefabEditorFolder;
    private const string CardPackPrefabResourcesPath = GameDefine.CardPackPrefabResourcesFolder;
    private const string FullCardPackObjectName = "CardPackOpeningFull";
    private const string CardPackDismantleResourcesPath =
        GameDefine.CardPackDismantleResourcesPath;
    private const string CardPackTearTrailResourcesPath =
        GameDefine.CardPackTearTrailResourcesPath;
    private const string RuntimeDismantleObjectName = "CardPackDismantleRuntime";
    private const string RuntimeTearTrailObjectName = "CardPackTearTrailRuntime";
    private const string CardPackBodyBoneName = "Dummy001";
    private const float MeasuredTearSeamNormalizedHeight = 0.9471f;
    private const float DismantleReferenceCardWidth = 0.96f;
    private const float DismantleWorldDepthOffset = -0.1f;
    private const float DismantleLifetime = 2.8f;
    private const float TearTrailHorizontalInsetRatio = 0.04f;
    private const float TearTrailFadeLifetime = 1.2f;
    private const float CardPackAuthoredYaw = 180f;
    private static readonly string[] CardPackAnimatedPrefabNames =
    {
        GameDefine.CardPackOpeningPrefabName
    };
    private static readonly string[] AuthoredCardPackPrefabResourcesPaths =
    {
        null,
        "Effects/CardPack/CardBagPrefab/CardBag_tutorial/CardPackOpening_springOuting_001",
        "Effects/CardPack/CardBagPrefab/CardBag_duckQuack/CardPackOpening_duckQuack_001",
        "Effects/CardPack/CardBagPrefab/CardBag_littleKittens/CardPackOpening_littleKittens_001",
        "Effects/CardPack/CardBagPrefab/CardBag_puppy/CardPackOpening_puppy_001",
        "Effects/CardPack/CardBagPrefab/CardBag_sushiFriends/CardPackOpening_sushiFriends_001",
        "Effects/CardPack/CardBagPrefab/CardBag_coffeeTime/CardPackOpening_coffeeTime_001",
        "Effects/CardPack/CardBagPrefab/CardBag_spellsMagic/CardPackOpening_spellsMagic_001",
        "Effects/CardPack/CardBagPrefab/CardBag_springOuting/CardPackOpening_springOuting_001",
        "Effects/CardPack/CardBagPrefab/CardBag_powerRock/CardPackOpening_powerRock_001",
        "Effects/CardPack/CardBagPrefab/CardBag_stoneAgePals/CardPackOpening_stoneAgePals_001",
        "Effects/CardPack/CardBagPrefab/CardBag_pharaohsTreasure/CardPackOpening_pharaohsTreasure_001",
        "Effects/CardPack/CardBagPrefab/CardBag_piggyPals/CardPackOpening_piggyPals_001",
        "Effects/CardPack/CardBagPrefab/CardBag_freshmanPerks/CardPackOpening_freshmanPerks_001",
        "Effects/CardPack/CardBagPrefab/CardBag_chefBunny/CardPackOpening_chefBunny_001",
        "Effects/CardPack/CardBagPrefab/CardBag_duchsQuack01/CardPackOpening_duchsQuack01_001",
        "Effects/CardPack/CardBagPrefab/CardBag_jollyHoliday/CardPackOpening_jollyHoliday_001",
        "Effects/CardPack/CardBagPrefab/CardBag_swimmingPool/CardPackOpening_swimmingPool_001",
        "Effects/CardPack/CardBagPrefab/CardBag_snappyCrab/CardPackOpening_snappyCrab_001",
        "Effects/CardPack/CardBagPrefab/CardBag_myLovelyHair/CardPackOpening_myLovelyHair_001",
        "Effects/CardPack/CardBagPrefab/CardBag_fairy/CardPackOpening_fairy_001",
        "Effects/CardPack/CardBagPrefab/CardBag_oldGadgets/CardPackOpening_oldGadgets_001"
    };
    private static readonly int BaseMapPropertyId = Shader.PropertyToID("_BaseMap");
    private static readonly int BaseMapTransformPropertyId = Shader.PropertyToID("_BaseMap_ST");
    private static readonly int MainTexturePropertyId = Shader.PropertyToID("_MainTex");
    private static readonly int MainTextureTransformPropertyId = Shader.PropertyToID("_MainTex_ST");
    private static readonly int FrontFacesAlbedoPropertyId = Shader.PropertyToID("_FrontFacesAlbedo");
    private static readonly int FrontFacesAlbedoTransformPropertyId = Shader.PropertyToID("_FrontFacesAlbedo_ST");
    private static readonly int FrontFacesColorPropertyId = Shader.PropertyToID("_FrontFacesColor");
    private static readonly int UiClipRectPropertyId = Shader.PropertyToID("_UiClipRect");
    private static readonly int UseUiClipRectPropertyId = Shader.PropertyToID("_UseUiClipRect");
    private static readonly Dictionary<string, CardPackEffectInstance> sSpawnedEffects =
        new Dictionary<string, CardPackEffectInstance>();

    internal sealed class CardPackEffectInstance
    {
        public GameObject Root;
        public Animator[] Animators;
        public Renderer[] CardRenderers;
        public Vector3 TearSeamRootLocalPosition;
        public bool HasTearSeamRootLocalPosition;
        public bool HasLoggedMissingTearSeam;
        public Vector3 BaseRootPosition;
        public Vector3 BaseRootScale;
        public Vector3 ScaleCenter;
        public bool HasPreparedPose;
        public bool PreserveAuthoredAppearance;
        public Transform[] RenderLayerTransforms;
        public int[] OriginalRenderLayers;
    }

    public sealed class CardPackIdleDisplay
    {
        internal CardPackEffectInstance Effect;
        internal Bounds ReferenceBounds;
        internal int SortingOrder;
        public bool UsesAuthoredAppearance => Effect != null
            && Effect.PreserveAuthoredAppearance;
        public bool IsValid => Effect != null
            && Effect.Root != null
            && Effect.CardRenderers != null
            && Effect.CardRenderers.Length > 0;
    }

    public enum EaseType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut
    }

    /// <summary>
    /// 用途：播放对象位移动画。返回：协程对象。
    /// </summary>
    /// <param name="host">参数：用于启动协程的 MonoBehaviour。</param>
    /// <param name="target">参数：需要执行动画的 Transform。</param>
    /// <param name="to">参数：目标位置。</param>
    /// <param name="duration">参数：动画时长（秒）。</param>
    /// <param name="ease">参数：缓动类型。</param>
    /// <param name="onComplete">参数：动画完成回调。</param>
    /// <returns>返回：启动的协程；当参数无效时返回 null。</returns>
    public static Coroutine PlayMove(
        MonoBehaviour host,
        Transform target,
        Vector3 to,
        float duration,
        EaseType ease = EaseType.EaseInOut,
        Action onComplete = null)
    {
        if (host == null || target == null)
        {
            return null;
        }

        var from = target.position;
        return host.StartCoroutine(Animate(
            duration,
            ease,
            t => target.position = Vector3.LerpUnclamped(from, to, t),
            onComplete));
    }

    /// <summary>
    /// 用途：播放对象缩放动画。返回：协程对象。
    /// </summary>
    /// <param name="host">参数：用于启动协程的 MonoBehaviour。</param>
    /// <param name="target">参数：需要执行动画的 Transform。</param>
    /// <param name="to">参数：目标缩放。</param>
    /// <param name="duration">参数：动画时长（秒）。</param>
    /// <param name="ease">参数：缓动类型。</param>
    /// <param name="onComplete">参数：动画完成回调。</param>
    /// <returns>返回：启动的协程；当参数无效时返回 null。</returns>
    public static Coroutine PlayScale(
        MonoBehaviour host,
        Transform target,
        Vector3 to,
        float duration,
        EaseType ease = EaseType.EaseInOut,
        Action onComplete = null)
    {
        if (host == null || target == null)
        {
            return null;
        }

        var from = target.localScale;
        return host.StartCoroutine(Animate(
            duration,
            ease,
            t => target.localScale = Vector3.LerpUnclamped(from, to, t),
            onComplete));
    }

    /// <summary>
    /// 用途：播放精灵透明度动画。返回：协程对象。
    /// </summary>
    /// <param name="host">参数：用于启动协程的 MonoBehaviour。</param>
    /// <param name="renderer">参数：目标 SpriteRenderer。</param>
    /// <param name="toAlpha">参数：目标透明度（0~1）。</param>
    /// <param name="duration">参数：动画时长（秒）。</param>
    /// <param name="ease">参数：缓动类型。</param>
    /// <param name="onComplete">参数：动画完成回调。</param>
    /// <returns>返回：启动的协程；当参数无效时返回 null。</returns>
    public static Coroutine PlayFade(
        MonoBehaviour host,
        SpriteRenderer renderer,
        float toAlpha,
        float duration,
        EaseType ease = EaseType.EaseInOut,
        Action onComplete = null)
    {
        if (host == null || renderer == null)
        {
            return null;
        }

        var color = renderer.color;
        var fromAlpha = color.a;
        var clampedTargetAlpha = Mathf.Clamp01(toAlpha);

        return host.StartCoroutine(Animate(
            duration,
            ease,
            t =>
            {
                color.a = Mathf.LerpUnclamped(fromAlpha, clampedTargetAlpha, t);
                renderer.color = color;
            },
            onComplete));
    }

    public static bool TryBindCardPackIdleDisplay(
        GameObject authoredRoot,
        int packId,
        Sprite coverSprite,
        int sortingOrder,
        out CardPackIdleDisplay display)
    {
        display = null;
        if (packId <= 0)
        {
            return false;
        }

        var effectRoot = authoredRoot;
        var preserveAuthoredAppearance = TryInstantiateAuthoredCardPackPrefab(
            packId,
            out var authoredPrefabRoot);
        if (preserveAuthoredAppearance)
        {
            if (effectRoot != null)
            {
                UnityEngine.Object.Destroy(effectRoot);
            }

            effectRoot = authoredPrefabRoot;
        }

        var effect = BindCardPackEffect(
            effectRoot,
            $"CardPackIdle_{packId:D3}",
            preserveAuthoredAppearance);
        if (effect == null)
        {
            return false;
        }

        effect.Root.SetActive(true);
        if (effect.PreserveAuthoredAppearance)
        {
            PauseCardPackAnimators(effect);
        }
        else
        {
            ResetCardPackAnimators(effect, pause: true);
        }
        effect.Root.transform.position = Vector3.zero;
        effect.Root.transform.rotation = Quaternion.identity;
        effect.Root.transform.localScale = Vector3.one;
        for (var i = 0; i < effect.CardRenderers.Length; i++)
        {
            var renderer = effect.CardRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (!effect.PreserveAuthoredAppearance)
            {
                renderer.sortingOrder = sortingOrder;
                ApplyCardPackAppearance(
                    renderer,
                    coverSprite,
                    null,
                    default,
                    false);
            }
        }

        if (!TryGetCurrentPoseBounds(effect.CardRenderers, out var referenceBounds))
        {
            UnityEngine.Object.Destroy(effect.Root);
            return false;
        }

        display = new CardPackIdleDisplay
        {
            Effect = effect,
            ReferenceBounds = referenceBounds,
            SortingOrder = sortingOrder
        };
        return true;
    }

    public static void UpdateCardPackIdleDisplay(
        CardPackIdleDisplay display,
        RectTransform anchor,
        Rect screenClipRect,
        float scaleMultiplier,
        bool visible)
    {
        if (display == null || !display.IsValid)
        {
            return;
        }

        var effect = display.Effect;
        var camera = Camera.main;
        if (!visible || anchor == null || camera == null)
        {
            if (effect.PreserveAuthoredAppearance)
            {
                effect.Root.SetActive(false);
            }
            else
            {
                SetRenderersEnabled(effect.CardRenderers, false);
            }
            return;
        }

        var anchorBounds = GameCommonUtility.GetRectTransformCameraWorldBounds(anchor, camera);
        var meshBounds = display.ReferenceBounds;
        if (anchorBounds.size.x <= 0.001f
            || anchorBounds.size.y <= 0.001f
            || meshBounds.size.x <= 0.001f
            || meshBounds.size.y <= 0.001f)
        {
            if (effect.PreserveAuthoredAppearance)
            {
                effect.Root.SetActive(false);
            }
            else
            {
                SetRenderersEnabled(effect.CardRenderers, false);
            }
            return;
        }

        var rootTransform = effect.Root.transform;
        effect.Root.SetActive(true);
        rootTransform.rotation = Quaternion.identity;
        if (effect.PreserveAuthoredAppearance)
        {
            rootTransform.localScale = Vector3.one;
            rootTransform.position = anchorBounds.center - meshBounds.center;
            return;
        }

        var baseScale = Mathf.Min(
            anchorBounds.size.x / meshBounds.size.x,
            anchorBounds.size.y / meshBounds.size.y);
        var finalScale = Mathf.Max(0.001f, baseScale * Mathf.Max(0.001f, scaleMultiplier));
        rootTransform.localScale = Vector3.one * finalScale;
        rootTransform.position = anchorBounds.center - meshBounds.center * finalScale;
        SetRendererSortingOrder(effect.CardRenderers, display.SortingOrder);
        ApplyCardPackClip(effect.CardRenderers, screenClipRect, true);
        SetRenderersEnabled(effect.CardRenderers, true);
    }

    public static void SetCardPackIdleDisplayVisible(CardPackIdleDisplay display, bool visible)
    {
        if (display != null && display.IsValid)
        {
            display.Effect.Root.SetActive(visible);
            if (display.Effect.PreserveAuthoredAppearance)
            {
                return;
            }

            if (visible)
            {
                ResetCardPackAnimators(display.Effect, pause: true);
                SetRendererSortingOrder(display.Effect.CardRenderers, display.SortingOrder);
            }
            SetRenderersEnabled(display.Effect.CardRenderers, visible);
        }
    }

    public static void DestroyCardPackIdleDisplay(CardPackIdleDisplay display)
    {
        if (display == null)
        {
            return;
        }

        if (display.Effect != null && display.Effect.Root != null)
        {
            if (sSpawnedEffects.TryGetValue(FullCardPackObjectName, out var preparedEffect)
                && ReferenceEquals(preparedEffect, display.Effect))
            {
                sSpawnedEffects.Remove(FullCardPackObjectName);
            }

            UnityEngine.Object.Destroy(display.Effect.Root);
        }

        display.Effect = null;
    }

    public static bool PrepareCardPackAnimation(
        int packId,
        Sprite coverSprite,
        Transform anchor,
        float scaleMultiplier = 1f)
    {
        return PrepareCardPackAnimation(
            null,
            packId,
            coverSprite,
            anchor,
            scaleMultiplier);
    }

    public static bool PrepareCardPackAnimation(
        CardPackIdleDisplay display,
        int packId,
        Sprite coverSprite,
        Transform anchor,
        float scaleMultiplier = 1f)
    {
        if (packId <= 0 || anchor == null)
        {
            return false;
        }

        var usesIdleDisplay = display != null && display.IsValid;
        var effect = usesIdleDisplay
            ? display.Effect
            : GetOrSpawnCardPackEffect(anchor);
        if (effect == null || effect.Animators == null || effect.Animators.Length == 0)
        {
            return false;
        }

        sSpawnedEffects[FullCardPackObjectName] = effect;
        effect.Root.SetActive(true);
        if (!usesIdleDisplay || !effect.PreserveAuthoredAppearance)
        {
            ResetCardPackAnimators(effect, pause: true);
        }
        ApplyPreviewPose(effect, anchor, coverSprite, !usesIdleDisplay);
        if (!effect.PreserveAuthoredAppearance)
        {
            ApplyCardPackClip(effect.CardRenderers, default, false);
        }
        CacheCardPackTearSeam(effect);
        SetPreparedCardPackScale(scaleMultiplier);
        return true;
    }

    public static void SetPreparedCardPackScale(float scaleMultiplier)
    {
        if (!TryGetSpawnedCardPackEffect(out var effect) || !effect.HasPreparedPose)
        {
            return;
        }

        SetPreparedCardPackPose(scaleMultiplier, effect.ScaleCenter);
    }

    public static bool TryGetPreparedCardPackCenter(out Vector3 center)
    {
        center = default;
        if (!TryGetSpawnedCardPackEffect(out var effect) || !effect.HasPreparedPose)
        {
            return false;
        }

        center = effect.ScaleCenter;
        return true;
    }

    public static bool TryGetPreparedCardPackWorldBounds(out Bounds bounds)
    {
        bounds = default;
        return TryGetSpawnedCardPackEffect(out var effect)
            && effect.HasPreparedPose
            && TryGetCurrentPoseBounds(effect.CardRenderers, out bounds);
    }

    public static void SetPreparedCardPackPose(float scaleMultiplier, Vector3 center)
    {
        if (!TryGetSpawnedCardPackEffect(out var effect) || !effect.HasPreparedPose)
        {
            return;
        }

        var multiplier = Mathf.Max(0.001f, scaleMultiplier);
        effect.Root.SetActive(true);
        effect.Root.transform.localScale = effect.BaseRootScale * multiplier;
        effect.Root.transform.position = center
            + (effect.BaseRootPosition - effect.ScaleCenter) * multiplier;
    }

    public static void SetPreparedCardPackVisible(bool visible)
    {
        if (TryGetSpawnedCardPackEffect(out var effect))
        {
            effect.Root.SetActive(visible);
        }
    }

    public static void SetPreparedCardPackSortingOrder(int sortingOrder)
    {
        if (!TryGetSpawnedCardPackEffect(out var effect) || effect.CardRenderers == null)
        {
            return;
        }

        if (effect.PreserveAuthoredAppearance)
        {
            return;
        }

        for (var i = 0; i < effect.CardRenderers.Length; i++)
        {
            var renderer = effect.CardRenderers[i];
            if (renderer != null)
            {
                renderer.sortingOrder = sortingOrder;
            }
        }
    }

    public static bool SetPreparedCardPackRenderLayer(int layer)
    {
        if (!TryGetSpawnedCardPackEffect(out var effect)
            || !effect.HasPreparedPose
            || layer < 0
            || layer > 31)
        {
            return false;
        }

        return SetCardPackEffectRenderLayer(effect, layer);
    }

    public static bool SetCardPackIdleDisplayRenderLayer(
        CardPackIdleDisplay display,
        int layer)
    {
        return display != null
            && display.IsValid
            && layer >= 0
            && layer <= 31
            && SetCardPackEffectRenderLayer(display.Effect, layer);
    }

    public static void RestoreCardPackIdleDisplayRenderLayers(
        CardPackIdleDisplay display)
    {
        if (display != null && display.Effect != null)
        {
            RestoreCardPackEffectRenderLayers(display.Effect);
        }
    }

    private static bool SetCardPackEffectRenderLayer(
        CardPackEffectInstance effect,
        int layer)
    {
        if (effect == null || effect.Root == null)
        {
            return false;
        }

        if (effect.RenderLayerTransforms == null
            || effect.OriginalRenderLayers == null)
        {
            effect.RenderLayerTransforms = effect.Root.GetComponentsInChildren<Transform>(true);
            effect.OriginalRenderLayers = new int[effect.RenderLayerTransforms.Length];
            for (var i = 0; i < effect.RenderLayerTransforms.Length; i++)
            {
                effect.OriginalRenderLayers[i] = effect.RenderLayerTransforms[i].gameObject.layer;
            }
        }

        for (var i = 0; i < effect.RenderLayerTransforms.Length; i++)
        {
            var target = effect.RenderLayerTransforms[i];
            if (target != null)
            {
                target.gameObject.layer = layer;
            }
        }

        return true;
    }

    public static void RestorePreparedCardPackRenderLayers()
    {
        if (!TryGetSpawnedCardPackEffect(out var effect)
            || effect == null)
        {
            return;
        }

        RestoreCardPackEffectRenderLayers(effect);
    }

    private static void RestoreCardPackEffectRenderLayers(
        CardPackEffectInstance effect)
    {
        if (effect == null
            || effect.RenderLayerTransforms == null
            || effect.OriginalRenderLayers == null)
        {
            return;
        }

        var count = Mathf.Min(
            effect.RenderLayerTransforms.Length,
            effect.OriginalRenderLayers.Length);
        for (var i = 0; i < count; i++)
        {
            var target = effect.RenderLayerTransforms[i];
            if (target != null)
            {
                target.gameObject.layer = effect.OriginalRenderLayers[i];
            }
        }

        effect.RenderLayerTransforms = null;
        effect.OriginalRenderLayers = null;
    }

    public static bool PlayPreparedCardPackAnimation()
    {
        if (!TryGetSpawnedCardPackEffect(out var effect) || !effect.HasPreparedPose)
        {
            return false;
        }

        ResetCardPackAnimators(effect, pause: false);
        return true;
    }

    public static bool TryGetPreparedCardPackTearSeamWorldPosition(out Vector3 position)
    {
        position = default;
        if (!TryGetSpawnedCardPackEffect(out var effect)
            || !effect.HasPreparedPose
            || !TryGetCurrentPoseBounds(effect.CardRenderers, out var cardBounds)
            || cardBounds.size.y <= 0.001f)
        {
            return false;
        }

        if (!effect.HasTearSeamRootLocalPosition)
        {
            CacheCardPackTearSeam(effect);
        }

        var hasValidSkinBoundary = false;
        if (effect.HasTearSeamRootLocalPosition)
        {
            position = effect.Root.transform.TransformPoint(effect.TearSeamRootLocalPosition);
            hasValidSkinBoundary = !float.IsNaN(position.y)
                && !float.IsInfinity(position.y)
                && position.y >= cardBounds.min.y
                && position.y <= cardBounds.max.y;
        }

        if (!hasValidSkinBoundary)
        {
            position = new Vector3(
                cardBounds.center.x,
                Mathf.Lerp(
                    cardBounds.min.y,
                    cardBounds.max.y,
                    MeasuredTearSeamNormalizedHeight),
                cardBounds.center.z);
            if (!effect.HasLoggedMissingTearSeam)
            {
                Debug.LogWarning(
                    $"Card-pack body skin bone '{CardPackBodyBoneName}' weighted boundary is "
                    + "missing or outside the rendered card; using the measured model fallback position.");
                effect.HasLoggedMissingTearSeam = true;
            }
        }

        return true;
    }

    public static bool PlayPreparedCardPackDismantleEffect(int sortingOrder)
    {
        if (!TryGetSpawnedCardPackEffect(out var effect)
            || !effect.HasPreparedPose
            || !TryGetCurrentPoseBounds(effect.CardRenderers, out var cardBounds)
            || cardBounds.size.x <= 0.001f
            || !TryGetPreparedCardPackTearSeamWorldPosition(out var seamPosition))
        {
            return false;
        }

        var prefab = Resources.Load<GameObject>(CardPackDismantleResourcesPath);
        if (prefab == null)
        {
            Debug.LogWarning(
                $"Card-pack dismantle effect missing: Resources/{CardPackDismantleResourcesPath}.");
            return false;
        }

        var root = UnityEngine.Object.Instantiate(prefab);
        root.name = RuntimeDismantleObjectName;
        SetLayerRecursively(root.transform, effect.Root.layer);
        root.transform.position = new Vector3(
            cardBounds.center.x,
            seamPosition.y,
            seamPosition.z + DismantleWorldDepthOffset);
        root.transform.rotation = Quaternion.identity;
        var worldScale = cardBounds.size.x / DismantleReferenceCardWidth;
        var sortingLayerId = effect.CardRenderers != null
            && effect.CardRenderers.Length > 0
            && effect.CardRenderers[0] != null
            ? effect.CardRenderers[0].sortingLayerID
            : 0;
        CardFxRuntimeUtility.PrepareRuntimeWorldEffect(
            root,
            worldScale,
            sortingLayerId,
            sortingOrder);
        CardFxRuntimeUtility.ReplayParticleSystems(root);
        CardPackDismantleLifetime.Attach(root, DismantleLifetime);
        return true;
    }

    public static IEnumerator PlayPreparedCardPackTearTrailEffect(
        int sortingOrder,
        float travelDuration)
    {
        if (!TryGetSpawnedCardPackEffect(out var effect)
            || !effect.HasPreparedPose
            || !TryGetCurrentPoseBounds(effect.CardRenderers, out var cardBounds)
            || cardBounds.size.x <= 0.001f
            || !TryGetPreparedCardPackTearSeamWorldPosition(out var seamPosition))
        {
            yield break;
        }

        var prefab = Resources.Load<GameObject>(CardPackTearTrailResourcesPath);
        if (prefab == null)
        {
            Debug.LogWarning(
                $"Card-pack tear trail effect missing: Resources/{CardPackTearTrailResourcesPath}.");
            yield break;
        }

        var root = UnityEngine.Object.Instantiate(prefab);
        root.name = RuntimeTearTrailObjectName;
        SetLayerRecursively(root.transform, effect.Root.layer);
        root.transform.rotation = Quaternion.identity;

        var startPosition = new Vector3(
            Mathf.Lerp(cardBounds.min.x, cardBounds.max.x, TearTrailHorizontalInsetRatio),
            seamPosition.y,
            seamPosition.z + DismantleWorldDepthOffset);
        var endPosition = new Vector3(
            Mathf.Lerp(cardBounds.min.x, cardBounds.max.x, 1f - TearTrailHorizontalInsetRatio),
            seamPosition.y,
            startPosition.z);
        root.transform.position = startPosition;

        var worldScale = cardBounds.size.x / DismantleReferenceCardWidth;
        var sortingLayerId = effect.CardRenderers != null
            && effect.CardRenderers.Length > 0
            && effect.CardRenderers[0] != null
            ? effect.CardRenderers[0].sortingLayerID
            : 0;
        CardFxRuntimeUtility.PrepareRuntimeWorldEffect(
            root,
            worldScale,
            sortingLayerId,
            sortingOrder);
        CardFxRuntimeUtility.ReplayParticleSystems(root);

        yield return null;
        var duration = Mathf.Max(0.05f, travelDuration);
        var elapsed = 0f;
        while (elapsed < duration && root != null)
        {
            elapsed += Time.unscaledDeltaTime;
            var normalized = Mathf.Clamp01(elapsed / duration);
            root.transform.position = Vector3.LerpUnclamped(
                startPosition,
                endPosition,
                Mathf.SmoothStep(0f, 1f, normalized));
            yield return null;
        }

        if (root == null)
        {
            yield break;
        }

        root.transform.position = endPosition;
        CardFxRuntimeUtility.StopEmittingParticleSystems(root);
        CardPackDismantleLifetime.Attach(root, TearTrailFadeLifetime);
    }

    /// <summary>
    /// 用途：播放通用卡包开包动画并替换为指定封面。返回：是否成功触发播放。
    /// </summary>
    /// <param name="packId">参数：当前卡包编号。</param>
    /// <param name="coverSprite">参数：需要显示在模型上的卡包封面。</param>
    /// <param name="anchor">参数：动画模型定位和缩放所参照的 UI 节点。</param>
    /// <returns>返回：true 表示播放成功，false 表示未找到目标或播放失败。</returns>
    public static bool PlayCardPackAnimation(int packId, Sprite coverSprite, Transform anchor)
    {
        if (packId <= 0)
        {
            Debug.LogWarning($"PlayCardPackAnimation failed: invalid packId={packId}.");
            return false;
        }

        if (coverSprite == null || coverSprite.texture == null)
        {
            Debug.LogWarning(
                $"PlayCardPackAnimation: cover missing for packId={packId}; using the authored model texture.");
        }

        if (!PrepareCardPackAnimation(packId, coverSprite, anchor))
        {
            return false;
        }

        return PlayPreparedCardPackAnimation();
    }

    /// <summary>
    /// 用途：估算卡包动画播放时长，供主场景在切页前等待。返回：时长大于 0 表示有效。
    /// </summary>
    public static float GetCardPackPlayDuration(Transform anchor = null)
    {
        var effect = GetOrSpawnCardPackEffect(anchor);
        if (effect == null || effect.Animators == null)
        {
            return 0f;
        }

        var duration = 0f;
        for (var i = 0; i < effect.Animators.Length; i++)
        {
            var animator = effect.Animators[i];
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                continue;
            }

            var clips = animator.runtimeAnimatorController.animationClips;
            for (var j = 0; j < clips.Length; j++)
            {
                if (clips[j] != null)
                {
                    duration = Mathf.Max(duration, clips[j].length);
                }
            }
        }

        return duration;
    }

    private static IEnumerator Animate(
        float duration,
        EaseType ease,
        Action<float> onUpdate,
        Action onComplete)
    {
        if (onUpdate == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            onUpdate(1f);
            onComplete?.Invoke();
            yield break;
        }

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var normalized = Mathf.Clamp01(elapsed / duration);
            onUpdate(EvaluateEase(normalized, ease));
            yield return null;
        }

        onUpdate(1f);
        onComplete?.Invoke();
    }

    private static float EvaluateEase(float t, EaseType ease)
    {
        switch (ease)
        {
            case EaseType.EaseIn:
                return t * t;
            case EaseType.EaseOut:
                return 1f - (1f - t) * (1f - t);
            case EaseType.EaseInOut:
                if (t < 0.5f)
                {
                    return 2f * t * t;
                }

                var x = -2f * t + 2f;
                return 1f - x * x * 0.5f;
            case EaseType.Linear:
            default:
                return t;
        }
    }

    private static bool TryGetSpawnedCardPackEffect(out CardPackEffectInstance effect)
    {
        return sSpawnedEffects.TryGetValue(FullCardPackObjectName, out effect)
            && effect != null
            && effect.Root != null;
    }

    private static void ResetCardPackAnimators(CardPackEffectInstance effect, bool pause)
    {
        if (effect == null || effect.Animators == null)
        {
            return;
        }

        for (var i = 0; i < effect.Animators.Length; i++)
        {
            var animator = effect.Animators[i];
            if (animator == null)
            {
                continue;
            }

            animator.enabled = true;
            animator.speed = 1f;
            animator.Rebind();
            animator.Update(0f);
            animator.Play(DefaultCardPackStateName, 0, 0f);
            animator.Update(0f);
            animator.speed = pause ? 0f : 1f;
        }
    }

    private static void PauseCardPackAnimators(CardPackEffectInstance effect)
    {
        if (effect == null || effect.Animators == null)
        {
            return;
        }

        for (var i = 0; i < effect.Animators.Length; i++)
        {
            var animator = effect.Animators[i];
            if (animator != null)
            {
                animator.speed = 0f;
            }
        }
    }

    private static CardPackEffectInstance GetOrSpawnCardPackEffect(Transform anchor)
    {
        if (sSpawnedEffects.TryGetValue(FullCardPackObjectName, out var cachedEffect)
            && cachedEffect != null
            && cachedEffect.Root != null)
        {
            return cachedEffect;
        }

        if (anchor == null)
        {
            return null;
        }

        var effect = SpawnCardPackEffect(FullCardPackObjectName);
        if (effect != null)
        {
            sSpawnedEffects[FullCardPackObjectName] = effect;
        }
        return effect;
    }

    private static CardPackEffectInstance SpawnCardPackEffect(string rootName)
    {
        var root = new GameObject(rootName);
        var animators = new List<Animator>(CardPackAnimatedPrefabNames.Length);
        var cardRenderers = new List<Renderer>(CardPackAnimatedPrefabNames.Length);
        for (var i = 0; i < CardPackAnimatedPrefabNames.Length; i++)
        {
            var prefabName = CardPackAnimatedPrefabNames[i];
            var prefab = LoadCardPackPrefab(prefabName);
            if (prefab == null)
            {
                Debug.LogWarning($"Card-pack effect layer missing: {prefabName}.");
                continue;
            }

            var layer = UnityEngine.Object.Instantiate(prefab, root.transform, false);
            layer.name = prefabName;
            layer.transform.localRotation = Quaternion.Euler(0f, CardPackAuthoredYaw, 0f);
            var animator = layer.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                animators.Add(animator);
            }

            var layerRenderers = layer.GetComponentsInChildren<Renderer>(true);
            for (var rendererIndex = 0; rendererIndex < layerRenderers.Length; rendererIndex++)
            {
                if (layerRenderers[rendererIndex] is SkinnedMeshRenderer skinnedRenderer)
                {
                    skinnedRenderer.updateWhenOffscreen = true;
                }
            }
            cardRenderers.AddRange(layerRenderers);
        }

        if (animators.Count != CardPackAnimatedPrefabNames.Length
            || cardRenderers.Count < CardPackAnimatedPrefabNames.Length)
        {
            Debug.LogWarning(
                $"Card-pack full effect is incomplete. animators={animators.Count}, "
                + $"renderers={cardRenderers.Count}, expected={CardPackAnimatedPrefabNames.Length}.");
            UnityEngine.Object.Destroy(root);
            return null;
        }

        var effect = new CardPackEffectInstance
        {
            Root = root,
            Animators = animators.ToArray(),
            CardRenderers = cardRenderers.ToArray()
        };
        return effect;
    }

    private static CardPackEffectInstance BindCardPackEffect(
        GameObject authoredRoot,
        string rootName,
        bool preserveAuthoredAppearance)
    {
        if (authoredRoot == null)
        {
            Debug.LogWarning("Card-pack effect is missing from PackItem prefab.");
            return null;
        }

        authoredRoot.name = rootName;
        authoredRoot.transform.SetParent(null, true);
        var animators = authoredRoot.GetComponentsInChildren<Animator>(true);
        var renderers = authoredRoot.GetComponentsInChildren<Renderer>(true);
        if (animators.Length == 0 || renderers.Length == 0)
        {
            Debug.LogWarning(
                $"PackItem card-pack effect is incomplete. animators={animators.Length}, renderers={renderers.Length}.");
            UnityEngine.Object.Destroy(authoredRoot);
            return null;
        }

        return new CardPackEffectInstance
        {
            Root = authoredRoot,
            Animators = animators,
            CardRenderers = renderers,
            PreserveAuthoredAppearance = preserveAuthoredAppearance
        };
    }

    private static void CacheCardPackTearSeam(CardPackEffectInstance effect)
    {
        if (effect == null
            || effect.Root == null
            || effect.HasTearSeamRootLocalPosition)
        {
            return;
        }

        var skinnedRenderers = effect.Root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (var rendererIndex = 0; rendererIndex < skinnedRenderers.Length; rendererIndex++)
        {
            var renderer = skinnedRenderers[rendererIndex];
            var sourceMesh = renderer != null ? renderer.sharedMesh : null;
            var bones = renderer != null ? renderer.bones : null;
            if (sourceMesh == null || bones == null || bones.Length == 0)
            {
                continue;
            }

            var bodyBoneIndex = -1;
            for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                if (bones[boneIndex] != null
                    && string.Equals(
                        bones[boneIndex].name,
                        CardPackBodyBoneName,
                        StringComparison.Ordinal))
                {
                    bodyBoneIndex = boneIndex;
                    break;
                }
            }

            var weights = sourceMesh.boneWeights;
            var sourceVertices = sourceMesh.vertices;
            var bindPoses = sourceMesh.bindposes;
            if (bodyBoneIndex < 0
                || weights == null
                || weights.Length == 0
                || sourceVertices.Length != weights.Length
                || bindPoses == null
                || bindPoses.Length != bones.Length)
            {
                continue;
            }

            var rendererWorldToLocal = renderer.transform.worldToLocalMatrix;
            var skinMatrices = new Matrix4x4[bones.Length];
            var hasCompleteBones = true;
            for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                if (bones[boneIndex] == null)
                {
                    hasCompleteBones = false;
                    break;
                }

                skinMatrices[boneIndex] = rendererWorldToLocal
                    * bones[boneIndex].localToWorldMatrix
                    * bindPoses[boneIndex];
            }

            if (!hasCompleteBones)
            {
                continue;
            }

            var posedVertices = new Vector3[sourceVertices.Length];
            var seamLocalY = float.NegativeInfinity;
            for (var vertexIndex = 0; vertexIndex < sourceVertices.Length; vertexIndex++)
            {
                posedVertices[vertexIndex] = SkinVertex(
                    sourceVertices[vertexIndex],
                    weights[vertexIndex],
                    skinMatrices);
                if (GetDominantBoneIndex(weights[vertexIndex]) == bodyBoneIndex)
                {
                    seamLocalY = Mathf.Max(seamLocalY, posedVertices[vertexIndex].y);
                }
            }

            if (float.IsNegativeInfinity(seamLocalY))
            {
                continue;
            }

            var rowTolerance = Mathf.Max(0.00001f, sourceMesh.bounds.size.y * 0.001f);
            var seamLocalPosition = Vector3.zero;
            var seamVertexCount = 0;
            for (var vertexIndex = 0; vertexIndex < posedVertices.Length; vertexIndex++)
            {
                if (GetDominantBoneIndex(weights[vertexIndex]) != bodyBoneIndex
                    || Mathf.Abs(posedVertices[vertexIndex].y - seamLocalY) > rowTolerance)
                {
                    continue;
                }

                seamLocalPosition += posedVertices[vertexIndex];
                seamVertexCount++;
            }

            if (seamVertexCount > 0)
            {
                seamLocalPosition /= seamVertexCount;
                var seamWorldPosition = renderer.transform.TransformPoint(seamLocalPosition);
                effect.TearSeamRootLocalPosition = effect.Root.transform.InverseTransformPoint(
                    seamWorldPosition);
                effect.HasTearSeamRootLocalPosition = true;
            }

            if (effect.HasTearSeamRootLocalPosition)
            {
                return;
            }
        }
    }

    private static Vector3 SkinVertex(
        Vector3 sourceVertex,
        BoneWeight weight,
        Matrix4x4[] skinMatrices)
    {
        var result = Vector3.zero;
        AddBoneContribution(
            ref result,
            sourceVertex,
            weight.boneIndex0,
            weight.weight0,
            skinMatrices);
        AddBoneContribution(
            ref result,
            sourceVertex,
            weight.boneIndex1,
            weight.weight1,
            skinMatrices);
        AddBoneContribution(
            ref result,
            sourceVertex,
            weight.boneIndex2,
            weight.weight2,
            skinMatrices);
        AddBoneContribution(
            ref result,
            sourceVertex,
            weight.boneIndex3,
            weight.weight3,
            skinMatrices);
        return result;
    }

    private static void AddBoneContribution(
        ref Vector3 result,
        Vector3 sourceVertex,
        int boneIndex,
        float weight,
        Matrix4x4[] skinMatrices)
    {
        if (weight <= 0f || boneIndex < 0 || boneIndex >= skinMatrices.Length)
        {
            return;
        }

        result += skinMatrices[boneIndex].MultiplyPoint3x4(sourceVertex) * weight;
    }

    private static int GetDominantBoneIndex(BoneWeight weight)
    {
        var boneIndex = weight.boneIndex0;
        var maximumWeight = weight.weight0;
        if (weight.weight1 > maximumWeight)
        {
            boneIndex = weight.boneIndex1;
            maximumWeight = weight.weight1;
        }
        if (weight.weight2 > maximumWeight)
        {
            boneIndex = weight.boneIndex2;
            maximumWeight = weight.weight2;
        }
        if (weight.weight3 > maximumWeight)
        {
            boneIndex = weight.boneIndex3;
        }
        return boneIndex;
    }

    private static bool TryInstantiateAuthoredCardPackPrefab(int packId, out GameObject instance)
    {
        instance = null;
        if (packId <= 0 || packId >= AuthoredCardPackPrefabResourcesPaths.Length)
        {
            return false;
        }

        var resourcesPath = AuthoredCardPackPrefabResourcesPaths[packId];
        if (string.IsNullOrEmpty(resourcesPath))
        {
            return false;
        }

        var prefab = Resources.Load<GameObject>(resourcesPath);
        if (prefab == null)
        {
            Debug.LogWarning(
                $"Card-pack authored prefab missing for PackId={packId}: Resources/{resourcesPath}.");
            return false;
        }

        var layoutRoot = new GameObject($"CardPackAuthored_{packId:D3}");
        var authoredInstance = UnityEngine.Object.Instantiate(prefab, layoutRoot.transform, false);
        if (authoredInstance == null)
        {
            UnityEngine.Object.Destroy(layoutRoot);
            return false;
        }

        instance = layoutRoot;
        return true;
    }

    private static GameObject LoadCardPackPrefab(string objectName)
    {
#if UNITY_EDITOR
        var editorPath = $"{CardPackPrefabEditorFolder}/{objectName}.prefab";
        var editorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(editorPath);
        if (editorPrefab != null)
        {
            return editorPrefab;
        }
#endif
        return Resources.Load<GameObject>($"{CardPackPrefabResourcesPath}{objectName}");
    }

    private static void ApplyPreviewPose(
        CardPackEffectInstance effect,
        Transform anchor,
        Sprite coverSprite = null,
        bool applyCover = true)
    {
        if (effect == null || effect.Root == null || anchor == null)
        {
            return;
        }

        var targetTransform = effect.Root.transform;
        var camera = Camera.main;
        const float worldDepth = 0f;
        Vector3 targetPosition;
        Bounds anchorBounds = default;
        var hasAnchorBounds = false;
        if (anchor is RectTransform rectTransform && camera != null)
        {
            targetPosition = GameCommonUtility.RectTransformToCameraWorld(rectTransform, camera, worldDepth);
            anchorBounds = GameCommonUtility.GetRectTransformCameraWorldBounds(rectTransform, camera, worldDepth);
            hasAnchorBounds = anchorBounds.size.x > 0.001f && anchorBounds.size.y > 0.001f;
        }
        else
        {
            targetPosition = anchor.position;
            targetPosition.z = worldDepth;
        }

        targetTransform.position = targetPosition;
        targetTransform.rotation = Quaternion.identity;
        targetTransform.localScale = Vector3.one;
        targetTransform.gameObject.SetActive(true);

        if (effect.PreserveAuthoredAppearance)
        {
            if (TryGetCurrentPoseBounds(effect.CardRenderers, out var authoredBounds))
            {
                targetTransform.position += targetPosition - authoredBounds.center;
            }

            effect.BaseRootPosition = targetTransform.position;
            effect.BaseRootScale = Vector3.one;
            effect.ScaleCenter = targetPosition;
            effect.HasPreparedPose = true;
            return;
        }

        SetRenderersEnabled(effect.CardRenderers, false);
        if (applyCover)
        {
            ApplyCardPackCovers(effect.CardRenderers, coverSprite);
        }

        if (hasAnchorBounds && TryGetCurrentPoseBounds(effect.CardRenderers, out var modelBounds))
        {
            var scale = Mathf.Min(
                anchorBounds.size.x / modelBounds.size.x,
                anchorBounds.size.y / modelBounds.size.y);
            targetTransform.localScale = Vector3.one * Mathf.Max(scale, 0.001f);

            if (TryGetCurrentPoseBounds(effect.CardRenderers, out var scaledBounds))
            {
                targetTransform.position += targetPosition - scaledBounds.center;
            }
        }
        else
        {
            var anchorSize = Mathf.Max(anchor.lossyScale.x, anchor.lossyScale.y, 0.01f) * 4f;
            targetTransform.localScale = Vector3.one * Mathf.Max(1.2f, anchorSize * 0.55f);
        }

        SetRenderersEnabled(effect.CardRenderers, true);
        effect.BaseRootPosition = targetTransform.position;
        effect.BaseRootScale = targetTransform.localScale;
        effect.ScaleCenter = targetPosition;
        effect.HasPreparedPose = true;
    }

    private static void SetRenderersEnabled(Renderer[] renderers, bool enabled)
    {
        if (renderers == null)
        {
            return;
        }

        for (var i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = enabled;
            }
        }
    }

    private static void SetRendererSortingOrder(Renderer[] renderers, int sortingOrder)
    {
        if (renderers == null)
        {
            return;
        }

        for (var i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].sortingOrder = sortingOrder;
            }
        }
    }

    private static void SetLayerRecursively(Transform root, int layer)
    {
        if (root == null)
        {
            return;
        }

        root.gameObject.layer = layer;
        for (var i = 0; i < root.childCount; i++)
        {
            SetLayerRecursively(root.GetChild(i), layer);
        }
    }

    private static bool TryGetCurrentPoseBounds(Renderer[] renderers, out Bounds bounds)
    {
        bounds = default;
        var hasBounds = false;
        if (renderers == null)
        {
            return false;
        }

        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            if (renderer == null || !renderer.gameObject.activeInHierarchy)
            {
                continue;
            }

            var rendererBounds = renderer.bounds;

            if (rendererBounds.size.x <= 0.001f || rendererBounds.size.y <= 0.001f)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = rendererBounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(rendererBounds);
            }
        }

        return hasBounds;
    }

    private static void ApplyCardPackCovers(Renderer[] renderers, Sprite coverSprite = null)
    {
        if (renderers == null)
        {
            return;
        }

        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            ApplyCardPackCover(renderer, coverSprite);
        }
    }

    private static void ApplyCardPackCover(Renderer renderer, Sprite coverSprite)
    {
        if (renderer == null)
        {
            return;
        }

        if (coverSprite == null || coverSprite.texture == null)
        {
            renderer.SetPropertyBlock(null);
            return;
        }

        var coverTexture = coverSprite.texture;
        coverTexture.wrapMode = TextureWrapMode.Clamp;
        coverTexture.filterMode = FilterMode.Bilinear;

        var propertyBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propertyBlock);
        var uvTransform = CalculateCoverUvTransform(coverSprite);
        propertyBlock.SetTexture(FrontFacesAlbedoPropertyId, coverTexture);
        propertyBlock.SetVector(FrontFacesAlbedoTransformPropertyId, uvTransform);
        propertyBlock.SetColor(FrontFacesColorPropertyId, renderer.sharedMaterial.GetColor(FrontFacesColorPropertyId));
        propertyBlock.SetTexture(BaseMapPropertyId, coverTexture);
        propertyBlock.SetVector(BaseMapTransformPropertyId, uvTransform);
        propertyBlock.SetTexture(MainTexturePropertyId, coverTexture);
        propertyBlock.SetVector(MainTextureTransformPropertyId, uvTransform);
        renderer.SetPropertyBlock(propertyBlock);
    }

    private static void ApplyCardPackAppearance(
        Renderer renderer,
        Sprite coverSprite,
        Color? frontColorOverride,
        Rect screenClipRect,
        bool useClipRect)
    {
        ApplyCardPackCover(renderer, coverSprite);
        if (renderer == null)
        {
            return;
        }

        var propertyBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propertyBlock);
        if (frontColorOverride.HasValue)
        {
            propertyBlock.SetColor(FrontFacesColorPropertyId, frontColorOverride.Value);
        }
        SetClipProperties(propertyBlock, screenClipRect, useClipRect);
        renderer.SetPropertyBlock(propertyBlock);
    }

    private static void ApplyCardPackClip(Renderer[] renderers, Rect screenClipRect, bool useClipRect)
    {
        if (renderers == null)
        {
            return;
        }

        for (var i = 0; i < renderers.Length; i++)
        {
            ApplyCardPackClip(renderers[i], screenClipRect, useClipRect);
        }
    }

    private static void ApplyCardPackClip(Renderer renderer, Rect screenClipRect, bool useClipRect)
    {
        if (renderer == null)
        {
            return;
        }

        var propertyBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propertyBlock);
        SetClipProperties(propertyBlock, screenClipRect, useClipRect);
        renderer.SetPropertyBlock(propertyBlock);
    }

    private static void SetClipProperties(
        MaterialPropertyBlock propertyBlock,
        Rect screenClipRect,
        bool useClipRect)
    {
        propertyBlock.SetVector(
            UiClipRectPropertyId,
            new Vector4(
                screenClipRect.xMin,
                screenClipRect.yMin,
                screenClipRect.xMax,
                screenClipRect.yMax));
        propertyBlock.SetFloat(UseUiClipRectPropertyId, useClipRect ? 1f : 0f);
    }

    private static Vector4 CalculateCoverUvTransform(Sprite coverSprite)
    {
        var texture = coverSprite != null ? coverSprite.texture : null;
        if (texture == null || texture.width <= 0 || texture.height <= 0)
        {
            return new Vector4(1f, 1f, 0f, 0f);
        }

        var sourceRect = coverSprite.textureRect;
        if (sourceRect.width <= 0f || sourceRect.height <= 0f)
        {
            sourceRect = new Rect(0f, 0f, texture.width, texture.height);
        }

        return new Vector4(
            sourceRect.width / texture.width,
            sourceRect.height / texture.height,
            sourceRect.x / texture.width,
            sourceRect.y / texture.height);
    }

}

internal sealed class CardPackDismantleLifetime : MonoBehaviour
{
    private float mLifetime;

    public static void Attach(GameObject root, float lifetime)
    {
        if (root == null)
        {
            return;
        }

        var component = root.GetComponent<CardPackDismantleLifetime>();
        if (component == null)
        {
            component = root.AddComponent<CardPackDismantleLifetime>();
        }

        component.mLifetime = Mathf.Max(0.1f, lifetime);
    }

    private IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(mLifetime);
        Destroy(gameObject);
    }
}

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
