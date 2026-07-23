#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal static class TemporaryOpenGameView
{
    static TemporaryOpenGameView()
    {
        EditorApplication.delayCall += () =>
        {
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            Debug.Log("TemporaryOpenGameView: requesting Play Mode.");
            EditorApplication.isPlaying = true;
        };
    }
}
#endif
