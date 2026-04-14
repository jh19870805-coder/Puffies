using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScene : MonoBehaviour
{
    private const float ReferenceHeight = 1080f;
    private const float PixelsPerUnit = 100f;
    private static bool sHookedSceneLoaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (!sHookedSceneLoaded)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            sHookedSceneLoaded = true;
        }

        TryBootstrap(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBootstrap(scene);
    }

    private static void TryBootstrap(Scene scene)
    {
        if (!IsGameScene(scene))
        {
            return;
        }

        if (FindObjectOfType<GameScene>() == null)
        {
            var bootstrapObject = new GameObject("GameSceneBootstrap");
            bootstrapObject.AddComponent<GameScene>();
        }
    }

    private static bool IsGameScene(Scene scene)
    {
        if (scene.name.Equals("GameScene", StringComparison.Ordinal))
        {
            return true;
        }

        return scene.path.EndsWith("/GameScene.unity", StringComparison.OrdinalIgnoreCase);
    }

    private void Start()
    {
        if (!IsGameScene(SceneManager.GetActiveScene()))
        {
            Destroy(gameObject);
            return;
        }

        GameManager.CreateInstance();
        if (Camera.main != null)
        {
            SetupMainCamera(Camera.main);
        }
        Debug.Log("GameScene init-only bootstrap completed.");
    }

    private static void SetupMainCamera(Camera camera)
    {
        camera.orthographic = true;
        camera.orthographicSize = ReferenceHeight / (2f * PixelsPerUnit);
    }

}
