using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 用途：运行时准备 CardFx 粒子预制体用于预览/播放。返回：按方法说明。
/// </summary>
public static class CardFxRuntimeUtility
{
    private const float PreviewMaxParticleSize = 9000f;
    private const string UiFxShaderToken = "UI_FX";
    private const string WorldFxShaderName = "URP/Effect/UPR_FX_Common";
    private const string BuiltInPacketShaderName = "BF/Effect/EffectPacket";
    private const string BuiltInClipShaderName = "BF/Effect/A/AParticleFireClip10";
    private const string InternalErrorShaderName = "Hidden/InternalErrorShader";

    private static Shader sWorldFxShader;
    private static Shader sBuiltInPacketShader;
    private static Shader sBuiltInClipShader;
    private static Material sFallbackMaterial;

    /// <summary>
    /// 用途：世界空间预览准备（解除尺寸上限、UI 材质转世界 shader）。返回：无。
    /// </summary>
    public static void PreparePreview(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        root.transform.localScale = Vector3.one * GameDefine.PixelsPerUnit;
        PrepareParticleSystems(root);
        PrepareRenderers(root, false, 0, 0);
    }

    public static void PrepareRuntimeWorldEffect(
        GameObject root,
        float worldScale,
        int sortingLayerId,
        int sortingOrder)
    {
        if (root == null)
        {
            return;
        }

        root.transform.localScale = Vector3.one * Mathf.Max(worldScale, 0.001f);
        PrepareParticleSystems(root);
        PrepareRenderers(root, true, sortingLayerId, sortingOrder);
    }

    /// <summary>
    /// 用途：重播根节点下全部粒子系统。返回：无。
    /// </summary>
    public static void ReplayParticleSystems(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        var particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);
        for (var i = 0; i < particleSystems.Length; i++)
        {
            var particleSystem = particleSystems[i];
            if (particleSystem == null)
            {
                continue;
            }

            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystem.Clear(true);
            particleSystem.Play(true);
        }
    }

    private static void PrepareParticleSystems(GameObject root)
    {
        var particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);
        for (var i = 0; i < particleSystems.Length; i++)
        {
            var particleSystem = particleSystems[i];
            if (particleSystem == null)
            {
                continue;
            }

            var main = particleSystem.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.playOnAwake = false;
            particleSystem.gameObject.SetActive(true);
        }
    }

    public static void ReleasePreparedMaterials(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        var renderers = root.GetComponentsInChildren<ParticleSystemRenderer>(true);
        for (var i = 0; i < renderers.Length; i++)
        {
            var materials = renderers[i] != null ? renderers[i].sharedMaterials : null;
            if (materials == null)
            {
                continue;
            }

            for (var materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                var material = materials[materialIndex];
                if (material != null
                    && (material.name.EndsWith("_Preview")
                        || material.name.EndsWith("_WorldPreview")))
                {
                    Object.Destroy(material);
                }
            }
        }
    }

    private static void PrepareRenderers(
        GameObject root,
        bool overrideSorting,
        int sortingLayerId,
        int sortingOrder)
    {
        EnsureWorldFxShader();
        var renderers = root.GetComponentsInChildren<ParticleSystemRenderer>(true);
        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            renderer.enabled = true;
            renderer.maxParticleSize = PreviewMaxParticleSize;
            renderer.minParticleSize = 0f;
            if (overrideSorting)
            {
                renderer.sortingLayerID = sortingLayerId;
                renderer.sortingOrder = sortingOrder;
            }

            var sourceMaterials = renderer.sharedMaterials;
            var previewMaterials = new Material[sourceMaterials.Length];
            for (var j = 0; j < sourceMaterials.Length; j++)
            {
                previewMaterials[j] = CreatePreviewMaterial(sourceMaterials[j]);
            }

            renderer.materials = previewMaterials;
        }
    }

    private static Material CreatePreviewMaterial(Material source)
    {
        if (source == null)
        {
            return GetFallbackMaterial();
        }

        var shaderName = source.shader != null ? source.shader.name : string.Empty;
        Material previewMaterial;
        if (source.shader == null
            || !source.shader.isSupported
            || shaderName == InternalErrorShaderName)
        {
            var replacementShader = GetBuiltInReplacementShader(source.name);
            previewMaterial = new Material(source)
            {
                name = source.name + "_BuiltInPreview"
            };
            if (replacementShader != null)
            {
                previewMaterial.shader = replacementShader;
                previewMaterial.renderQueue = source.renderQueue > 0
                    ? source.renderQueue
                    : 3000;
            }
        }
        else if (!string.IsNullOrEmpty(shaderName) && shaderName.Contains(UiFxShaderToken))
        {
            EnsureWorldFxShader();
            if (sWorldFxShader == null)
            {
                previewMaterial = new Material(source) { name = source.name + "_Preview" };
            }
            else
            {
                previewMaterial = new Material(sWorldFxShader)
                {
                    name = source.name + "_WorldPreview",
                    renderQueue = source.renderQueue > 0 ? source.renderQueue : 3000
                };
                previewMaterial.CopyPropertiesFromMaterial(source);
                previewMaterial.shaderKeywords = source.shaderKeywords;
            }
        }
        else
        {
            previewMaterial = new Material(source) { name = source.name + "_Preview" };
        }

        ApplyDepthPreviewFix(previewMaterial);
        return previewMaterial;
    }

    private static Shader GetBuiltInReplacementShader(string materialName)
    {
        if (!string.IsNullOrEmpty(materialName)
            && materialName.Contains("Trail"))
        {
            if (sBuiltInPacketShader == null)
            {
                sBuiltInPacketShader = Shader.Find(BuiltInPacketShaderName);
            }
            return sBuiltInPacketShader;
        }

        if (sBuiltInClipShader == null)
        {
            sBuiltInClipShader = Shader.Find(BuiltInClipShaderName);
        }
        return sBuiltInClipShader;
    }

    private static void ApplyDepthPreviewFix(Material material)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_ZTestMode"))
        {
            material.SetFloat("_ZTestMode", (float)CompareFunction.Always);
        }

        if (material.HasProperty("_ZWriteMode"))
        {
            material.SetFloat("_ZWriteMode", 0f);
        }

        if (material.HasProperty("_StencilComp"))
        {
            material.SetFloat("_StencilComp", (float)CompareFunction.Always);
        }
    }

    private static Material GetFallbackMaterial()
    {
        if (sFallbackMaterial != null)
        {
            return sFallbackMaterial;
        }

        EnsureWorldFxShader();
        var shader = sWorldFxShader
            ?? Shader.Find("URP/Effect/URP_UI_FX_Common")
            ?? Shader.Find("Particles/Standard Unlit");
        if (shader == null)
        {
            return null;
        }

        sFallbackMaterial = new Material(shader) { name = "CardFxFallback" };
        if (sFallbackMaterial.HasProperty("_MainColor"))
        {
            sFallbackMaterial.SetColor("_MainColor", Color.white);
        }

        ApplyDepthPreviewFix(sFallbackMaterial);
        return sFallbackMaterial;
    }

    private static void EnsureWorldFxShader()
    {
        if (sWorldFxShader != null)
        {
            return;
        }

        sWorldFxShader = Shader.Find(WorldFxShaderName);
        if (sWorldFxShader == null)
        {
            Debug.LogWarning($"CardFxRuntimeUtility: shader not found: {WorldFxShaderName}");
        }
    }
}
