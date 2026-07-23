#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CardPackDismantlePreviewEditor
{
    private const string AnimatedCardPackRootName = "AnimatedCardPack";
    private const string DismantleEffectRootName = "DismantleEffect";
    private const string CardPackFolder = "Assets/Resources/Effects/CardPack";
    private const string CardPackMaterialPath = CardPackFolder + "/CardPackOpeningMaterial.mat";
    private const string SourceEffectPath =
        "Assets/Resources/Effects/CardPackDismantle/CardPackDismantle_001.prefab";
    private const string CoverSpritePath = "Assets/UI/PackImages/PackIcon001.png";
    private const string PreviewPrefabPath =
        "Assets/Resources/Effects/CardPackDismantle/CardPackDismantlePreview.prefab";
    private const float PreviewCoverWidth = 2.4f;
    private const float LoopPauseDuration = 0.5f;
    private const int MaxFocusAttempts = 20;

    private static readonly string[] AnimatedCardPackPrefabPaths =
    {
        CardPackFolder + "/CardPackOpening.prefab",
        CardPackFolder + "/CardPackOpening_002.prefab",
        CardPackFolder + "/CardPackOpening_003.prefab",
        CardPackFolder + "/CardPackOpening_004.prefab",
        CardPackFolder + "/CardPackOpening_005.prefab",
        CardPackFolder + "/CardPackOpening_006.prefab"
    };

    private static readonly int BaseMapPropertyId = Shader.PropertyToID("_BaseMap");
    private static readonly int BaseMapTransformPropertyId = Shader.PropertyToID("_BaseMap_ST");
    private static readonly int MainTexturePropertyId = Shader.PropertyToID("_MainTex");
    private static readonly int MainTextureTransformPropertyId = Shader.PropertyToID("_MainTex_ST");
    private static readonly int FrontFacesAlbedoPropertyId = Shader.PropertyToID("_FrontFacesAlbedo");
    private static readonly int FrontFacesAlbedoTransformPropertyId = Shader.PropertyToID("_FrontFacesAlbedo_ST");

    private static Animator[] sPreviewAnimators;
    private static ParticleSystem sPreviewParticleRoot;
    private static SceneView sPreviewSceneView;
    private static double sPreviewStartedAt;
    private static float sPreviewDuration;

    [DidReloadScripts]
    private static void CreatePreviewAfterReload()
    {
        EditorApplication.delayCall += EnsureCompletePreviewExists;
    }

    [MenuItem("Puffies/Effects/Preview Card Pack Dismantle %#d")]
    public static void OpenPreview()
    {
        var previewPrefab = EnsurePreviewExists();
        if (previewPrefab == null)
        {
            return;
        }

        AssetDatabase.OpenAsset(previewPrefab);
        QueueFocusPreviewStage();
    }

    [MenuItem("Puffies/Effects/Rebuild Card Pack Dismantle Preview")]
    public static void RebuildPreview()
    {
        StopCombinedPreview();
        var previewPrefab = BuildPreviewPrefab();
        if (previewPrefab == null)
        {
            return;
        }

        AssetDatabase.OpenAsset(previewPrefab);
        QueueFocusPreviewStage();
    }

    private static GameObject EnsurePreviewExists()
    {
        var previewPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PreviewPrefabPath);
        return IsCompletePreview(previewPrefab) ? previewPrefab : BuildPreviewPrefab();
    }

    private static void EnsureCompletePreviewExists()
    {
        EnsurePreviewExists();
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null && stage.assetPath == PreviewPrefabPath)
        {
            QueueFocusPreviewStage();
        }
    }

    private static bool IsCompletePreview(GameObject previewPrefab)
    {
        return previewPrefab != null
            && previewPrefab.transform.Find(AnimatedCardPackRootName) != null
            && previewPrefab.transform.Find(DismantleEffectRootName) != null;
    }

    private static GameObject BuildPreviewPrefab()
    {
        var sourceEffect = AssetDatabase.LoadAssetAtPath<GameObject>(SourceEffectPath);
        var coverSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CoverSpritePath);
        var cardPackMaterial = AssetDatabase.LoadAssetAtPath<Material>(CardPackMaterialPath);
        if (sourceEffect == null || coverSprite == null || cardPackMaterial == null)
        {
            Debug.LogError(
                $"Card-pack dismantle preview is missing source assets. "
                + $"effect={sourceEffect != null}, cover={coverSprite != null}, "
                + $"material={cardPackMaterial != null}");
            return null;
        }

        var root = new GameObject("CardPackDismantlePreview");
        try
        {
            if (!CreateAnimatedCardPack(root.transform, cardPackMaterial))
            {
                return null;
            }

            var effect = PrefabUtility.InstantiatePrefab(sourceEffect) as GameObject;
            if (effect == null)
            {
                Debug.LogError("Failed to instantiate the card-pack dismantle effect.");
                return null;
            }

            effect.name = DismantleEffectRootName;
            effect.transform.SetParent(root.transform, false);

            var previewPrefab = PrefabUtility.SaveAsPrefabAsset(root, PreviewPrefabPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Complete card-pack dismantle preview generated: {PreviewPrefabPath}");
            return previewPrefab;
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    private static bool CreateAnimatedCardPack(Transform parent, Material cardPackMaterial)
    {
        var animatedRoot = new GameObject(AnimatedCardPackRootName);
        animatedRoot.transform.SetParent(parent, false);
        animatedRoot.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        for (var i = 0; i < AnimatedCardPackPrefabPaths.Length; i++)
        {
            var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AnimatedCardPackPrefabPaths[i]);
            if (sourcePrefab == null)
            {
                Debug.LogError($"Card-pack preview layer missing: {AnimatedCardPackPrefabPaths[i]}");
                return false;
            }

            var layer = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
            if (layer == null)
            {
                Debug.LogError($"Failed to instantiate card-pack preview layer: {sourcePrefab.name}");
                return false;
            }

            layer.name = sourcePrefab.name;
            layer.transform.SetParent(animatedRoot.transform, false);
            ApplyMaterial(layer.GetComponentsInChildren<Renderer>(true), cardPackMaterial);
        }

        FitAnimatedCardPack(animatedRoot.transform);
        return true;
    }

    private static void ApplyMaterial(Renderer[] renderers, Material material)
    {
        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            var materials = renderer.sharedMaterials;
            for (var materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                materials[materialIndex] = material;
            }

            renderer.sharedMaterials = materials;
        }
    }

    private static void FitAnimatedCardPack(Transform animatedRoot)
    {
        var renderers = animatedRoot.GetComponentsInChildren<Renderer>(true);
        if (!TryGetRendererBounds(renderers, out var bounds) || bounds.size.x <= 0.001f)
        {
            Debug.LogWarning("Could not measure the assembled card-pack preview bounds.");
            return;
        }

        animatedRoot.localScale = Vector3.one * (PreviewCoverWidth / bounds.size.x);
        if (TryGetRendererBounds(renderers, out bounds))
        {
            animatedRoot.position += new Vector3(-bounds.center.x, -bounds.center.y, -bounds.max.z);
        }
    }

    private static bool TryGetRendererBounds(Renderer[] renderers, out Bounds bounds)
    {
        bounds = default;
        var hasBounds = false;
        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return hasBounds;
    }

    private static void QueueFocusPreviewStage(int attempt = 0)
    {
        EditorApplication.delayCall += () => FocusPreviewStage(attempt);
    }

    private static void FocusPreviewStage(int attempt)
    {
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage == null || stage.assetPath != PreviewPrefabPath)
        {
            if (attempt < MaxFocusAttempts)
            {
                QueueFocusPreviewStage(attempt + 1);
            }

            return;
        }

        var animatedCardPack = stage.prefabContentsRoot.transform.Find(AnimatedCardPackRootName);
        var effect = stage.prefabContentsRoot.transform.Find(DismantleEffectRootName);
        if (animatedCardPack == null || effect == null)
        {
            return;
        }

        var coverSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CoverSpritePath);
        ApplyCover(animatedCardPack.GetComponentsInChildren<Renderer>(true), coverSprite);
        SceneVisibilityManager.instance.ShowAll();
        var animators = animatedCardPack.GetComponentsInChildren<Animator>(true);
        var particleSystems = effect.GetComponentsInChildren<ParticleSystem>(true);
        Selection.activeGameObject = stage.prefabContentsRoot;
        StartCombinedPreview(animators, particleSystems);
        Debug.Log(
            $"Card-pack combined preview started. animators={animators.Length}, "
            + $"particles={particleSystems.Length}");

        var focusedWindow = EditorWindow.focusedWindow;
        if (focusedWindow != null && focusedWindow.maximized)
        {
            focusedWindow.maximized = false;
        }

        var sceneView = GetOrCreatePreviewSceneView();
        sceneView.maximized = false;
        sceneView.Show();
        sceneView.Focus();

        var aspect = coverSprite != null && coverSprite.bounds.size.x > 0.001f
            ? coverSprite.bounds.size.y / coverSprite.bounds.size.x
            : 1f;
        var coverHeight = PreviewCoverWidth * aspect;
        var rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
        sceneView.in2DMode = false;
        sceneView.LookAt(new Vector3(0f, 0f, -coverHeight * 0.5f), rotation, coverHeight * 0.7f, true);
        sceneView.Repaint();
    }

    private static SceneView GetOrCreatePreviewSceneView()
    {
        if (sPreviewSceneView != null)
        {
            return sPreviewSceneView;
        }

        sPreviewSceneView = EditorWindow.GetWindow<SceneView>(
            false,
            "Card Pack Preview",
            true);
        return sPreviewSceneView;
    }

    private static void ApplyCover(Renderer[] renderers, Sprite coverSprite)
    {
        if (renderers == null || coverSprite == null || coverSprite.texture == null)
        {
            return;
        }

        var texture = coverSprite.texture;
        var textureRect = coverSprite.textureRect;
        var uvTransform = new Vector4(
            textureRect.width / texture.width,
            textureRect.height / texture.height,
            textureRect.x / texture.width,
            textureRect.y / texture.height);
        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            var propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetTexture(FrontFacesAlbedoPropertyId, texture);
            propertyBlock.SetVector(FrontFacesAlbedoTransformPropertyId, uvTransform);
            propertyBlock.SetTexture(BaseMapPropertyId, texture);
            propertyBlock.SetVector(BaseMapTransformPropertyId, uvTransform);
            propertyBlock.SetTexture(MainTexturePropertyId, texture);
            propertyBlock.SetVector(MainTextureTransformPropertyId, uvTransform);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    private static void StartCombinedPreview(Animator[] animators, ParticleSystem[] particleSystems)
    {
        StopCombinedPreview();
        sPreviewAnimators = animators;
        sPreviewParticleRoot = FindParticleRoot(particleSystems);
        sPreviewDuration = GetAnimationDuration(animators);
        sPreviewStartedAt = EditorApplication.timeSinceStartup;
        AnimationMode.StartAnimationMode();
        EditorApplication.update += UpdateCombinedPreview;
    }

    private static ParticleSystem FindParticleRoot(ParticleSystem[] particleSystems)
    {
        if (particleSystems == null || particleSystems.Length == 0)
        {
            return null;
        }

        for (var i = 0; i < particleSystems.Length; i++)
        {
            var candidate = particleSystems[i];
            if (candidate != null && candidate.transform.parent != null
                && candidate.transform.parent.name == DismantleEffectRootName)
            {
                return candidate;
            }
        }

        return particleSystems[0];
    }

    private static float GetAnimationDuration(Animator[] animators)
    {
        var duration = 0f;
        if (animators == null)
        {
            return 1f;
        }

        for (var i = 0; i < animators.Length; i++)
        {
            var controller = animators[i] != null ? animators[i].runtimeAnimatorController : null;
            if (controller == null)
            {
                continue;
            }

            var clips = controller.animationClips;
            for (var clipIndex = 0; clipIndex < clips.Length; clipIndex++)
            {
                if (clips[clipIndex] != null)
                {
                    duration = Mathf.Max(duration, clips[clipIndex].length);
                }
            }
        }

        return Mathf.Max(duration, 1f);
    }

    private static void UpdateCombinedPreview()
    {
        if (!AnimationMode.InAnimationMode())
        {
            StopCombinedPreview();
            return;
        }

        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage == null || stage.assetPath != PreviewPrefabPath)
        {
            StopCombinedPreview();
            return;
        }

        var loopDuration = sPreviewDuration + LoopPauseDuration;
        var elapsed = (float)(EditorApplication.timeSinceStartup - sPreviewStartedAt);
        var sampleTime = Mathf.Min(Mathf.Repeat(elapsed, loopDuration), sPreviewDuration);

        if (sPreviewAnimators != null)
        {
            for (var i = 0; i < sPreviewAnimators.Length; i++)
            {
                var animator = sPreviewAnimators[i];
                var controller = animator != null ? animator.runtimeAnimatorController : null;
                if (controller == null || controller.animationClips.Length == 0)
                {
                    continue;
                }

                AnimationMode.SampleAnimationClip(
                    animator.gameObject,
                    controller.animationClips[0],
                    sampleTime);
            }
        }

        if (sPreviewParticleRoot != null)
        {
            sPreviewParticleRoot.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            sPreviewParticleRoot.Simulate(sampleTime, true, true, true);
            sPreviewParticleRoot.Pause(true);
        }

        SceneView.RepaintAll();
    }

    private static void StopCombinedPreview()
    {
        EditorApplication.update -= UpdateCombinedPreview;
        sPreviewAnimators = null;
        sPreviewParticleRoot = null;
        sPreviewDuration = 0f;
        if (AnimationMode.InAnimationMode())
        {
            AnimationMode.StopAnimationMode();
        }
    }
}
#endif
