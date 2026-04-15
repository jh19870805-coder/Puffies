using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class MainScene : MonoBehaviour
{
    private const float ReferenceHeight = 1080f;
    private const float PixelsPerUnit = 100f;
    private const string BootstrapObjectName = "MainSceneBootstrap";
    private const string MainBackgroundObjectName = "MainBackground";
    private static readonly string MainBackgroundPath = $"{GameDefine.TexturesRoot}/{GameDefine.MainBackgroundFileName}";
    private static bool sHookedSceneLoaded;

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

        GameManager.CreateInstance();

        var targetCamera = Camera.main;
        if (targetCamera != null)
        {
            SetupMainCamera(targetCamera);
        }

        CreateCenteredBackground(targetCamera);
    }

    /// <summary>
    /// 用途：将指定相机设置为正交投影并按参考分辨率计算正交尺寸。返回：无。
    /// </summary>
    /// <param name="camera">参数：需要配置的相机对象。</param>
    private static void SetupMainCamera(Camera camera)
    {
        GameCommonUtility.SetupOrthographicCamera(camera, ReferenceHeight, PixelsPerUnit);
    }

    /// <summary>
    /// 用途：在主场景中创建并居中显示背景对象，避免重复创建。返回：无。
    /// </summary>
    /// <param name="targetCamera">参数：用于适配背景显示比例的目标相机。</param>
    private void CreateCenteredBackground(Camera targetCamera)
    {
        if (GameObject.Find(MainBackgroundObjectName) != null)
        {
            return;
        }

        var sprite = CreateSpriteByPath(MainBackgroundPath);
        if (sprite == null)
        {
            Debug.LogWarning($"Failed to create background sprite from {MainBackgroundPath}.");
            return;
        }

        var backgroundObject = new GameObject(MainBackgroundObjectName);
        var spriteRenderer = backgroundObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = -100;
        backgroundObject.transform.position = Vector3.zero;
        FitSpriteToCamera(spriteRenderer, targetCamera);
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

    /// <summary>
    /// 用途：根据图片资源路径读取文件并创建 Sprite 资源对象。返回：创建后的 Sprite，失败返回 null。
    /// </summary>
    /// <param name="imageResourcePath">参数：图片资源路径，支持绝对路径或相对 Assets 的路径。</param>
    /// <returns>返回：成功时为有效的 Sprite，失败时为 null。</returns>
    public Sprite CreateSpriteByPath(string imageResourcePath)
    {
        if (string.IsNullOrWhiteSpace(imageResourcePath))
        {
            Debug.LogWarning("CreateSpriteByPath failed: imageResourcePath is empty.");
            return null;
        }

        var imagePathOnDisk = GameCommonUtility.ToDiskPath(imageResourcePath);

        if (!File.Exists(imagePathOnDisk))
        {
            Debug.LogWarning($"CreateSpriteByPath failed: file not found: {imagePathOnDisk}");
            return null;
        }

        var imageBytes = File.ReadAllBytes(imagePathOnDisk);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(imageBytes))
        {
            Debug.LogWarning($"CreateSpriteByPath failed: invalid image file: {imagePathOnDisk}");
            return null;
        }

        var imageSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            PixelsPerUnit);
        imageSprite.name = Path.GetFileNameWithoutExtension(imagePathOnDisk);
        return imageSprite;
    }
}
