#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// 用途：编辑器菜单快速打开 CardFx 特效预览场景。返回：无。
/// </summary>
public static class CardFxPreviewMenu
{
    private const string PreviewScenePath = "Assets/Scenes/effect.unity";

    [MenuItem("Puffies/Preview CardFx Effects")]
    public static void OpenCardFxPreviewScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        EditorSceneManager.OpenScene(PreviewScenePath);
        FocusGameView();
        EditorApplication.delayCall += static () =>
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
            }
        };
    }

    private static void FocusGameView()
    {
        var gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
        if (gameViewType == null)
        {
            return;
        }

        EditorWindow.GetWindow(gameViewType, false, null, false);
    }
}
#endif
