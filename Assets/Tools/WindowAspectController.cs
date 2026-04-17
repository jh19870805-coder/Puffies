using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// 用途：在 Windows 窗口模式下锁定窗口为固定宽高比，拖拽缩放时自动纠偏。返回：无。
/// </summary>
public class WindowAspectController : MonoBehaviour
{
    private const bool EnableAspectLock = false;
    private const string BootstrapObjectName = "WindowAspectControllerBootstrap";
    private const int AspectWidth = 16;
    private const int AspectHeight = 9;
    private const int MinWindowWidth = 360;
    private const int MaxWindowWidth = 1920;
    private const int MaxWindowHeight = 1080;
    private const float ResizeSettleDelaySeconds = 0.12f;
    private const float AspectTolerance = 0.01f;
    private const int GwlWndProc = -4;
    private const uint WmSizing = 0x0214;
    private const int WmszLeft = 1;
    private const int WmszRight = 2;
    private const int WmszTop = 3;
    private const int WmszTopLeft = 4;
    private const int WmszTopRight = 5;
    private const int WmszBottom = 6;
    private const int WmszBottomLeft = 7;
    private const int WmszBottomRight = 8;
    private static bool sInitialized;

    private int _lastWidth;
    private int _lastHeight;
    private int _lastAppliedWidth;
    private int _lastAppliedHeight;
    private int _pendingWidth;
    private int _pendingHeight;
    private float _lastResizeChangedTime;
    private bool _hasPendingResize;
    private bool _isApplyingResolution;
    private bool _isNativeHookInstalled;
    private IntPtr _windowHandle = IntPtr.Zero;
    private IntPtr _originalWndProc = IntPtr.Zero;
    private WndProcDelegate _wndProcDelegate;

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    /// <summary>
    /// 用途：应用启动后自动创建窗口比例控制器。返回：无。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (sInitialized || Application.isEditor)
        {
            return;
        }

        var host = new GameObject(BootstrapObjectName);
        DontDestroyOnLoad(host);
        host.AddComponent<WindowAspectController>();
        sInitialized = true;
    }

    /// <summary>
    /// 用途：初始化窗口尺寸记录并立即修正到目标比例。返回：无。
    /// </summary>
    private void Start()
    {
        if (!ShouldRun())
        {
            enabled = false;
            return;
        }

        _lastWidth = Screen.width;
        _lastHeight = Screen.height;
        _lastAppliedWidth = _lastWidth;
        _lastAppliedHeight = _lastHeight;
        TryInstallNativeSizingHook();
        ApplyByWidth(_lastWidth);
    }

    /// <summary>
    /// 用途：监听窗口尺寸变化并按 16:9 自动纠偏。返回：无。
    /// </summary>
    private void Update()
    {
        if (_isApplyingResolution || !ShouldRun())
        {
            return;
        }

        // Native hook installed: WM_SIZING already enforces ratio smoothly.
        if (_isNativeHookInstalled)
        {
            return;
        }

        var width = Screen.width;
        var height = Screen.height;
        if (width != _lastWidth || height != _lastHeight)
        {
            _pendingWidth = width;
            _pendingHeight = height;
            _lastResizeChangedTime = Time.unscaledTime;
            _hasPendingResize = true;
            _lastWidth = width;
            _lastHeight = height;
            return;
        }

        if (!_hasPendingResize || Time.unscaledTime - _lastResizeChangedTime < ResizeSettleDelaySeconds)
        {
            return;
        }

        var widthDelta = Mathf.Abs(_pendingWidth - _lastAppliedWidth);
        var heightDelta = Mathf.Abs(_pendingHeight - _lastAppliedHeight);
        if (widthDelta >= heightDelta)
        {
            ApplyByWidth(_pendingWidth);
        }
        else
        {
            ApplyByHeight(_pendingHeight);
        }
        _hasPendingResize = false;
    }

    /// <summary>
    /// 用途：判断当前运行环境是否需要启用窗口比例控制。返回：是否启用。
    /// </summary>
    private static bool ShouldRun()
    {
        if (!EnableAspectLock)
        {
            return false;
        }

        return Application.platform == RuntimePlatform.WindowsPlayer
            && Screen.fullScreenMode == FullScreenMode.Windowed;
    }

    /// <summary>
    /// 用途：根据指定宽度计算并应用 16:9 分辨率。返回：无。
    /// </summary>
    /// <param name="width">参数：目标窗口宽度。</param>
    private void ApplyByWidth(int width)
    {
        var clampedWidth = Mathf.Clamp(width, MinWindowWidth, MaxWindowWidth);
        var calculatedHeight = Mathf.RoundToInt(clampedWidth * AspectHeight / (float)AspectWidth);
        calculatedHeight = Mathf.Min(calculatedHeight, MaxWindowHeight);
        ApplyResolution(clampedWidth, calculatedHeight);
    }

    /// <summary>
    /// 用途：根据指定高度计算并应用 16:9 分辨率。返回：无。
    /// </summary>
    /// <param name="height">参数：目标窗口高度。</param>
    private void ApplyByHeight(int height)
    {
        var clampedHeight = Mathf.Clamp(height, Mathf.CeilToInt(MinWindowWidth * AspectHeight / (float)AspectWidth), MaxWindowHeight);
        var calculatedWidth = Mathf.RoundToInt(clampedHeight * AspectWidth / (float)AspectHeight);
        var clampedWidth = Mathf.Clamp(calculatedWidth, MinWindowWidth, MaxWindowWidth);
        var calculatedHeight = Mathf.RoundToInt(clampedWidth * AspectHeight / (float)AspectWidth);
        calculatedHeight = Mathf.Min(calculatedHeight, MaxWindowHeight);
        ApplyResolution(clampedWidth, calculatedHeight);
    }

    /// <summary>
    /// 用途：应用窗口分辨率并避免递归触发尺寸修正。返回：无。
    /// </summary>
    /// <param name="width">参数：窗口宽度。</param>
    /// <param name="height">参数：窗口高度。</param>
    private void ApplyResolution(int width, int height)
    {
        var currentAspect = Screen.height > 0 ? Screen.width / (float)Screen.height : 0f;
        var targetAspect = AspectWidth / (float)AspectHeight;
        var aspectDiff = Mathf.Abs(currentAspect - targetAspect);
        if (Screen.width == width && Screen.height == height)
        {
            _lastAppliedWidth = width;
            _lastAppliedHeight = height;
            return;
        }

        if (aspectDiff <= AspectTolerance
            && Mathf.Abs(Screen.width - width) <= 1
            && Mathf.Abs(Screen.height - height) <= 1)
        {
            _lastAppliedWidth = Screen.width;
            _lastAppliedHeight = Screen.height;
            return;
        }

        _isApplyingResolution = true;
        Screen.SetResolution(width, height, FullScreenMode.Windowed);
        _isApplyingResolution = false;
        _lastAppliedWidth = Screen.width;
        _lastAppliedHeight = Screen.height;
        _lastWidth = Screen.width;
        _lastHeight = Screen.height;
    }

    /// <summary>
    /// 用途：对象销毁时恢复原始窗口过程，避免影响后续窗口行为。返回：无。
    /// </summary>
    private void OnDestroy()
    {
        UninstallNativeSizingHook();
    }

    /// <summary>
    /// 用途：尝试安装 Windows 原生 WM_SIZING 钩子以平滑锁定窗口比例。返回：无。
    /// </summary>
    private void TryInstallNativeSizingHook()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (_isNativeHookInstalled)
        {
            return;
        }

        _windowHandle = GetActiveWindow();
        if (_windowHandle == IntPtr.Zero)
        {
            _windowHandle = GetForegroundWindow();
        }

        if (_windowHandle == IntPtr.Zero)
        {
            Debug.LogWarning("WindowAspectController: failed to get window handle, fallback to polling mode.");
            return;
        }

        _wndProcDelegate = WindowProc;
        var newWndProcPtr = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
        _originalWndProc = SetWindowLongPtr(_windowHandle, GwlWndProc, newWndProcPtr);
        if (_originalWndProc == IntPtr.Zero)
        {
            Debug.LogWarning("WindowAspectController: failed to install native sizing hook, fallback to polling mode.");
            return;
        }

        _isNativeHookInstalled = true;
#endif
    }

    /// <summary>
    /// 用途：卸载窗口钩子并恢复默认窗口过程。返回：无。
    /// </summary>
    private void UninstallNativeSizingHook()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (!_isNativeHookInstalled || _windowHandle == IntPtr.Zero || _originalWndProc == IntPtr.Zero)
        {
            return;
        }

        SetWindowLongPtr(_windowHandle, GwlWndProc, _originalWndProc);
        _isNativeHookInstalled = false;
        _windowHandle = IntPtr.Zero;
        _originalWndProc = IntPtr.Zero;
        _wndProcDelegate = null;
#endif
    }

    /// <summary>
    /// 用途：处理窗口尺寸消息并直接修正到目标比例。返回：消息处理结果。
    /// </summary>
    private IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        if (msg == WmSizing && lParam != IntPtr.Zero)
        {
            var rect = Marshal.PtrToStructure<Rect>(lParam);
            var edge = wParam.ToInt32();
            EnforceAspectRatio(ref rect, edge);
            Marshal.StructureToPtr(rect, lParam, false);
        }

        return CallWindowProc(_originalWndProc, hWnd, msg, wParam, lParam);
#else
        return IntPtr.Zero;
#endif
    }

    /// <summary>
    /// 用途：按拖拽边类型修正目标矩形，保持 16:9 与最小宽度约束。返回：无。
    /// </summary>
    private static void EnforceAspectRatio(ref Rect rect, int edge)
    {
        var ratio = AspectWidth / (float)AspectHeight;
        var minWidth = MinWindowWidth;
        var minHeight = Mathf.CeilToInt(MinWindowWidth / ratio);
        var maxWidth = MaxWindowWidth;
        var maxHeight = MaxWindowHeight;

        var currentWidth = Math.Max(1, rect.Right - rect.Left);
        var currentHeight = Math.Max(1, rect.Bottom - rect.Top);
        var useWidthAsDriver = edge == WmszLeft
                               || edge == WmszRight
                               || edge == WmszTopLeft
                               || edge == WmszTopRight
                               || edge == WmszBottomLeft
                               || edge == WmszBottomRight;

        int targetWidth;
        int targetHeight;
        if (useWidthAsDriver)
        {
            targetWidth = Math.Max(currentWidth, minWidth);
            targetWidth = Math.Min(targetWidth, maxWidth);
            targetHeight = Mathf.RoundToInt(targetWidth / ratio);
        }
        else
        {
            targetHeight = Math.Max(currentHeight, minHeight);
            targetHeight = Math.Min(targetHeight, maxHeight);
            targetWidth = Mathf.RoundToInt(targetHeight * ratio);
            if (targetWidth < minWidth)
            {
                targetWidth = minWidth;
                targetHeight = Mathf.RoundToInt(targetWidth / ratio);
            }
        }

        if (targetHeight > maxHeight)
        {
            targetHeight = maxHeight;
            targetWidth = Mathf.RoundToInt(targetHeight * ratio);
        }

        if (targetWidth > maxWidth)
        {
            targetWidth = maxWidth;
            targetHeight = Mathf.RoundToInt(targetWidth / ratio);
        }

        switch (edge)
        {
            case WmszLeft:
                rect.Left = rect.Right - targetWidth;
                rect.Bottom = rect.Top + targetHeight;
                break;
            case WmszRight:
                rect.Right = rect.Left + targetWidth;
                rect.Bottom = rect.Top + targetHeight;
                break;
            case WmszTop:
                rect.Top = rect.Bottom - targetHeight;
                rect.Right = rect.Left + targetWidth;
                break;
            case WmszTopLeft:
                rect.Left = rect.Right - targetWidth;
                rect.Top = rect.Bottom - targetHeight;
                break;
            case WmszTopRight:
                rect.Right = rect.Left + targetWidth;
                rect.Top = rect.Bottom - targetHeight;
                break;
            case WmszBottom:
                rect.Bottom = rect.Top + targetHeight;
                rect.Right = rect.Left + targetWidth;
                break;
            case WmszBottomLeft:
                rect.Left = rect.Right - targetWidth;
                rect.Bottom = rect.Top + targetHeight;
                break;
            case WmszBottomRight:
            default:
                rect.Right = rect.Left + targetWidth;
                rect.Bottom = rect.Top + targetHeight;
                break;
        }
    }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", EntryPoint = "CallWindowProcW")]
    private static extern IntPtr CallWindowProc(
        IntPtr lpPrevWndFunc,
        IntPtr hWnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr newProc)
    {
        return IntPtr.Size == 8
            ? SetWindowLongPtr64(hWnd, nIndex, newProc)
            : SetWindowLong32(hWnd, nIndex, newProc);
    }
#endif
}
