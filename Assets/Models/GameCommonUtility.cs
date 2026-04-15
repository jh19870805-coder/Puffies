using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameCommonUtility
{
    /// <summary>
    /// 用途：为指定场景组件注册一次场景加载引导逻辑，并在当前活动场景立即尝试创建组件。返回：无。
    /// </summary>
    /// <typeparam name="T">参数：需要自动挂载的组件类型。</typeparam>
    /// <param name="hasHookedSceneLoaded">参数：是否已注册过 sceneLoaded 事件的标记位。</param>
    /// <param name="sceneName">参数：目标场景名。</param>
    /// <param name="bootstrapObjectName">参数：自动创建对象时使用的对象名称。</param>
    public static void BootstrapSceneComponent<T>(
        ref bool hasHookedSceneLoaded,
        string sceneName,
        string bootstrapObjectName) where T : Component
    {
        if (!hasHookedSceneLoaded)
        {
            SceneManager.sceneLoaded += (_, _) => TryEnsureSceneComponent<T>(SceneManager.GetActiveScene(), sceneName, bootstrapObjectName);
            hasHookedSceneLoaded = true;
        }

        TryEnsureSceneComponent<T>(SceneManager.GetActiveScene(), sceneName, bootstrapObjectName);
    }

    /// <summary>
    /// 用途：当场景匹配目标场景名时，确保场景中存在指定类型组件实例。返回：无。
    /// </summary>
    /// <typeparam name="T">参数：需要确保存在的组件类型。</typeparam>
    /// <param name="scene">参数：待检查的场景对象。</param>
    /// <param name="sceneName">参数：目标场景名。</param>
    /// <param name="bootstrapObjectName">参数：自动创建对象时使用的对象名称。</param>
    public static void TryEnsureSceneComponent<T>(
        Scene scene,
        string sceneName,
        string bootstrapObjectName) where T : Component
    {
        if (!IsSceneMatch(scene, sceneName))
        {
            return;
        }

        if (UnityEngine.Object.FindObjectOfType<T>() != null)
        {
            return;
        }

        var bootstrapObject = new GameObject(bootstrapObjectName);
        bootstrapObject.AddComponent<T>();
    }

    /// <summary>
    /// 用途：判断场景对象是否匹配指定场景名（按场景名与场景路径双重判断）。返回：是否匹配。
    /// </summary>
    /// <param name="scene">参数：待判断的场景对象。</param>
    /// <param name="sceneName">参数：目标场景名。</param>
    /// <returns>返回：true 表示匹配，false 表示不匹配。</returns>
    public static bool IsSceneMatch(Scene scene, string sceneName)
    {
        if (scene.name.Equals(sceneName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var normalizedPath = (scene.path ?? string.Empty).Replace("\\", "/");
        return normalizedPath.EndsWith($"/{sceneName}.unity", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 用途：将相机设置为正交相机并按参考高度与像素单位计算正交尺寸。返回：无。
    /// </summary>
    /// <param name="camera">参数：需要配置的相机对象。</param>
    /// <param name="referenceHeight">参数：参考高度像素值。</param>
    /// <param name="pixelsPerUnit">参数：每单位像素数。</param>
    public static void SetupOrthographicCamera(Camera camera, float referenceHeight, float pixelsPerUnit)
    {
        if (camera == null)
        {
            return;
        }

        camera.orthographic = true;
        camera.orthographicSize = referenceHeight / (2f * pixelsPerUnit);
    }

    /// <summary>
    /// 用途：根据一组渲染器的包围盒自动调整正交相机，确保页面内容完整可见。返回：无。
    /// </summary>
    /// <param name="camera">参数：需要调整的正交相机。</param>
    /// <param name="padding">参数：在内容边界外额外保留的世界单位边距。</param>
    /// <param name="renderers">参数：需要纳入可视范围计算的渲染器集合。</param>
    public static void FitOrthographicCameraToRenderers(Camera camera, float padding, params Renderer[] renderers)
    {
        if (camera == null || !camera.orthographic || renderers == null || renderers.Length == 0)
        {
            return;
        }

        var hasBounds = false;
        var combinedBounds = new Bounds(Vector3.zero, Vector3.zero);
        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds = renderer.bounds;
                hasBounds = true;
                continue;
            }

            combinedBounds.Encapsulate(renderer.bounds);
        }

        if (!hasBounds)
        {
            return;
        }

        var targetPosition = camera.transform.position;
        targetPosition.x = combinedBounds.center.x;
        targetPosition.y = combinedBounds.center.y;
        camera.transform.position = targetPosition;

        var targetHalfHeight = combinedBounds.extents.y + padding;
        var targetHalfWidth = combinedBounds.extents.x + padding;
        if (camera.aspect > 0f)
        {
            targetHalfHeight = Mathf.Max(targetHalfHeight, targetHalfWidth / camera.aspect);
        }

        camera.orthographicSize = Mathf.Max(targetHalfHeight, 0.01f);
    }

    /// <summary>
    /// 用途：将资源路径统一转换为磁盘绝对路径。返回：可用于文件读取的绝对路径。
    /// </summary>
    /// <param name="resourcePath">参数：资源路径，支持绝对路径或相对 Assets 路径。</param>
    /// <returns>返回：磁盘绝对路径。</returns>
    public static string ToDiskPath(string resourcePath)
    {
        var normalizedPath = (resourcePath ?? string.Empty).Replace("\\", "/");
        if (Path.IsPathRooted(normalizedPath))
        {
            return normalizedPath;
        }

        if (normalizedPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            normalizedPath = normalizedPath.Substring("Assets/".Length);
        }

        return Path.Combine(Application.dataPath, normalizedPath);
    }
}
