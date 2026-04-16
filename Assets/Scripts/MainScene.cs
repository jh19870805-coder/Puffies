using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    private const float ReferenceHeight = 1080f;
    private const float PixelsPerUnit = 100f;
    private const float MinSwipeDistance = 0.5f;
    private const float MaxTapDistancePixels = 18f;
    private const float MainPageCameraPadding = 0.2f;
    private const float PackageScaleRatio = 0.3f;
    private const float PackageFocusAnimationDuration = 0.35f;
    private const int PackagesPerRow = 5;
    private const int RowsPerPage = 3;
    private const int MainPackageBagId = GameDefine.DefaultBagId;
    private const string BootstrapObjectName = "MainSceneBootstrap";
    private const string MainBackgroundObjectName = "MainBackground";
    private const string MainPackageObjectPrefix = "MainPackage";
    private static readonly string MainBackgroundPath = $"{GameDefine.TexturesRoot}/{GameDefine.MainBackgroundFileName}";
    private static bool sHookedSceneLoaded;
    private readonly List<PackageEntry> mPackageEntries = new List<PackageEntry>();
    private Vector3 mSwipeStartWorldPosition;
    private int mSwipeBagId = GameDefine.InvalidId;
    private SpriteRenderer mSwipeTargetRenderer;
    private SpriteRenderer mTapCandidateRenderer;
    private int mTapCandidateBagId = GameDefine.InvalidId;
    private Vector2 mTapStartScreenPosition;
    private bool mTapCandidateMoved;
    private SpriteRenderer mFocusedPackageRenderer;
    private int mFocusedBagId = GameDefine.InvalidId;
    private Coroutine mFocusAnimationCoroutine;
    private bool mIsPackageFocused;
    private bool mIsFocusAnimating;
    private bool mIsSwipeTracking;
    private bool mHasSwitchedToGameScene;

    private struct PackageEntry
    {
        public int BagId;
        public SpriteRenderer Renderer;
    }

    /// <summary>
    /// 用途：在场景加载后自动挂接主场景启动逻辑，并尝试对当前活动场景执行引导初始化。返回：无。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameCommonUtility.BootstrapSceneComponent<MainScene>(
            ref sHookedSceneLoaded,
            GameDefine.SceneMain,
            BootstrapObjectName);
    }

    /// <summary>
    /// 用途：主场景组件启动入口，完成管理器初始化、主相机设置与背景创建。返回：无。
    /// </summary>
    private void Start()
    {
        if (!GameCommonUtility.IsSceneMatch(SceneManager.GetActiveScene(), GameDefine.SceneMain))
        {
            Destroy(gameObject);
            return;
        }

        GameManager.Initialize();

        var targetCamera = Camera.main;
        if (targetCamera != null)
        {
            GameCommonUtility.SetupOrthographicCamera(targetCamera, ReferenceHeight, PixelsPerUnit);
        }

        var backgroundRenderer = CreateCenteredBackground(targetCamera);
        CreatePackageSprites(backgroundRenderer, targetCamera);

        if (targetCamera != null)
        {
            GameCommonUtility.FitOrthographicCameraToRenderers(
                targetCamera,
                MainPageCameraPadding,
                backgroundRenderer);
        }
    }

    /// <summary>
    /// 用途：主场景每帧轮询输入，检测是否在包图精灵内完成从左到右滑动并触发场景切换。返回：无。
    /// </summary>
    private void Update()
    {
        if (mHasSwitchedToGameScene)
        {
            return;
        }

        GameCommonUtility.ProcessPointerInput(
            TryBeginSwipe,
            TrackPointerMove,
            TryCompleteSwipe);
    }

    /// <summary>
    /// 用途：在主场景中创建并居中显示背景对象，避免重复创建。返回：无。
    /// </summary>
    /// <param name="targetCamera">参数：用于适配背景显示比例的目标相机。</param>
    private SpriteRenderer CreateCenteredBackground(Camera targetCamera)
    {
        return CreateCenteredSpriteObject(
            MainBackgroundObjectName,
            MainBackgroundPath,
            -100,
            targetCamera,
            fitToCamera: true);
    }

    /// <summary>
    /// 用途：扫描并创建卡包精灵，按每页 5x3 的网格从左上角开始自动平均分布。返回：无。
    /// </summary>
    private void CreatePackageSprites(SpriteRenderer backgroundRenderer, Camera targetCamera)
    {
        mPackageEntries.Clear();
        var layoutBounds = ResolveLayoutBounds(backgroundRenderer, targetCamera);
        var packagePaths = LoadPackageSpritePaths();
        for (var i = 0; i < packagePaths.Count; i++)
        {
            var packagePath = packagePaths[i];
            var bagId = ParseBagId(packagePath);
            var objectName = $"{MainPackageObjectPrefix}{bagId:D3}";
            var spriteRenderer = GameCommonUtility.CreateSpriteRendererObject(
                objectName,
                packagePath,
                0,
                PixelsPerUnit);
            if (spriteRenderer == null)
            {
                continue;
            }

            spriteRenderer.transform.localScale = new Vector3(PackageScaleRatio, PackageScaleRatio, 1f);
            LayoutPackageSprite(spriteRenderer, i, layoutBounds);
            mPackageEntries.Add(new PackageEntry
            {
                BagId = bagId,
                Renderer = spriteRenderer
            });
        }
    }

    /// <summary>
    /// 用途：加载可用于主界面的卡包封面路径，按名称升序排序。返回：资源路径列表。
    /// </summary>
    private static List<string> LoadPackageSpritePaths()
    {
        var packageFolderRelativePath = $"{GameDefine.TexturesRoot}/{GameDefine.PackImagesFolder}";
        var packageFolderOnDisk = GameCommonUtility.ToDiskPath(packageFolderRelativePath);
        if (!Directory.Exists(packageFolderOnDisk))
        {
            return new List<string> { GameManager.GetBagPackagePath() };
        }

        var packagePaths = Directory
            .GetFiles(packageFolderOnDisk)
            .Where(IsSupportedImagePath)
            .OrderBy(Path.GetFileName)
            .Select(path => $"{packageFolderRelativePath}/{Path.GetFileName(path)}")
            .ToList();
        if (packagePaths.Count > 0)
        {
            return packagePaths;
        }

        return new List<string> { GameManager.GetBagPackagePath() };
    }

    /// <summary>
    /// 用途：解析卡包布局容器范围，优先使用背景图边界。返回：布局边界。
    /// </summary>
    private static Bounds ResolveLayoutBounds(SpriteRenderer backgroundRenderer, Camera targetCamera)
    {
        if (backgroundRenderer != null && backgroundRenderer.sprite != null)
        {
            return backgroundRenderer.bounds;
        }

        if (targetCamera != null)
        {
            var width = targetCamera.orthographicSize * 2f * targetCamera.aspect;
            var height = targetCamera.orthographicSize * 2f;
            return new Bounds(targetCamera.transform.position, new Vector3(width, height, 1f));
        }

        return new Bounds(Vector3.zero, new Vector3(10f, 10f, 1f));
    }

    /// <summary>
    /// 用途：将单个卡包放置到分页网格位置。返回：无。
    /// </summary>
    private static void LayoutPackageSprite(SpriteRenderer spriteRenderer, int packageIndex, Bounds layoutBounds)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        var spriteSize = spriteRenderer.sprite != null ? spriteRenderer.sprite.bounds.size : Vector3.one;
        var packageWidth = Mathf.Max(0.01f, spriteSize.x * spriteRenderer.transform.localScale.x);
        var packageHeight = Mathf.Max(0.01f, spriteSize.y * spriteRenderer.transform.localScale.y);

        var pageWidth = Mathf.Max(packageWidth, layoutBounds.size.x);
        var pageHeight = Mathf.Max(packageHeight, layoutBounds.size.y);
        var packagesPerPage = PackagesPerRow * RowsPerPage;
        var pageIndex = packageIndex / packagesPerPage;
        var indexInPage = packageIndex % packagesPerPage;
        var row = indexInPage / PackagesPerRow;
        var column = indexInPage % PackagesPerRow;

        var horizontalGap = Mathf.Max(0.02f, (pageWidth - packageWidth * PackagesPerRow) / (PackagesPerRow + 1));
        var verticalGap = Mathf.Max(0.02f, (pageHeight - packageHeight * RowsPerPage) / (RowsPerPage + 1));

        var pageLeft = layoutBounds.min.x + pageIndex * pageWidth;
        var pageTop = layoutBounds.max.y;

        var x = pageLeft + horizontalGap + packageWidth * 0.5f + column * (packageWidth + horizontalGap);
        var y = pageTop - verticalGap - packageHeight * 0.5f - row * (packageHeight + verticalGap);
        spriteRenderer.transform.position = new Vector3(x, y, 0f);
    }

    /// <summary>
    /// 用途：根据卡包封面路径解析包编号，失败时回退默认包编号。返回：包编号。
    /// </summary>
    private static int ParseBagId(string packagePath)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(packagePath);
        if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
        {
            return MainPackageBagId;
        }

        var prefixLength = GameDefine.PackageFilePrefix.Length;
        if (fileNameWithoutExtension.Length <= prefixLength
            || !fileNameWithoutExtension.StartsWith(GameDefine.PackageFilePrefix))
        {
            return MainPackageBagId;
        }

        var idText = fileNameWithoutExtension.Substring(prefixLength);
        return int.TryParse(idText, out var bagId) ? bagId : MainPackageBagId;
    }

    /// <summary>
    /// 用途：判断路径是否为支持的图片资源。返回：是否支持。
    /// </summary>
    private static bool IsSupportedImagePath(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return extension == GameDefine.ImageExtPng
            || extension == GameDefine.ImageExtJpg
            || extension == GameDefine.ImageExtJpeg
            || extension == GameDefine.ImageExtWebp;
    }

    /// <summary>
    /// 用途：根据对象名和资源路径创建精灵对象并居中放置，可选按相机适配缩放。返回：创建后的精灵渲染器。
    /// </summary>
    /// <param name="objectName">参数：要创建的场景对象名。</param>
    /// <param name="spritePath">参数：精灵资源路径。</param>
    /// <param name="sortingOrder">参数：渲染层级。</param>
    /// <param name="camera">参数：用于适配缩放的相机，未适配时可为空。</param>
    /// <param name="fitToCamera">参数：是否按相机视口适配精灵缩放。</param>
    /// <returns>返回：创建或已存在对象上的 SpriteRenderer，失败返回 null。</returns>
    private SpriteRenderer CreateCenteredSpriteObject(
        string objectName,
        string spritePath,
        int sortingOrder,
        Camera camera,
        bool fitToCamera)
    {
        var spriteRenderer = GameCommonUtility.CreateSpriteRendererObject(
            objectName,
            spritePath,
            sortingOrder,
            PixelsPerUnit);
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"Failed to create sprite from {spritePath}.");
            return null;
        }

        spriteRenderer.transform.position = Vector3.zero;

        if (fitToCamera)
        {
            FitSpriteToCamera(spriteRenderer, camera);
        }

        return spriteRenderer;
    }

    /// <summary>
    /// 用途：尝试开始一次滑动记录，只有触点落在包图精灵内部才会启动跟踪。返回：无。
    /// </summary>
    /// <param name="screenPosition">参数：屏幕坐标输入位置。</param>
    private void TryBeginSwipe(Vector2 screenPosition)
    {
        if (mIsFocusAnimating)
        {
            return;
        }

        var worldPosition = GameCommonUtility.ScreenToWorld(screenPosition);
        if (!mIsPackageFocused)
        {
            if (TryGetPackageAtPoint(worldPosition, out var packageToFocus))
            {
                mTapCandidateRenderer = packageToFocus.Renderer;
                mTapCandidateBagId = packageToFocus.BagId;
                mTapStartScreenPosition = screenPosition;
                mTapCandidateMoved = false;
            }
            else
            {
                mTapCandidateRenderer = null;
                mTapCandidateBagId = GameDefine.InvalidId;
                mTapCandidateMoved = false;
            }

            mIsSwipeTracking = false;
            mSwipeBagId = GameDefine.InvalidId;
            mSwipeTargetRenderer = null;
            return;
        }

        if (mFocusedPackageRenderer == null
            || mFocusedPackageRenderer.sprite == null
            || !mFocusedPackageRenderer.bounds.Contains(worldPosition))
        {
            mIsSwipeTracking = false;
            mSwipeBagId = GameDefine.InvalidId;
            mSwipeTargetRenderer = null;
            return;
        }

        mSwipeStartWorldPosition = worldPosition;
        mSwipeBagId = mFocusedBagId;
        mSwipeTargetRenderer = mFocusedPackageRenderer;
        mIsSwipeTracking = true;
    }

    /// <summary>
    /// 用途：记录按压后的移动距离，用于区分点击与滑动。返回：无。
    /// </summary>
    private void TrackPointerMove(Vector2 screenPosition)
    {
        if (mIsPackageFocused || mTapCandidateRenderer == null || mTapCandidateMoved)
        {
            return;
        }

        if (Vector2.Distance(mTapStartScreenPosition, screenPosition) > MaxTapDistancePixels)
        {
            mTapCandidateMoved = true;
        }
    }

    /// <summary>
    /// 用途：尝试完成一次滑动记录，满足左到右且位移足够时切换到游戏场景。返回：无。
    /// </summary>
    /// <param name="screenPosition">参数：屏幕坐标输入位置。</param>
    private void TryCompleteSwipe(Vector2 screenPosition)
    {
        if (!mIsPackageFocused)
        {
            TryCompleteTap(screenPosition);
            return;
        }

        if (!mIsSwipeTracking)
        {
            return;
        }

        mIsSwipeTracking = false;
        var worldPosition = GameCommonUtility.ScreenToWorld(screenPosition);
        if (mSwipeTargetRenderer == null
            || mSwipeTargetRenderer.sprite == null
            || !mSwipeTargetRenderer.bounds.Contains(worldPosition))
        {
            return;
        }

        var delta = worldPosition - mSwipeStartWorldPosition;
        if (delta.x < MinSwipeDistance)
        {
            return;
        }

        if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y))
        {
            return;
        }

        mHasSwitchedToGameScene = true;
        GameManager.EnterGameScene(mSwipeBagId > 0 ? mSwipeBagId : MainPackageBagId);
    }

    /// <summary>
    /// 用途：在抬起阶段确认点击命中后，触发卡包聚焦动画。返回：无。
    /// </summary>
    private void TryCompleteTap(Vector2 screenPosition)
    {
        if (mTapCandidateRenderer == null || mTapCandidateBagId <= 0 || mIsFocusAnimating)
        {
            mTapCandidateRenderer = null;
            mTapCandidateBagId = GameDefine.InvalidId;
            mTapCandidateMoved = false;
            return;
        }

        var worldPosition = GameCommonUtility.ScreenToWorld(screenPosition);
        var isTapConfirmed = !mTapCandidateMoved
            && mTapCandidateRenderer.sprite != null
            && mTapCandidateRenderer.bounds.Contains(worldPosition);
        if (isTapConfirmed)
        {
            FocusPackage(new PackageEntry
            {
                BagId = mTapCandidateBagId,
                Renderer = mTapCandidateRenderer
            });
        }

        mTapCandidateRenderer = null;
        mTapCandidateBagId = GameDefine.InvalidId;
        mTapCandidateMoved = false;
    }

    /// <summary>
    /// 用途：将选中的卡包缓慢移动到屏幕中心并放大到原始尺寸。返回：无。
    /// </summary>
    private void FocusPackage(PackageEntry packageEntry)
    {
        if (packageEntry.Renderer == null)
        {
            return;
        }

        if (mFocusAnimationCoroutine != null)
        {
            StopCoroutine(mFocusAnimationCoroutine);
            mFocusAnimationCoroutine = null;
        }

        mFocusedPackageRenderer = packageEntry.Renderer;
        mFocusedBagId = packageEntry.BagId;
        mIsPackageFocused = true;
        mFocusAnimationCoroutine = StartCoroutine(AnimateFocusPackage(packageEntry.Renderer));
    }

    /// <summary>
    /// 用途：播放卡包聚焦动画，结束后允许滑动进入游戏。返回：协程。
    /// </summary>
    private IEnumerator AnimateFocusPackage(SpriteRenderer renderer)
    {
        if (renderer == null)
        {
            yield break;
        }

        mIsFocusAnimating = true;
        renderer.sortingOrder = 10;
        var fromPosition = renderer.transform.position;
        var fromScale = renderer.transform.localScale;
        var camera = Camera.main;
        var targetPosition = camera != null
            ? new Vector3(camera.transform.position.x, camera.transform.position.y, fromPosition.z)
            : Vector3.zero;
        var targetScale = Vector3.one;

        var elapsed = 0f;
        while (elapsed < PackageFocusAnimationDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / PackageFocusAnimationDuration);
            var eased = Mathf.SmoothStep(0f, 1f, t);
            renderer.transform.position = Vector3.LerpUnclamped(fromPosition, targetPosition, eased);
            renderer.transform.localScale = Vector3.LerpUnclamped(fromScale, targetScale, eased);
            yield return null;
        }

        renderer.transform.position = targetPosition;
        renderer.transform.localScale = targetScale;
        mIsFocusAnimating = false;
        mFocusAnimationCoroutine = null;
    }

    /// <summary>
    /// 用途：查找指定点命中的卡包条目。返回：是否命中。
    /// </summary>
    private bool TryGetPackageAtPoint(Vector3 worldPosition, out PackageEntry packageEntry)
    {
        for (var i = 0; i < mPackageEntries.Count; i++)
        {
            var current = mPackageEntries[i];
            if (current.Renderer == null
                || current.Renderer.sprite == null
                || !current.Renderer.bounds.Contains(worldPosition))
            {
                continue;
            }

            packageEntry = current;
            return true;
        }

        packageEntry = default;
        return false;
    }

    /// <summary>
    /// 用途：将精灵渲染器按相机可视区域进行缩放适配，保证完整显示。返回：无。
    /// </summary>
    /// <param name="spriteRenderer">参数：需要调整缩放的精灵渲染器。</param>
    /// <param name="camera">参数：用于计算可视范围的相机。</param>
    private static void FitSpriteToCamera(SpriteRenderer spriteRenderer, Camera camera)
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

}
