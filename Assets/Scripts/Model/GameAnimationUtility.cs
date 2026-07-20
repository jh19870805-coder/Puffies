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
    private const string CardPackMaterialEditorPath = GameDefine.CardPackMaterialEditorPath;
    private const string CardPackPrefabResourcesPath = GameDefine.CardPackPrefabResourcesFolder;
    private const string CardPackMaterialResourcesPath = GameDefine.CardPackMaterialResourcesPath;
    private const string GenericCardPackObjectName = GameDefine.CardPackOpeningPrefabName;
    private static readonly int BaseMapPropertyId = Shader.PropertyToID("_BaseMap");
    private static readonly int BaseMapTransformPropertyId = Shader.PropertyToID("_BaseMap_ST");
    private static readonly int MainTexturePropertyId = Shader.PropertyToID("_MainTex");
    private static readonly int MainTextureTransformPropertyId = Shader.PropertyToID("_MainTex_ST");
    private static readonly Dictionary<string, Animator> sSpawnedAnimators = new Dictionary<string, Animator>();
    private static Material sCardPackLitMaterial;

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

        var animator = FindAnimatorByObjectName(GenericCardPackObjectName, anchor);
        if (animator == null)
        {
            animator = TrySpawnCardPackAnimator(GenericCardPackObjectName, anchor);
        }

        if (animator == null)
        {
            return false;
        }

        animator.Rebind();
        animator.Update(0f);
        animator.Play(DefaultCardPackStateName, 0, 0f);
        animator.Update(0f);
        if (anchor != null)
        {
            ApplyPreviewPose(animator, anchor, coverSprite);
        }
        else
        {
            ApplyCardPackMaterials(animator.GetComponentsInChildren<Renderer>(true), coverSprite);
        }

        return true;
    }

    /// <summary>
    /// 用途：估算卡包动画播放时长，供主场景在切页前等待。返回：时长大于 0 表示有效。
    /// </summary>
    public static float GetCardPackPlayDuration(Transform anchor = null)
    {
        Animator animator = null;
        if (sSpawnedAnimators.TryGetValue(GenericCardPackObjectName, out var cachedAnimator)
            && cachedAnimator != null)
        {
            animator = cachedAnimator;
        }

        if (animator == null)
        {
            animator = FindAnimatorByObjectName(GenericCardPackObjectName, anchor);
        }

        if (animator == null)
        {
            animator = TrySpawnCardPackAnimator(GenericCardPackObjectName, anchor);
        }

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return 0f;
        }

        var clips = animator.runtimeAnimatorController.animationClips;
        for (var i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null)
            {
                return clips[i].length;
            }
        }

        return 0f;
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

    private static Animator FindAnimatorByObjectName(string objectName, Transform searchRoot)
    {
        if (searchRoot != null)
        {
            var target = FindDeepChild(searchRoot, objectName);
            return target != null ? target.GetComponentInChildren<Animator>(true) : null;
        }

        var sceneObject = GameObject.Find(objectName);
        return sceneObject != null ? sceneObject.GetComponentInChildren<Animator>(true) : null;
    }

    private static Transform FindDeepChild(Transform root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        if (string.Equals(root.name, childName, StringComparison.OrdinalIgnoreCase))
        {
            return root;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var result = FindDeepChild(root.GetChild(i), childName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static Animator TrySpawnCardPackAnimator(
        string objectName,
        Transform anchor)
    {
        if (string.IsNullOrWhiteSpace(objectName) || anchor == null)
        {
            return null;
        }

        if (sSpawnedAnimators.TryGetValue(objectName, out var cachedAnimator) && cachedAnimator != null)
        {
            return cachedAnimator;
        }

        var prefab = LoadCardPackPrefab(objectName);
        if (prefab == null)
        {
            return null;
        }

        var instance = UnityEngine.Object.Instantiate(prefab);
        instance.name = objectName;
        var animator = instance.GetComponentInChildren<Animator>(true);
        if (animator == null)
        {
            UnityEngine.Object.Destroy(instance);
            return null;
        }

        sSpawnedAnimators[objectName] = animator;
        return animator;
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

    private static void ApplyPreviewPose(Animator animator, Transform anchor, Sprite coverSprite = null)
    {
        if (animator == null || anchor == null)
        {
            return;
        }

        var targetTransform = animator.transform;
        var prefabRotation = targetTransform.rotation;
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
        targetTransform.rotation = prefabRotation;
        targetTransform.localScale = Vector3.one;
        targetTransform.gameObject.SetActive(true);

        var renderers = animator.GetComponentsInChildren<Renderer>(true);
        for (var i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = false;
            }
        }

        ApplyCardPackMaterials(renderers, coverSprite);

        if (hasAnchorBounds && TryGetCurrentPoseBounds(renderers, out var modelBounds))
        {
            var scale = Mathf.Min(
                anchorBounds.size.x / modelBounds.size.x,
                anchorBounds.size.y / modelBounds.size.y);
            targetTransform.localScale = Vector3.one * Mathf.Max(scale, 0.001f);

            if (TryGetCurrentPoseBounds(renderers, out var scaledBounds))
            {
                targetTransform.position += targetPosition - scaledBounds.center;
            }
        }
        else
        {
            var anchorSize = Mathf.Max(anchor.lossyScale.x, anchor.lossyScale.y, 0.01f) * 4f;
            targetTransform.localScale = Vector3.one * Mathf.Max(1.2f, anchorSize * 0.55f);
        }

        for (var i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = true;
            }
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
            if (renderer is SkinnedMeshRenderer skinnedRenderer
                && TryGetSkinnedMeshBounds(skinnedRenderer, out var skinnedBounds))
            {
                rendererBounds = skinnedBounds;
            }

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

    private static bool TryGetSkinnedMeshBounds(SkinnedMeshRenderer renderer, out Bounds bounds)
    {
        bounds = default;
        if (renderer == null || renderer.sharedMesh == null)
        {
            return false;
        }

        var bakedMesh = new Mesh();
        try
        {
            renderer.BakeMesh(bakedMesh, false);
            var vertices = bakedMesh.vertices;
            if (vertices == null || vertices.Length == 0)
            {
                return false;
            }

            var localToWorld = renderer.transform.localToWorldMatrix;
            bounds = new Bounds(localToWorld.MultiplyPoint3x4(vertices[0]), Vector3.zero);
            for (var i = 1; i < vertices.Length; i++)
            {
                bounds.Encapsulate(localToWorld.MultiplyPoint3x4(vertices[i]));
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Failed to measure card-pack skinned mesh bounds: {exception.Message}");
            return false;
        }
        finally
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(bakedMesh);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(bakedMesh);
            }
        }
    }

    /// <summary>
    /// 用途：将卡包模型上不兼容 URP 的内置管线材质替换为可用的 Lit 材质。返回：无。
    /// </summary>
    private static void ApplyCardPackMaterials(Renderer[] renderers, Sprite coverSprite = null)
    {
        var litMaterial = GetCardPackLitMaterial();
        if (litMaterial == null || renderers == null)
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

            var materials = renderer.sharedMaterials;
            var changed = false;
            for (var j = 0; j < materials.Length; j++)
            {
                if (!IsSupportedCardPackMaterial(materials[j]))
                {
                    materials[j] = litMaterial;
                    changed = true;
                }
            }

            if (changed)
            {
                renderer.sharedMaterials = materials;
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
        propertyBlock.SetTexture(BaseMapPropertyId, coverTexture);
        propertyBlock.SetVector(BaseMapTransformPropertyId, uvTransform);
        propertyBlock.SetTexture(MainTexturePropertyId, coverTexture);
        propertyBlock.SetVector(MainTextureTransformPropertyId, uvTransform);
        renderer.SetPropertyBlock(propertyBlock);
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

    private static bool IsSupportedCardPackMaterial(Material material)
    {
        if (material == null || material.shader == null)
        {
            return false;
        }

        return material.shader.isSupported
            && material.shader.name.IndexOf("InternalError", StringComparison.OrdinalIgnoreCase) < 0
            && material.shader.name.IndexOf("ASESampleShaders", StringComparison.OrdinalIgnoreCase) < 0;
    }

    private static Material GetCardPackLitMaterial()
    {
        if (sCardPackLitMaterial != null)
        {
            return sCardPackLitMaterial;
        }

        sCardPackLitMaterial = Resources.Load<Material>(CardPackMaterialResourcesPath);
        if (sCardPackLitMaterial != null)
        {
            return sCardPackLitMaterial;
        }

#if UNITY_EDITOR
        sCardPackLitMaterial = AssetDatabase.LoadAssetAtPath<Material>(CardPackMaterialEditorPath);
        if (sCardPackLitMaterial != null)
        {
            return sCardPackLitMaterial;
        }
#endif

        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            return null;
        }

        sCardPackLitMaterial = new Material(shader)
        {
            name = "CardPackRuntimeLit"
        };
        return sCardPackLitMaterial;
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
