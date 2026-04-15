using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    private const float ReferenceHeight = 1080f;
    private const float PixelsPerUnit = 100f;
    private const float MinSwipeDistance = 0.5f;
    private const float MainPageCameraPadding = 0.2f;
    private const int MainPackageBagId = GameDefine.DefaultBagId;
    private const string BootstrapObjectName = "MainSceneBootstrap";
    private const string MainBackgroundObjectName = "MainBackground";
    private const string MainPackageObjectName = "MainPackage001";
    private static readonly string MainBackgroundPath = $"{GameDefine.TexturesRoot}/{GameDefine.MainBackgroundFileName}";
    private static bool sHookedSceneLoaded;
    private SpriteRenderer mMainPackageRenderer;
    private Vector3 mSwipeStartWorldPosition;
    private bool mIsSwipeTracking;
    private bool mHasSwitchedToGameScene;

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
        CreateCenteredPackageSprite();

        if (targetCamera != null)
        {
            GameCommonUtility.FitOrthographicCameraToRenderers(
                targetCamera,
                MainPageCameraPadding,
                backgroundRenderer,
                mMainPackageRenderer);
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
            onMove: null,
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
    /// 用途：在主场景中创建 Package001 精灵并居中放置，避免重复创建。返回：无。
    /// </summary>
    private void CreateCenteredPackageSprite()
    {
        var packagePath = GameManager.GetBagPackagePath();

        mMainPackageRenderer = CreateCenteredSpriteObject(
            MainPackageObjectName,
            packagePath,
            0,
            camera: null,
            fitToCamera: false);
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
        var worldPosition = GameCommonUtility.ScreenToWorld(screenPosition);
        if (!IsPointInsideMainPackage(worldPosition))
        {
            mIsSwipeTracking = false;
            return;
        }

        mSwipeStartWorldPosition = worldPosition;
        mIsSwipeTracking = true;
    }

    /// <summary>
    /// 用途：尝试完成一次滑动记录，满足左到右且位移足够时切换到游戏场景。返回：无。
    /// </summary>
    /// <param name="screenPosition">参数：屏幕坐标输入位置。</param>
    private void TryCompleteSwipe(Vector2 screenPosition)
    {
        if (!mIsSwipeTracking)
        {
            return;
        }

        mIsSwipeTracking = false;
        var worldPosition = GameCommonUtility.ScreenToWorld(screenPosition);
        if (!IsPointInsideMainPackage(worldPosition))
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
        GameManager.EnterGameScene(MainPackageBagId);
    }

    /// <summary>
    /// 用途：判断指定世界坐标是否位于主场景包图精灵可交互区域内。返回：是否在精灵内。
    /// </summary>
    /// <param name="worldPosition">参数：待检测的世界坐标。</param>
    /// <returns>返回：true 表示点位于包图精灵区域内，false 表示不在区域内。</returns>
    private bool IsPointInsideMainPackage(Vector3 worldPosition)
    {
        return mMainPackageRenderer != null
            && mMainPackageRenderer.sprite != null
            && mMainPackageRenderer.bounds.Contains(worldPosition);
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
