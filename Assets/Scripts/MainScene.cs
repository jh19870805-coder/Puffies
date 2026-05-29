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
    private const float MaxTapDistancePixels = 18f;
    private const float PackageScaleRatio = 0.3f;
    private const float PackageClickScaleRatio = 1.15f;
    private const float PackageClickAnimDuration = 0.12f;
    private const int PackagesPerRow = 5;
    private const int RowsPerPage = 3;
    private const int MainPackageBagId = GameDefine.DefaultBagId;
    private const string BootstrapObjectName = "MainSceneBootstrap";
    private const string MainPackageObjectPrefix = "MainPackage";
    private static bool sHookedSceneLoaded;
    private readonly List<PackageEntry> mPackageEntries = new List<PackageEntry>();
    private SpriteRenderer mTapCandidateRenderer;
    private int mTapCandidateBagId = GameDefine.InvalidId;
    private Vector2 mTapStartScreenPosition;
    private bool mTapCandidateMoved;
    private bool mIsPlayingAnimation;
    private bool mHasSwitchedToGameScene;
    private Coroutine mPlayAnimationCoroutine;

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
    /// 用途：主场景组件启动入口，完成管理器初始化、主相机设置与卡包创建。返回：无。
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

        CreatePackageSprites(targetCamera);
    }

    /// <summary>
    /// 用途：主场景每帧轮询输入，点击卡包后直接播放对应动画。返回：无。
    /// </summary>
    private void Update()
    {
        if (mHasSwitchedToGameScene)
        {
            return;
        }

        GameCommonUtility.ProcessPointerInput(
            TryBeginTap,
            TrackPointerMove,
            TryCompleteTap);
    }

    /// <summary>
    /// 用途：扫描并创建卡包精灵，按每页 5x3 的网格从左上角开始自动平均分布。返回：无。
    /// </summary>
    private void CreatePackageSprites(Camera targetCamera)
    {
        mPackageEntries.Clear();
        var layoutBounds = ResolveLayoutBounds(targetCamera);
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
            .Where(GameCommonUtility.IsSupportedImageFile)
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
    /// 用途：解析卡包布局容器范围，使用主相机视口作为布局区域。返回：布局边界。
    /// </summary>
    private static Bounds ResolveLayoutBounds(Camera targetCamera)
    {
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
    /// 用途：记录点击开始点，仅在卡包命中时进入点击候选。返回：无。
    /// </summary>
    /// <param name="screenPosition">参数：屏幕坐标输入位置。</param>
    private void TryBeginTap(Vector2 screenPosition)
    {
        if (mIsPlayingAnimation)
        {
            return;
        }

        var worldPosition = GameCommonUtility.ScreenToWorld(screenPosition);
        if (TryGetPackageAtPoint(worldPosition, out var packageToPlay))
        {
            mTapCandidateRenderer = packageToPlay.Renderer;
            mTapCandidateBagId = packageToPlay.BagId;
            mTapStartScreenPosition = screenPosition;
            mTapCandidateMoved = false;
        }
        else
        {
            mTapCandidateRenderer = null;
            mTapCandidateBagId = GameDefine.InvalidId;
            mTapCandidateMoved = false;
        }
    }

    /// <summary>
    /// 用途：记录按压后的移动距离，用于区分点击与滑动。返回：无。
    /// </summary>
    private void TrackPointerMove(Vector2 screenPosition)
    {
        if (mTapCandidateRenderer == null || mTapCandidateMoved)
        {
            return;
        }

        if (Vector2.Distance(mTapStartScreenPosition, screenPosition) > MaxTapDistancePixels)
        {
            mTapCandidateMoved = true;
        }
    }

    /// <summary>
    /// 用途：在抬起阶段确认点击命中后，直接播放对应卡包动画。返回：无。
    /// </summary>
    /// <param name="screenPosition">参数：屏幕坐标输入位置。</param>
    private void TryCompleteTap(Vector2 screenPosition)
    {
        if (mIsPlayingAnimation)
        {
            ClearTapCandidate();
            return;
        }

        if (mTapCandidateRenderer == null || mTapCandidateBagId <= 0)
        {
            ClearTapCandidate();
            return;
        }

        var worldPosition = GameCommonUtility.ScreenToWorld(screenPosition);
        var isTapConfirmed = !mTapCandidateMoved
            && mTapCandidateRenderer.sprite != null
            && mTapCandidateRenderer.bounds.Contains(worldPosition);
        if (isTapConfirmed)
        {
            var bagId = mTapCandidateBagId;
            var renderer = mTapCandidateRenderer;
            ClearTapCandidate();

            if (mPlayAnimationCoroutine != null)
            {
                StopCoroutine(mPlayAnimationCoroutine);
            }

            mPlayAnimationCoroutine = StartCoroutine(PlayPackageInteraction(bagId, renderer));
            return;
        }

        ClearTapCandidate();
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

    private void ClearTapCandidate()
    {
        mTapCandidateRenderer = null;
        mTapCandidateBagId = GameDefine.InvalidId;
        mTapCandidateMoved = false;
    }

    private IEnumerator PlayPackageClickFallback(SpriteRenderer renderer)
    {
        if (renderer == null)
        {
            yield break;
        }

        var originalScale = renderer.transform.localScale;
        var targetScale = originalScale * PackageClickScaleRatio;
        var elapsed = 0f;
        while (elapsed < PackageClickAnimDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / PackageClickAnimDuration);
            renderer.transform.localScale = Vector3.LerpUnclamped(originalScale, targetScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < PackageClickAnimDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / PackageClickAnimDuration);
            renderer.transform.localScale = Vector3.LerpUnclamped(targetScale, originalScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        renderer.transform.localScale = originalScale;
    }

    private IEnumerator PlayPackageInteraction(int bagId, SpriteRenderer renderer)
    {
        mIsPlayingAnimation = true;
        var resolvedBagId = bagId > 0 ? bagId : MainPackageBagId;
        var animationFileName = $"mesh_ani_cardPack_{resolvedBagId:D3}.FBX";
        var anchor = renderer != null ? renderer.transform : null;
        var hasPlayed = GameAnimationUtility.PlayCardPackAnimation(animationFileName, anchor);
        if (hasPlayed)
        {
            var duration = GameAnimationUtility.GetCardPackPlayDuration(animationFileName, anchor);
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }
        }
        else if (renderer != null)
        {
            yield return PlayPackageClickFallback(renderer);
        }

        mIsPlayingAnimation = false;
        mPlayAnimationCoroutine = null;
        mHasSwitchedToGameScene = true;
        GameManager.EnterGameScene(resolvedBagId);
    }

}
