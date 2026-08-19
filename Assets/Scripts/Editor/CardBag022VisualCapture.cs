#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

internal static class CardBag022VisualCapture
{
    private const string SessionKey = "Puffies.CardBag022VisualCapture";
    private static int sFrameCount;

    static CardBag022VisualCapture()
    {
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void SelectCardBag022()
    {
        if (SessionState.GetBool(SessionKey, false)
            && SceneManager.GetActiveScene().name == GameDefine.SceneGame)
        {
            GameManager.SetBagId(22);
        }
    }

    public static void Run()
    {
        SessionState.SetBool(SessionKey, true);
        sFrameCount = 0;
        EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity", OpenSceneMode.Single);
        EditorApplication.isPlaying = true;
    }

    private static void Update()
    {
        if (!SessionState.GetBool(SessionKey, false) || !EditorApplication.isPlaying)
        {
            return;
        }

        if (++sFrameCount < 180)
        {
            return;
        }

        var root = GameObject.Find("CardBag022")?.GetComponent<RectTransform>();
        var board = GameObject.Find(GameDefine.GameBoardObjectName)?.GetComponent<RectTransform>();
        Debug.Log(
            $"VisualCaptureVerify: root={root?.rect.size.ToString() ?? "missing"}, "
            + $"gameBoard={board?.rect.size.ToString() ?? "missing"}.");
        SessionState.SetBool(SessionKey, false);
        EditorApplication.isPlaying = false;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode
            && !SessionState.GetBool(SessionKey, false)
            && Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }
}
#endif
