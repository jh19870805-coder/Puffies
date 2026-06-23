using TMPro;
using UnityEngine;

/// <summary>
/// 用途：加载项目默认中文字体（Noto Sans SC）。返回：按方法说明。
/// </summary>
public static class GameFontUtility
{
    private static Font sDefaultUiFont;
    private static TMP_FontAsset sDefaultTmpFont;

    /// <summary>
    /// 用途：获取默认 Unity UI Text 字体。返回：Font 或 null。
    /// </summary>
    public static Font GetDefaultUIFont()
    {
        if (sDefaultUiFont == null)
        {
            sDefaultUiFont = Resources.Load<Font>(GameDefine.DefaultChineseFontResourcesPath);
        }

        return sDefaultUiFont;
    }

    /// <summary>
    /// 用途：获取默认 TextMeshPro 字体资源。返回：TMP_FontAsset 或 null。
    /// </summary>
    public static TMP_FontAsset GetDefaultTmpFont()
    {
        if (sDefaultTmpFont == null)
        {
            sDefaultTmpFont = Resources.Load<TMP_FontAsset>(GameDefine.DefaultChineseTmpFontResourcesPath);
        }

        return sDefaultTmpFont;
    }

    /// <summary>
    /// 用途：为 Unity UI Text 套用默认中文字体（无字体时跳过）。返回：是否已设置。
    /// </summary>
    public static bool ApplyDefaultFont(UnityEngine.UI.Text text)
    {
        if (text == null)
        {
            return false;
        }

        var font = GetDefaultUIFont();
        if (font == null)
        {
            return false;
        }

        text.font = font;
        return true;
    }

    /// <summary>
    /// 用途：为 TextMeshPro 组件套用默认中文字体（无资源时跳过）。返回：是否已设置。
    /// </summary>
    public static bool ApplyDefaultFont(TMP_Text text)
    {
        if (text == null)
        {
            return false;
        }

        var fontAsset = GetDefaultTmpFont();
        if (fontAsset == null)
        {
            return false;
        }

        text.font = fontAsset;
        return true;
    }
}
