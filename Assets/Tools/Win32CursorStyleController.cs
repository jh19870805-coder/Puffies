using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 用途：在 Win32 平台按背景范围切换自定义鼠标与系统鼠标。返回：无。
/// </summary>
public class Win32CursorStyleController : MonoBehaviour
{
    private const string BootstrapObjectName = "Win32CursorStyleControllerBootstrap";
    private const string CursorCanvasObjectName = "Win32CursorCanvas";
    private const string CursorImageObjectName = "Win32CursorImage";
    private const string MainBackgroundObjectName = "MainBackground";
    private const string GameBoardObjectName = "GameBoard";
    private const string CursorSpritePathPrimary = "Textures/BasicUI/ImgHand.png";
    private const string CursorSpritePathFallback = "Textures/BasicUi/ImgHand.png";
    private const float CursorPixelsPerUnit = 100f;
    private const float CursorVisualSize = 56f;
    private const float BoundsScreenPaddingPixels = 6f;
    private static bool sBootstrapped;

    private Camera _mainCamera;
    private Canvas _cursorCanvas;
    private RectTransform _cursorRect;
    private Image _cursorImage;
    private SpriteRenderer _targetBoundsRenderer;
    private string _cachedSceneName;
    private Vector2 _lastCursorAnchoredPosition;

    /// <summary>
    /// 用途：运行后自动挂载 Win32 鼠标样式控制器。返回：无。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (sBootstrapped || Application.isEditor)
        {
            return;
        }

        var host = new GameObject(BootstrapObjectName);
        DontDestroyOnLoad(host);
        host.AddComponent<Win32CursorStyleController>();
        sBootstrapped = true;
    }

    private void Start()
    {
        if (!ShouldRun())
        {
            enabled = false;
            return;
        }

        EnsureCursorVisual();
        RefreshSceneReferences(force: true);
        _lastCursorAnchoredPosition = Input.mousePosition;
        ApplyCursorPosition(_lastCursorAnchoredPosition);
    }

    private void Update()
    {
        if (!ShouldRun() || _cursorRect == null)
        {
            return;
        }

        RefreshSceneReferences();
        var currentMousePosition = (Vector2)Input.mousePosition;
        if (IsMouseInsideTargetBounds(currentMousePosition))
        {
            _lastCursorAnchoredPosition = ScreenToCanvasPosition(currentMousePosition);
            ApplyCursorPosition(_lastCursorAnchoredPosition);
            Cursor.visible = false;
            return;
        }

        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        Cursor.visible = true;
    }

    /// <summary>
    /// 用途：判断当前环境是否需要启用 Win32 鼠标样式控制。返回：是否启用。
    /// </summary>
    private static bool ShouldRun()
    {
        return Application.platform == RuntimePlatform.WindowsPlayer;
    }

    /// <summary>
    /// 用途：按场景状态刷新主相机与目标背景渲染器引用。返回：无。
    /// </summary>
    private void RefreshSceneReferences(bool force = false)
    {
        var activeSceneName = SceneManager.GetActiveScene().name;
        if (!force && activeSceneName == _cachedSceneName && _targetBoundsRenderer != null)
        {
            return;
        }

        _cachedSceneName = activeSceneName;
        _mainCamera = Camera.main;
        _targetBoundsRenderer = ResolveTargetBoundsRenderer();
    }

    /// <summary>
    /// 用途：创建用于自定义鼠标显示的 Overlay Canvas 与 Image。返回：无。
    /// </summary>
    private void EnsureCursorVisual()
    {
        if (_cursorCanvas != null && _cursorRect != null && _cursorImage != null)
        {
            return;
        }

        var canvasObject = new GameObject(CursorCanvasObjectName);
        DontDestroyOnLoad(canvasObject);
        _cursorCanvas = canvasObject.AddComponent<Canvas>();
        _cursorCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _cursorCanvas.sortingOrder = short.MaxValue;

        canvasObject.AddComponent<GraphicRaycaster>();
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

        var imageObject = new GameObject(CursorImageObjectName);
        imageObject.transform.SetParent(canvasObject.transform, false);
        _cursorRect = imageObject.AddComponent<RectTransform>();
        _cursorRect.anchorMin = Vector2.zero;
        _cursorRect.anchorMax = Vector2.zero;
        _cursorRect.pivot = new Vector2(0.5f, 0.5f);
        _cursorRect.sizeDelta = new Vector2(CursorVisualSize, CursorVisualSize);

        _cursorImage = imageObject.AddComponent<Image>();
        _cursorImage.raycastTarget = false;
        _cursorImage.sprite = LoadCursorSprite();
        _cursorImage.SetNativeSize();
        var size = _cursorRect.sizeDelta;
        if (size.x > 0f && size.y > 0f)
        {
            var scale = CursorVisualSize / Mathf.Max(size.x, size.y);
            _cursorRect.sizeDelta = new Vector2(size.x * scale, size.y * scale);
        }
    }

    /// <summary>
    /// 用途：根据预设路径加载自定义鼠标精灵。返回：精灵。
    /// </summary>
    private static Sprite LoadCursorSprite()
    {
        var sprite = GameCommonUtility.LoadSpriteByPath(CursorSpritePathPrimary, CursorPixelsPerUnit);
        if (sprite != null)
        {
            return sprite;
        }

        return GameCommonUtility.LoadSpriteByPath(CursorSpritePathFallback, CursorPixelsPerUnit);
    }

    /// <summary>
    /// 用途：判定鼠标是否位于背景图世界范围内。返回：是否在范围内。
    /// </summary>
    private bool IsMouseInsideTargetBounds(Vector2 mousePosition)
    {
        if (_targetBoundsRenderer == null || _mainCamera == null)
        {
            return false;
        }

        var screenRect = BuildRendererScreenRect(_targetBoundsRenderer, _mainCamera);
        return screenRect.Contains(mousePosition);
    }

    /// <summary>
    /// 用途：把屏幕坐标转换为画布锚点坐标。返回：锚点坐标。
    /// </summary>
    private Vector2 ScreenToCanvasPosition(Vector2 screenPosition)
    {
        return screenPosition;
    }

    /// <summary>
    /// 用途：应用自定义鼠标图标位置。返回：无。
    /// </summary>
    private void ApplyCursorPosition(Vector2 anchoredPosition)
    {
        if (_cursorRect == null)
        {
            return;
        }

        _cursorRect.anchoredPosition = anchoredPosition;
    }

    /// <summary>
    /// 用途：将渲染器世界包围盒转换为屏幕矩形命中区域。返回：屏幕矩形。
    /// </summary>
    private static Rect BuildRendererScreenRect(SpriteRenderer renderer, Camera camera)
    {
        var bounds = renderer.bounds;
        var min = camera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.center.z));
        var max = camera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.center.z));
        var left = Mathf.Min(min.x, max.x) - BoundsScreenPaddingPixels;
        var right = Mathf.Max(min.x, max.x) + BoundsScreenPaddingPixels;
        var bottom = Mathf.Min(min.y, max.y) - BoundsScreenPaddingPixels;
        var top = Mathf.Max(min.y, max.y) + BoundsScreenPaddingPixels;
        return Rect.MinMaxRect(left, bottom, right, top);
    }

    /// <summary>
    /// 用途：解析当前场景可用的背景图渲染器。返回：背景渲染器。
    /// </summary>
    private static SpriteRenderer ResolveTargetBoundsRenderer()
    {
        var background = GameObject.Find(MainBackgroundObjectName);
        if (background != null)
        {
            var renderer = background.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                return renderer;
            }
        }

        var gameBoard = GameObject.Find(GameBoardObjectName);
        if (gameBoard != null)
        {
            return gameBoard.GetComponent<SpriteRenderer>();
        }

        return null;
    }
}
