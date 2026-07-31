using UnityEngine;

/// <summary>
/// 用途：运行时准备 CardFx 粒子预制体用于预览/播放。返回：按方法说明。
/// </summary>
public static class CardFxRuntimeUtility
{
    public static void PreparePreview(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        root.transform.localScale = Vector3.one * GameDefine.PixelsPerUnit;
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
        var renderers = root.GetComponentsInChildren<ParticleSystemRenderer>(true);
        for (var i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].sortingLayerID = sortingLayerId;
                renderers[i].sortingOrder = sortingOrder;
            }
        }
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

    public static void StopEmittingParticleSystems(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        var particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);
        for (var i = 0; i < particleSystems.Length; i++)
        {
            var particleSystem = particleSystems[i];
            if (particleSystem != null)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

}
