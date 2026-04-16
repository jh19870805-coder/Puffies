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

        var streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, normalizedPath);
        if (File.Exists(streamingAssetsPath) || Directory.Exists(streamingAssetsPath))
        {
            return streamingAssetsPath;
        }

        // Editor 下兼容直接读取 Assets 目录，避免本地调试依赖构建前同步。
        return Path.Combine(Application.dataPath, normalizedPath);
    }

    /// <summary>
    /// 用途：将屏幕坐标转换为世界坐标。返回：世界坐标。
    /// </summary>
    /// <param name="screenPosition">参数：屏幕坐标。</param>
    /// <param name="camera">参数：用于转换的相机，传 null 时自动使用主相机。</param>
    /// <returns>返回：转换后的世界坐标，若无可用相机则返回零向量。</returns>
    public static Vector3 ScreenToWorld(Vector2 screenPosition, Camera camera = null)
    {
        var targetCamera = camera != null ? camera : Camera.main;
        if (targetCamera == null)
        {
            return Vector3.zero;
        }

        var world = targetCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -targetCamera.transform.position.z));
        world.z = 0f;
        return world;
    }

    /// <summary>
    /// 用途：根据资源路径读取图片并构建 Sprite。返回：Sprite 对象。
    /// </summary>
    /// <param name="imageResourcePath">参数：图片资源路径，支持绝对路径或相对 Assets 路径。</param>
    /// <param name="pixelsPerUnit">参数：构建 Sprite 时使用的 PPU。</param>
    /// <returns>返回：成功时为 Sprite，失败返回 null。</returns>
    public static Sprite LoadSpriteByPath(string imageResourcePath, float pixelsPerUnit)
    {
        if (string.IsNullOrWhiteSpace(imageResourcePath))
        {
            Debug.LogWarning("LoadSpriteByPath failed: imageResourcePath is empty.");
            return null;
        }

        var imagePathOnDisk = ToDiskPath(imageResourcePath);
        if (!File.Exists(imagePathOnDisk))
        {
            Debug.LogWarning($"LoadSpriteByPath failed: file not found: {imagePathOnDisk}");
            return null;
        }

        var imageBytes = File.ReadAllBytes(imagePathOnDisk);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(imageBytes))
        {
            Debug.LogWarning($"LoadSpriteByPath failed: invalid image file: {imagePathOnDisk}");
            return null;
        }

        var imageSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit);
        imageSprite.name = Path.GetFileNameWithoutExtension(imagePathOnDisk);
        return imageSprite;
    }

    /// <summary>
    /// 用途：按资源路径创建精灵对象并返回渲染器，可选择复用同名对象。返回：精灵渲染器。
    /// </summary>
    /// <param name="objectName">参数：场景对象名。</param>
    /// <param name="spritePath">参数：精灵资源路径。</param>
    /// <param name="sortingOrder">参数：渲染顺序。</param>
    /// <param name="pixelsPerUnit">参数：构建 Sprite 时使用的 PPU。</param>
    /// <param name="parent">参数：父节点，传 null 表示无父节点。</param>
    /// <param name="forceCreate">参数：是否强制新建同名对象而不复用。</param>
    /// <returns>返回：创建或已存在的 SpriteRenderer，失败返回 null。</returns>
    public static SpriteRenderer CreateSpriteRendererObject(
        string objectName,
        string spritePath,
        int sortingOrder,
        float pixelsPerUnit,
        Transform parent = null,
        bool forceCreate = false)
    {
        if (!forceCreate)
        {
            var existing = GameObject.Find(objectName);
            if (existing != null)
            {
                return existing.GetComponent<SpriteRenderer>();
            }
        }

        var sprite = LoadSpriteByPath(spritePath, pixelsPerUnit);
        if (sprite == null)
        {
            return null;
        }

        var go = new GameObject(objectName);
        if (parent != null)
        {
            go.transform.SetParent(parent, worldPositionStays: true);
        }

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    /// <summary>
    /// 用途：统一分发鼠标与触屏输入事件，按阶段回调 Begin/Move/End。返回：无。
    /// </summary>
    /// <param name="onBegin">参数：输入开始时的回调。</param>
    /// <param name="onMove">参数：输入移动或按住时的回调。</param>
    /// <param name="onEnd">参数：输入结束时的回调。</param>
    public static void ProcessPointerInput(
        Action<Vector2> onBegin,
        Action<Vector2> onMove,
        Action<Vector2> onEnd)
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                onBegin?.Invoke(touch.position);
                return;
            }

            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                onMove?.Invoke(touch.position);
                return;
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                onEnd?.Invoke(touch.position);
            }

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            onBegin?.Invoke(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            onMove?.Invoke(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            onEnd?.Invoke(Input.mousePosition);
        }
    }

    /// <summary>
    /// 用途：判断文件路径是否为支持的图片扩展名。返回：是否支持。
    /// </summary>
    /// <param name="filePath">参数：待判断的文件路径。</param>
    /// <returns>返回：true 表示为支持的图片格式。</returns>
    public static bool IsSupportedImageFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return extension == GameDefine.ImageExtPng
            || extension == GameDefine.ImageExtJpg
            || extension == GameDefine.ImageExtJpeg
            || extension == GameDefine.ImageExtWebp;
    }

    /// <summary>
    /// 用途：将精灵按相机可视范围等比缩放，保证完整显示。返回：无。
    /// </summary>
    /// <param name="spriteRenderer">参数：目标精灵渲染器。</param>
    /// <param name="camera">参数：用于计算可视区域的相机。</param>
    public static void FitSpriteToCamera(SpriteRenderer spriteRenderer, Camera camera)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null || camera == null)
        {
            return;
        }

        var spriteSize = spriteRenderer.sprite.bounds.size;
        var cameraWorldHeight = 2f * camera.orthographicSize;
        var cameraWorldWidth = cameraWorldHeight * camera.aspect;
        var scale = Mathf.Min(cameraWorldWidth / spriteSize.x, cameraWorldHeight / spriteSize.y);
        spriteRenderer.transform.localScale = new Vector3(scale, scale, 1f);
    }

    /// <summary>
    /// 用途：创建纯色占位精灵，常用于兜底显示。返回：纯色精灵。
    /// </summary>
    /// <param name="fillColor">参数：填充颜色。</param>
    /// <param name="pixelsPerUnit">参数：生成精灵使用的 PPU。</param>
    /// <param name="textureSize">参数：生成纹理边长像素。</param>
    /// <returns>返回：创建成功的纯色 Sprite。</returns>
    public static Sprite CreateSolidSprite(Color fillColor, float pixelsPerUnit, int textureSize = 4)
    {
        var size = Mathf.Max(2, textureSize);
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var colors = new Color[size * size];
        for (var i = 0; i < colors.Length; i++)
        {
            colors[i] = fillColor;
        }

        texture.SetPixels(colors);
        texture.Apply();
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit,
            0,
            SpriteMeshType.FullRect,
            new Vector4(1f, 1f, 1f, 1f));
    }

    /// <summary>
    /// 用途：从图片路径构建九宫格可拉伸精灵。返回：九宫格 Sprite。
    /// </summary>
    /// <param name="spritePath">参数：精灵资源路径。</param>
    /// <param name="pixelsPerUnit">参数：生成精灵使用的 PPU。</param>
    /// <param name="fallbackSprite">参数：读取失败时使用的兜底精灵。</param>
    /// <returns>返回：成功时返回九宫格 Sprite，失败返回 fallbackSprite。</returns>
    public static Sprite CreateSlicedSpriteByPath(string spritePath, float pixelsPerUnit, Sprite fallbackSprite = null)
    {
        var imagePathOnDisk = ToDiskPath(spritePath);
        if (!File.Exists(imagePathOnDisk))
        {
            return fallbackSprite;
        }

        var imageBytes = File.ReadAllBytes(imagePathOnDisk);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(imageBytes))
        {
            return fallbackSprite;
        }

        var borderSize = Mathf.Clamp(Mathf.RoundToInt(Mathf.Min(texture.width, texture.height) * 0.12f), 8, 64);
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit,
            0,
            SpriteMeshType.FullRect,
            new Vector4(borderSize, borderSize, borderSize, borderSize));
    }

    /// <summary>
    /// 用途：将棋盘相对像素坐标转换为世界坐标。返回：世界坐标。
    /// </summary>
    /// <param name="boardWorldCenter">参数：棋盘中心世界坐标。</param>
    /// <param name="boardTextureSize">参数：棋盘纹理尺寸（像素）。</param>
    /// <param name="relativePixelPosition">参数：棋盘左下原点下的相对像素坐标。</param>
    /// <param name="pixelsPerUnit">参数：每单位像素数。</param>
    /// <returns>返回：转换后的世界坐标。</returns>
    public static Vector3 ConvertBoardRelativeToWorldPosition(
        Vector3 boardWorldCenter,
        Vector2 boardTextureSize,
        Vector2 relativePixelPosition,
        float pixelsPerUnit)
    {
        var localX = (relativePixelPosition.x - boardTextureSize.x * 0.5f) / pixelsPerUnit;
        var localY = (relativePixelPosition.y - boardTextureSize.y * 0.5f) / pixelsPerUnit;
        return new Vector3(
            boardWorldCenter.x + localX,
            boardWorldCenter.y + localY,
            0f);
    }

    /// <summary>
    /// 用途：设置精灵渲染器透明度。返回：无。
    /// </summary>
    /// <param name="renderer">参数：目标渲染器。</param>
    /// <param name="alpha">参数：透明度（0~1）。</param>
    public static void SetRendererAlpha(SpriteRenderer renderer, float alpha)
    {
        if (renderer == null)
        {
            return;
        }

        var color = renderer.color;
        color.a = Mathf.Clamp01(alpha);
        renderer.color = color;
    }

    /// <summary>
    /// 用途：根据托盘高度限制计算贴片缩放。返回：等比缩放向量。
    /// </summary>
    /// <param name="pieceRenderer">参数：贴片渲染器。</param>
    /// <param name="trayBounds">参数：托盘范围。</param>
    /// <param name="maxHeightRatio">参数：托盘可用最大高度比例。</param>
    /// <returns>返回：贴片在托盘中的目标缩放。</returns>
    public static Vector3 CalculateTrayScale(SpriteRenderer pieceRenderer, Bounds trayBounds, float maxHeightRatio)
    {
        if (pieceRenderer == null || pieceRenderer.sprite == null)
        {
            return Vector3.one;
        }

        var spriteHeight = Mathf.Max(0.0001f, pieceRenderer.sprite.bounds.size.y);
        var maxHeight = Mathf.Max(0.0001f, trayBounds.size.y * maxHeightRatio);
        var scale = Mathf.Min(1f, maxHeight / spriteHeight);
        return new Vector3(scale, scale, 1f);
    }

    /// <summary>
    /// 用途：按指定缩放计算贴片世界宽度。返回：宽度值。
    /// </summary>
    /// <param name="pieceRenderer">参数：贴片渲染器。</param>
    /// <param name="scale">参数：缩放向量。</param>
    /// <returns>返回：贴片世界宽度。</returns>
    public static float GetPieceWidth(SpriteRenderer pieceRenderer, Vector3 scale)
    {
        if (pieceRenderer == null || pieceRenderer.sprite == null)
        {
            return 0.01f;
        }

        return Mathf.Max(0.01f, pieceRenderer.sprite.bounds.size.x * scale.x);
    }
}
