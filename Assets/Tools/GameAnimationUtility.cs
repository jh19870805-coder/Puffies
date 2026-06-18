using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class GameAnimationUtility
{
    private const string DefaultCardPackStateName = "Take 001";
    private const string CardPackAniPrefix = "mesh_ani_";
    private const string CardPackSkinPrefix = "mesh_skin_";
    private const string CardPackAnimationFolder = GameDefine.CardPackAnimationEditorFolder;
    private const string CardPackPrefabEditorFolder = GameDefine.CardPackPrefabEditorFolder;
    private const string CardPackMaterialEditorPath = GameDefine.CardPackMaterialEditorPath;
    private const string CardPackPrefabResourcesPath = GameDefine.CardPackPrefabResourcesFolder;
    private const string CardPackMaterialResourcesPath = GameDefine.CardPackMaterialResourcesPath;
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
    /// 用途：按动画文件名播放对应卡包动画。返回：是否成功触发播放。
    /// </summary>
    /// <param name="animationFileName">参数：动画文件名或状态名，例如 mesh_ani_cardPack_001.FBX。</param>
    /// <param name="searchRoot">参数：查找对象的根节点；传 null 时在全场景查找。</param>
    /// <returns>返回：true 表示播放成功，false 表示未找到目标或播放失败。</returns>
    public static bool PlayCardPackAnimation(string animationFileName, Transform searchRoot = null)
    {
        if (string.IsNullOrWhiteSpace(animationFileName))
        {
            Debug.LogWarning("PlayCardPackAnimation failed: animationFileName is empty.");
            return false;
        }

        var objectName = ResolveCardPackTargetObjectName(animationFileName);
        var animator = FindAnimatorByObjectName(objectName, searchRoot);
        if (animator == null)
        {
            animator = TrySpawnCardPackAnimator(objectName, searchRoot);
        }
        else if (searchRoot != null)
        {
            ApplyPreviewPose(animator, searchRoot);
        }
        else
        {
            ApplyCardPackMaterials(animator.GetComponentsInChildren<Renderer>(true));
        }

        if (animator == null)
        {
            return false;
        }

        var stateName = ResolveCardPackStateName(animationFileName);
        animator.Rebind();
        animator.Update(0f);
        animator.Play(stateName, 0, 0f);
        animator.Update(0f);
        return true;
    }

    /// <summary>
    /// 用途：估算卡包动画播放时长，供主场景在切页前等待。返回：时长大于 0 表示有效。
    /// </summary>
    public static float GetCardPackPlayDuration(string animationFileName, Transform searchRoot = null)
    {
        if (string.IsNullOrWhiteSpace(animationFileName))
        {
            return 0f;
        }

        var objectName = ResolveCardPackTargetObjectName(animationFileName);
        var animator = FindAnimatorByObjectName(objectName, searchRoot);
        if (animator == null)
        {
            animator = TrySpawnCardPackAnimator(objectName, searchRoot);
        }

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return 0f;
        }

        var stateName = ResolveCardPackStateName(animationFileName);
        var clips = animator.runtimeAnimatorController.animationClips;
        for (var i = 0; i < clips.Length; i++)
        {
            var clip = clips[i];
            if (clip != null && clip.name.Equals(stateName, StringComparison.OrdinalIgnoreCase))
            {
                return clip.length;
            }
        }

        return clips.Length > 0 && clips[0] != null ? clips[0].length : 0f;
    }

    /// <summary>
    /// 用途：获取工程中实际存在的卡包动画文件名列表（仅含文件名）。返回：按名称升序的文件名集合。
    /// </summary>
    /// <returns>返回：例如 mesh_ani_cardPack_001.FBX。</returns>
    public static List<string> GetAvailableCardPackAnimationFileNames()
    {
        var result = new List<string>();
#if UNITY_EDITOR
        if (!Directory.Exists(CardPackAnimationFolder))
        {
            return result;
        }

        result = Directory
            .GetFiles(CardPackAnimationFolder, $"{CardPackAniPrefix}*.FBX")
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
#endif
        return result;
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

    private static string ResolveCardPackTargetObjectName(string animationFileName)
    {
        var normalizedName = Path.GetFileNameWithoutExtension(animationFileName.Trim());
        if (normalizedName.StartsWith(CardPackAniPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return CardPackSkinPrefix + normalizedName.Substring(CardPackAniPrefix.Length);
        }

        return normalizedName;
    }

    private static string ResolveCardPackStateName(string animationFileName)
    {
        var normalizedName = Path.GetFileNameWithoutExtension(animationFileName.Trim());
        return normalizedName.Equals(DefaultCardPackStateName, StringComparison.OrdinalIgnoreCase)
            ? normalizedName
            : DefaultCardPackStateName;
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

    private static Animator TrySpawnCardPackAnimator(string objectName, Transform anchor)
    {
        if (string.IsNullOrWhiteSpace(objectName) || anchor == null)
        {
            return null;
        }

        if (sSpawnedAnimators.TryGetValue(objectName, out var cachedAnimator) && cachedAnimator != null)
        {
            ApplyPreviewPose(cachedAnimator, anchor);
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

        ApplyPreviewPose(animator, anchor);
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

    private static void ApplyPreviewPose(Animator animator, Transform anchor)
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
        float anchorSize;
        if (anchor is RectTransform rectTransform && camera != null)
        {
            targetPosition = GameCommonUtility.RectTransformToCameraWorld(rectTransform, camera, worldDepth);
            anchorSize = GameCommonUtility.GetRectTransformWorldHeight(rectTransform, camera, worldDepth);
        }
        else
        {
            targetPosition = anchor.position;
            targetPosition.z = worldDepth;
            anchorSize = Mathf.Max(anchor.lossyScale.x, anchor.lossyScale.y, 0.01f) * 4f;
        }

        targetTransform.position = targetPosition;
        targetTransform.rotation = prefabRotation;
        targetTransform.localScale = Vector3.one * Mathf.Max(1.2f, anchorSize * 0.55f);
        targetTransform.gameObject.SetActive(true);

        var renderers = animator.GetComponentsInChildren<Renderer>(true);
        for (var i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = true;
            }
        }

        ApplyCardPackMaterials(renderers);
    }

    /// <summary>
    /// 用途：将卡包模型上不兼容 URP 的内置管线材质替换为可用的 Lit 材质。返回：无。
    /// </summary>
    private static void ApplyCardPackMaterials(Renderer[] renderers)
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
        }
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
